using SharpDX;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TombEditor.Geometry
{
    public enum AnimatedTextureAnimationType
    {
        Frames,
        PFrames,
        FullRotate,
        RiverRotate,
        HalfRotate
    }

    public class AnimatedTextureFrame : ICloneable, IEquatable<AnimatedTextureFrame>
    {
        public LevelTexture Texture { get; set; }
        public Vector2 TexCoord0 { get; set; }
        public Vector2 TexCoord1 { get; set; }
        public Vector2 TexCoord2 { get; set; }
        public Vector2 TexCoord3 { get; set; }
        public Rectangle Area
        {
            get
            {
                Vector2 min = Vector2.Min(Vector2.Min(TexCoord0, TexCoord1), Vector2.Min(TexCoord2, TexCoord3));
                Vector2 max = Vector2.Max(Vector2.Max(TexCoord0, TexCoord1), Vector2.Max(TexCoord2, TexCoord3));
                return new Rectangle((int)Math.Floor(min.X), (int)Math.Floor(min.Y), (int)Math.Ceiling(max.X), (int)Math.Ceiling(max.Y));
            }
        }
        private int _repeat = 1;
        public int Repeat
        {
            get { return _repeat; }
            set
            {
                if (value < 1)
                    throw new ArgumentOutOfRangeException("'Repeat' must be at least 1.");
                _repeat = value;
            }
        }

        public AnimatedTextureFrame Clone() => (AnimatedTextureFrame)MemberwiseClone();
        object ICloneable.Clone() => Clone();

        public bool Equals(AnimatedTextureFrame other)
        {
            return (Texture == other.Texture) &&
                (TexCoord0.Equals(other.TexCoord0)) &&
                (TexCoord1.Equals(other.TexCoord1)) &&
                (TexCoord2.Equals(other.TexCoord2)) &&
                (TexCoord3.Equals(other.TexCoord3));
        }
        public override bool Equals(object other) => Equals((AnimatedTextureFrame)other);
        public override int GetHashCode() => base.GetHashCode();
    }

    public class AnimatedTextureSet : ICloneable, IEquatable<AnimatedTextureSet>
    {
        public AnimatedTextureAnimationType AnimationType { get; set; }
        public sbyte Fps { get; set; }
        public sbyte UvRotate { get; set; }

        public List<AnimatedTextureFrame> Frames { get; set; } = new List<AnimatedTextureFrame>();

        public bool IsUvRotate
        {
            get
            {
                return AnimationType == AnimatedTextureAnimationType.FullRotate ||
                       AnimationType == AnimatedTextureAnimationType.HalfRotate ||
                       AnimationType == AnimatedTextureAnimationType.RiverRotate; 
            }
        }

        public AnimatedTextureSet Clone()
        {
            return new AnimatedTextureSet { Frames = Frames.ConvertAll(frame => frame.Clone()),
                                            AnimationType = AnimationType,
                                            Fps = Fps,
                                            UvRotate = UvRotate
                                          };
        }
        object ICloneable.Clone() => Clone();
        public bool AnimationIsTrivial => Frames.Count < 1;

        public bool Equals(AnimatedTextureSet other) => Frames.SequenceEqual(other.Frames);
        public override bool Equals(object other) => Equals((AnimatedTextureSet)other);
        public override int GetHashCode() => base.GetHashCode();

        public override string ToString()
        {
            return "Set (" + Frames.Count + " frames)";
        }
    }
}
