using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TombLib.Utils
{
    public struct Hash : IEquatable<Hash>
    {
        private static Random _rng = new Random();
        private static ulong _keyLow = (ulong)(_rng.Next()) ^ ((ulong)(_rng.Next()) << 32);
        private static ulong _keyHigh = (ulong)(_rng.Next()) ^ ((ulong)(_rng.Next()) << 32);

        public ulong Hash1;
        public ulong Hash2;

        public static unsafe bool operator ==(Hash first, Hash second)
        {
            return (first.Hash1 == second.Hash1) && (first.Hash2 == second.Hash2);
        }

        public static bool operator !=(Hash first, Hash second)
        {
            return !(first == second);
        }

        public bool Equals(Hash other)
        {
            return this == other;
        }

        public override bool Equals(object obj)
        {
            return this == (Hash)obj;
        }

        public static Hash FromByteArray(byte[] data)
        {
            ulong result = CH.SipHash.SipHash.SipHash_2_4_UlongCast_ForcedInline(data, _keyLow, _keyHigh);
            return new Hash { Hash1 = result, Hash2 = 0 };
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
    }
}
