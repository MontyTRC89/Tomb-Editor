using System.Collections.Generic;
using System.Numerics;

namespace TombLib.Graphics
{
    public class KeyFrame
    {
        public List<Vector3> Translations { get; set; } = new List<Vector3>();
        public List<Vector3> Rotations { get; set; } = new List<Vector3>();
        public List<Matrix4x4> TranslationsMatrices { get; set; } = new List<Matrix4x4>();
        //public List<Matrix4x4> RotationsMatrices { get; set; } = new List<Matrix4x4>();
        public BoundingBox BoundingBox { get; set; }
        public List<Quaternion> Quaternions { get; set; } = new List<Quaternion>();

        public BoundingBox CalculateBoundingBox(AnimatedModel model, AnimatedModel skin)
        {
            Vector3 min = new Vector3(float.MaxValue);
            Vector3 max = new Vector3(float.MinValue);

            foreach (var mesh in skin.Meshes)
            {
                foreach (var vertex in mesh.Vertices)
                {
                    var meshIndex = skin.Meshes.IndexOf(mesh);
                    var transformedPosition = MathC.HomogenousTransform(vertex.Position, model.AnimationTransforms[meshIndex]);
                    min = Vector3.Min(transformedPosition, min);
                    max = Vector3.Max(transformedPosition, max);
                }
            }

            BoundingBox = new BoundingBox(new Vector3((int)min.X, (int)min.Y, (int)min.Z),
                                          new Vector3((int)max.X, (int)max.Y, (int)max.Z));

            return BoundingBox;
        }

        public KeyFrame Clone()
        {
            var keyframe = new KeyFrame();
            foreach (var translation in Translations)
            {
                keyframe.Translations.Add(new Vector3(translation.X, translation.Y, translation.Z));
                keyframe.TranslationsMatrices.Add(Matrix4x4.CreateTranslation(translation));
            }
            foreach (var rotation in Rotations)
            {
                keyframe.Rotations.Add(new Vector3(rotation.X, rotation.Y, rotation.Z));
                //keyframe.RotationsMatrices.Add(Matrix4x4.CreateFromYawPitchRoll(rotation.Y, rotation.X, rotation.Z));
                keyframe.Quaternions.Add(Quaternion.CreateFromYawPitchRoll(rotation.Y, rotation.X, rotation.Z));
            }
            keyframe.BoundingBox = new BoundingBox(new Vector3(BoundingBox.Minimum.X, BoundingBox.Minimum.Y, BoundingBox.Minimum.Z),
                                                   new Vector3(BoundingBox.Maximum.X, BoundingBox.Maximum.Y, BoundingBox.Maximum.Z));
            return keyframe;
        }
    }
}
