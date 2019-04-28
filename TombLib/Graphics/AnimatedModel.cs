using SharpDX.Toolkit.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using TombLib.Utils;
using TombLib.Wad;
using Buffer = SharpDX.Toolkit.Graphics.Buffer;

namespace TombLib.Graphics
{
    public class AnimatedModel : Model<ObjectMesh, ObjectVertex>
    {
        public Vector3 Offset;
        public Bone Root { get; set; }
        public List<Animation> Animations { get; set; } = new List<Animation>();
        public List<Bone> Bones { get; set; } = new List<Bone>();
        public List<Matrix4x4> BindPoseTransforms { get; set; } = new List<Matrix4x4>();
        public List<Matrix4x4> AnimationTransforms { get; set; } = new List<Matrix4x4>();

        public AnimatedModel(GraphicsDevice device)
            : base(device, ModelType.Skinned)
        {
            UpdateBuffers();
        }

        public override void UpdateBuffers()
        {
            foreach (var mesh in Meshes)
            {
                mesh.UpdateBoundingBox();
                mesh.UpdateBuffers();
            }
        }

        public void BuildHierarchy()
        {
            Root.GlobalTransform = Root.Transform;
            BindPoseTransforms[Root.Index] = Root.GlobalTransform;

            foreach (var node in Root.Children)
            {
                BuildHierarchy(node, Root.GlobalTransform);
            }
        }

        private void BuildHierarchy(Bone node, Matrix4x4 parentTransform)
        {
            node.GlobalTransform = node.Transform * parentTransform;
            BindPoseTransforms[node.Index] = node.GlobalTransform;

            foreach (var child in node.Children)
            {
                BuildHierarchy(child, node.GlobalTransform);
            }
        }

        public void UpdateAnimation(int animationIndex, int frameIndex)
        {
            if (animationIndex >= Animations.Count)
                return;
            var animation = Animations[animationIndex];
            int frameRate = Math.Max(animation.Framerate, (short)1);
            int keyFrameIndex1 = frameIndex / frameRate;
            int keyFrameIndex2 = keyFrameIndex1 + 1;
            if (keyFrameIndex1 >= animation.KeyFrames.Count || keyFrameIndex2 >= animation.KeyFrames.Count)
                return;
            float k = (frameIndex - keyFrameIndex1 * frameRate) / (float)frameRate;
            BuildAnimationPose(animation.KeyFrames[keyFrameIndex1], animation.KeyFrames[keyFrameIndex2], k);
        }

        public void BuildAnimationPose(KeyFrame frame)
        {
            var globalScale = Matrix4x4.CreateTranslation(Offset) * frame.TranslationsMatrices[0];
            AnimationTransforms[0] = Matrix4x4.CreateFromQuaternion(frame.Quaternions[0]) * globalScale;

            foreach (var node in Root.Children)
                BuildAnimationPose(node, AnimationTransforms[0], frame);
        }

        private void BuildAnimationPose(Bone node, Matrix4x4 parentTransform, KeyFrame frame)
        {
            AnimationTransforms[node.Index] = Matrix4x4.CreateFromQuaternion(frame.Quaternions[node.Index]) * node.Transform * parentTransform;

            foreach (Bone child in node.Children)
                BuildAnimationPose(child, AnimationTransforms[node.Index], frame);
        }

        public void BuildAnimationPose(KeyFrame frame1, KeyFrame frame2, float k)
        {
            Matrix4x4 translation = Matrix4x4.Lerp(frame1.TranslationsMatrices[0], frame2.TranslationsMatrices[0], k);
            Matrix4x4 rotation = Matrix4x4.CreateFromQuaternion(Quaternion.Slerp(frame1.Quaternions[0], frame2.Quaternions[0], k));

            var globalScale = Matrix4x4.CreateTranslation(Offset) * translation;
            AnimationTransforms[0] = rotation * globalScale;

            foreach (var node in Root.Children)
                BuildAnimationPose(node, AnimationTransforms[0], frame1, frame2, k);
        }

