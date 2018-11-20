using System;
using System.Numerics;

namespace TombLib.Utils
{
    public abstract class Texture : ICloneable
    {
        public static ImageC UnloadedPlaceholder { get; } = ImageC.Black;

        // Do not change the image with this methode
        public ImageC Image { get; protected set; } = UnloadedPlaceholder;

        public abstract Texture Clone();

        object ICloneable.Clone()
        {
            return Clone();
        }

        public bool IsUnavailable => Image == UnloadedPlaceholder;
        public bool IsAvailable => Image != UnloadedPlaceholder;
    }

    public sealed class TextureInvisible : Texture
    {
        public static Texture Instance { get; } = new TextureInvisible();

        private TextureInvisible()
        {
            Image = ImageC.Transparent;
        }

        public override Texture Clone()
        {
            return Instance;
        }
    }

    public interface TextureHashed
    {
        Hash Hash { get; }
    }

    public enum BlendMode : ushort
    {
        Normal = 0,
        AlphaTest = 1,
        Additive = 2,
        NoZTest = 4,
        Subtract = 5,
        Wireframe = 6,
        Exclude = 8,
        Screen = 9,
        Lighten = 10
    }

    public enum TextureShapeType
    {
        XnYnClockwise = 0,
        QuadClockwise = 0,
        XpYnClockwise = 1,
        QuadCounterclockwise = 1,
        XpYpClockwise = 2,
        XnYpClockwise = 3,
        XpYnCounterclockwise = 4,
        XnYnCounterclockwise = 5,
        XnYpCounterclockwise = 6,
        XpYpCounterclockwise = 7
    }

    public static class TextureExtensions
    {
        // Mapping correction compensation coordinate sets.
        // Used to counterbalance TR4/5 internal mapping correction applied in regard to NewFlags
        // value.

        public static readonly Vector2[,] CompensationTris = new Vector2[,]
        {
            { new Vector2( 0.5f,  0.5f), new Vector2(-0.5f,  0.5f), new Vector2( 0.5f, -0.5f) },
            { new Vector2(-0.5f,  0.5f), new Vector2(-0.5f, -0.5f), new Vector2( 0.5f,  0.5f) },
            { new Vector2(-0.5f, -0.5f), new Vector2( 0.5f, -0.5f), new Vector2(-0.5f,  0.5f) },
            { new Vector2( 0.5f, -0.5f), new Vector2( 0.5f,  0.5f), new Vector2(-0.5f, -0.5f) },
            { new Vector2(-0.5f,  0.5f), new Vector2( 0.5f,  0.5f), new Vector2(-0.5f, -0.5f) },
            { new Vector2( 0.5f,  0.5f), new Vector2( 0.5f, -0.5f), new Vector2(-0.5f,  0.5f) },
            { new Vector2( 0.5f, -0.5f), new Vector2(-0.5f, -0.5f), new Vector2( 0.5f,  0.5f) },
            { new Vector2(-0.5f, -0.5f), new Vector2(-0.5f,  0.5f), new Vector2( 0.5f, -0.5f) }
        };

        public static readonly Vector2[,] CompensationQuads = new Vector2[,]
        {
            { new Vector2( 0.5f, 0.5f), new Vector2(-0.5f, 0.5f), new Vector2(-0.5f, -0.5f), new Vector2( 0.5f, -0.5f) },
            { new Vector2(-0.5f, 0.5f), new Vector2( 0.5f, 0.5f), new Vector2( 0.5f, -0.5f), new Vector2(-0.5f, -0.5f) }
        };

