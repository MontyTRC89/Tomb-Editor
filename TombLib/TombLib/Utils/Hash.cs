using HashDepot;
using System;

namespace TombLib.Utils
{
    public struct Hash : IEquatable<Hash>
    {
        public ulong HashLow;
        public ulong HashHigh;

        public static bool operator ==(Hash first, Hash second) => first.HashLow == second.HashLow && first.HashHigh == second.HashHigh;
        public static bool operator !=(Hash first, Hash second) => first.HashLow != second.HashLow || first.HashHigh != second.HashHigh;
        public bool Equals(Hash other) => this == other;
        public override bool Equals(object other) => other is Hash && this == (Hash)other;
        public override int GetHashCode() => unchecked((int)HashLow);

        public static Hash FromByteArray(byte[] data)
        {
            byte[] key = { 0x10, 0x21, 0x32, 0x23, 0x14, 0x25, 0x36, 0x27, 0x18, 0x09, 0x0a, 0x0b, 0x0c, 0x0d, 0x0e, 0x0f };

            ulong result = SipHash24.Hash64(data, key); // TODO: Check if .NET 6.0 port of this is done well.
            // TODO are 64 bit hashes really enough?
            // Seems a little bit risky.
            return new Hash { HashLow = result, HashHigh = 0 };
        }

        public static Hash Zero => new Hash() { HashHigh = 0, HashLow = 0 };
    }
}
