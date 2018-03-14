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
        {}

        public void ApplyTransforms()
        { }

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
            var globalScale = Matrix4x4.CreateTranslation(Offset) * frame.Translations[0];
            AnimationTransforms[0] = frame.Rotations[0] * globalScale;

            foreach (var node in Root.Children)
            {
                BuildAnimationPose(node, AnimationTransforms[0], frame);
            }
        }

        private void BuildAnimationPose(Bone node, Matrix4x4 parentTransform, KeyFrame frame)
        {
            AnimationTransforms[node.Index] = frame.Rotations[node.Index] * node.Transform * parentTransform;

            foreach (Bone child in node.Children)
            {
                BuildAnimationPose(child, AnimationTransforms[node.Index], frame);
            }
        }

        public void BuildAnimationPose(KeyFrame frame1, KeyFrame frame2, float k)
        {
            Matrix4x4 translation = Matrix4x4.Lerp(frame1.Translations[0], frame2.Translations[0], k);
            Matrix4x4 rotation = Matrix4x4.Lerp(frame1.Rotations[0], frame2.Rotations[0], k);

            var globalScale = Matrix4x4.CreateTranslation(Offset) * translation;
            AnimationTransforms[0] = rotation * globalScale;

            foreach (var node in Root.Children)
            {
                BuildAnimationPose(node, AnimationTransforms[0], frame1, frame2, k);
            }
        }

        private void BuildAnimationPose(Bone node, Matrix4x4 parentTransform, KeyFrame frame1, KeyFrame frame2, float k)
        {
            Matrix4x4 rotation = Matrix4x4.Lerp(frame1.Rotations[node.Index], frame2.Rotations[node.Index], k);

            AnimationTransforms[node.Index] = rotation * node.Transform * parentTransform;

            foreach (Bone child in node.Children)
            {
                BuildAnimationPose(child, AnimationTransforms[node.Index], frame1, frame2, k);
            }
        }

        public override void UpdateBuffers()
        {
            int lastBaseIndex = 0;

            Vertices = new List<ObjectVertex>();
            Indices = new List<int>();

            foreach (var mesh in Meshes)
            {
                Vertices.AddRange(mesh.Vertices);

                foreach (var submesh in mesh.Submeshes)
                {
                    submesh.Value.BaseIndex = lastBaseIndex;
                    foreach (var index in submesh.Value.Indices)
                        Indices.Add((ushort)(lastBaseIndex + index));
                    lastBaseIndex += submesh.Value.NumIndices;
                }

                mesh.UpdateBoundingBox();
            }

            if (Vertices.Count == 0)
                return;

            if (VertexBuffer != null)
                VertexBuffer.Dispose();
            if (IndexBuffer != null)
                IndexBuffer.Dispose();

            VertexBuffer = Buffer.Vertex.New(GraphicsDevice, Vertices.ToArray<ObjectVertex>(), SharpDX.Direct3D11.ResourceUsage.Dynamic);
            IndexBuffer = Buffer.Index.New(GraphicsDevice, Indices.ToArray(), SharpDX.Direct3D11.ResourceUsage.Dynamic);
        }

        public static AnimatedModel FromWad2(GraphicsDevice device, Wad2 wad, WadMoveable mov, Dictionary<WadTexture, VectorInt2> reallocatedTextures)
        {
            AnimatedModel model = new AnimatedModel(device);

            // Create meshes
            for (int m = 0; m < mov.Meshes.Count; m++)
            {
                WadMesh msh = mov.Meshes[m];
                var mesh = ObjectMesh.FromWad2(device, wad, msh, reallocatedTextures);
                if (!wad.DirectXMeshes.ContainsKey(msh))
                    wad.DirectXMeshes.TryAdd(msh, mesh);
                model.Meshes.Add(mesh);
            }

            // HACK: Add matrices here because if original WAD stack was corrupted, we could have broken parent - children
            // relations and so we could have meshes count different from matrices count
            for (int j = 0; j < mov.Meshes.Count; j++)
            {
                model.BindPoseTransforms.Add(Matrix4x4.Identity);
                model.AnimationTransforms.Add(Matrix4x4.Identity);
            }

            // Build the skeleton
            model.Root = BuildSkeleton(model, mov.Skeleton, null);

            // Prepare animations
            for (int j = 0; j < mov.Animations.Count; j++)
            {
                Animation animation = new Animation();
                WadAnimation wadAnim = mov.Animations[j];

                animation.Framerate = wadAnim.FrameDuration;

                animation.KeyFrames = new List<KeyFrame>();

                for (int f = 0; f < wadAnim.KeyFrames.Count; f++)
                {
                    KeyFrame frame = new KeyFrame();
                    WadKeyFrame wadFrame = wadAnim.KeyFrames[f];

                    for (int k = 0; k < mov.Meshes.Count; k++)
                    {
                        frame.Rotations.Add(Matrix4x4.Identity);
                        frame.Translations.Add(Matrix4x4.Identity);
                    }

                    frame.Translations[0] = Matrix4x4.CreateTranslation(new Vector3(wadFrame.Offset.X, wadFrame.Offset.Y, wadFrame.Offset.Z));

                    for (int k = 1; k < frame.Translations.Count; k++)
                        frame.Translations[k] = Matrix4x4.CreateTranslation(Vector3.Zero);

                    for (int n = 0; n < frame.Rotations.Count; n++)
                    {
                        frame.Rotations[n] = wadFrame.Angles[n].RotationMatrix;
                    }

                    animation.KeyFrames.Add(frame);
                }

                model.Animations.Add(animation);
            }

            // Prepare data by loading the first animation and uploading data to the GPU
            model.BuildHierarchy();
            if (model.Animations.Count > 0 && model.Animations[0].KeyFrames.Count > 0)
                model.BuildAnimationPose(model.Animations[0].KeyFrames[0]);

            model.UpdateBuffers();

            return model;
        }

        private static Bone BuildSkeleton(AnimatedModel model, WadBone bone, Bone parentBone)
        {
            Bone currentBone = new Bone();
            currentBone.Name = bone.Name;
            currentBone.Parent = parentBone;
            currentBone.Transform = bone.Transform;
            currentBone.Index = bone.Index;
            model.Bones.Add(currentBone);
            model.BindPoseTransforms[currentBone.Index] = bone.Transform;

            foreach (var childBone in bone.Children)
                currentBone.Children.Add(BuildSkeleton(model, childBone, currentBone));

            if (parentBone != null)
            {
                foreach (var childBone in currentBone.Children)
                    childBone.Parent = currentBone;
            }

            return currentBone;
        }
    }
}
