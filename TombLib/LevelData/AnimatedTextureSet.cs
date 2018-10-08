using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

namespace TombLib.LevelData
{
    public enum AnimatedTextureAnimationType
    {
        Frames,
        PFrames,
        UVRotate,
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
        public RectangleInt2 Area
        {
            get
            {
                Vector2 min = Vector2.Min(Vector2.Min(TexCoord0, TexCoord1), Vector2.Min(TexCoord2, TexCoord3));
                Vector2 max = Vector2.Max(Vector2.Max(TexCoord0, TexCoord1), Vector2.Max(TexCoord2, TexCoord3));
                return new RectangleInt2((int)Math.Floor(min.X), (int)Math.Floor(min.Y), (int)Math.Ceiling(max.X), (int)Math.Ceiling(max.Y));
            }
        }
        private int _repeat = 1;
        public int Repeat
        {
            get { return _repeat; }
            set
            {
                if (value < 1)
                    throw new ArgumentOutOfRangeException(nameof(value), "'Repeat' must be at least 1.");
                _repeat = value;
            }
        }

        public AnimatedTextureFrame Clone() => (AnimatedTextureFrame)MemberwiseClone();
        object ICloneable.Clone() => Clone();

        public bool Equals(AnimatedTextureFrame other)
        {
            return Texture == other.Texture &&
                TexCoord0.Equals(other.TexCoord0) &&
                TexCoord1.Equals(other.TexCoord1) &&
                TexCoord2.Equals(other.TexCoord2) &&
                TexCoord3.Equals(other.TexCoord3);
        }
        public override bool Equals(object other) => other is AnimatedTextureFrame && Equals((AnimatedTextureFrame)other);
        public override int GetHashCode() => base.GetHashCode();
    }

    public class AnimatedTextureSet : ICloneable, IEquatable<AnimatedTextureSet>
    {
        public AnimatedTextureAnimationType AnimationType { get; set; }
        public string Name { get; set; }
        public float Fps { get; set; }  // float is for SPF (seconds per frame) values
        public int UvRotate { get; set; }

        public List<AnimatedTextureFrame> Frames { get; set; } = new List<AnimatedTextureFrame>();

        public bool IsUvRotate
        {
            get
            {
                return AnimationType == AnimatedTextureAnimationType.UVRotate ||
                       AnimationType == AnimatedTextureAnimationType.HalfRotate ||
                       AnimationType == AnimatedTextureAnimationType.RiverRotate;
            }
        }

        public AnimatedTextureSet Clone()
        {
            return new AnimatedTextureSet { Frames = Frames.ConvertAll(frame => frame.Clone()),
                                            AnimationType = AnimationType,
                                            Fps = Fps,
                                            UvRotate = UvRotate,
                                            Name = Name
                                          };
        }
        object ICloneable.Clone() => Clone();
        public bool AnimationIsTrivial => (Frames.Count < 1 && AnimationType == AnimatedTextureAnimationType.Frames) || Frames.Count == 0;

        public bool Equals(AnimatedTextureSet other) => Frames.SequenceEqual(other.Frames);
        public override bool Equals(object other) => other is AnimatedTextureSet && Equals((AnimatedTextureSet)other);
        public override int GetHashCode() => base.GetHashCode();

        public override string ToString()
        {
            return (string.IsNullOrEmpty(Name) ? "Set (" + Frames.Count + " frames)" : Name);
        }
    }
}
