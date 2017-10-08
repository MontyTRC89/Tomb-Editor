using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpDX;
using TombLib.Graphics;
using SharpDX.Toolkit.Graphics;
using WadTool.Controls;

namespace WadTool
{
    public class GizmoStaticMeshEditor : BaseGizmo
    {
        private WadToolClass _tool;
        private PanelRenderingStaticMeshEditor _control;

        public GizmoStaticMeshEditor(GraphicsDevice device, Effect effect, PanelRenderingStaticMeshEditor control)
            : base(device, effect)
        {
            _tool = WadToolClass.Instance;
            _control = control;
        }

        protected override Vector3 Position => (_control != null ? _control.StaticPosition : Vector3.Zero);
        protected override float CentreCubeSize => 128.0f;
        protected override float TranslationSphereSize => 128.0f;
        protected override float ScaleCubeSize => 128.0f;
        protected override float Size => 1024.0f;
        protected override bool SupportScale => true;
        protected override bool SupportTranslate => true;
        protected override bool SupportRotationY => true;
        protected override bool SupportRotationX => true;
        protected override bool SupportRotationZ => true;

        protected override void DoGizmoAction(Vector3 newPos, float angle, float scale)
        {
            if (Action == GizmoAction.Translate)
            {
                _control.StaticPosition = newPos;
            }
            else if (Action == GizmoAction.Rotate)
            {
                switch (Axis)
                {
                    case GizmoAxis.X:
                        _control.StaticRotation += new Vector3(angle, 0, 0);
                        break;
                    case GizmoAxis.Y:
                        _control.StaticRotation += new Vector3(0, angle, 0);
                        break;
                    case GizmoAxis.Z:
                        _control.StaticRotation += new Vector3(0, 0, angle);
                        break;
                }
            }
            else if (Action == GizmoAction.Scale)
            {
                float newScale = scale / 1024.0f; // TODO: adjust
                newScale += _control.StaticScale;

                // Set some limits to scale
                // TODO: object risks to be too small and to be not pickable. We should add some size check
                if (newScale < 1.0f) newScale = 1.0f;
                if (newScale > 128.0f) newScale = 128.0f;

                _control.StaticScale = newScale;
            }

            _control.Invalidate();
        }
    }
}
