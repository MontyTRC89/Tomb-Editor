using SharpDX.Toolkit.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using TombLib.Graphics;
using WadTool.Controls;

namespace WadTool
{
    public class GizmoStaticEditorLight : BaseGizmo
    {
        private readonly Configuration _configuration;
        private readonly PanelRenderingStaticEditor _control;

        public GizmoStaticEditorLight(Configuration configuration, GraphicsDevice device, Effect effect, PanelRenderingStaticEditor control)
            : base(device, effect)
        {
            _configuration = configuration;
            _control = control;
        }

        protected override void GizmoMove(Vector3 newPos)
        {
            _control.SelectedLight.Position = newPos;
            _control.UpdateLights();
        }
        protected override void GizmoMoveDelta(Vector3 delta) { }
        protected override void GizmoRotateX(float newAngle) { }
        protected override void GizmoRotateY(float newAngle) { }
        protected override void GizmoRotateZ(float newAngle) { }
        protected override void GizmoScale(float newScale)
        {
            // Set some limits to scale
            // TODO: object risks to be too small and to be not pickable. We should add some size check
            if (newScale < 1.0f)
                newScale = 1.0f;
            if (newScale > 128.0f)
                newScale = 128.0f;

            _control.SelectedLight.Radius = newScale;
            _control.UpdateLights();
        }

        protected override Vector3 Position => _control != null && _control.SelectedLight != null ? _control.SelectedLight.Position : Vector3.Zero;
        protected override float RotationY => 0;
        protected override float RotationX => 0;
        protected override float RotationZ => 0;
        protected override float Scale => _control != null && _control.SelectedLight != null ? _control.SelectedLight.Radius : 0.0f;

        protected override float CentreCubeSize => _configuration.Gizmo_CenterCubeSize;
        protected override float TranslationConeSize => _configuration.Gizmo_TranslationConeSize;
        protected override float Size => _configuration.Gizmo_Size;
        protected override float ScaleCubeSize => _configuration.Gizmo_ScaleCubeSize;
        protected override float LineThickness => _configuration.Gizmo_LineThickness;

        protected override bool SupportScale => true;
        protected override bool SupportTranslate => true;
        protected override bool SupportRotationY => false;
        protected override bool SupportRotationX => false;
        protected override bool SupportRotationZ => false;
    }
}
