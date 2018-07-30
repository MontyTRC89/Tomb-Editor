using NLog;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.CompilerServices;
using System.Threading;
using TombLib.IO;
using TombLib.Utils;
using ThreadState = System.Threading.ThreadState;

namespace TombLib.LevelData.IO
{
    public class Snapshot
    {
        private static readonly ConditionalWeakTable<Snapshot, WeakReference<byte[]>> _snapshotDecompressedDataCache = new ConditionalWeakTable<Snapshot, WeakReference<byte[]>>();
        private readonly string _fileName;
        private readonly long _dataLength;
        private volatile byte[] _data;
        private byte[] _dataCompressed;
        private Snapshot _deltaParent;
        private TimeSpan _dbgCmpressionTimespan = TimeSpan.MinValue;

        private Snapshot(string fileName, long dataLength, byte[] data)
        {
            _fileName = fileName;
            _dataLength = dataLength;
            _data = data;
            _snapshotDecompressedDataCache.Add(this, new WeakReference<byte[]>(data)); // Cache decompressed data for quick reusal.
        }

        // Note that the array may be longer than necessary and contain unnecessary data at the end. Use "_dataLength" to determine true length.
        private byte[] MaterializePrj2DataAsArray()
        {
            // See if data is cached already. If it is cached, use that data.
            WeakReference<byte[]> weakData;
            byte[] data;
            if (_snapshotDecompressedDataCache.TryGetValue(this, out weakData))
            {
                if (weakData.TryGetTarget(out data))
                    return data;
                _snapshotDecompressedDataCache.Remove(this);
            }

            // See if the data is directly available
            data = _data; // Copy in a thread-save way
            if (data != null)
            {
                _snapshotDecompressedDataCache.Add(this, new WeakReference<byte[]>(data)); // Cache decompressed data for quick reusal.
                return data;
            }

            // It's directly compressed
            if (_deltaParent == null)
                using (MemoryStream outStream = new MemoryStream(_dataCompressed.Length))
                {
                    MiniZ.Functions.Decompress(new MemoryStream(_dataCompressed, false), outStream);
                    data = outStream.GetBuffer();
                    _snapshotDecompressedDataCache.Add(this, new WeakReference<byte[]>(data)); // Cache decompressed data for quick reusal.
                    return data;
                }

            // It's delta compressed
            byte[] parentData = _deltaParent.MaterializePrj2DataAsArray();
            long parentSize = _deltaParent._dataLength;
            using (MemoryStream outStream = new MemoryStream(_dataCompressed.Length))
            {
                MiniZ.Functions.Decompress(new MemoryStream(_dataCompressed, false), outStream);
                byte[] deltaData = outStream.GetBuffer();
                long deltaSize = (int)outStream.Length;
                data = FossilDelta.Apply(parentData, (uint)parentSize, deltaData, (uint)deltaSize);
                _snapshotDecompressedDataCache.Add(this, new WeakReference<byte[]>(data)); // Cache decompressed data for quick reusal.
                return data;
            }
        }

        private Stream MaterializePrj2DataAsStream()
        {
            return new MemoryStream(MaterializePrj2DataAsArray(), 0, (int)_dataLength, false);
        }

        public Level MaterializeLevel()
        {
            using (var dataStream = MaterializePrj2DataAsStream())
                return Prj2Loader.LoadFromPrj2(_fileName, dataStream, new ProgressReporterSimple());
        }

        public bool DbgIsCompressed => _dbgCmpressionTimespan != TimeSpan.MinValue;
        public long DbgUncompressedSize => _dataLength;
        public long DbgCompressedSize => _dataCompressed.LongLength;
        public TimeSpan DbgCompressionTime => _dbgCmpressionTimespan;

        public class Engine : IDisposable
        {
            private static readonly Logger logger = LogManager.GetCurrentClassLogger();

