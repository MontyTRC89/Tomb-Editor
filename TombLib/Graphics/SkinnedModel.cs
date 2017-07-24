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

namespace TombLib.Graphics
{
    public class SkinnedModel : Model<SkinnedMesh, SkinnedVertex>
    {
        public Vector3 Offset;

        public SkinnedModel(GraphicsDevice device, uint objectId)
            : base(device, objectId, ModelType.Skinned)
        {
            Animations = new List<Animation>();
            Bones = new List<Bone>();
            Transforms = new List<Matrix>();
            AnimationTransforms = new List<Matrix>();
            InverseTransforms = new List<Matrix>();
        }

        public Bone Root { get; set; }

        public List<Animation> Animations { get; set; }

        public List<Bone> Bones { get; set; }

        public List<Matrix> Transforms { get; set; }


        public List<Matrix> InverseTransforms { get; set; }

        public List<Matrix> AnimationTransforms { get; set; }

        public void ApplyTransforms()
        {

        }

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

            if (Vertices.Count == 0) return;

            _vb = Buffer.Vertex.New<SkinnedVertex>(GraphicsDevice, Vertices.ToArray<SkinnedVertex>(), SharpDX.Direct3D11.ResourceUsage.Dynamic);
            _ib = Buffer.Index.New(GraphicsDevice, Indices.ToArray(), SharpDX.Direct3D11.ResourceUsage.Dynamic);
        }
    }
}
