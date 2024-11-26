using System;
using System.Security.Cryptography;

namespace TombLib.Utils
{
    public struct Hash : IEquatable<Hash>
    {
        private static readonly Random _rng = new Random();
        private static readonly ulong _keyLow = (ulong)_rng.Next() ^ ((ulong)_rng.Next() << 32);
        private static readonly ulong _keyHigh = (ulong)_rng.Next() ^ ((ulong)_rng.Next() << 32);

        public ulong HashLow;
        public ulong HashHigh;

        public static bool operator ==(Hash first, Hash second) => first.HashLow == second.HashLow && first.HashHigh == second.HashHigh;
        public static bool operator !=(Hash first, Hash second) => first.HashLow != second.HashLow || first.HashHigh != second.HashHigh;
        public bool Equals(Hash other) => this == other;
        public override bool Equals(object other) => other is Hash && this == (Hash)other;
        public override int GetHashCode() => unchecked((int)HashLow);

        public static Hash FromByteArray(byte[] data)
        {
            ulong result = CH.SipHash.SipHash.SipHash_2_4_UlongCast_ForcedInline(data, _keyLow, _keyHigh);
            // TODO are 64 bit hashes really enough?
            // Seems a little bit risky.
            return new Hash { HashLow = result, HashHigh = 0 };
        }

        public static Hash Zero => new Hash() { HashHigh = 0, HashLow = 0 };
    }

    public class Checksum
    {
        public static int Calculate(byte[] data)
        {
            using (SHA256 sha256 = SHA256.Create())
            {
                // Compute the SHA-256 hash
                byte[] hash = sha256.ComputeHash(data);

                // Reduce the 256-bit hash to a 32-bit integer by XORing 4-byte chunks
                int checksum = 0;
                for (int i = 0; i < hash.Length; i += 4)
                {
                    // Combine each 4-byte segment into an integer and XOR it with checksum
                    int segment = BitConverter.ToInt32(hash, i);
                    checksum ^= segment;
                }

                return checksum;
            }
        }
    }
}
