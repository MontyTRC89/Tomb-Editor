using System.Collections.Generic;
using System.Numerics;
using TombLib.Wad;

namespace TombLib.Graphics
{
    public class Animation
    {
        public string Name { get; set; }
        public short Framerate { get; set; }
        public float Speed { get; set; }
        public float Acceleration { get; set; }
        public List<KeyFrame> KeyFrames { get; set; } = new List<KeyFrame>();

        public static Animation FromWad2(List<WadBone> bones, WadAnimation wadAnim)
        {
            Animation animation = new Animation();

            animation.Framerate = wadAnim.FrameDuration;
            animation.KeyFrames = new List<KeyFrame>();

            for (int f = 0; f < wadAnim.KeyFrames.Count; f++)
            {
                KeyFrame frame = new KeyFrame();
                WadKeyFrame wadFrame = wadAnim.KeyFrames[f];

                for (int k = 0; k < bones.Count; k++)
                {
                    frame.Rotations.Add(Vector3.Zero);
                    frame.Translations.Add(Vector3.Zero);
                    frame.RotationsMatrices.Add(Matrix4x4.Identity);
                    frame.TranslationsMatrices.Add(Matrix4x4.Identity);
                }

                frame.Translations[0] = new Vector3(wadFrame.Offset.X, wadFrame.Offset.Y, wadFrame.Offset.Z); 
                frame.TranslationsMatrices[0] = Matrix4x4.CreateTranslation(wadFrame.Offset);

                for (int k = 1; k < frame.Translations.Count; k++)
                {
                    frame.Translations[k] = Vector3.Zero;
                    frame.RotationsMatrices[k] = Matrix4x4.CreateTranslation(Vector3.Zero);
                }

                for (int n = 0; n < frame.Rotations.Count; n++)
                {
                    frame.Rotations[n] = wadFrame.Angles[n].RotationVectorInRadians;
                    frame.RotationsMatrices[n] = wadFrame.Angles[n].RotationMatrix;
                }

                frame.BoundingBox = new BoundingBox(wadFrame.BoundingBox.Minimum, wadFrame.BoundingBox.Maximum);

                animation.KeyFrames.Add(frame);
            }

            return animation;
        }
    }
}
