using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace TombLib.Rendering
{
    public class RenderingState
    {
		public Matrix4x4 TransformMatrix = Matrix4x4.Identity;
        public float RoomGridLineWidth = 10.0f;
        public bool RoomGridForce = false;
        public bool RoomDisableVertexColors = false;
    }
    public abstract class RenderingStateBuffer : IDisposable
    {
        public abstract void Dispose();
        public abstract void Set(RenderingState State);
    }
}
