using Blake3;
using System;
using System.Buffers.Binary;
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

		public static bool operator !=(Hash first, Hash second) => !(first == second);

		public bool Equals(Hash other) => this == other;
		public override bool Equals(object? obj) => obj is Hash other && this == other;

		public override int GetHashCode()
		{
			ulong x = HashLow ^ HashHigh;
			x ^= x >> 30; x *= 0xBF58476D1CE4E5B9UL;
			x ^= x >> 27; x *= 0x94D049BB133111EBUL;
			x ^= x >> 31;
			return (int)x;
		}

		public static Hash FromByteArray(byte[] data)
		{
			using var hasher = Hasher.New();
			hasher.Update(data);

			Span<byte> digest = stackalloc byte[32];
			hasher.Finalize(digest);

			ulong low = BinaryPrimitives.ReadUInt64LittleEndian(digest.Slice(0, 8));
			ulong high = BinaryPrimitives.ReadUInt64LittleEndian(digest.Slice(8, 8));
			return new Hash { HashLow = low, HashHigh = high };
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
