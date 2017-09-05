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
        public byte[] WaveData { get { return _wave; } }
        public Hash Hash { get { return _hash; } }

        private byte[] _wave;
        private Hash _hash;

        public WadSound(byte[] data)
        {
            _wave = data;
            UpdateHash();
        }

        public Hash UpdateHash()
        {
            _hash = Hash.FromByteArray(_wave);
            return _hash;
        }

        public bool Equals(WadSound other)
        {
            return (Hash == other.Hash);
        }
    }
}