        public static TextureShapeType GetTextureShapeType(Vector2[] texCoords, bool isForTriangle)
        {
            bool isClockwise = !(MathC.CalculateArea(texCoords) > 0.0f);

            if (isForTriangle)
            {
                Vector2 midPoint = (texCoords[0] + texCoords[1] + texCoords[2]) * (1.0f / 3.0f);

                // Determine closest edge to the mid
                float distance0 = (texCoords[0] - midPoint).LengthSquared();
                float distance1 = (texCoords[1] - midPoint).LengthSquared();
                float distance2 = (texCoords[2] - midPoint).LengthSquared();

                byte closeEdgeIndex = 0;
                if (distance1 < Math.Min(distance0, distance2))
                    closeEdgeIndex = 1;
                if (distance2 < Math.Min(distance0, distance1))
                    closeEdgeIndex = 2;

                // Determine case
                Vector2 toClosestEdge = texCoords[closeEdgeIndex] - midPoint;
                if (toClosestEdge.X < 0)
                    if (toClosestEdge.Y < 0)
                    { // Negative X, Negative Y
                        // +---+
                        // |  /
                        // | /
                        // |/
                        // +
                        if (isClockwise)
                            return (TextureShapeType)0; //static constexpr Diverse::Vec<2, float> Triangle0[3] = { { 0.5f, 0.5f }, { -0.5f, 0.5f }, { 0.5f, -0.5f } };
                        else
                            return (TextureShapeType)5; //static constexpr Diverse::Vec<2, float> Triangle5[3] = { { 0.5f, 0.5f }, { 0.5f, -0.5f }, { -0.5f, 0.5f } };
                    }
                    else
                    { // Negative X, Postive Y
                        // +
                        // |\
                        // | \
                        // |  \
                        // +---+
                        if (isClockwise)
                            return (TextureShapeType)3; //static constexpr Diverse::Vec<2, float> Triangle3[3] = { { 0.5f, -0.5f }, { 0.5f, 0.5f }, { -0.5f, -0.5f } };
                        else
                            return (TextureShapeType)6; //static constexpr Diverse::Vec<2, float> Triangle6[3] = { { 0.5f, -0.5f }, { -0.5f, -0.5f }, { 0.5f, 0.5f } };
                    }
                else
                    if (toClosestEdge.Y < 0)
                { // Postive X, Negative Y
                  // +---+
                  //  \  |
                  //   \ |
                  //    \|
                  //     +
                    if (isClockwise)
                        return (TextureShapeType)1; //static constexpr Diverse::Vec<2, float> Triangle1[3] = { { -0.5f, 0.5f }, { -0.5f, -0.5f }, { 0.5f, 0.5f } };
                    else
                        return (TextureShapeType)4; //static constexpr Diverse::Vec<2, float> Triangle4[3] = { { -0.5f, 0.5f }, { 0.5f, 0.5f }, { -0.5f, -0.5f } };
                }
                else
                { // Postive X, Postive Y
                  //     +
                  //    /|
                  //   / |
                  //  /  |
                  // +---+
                    if (isClockwise)
                        return (TextureShapeType)2; //static constexpr Diverse::Vec<2, float> Triangle2[3] = { { -0.5f, -0.5f }, { 0.5f, -0.5f }, { -0.5f, 0.5f } };
                    else
                        return (TextureShapeType)7; //static constexpr Diverse::Vec<2, float> Triangle7[3] = { { -0.5f, -0.5f }, { -0.5f, 0.5f }, { 0.5f, -0.5f } };
                }
            }
            else if (!isClockwise)
                return (TextureShapeType)1;
            else
                return (TextureShapeType)0;
        }
    }

    public struct TextureArea : IEquatable<TextureArea>
    {
        public static readonly TextureArea None;
        public static readonly TextureArea Invisible = new TextureArea { Texture = TextureInvisible.Instance };

        public Texture Texture;
        public Rectangle2 ParentArea;
        public Vector2 TexCoord0; // No array for those because:
        public Vector2 TexCoord1; //    - Cache locality
        public Vector2 TexCoord2; //    - No array bounds checks
        public Vector2 TexCoord3; //    - 'Clone', 'GetHashCode' and so on work by default
        public BlendMode BlendMode;
        public bool DoubleSided;

        public static bool operator ==(TextureArea first, TextureArea second)
        {
            return
                first.Texture == second.Texture &&
                first.TexCoord0.Equals(second.TexCoord0) &&
                first.TexCoord1.Equals(second.TexCoord1) &&
                first.TexCoord2.Equals(second.TexCoord2) &&
                first.TexCoord3.Equals(second.TexCoord3) &&
                first.ParentArea.Start.Equals(second.ParentArea.Start) &&
                first.ParentArea.End.Equals(second.ParentArea.End) &&
                first.BlendMode == second.BlendMode &&
                first.DoubleSided == second.DoubleSided;
        }

        public static bool operator !=(TextureArea first, TextureArea second) => !(first == second);
        public bool Equals(TextureArea other) => this == other;
        public override bool Equals(object other) => other is TextureArea && this == (TextureArea)other;
        public override int GetHashCode() => base.GetHashCode();

        public bool TextureIsUnavailable => Texture == null || Texture.IsUnavailable;
        public bool TextureIsInvisible => Texture == TextureInvisible.Instance || Texture == null;
        public bool TextureIsTriangle => TexCoord2 == TexCoord3 || (TexCoord3.X == 0 && TexCoord3.Y == 0);

        public bool TriangleCoordsOutOfBounds
        {
            get
            {
                if (TextureIsInvisible || TextureIsUnavailable)
                    return false;

                Vector2 max = Vector2.Max(Vector2.Max(TexCoord0, TexCoord1), TexCoord2);
                Vector2 min = Vector2.Min(Vector2.Min(TexCoord0, TexCoord1), TexCoord2);
                return min.X < 0.0f || min.Y < 0.0f || max.X > Texture.Image.Width || max.Y > Texture.Image.Height;
            }
        }

