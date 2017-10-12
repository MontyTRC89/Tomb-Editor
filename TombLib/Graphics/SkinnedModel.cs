using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpDX.Toolkit.Graphics;
using SharpDX.Toolkit;
using SharpDX;
using TombLib.Graphics;
using Buffer = SharpDX.Toolkit.Graphics.Buffer;
using TombLib.Wad;

namespace TombLib.Graphics
{
    public class SkinnedModel : Model<SkinnedMesh, SkinnedVertex>
    {
        public Vector3 Offset;
        public Bone Root { get; set; }
        public List<Animation> Animations { get; set; } = new List<Animation>();
        public List<Bone> Bones { get; set; } = new List<Bone>();
        public List<Matrix> Transforms { get; set; } = new List<Matrix>();
        public List<Matrix> InverseTransforms { get; set; } = new List<Matrix>();
        public List<Matrix> AnimationTransforms { get; set; } = new List<Matrix>();

        public SkinnedModel(GraphicsDevice device)
            : base(device, ModelType.Skinned)
        {}
        
        public void ApplyTransforms()
        { }

        public void BuildHierarchy()
        {
            this.Root.GlobalTransform = Root.Transform;
            Transforms[Root.Index] = Root.GlobalTransform;
            InverseTransforms[Root.Index] = new Matrix(Root.GlobalTransform.ToArray());
            InverseTransforms[Root.Index].Invert();

            foreach (var node in this.Root.Children)
            {
                BuildHierarchy(node, this.Root.GlobalTransform, 0);
            }
        }

        private void BuildHierarchy(Bone node, Matrix parentTransform, int level)
        {
            node.GlobalTransform = node.Transform * parentTransform;
            Transforms[node.Index] = node.GlobalTransform;
            InverseTransforms[node.Index] = Matrix.Invert(node.GlobalTransform);

            foreach (var child in node.Children)
            {
                BuildHierarchy(child, node.GlobalTransform, level + 1);
            }
        }

        public void BuildAnimationPose(KeyFrame frame)
        {
            var globalScale = Matrix.Translation(Offset) * frame.Translations[0];
            AnimationTransforms[0] = frame.Rotations[0] * globalScale;//*Transforms[0];

            foreach (var node in this.Root.Children)
            {
                BuildAnimationPose(node, AnimationTransforms[0], 0, frame);
            }
        }

        private void BuildAnimationPose(Bone node, Matrix parentTransform, int level, KeyFrame frame)
        {
            AnimationTransforms[node.Index] = (frame.Rotations[node.Index] * node.Transform) * parentTransform;// *Transforms[node.Index];

            foreach (Bone child in node.Children)
            {
                BuildAnimationPose(child, AnimationTransforms[node.Index], level + 1, frame);
            }
        }

        public override void BuildBuffers()
        {
            int lastBaseIndex = 0;

            Vertices = new List<SkinnedVertex>();
            Indices = new List<int>();

            for (int i = 0; i < Meshes.Count; i++)
            {
                Vertices.AddRange(Meshes[i].Vertices);

                Meshes[i].BaseIndex = lastBaseIndex;
                Meshes[i].NumIndices = Meshes[i].Indices.Count;

                for (int j = 0; j < Meshes[i].Indices.Count; j++)
                {
                    Indices.Add((ushort)(lastBaseIndex + Meshes[i].Indices[j]));
                }

                lastBaseIndex += Meshes[i].Vertices.Count;
            }

            if (Vertices.Count == 0)
                return;

            VertexBuffer = Buffer.Vertex.New<SkinnedVertex>(GraphicsDevice, Vertices.ToArray<SkinnedVertex>(), SharpDX.Direct3D11.ResourceUsage.Dynamic);
            IndexBuffer = Buffer.Index.New(GraphicsDevice, Indices.ToArray(), SharpDX.Direct3D11.ResourceUsage.Dynamic);
        }

