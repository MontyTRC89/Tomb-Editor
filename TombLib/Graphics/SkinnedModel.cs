using SharpDX.Toolkit;
using SharpDX.Toolkit.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using TombLib.Graphics;
using TombLib.Utils;
using TombLib.Wad;
using Buffer = SharpDX.Toolkit.Graphics.Buffer;

namespace TombLib.Graphics
{
    public class SkinnedModel : Model<SkinnedMesh, SkinnedVertex>
    {
        public Vector3 Offset;
        public Bone Root { get; set; }
        public List<Animation> Animations { get; set; } = new List<Animation>();
        public List<Bone> Bones { get; set; } = new List<Bone>();
        public List<Matrix4x4> BindPoseTransforms { get; set; } = new List<Matrix4x4>();
        public List<Matrix4x4> AnimationTransforms { get; set; } = new List<Matrix4x4>();

        public SkinnedModel(GraphicsDevice device)
            : base(device, ModelType.Skinned)
        {}

        public void ApplyTransforms()
        { }

        public void BuildHierarchy()
        {
            this.Root.GlobalTransform = Root.Transform;
            BindPoseTransforms[Root.Index] = Root.GlobalTransform;

            foreach (var node in this.Root.Children)
            {
                BuildHierarchy(node, this.Root.GlobalTransform, 0);
            }
        }

        private void BuildHierarchy(Bone node, Matrix4x4 parentTransform, int level)
        {
            node.GlobalTransform = node.Transform * parentTransform;
            BindPoseTransforms[node.Index] = node.GlobalTransform;

            foreach (var child in node.Children)
            {
                BuildHierarchy(child, node.GlobalTransform, level + 1);
            }
        }

        public void UpdateAnimation(int animationIndex, int frameIndex)
        {
            var animation = Animations[animationIndex];
            int frameRate = (int)Math.Max(animation.Framerate, (short)1);
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

            foreach (var node in this.Root.Children)
            {
                BuildAnimationPose(node, AnimationTransforms[0], 0, frame);
            }
        }

        private void BuildAnimationPose(Bone node, Matrix4x4 parentTransform, int level, KeyFrame frame)
        {
            AnimationTransforms[node.Index] = (frame.Rotations[node.Index] * node.Transform) * parentTransform;

            foreach (Bone child in node.Children)
            {
                BuildAnimationPose(child, AnimationTransforms[node.Index], level + 1, frame);
            }
        }

        public void BuildAnimationPose(KeyFrame frame1, KeyFrame frame2, float k)
        {
            Matrix4x4 translation = Matrix4x4.Lerp(frame1.Translations[0], frame2.Translations[0], k);
            Matrix4x4 rotation = Matrix4x4.Lerp(frame1.Rotations[0], frame2.Rotations[0], k);

            var globalScale = Matrix4x4.CreateTranslation(Offset) * translation;
            AnimationTransforms[0] = rotation * globalScale;

            foreach (var node in this.Root.Children)
            {
                BuildAnimationPose(node, AnimationTransforms[0], 0, frame1, frame2, k);
            }
        }

        private void BuildAnimationPose(Bone node, Matrix4x4 parentTransform, int level, KeyFrame frame1, KeyFrame frame2, float k)
        {
            Matrix4x4 rotation = Matrix4x4.Lerp(frame1.Rotations[node.Index], frame2.Rotations[node.Index], k);

            AnimationTransforms[node.Index] = (rotation * node.Transform) * parentTransform;

            foreach (Bone child in node.Children)
            {
                BuildAnimationPose(child, AnimationTransforms[node.Index], level + 1, frame1, frame2, k);
            }
        }

        public override void BuildBuffers()
        {
            int lastBaseIndex = 0;

            Vertices = new List<SkinnedVertex>();
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

            VertexBuffer = Buffer.Vertex.New<SkinnedVertex>(GraphicsDevice, Vertices.ToArray<SkinnedVertex>(), SharpDX.Direct3D11.ResourceUsage.Dynamic);
            IndexBuffer = Buffer.Index.New(GraphicsDevice, Indices.ToArray(), SharpDX.Direct3D11.ResourceUsage.Dynamic);
        }

