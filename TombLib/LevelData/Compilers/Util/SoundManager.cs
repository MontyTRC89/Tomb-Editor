﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using TombLib.IO;
using TombLib.Utils;
using TombLib.Wad;
using TombLib.Wad.Catalog;

namespace TombLib.LevelData.Compilers.Util
{
    public class SoundManager
    {
        private struct SoundDetail
        {
            public ushort FirstSample;
            public WadSoundInfoMetaData Data;
        }

        private readonly GameVersion _gameVersion;
        private readonly Dictionary<Hash, ushort> _soundInfoToMapIndexLookup = new Dictionary<Hash, ushort>();
        private readonly SortedSet<ushort> _freeSoundMapIndices = new SortedSet<ushort>(); // To remember free space in the middle of the sound map.

        private readonly List<WadSample> _samples = new List<WadSample>();
        private readonly List<uint> _sampleIndices = new List<uint>(); // Set them to filler zeros for TR4, TR5
        private readonly List<SoundDetail> _soundDetails = new List<SoundDetail>();
        private readonly List<ushort> _soundMap = new List<ushort>(); // 0xffff for empty sounds.


        public SoundManager(LevelSettings settings, Wad2 wad)
        {
            _gameVersion = settings.GameVersion;

            // Fix all the sounds for the TR version
            foreach (uint fixedSound in TrCatalog.GetAllFixedByDefaultSounds(settings.WadGameVersion).Keys)
                SetSoundMapEntryToSoundInfo(checked((ushort)fixedSound), null);

            // Add fixed sounds from the wad
            foreach (WadFixedSoundInfo fixedSoundInfo in wad.FixedSoundInfos.Values)
                SetSoundMapEntryToSoundInfo(checked((ushort)fixedSoundInfo.Id.TypeId), fixedSoundInfo.SoundInfo);
        }

        private void SetSoundMapEntryToSoundInfo(ushort soundMapIndex, WadSoundInfo soundInfo)
        {
            ushort soundDetailIndex = 0xffff; // Empty sound detail index.
            if (soundInfo != null)
            {
                // Create sound detail
                SoundDetail soundDetail = new SoundDetail { Data = soundInfo.Data };
                if (soundInfo.Data.Samples.Count == 0 ||
                    soundInfo.Data.Chance < (0.5f / 256.0f) ||
                    soundInfo.Data.Volume < (0.5f / 256.0f))
                { // Use null sample if there are no samples, or we shouldn't play anything. The engine otherwise messes this up otherwise.
                    soundDetail.Data.Samples = new List<WadSample>(new[] { WadSample.NullSample });
                }

                // Add sound samples.
                _sampleIndices.AddRange(Enumerable.Repeat((uint)0, soundDetail.Data.Samples.Count)); // Unused filler for TR4 and TR5
                soundDetail.FirstSample = checked((ushort)_samples.Count);
                _samples.AddRange(soundDetail.Data.Samples);

                // Add the sound details.
                soundDetailIndex = checked((ushort)_soundDetails.Count);
                _soundDetails.Add(soundDetail);
            }

            // Add the sound map entry.
            ushort oldSoundMapSize = checked((ushort)_soundMap.Count);
            if (_soundMap.Count <= soundMapIndex)
                _soundMap.Resize(soundMapIndex + 1, (ushort)0xffff);
            if (_soundMap[soundMapIndex] != 0xffff)
                throw new Exception("Sound map index " + soundMapIndex + " is used twice..");
            _soundMap[soundMapIndex] = soundDetailIndex;

            // Fill accelerated data structure to find free sound map entries
            if (soundInfo != null)
                _soundInfoToMapIndexLookup.Add(soundInfo.Hash, soundMapIndex);
            for (ushort i = oldSoundMapSize; i < soundMapIndex - 1; ++i)
                _freeSoundMapIndices.Add(i);
            _freeSoundMapIndices.Remove(soundMapIndex);
        }

        public ushort AllocateSoundInfo(WadSoundInfo soundInfo)
        {
            ushort soundMapIndex;
            if (!_soundInfoToMapIndexLookup.TryGetValue(soundInfo.Hash, out soundMapIndex))
            {
                // Search for a free space in the sound map.
                // Possibly there is a free place in the middle.
                if (_freeSoundMapIndices.Count != 0)
                {
                    soundMapIndex = _freeSoundMapIndices.First();
                    _freeSoundMapIndices.Remove(soundMapIndex);
                }
                else
                    soundMapIndex = checked((ushort)_soundDetails.Count);
                SetSoundMapEntryToSoundInfo(soundMapIndex, soundInfo);
            }
            return soundMapIndex;
        }

