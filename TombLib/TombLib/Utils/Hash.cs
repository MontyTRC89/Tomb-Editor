using System;
using System.IO.Hashing;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;

namespace TombLib.Utils
{
    public struct Hash : IEquatable<Hash>
    {
        public ulong HashLow;
        public ulong HashHigh;

        public static bool operator ==(Hash first, Hash second) =>
            first.HashLow == second.HashLow && first.HashHigh == second.HashHigh;

        public static bool operator !=(Hash first, Hash second) =>
            !(first == second);

        public bool Equals(Hash other) => this == other;
        public override bool Equals(object? obj) => obj is Hash other && this == other;
        public override int GetHashCode() => unchecked((int)HashLow);

        public static Hash FromByteArray(byte[] data)
        {
            byte[] hashBytes = XxHash64.Hash(data);
            ulong hash64 = BitConverter.ToUInt64(hashBytes, 0);

            return new Hash
            {
                HashLow = hash64,
                HashHigh = 0
            };
        }

        public static Hash Zero => new Hash { HashHigh = 0, HashLow = 0 };
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
