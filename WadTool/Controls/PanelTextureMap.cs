using System.ComponentModel;
using TombLib.Controls;

namespace WadTool.Controls
{
    public class PanelTextureMap : TextureMapBase
    {
        private WadToolClass _tool;

        protected override float TileSelectionSize => 32.0f;
        protected override bool ResetAttributesOnNewSelection => false;
        protected override bool MouseWheelMovesTheTextureInsteadOfZooming => _tool?.Configuration.MeshEditor_MouseWheelMovesTheTextureInsteadOfZooming ?? false;
        protected override float NavigationSpeedKeyMove => 100.0f;
        protected override float NavigationSpeedKeyZoom => 0.15f;
        protected override float NavigationSpeedMouseZoom => (_tool?.Configuration.RenderingItem_NavigationSpeedMouseZoom ?? 1.0f) * 0.00225f;
        protected override float NavigationSpeedMouseWheelZoom => (_tool?.Configuration.RenderingItem_NavigationSpeedMouseWheelZoom ?? 1.0f) * 0.00025f;
        protected override float NavigationMaxZoom => 2000.0f;
        protected override float NavigationMinZoom => 0.5f;
        protected override bool DrawSelectionDirectionIndicators => true;

        public PanelTextureMap() : base() { }

        public void Initialize(WadToolClass tool)
        {
            if (LicenseManager.UsageMode == LicenseUsageMode.Runtime)
            {
                _tool = tool;
                _tool.EditorEventRaised += EditorEventRaised;
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing && _tool != null)
                _tool.EditorEventRaised -= EditorEventRaised;
            base.Dispose(disposing);
        }

        private void EditorEventRaised(IEditorEvent obj)
        {
            //if (obj is WadToolClass.MeshEditorTextureChangedEvent)
            //    Invalidate();
        }
    }
}
