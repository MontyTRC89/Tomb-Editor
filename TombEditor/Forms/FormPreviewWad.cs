using DarkUI.Forms;
using System.Linq;
using System.Numerics;
using TombLib.Controls;
using TombLib.Rendering;
using TombLib.Wad;

namespace TombEditor.Forms
{
    public partial class FormPreviewWad : DarkForm
    {
        private Wad2 _wad;

        public FormPreviewWad(Wad2 wad, RenderingDevice device, Editor editor)
        {
            _wad = wad;
            InitializeComponent();

            panelItem.Editor = editor;
            panelItem.InitializeRendering(device, editor.Configuration.RenderingItem_Antialias);
            wadTree.Wad = wad;
            wadTree.MultiSelect = false;
            wadTree.SelectFirst();
        }

        private void wadTree_SelectedWadObjectIdsChanged(object sender, System.EventArgs e)
        {
            IWadObjectId selectedObjectId = wadTree.SelectedWadObjectIds.FirstOrDefault();
            panelItem.CurrentObject = selectedObjectId == null ? null : _wad.TryGet(selectedObjectId);
        }

        private void FormWadPreview_Deactivate(object sender, System.EventArgs e)
        {
            Close();
        }

        public class PanelRenderingItemPreview : PanelItemPreview
        {
            public Editor Editor { get; set; }

            protected override Vector4 ClearColor => Editor.Configuration.UI_ColorScheme.Color3DBackground;
            public override float FieldOfView => Editor.Configuration.RenderingItem_FieldOfView;
            public override float NavigationSpeedMouseWheelZoom => Editor.Configuration.RenderingItem_NavigationSpeedMouseWheelZoom;
            public override float NavigationSpeedMouseZoom => Editor.Configuration.RenderingItem_NavigationSpeedMouseZoom;
            public override float NavigationSpeedMouseTranslate => Editor.Configuration.RenderingItem_NavigationSpeedMouseTranslate;
            public override float NavigationSpeedMouseRotate => Editor.Configuration.RenderingItem_NavigationSpeedMouseRotate;
            public override bool ReadOnly => true;
        }
    }
}