        public static SkinnedModel FromWad2(GraphicsDevice device, Wad2 wad, WadMoveable mov, List<WadTexture> reallocatedTextures)
        {
            SkinnedModel model = new SkinnedModel(device);
            model.Offset = mov.Offset;

            // Initialize the mesh
            for (int m = 0; m < mov.Meshes.Count; m++)
            {
                WadMesh msh = mov.Meshes[m];
                SkinnedMesh mesh = new SkinnedMesh(device, mov.ToString() + "_mesh_" + m.ToString());

                mesh.BoundingBox = msh.BoundingBox;
                mesh.BoundingSphere = msh.BoundingSphere;

                for (int j = 0; j < msh.Polys.Count; j++)
                {
                    WadPolygon poly = msh.Polys[j];
                  
                    if (poly.Shape == WadPolygonShape.Triangle)
                    {
                        int v1 = poly.Indices[0];
                        int v2 = poly.Indices[1];
                        int v3 = poly.Indices[2];

                        PutSkinnedVertexAndIndex(msh.VerticesPositions[v1], mesh, poly.UV[0], 0, m, poly.Texture.PositionInPackedTexture);
                        PutSkinnedVertexAndIndex(msh.VerticesPositions[v2], mesh, poly.UV[1], 0, m, poly.Texture.PositionInPackedTexture);
                        PutSkinnedVertexAndIndex(msh.VerticesPositions[v3], mesh, poly.UV[2], 0, m, poly.Texture.PositionInPackedTexture);
                    }
                    else
                    {
                        int v1 = poly.Indices[0];
                        int v2 = poly.Indices[1];
                        int v3 = poly.Indices[2];
                        int v4 = poly.Indices[3];

                        PutSkinnedVertexAndIndex(msh.VerticesPositions[v1], mesh, poly.UV[0], 0, m, poly.Texture.PositionInPackedTexture);
                        PutSkinnedVertexAndIndex(msh.VerticesPositions[v2], mesh, poly.UV[1], 0, m, poly.Texture.PositionInPackedTexture);
                        PutSkinnedVertexAndIndex(msh.VerticesPositions[v4], mesh, poly.UV[3], 0, m, poly.Texture.PositionInPackedTexture);

                        PutSkinnedVertexAndIndex(msh.VerticesPositions[v4], mesh, poly.UV[3], 0, m, poly.Texture.PositionInPackedTexture);
                        PutSkinnedVertexAndIndex(msh.VerticesPositions[v2], mesh, poly.UV[1], 0, m, poly.Texture.PositionInPackedTexture);
                        PutSkinnedVertexAndIndex(msh.VerticesPositions[v3], mesh, poly.UV[2], 0, m, poly.Texture.PositionInPackedTexture);
                    }
                }
                
                model.Meshes.Add(mesh);
            }

            // Initialize bones
            Bone root = new Bone();
            root.Name = "root_bone";
            root.Parent = null;
            root.Transform = Matrix.Identity;
            root.Index = 0;
            model.Bones.Add(root);
            model.Root = root;
            model.Transforms.Add(Matrix.Translation(Vector3.Zero));
            model.InverseTransforms.Add(Matrix.Translation(Vector3.Zero));
            model.AnimationTransforms.Add(Matrix.Translation(Vector3.Zero));

            for (int j = 0; j < mov.Meshes.Count - 1; j++)
            {
                Bone bone = new Bone();
                bone.Name = "bone_" + (j + 1).ToString();
                bone.Parent = null;
                bone.Transform = Matrix.Translation(Vector3.Zero);
                bone.Index = (short)(j + 1);
                model.Transforms.Add(Matrix.Translation(Vector3.Zero));
                model.InverseTransforms.Add(Matrix.Translation(Vector3.Zero));
                model.AnimationTransforms.Add(Matrix.Translation(Vector3.Zero));
                model.Bones.Add(bone);
            }

            Bone currentBone = root;
            Bone stackBone = root;
            Stack<Bone> stack = new Stack<Bone>();

            for (int m = 0; m < (mov.Meshes.Count - 1); m++)
            {
                int j = m + 1;
                WadLink link = mov.Links[m];

                switch (link.Opcode)
                {
                    case WadLinkOpcode.NotUseStack:
                        model.Bones[j].Transform = Matrix.Translation(link.Offset);
                        model.Bones[j].Parent = currentBone;
                        currentBone.Children.Add(model.Bones[j]);
                        currentBone = model.Bones[j];

                        break;
                    case WadLinkOpcode.Push:
                        if (stack.Count <= 0)
                            continue;
                        currentBone = stack.Pop();

                        model.Bones[j].Transform = Matrix.Translation(link.Offset);
                        model.Bones[j].Parent = currentBone;
                        currentBone.Children.Add(model.Bones[j]);
                        currentBone = model.Bones[j];

                        break;
                    case WadLinkOpcode.Pop:
                        stack.Push(currentBone);

                        model.Bones[j].Transform = Matrix.Translation(link.Offset);
                        model.Bones[j].Parent = currentBone;
                        currentBone.Children.Add(model.Bones[j]);
                        currentBone = model.Bones[j];

                        break;
                    case WadLinkOpcode.Read:
                        if (stack.Count <= 0)
                            continue;
                        Bone bone = stack.Pop();
                        model.Bones[j].Transform = Matrix.Translation(link.Offset);
                        model.Bones[j].Parent = bone;
                        bone.Children.Add(model.Bones[j]);
                        currentBone = model.Bones[j];
                        stack.Push(bone);

                        break;
                }
            }

            // Prepare animations
            for (int j = 0; j < mov.Animations.Count; j++)
            {
                Animation animation = new Animation();
                WadAnimation wadAnim = mov.Animations[j];

                animation.KeyFrames = new List<KeyFrame>();

                for (int f = 0; f < wadAnim.KeyFrames.Count; f++)
                {
                    KeyFrame frame = new KeyFrame();
                    WadKeyFrame wadFrame = wadAnim.KeyFrames[f];

                    for (int k = 0; k < mov.Meshes.Count; k++)
                    {
                        frame.Rotations.Add(Matrix.Identity);
                        frame.Translations.Add(Matrix.Identity);
                    }

                    frame.Translations[0] = Matrix.Translation(new Vector3(wadFrame.Offset.X, wadFrame.Offset.Y, wadFrame.Offset.Z));

                    for (int k = 1; k < frame.Translations.Count; k++)
                        frame.Translations[k] = Matrix.Translation(Vector3.Zero);

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

    }
}
