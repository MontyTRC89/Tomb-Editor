using System;
using System.Numerics;
using TombLib.LevelData;

namespace TombLib.Rendering
{
    public abstract class RenderingDrawingRoom : IDisposable
    {
        public class Description
        {
            public Room Room;
            public RenderingTextureAllocator TextureAllocator;
            /// <summary>
            /// Suggests that the rendering batch is probably only going to be used one time. This may stop
            /// certain slightly expensive rendering optimizations that are not advantageous if they are only going to be used once.
            /// </summary>
            public bool HintOneTime = false;
			public Vector3 Offset = new Vector3();
            public SectorTextureGetDelegate SectorTextureGet = SectorTextureDefault.Default.Get; //(Room room, int x, int z, BlockFace face) => new SectorTextureResult();
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