        // This function has to be called
        //    *after* the last call to 'AllocateSound' and
        //    *before* the first call to 'WriteSound...'
        public void PrepareSoundsData(IProgressReporter progressReporter)
        {
            // Determine size of sound map
            int soundMapSize;
            switch (_gameVersion)
            {
                case GameVersion.TR2:
                case GameVersion.TR3:
                case GameVersion.TR4:
                    soundMapSize = 370;
                    break;
                case GameVersion.TRNG:
                    soundMapSize = 2048;
                    break;
                case GameVersion.TR5:
                    soundMapSize = 450; // TODO Check if this supports a dynamically sized sound map.
                    break;
                default:
                    throw new NotImplementedException("Game version '" + _gameVersion + "'not supported.");
            }

            // Output some information
            progressReporter.ReportInfo("Sound map size: " + _soundMap.Count + " of " + soundMapSize);
            progressReporter.ReportInfo("Sound detail count: " + _soundDetails.Count + " of " + soundMapSize);
            progressReporter.ReportInfo("Sound indices count: " + _sampleIndices.Count);
            progressReporter.ReportInfo("Sound sample count: " + _samples.Count);

            // Fit size of sound map
            if (soundMapSize < _soundMap.Count)
                throw new Exception("Sound map to0 big with " + _soundMap.Count + " of " +
                    soundMapSize + " entries for the game version " + _gameVersion + ".");
            _soundMap.Resize(soundMapSize, (ushort)0xffff);
        }

        public void WriteSoundMetadata(BinaryWriterEx writer)
        {
            // Write sound map
            switch (_gameVersion)
            {
                case GameVersion.TRNG:
                    writer.Write(checked((ushort)_soundMap.Count)); // Num demo data
                    break;
                case GameVersion.TR2:
                case GameVersion.TR3:
                case GameVersion.TR4:
                    if (_soundMap.Count != 370)
                        throw new Exception("Internal error occurred: Sound map was not set to 370.");
                    writer.Write((ushort)0); // Num demo data
                    break;

                case GameVersion.TR5:
                    if (_soundMap.Count != 450)
                        throw new Exception("Internal error occurred: Sound map was not set to 450.");
                    writer.Write((ushort)0); // Num demo data
                    break;


                default:
                    throw new Exception("Unknown game version " + _gameVersion);
            }

            for (int i = 0; i < _soundMap.Count; i++)
                writer.Write(_soundMap[i]);

            // Write sound details
            writer.Write((uint)_soundDetails.Count);
            for (int i = 0; i < _soundDetails.Count; i++)
            {
                var soundDetail = _soundDetails[i];

                if (soundDetail.Data.Samples.Count > 0x3f)
                    throw new Exception("Too many sound effects for sound info '" + soundDetail.Data.Name + "'.");
                ushort characteristics = (ushort)(3 & (int)soundDetail.Data.LoopBehaviour);
                characteristics |= (ushort)(soundDetail.Data.Samples.Count << 2);
                if (soundDetail.Data.DisablePanning)
                    characteristics |= 0x1000;
                if (soundDetail.Data.RandomizePitch)
                    characteristics |= 0x2000;
                if (soundDetail.Data.RandomizeVolume)
                    characteristics |= 0x4000;

                if (_gameVersion == GameVersion.TR2)
                {
                    var newSoundDetail = new tr_sound_details();
                    newSoundDetail.Sample = soundDetail.FirstSample;
                    newSoundDetail.Volume = soundDetail.Data.VolumeByte;
                    newSoundDetail.Chance = soundDetail.Data.ChanceByte;
                    newSoundDetail.Characteristics = characteristics;
                    writer.WriteBlock(newSoundDetail);
                }
                else
                {
                    var newSoundDetail = new tr3_sound_details();
                    newSoundDetail.Sample = soundDetail.FirstSample;
                    newSoundDetail.Volume = soundDetail.Data.VolumeByte;
                    newSoundDetail.Chance = soundDetail.Data.ChanceByte;
                    newSoundDetail.Range = soundDetail.Data.RangeInSectorsByte;
                    newSoundDetail.Pitch = soundDetail.Data.PitchFactorByte;
                    newSoundDetail.Characteristics = characteristics;
                    writer.WriteBlock(newSoundDetail);
                }
            }

            writer.Write((uint)_sampleIndices.Count);
            for (int i = 0; i < _sampleIndices.Count; i++)
                writer.Write(_sampleIndices[i]);
        }

        public void WriteSoundData(BinaryWriter writer)
        {
            writer.Write((uint)_samples.Count); // Write sample count
            if (_gameVersion == GameVersion.TR5)
            { // We have to compress the samples first
              // TR5 uses compressed MS-ADPCM samples
                byte[][] compressedSamples = new byte[_samples.Count][];
                int[] uncompressedSizes = new int[_samples.Count];
                Parallel.For(0, _samples.Count, delegate (int i)
                    {
                        compressedSamples[i] = _samples[i].CompressToMsAdpcm(WadSample.GameSupportedSampleRate, out uncompressedSizes[i]);
                    });
                for (int i = 0; i < _samples.Count; ++i)
                {
                    writer.Write((uint)uncompressedSizes[i]);
                    writer.Write((uint)compressedSamples[i].Length);
                    writer.Write(compressedSamples[i]);
                }
            }
            else
            { // Uncompressed samples
                foreach (WadSample sample in _samples)
                {
                    writer.Write((uint)sample.Data.Length);
                    writer.Write((uint)sample.Data.Length);
                    writer.Write(sample.Data, 0, 24);
                    writer.Write((uint)WadSample.GameSupportedSampleRate); // Overwrite sample rate because the engine actually doens't read this and assumes 22050 anyway. Anything else than 22050 would just be a lie.
                    writer.Write((uint)(WadSample.GameSupportedSampleRate * 2));
                    writer.Write(sample.Data, 32, sample.Data.Length - 32);
                }
            }
        }

        public void UpdateMainSfx()
        {
            // TODO
        }
    }
}
