using BrandonHaynes.Security.SipHash;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TombLib.Utils
{
    public struct Hash : IEquatable<Hash>
    {
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
            byte[] key = { 0x10, 0x21, 0x32, 0x23, 0x14, 0x25, 0x36, 0x27, 0x18, 0x09, 0x0a, 0x0b, 0x0c, 0x0d, 0x0e, 0x0f };

            Hash hash = new Hash();

            using (var h = new SipHash(key))
            {
                var buffer = h.ComputeHash(data);
                hash.Hash1 = BitConverter.ToUInt64(buffer, 0);
            }

            return hash;
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
    }
}
