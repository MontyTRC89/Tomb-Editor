using System;

namespace TombLib.Utils
{
    public struct Hash : IEquatable<Hash>
    {
        private static readonly Random _rng = new Random();
        private static readonly ulong _keyLow = (ulong)(_rng.Next()) ^ ((ulong)(_rng.Next()) << 32);
        private static readonly ulong _keyHigh = (ulong)(_rng.Next()) ^ ((ulong)(_rng.Next()) << 32);

        public ulong HashLow;
        public ulong HashHigh;

        public static bool operator ==(Hash first, Hash second) => (first.HashLow == second.HashLow) && (first.HashHigh == second.HashHigh);
        public static bool operator !=(Hash first, Hash second) => (first.HashLow != second.HashLow) || (first.HashHigh != second.HashHigh);
        public bool Equals(Hash other) => this == other;
        public override bool Equals(object other) => (other is Hash) && this == (Hash)other;
        public override int GetHashCode() => unchecked((int)HashLow);

        public static Hash FromByteArray(byte[] data)
        {
            ulong result = CH.SipHash.SipHash.SipHash_2_4_UlongCast_ForcedInline(data, _keyLow, _keyHigh);
            // TODO are 64 bit hashes really enough?
            // Seems a little bit risky.
            return new Hash { HashLow = result, HashHigh = 0 };
        }
    }
}
