using System.Numerics;
using SharpDX.Toolkit.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TombLib.Graphics;
using WadTool.Controls;
using TombLib;

namespace WadTool
{
    public class GizmoAnimationEditor : BaseGizmo
    {
        private readonly Configuration _configuration;
        private readonly PanelRenderingAnimationEditor _control;
        private readonly AnimationEditor _editor;

        public GizmoAnimationEditor(AnimationEditor editor, GraphicsDevice device,
                                    Effect effect, PanelRenderingAnimationEditor control)
            : base(device, effect)
        {
            _editor = editor;
            _configuration = editor.Tool.Configuration;
            _control = control;
        }

        protected override void GizmoMove(Vector3 newPos) { } // Absorb event
        protected override void GizmoRotateX(float newAngle) => GizmoRotate(GizmoMode.RotateX, newAngle);
        protected override void GizmoRotateY(float newAngle) => GizmoRotate(GizmoMode.RotateY, newAngle);
        protected override void GizmoRotateZ(float newAngle) => GizmoRotate(GizmoMode.RotateZ, newAngle);

        private void GizmoRotate(GizmoMode mode, float newAngle)
        {
            if (_control != null)
            {
                var model = _control.Model;
                var animation = _editor.CurrentAnim;
                if (animation == null || _control.SelectedMesh == null)
                    return;

                var meshIndex = model.Meshes.IndexOf(_control.SelectedMesh);
                var rotationVector = _editor.CurrentKeyFrame.Rotations[meshIndex];
                var delta = 0f;
                var axis = Vector3.Zero;

                switch (mode)
                {
                    case GizmoMode.RotateX: delta = newAngle - rotationVector.X; axis = Vector3.UnitX; break;
                    case GizmoMode.RotateY: delta = newAngle - rotationVector.Y; axis = Vector3.UnitY; break;
                    case GizmoMode.RotateZ: delta = newAngle - rotationVector.Z; axis = Vector3.UnitZ; break;
                }

                var quat = _editor.CurrentKeyFrame.Quaternions[meshIndex] * Quaternion.CreateFromAxisAngle(axis, delta);
                _editor.UpdateTransform(meshIndex, MathC.QuaternionToEuler(quat), _editor.CurrentKeyFrame.Translations[0]);

                _control.Model.BuildAnimationPose(_editor.CurrentKeyFrame);
                _control.Invalidate();
            }
        }

        protected override void GizmoScale(float newScale) { }

        protected override void GizmoMoveDelta(Vector3 delta)
        {
            if (_control != null)
            {
                var model = _control.Model;
                var animation = _editor.CurrentAnim;
                if (animation == null || _control.SelectedMesh == null)
                    return;

                var meshIndex = model.Meshes.IndexOf(_control.SelectedMesh);
                var translationVector = _editor.CurrentKeyFrame.Translations[meshIndex];
                translationVector += delta;
                _editor.UpdateTransform(meshIndex, _editor.CurrentKeyFrame.Rotations[meshIndex], translationVector);

                _control.Model.BuildAnimationPose(_editor.CurrentKeyFrame);
                _control.Invalidate();
            }
        }

        protected override Vector3 Position
        {
            get
            {
                if (_control != null)
                {
                    var model = _control.Model;
                    var animation = _editor.CurrentAnim;
                    if (animation == null || _control.SelectedMesh == null)
                        return Vector3.Zero;
                    var meshIndex = model.Meshes.IndexOf(_control.SelectedMesh);
                    var centre = new Vector3(0, 0, 0);
                    return MathC.HomogenousTransform(centre, model.AnimationTransforms[meshIndex]);
                }
                else
                    return Vector3.Zero;
            }
        }

        protected override float RotationY
        {
            get
            {
                if (_control == null || _editor.CurrentAnim == null || _control.SelectedMesh == null ||
                    _editor.CurrentFrameIndex >= _editor.CurrentAnim.DirectXAnimation.KeyFrames.Count)
                    return 0;
                var meshIndex = _control.Model.Meshes.IndexOf(_control.SelectedMesh);
                return _editor.CurrentKeyFrame.Rotations[meshIndex].Y;
            }
        }

        protected override float RotationX
        {
            get
            {
                if (_control == null || _editor.CurrentAnim == null || _control.SelectedMesh == null ||
                    _editor.CurrentFrameIndex >= _editor.CurrentAnim.DirectXAnimation.KeyFrames.Count)
                    return 0;
                var meshIndex = _control.Model.Meshes.IndexOf(_control.SelectedMesh);
                return _editor.CurrentKeyFrame.Rotations[meshIndex].X;
            }
        }

        protected override float RotationZ
        {
            get
            {
                if (_control == null || _editor.CurrentAnim == null || _control.SelectedMesh == null ||
                    _editor.CurrentFrameIndex >= _editor.CurrentAnim.DirectXAnimation.KeyFrames.Count)
                    return 0;
                var meshIndex = _control.Model.Meshes.IndexOf(_control.SelectedMesh);
                return _editor.CurrentKeyFrame.Rotations[meshIndex].Z;
            }
        }

        protected override float Scale => 1.0f;
        protected override GizmoOrientation Orientation => GizmoOrientation.Normal;

        protected override float CentreCubeSize => _configuration.GizmoAnimationEditor_CenterCubeSize;
        protected override float TranslationConeSize => _configuration.GizmoAnimationEditor_TranslationConeSize;
        protected override float Size => _configuration.GizmoAnimationEditor_Size;
        protected override float ScaleCubeSize => _configuration.GizmoAnimationEditor_ScaleCubeSize;
        protected override float LineThickness => _configuration.GizmoAnimationEditor_LineThickness;

        protected override bool SupportScale => false;
        protected override bool SupportTranslateX
        {
            get
            {
                return (_control != null &&
                        _control.SelectedMesh != null &&
                        _control.Model.Meshes.IndexOf(_control.SelectedMesh) == 0);
            }
        }
        protected override bool SupportTranslateY => SupportTranslateX;
        protected override bool SupportTranslateZ => SupportTranslateX;
        protected override bool SupportRotationY => true;
        protected override bool SupportRotationX => true;
        protected override bool SupportRotationZ => true;
    }
}
