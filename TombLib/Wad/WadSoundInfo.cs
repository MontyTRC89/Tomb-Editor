using System;
using System.Collections.Generic;
using System.Xml.Serialization;
using TombLib.LevelData;

namespace TombLib.Wad
{
    public enum WadSoundLoopBehaviour : byte
    {
        None = 0,
        OneShotWait = 1, // The same sound will be ignored until current one stops
        OneShotRewound = 2, // The sound will be rewound if triggered again
        Looped = 3 // The sound will be looped until strictly stopped by an engine event
    }

    public class WadSoundInfo
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int Volume { get; set; }
        public int RangeInSectors { get; set; }
        public int Chance { get; set; }
        public int PitchFactor { get; set; } 
        public bool DisablePanning { get; set; }
        public bool RandomizePitch { get; set; } // The pitch is sped up and slowed down by 6000/(2^16). (Not relative to the pitch)
        public bool RandomizeVolume { get; set; } // The volume is reduced by an absolute value of 1/8 of the full volume. (Not relative to the volume value)
        public WadSoundLoopBehaviour LoopBehaviour { get; set; }
        public List<WadSample> Samples { get; set; }
        public bool Global { get; set; }
        public bool Indexed { get; set; } // Legacy flag to index sound for TR2-3 MAIN.SFX file
        [XmlIgnore]
        public bool AddToLevel { get; set; }
        [XmlIgnore]
        public string SoundCatalog { get; set; }

        // Only for XML serialization!
        public WadSoundInfo() { }

        public WadSoundInfo(int id)
        {
            Id = id;
            Name = "NEW_SOUND";
            Volume = 100;
            RangeInSectors = 10;
            Chance = 100;
            PitchFactor = 0;
            DisablePanning = false;
            RandomizePitch = false;
            RandomizeVolume = false;
            LoopBehaviour = WadSoundLoopBehaviour.None;
            Global = false;
            Indexed = false;
            Samples = new List<WadSample>();
            SoundCatalog = "";
        }

        public WadSoundInfo(WadSoundInfo s)
        {
            Id = s.Id;
            Name = s.Name;
            Volume = s.Volume;
            RangeInSectors = s.RangeInSectors;
            Chance = s.Chance;
            PitchFactor = s.PitchFactor;
            DisablePanning = s.DisablePanning;
            RandomizePitch = s.RandomizePitch;
            RandomizeVolume = s.RandomizeVolume;
            LoopBehaviour = s.LoopBehaviour;
            Samples = new List<WadSample>();
            SoundCatalog = s.SoundCatalog;
            Global = s.Global;
            Indexed = s.Indexed;
            foreach (var sample in s.Samples)
                Samples.Add(new WadSample(sample.FileName));
        }


        public static float GetMaxPitch(uint sampleFrequency)
        {
            float pitchFactor = (float)WadSample.GameSupportedSampleRate / (float)sampleFrequency;
            return pitchFactor * (255.5f / 128.0f);
        }

        [XmlIgnore]
        public int SampleDataSize
        {
            get
            {
                int dataSize = 0;
                foreach (WadSample sample in Samples)
                    dataSize += sample.Data.Length;
                return dataSize;
            }
        }

        public int SampleCount(LevelSettings settings)
        {
            if ((settings.GameVersion == TRVersion.Game.TR2 || settings.GameVersion == TRVersion.Game.TR3) || 
                Samples == null || Samples.Count <= 0)
                return -1;

            int result = 0;
            foreach (var sample in Samples)
            {
                var path = WadSounds.TryGetSamplePath(settings, sample.FileName);
                if (path != null) result++;
            }
            return result;
        }

        public override string ToString()
        {
            return Name;
        }
    }
}