        private void BuildAnimationPose(Bone node, Matrix4x4 parentTransform, KeyFrame frame1, KeyFrame frame2, float k)
        {
            Matrix4x4 rotation = Matrix4x4.CreateFromQuaternion(Quaternion.Slerp(frame1.Quaternions[0], frame2.Quaternions[0], k));

            AnimationTransforms[node.Index] = rotation * node.Transform * parentTransform;

            foreach (Bone child in node.Children)
                BuildAnimationPose(child, AnimationTransforms[node.Index], frame1, frame2, k);
        }

        public static AnimatedModel FromWadMoveable(GraphicsDevice device, WadMoveable mov, Func<WadTexture, VectorInt2> allocateTexture)
        {
            AnimatedModel model = new AnimatedModel(device);
            List<WadBone> bones = mov.Bones;  

            // Create meshes
            for (int m = 0; m < bones.Count; m++)
                model.Meshes.Add(ObjectMesh.FromWad2(device, bones[m].Mesh, allocateTexture));

            // HACK: Add matrices here because if original WAD stack was corrupted, we could have broken parent - children
            // relations and so we could have meshes count different from matrices count
            for (int j = 0; j < bones.Count; j++)
            {
                model.BindPoseTransforms.Add(Matrix4x4.Identity);
                model.AnimationTransforms.Add(Matrix4x4.Identity);
            }

            // Build the skeleton
            model.Root = BuildSkeleton(model, null, bones);

            // Prepare animations
            for (int j = 0; j < mov.Animations.Count; j++)
                model.Animations.Add(Animation.FromWad2(bones, mov.Animations[j]));

            // Prepare data by loading the first valid animation and uploading data to the GPU
            model.BuildHierarchy();
            
            if (model.Animations.Count > 0 && model.Animations.Any(a => a.KeyFrames.Count > 0))
                model.BuildAnimationPose(model.Animations.FirstOrDefault(a => a.KeyFrames.Count > 0)?.KeyFrames[0]);
            model.UpdateBuffers();

            return model;
        }

        private static Bone BuildSkeleton(AnimatedModel model, Bone parentBone, List<WadBone> bones)
        {
            model.Bones = new List<Bone>();
            var stack = new Stack<Bone>();

            for (int i = 0; i < bones.Count; i++)
            {
                var nb = new Bone();
                nb.Name = bones[i].Name;
                nb.Index = i;
                model.Bones.Add(nb);
            }

            var currentBone = model.Bones[0];
            currentBone.Transform = Matrix4x4.Identity;
            currentBone.GlobalTransform = Matrix4x4.Identity;

            for (int j = 1; j < model.Bones.Count; j++)
            {
                int linkX = (int)bones[j].Translation.X;
                int linkY = (int)bones[j].Translation.Y;
                int linkZ = (int)bones[j].Translation.Z;

                switch (bones[j].OpCode)
                {
                    case WadLinkOpcode.NotUseStack:
                        model.Bones[j].Transform = Matrix4x4.CreateTranslation(linkX, linkY, linkZ);
                        model.Bones[j].Parent = currentBone;
                        currentBone.Children.Add(model.Bones[j]);
                        currentBone = model.Bones[j];

                        break;
                    case WadLinkOpcode.Pop:
                        if (stack.Count <= 0)
                            continue;
                        currentBone = stack.Pop();

                        model.Bones[j].Transform = Matrix4x4.CreateTranslation(linkX, linkY, linkZ);
                        model.Bones[j].Parent = currentBone;
                        currentBone.Children.Add(model.Bones[j]);
                        currentBone = model.Bones[j];

                        break;
                    case WadLinkOpcode.Push:
                        stack.Push(currentBone);

                        model.Bones[j].Transform = Matrix4x4.CreateTranslation(linkX, linkY, linkZ);
                        model.Bones[j].Parent = currentBone;
                        currentBone.Children.Add(model.Bones[j]);
                        currentBone = model.Bones[j];

                        break;
                    case WadLinkOpcode.Read:
                        if (stack.Count <= 0)
                            continue;
                        var bone = stack.Pop();
                        model.Bones[j].Transform = Matrix4x4.CreateTranslation(linkX, linkY, linkZ);
                        model.Bones[j].Parent = bone;
                        bone.Children.Add(model.Bones[j]);
                        currentBone = model.Bones[j];
                        stack.Push(bone);

                        break;
                }
            }
            
            return model.Bones[0];
        }
    }
}
