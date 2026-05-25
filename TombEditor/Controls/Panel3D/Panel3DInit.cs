namespace TombEditor.Controls.Panel3D
{
    public partial class Panel3D
    {
        public void InitializeRendering()
        {
            _renderer = new TombEditor.Rendering.LevelRenderer(
                Handle, ClientSize.Width, ClientSize.Height,
                _editor.Configuration.Rendering3D_Backend);

            // Headless BaseGizmo ctor -- picking + drag math only. The V2
            // GizmoRenderer reads the gizmo's PublicState snapshot at render
            // time and produces the visuals.
            _gizmo = new Gizmo();

            ResetCamera(true);
        }
    }
}
