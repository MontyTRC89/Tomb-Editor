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

        public static SkinnedModel FromWad(GraphicsDevice device, WadMoveable mov,
            Dictionary<uint, WadTexturePage> texturePages, Dictionary<uint, WadTextureSample> textureSamples)
        {
            SkinnedModel model = new SkinnedModel(device);
            model.Offset = mov.Offset;

            // Initialize the mesh
            for (int m = 0; m < mov.Meshes.Count; m++)
            {
                WadMesh msh = mov.Meshes[m];
                SkinnedMesh mesh = new SkinnedMesh(device, mov.ToString() + "_mesh_" + m.ToString());

                mesh.BoundingBox = msh.BoundingBox;

                for (int j = 0; j < texturePages.Count; j++)
                {
                    Submesh submesh = new Submesh();
                    submesh.Material = new TombLib.Graphics.Material();
                    submesh.Material.Type = MaterialType.Flat;
                    submesh.Material.Name = "material_" + j.ToString();
                    submesh.Material.DiffuseMap = (uint)j;
                    mesh.SubMeshes.Add(submesh);
                }

                for (int j = 0; j < msh.Polygons.Length; j++)
                {
                    WadPolygon poly = msh.Polygons[j];
                    int textureId = poly.Texture & 0xfff;
                    if (textureId > 2047)
                        textureId = -(textureId - 4096);
                    short submeshIndex = textureSamples[(uint)textureId].Page;

                    List<Vector2> uv = CalculateUVCoordinates(poly, textureSamples);

                    if (poly.Shape == Shape.Triangle)
                    {
                        AddSkinnedVertexAndIndex(msh.Vertices[poly.V1], mesh, uv[0], submeshIndex, m);
                        AddSkinnedVertexAndIndex(msh.Vertices[poly.V2], mesh, uv[1], submeshIndex, m);
                        AddSkinnedVertexAndIndex(msh.Vertices[poly.V3], mesh, uv[2], submeshIndex, m);
                    }
                    else
                    {
                        AddSkinnedVertexAndIndex(msh.Vertices[poly.V1], mesh, uv[0], submeshIndex, m);
                        AddSkinnedVertexAndIndex(msh.Vertices[poly.V2], mesh, uv[1], submeshIndex, m);
                        AddSkinnedVertexAndIndex(msh.Vertices[poly.V4], mesh, uv[3], submeshIndex, m);

                        AddSkinnedVertexAndIndex(msh.Vertices[poly.V4], mesh, uv[3], submeshIndex, m);
                        AddSkinnedVertexAndIndex(msh.Vertices[poly.V2], mesh, uv[1], submeshIndex, m);
                        AddSkinnedVertexAndIndex(msh.Vertices[poly.V3], mesh, uv[2], submeshIndex, m);
                    }
                }

                for (int j = 0; j < mesh.SubMeshes.Count; j++)
                {
                    Submesh current = mesh.SubMeshes[j];
                    current.StartIndex = (ushort)mesh.Indices.Count;
                    for (int k = 0; k < current.Indices.Count; k++)
                        mesh.Indices.Add(current.Indices[k]);
                    current.NumIndices = (ushort)current.Indices.Count;
                }

                mesh.BoundingSphere = new BoundingSphere(new Vector3(msh.SphereX, msh.SphereY, msh.SphereZ), msh.Radius);

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
                        model.Bones[j].Transform = Matrix.Translation(new Vector3(link.X, -link.Y, link.Z));
                        model.Bones[j].Parent = currentBone;
                        currentBone.Children.Add(model.Bones[j]);
                        currentBone = model.Bones[j];

                        break;
                    case WadLinkOpcode.Push:
                        try
                        {
                            currentBone = stack.Pop();
                        }
                        catch (Exception)
                        {
                            continue;
                        }

                        model.Bones[j].Transform = Matrix.Translation(new Vector3(link.X, -link.Y, link.Z));
                        model.Bones[j].Parent = currentBone;
                        currentBone.Children.Add(model.Bones[j]);
                        currentBone = model.Bones[j];

                        break;
                    case WadLinkOpcode.Pop:
                        model.Bones[j].Transform = Matrix.Translation(new Vector3(link.X, -link.Y, link.Z));
                        try
                        {
                            stack.Push(currentBone);
                        }
                        catch (Exception)
                        {
                            continue;
                        }
                        model.Bones[j].Parent = currentBone;
                        currentBone.Children.Add(model.Bones[j]);
                        currentBone = model.Bones[j];

                        break;
                    case WadLinkOpcode.Read:
                        Bone bone;
                        try
                        {
                            bone = stack.Pop();
                        }
                        catch (Exception)
                        {
                            continue;
                        }
                        model.Bones[j].Transform = Matrix.Translation(new Vector3(link.X, -link.Y, link.Z));
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

                    frame.Translations[0] = Matrix.Translation(new Vector3(wadFrame.Offset.X, -wadFrame.Offset.Y, wadFrame.Offset.Z));

                    for (int k = 1; k < frame.Translations.Count; k++)
                        frame.Translations[k] = Matrix.Translation(Vector3.Zero);

                    for (int n = 0; n < frame.Rotations.Count; n++)
                    {
                        WadKeyFrameRotation rot = wadFrame.Angles[n];
                        switch (rot.Axis)
                        {
                            case WadKeyFrameRotationAxis.ThreeAxes:
                                frame.Rotations[n] = Matrix.RotationYawPitchRoll((float)rot.Y, (float)-rot.X, (float)-rot.Z);
                                break;

                            case WadKeyFrameRotationAxis.AxisX:
                                frame.Rotations[n] = Matrix.RotationX((float)-rot.X);
                                break;

                            case WadKeyFrameRotationAxis.AxisY:
                                frame.Rotations[n] = Matrix.RotationY((float)rot.Y);
                                break;

                            case WadKeyFrameRotationAxis.AxisZ:
                                frame.Rotations[n] = Matrix.RotationZ((float)-rot.Z);
                                break;
                        }
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
            //   InverseTransforms[node.Index] = new Matrix(node.GlobalTransform.ToArray());
            InverseTransforms[node.Index] = Matrix.Invert(node.GlobalTransform);

            foreach (var child in node.Children)
            {
                BuildHierarchy(child, node.GlobalTransform, level + 1);
            }
        }

        public void BuildAnimationPose(KeyFrame frame)
        {
            /*for (int i = 0; i < Bones.Count; i++)
            {
                if (i > 0)
                {
                    Matrix tmp2 = Bones[i].Transform;
                    tmp2 = Matrix.Multiply(frame.Rotations[i], tmp2);
                    AnimationTransforms[i] = Matrix.Multiply(tmp2, AnimationTransforms[Bones[i].Parent.Index]);
                }
                else
                {
                    AnimationTransforms[0] = Matrix.Multiply(frame.Rotations[0] , Matrix.Translation(Vector3.Zero));
                }
            }


            return;*/
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

        /*public void PrepareVertices()
        {
            for (int i = 0; i < Meshes.Count; i++)
            {
                Matrix inverse = new Matrix(Transforms[i].ToArray());
                inverse.Invert();
                for (int j = 0; j < Meshes[i].Vertices.Count; j++)
                {
                    SkinnedVertex vertex = Meshes[i].Vertices[j];
                    vertex.Position = Vector4.Transform(vertex.Position, inverse);
                    Meshes[i].Vertices[j] = vertex;
                }
            }
        }*/

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
    }
}
