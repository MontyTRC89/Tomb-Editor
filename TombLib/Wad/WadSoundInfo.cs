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
        OneShotRewound = 1, // The sound will be rewound if triggered again
        OneShotWait = 2, // The same sound will be ignored until current one stops
        Looped = 3 // The sound will be looped until strictly stopped by an engine event
    }

    public struct WadSoundInfoMetaData
    {
        public string Name { get; set; }
        public List<WadSample> Samples { get; set; }
        public byte VolumeDiv255 { get; set; }
        public byte RangeInSectors { get; set; }
        public byte ChanceDiv255 { get; set; }
        public byte PitchFactorDiv128 { get; set; } // 128 here will keep the pitch identical, 255 will *almost* double it.
        public bool FlagN { get; set; }
        public bool RandomizePitch { get; set; }
        public bool RandomizeGain { get; set; }
        public WadSoundLoopBehaviour LoopBehaviour { get; set; }

        public WadSoundInfoMetaData(string name)
        {
            Name = name;
            Samples = new List<WadSample>();
            VolumeDiv255 = 128;
            RangeInSectors = 8;
            ChanceDiv255 = 255;
            PitchFactorDiv128 = 128;
            FlagN = false;
            RandomizePitch = false;
            RandomizeGain = false;
            LoopBehaviour = WadSoundLoopBehaviour.None;
        }

        private static Random PlayRng = new Random();
        public void Play()
        {
            if (Samples.Count < 0)
                return;

            int rngSampleIndex;
            lock (PlayRng)
                rngSampleIndex = PlayRng.Next(0, Samples.Count - 1);

            // TODO How about the other parameters?
            Samples[rngSampleIndex].Play();
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

            using (var ms = new MemoryStream())
            {
                var writer = new BinaryWriterEx(ms);
                writer.Write(data.VolumeDiv255);
                writer.Write(data.RangeInSectors);
                writer.Write(data.ChanceDiv255);
                writer.Write(data.PitchFactorDiv128);
                writer.Write(data.FlagN);
                writer.Write(data.RandomizePitch);
                writer.Write(data.RandomizeGain);
                writer.Write((byte)data.LoopBehaviour);
                foreach (var sample in data.Samples)
                    writer.Write(sample.Hash);

                Hash = Hash.FromByteArray(ms.ToArray());
            }
        }

        public string Name => Data.Name;
        public void Play() => Data.Play();

        public static bool operator ==(WadSoundInfo first, WadSoundInfo second) => ReferenceEquals(first, null) ? ReferenceEquals(second, null) : (ReferenceEquals(second, null) ? false : (first.Hash == second.Hash));
        public static bool operator !=(WadSoundInfo first, WadSoundInfo second) => !(first == second);
        public bool Equals(WadSoundInfo other) => Hash == other.Hash;
        public override bool Equals(object other) => (other is WadSoundInfo) && Hash == ((WadSoundInfo)other).Hash;
        public override int GetHashCode() { return Hash.GetHashCode(); }

        public static WadSoundInfo Empty { get; } = new WadSoundInfo();
    }
}