            // Uses a tree to have a compromise between the length of the delta chain
            // and delta patches.
            // Length of the delta chain grows like log(n) while most new snaphosts can be chained on top of fairly recent snaphosts.
            //
            // Illustration for _deltaParentUseCountMax = 4: (Each text lines is a snapshot, so the vertical axis is time)
            // +-+-----              (Tree level 1, Snapshot chain length 0, the base snapshot)
            //   +-----              (Tree level 1, Snapshot chain length 1)
            //   +-----              (Tree level 1, Snapshot chain length 1)
            //   +-----              (Tree level 1, Snapshot chain length 1)
            //   +---+-----          (Tree level 2, Snapshot chain length 1)
            //   |   +-----          (Tree level 2, Snapshot chain length 2)
            //   |   +-----          (Tree level 2, Snapshot chain length 2)
            //   |   +-----          (Tree level 2, Snapshot chain length 2)
            //   +---+-----          (Tree level 2, Snapshot chain length 2)
            //   |   +-----          (Tree level 2, Snapshot chain length 2)
            //   |   +-----          (Tree level 2, Snapshot chain length 2)
            //   |   +-----          (Tree level 2, Snapshot chain length 2)
            //   +---+-----          (Tree level 2, Snapshot chain length 2)
            //   |   +-----          (Tree level 2, Snapshot chain length 2)
            //   |   +-----          (Tree level 2, Snapshot chain length 2)
            //   |   +-----          (Tree level 2, Snapshot chain length 2)
            //   +---+-----          (Tree level 2, Snapshot chain length 2)
            //   |   +-----          (Tree level 2, Snapshot chain length 2)
            //   |   +-----          (Tree level 2, Snapshot chain length 2)
            //   |   +-----          (Tree level 2, Snapshot chain length 2)
            //   +---+---+-----      (Tree level 3, Snapshot chain length 1)
            //   |   |   +-----      (Tree level 3, Snapshot chain length 2)
            //   |   |   +-----      (Tree level 3, Snapshot chain length 2)
            //   |   |   +-----      (Tree level 3, Snapshot chain length 2)
            //   |   +---+-----      (Tree level 3, Snapshot chain length 3)
            //   |   |   +-----      (Tree level 3, Snapshot chain length 3)
            //   |   |   +-----      (Tree level 3, Snapshot chain length 3)
            //   |   |   +-----      (Tree level 3, Snapshot chain length 3)
            //   |   +---+-----      (Tree level 3, Snapshot chain length 3)
            //   |   |   +-----      (Tree level 3, Snapshot chain length 3)
            //   |   |   +-----      (Tree level 3, Snapshot chain length 3)
            //   |   |   +-----      (Tree level 3, Snapshot chain length 3)
            //   |   +---+-----      (Tree level 3, Snapshot chain length 3)
            //   |       +-----      (Tree level 3, Snapshot chain length 3)
            //   |       +-----      (Tree level 3, Snapshot chain length 3)
            //   |       +-----      (Tree level 3, Snapshot chain length 3)
            //   |  (Now 3 more level 3 trees, and so on)
            //   ...
            //
            //   Note that at level n, each vertical layer in column i has (n - i) * _deltaParentUseCountMax nodes.
            //

            private const int _deltaParentUseCountMax = 4;
            private class ParentDeltaNode
            {
                public int _childCount;
                public WeakReference<Snapshot> _snapshot;
            }

            private readonly List<WeakReference<Snapshot>> _uncompressedSnapshots = new List<WeakReference<Snapshot>>(); // Use weak references so that snapshots can the thrown out by the garbage collector if they are no longer used.
            private readonly List<ParentDeltaNode> _deltaParentTree = new List<ParentDeltaNode>();
            private long _lastSize = 0;
            private readonly AutoResetEvent _workEvent = new AutoResetEvent(false);
            private readonly Thread[] _threads;
            private volatile bool _stop = false;

            public Engine(int snapshotCompressionThreadCount = 2)
            {
                _threads = new Thread[snapshotCompressionThreadCount];
                for (int i = 0; i < snapshotCompressionThreadCount; ++i)
                {
                    _threads[i] = new Thread(ThreadRun);
                    _threads[i].IsBackground = true;
                    _threads[i].Priority = ThreadPriority.Lowest;
                    _threads[i].Start();
                }
            }

            public void Dispose()
            {
                _stop = true;
                _workEvent.Set();
                for (int i = 0; i < _threads.Length; ++i)
                    if (_threads[i].ThreadState != ThreadState.Aborted || _threads[i].ThreadState != ThreadState.AbortRequested)
                        _threads[i].Abort(); // The threads don't have any direct interactions, we can just abort them without risk.
            }

            public Snapshot MakeSnapshot(Level level)
            {
                using (MemoryStream stream = new MemoryStream((int)_lastSize + 1024))
                {
                    Prj2Writer.SaveToPrj2(stream, level);
                    _lastSize = stream.Length;

                    Snapshot newSnapshot = new Snapshot(level.Settings.LevelFilePath, stream.Length, stream.GetBuffer());

                    // Remember object for compression in other threads
                    lock (_uncompressedSnapshots)
                        _uncompressedSnapshots.Add(new WeakReference<Snapshot>(newSnapshot));
                    _workEvent.Set();
                    return newSnapshot;
                }
            }

