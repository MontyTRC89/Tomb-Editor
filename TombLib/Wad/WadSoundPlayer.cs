using NAudio.Wave;
using NAudio.Wave.SampleProviders;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Media;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using TombLib.Utils;

namespace TombLib.Wad
{
    public static class WadSoundPlayer
    {
        private static Random _rng = new Random();

        public static void PlaySoundInfo(WadSoundInfo soundInfo)
        {
            if (soundInfo.Data.Samples.Count == 0)
                return;

            // Figure out the precise play parameters
            int sampleIndex;
            int loopCount = soundInfo.Data.LoopBehaviour == WadSoundLoopBehaviour.Looped ? 3 : 1;
            float pan = 0.0f;
            float volume = soundInfo.Data.Volume;
            float pitch = soundInfo.Data.PitchFactor;
            lock (_rng)
            {
                if (_rng.NextDouble() > soundInfo.Data.Chance)
                    return;
                sampleIndex = _rng.Next(0, soundInfo.Data.Samples.Count - 1);
                if (!soundInfo.Data.DisablePanning)
                    pan = (float)((_rng.NextDouble() - 0.5f) * 1.6);
                if (soundInfo.Data.RandomizePitch)
                    pitch += (float)(_rng.NextDouble() * (6000.0 / 65536.0));
                if (soundInfo.Data.RandomizeVolume)
                    volume -= (float)_rng.NextDouble() * 0.125f;
            }
            PlaySample(soundInfo.Data.Samples[sampleIndex], volume, pitch, pan, loopCount);
        }

        public static void PlaySample(WadSample sample, float volume = 1.0f, float pitch = 1.0f, float pan = 0.0f, int loopCount = 1)
        {
            if (volume <= 0.0f || loopCount <= 0)
                return;

            //new PanningSampleProvider
            // To change the pitch: SmbPitchShiftingSampleProvider or WdlResamplingSampleProvider
            var disposables = new List<IDisposable>();
            try
            {
                // Load data
                var memoryStream = disposables.AddAndReturn(new MemoryStream(sample.Data, false));
                var waveStream = disposables.AddAndReturn(new WaveFileReader(memoryStream));

                // Apply looping
                ISampleProvider sampleStream;
                if (loopCount <= 1)
                    sampleStream = waveStream.ToSampleProvider();
                else
                    sampleStream = new RepeatedStream(waveStream) { LoopCount = loopCount };

                // Apply panning
                if (pan != 1.0f)
                    sampleStream = new PanningSampleProvider(sampleStream) { Pan = pan };

                // Apply pitch
                if (pitch != 1.0f)
                    sampleStream = new PitchedStream { Source = sampleStream, Pitch = pitch };

                // Apply volume
                if (volume != 1.0f)
                    sampleStream = new VolumeSampleProvider(sampleStream) { Volume = volume };

                // Add some silence to make sure the audio plays out.
                const int latencyInMilliseconds = 200;
                int latencyInSamples = (sampleStream.WaveFormat.SampleRate * latencyInMilliseconds * 2) / 1000;
                sampleStream = new OffsetSampleProvider(sampleStream)
                {
                    LeadOutSamples = sampleStream.WaveFormat.Channels * latencyInSamples
                };

                // Play
                var output = disposables.AddAndReturn(new WasapiOut(NAudio.CoreAudioApi.AudioClientShareMode.Shared, latencyInMilliseconds));
                output.Init(sampleStream);
                output.PlaybackStopped += (s, e) =>
                {
                    foreach (IDisposable disposable in disposables)
                        disposable.Dispose();
                };
                output.Play();
            }
            catch
            {
                // Clean up in case of a problem
                foreach (IDisposable disposable in disposables)
                    disposable.Dispose();
                throw;
            }
        }

        public static ImageC DrawWaveformForSample(WadSample sampleData, VectorInt2 imageSize, double startPositionInSeconds, double secondsPerPixel, ColorC graphColor)
        {
            // Read samples
            using (var memoryStream = new MemoryStream(sampleData.Data, false))
            using (var waveStream = new WaveFileReader(memoryStream))
            {
                double startPositionInSamples = waveStream.WaveFormat.SampleRate * startPositionInSeconds;
                double samplesPerPixel = waveStream.WaveFormat.SampleRate * secondsPerPixel;

                ImageC result = ImageC.CreateNew(imageSize.X, imageSize.Y);
                const int bytesPerSample = 2;
                byte[] waveData = new byte[((int)(samplesPerPixel) + 10) * bytesPerSample];
                waveStream.Position = ((int)(startPositionInSamples)) * bytesPerSample;

                for (int i = 0; i < imageSize.X; i += 1)
                {
                    // Read raw data
                    int samplesForPixel = (int)(samplesPerPixel * (i + 1) + 0.5f) - (int)(samplesPerPixel * i + 0.5f);
                    int bytesRead = waveStream.Read(waveData, 0, samplesForPixel * bytesPerSample);
                    if (bytesRead == 0)
                        break;

                    // Combine samples that all fall into a single pixel
                    short min = BitConverter.ToInt16(waveData, 0);
                    short max = min;
                    for (int j = bytesPerSample; j < bytesRead; j += bytesPerSample)
                    {
                        short sample = BitConverter.ToInt16(waveData, j);
                        min = Math.Min(min, sample);
                        max = Math.Max(max, sample);
                    }

                    // Fill in the line
                    int factor = imageSize.Y - 1;
                    int minImageY = ((min + 32768) * factor + 32768) >> 16;
                    int maxImageY = ((max + 32768) * factor + 32768) >> 16;
                    for (int j = minImageY; j <= maxImageY; ++j)
                        result.SetPixel(i, j, graphColor);
                }
                return result;
            }
        }
        private class RepeatedStream : ISampleProvider
        {
            public int LoopCount;
            private ISampleProvider _provider;
            private WaveStream _waveStream;

            public RepeatedStream(WaveStream waveStream)
            {
                _waveStream = waveStream;
                _provider = waveStream.ToSampleProvider();
            }

            public WaveFormat WaveFormat => _provider.WaveFormat;

            public int Read(float[] buffer, int offset, int count)
            {
                int read = 0;
                while (LoopCount > 0)
                {
                    read += _provider.Read(buffer, offset + read, count - read);
                    if (read >= count)
                        return read;

                    LoopCount--;
                    _waveStream.Seek(0, SeekOrigin.Begin);
                }
                return read;
            }
        }

        private class PitchedStream : ISampleProvider
        {
            public ISampleProvider Source;
            public float Pitch;

            public WaveFormat WaveFormat
            {
                get
                {
                    WaveFormat sourceFormat = Source.WaveFormat;
                    float newSampleRate = Math.Max(40, Math.Min(int.MaxValue, sourceFormat.SampleRate * Pitch + 0.5f));
                    return WaveFormat.CreateIeeeFloatWaveFormat((int)newSampleRate, sourceFormat.Channels);
                }
            }

            public int Read(float[] buffer, int offset, int count) => Source.Read(buffer, offset, count);
        }
    }
}
