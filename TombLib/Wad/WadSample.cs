using NAudio.Wave;
using NAudio.Flac;
using NAudio.Vorbis;
using NLog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TombLib.Utils;
using NAudio.Wave.SampleProviders;

namespace TombLib.Wad
{
    public class WadSample : IEquatable<WadSample>
    {
        public const uint GameSupportedSampleRate = 22050;
        private const int SampleRateOffset = 24;
        private const int SampleBytesPerSecondOffset = 28;

        private static readonly Logger logger = LogManager.GetCurrentClassLogger();

        /// <summary>This *always* *must* be Wave PCM mono data.</summary>
        public byte[] Data { get; }
        public Hash Hash { get; }

        public WadSample(byte[] data)
        {
            if (!CheckSampleDataForFormat(data))
                throw new NotSupportedException("Sample data is of an unsupported format.");
            Data = data;
            Hash = Hash.FromByteArray(Data);
        }

        public static bool CheckSampleDataForFormat(byte[] data)
        {
            // Based on: https://de.wikipedia.org/wiki/RIFF_WAVE
            // Just a very simple, ceap but strict checking routine to make sure
            // we don't get strange wave files in an *.wad2 or the game itself
            // that we then (or other tools) have to support later on.
            try
            {
                uint riffSignature = BitConverter.ToUInt32(data, 0);
                uint fileSize = BitConverter.ToUInt32(data, 4);
                uint waveSignature = BitConverter.ToUInt32(data, 8);
                if (riffSignature != 0x46464952)
                    return false;
                if (fileSize != (data.Length - 8))
                    return false;
                if (waveSignature != 0x45564157)
                    return false;

                uint fmt_Signature = BitConverter.ToUInt32(data, 12);
                uint fmtLength = BitConverter.ToUInt32(data, 16);
                ushort formatTag = BitConverter.ToUInt16(data, 20);
                ushort channelCount = BitConverter.ToUInt16(data, 22);
                uint sampleRate = BitConverter.ToUInt32(data, SampleRateOffset);
                uint bytesPerSecond = BitConverter.ToUInt32(data, SampleBytesPerSecondOffset);
                ushort blockAlign = BitConverter.ToUInt16(data, 32);
                ushort bitsPerSample = BitConverter.ToUInt16(data, 34);
                if (fmt_Signature != 0x20746D66)
                    return false;
                if (fmtLength != 16) // File generated with NAudio have a 18 bit header. Tomb Raider does not support this!
                    return false;
                if (formatTag != 1) // We want default PCM
                    return false;
                if (channelCount != 1) // We want mono audio
                    return false;
                // if (requiredSampleRate != 22050) // We allow custom sample rates
                //    return false;
                if (bytesPerSecond != (sampleRate * blockAlign))
                    return false;
                if (blockAlign != ((bitsPerSample * channelCount) / 8))
                    return false;
                if (bitsPerSample != 16) // We want 16 bit audio
                    return false;

                uint dataSignature = BitConverter.ToUInt32(data, 20 + (int)fmtLength);
                uint dataLength = BitConverter.ToUInt32(data, 24 + (int)fmtLength);
                if (dataSignature != 0x61746164)
                    return false;
                if (dataLength != (data.Length - (28 + (int)fmtLength)))
                    return false;
                return true;
            }
            catch
            {
                return false;
            }
        }

        public TimeSpan Duration
        {
            get
            {
                using (var stream = new MemoryStream(Data))
                using (var wfr = new WaveFileReader(stream))
                    return wfr.TotalTime;
            }
        }

        public uint SampleRate => BitConverter.ToUInt32(Data, SampleRateOffset);

