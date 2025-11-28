using System.Collections.Generic;
using System.IO;
using System.Numerics;
using System.Runtime.InteropServices;
using TombLib.LevelData.Compilers.TombEngine;
using TombLib.Utils;
using static SharpDX.Serialization.BinarySerializer;

namespace TombLib.IO
{
    public class BinaryWriterEx : BinaryWriter
    {
        public BinaryWriterEx(Stream output)
            : base(output)
        {}

        public BinaryWriterEx(Stream output, bool leaveOpen)
          : base(output, System.Text.Encoding.UTF8, leaveOpen)
        { }

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

        public void Write(Quaternion value)
        {
            Write(value.X);
            Write(value.Y);
            Write(value.Z);
            Write(value.W);
        }

        public void Write(Hash hash)
        {
            Write(hash.HashLow);
            Write(hash.HashHigh);
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

        public void Write(TombEngineKeyFrame keyFrame)
        {
            var center = new Vector3(
                keyFrame.BoundingBox.X1 + keyFrame.BoundingBox.X2,
                keyFrame.BoundingBox.Y1 + keyFrame.BoundingBox.Y2,
                keyFrame.BoundingBox.Z1 + keyFrame.BoundingBox.Z2) / 2;
            var extents = new Vector3(
                keyFrame.BoundingBox.X2 - keyFrame.BoundingBox.X1,
                keyFrame.BoundingBox.Y2 - keyFrame.BoundingBox.Y1,
                keyFrame.BoundingBox.Z2 - keyFrame.BoundingBox.Z1) / 2;

            Write(center);
            Write(extents);
            Write(keyFrame.RootOffset);

            Write(keyFrame.BoneOrientations.Count);
            WriteBlockArray(keyFrame.BoneOrientations);
        }
    }
}
