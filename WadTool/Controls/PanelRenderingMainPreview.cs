using System;
using System.ComponentModel;
using System.Numerics;
using TombLib.Rendering.Graphics.Preview;

namespace WadTool.Controls
{
    /// <summary>
    /// Main WAD preview panel for the WadTool main window. V2 RHI; lazily
    /// creates its own device + swapchain on first paint (see
    /// <see cref="ItemPreviewPanel"/>), so no <c>InitializeRendering</c> needed.
    /// </summary>
    public class PanelRenderingMainPreview : ItemPreviewPanel
    {
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public Configuration Configuration { get; set; }

        protected override Vector4 ClearColor => Configuration.RenderingItem_BackgroundColor;
        public override float FieldOfView => Configuration.RenderingItem_FieldOfView;
        public override float NavigationSpeedMouseRotate => Configuration.RenderingItem_NavigationSpeedMouseRotate;
        public override float NavigationSpeedMouseTranslate => Configuration.RenderingItem_NavigationSpeedMouseTranslate;
        public override float NavigationSpeedMouseWheelZoom => Configuration.RenderingItem_NavigationSpeedMouseWheelZoom;
        public override float NavigationSpeedMouseZoom => Configuration.RenderingItem_NavigationSpeedMouseZoom;
        public override bool ReadOnly => false;

        protected override void OnMouseEnter(EventArgs e)
        {
            // Absorb to prevent the V2 base from grabbing focus from
            // the WadTool main window's other controls.
        }
    }
}