        public struct ResampleInfo
        {
            public bool Resample;
            public uint SampleRate;
        }
        public WadSample ChangeSampleRate(uint targetSampleRate, bool resample = true) =>
            new WadSample(ConvertSampleFormat(Data, resample, targetSampleRate));
        public static byte[] ConvertSampleFormat(byte[] data, bool resample = true, uint sampleRate = GameSupportedSampleRate) =>
            ConvertSampleFormat(data, r => new ResampleInfo { Resample = resample, SampleRate = sampleRate });
        public static byte[] ConvertSampleFormat(byte[] data, Func<uint, ResampleInfo> negotiateSampleRate)
        {
            // Check if the format is actually already correct.
            // If it is we cou
            ResampleInfo? resampleInfo = null;
            if (CheckSampleDataForFormat(data))
            {
                // Performance: We could have a special case if it's just the file size in the WAVE header that's wrong
                // like with the original TR4 samples that have some garbage data at the end.

                uint sampleRate = BitConverter.ToUInt32(data, SampleRateOffset);
                resampleInfo = negotiateSampleRate(sampleRate);
                if (sampleRate == resampleInfo.Value.SampleRate)
                    return data;

                // If we don't need to resample we can just replace the sample rate in place without NAudio.
                if (!resampleInfo.Value.Resample)
                {
                    uint oldBytesPerSecond = BitConverter.ToUInt32(data, SampleBytesPerSecondOffset);
                    uint newBytesPerSecond = (oldBytesPerSecond / sampleRate) * resampleInfo.Value.SampleRate;

                    byte[] arrayClone = (byte[])data.Clone();
                    arrayClone[SampleRateOffset] = unchecked((byte)(resampleInfo.Value.SampleRate));
                    arrayClone[SampleRateOffset + 1] = unchecked((byte)(resampleInfo.Value.SampleRate >> 8));
                    arrayClone[SampleRateOffset + 2] = unchecked((byte)(resampleInfo.Value.SampleRate >> 16));
                    arrayClone[SampleRateOffset + 3] = unchecked((byte)(resampleInfo.Value.SampleRate >> 24));
                    arrayClone[SampleBytesPerSecondOffset] = unchecked((byte)(newBytesPerSecond));
                    arrayClone[SampleBytesPerSecondOffset + 1] = unchecked((byte)(newBytesPerSecond >> 8));
                    arrayClone[SampleBytesPerSecondOffset + 2] = unchecked((byte)(newBytesPerSecond >> 16));
                    arrayClone[SampleBytesPerSecondOffset + 3] = unchecked((byte)(newBytesPerSecond >> 24));
                    return arrayClone;
                }
            }

            // Use NAudio now to convert the audio data
            using (var inStream = new MemoryStream(data, false))
            using (var anyWaveStream = CreateReader(inStream, data))
            {
                if (resampleInfo == null) // Negotiate sample rate now, if we haven't already.
                    resampleInfo = negotiateSampleRate((uint)anyWaveStream.WaveFormat.SampleRate);
                WaveFormat targetFormat = new WaveFormat((int)resampleInfo.Value.SampleRate, 16, 1);

                // The TR4 engine doesn't seem to resample. So for reading wad samples we stay true to the intended sound by not resampling.
                // We just pretend to actually use the original sample rate.
                WaveFormat artificialFormat = targetFormat;
                if (!resampleInfo.Value.Resample)
                    artificialFormat = new WaveFormat(anyWaveStream.WaveFormat.SampleRate, 16, 1);

                var pcmStream = CreateWaveConverter(artificialFormat, anyWaveStream);
                try // The pcm stream may need to get disposed.
                {
                    using (var outStream = new MemoryStream())
                    {
                        // NAudio's WaveFileWriter produces incompatible files with an extra 2 bytes in the fmt header.
                        // This breaks the sounds in the TR4 engine.
                        // To work around the issue, write the wave header ourselves...
                        BinaryWriter writer = new BinaryWriter(outStream);
                        writer.Write((uint)0x46464952); // riff
                        writer.Write((uint)0);
                        writer.Write((uint)0x45564157); // wave

                        // fmt chunk
                        writer.Write((uint)0x20746D66); // fmt
                        writer.Write((uint)16);
                        writer.Write((ushort)targetFormat.Encoding);
                        writer.Write((ushort)targetFormat.Channels);
                        writer.Write((uint)targetFormat.SampleRate); // We always pretend to have the target sample rate (by default 22050Hz)
                        writer.Write((uint)targetFormat.AverageBytesPerSecond);
                        writer.Write((ushort)targetFormat.BlockAlign);
                        writer.Write((ushort)targetFormat.BitsPerSample);

                        // data chunk
                        writer.Write(0x61746164); // data
                        writer.Write((uint)0); // placeholder
                        {
                            byte[] buffer = new byte[8192];
                            int bytesRead;
                            while ((bytesRead = pcmStream.Read(buffer, 0, buffer.Length)) != 0)
                                outStream.Write(buffer, 0, bytesRead);
                        }

                        // Fix up file sizes
                        writer.BaseStream.Position = 4;
                        writer.Write((uint)(writer.BaseStream.Length - 8));
                        writer.BaseStream.Position = 40;
                        writer.Write((uint)(writer.BaseStream.Length - 44));

                        return outStream.ToArray();
                    }
                }
                finally
                {
                    (pcmStream as IDisposable)?.Dispose();
                }
            }
        }

