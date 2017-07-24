using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SharpDX;
using SharpDX.Toolkit.Graphics;

namespace TombEditor
{
    public enum PickingElementType : int
    {
        None = 63,
        Block = 1,
        SkinnedModel = 2,
        StaticModel = 3,
        Light = 4,
        Camera = 5,
        SoundSource = 6,
        FogBulb = 7,
        CollisionBlock = 8,
        Path = 9,
        Sink = 10,
        Portal = 11,
        Trigger = 12,
        FlyByCamera = 13
    }

    public struct PickingResult
    {
        public PickingElementType ElementType;
        public int SubElementType;
        public int Element;
        public int SubElement;
    }

    public class PickingManager
    {
        private static PickingManager _instance;

        private RenderTarget2D _renderTarget1;

        private RenderTarget2D _renderTarget2;

        private RenderTarget2D _oldTarget;

        private RenderTarget2D _depthTarget;

        private Editor _editor;

        public delegate void PickingDrawDelegate();

        private PickingManager()
        {
            _editor = Editor.Instance;
            _renderTarget1 = RenderTarget2D.New(_editor.GraphicsDevice, _editor.GraphicsDevice.BackBuffer.Width, _editor.GraphicsDevice.BackBuffer.Height,
                                                MSAALevel.None, _editor.GraphicsDevice.BackBuffer.Description.Format);
            _renderTarget2 = RenderTarget2D.New(_editor.GraphicsDevice, _editor.GraphicsDevice.BackBuffer.Width, _editor.GraphicsDevice.BackBuffer.Height,
                                                MSAALevel.None, _editor.GraphicsDevice.BackBuffer.Description.Format);
            _depthTarget = RenderTarget2D.New(_editor.GraphicsDevice, _editor.GraphicsDevice.BackBuffer.Width, _editor.GraphicsDevice.BackBuffer.Height,
                                                MSAALevel.None, PixelFormat.R32.Float);
        }

        public void BeginPicking()
        {
            // imposto il selection buffer
            _oldTarget = _editor.GraphicsDevice.BackBuffer;
            _editor.GraphicsDevice.SetRenderTargets(_editor.GraphicsDevice.DepthStencilBuffer, _renderTarget1, _renderTarget2);
            _editor.GraphicsDevice.SetViewports(0, 0, _renderTarget1.Width, _renderTarget1.Height);

            // disegno un quadrato bianco a tutto schermo per cancellare il vecchio selection buffer
            Effect effect = _editor.Effects["ClearSelectionBuffer"];
            effect.CurrentTechnique.Passes[0].Apply();
            _editor.GraphicsDevice.DrawQuad();

            // cancello lo Z-buffer in modo che posso disegnare sopra il bianco
            _editor.GraphicsDevice.Clear(ClearOptions.DepthBuffer, Color.Yellow, 1.0f, 0);
            _editor.GraphicsDevice.SetDepthStencilState(_editor.GraphicsDevice.DepthStencilStates.Default);
        }

        public PickingResult EndPicking(float x, float y)
        {
            _editor.GraphicsDevice.SetRenderTargets(_oldTarget);

            byte[] data = _renderTarget1.GetData<byte>();

            byte r = data[(int)(y * _editor.GraphicsDevice.Viewport.Width * 4 + 4 * x)];
            byte g = data[(int)(y * _editor.GraphicsDevice.Viewport.Width * 4 + 4 * x + 1)];
            byte b = data[(int)(y * _editor.GraphicsDevice.Viewport.Width * 4 + 4 * x + 2)];
            byte a = data[(int)(y * _editor.GraphicsDevice.Viewport.Width * 4 + 4 * x + 3)];

            PickingResult result = new PickingResult();
            
            int elementType = (r >> 2) & 0x3f;
            int subelementType = r & 0x03;
            int element = (g << 2) + ((b >> 6) & 0x03);

            data = _renderTarget2.GetData<byte>();

            r = data[(int)(y * _editor.GraphicsDevice.Viewport.Width * 4 + 4 * x)];
            
            int subelement = (b & 0x3f) + r;

            result.Element = element;
            result.ElementType = (PickingElementType)elementType;
            result.SubElement = subelement;
            result.SubElementType = subelementType ;

            _renderTarget1.Save("D:\\rt1.png", ImageFileType.Png);
            _renderTarget2.Save("D:\\rt2.png", ImageFileType.Png);

            return result;
        }

        public float EndPickingDepth(float x, float y)
        {
            _editor.GraphicsDevice.SetRenderTargets(_oldTarget);

            float[] data = _depthTarget.GetData<float>();

            return data[(int)(y * _editor.GraphicsDevice.Viewport.Width + x)];
        }

        public static PickingManager Instance
        {
            get
            {
                if (_instance == null) _instance = new PickingManager();
                return _instance;
            }
        }

        public Vector4 PreparePickingColor(PickingElementType type, int subelementType, int element, int subelement)
        {
            float r = (((int)type) << 2) + (subelementType & 0x03);
            float g = element >> 2;
            float b = ((element & 0x03) << 6) + ((subelement & 0x3f00) >> 8);
            float a = (subelement & 0xff);

            r /= 255.0f;
            g /= 255.0f;
            b /= 255.0f;
            a /= 255.0f;

            return new Vector4(r, g, b, a);
        }
    }
}