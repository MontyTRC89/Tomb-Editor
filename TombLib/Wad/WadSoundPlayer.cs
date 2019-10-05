using NAudio.Wave;
using NAudio.Wave.SampleProviders;
using System;
using System.Collections.Generic;
using System.IO;
using TombLib.LevelData;
using TombLib.Utils;

namespace TombLib.Wad
{
    public static class WadSoundPlayer
    {
        private static Random _rng = new Random();
        private static WasapiOut _channel = null;

        public static void PlaySoundInfo(Level level, WadSoundInfo soundInfo, bool rewind)
        {
            if (_channel != null && _channel.PlaybackState == PlaybackState.Playing && rewind)
                StopSample();

            if (soundInfo.EmbeddedSamples.Count == 0)
                return;

            // Figure out the precise play parameters
            int sampleIndex;
            int loopCount = soundInfo.LoopBehaviour == WadSoundLoopBehaviour.Looped ? 3 : 1;
            float pan = 0.0f;
            float volume = soundInfo.Volume / 100.0f;
            float pitch = (soundInfo.PitchFactor / 127.0f) + 1.0f;
            float chance = soundInfo.Chance == 0 ? 1.0f : soundInfo.Chance / 100.0f;

            lock (_rng)
            {
                if (chance != 1.0f && _rng.NextDouble() > chance)
                    return;

                sampleIndex = _rng.Next(0, soundInfo.EmbeddedSamples.Count - 1);

                if (!soundInfo.DisablePanning)
                    pan = (float)((_rng.NextDouble() - 0.5f) * 1.6);
                if (soundInfo.RandomizePitch)
                    pitch += (float)(_rng.NextDouble() * (6000.0 / 65536.0));
                if (soundInfo.RandomizeVolume)
                    volume -= (float)_rng.NextDouble() * 0.125f;
            }

            PlaySample(level, soundInfo.EmbeddedSamples[sampleIndex], rewind, volume, pitch, pan, loopCount);
        }

        public static void PlaySample(Level level, WadSample sample, bool rewind, float volume = 1.0f, float pitch = 1.0f, float pan = 0.0f, int loopCount = 1)
        {
            if (volume <= 0.0f || loopCount <= 0)
                return;

            var disposables = new List<IDisposable>();
            try
            {
                // Load data.
                // If waveform data is loaded into memory, use it to play sound, otherwise find wav file on disk.

                WaveFileReader waveStream;
                MemoryStream memoryStream;

                if (sample.IsLoaded)
                {
                    memoryStream = disposables.AddAndReturn(new MemoryStream(sample.Data, false));
                    waveStream = disposables.AddAndReturn(new WaveFileReader(memoryStream));
                }
                else
                {
                    var path = WadSounds.TryGetSamplePath(level.Settings, sample.FileName);
                    if (path == null)
                        return;
                    else
                        waveStream = disposables.AddAndReturn(new WaveFileReader(path));
                }
                    

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
                sampleStream = new OffsetSampleProvider(sampleStream) { LeadOutSamples = sampleStream.WaveFormat.Channels * latencyInSamples };

                // Clean previous preview, if by any chance it's still playing
                if (rewind)
                    StopSample(); 

                // Play
                _channel = disposables.AddAndReturn(new WasapiOut(NAudio.CoreAudioApi.AudioClientShareMode.Shared, latencyInMilliseconds));
                _channel.Init(sampleStream);
                _channel.PlaybackStopped += (s, e) =>
                {
                    foreach (IDisposable disposable in disposables)
                        disposable.Dispose();
                };
                _channel.Play();
            }
            catch
            {
                // Clean up in case of a problem
                foreach (IDisposable disposable in disposables)
                    disposable.Dispose();
                throw;
            }
        }

        public static void StopSample()
        {
            if (_channel != null)
            {
                _channel.Stop();
                _channel.Dispose();
                _channel = null;
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
