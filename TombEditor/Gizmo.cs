using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpDX;
using TombLib.Graphics;
using SharpDX.Toolkit.Graphics;
using TombEditor.Geometry;
using System.Windows.Forms;

namespace TombEditor
{
    public class Gizmo : BaseGizmo
    {
        private Editor _editor;

        public Gizmo(GraphicsDevice device, Effect effect)
            : base(device, effect)
        {
            _editor = Editor.Instance;
        }

        public override bool DrawGizmo
        { 
            get
            {
                return _editor.SelectedObject is PositionBasedObjectInstance;
            }
        }

        public override Vector3 Position
        {
            get
            {
                var obj = (PositionBasedObjectInstance)_editor.SelectedObject;
                return obj.Position + obj.Room.WorldPos;
            }
        }

        public override void DoGizmoAction(Vector3 newPos)
        {
            EditorActions.MoveObject(_editor.SelectedObject as PositionBasedObjectInstance,
                                     newPos - _editor.SelectedObject.Room.WorldPos, Control.ModifierKeys);
        }

        public override GizmoAction Action => GizmoAction.Translate;

        protected override float CentreCubeSize => _editor.Configuration.Gizmo_CenterCubeSize;

        protected override float TranslationSphereSize => _editor.Configuration.Gizmo_TranslationSphereSize;

        protected override float Size => _editor.Configuration.Gizmo_Size;
    }
}
