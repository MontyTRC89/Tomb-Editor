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

        protected override void GizmoMove(Vector3 newPos)
        {

        }

        protected override void GizmoRotateX(float newAngle)
        {
            if (_control != null)
            {
                var model = _control.Model;
                var animation = _editor.CurrentAnim;
                if (animation == null || _control.SelectedMesh == null)
                    return;
                var meshIndex = model.Meshes.IndexOf(_control.SelectedMesh);

                foreach (var keyframe in _editor.ActiveFrames)
                {
                    var rotationVector = keyframe.Rotations[meshIndex];
                    float delta = newAngle - rotationVector.X;
                    keyframe.Quaternions[meshIndex] *= Quaternion.CreateFromAxisAngle(Vector3.UnitX, delta);
                    keyframe.Rotations[meshIndex] = MathC.QuaternionToEuler(keyframe.Quaternions[meshIndex]);

                    _control.Model.BuildAnimationPose(_editor.CurrentKeyFrame);
                }
                _control.Invalidate();
            }
        }

        protected override void GizmoRotateY(float newAngle)
        {
            if (_control != null)
            {
                var model = _control.Model;
                var animation = _editor.CurrentAnim;
                if (animation == null || _control.SelectedMesh == null)
                    return;
                var meshIndex = model.Meshes.IndexOf(_control.SelectedMesh);

                foreach (var keyframe in _editor.ActiveFrames)
                {
                    var rotationVector = keyframe.Rotations[meshIndex];
                    float delta = newAngle - rotationVector.Y;
                    keyframe.Quaternions[meshIndex] *= Quaternion.CreateFromAxisAngle(Vector3.UnitY, delta);
                    keyframe.Rotations[meshIndex] = MathC.QuaternionToEuler(keyframe.Quaternions[meshIndex]);

                    _control.Model.BuildAnimationPose(_editor.CurrentKeyFrame);
                }
                _control.Invalidate();
            }
        }

        protected override void GizmoRotateZ(float newAngle)
        {
            if (_control != null)
            {
                var model = _control.Model;
                var animation = _editor.CurrentAnim;
                if (animation == null || _control.SelectedMesh == null)
                    return;
                var meshIndex = model.Meshes.IndexOf(_control.SelectedMesh);

                foreach (var keyframe in _editor.ActiveFrames)
                {
                    var rotationVector = keyframe.Rotations[meshIndex];
                    float delta = newAngle - rotationVector.Z;
                    keyframe.Quaternions[meshIndex] *= Quaternion.CreateFromAxisAngle(Vector3.UnitZ, delta);
                    keyframe.Rotations[meshIndex] = MathC.QuaternionToEuler(keyframe.Quaternions[meshIndex]);

                    _control.Model.BuildAnimationPose(_editor.CurrentKeyFrame);
                }
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
                if (meshIndex != 0)
                    return;

                foreach (var keyframe in _editor.ActiveFrames)
                {
                    var translationVector = keyframe.Translations[meshIndex];
                    translationVector += delta;
                    keyframe.Translations[meshIndex] = translationVector;
                    keyframe.TranslationsMatrices[meshIndex] = Matrix4x4.CreateTranslation(translationVector);

                    _control.Model.BuildAnimationPose(_editor.CurrentKeyFrame);
                }
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

        protected override float CentreCubeSize => _configuration.GizmoAnimationEditor_CenterCubeSize;
        protected override float TranslationConeSize => _configuration.GizmoAnimationEditor_TranslationConeSize;
        protected override float Size => _configuration.GizmoAnimationEditor_Size;
        protected override float ScaleCubeSize => _configuration.GizmoAnimationEditor_ScaleCubeSize;
        protected override float LineThickness => _configuration.GizmoAnimationEditor_LineThickness;

        protected override bool SupportScale => false;
        protected override bool SupportTranslate
        {
            get
            {
                return (_control != null &&
                        _control.SelectedMesh != null &&
                        _control.Model.Meshes.IndexOf(_control.SelectedMesh) == 0);
            }
        }
        protected override bool SupportRotationY => true;
        protected override bool SupportRotationX => true;
        protected override bool SupportRotationZ => true;
    }
}
