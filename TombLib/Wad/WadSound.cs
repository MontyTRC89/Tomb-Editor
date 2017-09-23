using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TombLib.Utils;
using System.IO;

namespace TombLib.Wad
{
    public class WadSound : IEquatable<WadSound>
    {
        public string Name { get; private set; }
        public byte[] WaveData { get; }
        public Hash Hash { get { return _hash; } }

        private Hash _hash;

        public WadSound(string name, byte[] data)
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

        public bool Equals(WadSound other)
        {
            return (Hash == other.Hash);
        }

        public WadSound Clone()
        {
            return new WadSound(Name, WaveData);
        }
    }
}
