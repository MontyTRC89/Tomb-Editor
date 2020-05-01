using SharpDX.Toolkit.Graphics;
using System;
using System.Numerics;
using System.Windows.Forms;
using TombLib.Graphics;
using TombLib.LevelData;
using TombLib.Rendering;

namespace TombEditor
{
    public class Gizmo : BaseGizmo
    {
        private readonly Editor _editor;

        public Gizmo(Effect effect)
            : base(DeviceManager.DefaultDeviceManager.___LegacyDevice, effect)
        {
            _editor = Editor.Instance;
        }

        protected override void GizmoMove(Vector3 newPos)
        {
            if (_editor.SelectedObject is PositionBasedObjectInstance)
                EditorActions.MoveObject(_editor.SelectedObject as PositionBasedObjectInstance,
                                     newPos - _editor.SelectedObject.Room.WorldPos, Control.ModifierKeys);
        }

        private float RotationQuanization(bool smoothByDefault = true)
        {
            if (Control.ModifierKeys.HasFlag(Keys.Shift))
                return 5.0f;
            else if (Control.ModifierKeys.HasFlag(Keys.Control))
                return smoothByDefault ? 45.0f : 0.0f;
            else
                return smoothByDefault ? 0.0f : 45.0f;
        }

        protected override void GizmoRotateY(float newAngle)
        {
            bool smoothRotationPreference = !(_editor.SelectedObject is MoveableInstance || _editor.SelectedObject is StaticInstance);
            EditorActions.RotateObject(_editor.SelectedObject, RotationAxis.Y, (float)(newAngle * (180 / Math.PI)), RotationQuanization(smoothRotationPreference), false, true);
        }

        protected override void GizmoRotateX(float newAngle)
        {
            EditorActions.RotateObject(_editor.SelectedObject, RotationAxis.X, -(float)(newAngle * (180 / Math.PI)), RotationQuanization(), false, true);
        }

        protected override void GizmoRotateZ(float newAngle)
        {
            EditorActions.RotateObject(_editor.SelectedObject, RotationAxis.Roll, (float)(newAngle * (180 / Math.PI)), RotationQuanization(), false, true);
        }

        protected override void GizmoScale(float scale)
        {
            bool quantized = Control.ModifierKeys.HasFlag(Keys.Control) | Control.ModifierKeys.HasFlag(Keys.Shift);
            EditorActions.ScaleObject(_editor.SelectedObject as IScaleable, scale, quantized ? Math.Sqrt(2) : 0.0f);
        }

        protected override void GizmoMoveDelta(Vector3 delta)
        {
            if (_editor.SelectedObject is GhostBlockInstance)
                ((GhostBlockInstance)_editor.SelectedObject).Move((int)(delta.Y / 256));
        }

        protected override Vector3 Position
        {
            get
            {
                if (_editor.SelectedObject is GhostBlockInstance)
                {
                    var ghost = (GhostBlockInstance)_editor.SelectedObject;

                    if (ghost.SelectedCorner.HasValue)
                        return ghost.ControlPositions(ghost.SelectedFloor)[(int)ghost.SelectedCorner];
                    else
                        return new Vector3();
                }
                else if (_editor.SelectedObject is PositionBasedObjectInstance)
                    return ((PositionBasedObjectInstance)_editor.SelectedObject).Position + _editor.SelectedObject.Room.WorldPos;
                else
                    return new Vector3();
            }
        }

        protected override float RotationY => (float)(((IRotateableY)_editor.SelectedObject).RotationY * (Math.PI / 180));
        protected override float RotationX => (float)(((IRotateableYX)_editor.SelectedObject).RotationX * -(Math.PI / 180));
        protected override float RotationZ => (float)(((IRotateableYXRoll)_editor.SelectedObject).Roll * (Math.PI / 180));
        protected override float Scale => ((IScaleable)_editor.SelectedObject).Scale;

        protected override float CentreCubeSize => _editor.Configuration.Gizmo_CenterCubeSize;
        protected override float TranslationConeSize => _editor.Configuration.Gizmo_TranslationConeSize;
        protected override float Size => _editor.Configuration.Gizmo_Size / (_editor.SelectedObject is GhostBlockInstance ? 2 : 1);
        protected override float ScaleCubeSize => _editor.Configuration.Gizmo_ScaleCubeSize;
        protected override float LineThickness => _editor.Configuration.Gizmo_LineThickness;

        protected override GizmoOrientation Orientation => (_editor.SelectedObject is GhostBlockInstance && 
                                                           !((GhostBlockInstance)_editor.SelectedObject).SelectedFloor) ? GizmoOrientation.UpsideDown : GizmoOrientation.Normal;

        protected override bool SupportTranslateX => _editor.SelectedObject is PositionBasedObjectInstance;
        protected override bool SupportTranslateZ => _editor.SelectedObject is PositionBasedObjectInstance;

        protected override bool SupportTranslateY => _editor.SelectedObject is PositionBasedObjectInstance || 
                                                    (_editor.SelectedObject is GhostBlockInstance && ((GhostBlockInstance)_editor.SelectedObject).SelectedCorner.HasValue);

        protected override bool SupportScale => _editor.SelectedObject is IScaleable;
        protected override bool SupportRotationY => _editor.SelectedObject is IRotateableY;
        protected override bool SupportRotationX => _editor.SelectedObject is IRotateableYX;
        protected override bool SupportRotationZ => _editor.SelectedObject is IRotateableYXRoll;
    }
}
