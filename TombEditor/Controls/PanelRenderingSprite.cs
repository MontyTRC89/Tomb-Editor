using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Numerics;
using TombEditor;
using TombLib.Controls;
using TombLib.Rendering;
using TombLib.Wad;

namespace WadTool.Controls
{
    public class PanelRenderingSprite : RenderingPanel
    {
        private RenderingTextureAllocator _renderingTextures;
        private readonly Editor _editor; 
        private List<WadSprite> _spriteList;

        private int _spriteID;
        public int SpriteID
        {
            get { return _spriteID; }
            set { _spriteID = value; Invalidate(); }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public Configuration Configuration { get; set; }

        public PanelRenderingSprite()
        {
            if (LicenseManager.UsageMode == LicenseUsageMode.Runtime)
            {
                _editor = Editor.Instance;
                _editor.EditorEventRaised += EditorEventRaised;
                RebuildSpriteList();
            }
        }

        private void EditorEventRaised(IEditorEvent obj)
        {
            if (obj is Editor.LoadedWadsChangedEvent ||
                obj is Editor.LevelChangedEvent ||
                obj is Editor.EditorFocusedEvent)
            {
                RebuildSpriteList();
                Invalidate();
            }
        }

        private void RebuildSpriteList() => _spriteList = _editor.Level.Settings.WadGetAllSprites();

        public override void InitializeRendering(RenderingDevice device, bool antialias)
        {
            base.InitializeRendering(device, antialias);
            _renderingTextures = device.CreateTextureAllocator(new RenderingTextureAllocator.Description());
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _editor.EditorEventRaised -= EditorEventRaised;
                _renderingTextures?.Dispose();
            }
            base.Dispose(disposing);
        }

        protected override void OnDraw()
        {
            ((TombLib.Rendering.DirectX11.Dx11RenderingDevice)Device).ResetState();

            if (_spriteList.Count <= SpriteID || SpriteID == -1)
                return;

            var sprite = _spriteList[SpriteID];
            float aspectRatioViewport = (float)ClientSize.Width / ClientSize.Height;
            float aspectRatioImage = (float)sprite.Texture.Image.Width / sprite.Texture.Image.Height;
            float aspectRatioAdjust = aspectRatioViewport / aspectRatioImage;
            var factor = Vector2.Min(new Vector2(1.0f / aspectRatioAdjust, aspectRatioAdjust), new Vector2(1.0f));

            SwapChain.RenderSprites(_renderingTextures, false, true, new List<Sprite>() { new Sprite
            {
                Texture = sprite.Texture.Image,
                PosStart = -0.9f * factor,
                PosEnd = 0.9f * factor
            } });
        }

        protected override Vector4 ClearColor => _editor.Configuration.UI_ColorScheme.Color3DBackground;
    }
}