            private void ThreadRun()
            {
                do
                {
                    // Wait for work
                    _workEvent.WaitOne();

                    // Work loop
                    do
                    {
                        if (_stop)
                        {
                            _workEvent.Set(); // Inform other threads too.
                            return;
                        }

                        // Find snapshot to compress
                        Snapshot snapshotToCompress = null;
                        int remainingCount = 0;
                        lock (_uncompressedSnapshots)
                        {
                            int snapshotIndex = 0;
                            while (snapshotIndex < _uncompressedSnapshots.Count)
                                if (_uncompressedSnapshots[snapshotIndex++].TryGetTarget(out snapshotToCompress))
                                    break;
                            _uncompressedSnapshots.RemoveRange(0, snapshotIndex);
                            remainingCount = _uncompressedSnapshots.Count;
                        }
                        if (snapshotToCompress == null)
                            break;
                        if (remainingCount > 1)
                            _workEvent.Set(); // Start up other threads to help with snapshot compression
                        if (remainingCount > 50)
                            logger.Warn(remainingCount + " objects in the snapshot engine uncompressed.");

                        // Compress snapshot
                        try
                        {
                            Stopwatch watch = new Stopwatch();
                            watch.Start();
                            CompressSnapshot(snapshotToCompress);
                            watch.Stop();
                            snapshotToCompress._dbgCmpressionTimespan = watch.Elapsed;
                        }
                        catch (Exception exc)
                        {
                            logger.Error(exc, "Snapshot compression thread crashed!");
                        }
                    } while (true);
                } while (true);
            }

            private void CompressSnapshot(Snapshot snapshotToCompress)
            {
                Snapshot snapshotToAvoid = null;
                do
                {
                    Snapshot deltaParentSnapshot = GetDeltaParent(snapshotToCompress, snapshotToAvoid);
                    if (deltaParentSnapshot == null)
                    { // Direction compression
                        Stream stream = snapshotToCompress.MaterializePrj2DataAsStream();
                        using (var compressedStream = new MemoryStream())
                        {
                            MiniZ.Functions.Compress(stream, compressedStream, 9);
                            snapshotToCompress._dataCompressed = compressedStream.ToArray(); // No "GetBuffer" to save the extra memory long term.
                            snapshotToCompress._data = null; // Remove old data
                        }
                        return;
                    }
                    else
                    { // Attempt delta compression
                        byte[] newData = snapshotToCompress.MaterializePrj2DataAsArray();
                        byte[] parentData = deltaParentSnapshot.MaterializePrj2DataAsArray();
                        byte[] deltaData = FossilDelta.Create(parentData, (int)deltaParentSnapshot._dataLength, newData, (int)snapshotToCompress._dataLength);
                        if (6 * deltaData.Length < (deltaParentSnapshot._dataLength + snapshotToCompress._dataLength)) // Use delta compression only if it saves enough memory
                            using (var uncompressedStream = new MemoryStream(deltaData, false))
                            using (var compressedStream = new MemoryStream())
                            {
                                MiniZ.Functions.Compress(uncompressedStream, compressedStream, 9);
                                snapshotToCompress._dataCompressed = compressedStream.ToArray(); // No "GetBuffer" to save the extra memory long term.
                                snapshotToCompress._deltaParent = deltaParentSnapshot;
                                snapshotToCompress._data = null; // Remove old data
                                return;
                            }

                        // It did not save enough memory, try again but use different parent.
                        snapshotToAvoid = deltaParentSnapshot;
                    }
                } while (true);
            }

            private Snapshot GetDeltaParent(Snapshot snapshotToCompress, Snapshot snapshotToAvoid)
            {
                lock (_deltaParentTree)
                {
                    if (_deltaParentTree.Count == 0)
                    { // No tree available, so create one.
                        _deltaParentTree.Add(new ParentDeltaNode { _childCount = 1, _snapshot = new WeakReference<Snapshot>(snapshotToCompress) });
                        return null;
                    }

                    // Search for most top node of the tree, that still has space
                    int column = _deltaParentTree.Count - 1;
                    for (; column >= 0; --column)
                    {
                        int maxColumnNodes = (_deltaParentTree.Count - column) * _deltaParentUseCountMax; // See ASCII art above for visual explanation
                        if (_deltaParentTree[column]._childCount < maxColumnNodes)
                        {
                            _deltaParentTree[column]._childCount++;
                            break;
                        }
                    }

                    if (column == -1) // No node had space, we expand the tree now
                    {
                        _deltaParentTree[0]._childCount++;
                        ++column;
                        _deltaParentTree.Add(new ParentDeltaNode());
                    }

                    // Reset nodes above the current node
                    Snapshot result;
                    if (!_deltaParentTree[column]._snapshot.TryGetTarget(out result) || result == snapshotToAvoid)
                    { // If the tree is dead, we create a new one
                        _deltaParentTree.Clear();
                        _deltaParentTree.Add(new ParentDeltaNode { _childCount = 1, _snapshot = new WeakReference<Snapshot>(snapshotToCompress) });
                        return null;
                    }
                    for (int columnForwardIter = column + 1; columnForwardIter < _deltaParentTree.Count; ++columnForwardIter)
                    {
                        _deltaParentTree[columnForwardIter]._childCount = 1;
                        _deltaParentTree[columnForwardIter]._snapshot = new WeakReference<Snapshot>(snapshotToCompress);
                    }
                    return result;
                }
            }
        }
    }
}
