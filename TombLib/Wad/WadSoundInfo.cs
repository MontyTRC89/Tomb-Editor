using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TombLib.Utils;

namespace TombLib.Wad
{
    public class WadSoundInfo
    {
        private List<WadSample> _sound;
        private List<string> _sampleNames;

        public string Name { get; set; }

        public List<WadSample> Samples { get { return _sound; } }

        public short Volume { get; set; }
        public short Range { get; set; }
        public short Chance { get; set; }
        public short Pitch { get; set; }
        public bool FlagN { get; set; }
        public bool RandomizePitch { get; set; }
        public bool RandomizeGain { get; set; }
        public WadSoundLoopType Loop { get; set; }
        public Hash Hash { get { return _hash; } }

        private Hash _hash;

        // Internally used in tools
        public bool Enabled { get; set; }

        public WadSoundInfo()
        {
            _sound = new List<WadSample>();
            _sampleNames = new List<string>();
        }

        public Hash UpdateHash()
        {
            _hash = Hash.FromByteArray(this.ToByteArray());
            return _hash;
        }

        public byte[] ToByteArray()
        {
            using (var ms = new MemoryStream())
            {
                var writer = new BinaryWriter(ms);

                writer.Write(Volume);
                writer.Write(Range);
                writer.Write(Chance);
                writer.Write(Pitch);
                writer.Write(FlagN);
                writer.Write(RandomizePitch);
                writer.Write(RandomizeGain);
                writer.Write((byte)Loop);

                foreach (var sample in Samples)
                    writer.Write(sample.WaveData);

                return ms.ToArray();
            }
        }

        public WadSoundInfo Clone()
        {
            var soundInfo = new WadSoundInfo();

            soundInfo.Volume = Volume;
            soundInfo.Chance = Chance;
            soundInfo.FlagN = FlagN;
            soundInfo.Loop = Loop;
            soundInfo.Name = Name;
            soundInfo.Range = Range;
            soundInfo.Pitch = Pitch;
            soundInfo.RandomizeGain = RandomizeGain;
            soundInfo.RandomizePitch = RandomizePitch;
            foreach (var wave in Samples)
                soundInfo.Samples.Add(wave.Clone());
            soundInfo.UpdateHash();

            return soundInfo;
        }
    }
}
