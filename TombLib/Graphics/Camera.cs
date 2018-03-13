using System.Numerics;

namespace TombLib.Graphics
{
    public abstract class Camera
    {
        public abstract Matrix4x4 GetViewProjectionMatrix(float width, float height);
    }
}
