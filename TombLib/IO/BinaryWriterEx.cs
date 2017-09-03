using System.Collections.Generic;
using System.Text;
using System.IO;
using SharpDX;
using System.Runtime.InteropServices;

namespace TombLib.IO
{
    public class BinaryWriterEx : BinaryWriter
    {
        public BinaryWriterEx(Stream output)
            : base(output)
        {}

        public void Write(Vector2 value)
        {
            Write(value.X);
            Write(value.Y);
        }

        public void Write(Vector3 value)
        {
            Write(value.X);
            Write(value.Y);
            Write(value.Z);
        }

        public void Write(Vector4 value)
        {
            Write(value.X);
            Write(value.Y);
            Write(value.Z);
            Write(value.W);
        }

        public void Write(Matrix value)
        {
            Write(value.M11);
            Write(value.M12);
            Write(value.M13);
            Write(value.M14);
            Write(value.M21);
            Write(value.M22);
            Write(value.M23);
            Write(value.M24);
            Write(value.M31);
            Write(value.M32);
            Write(value.M33);
            Write(value.M34);
            Write(value.M41);
            Write(value.M42);
            Write(value.M43);
            Write(value.M44);
        }

        public void Write(BoundingBox value)
        {
            Write(value.Minimum);
            Write(value.Maximum);
        }

        public void WriteBlock<T>(T block)
        {
            var sizeOfT = Marshal.SizeOf(typeof(T));
            var unmanaged = Marshal.AllocHGlobal(sizeOfT);
            try
            {
                var buffer = new byte[sizeOfT];
                Marshal.StructureToPtr(block, unmanaged, false);
                Marshal.Copy(unmanaged, buffer, 0, sizeOfT);
                Write(buffer);
            }
            finally
            {
                Marshal.FreeHGlobal(unmanaged);
            }
        }

        public void WriteBlockArray<T>(IEnumerable<T> values)
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
                    Write(buffer);
                }
            }
            finally
            {
                Marshal.FreeHGlobal(unmanaged);
            }
        }

        public void WriteStringUtf8(string str)
        {
            var stringData = Encoding.UTF8.GetBytes(str);
            Write(stringData.GetLength(0));
            Write(stringData);
        }
    }
}
