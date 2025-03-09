using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace TombLib.IO
{
    public static class BinaryReaderExtensions
    {
        public static Vector2 ReadVector2(this BinaryReader reader)
        {
            return new Vector2(reader.ReadSingle(),reader.ReadSingle());
        }

        public static Vector3 ReadVector3(this BinaryReader reader)
        {
            return new Vector3(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle());
        }

        public static Vector4 ReadVector4(this BinaryReader reader)
        {
            return new Vector4(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle());
        }

        public static void ReadBlock<T>(this BinaryReader reader, out T output)
        {
            int sizeOfT = Marshal.SizeOf(typeof(T));
            IntPtr unmanaged = Marshal.AllocHGlobal(sizeOfT);
            byte[] buffer = new byte[sizeOfT];

            reader.Read(buffer, 0, sizeOfT);
            Marshal.Copy(buffer, 0, unmanaged, sizeOfT);

            output = (T)Marshal.PtrToStructure(unmanaged, typeof(T));
            Marshal.FreeHGlobal(unmanaged);
        }

        public static void ReadBlockArray<T>(this BinaryReader reader, out T[] output, uint count)
        {
            int sizeOfT = Marshal.SizeOf(typeof(T));
            IntPtr unmanaged = Marshal.AllocHGlobal(sizeOfT * (int)count);
            byte[] buffer = new byte[sizeOfT];

            output = new T[count];

            for (int i = 0; i < count; i++)
            {
                reader.Read(buffer, 0, sizeOfT);
                Marshal.Copy(buffer, 0, unmanaged, sizeOfT);
                output[i] = (T)Marshal.PtrToStructure(unmanaged, typeof(T));
            }
            Marshal.FreeHGlobal(unmanaged);
        }

        public static void ReadBlockArray<T>(this BinaryReader reader, out T[] output, int count)
        {
            int sizeOfT = Marshal.SizeOf(typeof(T));
            IntPtr unmanaged = Marshal.AllocHGlobal(sizeOfT * count);
            byte[] buffer = new byte[sizeOfT];

            output = new T[count];

            for (int i = 0; i < count; i++)
            {
                reader.Read(buffer, 0, sizeOfT);
                Marshal.Copy(buffer, 0, unmanaged, sizeOfT);
                output[i] = (T)Marshal.PtrToStructure(unmanaged, typeof(T));
            }
            Marshal.FreeHGlobal(unmanaged);
        }

        public static string ReadStringUTF8(this BinaryReader reader)
        {
            int stringLength = reader.ReadInt32();
            byte[] stringData = reader.ReadBytes(stringLength);
            string result = Encoding.UTF8.GetString(stringData);
            return result;
        }
    }
}
