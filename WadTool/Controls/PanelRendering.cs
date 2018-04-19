using System;
using System.ComponentModel;
using System.Numerics;
using TombLib.Controls;

namespace WadTool.Controls
{
    public class PanelRendering : PanelItemPreview
    {
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public Configuration Configuration { get; set; }

        public override Vector4 BackgroundColor => Configuration.RenderingItem_BackgroundColor;
        public override float FieldOfView => Configuration.RenderingItem_FieldOfView;
        public override float NavigationSpeedMouseRotate => Configuration.RenderingItem_NavigationSpeedMouseRotate;
        public override float NavigationSpeedMouseTranslate => Configuration.RenderingItem_NavigationSpeedMouseTranslate;
        public override float NavigationSpeedMouseWheelZoom => Configuration.RenderingItem_NavigationSpeedMouseWheelZoom;
        public override float NavigationSpeedMouseZoom => Configuration.RenderingItem_NavigationSpeedMouseZoom;
        public override bool ReadOnly => false;
    }
}
