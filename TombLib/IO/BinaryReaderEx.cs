using System.Text;
using System.IO;
using SharpDX;
using System.Runtime.InteropServices;

namespace TombLib.IO
{
    public class BinaryReaderEx : BinaryReader
    {
        public BinaryReaderEx(Stream input)
            : base(input)
        { }

        public Vector2 ReadVector2()
        {
            return new Vector2(ReadSingle(), ReadSingle());
        }

        public Vector3 ReadVector3()
        {
            return new Vector3(ReadSingle(), ReadSingle(), ReadSingle());
        }

        public Vector4 ReadVector4()
        {
            return new Vector4(ReadSingle(), ReadSingle(), ReadSingle(), ReadSingle());
        }

        public Matrix ReadMatrix()
        {
            var values = new float[16];
            for (int i = 0; i < 16; i++)
                values[i] = ReadSingle();
            return new Matrix(values);
        }

        public BoundingSphere ReadBoundingSphere()
        {
            return new BoundingSphere(ReadVector3(), ReadSingle());
        }

        public BoundingBox ReadBoundingBox()
        {
            return new BoundingBox(ReadVector3(), ReadVector3());
        }

        public void ReadBlock<T>(out T output)
        {
            int sizeOfT = Marshal.SizeOf(typeof(T));
            var unmanaged = Marshal.AllocHGlobal(sizeOfT);
            var buffer = new byte[sizeOfT];

            Read(buffer, 0, sizeOfT);
            Marshal.Copy(buffer, 0, unmanaged, sizeOfT);

            output = (T)Marshal.PtrToStructure(unmanaged, typeof(T));
            Marshal.FreeHGlobal(unmanaged);
        }

        public void ReadBlockArray<T>(out T[] output, uint count)
        {
            int sizeOfT = Marshal.SizeOf(typeof(T));
            var unmanaged = Marshal.AllocHGlobal(sizeOfT * (int)count);
            var buffer = new byte[sizeOfT];

            output = new T[count];

            for (int i = 0; i < count; i++)
            {
                Read(buffer, 0, sizeOfT);
                Marshal.Copy(buffer, 0, unmanaged, sizeOfT);
                output[i] = (T)Marshal.PtrToStructure(unmanaged, typeof(T));
            }
            Marshal.FreeHGlobal(unmanaged);
        }

        public void ReadBlockArray<T>(out T[] output, int count)
        {
            int sizeOfT = Marshal.SizeOf(typeof(T));
            var unmanaged = Marshal.AllocHGlobal(sizeOfT * count);
            var buffer = new byte[sizeOfT];

            output = new T[count];

            for (int i = 0; i < count; i++)
            {
                Read(buffer, 0, sizeOfT);
                Marshal.Copy(buffer, 0, unmanaged, sizeOfT);
                output[i] = (T)Marshal.PtrToStructure(unmanaged, typeof(T));
            }
            Marshal.FreeHGlobal(unmanaged);
        }

        public string ReadStringUtf8()
        {
            int stringLength = ReadInt32();
            var stringData = ReadBytes(stringLength);
            string result = Encoding.UTF8.GetString(stringData);
            return result;
        }
    }
}
