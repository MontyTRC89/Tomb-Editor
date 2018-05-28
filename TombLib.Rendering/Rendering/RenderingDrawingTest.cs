using System;
using System.Numerics;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TombLib.Rendering
{
    public abstract class RenderingDrawingTest : IDisposable
    {
        public class Description
        {
            /// <summary>
            /// Suggests that the rendering batch is probably only going to be used one time. This may stop
            /// certain slightly expensive rendering optimizations that are not advantageous if they are only going to be used once.
            /// </summary>
            public bool HintOneTime = false;
        };

        public class RenderArgs
        {
            public RenderingSwapChain RenderTarget;
            public RenderingStateBuffer StateBuffer;
        }

        public abstract void Dispose();

        public abstract void Render(RenderArgs arg);
    }
}
