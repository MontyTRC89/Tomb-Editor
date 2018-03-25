using SharpDX.Toolkit.Graphics;
using System.Numerics;
using TombLib;
using TombLib.Graphics;
using WadTool.Controls;

namespace WadTool
{
    public class GizmoSkeletonEditor : BaseGizmo
    {
        private readonly Configuration _configuration;
        private readonly PanelRenderingSkeleton _control;
        private readonly WadToolClass _tool;

        public GizmoSkeletonEditor(WadToolClass tool, Configuration configuration, GraphicsDevice device, 
                                   Effect effect, PanelRenderingSkeleton control)
            : base(device, effect)
        {
            _configuration = configuration;
            _control = control;
            _tool = tool;
        }

        protected override void GizmoMove(Vector3 newPos)
        {

        }

        protected override void GizmoRotateX(float newAngle) { }
        protected override void GizmoRotateY(float newAngle) { }
        protected override void GizmoRotateZ(float newAngle) { }
        protected override void GizmoScale(float newScale) { }

        protected override void GizmoMoveDelta(Vector3 delta)
        {
            // Move the bone offset
            _control.SelectedNode.Bone.Translation += delta;
            _tool.BoneOffsetMoved();

            // Draw scene
            _control.Invalidate();
        }

        protected override Vector3 Position => _control != null && _control.SelectedNode != null ? _control.SelectedNode.Centre : Vector3.Zero;
        protected override float RotationY => 0;
        protected override float RotationX => 0;
        protected override float RotationZ => 0;
        protected override float Scale => 1.0f;

        protected override float CentreCubeSize => _configuration.GizmoSkeleton_CenterCubeSize;
        protected override float TranslationConeSize => _configuration.GizmoSkeleton_TranslationConeSize;
        protected override float Size => _configuration.GizmoSkeleton_Size;
        protected override float ScaleCubeSize => _configuration.GizmoSkeleton_ScaleCubeSize;
        protected override float LineThickness => _configuration.GizmoSkeleton_LineThickness;

        protected override bool SupportScale => false;
        protected override bool SupportTranslate => true;
        protected override bool SupportRotationY => false;
        protected override bool SupportRotationX => false;
        protected override bool SupportRotationZ => false;
    }
}