        public static SkinnedModel FromWad2(GraphicsDevice device, Wad2 wad, WadMoveable mov, List<WadTexture> reallocatedTextures)
        {
            SkinnedModel model = new SkinnedModel(device);

            // Prepare materials
            var materialOpaque = new Material(Material.Material_Opaque + "_0_0_0_0", null, false, false, 0);
            var materialOpaqueDoubleSided = new Material(Material.Material_OpaqueDoubleSided + "_0_0_1_0", null, false, true, 0);
            var materialAdditiveBlending = new Material(Material.Material_AdditiveBlending + "_0_1_0_0", null, true, false, 0);
            var materialAdditiveBlendingDoubleSided = new Material(Material.Material_AdditiveBlendingDoubleSided + "_0_1_1_0", null, true, true, 0);

            model.Materials.Add(materialOpaque);
            model.Materials.Add(materialOpaqueDoubleSided);
            model.Materials.Add(materialAdditiveBlending);
            model.Materials.Add(materialAdditiveBlendingDoubleSided);

            // Initialize the mesh
            for (int m = 0; m < mov.Meshes.Count; m++)
            {
                WadMesh msh = mov.Meshes[m];
                var mesh = new SkinnedMesh(device, mov.ToString() + "_mesh_" + m.ToString());

                mesh.Submeshes.Add(materialOpaque, new Submesh(materialOpaque));
                mesh.Submeshes.Add(materialOpaqueDoubleSided, new Submesh(materialOpaqueDoubleSided));
                mesh.Submeshes.Add(materialAdditiveBlending, new Submesh(materialAdditiveBlending));
                mesh.Submeshes.Add(materialAdditiveBlendingDoubleSided, new Submesh(materialAdditiveBlendingDoubleSided));

                mesh.BoundingBox = msh.BoundingBox;
                mesh.BoundingSphere = msh.BoundingSphere;

                for (int j = 0; j < msh.Polys.Count; j++)
                {
                    WadPolygon poly = msh.Polys[j];
                    Vector2 positionInPackedTexture = ((WadTexture)(poly.Texture.Texture)).PositionInTextureAtlas;

                    // Get the right submesh
                    var submesh = mesh.Submeshes[materialOpaque];
                    if (poly.Texture.BlendMode == BlendMode.Additive)
                    {
                        if (poly.Texture.DoubleSided)
                            submesh = mesh.Submeshes[materialAdditiveBlendingDoubleSided];
                        else
                            submesh = mesh.Submeshes[materialAdditiveBlending];
                    }
                    else
                    {
                        if (poly.Texture.DoubleSided)
                            submesh = mesh.Submeshes[materialOpaqueDoubleSided];
                    }

                    if (poly.Shape == WadPolygonShape.Triangle)
                    {
                        int v1 = poly.Index0;
                        int v2 = poly.Index1;
                        int v3 = poly.Index2;

                        PutSkinnedVertexAndIndex(msh.VerticesPositions[v1], mesh, submesh, poly.Texture.TexCoord0, 0, m, positionInPackedTexture);
                        PutSkinnedVertexAndIndex(msh.VerticesPositions[v2], mesh, submesh, poly.Texture.TexCoord1, 0, m, positionInPackedTexture);
                        PutSkinnedVertexAndIndex(msh.VerticesPositions[v3], mesh, submesh, poly.Texture.TexCoord2, 0, m, positionInPackedTexture);
                    }
                    else
                    {
                        int v1 = poly.Index0;
                        int v2 = poly.Index1;
                        int v3 = poly.Index2;
                        int v4 = poly.Index3;

                        PutSkinnedVertexAndIndex(msh.VerticesPositions[v1], mesh, submesh, poly.Texture.TexCoord0, 0, m, positionInPackedTexture);
                        PutSkinnedVertexAndIndex(msh.VerticesPositions[v2], mesh, submesh, poly.Texture.TexCoord1, 0, m, positionInPackedTexture);
                        PutSkinnedVertexAndIndex(msh.VerticesPositions[v4], mesh, submesh, poly.Texture.TexCoord3, 0, m, positionInPackedTexture);

                        PutSkinnedVertexAndIndex(msh.VerticesPositions[v4], mesh, submesh, poly.Texture.TexCoord3, 0, m, positionInPackedTexture);
                        PutSkinnedVertexAndIndex(msh.VerticesPositions[v2], mesh, submesh, poly.Texture.TexCoord1, 0, m, positionInPackedTexture);
                        PutSkinnedVertexAndIndex(msh.VerticesPositions[v3], mesh, submesh, poly.Texture.TexCoord2, 0, m, positionInPackedTexture);
                    }
                }

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
            model.Root = BuildSkeleton(model, mov, mov.Skeleton, null);

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

            model.BuildBuffers();

            return model;
        }

        private static Bone BuildSkeleton(SkinnedModel model, WadMoveable mov, WadBone bone, Bone parentBone)
        {
            Bone currentBone = new Bone();
            currentBone.Name = bone.Name;
            currentBone.Parent = parentBone;
            currentBone.Transform = bone.Transform;
            currentBone.Index = bone.Index;
            model.Bones.Add(currentBone);
            model.BindPoseTransforms[currentBone.Index] = bone.Transform;

            foreach (var childBone in bone.Children)
                currentBone.Children.Add(BuildSkeleton(model, mov, childBone, currentBone));

            if (parentBone != null)
            {
                foreach (var childBone in currentBone.Children)
                    childBone.Parent = currentBone;
            }

            return currentBone;
        }
    }
}
