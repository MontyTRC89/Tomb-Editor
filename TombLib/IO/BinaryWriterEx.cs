using System.IO;
using SharpDX;
using System.Runtime.InteropServices;

namespace TombLib.IO
{
    public class BinaryWriterEx : BinaryWriter
    {
        public BinaryWriterEx(Stream output)
            : base(output)
        {
        }

        public void Write(Vector2 value)
        {
            Write(value.X);
            Write(value.Y);
        }

        public void WriteBlock<T>(T block)
        {
            var sizeOfT = Marshal.SizeOf(typeof(T));
            var unmanaged = Marshal.AllocHGlobal(sizeOfT);
            var buffer = new byte[sizeOfT];

            Marshal.StructureToPtr(block, unmanaged, false);
            Marshal.Copy(unmanaged, buffer, 0, sizeOfT);

            Write(buffer);

            Marshal.FreeHGlobal(unmanaged);
        }

        public void WriteBlockArray<T>(T[] values)
        {
            foreach (var value in values)
            {
                var sizeOfT = Marshal.SizeOf(typeof(T));
                var unmanaged = Marshal.AllocHGlobal(sizeOfT);
                var buffer = new byte[sizeOfT];

                Marshal.StructureToPtr(value, unmanaged, false);
                Marshal.Copy(unmanaged, buffer, 0, sizeOfT);

                Write(buffer);

                Marshal.FreeHGlobal(unmanaged);
            }
        }
    }
}