        public bool QuadCoordsOutOfBounds
        {
            get
            {
                if (TextureIsInvisible || TextureIsUnavailable)
                    return false;
                Vector2 max = Vector2.Max(Vector2.Max(TexCoord0, TexCoord1), Vector2.Max(TexCoord2, TexCoord3));
                Vector2 min = Vector2.Min(Vector2.Min(TexCoord0, TexCoord1), Vector2.Min(TexCoord2, TexCoord3));
                return min.X < 0.0f || min.Y < 0.0f || max.X > Texture.Image.Width || max.Y > Texture.Image.Height;
            }
        }

        public Rectangle2 GetRect(bool? isTriangle = null)
        {
            if (!isTriangle.HasValue)
                isTriangle = TextureIsTriangle;

            if (isTriangle.Value)
                return Rectangle2.FromCoordinates(TexCoord0, TexCoord1, TexCoord2);
            else
                return Rectangle2.FromCoordinates(TexCoord0, TexCoord1, TexCoord2, TexCoord3);
        }

        public Vector2[] TexCoords
        {
            get
            {
                return new Vector2[]
                {
                TexCoord0,
                TexCoord1,
                TexCoord2,
                TexCoord3
                };
            }
        }

        // Gets canonical texture area which is compatible with UVRotate routine
        // and also puts rotational difference into Rotation out parameter
        public TextureArea GetCanonicalTexture(bool isTriangle)
        {
            var minY = GetRect(isTriangle).Start.Y;
            var transformedTexture = this;

            while (transformedTexture.TexCoord0.Y != minY)
                transformedTexture.Rotate(1, isTriangle);

            // Perform extra rotation in case it's texture with similar upper coordinates
            if (minY == (isTriangle ? transformedTexture.TexCoord2.Y : transformedTexture.TexCoord3.Y))
                transformedTexture.Rotate(1, isTriangle);

            return transformedTexture;
        }

        public TextureArea RestoreQuad()
        {
            var area = GetRect(true);
            var triangleCoords = TexCoords;
            var corners = new bool[4];
            var restoredTexture = this;
            var shape = (int)TextureExtensions.GetTextureShapeType(triangleCoords, true);

            restoredTexture.TexCoord3 = restoredTexture.TexCoord2; // Just in case...

            var coords = new Vector2[4]
            {
                area.Start,
                new Vector2(area.X1, area.Y0),
                new Vector2(area.X0, area.Y1),
                area.End
            };

            for(int i = 0; i < 4; i++)
                for(int j = 0; j < 3; j++)
                    if (triangleCoords[j] == coords[i])
                        corners[i] = true;

            restoredTexture.TexCoord3 = coords[Array.FindIndex(corners, corner => !corner)];
            return restoredTexture;
        }

        public Vector2 GetTexCoord(int index)
        {
            switch (index)
            {
                case 0:
                    return TexCoord0;
                case 1:
                    return TexCoord1;
                case 2:
                    return TexCoord2;
                case 3:
                    return TexCoord3;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public void SetTexCoord(int index, Vector2 value)
        {
            switch (index)
            {
                case 0:
                    TexCoord0 = value;
                    break;
                case 1:
                    TexCoord1 = value;
                    break;
                case 2:
                    TexCoord2 = value;
                    break;
                case 3:
                    TexCoord3 = value;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public void Mirror(bool isTriangle = false)
        {
            if(!isTriangle)
            {
                Swap.Do(ref TexCoord0, ref TexCoord3);
                Swap.Do(ref TexCoord1, ref TexCoord2);
            }
            else
            {
                Swap.Do(ref TexCoord0, ref TexCoord2);
                TexCoord3 = TexCoord2;
            }
        }

        public void Rotate(int iter = 1, bool isTriangle = false)
        {
            for(int i = 0; i < iter; i++)
            {
                if (!isTriangle)
                {
                    Vector2 tempTexCoord = TexCoord3;
                    TexCoord3 = TexCoord2;
                    TexCoord2 = TexCoord1;
                    TexCoord1 = TexCoord0;
                    TexCoord0 = tempTexCoord;
                }
                else
                {
                    Vector2 tempTexCoord = TexCoord2;
                    TexCoord2 = TexCoord1;
                    TexCoord1 = TexCoord0;
                    TexCoord0 = tempTexCoord;
                    TexCoord3 = TexCoord2;
                }
            }
        }

        public TextureArea Transform(RectTransformation transformation)
        {
            TextureArea result = this;
            transformation.TransformValueDiagonalQuad(ref result.TexCoord0, ref result.TexCoord1, ref result.TexCoord2, ref result.TexCoord3);
            return result;
        }

        public float TriangleArea => MathC.CalculateArea(TexCoord0, TexCoord1, TexCoord2);
        public float QuadArea => MathC.CalculateArea(TexCoord0, TexCoord1, TexCoord2, TexCoord3);
    }
}
