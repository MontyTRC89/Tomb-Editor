using SharpDX;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TombLib.Graphics
{
    public abstract class Camera
    {
        public abstract Matrix GetViewProjectionMatrix(float width, float height);
    }
}
