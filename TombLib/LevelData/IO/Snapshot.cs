using NLog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using TombLib.IO;
using TombLib.Utils;

namespace TombLib.LevelData.IO
{
    public class Snapshot
    {
        private string _fileName;
        private volatile byte[] _data;
        private long _dataLength;
        private byte[] _dataCompressed;
        private Snapshot _deltaParent;

        private void MaterializePrj2DataAsArray(out byte[] byteArray, out long length)
        {
            // See if the data is directly available
            byte[] data = _data; // Copy in a thread-save way
            if (data != null)
            {
                byteArray = data;
                length = _dataLength;
                return;
            }

            // It's directly compressed
            if (_deltaParent == null)
                using (MemoryStream outStream = new MemoryStream(_dataCompressed.Length))
                {
                    MiniZ.Functions.Decompress(new MemoryStream(_dataCompressed, false), outStream);
                    byteArray = outStream.GetBuffer();
                    length = (int)outStream.Length;
                    return;
                }

            // It's delta compressed
            byte[] parentData, deltaData;
            long parentSize, deltaSize;
            _deltaParent.MaterializePrj2DataAsArray(out parentData, out parentSize);
            using (MemoryStream outStream = new MemoryStream(_dataCompressed.Length))
            {
                MiniZ.Functions.Decompress(new MemoryStream(_dataCompressed, false), outStream);
                deltaData = outStream.GetBuffer();
                deltaSize = (int)outStream.Length;
            }
            byteArray = FossilDelta.Apply(parentData, (uint)parentSize, deltaData, (uint)deltaSize);
            length = byteArray.Length;
            return;
        }

        private Stream MaterializePrj2DataAsStream()
        {
            byte[] data;
            long length;
            MaterializePrj2DataAsArray(out data, out length);
            return new MemoryStream(data, 0, (int)length, false);
        }

        public Level MaterializeLevel()
        {
            using (var dataStream = MaterializePrj2DataAsStream())
                return Prj2Loader.LoadFromPrj2(_fileName, dataStream, new ProgressReporterSimple());
        }

        public class Engine : IDisposable
        {
            private static readonly Logger logger = LogManager.GetCurrentClassLogger();

            private readonly List<WeakReference<Snapshot>> _uncompressedSnapshots = new List<WeakReference<Snapshot>>(); // Use weak references so that snapshots can the thrown out by the garbage collector if they are no longer used.
            private readonly WeakReference<Snapshot> _deltaParentSnapshot = new WeakReference<Snapshot>(null);
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

                    Snapshot newSnapshot = new Snapshot
                    {
                        _fileName = level.Settings.LevelFilePath,
                        _data = stream.GetBuffer(),
                        _dataLength = stream.Length
                    };

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
                            CompressSnapshot(snapshotToCompress);
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
                Snapshot snapshotToAvoidAsParent = null;
                do
                {
                    // Figure out parent snapshot for delta compression
                    Snapshot deltaParentSnapshot;
                    lock (_deltaParentSnapshot)
                    {
                        _deltaParentSnapshot.TryGetTarget(out deltaParentSnapshot);
                        if (deltaParentSnapshot == null || deltaParentSnapshot == snapshotToAvoidAsParent)
                        {
                            deltaParentSnapshot = snapshotToCompress;
                            _deltaParentSnapshot.SetTarget(snapshotToCompress);
                        }
                    }

                    // Compress
                    if (deltaParentSnapshot == snapshotToCompress)
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
                        byte[] newData, parentData;
                        long newLength, parentLength;
                        snapshotToCompress.MaterializePrj2DataAsArray(out newData, out newLength);
                        deltaParentSnapshot.MaterializePrj2DataAsArray(out parentData, out parentLength);
                        byte[] deltaData = FossilDelta.Create(parentData, (int)parentLength, newData, (int)newLength);
                        if (4 * deltaData.Length < (parentLength + newLength)) // Use delta compression only if it saves enough memory
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
                        snapshotToAvoidAsParent = deltaParentSnapshot;
                    }
                } while (true);
            }
        }
    }
}
