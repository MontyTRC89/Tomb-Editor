using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using TombLib;
using TombLib.Graphics;
using TombLib.LevelData;
using TombLib.LevelData.IO;
using TombLib.Rendering;
using TombLib.Utils;

namespace RenderingTest_TemporaryProject
{
    public partial class Form1 : Form
    {
        Room Room;
        RenderingDevice Device;
        RenderingTextureAllocator TextureAllocator;
        RenderingStateBuffer StateBuffer;
        RenderingDrawingRoom TestBatch0;
        RenderingDrawingTest TestBatch1;

        public Form1()
        {
            InitializeComponent();

            Level Level = PrjLoader.LoadFromPrj(@"D:\Eigenes\Spiele\TR\Levelbau\Levels\_Original\maps\tut1\Tut1.PRJ", new ProgressReporterSimple());
            Room = Level.Rooms[0]; // new Room(new Level(), new VectorInt2(20, 20), new Vector3(0.3f));

            // Create
            Device = new TombLib.Rendering.DirectX11.Dx11RenderingDevice();
            TextureAllocator = Device.CreateTextureAllocator();
            StateBuffer = Device.CreateStateBuffer();
            TestBatch0 = Device.CreateDrawingRoom(new RenderingDrawingRoom.Description
            {
                Room = Room,
                Allocator = TextureAllocator,
                SectorTextureGet = new SectorTextureDefault { SelectionArea = new RectangleInt2(0, 0, 20, 20), SelectionArrow = ArrowType.EntireFace }.Get
            });

            TestBatch1 = Device.CreateDrawingTest(new RenderingDrawingTest.Description { });
            renderingPanel1.InitializeRendering(Device);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                components?.Dispose();
                TextureAllocator.Dispose();
                StateBuffer.Dispose();
                Device.Dispose();
            }
            base.Dispose(disposing);

        }

        public void renderingPanel1_Draw(object sender, EventArgs e)
        {
            var Camera = new ArcBallCamera(Room.WorldPos + Room.GetLocalCenter(), 0.6f,
                (float)Math.PI, -(float)Math.PI / 2, (float)Math.PI / 2, 7000.0f, 2750, 1000000, 110 * (float)(Math.PI / 180));
            var Matrix = Camera.GetViewProjectionMatrix(renderingPanel1.ClientSize.Width, renderingPanel1.ClientSize.Height);
            StateBuffer.Set(new RenderingState { TransformMatrix = Matrix, RoomDisableVertexColors = true });
            TestBatch0.Render(new RenderingDrawingRoom.RenderArgs { RenderTarget = renderingPanel1.SwapChain, StateBuffer = StateBuffer });
            StateBuffer.Set(new RenderingState { TransformMatrix = Matrix4x4.Identity });
            TestBatch1.Render(new RenderingDrawingTest.RenderArgs { RenderTarget = renderingPanel1.SwapChain, StateBuffer = StateBuffer });
        }
    }
}
