using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using TombLib.Utils;

namespace TombLib.IO
{
    public static class BinaryWriterExtensions
    {

        public static void Write(this BinaryWriter writer, in Vector2 value)
        {
            writer.Write(value.X);
            writer.Write(value.Y);
        }

        public static void Write(this BinaryWriter writer, in Vector3 value)
        {
            writer.Write(value.X);
            writer.Write(value.Y);
            writer.Write(value.Z);
        }

        public static void Write(this BinaryWriter writer, in Vector4 value)
        {
            writer.Write(value.X);
            writer.Write(value.Y);
            writer.Write(value.Z);
            writer.Write(value.W);
        }

        public static void Write(this BinaryWriter writer, in Quaternion value)
        {
            writer.Write(value.X);
            writer.Write(value.Y);
            writer.Write(value.Z);
            writer.Write(value.W);
        }

        public static void Write(this BinaryWriter writer, in Hash hash)
        {
            writer.Write(hash.HashLow);
            writer.Write(hash.HashHigh);
        }

        public static void WriteBlock<T>(this BinaryWriter writer, T block)
        {
            var sizeOfT = Marshal.SizeOf(typeof(T));
            var unmanaged = Marshal.AllocHGlobal(sizeOfT);

            try
            {
                var buffer = new byte[sizeOfT];
                Marshal.StructureToPtr(block, unmanaged, false);
                Marshal.Copy(unmanaged, buffer, 0, sizeOfT);
                writer.Write(buffer);
            }
            finally
            {
                Marshal.FreeHGlobal(unmanaged);
            }
        }

        public static void WriteBlockArray<T>(this BinaryWriter writer, IEnumerable<T> values)
        {
            var sizeOfT = Marshal.SizeOf(typeof(T));
            var unmanaged = Marshal.AllocHGlobal(sizeOfT);

            try
            {
                var buffer = new byte[sizeOfT];

                foreach (var s in values)
                {
                    Marshal.StructureToPtr(s, unmanaged, false);
                    Marshal.Copy(unmanaged, buffer, 0, sizeOfT);
                    writer.Write(buffer);
                }
            }
            finally
            {
                Marshal.FreeHGlobal(unmanaged);
            }
        }

        public static void WriteStringUTF8(this BinaryWriter writer, string value)
        {
            byte[] stringData = Encoding.UTF8.GetBytes(value);
            writer.Write(stringData.Length);
            writer.Write(stringData);
        }
    }
}