        private static IWaveProvider CreateWaveConverter(WaveFormat format, WaveStream stream)
        {
            if (stream.WaveFormat.Encoding != WaveFormatEncoding.IeeeFloat)
            {
                try
                {
                    return new WaveFormatConversionStream(format, stream);
                }
                catch (Exception exc)
                { // This happens for example with floating point wave files.
                    logger.Warn(exc, "'WaveFormatConversionStream' failed. Attempting to use sample provider conversion now.");
                }
            }

            // Try going through the sample provider and then remix/resample manually if necessary...
            ISampleProvider sampleProvider = stream.ToSampleProvider();
            if (sampleProvider.WaveFormat.Channels > 1)
                sampleProvider = new StereoToMonoSampleProvider(sampleProvider);
            if (sampleProvider.WaveFormat.SampleRate != format.SampleRate)
                sampleProvider = new WdlResamplingSampleProvider(sampleProvider, format.SampleRate);
            return sampleProvider.ToWaveProvider16();
        }

        private static WaveStream CreateReader(MemoryStream dataStream, byte[] data)
        {
            uint startWord = BitConverter.ToUInt32(data, 0);
            if (startWord == 0x46464952) // "riff"
                return new WaveFileReader(dataStream);
            else if (startWord == 0x5367674f) // "OggS"
                return new VorbisWaveReader(dataStream);
            else if (startWord == 0x43614C66) // "fLaC"
                return new FlacReader(dataStream);
            else if (startWord == 0x4D524F46) // "FORM"
                return new FlacReader(dataStream);

            // Mp3 files are hard to identify,
            // so let's just give that a shot if it's obviously not one of the others.
            return new Mp3FileReader(dataStream);
        }

        public byte[] CompressToMsAdpcm(uint overwriteSampleRate, out int uncompressedSize) =>
            CompressToMsAdpcm(Data, overwriteSampleRate, out uncompressedSize);
        public static byte[] CompressToMsAdpcm(byte[] data, uint overwriteSampleRate, out int uncompressedSize)
        {
            AdpcmWaveFormat pcmFormat = new AdpcmWaveFormat((int)overwriteSampleRate, 1);
            AdpcmWaveFormat adpcmFormat = new AdpcmWaveFormat((int)overwriteSampleRate, 1);

            using (var inStream = new MemoryStream(data))
            using (var anyWaveStream = new WaveFileReader(inStream))
            using (var pcmStream = new RawSourceWaveStream(anyWaveStream, pcmFormat))
            {
                int sampleSize = ((pcmStream.WaveFormat.BitsPerSample * pcmStream.WaveFormat.Channels) / 8);
                int uncompressedSampleCount = (int)pcmStream.Length / sampleSize;
                uncompressedSampleCount = AlignTo(uncompressedSampleCount, adpcmFormat.SamplesPerBlock);
                uncompressedSize = uncompressedSampleCount * 2; // Time 2 because 16 bit mono samples (2 byte per sample)

                // We have to align the wave data to the wave block size
                // otherise NAudio will just cut off some samples!
                using (var alignedPcmStream = new AlignStream { _baseStream = pcmStream, _extendedLengthInBytes = uncompressedSize })
                using (var adpcmStream = new WaveFormatConversionStream(adpcmFormat, alignedPcmStream))
                using (var outStream = new MemoryStream())
                {
                    using (WaveFileWriter outWaveFileformat = new WaveFileWriter(outStream, adpcmFormat))
                    {
                        byte[] buffer = new byte[8192];
                        int bytesRead;
                        while ((bytesRead = adpcmStream.Read(buffer, 0, buffer.Length)) != 0)
                            outWaveFileformat.Write(buffer, 0, bytesRead);
                    }
                    return outStream.ToArray();
                }
            }
        }

