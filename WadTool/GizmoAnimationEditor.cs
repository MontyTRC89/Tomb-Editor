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
        private readonly WadToolClass _tool;

        public GizmoAnimationEditor(WadToolClass tool, Configuration configuration, GraphicsDevice device,
                                    Effect effect, PanelRenderingAnimationEditor control)
            : base(device, effect, () => device.Clear(ClearOptions.DepthBuffer, SharpDX.Vector4.Zero, 1.0f, 0))
        {
            _configuration = configuration;
            _control = control;
            _tool = tool;
        }

        protected override void GizmoMove(Vector3 newPos)
        {

        }

        protected override void GizmoRotateX(float newAngle)
        {
            if (_control != null)
            {
                var model = _control.Model;
                var animation = _control.Animation;
                if (animation == null || _control.SelectedMesh == null)
                    return;
                var meshIndex = model.Meshes.IndexOf(_control.SelectedMesh);
                var keyframe = _control.Animation.DirectXAnimation.KeyFrames[_control.CurrentKeyFrame];
                var rotationVector = keyframe.Rotations[meshIndex];
                float delta = newAngle - rotationVector.X;
                keyframe.Quaternions[meshIndex] *= Quaternion.CreateFromAxisAngle(Vector3.UnitX, delta);
                keyframe.Rotations[meshIndex] = MathC.QuaternionToEuler(keyframe.Quaternions[meshIndex]);

                _control.Model.BuildAnimationPose(keyframe);
                _control.Invalidate();
            }
        }

        protected override void GizmoRotateY(float newAngle)
        {
            if (_control != null)
            {
                var model = _control.Model;
                var animation = _control.Animation;
                if (animation == null || _control.SelectedMesh == null)
                    return;
                var meshIndex = model.Meshes.IndexOf(_control.SelectedMesh);
                var keyframe = _control.Animation.DirectXAnimation.KeyFrames[_control.CurrentKeyFrame];
                var rotationVector = keyframe.Rotations[meshIndex];
                float delta = newAngle - rotationVector.Y;
                keyframe.Quaternions[meshIndex] *= Quaternion.CreateFromAxisAngle(Vector3.UnitY, delta);
                keyframe.Rotations[meshIndex] = MathC.QuaternionToEuler(keyframe.Quaternions[meshIndex]);

                _control.Model.BuildAnimationPose(keyframe);
                _control.Invalidate();
            }
        }

        protected override void GizmoRotateZ(float newAngle)
        {
            if (_control != null)
            {
                var model = _control.Model;
                var animation = _control.Animation;
                if (animation == null || _control.SelectedMesh == null)
                    return;
                var meshIndex = model.Meshes.IndexOf(_control.SelectedMesh);
                var keyframe = _control.Animation.DirectXAnimation.KeyFrames[_control.CurrentKeyFrame];
                var rotationVector = keyframe.Rotations[meshIndex];
                float delta = newAngle - rotationVector.Z;
                keyframe.Quaternions[meshIndex] *= Quaternion.CreateFromAxisAngle(Vector3.UnitZ, delta);
                keyframe.Rotations[meshIndex] = MathC.QuaternionToEuler(keyframe.Quaternions[meshIndex]);

                _control.Model.BuildAnimationPose(keyframe);
                _control.Invalidate();
            }
        }

        protected override void GizmoScale(float newScale) { }

        protected override void GizmoMoveDelta(Vector3 delta)
        {
            if (_control != null)
            {
                var model = _control.Model;
                var animation = _control.Animation;
                if (animation == null || _control.SelectedMesh == null)
                    return;
                var meshIndex = model.Meshes.IndexOf(_control.SelectedMesh);
                if (meshIndex != 0)
                    return;
                var keyframe = _control.Animation.DirectXAnimation.KeyFrames[_control.CurrentKeyFrame];
                var translationVector = keyframe.Translations[meshIndex];
                translationVector += delta;
                keyframe.Translations[meshIndex] = translationVector;
                keyframe.TranslationsMatrices[meshIndex] = Matrix4x4.CreateTranslation(translationVector);
                _control.Model.BuildAnimationPose(keyframe);
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
                    var animation = _control.Animation;
                    if (animation == null || _control.SelectedMesh == null)
                        return Vector3.Zero;
                    var meshIndex = model.Meshes.IndexOf(_control.SelectedMesh);
                    var centre = model.Meshes[meshIndex].BoundingBox.Center;
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
                if (_control == null || _control.Animation == null || _control.SelectedMesh == null ||
                    _control.CurrentKeyFrame >= _control.Animation.DirectXAnimation.KeyFrames.Count)
                    return 0;
                var meshIndex = _control.Model.Meshes.IndexOf(_control.SelectedMesh);
                return _control.Animation.DirectXAnimation.KeyFrames[_control.CurrentKeyFrame].Rotations[meshIndex].Y;
            }
        }

        protected override float RotationX
        {
            get
            {
                if (_control == null || _control.Animation == null || _control.SelectedMesh == null ||
                    _control.CurrentKeyFrame >= _control.Animation.DirectXAnimation.KeyFrames.Count)
                    return 0;
                var meshIndex = _control.Model.Meshes.IndexOf(_control.SelectedMesh);
                return _control.Animation.DirectXAnimation.KeyFrames[_control.CurrentKeyFrame].Rotations[meshIndex].X;
            }
        }

        protected override float RotationZ
        {
            get
            {
                if (_control == null || _control.Animation == null || _control.SelectedMesh == null ||
                    _control.CurrentKeyFrame >= _control.Animation.DirectXAnimation.KeyFrames.Count)
                    return 0;
                var meshIndex = _control.Model.Meshes.IndexOf(_control.SelectedMesh);
                return _control.Animation.DirectXAnimation.KeyFrames[_control.CurrentKeyFrame].Rotations[meshIndex].Z;
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
