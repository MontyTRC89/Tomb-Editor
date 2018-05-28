using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace TombLib.Rendering
{
    public abstract class RenderingSwapChain : IDisposable
    {
        public class Description
        {
            public IntPtr WindowHandle;
            public VectorInt2 Size;
        };

        public abstract void Dispose();
        public abstract void Clear(Vector4 Color);
        public abstract void ClearDepth();
        public abstract void Present();
        public abstract void Resize(VectorInt2 newSize);
    }
}
