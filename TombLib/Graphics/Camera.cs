using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;

namespace TombLib.Graphics
{
    public abstract class Camera
    {
        public abstract Matrix4x4 GetViewProjectionMatrix(float width, float height);
    }
}
