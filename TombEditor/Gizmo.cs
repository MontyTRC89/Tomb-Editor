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

        protected override Vector3 Position
        {
            get
            {
                var obj = (PositionBasedObjectInstance)_editor.SelectedObject;
                return obj.Position + obj.Room.WorldPos;
            }
        }

        protected override void GizmoMove(Vector3 newPos)
        {
            EditorActions.MoveObject(_editor.SelectedObject as PositionBasedObjectInstance,
                                     newPos - _editor.SelectedObject.Room.WorldPos, Control.ModifierKeys);
        }

        protected override void GizmoRotateX(float angle)
        {
            EditorActions.RotateObject(_editor.SelectedObject, EditorActions.RotationAxis.X, angle * (float)(180 / Math.PI));
        }

        protected override void GizmoRotateY(float angle)
        {
            EditorActions.RotateObject(_editor.SelectedObject, EditorActions.RotationAxis.Y, angle * (float)(180 / Math.PI));
        }

        protected override void GizmoRotateZ(float angle)
        {
            EditorActions.RotateObject(_editor.SelectedObject, EditorActions.RotationAxis.Y, angle * (float)(180 / Math.PI));
        }

        protected override void GizmoScale(float scale)
        {
            EditorActions.ScaleObject(_editor.SelectedObject as IScaleable, scale, Control.ModifierKeys);
        }

        protected override float CentreCubeSize => _editor.Configuration.Gizmo_CenterCubeSize;
        protected override float TranslationSphereSize => _editor.Configuration.Gizmo_TranslationSphereSize;
        protected override float Size => _editor.Configuration.Gizmo_Size;
        protected override float ScaleCubeSize => _editor.Configuration.Gizmo_ScaleCubeSize;
        protected override float LineThickness => _editor.Configuration.Gizmo_LineThickness;
        protected override bool SupportTranslate => _editor.SelectedObject is PositionBasedObjectInstance;
        protected override bool SupportScale => _editor.SelectedObject is IScaleable;
        protected override bool SupportRotationY => _editor.SelectedObject is IRotateableY;
        protected override bool SupportRotationX => _editor.SelectedObject is IRotateableYX;
        protected override bool SupportRotationZ => _editor.SelectedObject is IRotateableYXRoll;
    }
}
