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
        private List<WadSound> _sound;

        public string Name { get; set; }
        public List<WadSound> WaveSounds { get { return _sound; } }
        public byte Volume { get; set; }
        public byte Range { get; set; }
        public byte Chance { get; set; }
        public byte Pitch { get; set; }
        public bool FlagN { get; set; }
        public bool RandomizePitch { get; set; }
        public bool RandomizeGain { get; set; }
        public byte Loop { get; set; }
        public Hash Hash { get { return _hash; } }

        private Hash _hash;

        public WadSoundInfo()
        {
            _sound = new List<WadSound>();
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
                writer.Write(Loop);

                foreach (var sample in WaveSounds)
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

            foreach (var wave in WaveSounds)
                soundInfo.WaveSounds.Add(wave.Clone());

            soundInfo.UpdateHash();

            return soundInfo;
        }
    }
}
