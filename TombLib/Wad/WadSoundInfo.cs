using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using TombLib.IO;
using TombLib.Utils;

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
        public float Volume { get; set; } // Increasing the volume above 1 is not supported by old games.
        public float RangeInSectors { get; set; }
        public float Chance { get; set; } // Must be a value between 0 and 1.
        public float PitchFactor { get; set; } // 1.0f here will keep the pitch identical, 2.0f will double it. (Attention about the value range of this, it depends on the sample frequency, by default almost 2.0 is the maximum)
        public bool DisablePanning { get; set; }
        public bool RandomizePitch { get; set; } // The pitch is sped up and slowed down by 6000/(2^16). (Not relative to the pitch)
        public bool RandomizeVolume { get; set; } // The volume is reduced by an absolete value of 1/8 of the full volume. (Not relative to the volume value)
        public WadSoundLoopBehaviour LoopBehaviour { get; set; }
        public List<WadSample> EmbeddedSamples { get; set; }
        public bool Global { get; set; }
        [XmlIgnore]
        public bool AddToLevel { get; set; }
        [XmlIgnore]
        public string Xml { get; set; }

        // Only for XML serialization!
        public WadSoundInfo() { }

        public WadSoundInfo(int id)
        {
            Id = id;
            Name = "UNKNOWN_SOUND";
            Volume = 1.0f;
            RangeInSectors = 8.0f;
            Chance = 1.0f;
            PitchFactor = 1.0f;
            DisablePanning = false;
            RandomizePitch = false;
            RandomizeVolume = false;
            LoopBehaviour = WadSoundLoopBehaviour.None;
            EmbeddedSamples = new List<WadSample>();
        }

        [XmlIgnore]
        public byte VolumeByte
        {
            get { return (byte)Math.Max(0, Math.Min(255, Volume * 255 + 0.5f)); }
            set { Volume = value / 255.0f; }
        }

        [XmlIgnore]
        public byte RangeInSectorsByte
        {
            get { return (byte)Math.Max(0, Math.Min(255, RangeInSectors)); }
            set { RangeInSectors = value; }
        }

        [XmlIgnore]
        public byte ChanceByte
        {
            get
            {
                byte result = (byte)Math.Max(0, Math.Min(255, Chance * 255 + 0.5f));
                return result == 255 ? (byte)0 : result;
            }
            set { Chance = (value == 0) ? 1.0f : (value / 255.0f); }
        }

        [XmlIgnore]
        public byte PitchFactorByte
        {
            get
            {
                float actualPitchFactor = PitchFactor;
                if (EmbeddedSamples.Count > 0 && EmbeddedSamples[0].IsLoaded)
                    actualPitchFactor *= (float)EmbeddedSamples[0].SampleRate / (float)WadSample.GameSupportedSampleRate;
                return (byte)(0x80 ^ (byte)Math.Max(0, Math.Min(255, actualPitchFactor * 128 + 0.5f)));
            }
            set { PitchFactor = (value ^ 0x80) / 128.0f; }
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
                foreach (WadSample sample in EmbeddedSamples)
                    dataSize += sample.Data.Length;
                return dataSize;
            }
        }

        public override string ToString()
        {
            return Name;
        }
    }
}
