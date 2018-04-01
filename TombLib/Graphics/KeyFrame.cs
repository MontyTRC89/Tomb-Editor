using System.Collections.Generic;
using System.Numerics;

namespace TombLib.Graphics
{
    public class KeyFrame
    {
        //public BoundingBox BoundingBox { get; set; }
        public List<Vector3> Translations { get; set; } = new List<Vector3>();
        public List<Vector3> Rotations { get; set; } = new List<Vector3>();
        public List<Matrix4x4> TranslationsMatrices { get; set; } = new List<Matrix4x4>();
        public List<Matrix4x4> RotationsMatrices { get; set; } = new List<Matrix4x4>();

        public BoundingBox GetBoundingBox(AnimatedModel model)
        {
            Vector3 min = new Vector3(float.MaxValue);
            Vector3 max = new Vector3(float.MinValue);

            foreach (var mesh in model.Meshes)
            {
                foreach (var vertex in mesh.Vertices)
                {
                    var meshIndex = model.Meshes.IndexOf(mesh);
                    var transformedPosition = MathC.HomogenousTransform(vertex.Position, model.AnimationTransforms[meshIndex]);
                    min = Vector3.Min(transformedPosition, min);
                    max = Vector3.Max(transformedPosition, max);
                }
            }

            return new BoundingBox(min, max);
        }
    }
}
