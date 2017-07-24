using System.IO;
using System.Runtime.InteropServices;

namespace TombLib.IO
{
    public class BinaryReaderEx : BinaryReader
    {
        public BinaryReaderEx(Stream input)
            : base(input)
        {
        }

        public void ReadBlock<T>(out T output)
        {
            var sizeOfT = Marshal.SizeOf(typeof(T));
            var unmanaged = Marshal.AllocHGlobal(sizeOfT);
            var buffer = new byte[sizeOfT];

            Read(buffer, 0, sizeOfT);
            Marshal.Copy(buffer, 0, unmanaged, sizeOfT);

            output = (T) Marshal.PtrToStructure(unmanaged, typeof(T));
            Marshal.FreeHGlobal(unmanaged);
        }

        public void ReadBlockArray<T>(out T[] output, uint count)
        {
            var sizeOfT = Marshal.SizeOf(typeof(T));
            var unmanaged = Marshal.AllocHGlobal(sizeOfT * (int) count);
            var buffer = new byte[sizeOfT];

            output = new T[count];

            for (var i = 0; i < count; i++)
            {
                Read(buffer, 0, sizeOfT);
                Marshal.Copy(buffer, 0, unmanaged, sizeOfT);
                output[i] = (T) Marshal.PtrToStructure(unmanaged, typeof(T));
            }
            Marshal.FreeHGlobal(unmanaged);
        }

        public void ReadBlockArray<T>(out T[] output, int count)
        {
            var sizeOfT = Marshal.SizeOf(typeof(T));
            var unmanaged = Marshal.AllocHGlobal(sizeOfT * (int) count);
            var buffer = new byte[sizeOfT];

            output = new T[count];

            for (var i = 0; i < count; i++)
            {
                Read(buffer, 0, sizeOfT);
                Marshal.Copy(buffer, 0, unmanaged, sizeOfT);
                output[i] = (T) Marshal.PtrToStructure(unmanaged, typeof(T));
            }
            Marshal.FreeHGlobal(unmanaged);
        }
    }
}
