using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TombLib.Utils;
using System.IO;
using System.Media;

namespace TombLib.Wad
{
    public class WadSample : IEquatable<WadSample>
    {
        public string Name { get; private set; }
        public byte[] WaveData { get; }
        public Hash Hash { get { return _hash; } }

        private Hash _hash;

        public WadSample(string name, byte[] data)
        {
            WaveData = data;
            Name = name;

            UpdateHash();
        }

        public Hash UpdateHash()
        {
            _hash = Hash.FromByteArray(WaveData);
            return _hash;
        }

        public bool Equals(WadSample other)
        {
            return (Hash == other.Hash);
        }

        public WadSample Clone()
        {
            return new WadSample(Name, WaveData);
        }

        public void Play()
        {
            using (var stream = new MemoryStream(WaveData))
            {
                using (var player = new SoundPlayer(stream))
                {
                    player.Play();
                }
            }
        }
    }
}
