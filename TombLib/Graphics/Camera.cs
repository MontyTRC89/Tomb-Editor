using SharpDX;

namespace TombLib.Graphics
{
    public abstract class Camera
    {
        public abstract Matrix GetViewProjectionMatrix(float width, float height);
    }
}