        private class AlignStream : WaveStream
        {
            public long _extendedLengthInBytes;
            public WaveStream _baseStream;
            private long _currentPosition = 0;

            public override long Length => _extendedLengthInBytes;
            public override long Position
            {
                get { return _currentPosition; }
                set { throw new NotImplementedException(); }
            }
            public override WaveFormat WaveFormat => _baseStream.WaveFormat;
            public override int Read(byte[] buffer, int offset, int count)
            {
                long baseStreamLength = _baseStream.Length;

                // Read base stream
                int readInTotal = 0;
                if (_currentPosition < baseStreamLength)
                {
                    int read = _baseStream.Read(buffer, offset, count);
                    offset += read;
                    count -= read;
                    _currentPosition += read;
                    readInTotal += read;
                }

                // Extend base stream with new samples
                if (_currentPosition >= baseStreamLength)
                {
                    int extraByteCount = (int)(_extendedLengthInBytes - _currentPosition);
                    int read = Math.Min(extraByteCount, count);
                    Array.Clear(buffer, offset, read);
                    offset += read;
                    count -= read;
                    _currentPosition += read;
                    readInTotal += read;
                }

                return readInTotal;
            }
        }

        private static int AlignTo(int value, int alignment)
        {
            return ((value + alignment - 1) / alignment) * alignment;
        }






        public static bool operator ==(WadSample first, WadSample second) => ReferenceEquals(first, null) ? ReferenceEquals(second, null) : (ReferenceEquals(second, null) ? false : (first.Hash == second.Hash));
        public static bool operator !=(WadSample first, WadSample second) => !(first == second);
        public bool Equals(WadSample other) => Hash == other.Hash;
        public override bool Equals(object other) => (other is WadSample) && Hash == ((WadSample)other).Hash;
        public override int GetHashCode() { return Hash.GetHashCode(); }









        public static string LookupSound(string soundName, bool ignoreMissingSounds, string wadPath, List<string> oldWadSoundPaths)
        {
            if (!Path.HasExtension(soundName))
                soundName += ".wav";

            foreach (var oldWadSoundPath in oldWadSoundPaths)
            {
                string realPath = Path.Combine(wadPath ?? "", oldWadSoundPath ?? "", soundName);
                if (File.Exists(realPath))
                    return realPath;
            }

            if (ignoreMissingSounds)
            {
                logger.Warn("Sound '" + soundName + "' not found.");
                return null;
            }
            throw new FileNotFoundException("Sound not found", soundName);
        }

        public static readonly WadSample NullSample = new WadSample(new byte[] {
            0x52, 0x49, 0x46, 0x46, 0x28, 0x00, 0x00, 0x00, 0x57, 0x41, 0x56, 0x45,
            0x66, 0x6D, 0x74, 0x20, 0x10, 0x00, 0x00, 0x00, 0x01, 0x00, 0x01, 0x00,
            0x22, 0x56, 0x00, 0x00, 0x44, 0xAC, 0x00, 0x00, 0x02, 0x00, 0x10, 0x00,
            0x64, 0x61, 0x74, 0x61, 0x04, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00
        });

        public static WadSample ReadSound(string soundName, bool ignoreMissingSounds, string wadPath, List<string> oldWadSoundPaths)
        {
            string path = LookupSound(soundName, ignoreMissingSounds, wadPath, oldWadSoundPaths);
            if (string.IsNullOrEmpty(path))
                return NullSample;

            // Try opening sound
            try
            {
                using (var fileStream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read))
                {
                    byte[] result = new byte[checked((int)fileStream.Length)];
                    fileStream.Read(result, 0, result.GetLength(0));
                    return new WadSample(result);
                }
            }
            catch (Exception exc)
            {
                logger.Warn(exc, "Unable to load sound '" + soundName + "'.");
                if (ignoreMissingSounds)
                    return NullSample;
                throw;
            }
        }

        public static readonly IEnumerable<FileFormat> FileFormatsToRead = new[]
            {
                new FileFormat("WAVE file", "wav", "wave"),
                new FileFormat("MP3 file", "mp3"),
                new FileFormat("Free Lossless Audio Codec file", "flac"),
                new FileFormat("Ogg Vorbis file", "ogg"),
                new FileFormat("Audio Interchange file", "aiff", "aif", "aifc")
            };
    }
}
