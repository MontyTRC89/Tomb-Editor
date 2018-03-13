using SharpDX.Toolkit.Graphics;
using System.Numerics;
using TombLib.Graphics;
using WadTool.Controls;

namespace WadTool
{
    public class GizmoStaticEditor : BaseGizmo
    {
        private readonly Configuration _configuration;
        private readonly PanelRenderingStaticEditor _control;

        public GizmoStaticEditor(Configuration configuration, GraphicsDevice device, Effect effect, PanelRenderingStaticEditor control)
            : base(device, effect)
        {
            _configuration = configuration;
            _control = control;
        }

        protected override void GizmoMove(Vector3 newPos) => _control.StaticPosition = newPos;
        protected override void GizmoRotateX(float newAngle) => _control.StaticRotation = new Vector3(newAngle, _control.StaticRotation.Y, _control.StaticRotation.Z);
        protected override void GizmoRotateY(float newAngle) => _control.StaticRotation = new Vector3(_control.StaticRotation.X, newAngle, _control.StaticRotation.Z);
        protected override void GizmoRotateZ(float newAngle) => _control.StaticRotation = new Vector3(_control.StaticRotation.X, _control.StaticRotation.Y, newAngle);
        protected override void GizmoScale(float newScale)
        {
            // Set some limits to scale
            // TODO: object risks to be too small and to be not pickable. We should add some size check
            if (newScale < 1.0f)
                newScale = 1.0f;
            if (newScale > 128.0f)
                newScale = 128.0f;

            _control.StaticScale = newScale;
        }

        protected override Vector3 Position => _control != null ? _control.StaticPosition : Vector3.Zero;
        protected override float RotationY => _control.StaticRotation.Y;
        protected override float RotationX => _control.StaticRotation.X;
        protected override float RotationZ => _control.StaticRotation.Z;
        protected override float Scale => _control.StaticScale;

        protected override float CentreCubeSize => _configuration.Gizmo_CenterCubeSize;
        protected override float TranslationConeSize => _configuration.Gizmo_TranslationConeSize;
        protected override float Size => _configuration.Gizmo_Size;
        protected override float ScaleCubeSize => _configuration.Gizmo_ScaleCubeSize;
        protected override float LineThickness => _configuration.Gizmo_LineThickness;

        protected override bool SupportScale => true;
        protected override bool SupportTranslate => true;
        protected override bool SupportRotationY => true;
        protected override bool SupportRotationX => true;
        protected override bool SupportRotationZ => true;
    }
}
