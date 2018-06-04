using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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

    public struct WadSoundInfoMetaData
    {
        public string Name { get; set; }
        public List<WadSample> Samples { get; set; }
        public float Volume { get; set; } // Increasing the volume above 1 is not supported by old games.
        public float RangeInSectors { get; set; }
        public float Chance { get; set; } // Must be a value between 0 and 1.
        public float PitchFactor { get; set; } // 1.0f here will keep the pitch identical, 2.0f will double it. (Attention about the value range of this, it depends on the sample frequency, by default almost 2.0 is the maximum)
        public bool DisablePanning { get; set; }
        public bool RandomizePitch { get; set; } // The pitch is sped up and slowed down by 6000/(2^16). (Not relative to the pitch)
        public bool RandomizeVolume { get; set; } // The volume is reduced by an absolete value of 1/8 of the full volume. (Not relative to the volume value)
        public WadSoundLoopBehaviour LoopBehaviour { get; set; }

        public WadSoundInfoMetaData(string name)
        {
            Name = name;
            Samples = new List<WadSample>();
            Volume = 1.0f;
            RangeInSectors = 8.0f;
            Chance = 1.0f;
            PitchFactor = 1.0f;
            DisablePanning = false;
            RandomizePitch = false;
            RandomizeVolume = false;
            LoopBehaviour = WadSoundLoopBehaviour.None;
        }

        public byte VolumeByte
        {
            get { return (byte)Math.Max(0, Math.Min(255, Volume * 255 + 0.5f)); }
            set { Volume = value / 255.0f; }
        }

        public byte RangeInSectorsByte
        {
            get { return (byte)Math.Max(0, Math.Min(255, RangeInSectors)); }
            set { RangeInSectors = value; }
        }

        public byte ChanceByte
        {
            get
            {
                byte result = (byte)Math.Max(0, Math.Min(255, Chance * 255 + 0.5f));
                return result == 255 ? (byte)0 : result;
            }
            set { Chance = (value == 0) ? 1.0f : (value / 255.0f); }
        }

        public byte PitchFactorByte
        {
            get
            {
                float actualPitchFactor = PitchFactor;
                if (Samples.Count > 0)
                    actualPitchFactor *= (float)Samples[0].SampleRate / (float)WadSample.GameSupportedSampleRate;
                return (byte)(0x80 ^ (byte)Math.Max(0, Math.Min(255, actualPitchFactor * 128 + 0.5f)));
            }
            set { PitchFactor = (value ^ 0x80) / 128.0f; }
        }

        public static float GetMaxPitch(uint sampleFrequency)
        {
            float pitchFactor = (float)WadSample.GameSupportedSampleRate / (float)sampleFrequency;
            return pitchFactor * (255.5f / 128.0f);
        }

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
    }

    public class WadSoundInfo
    {
        public WadSoundInfoMetaData Data { get; }
        public Hash Hash { get; }

        public WadSoundInfo()
            : this(new WadSoundInfoMetaData("Unnamed"))
        { }
        public WadSoundInfo(WadSoundInfoMetaData data)
        {
            if (data.Samples == null)
                throw new ArgumentNullException("data.Samples");
            Data = data;

            if (data.Samples.Count > 1)
                for (int i = 1; i < data.Samples.Count; ++i)
                    if (data.Samples[i].SampleRate != data.Samples[0].SampleRate)
                        throw new Exception("Different sound samples of the same sound have different sample rates!");

            using (var ms = new MemoryStream())
            {
                var writer = new BinaryWriterEx(ms);
                writer.Write(data.VolumeByte);
                writer.Write(data.RangeInSectorsByte);
                writer.Write(data.ChanceByte);
                writer.Write(data.PitchFactorByte);
                writer.Write(data.DisablePanning);
                writer.Write(data.RandomizePitch);
                writer.Write(data.RandomizeVolume);
                writer.Write((byte)data.LoopBehaviour);
                foreach (var sample in data.Samples)
                    writer.Write(sample.Hash);

                Hash = Hash.FromByteArray(ms.ToArray());
            }
        }

        public string Name => Data.Name;

        public static bool operator ==(WadSoundInfo first, WadSoundInfo second) => ReferenceEquals(first, null) ? ReferenceEquals(second, null) : (ReferenceEquals(second, null) ? false : (first.Hash == second.Hash));
        public static bool operator !=(WadSoundInfo first, WadSoundInfo second) => !(first == second);
        public bool Equals(WadSoundInfo other) => Hash == other.Hash;
        public override bool Equals(object other) => (other is WadSoundInfo) && Hash == ((WadSoundInfo)other).Hash;
        public override int GetHashCode() { return Hash.GetHashCode(); }

        public static WadSoundInfo Empty { get; } = new WadSoundInfo();

        public WadSoundInfo ChangeName(string newName)
        {
            if (Name == newName)
                return this;
            WadSoundInfoMetaData metaData = Data;
            metaData.Name = newName;
            return new WadSoundInfo(metaData);
        }

        public override string ToString()
        {
            if (Data.Name == null)
                return base.ToString();
            else
                return Data.Name;
        }
    }
}
