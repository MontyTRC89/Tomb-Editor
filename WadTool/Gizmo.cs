using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpDX;
using TombLib.Graphics;
using SharpDX.Toolkit.Graphics;

namespace WadTool
{
    public class Gizmo : BaseGizmo
    {
        private WadToolClass _tool;

        public Gizmo(GraphicsDevice device, Effect effect)
            : base(device, effect)
        {
            _tool = WadToolClass.Instance;
        }

        protected override bool DrawGizmo => true;

        protected override Vector3 Position => Vector3.Zero;

        protected override float CentreCubeSize => 128.0f;

        protected override float TranslationSphereSize => 128.0f;

        protected override float ScaleCubeSize => 128.0f;

        protected override float Size => 1024.0f;

        protected override bool SupportScale => true;

        protected override bool SupportRotationY => true;

        protected override bool SupportRotationYX => true;

        protected override bool SupportRotationYXRoll => true;

        protected override void DoGizmoAction(Vector3 newPos, float angle, float scale)
        {
            
        }
    }
}
