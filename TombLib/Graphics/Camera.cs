using System;
using System.Numerics;

namespace TombLib.Graphics
{
    public abstract class Camera : ICloneable
    {
        public abstract object Clone();
        public abstract Matrix4x4 GetViewProjectionMatrix(float width, float height);
    }
}
