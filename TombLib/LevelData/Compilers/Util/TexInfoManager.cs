using NLog;
using System;
using System.Collections.Generic;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using TombLib.IO;
using TombLib.Utils;

namespace TombLib.LevelData.Compilers.Util
{
    public class TexInfoManager
    {
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();

        // List of parent textures should contain all "ancestor" texture areas in which all variations
        // are placed, including mirrored and rotated ones.

        public List<ParentTextureArea> ParentTextures = new List<ParentTextureArea>();
        public int TexInfoCount { get; private set; } = 0;

        public int TexturePageSize = 256;
        public int NumNonBumpedTexturePages = 0;
        public int NumBumpedTexturePages = 0;

        // Get page offset for bump-mapped texture pages
        private int PageOffset(ushort flags) => ((flags & 0x1800) != 0) ? NumNonBumpedTexturePages : 0;


        // ParentTextureArea is a texture area which contains all other texture areas which are
        // completely inside current one. Blending mode and bumpmapping parameters define that
        // parent is different, hence two TextureAreas with same UV coordinates but with different
        // BlendMode and BumpLevel will be saved as different parents.

        public class ParentTextureArea
        {
            public TextureArea Area { get; private set; }
            public List<KeyValuePair<int, Vector2[]>> Children;

            // Generates new ParentTextureArea from raw texture coordinates.
            public ParentTextureArea(TextureArea newTextureArea)
            {
                var minMax = newTextureArea.GetMinMax();

                Area = new TextureArea()
                {
                    BlendMode = newTextureArea.BlendMode,
                    BumpLevel = newTextureArea.BumpLevel,
                    DoubleSided = newTextureArea.DoubleSided,
                    Texture = newTextureArea.Texture,

                    // Please note that resulting parent area is a min-max quad, because
                    // generally it makes more sense to operate on it than on raw tex coords.
                    TexCoord0 = new Vector2(minMax[0].X, minMax[0].Y),
                    TexCoord1 = new Vector2(minMax[1].X, minMax[0].Y),
                    TexCoord2 = new Vector2(minMax[1].X, minMax[1].Y),
                    TexCoord3 = new Vector2(minMax[0].X, minMax[1].Y)
                };

                Children = new List<KeyValuePair<int, Vector2[]>>();
            }

            // Checks if parameters are similar to another texture area, and if so,
            // also checks if texture area is enclosed in parent's area.
            public bool IsPotentialChild(TextureArea texture)
            {
                return (Area.ParametersSimilar(texture) &&
                       (PolygonFunctions.DoesQuadContainPoint(texture.TexCoord0, texture.TexCoord1, texture.TexCoord2, texture.TexCoord3, Area.TexCoord0)) &&
                       (PolygonFunctions.DoesQuadContainPoint(texture.TexCoord0, texture.TexCoord1, texture.TexCoord2, texture.TexCoord3, Area.TexCoord1)) &&
                       (PolygonFunctions.DoesQuadContainPoint(texture.TexCoord0, texture.TexCoord1, texture.TexCoord2, texture.TexCoord3, Area.TexCoord2)) &&
                       (PolygonFunctions.DoesQuadContainPoint(texture.TexCoord0, texture.TexCoord1, texture.TexCoord2, texture.TexCoord3, Area.TexCoord3)));
            }

            // Adds texture as a child to existing parent, with recalculating coordinates to relative.
            public void AddChild(TextureArea texture, int newTextureID)
            {
                Children.Add(new KeyValuePair<int, Vector2[]>(newTextureID,
                    new Vector2[4]
                    {
                        new Vector2 { X = Area.TexCoord0.X - texture.TexCoord0.X, Y = Area.TexCoord0.Y - texture.TexCoord0.Y },
                        new Vector2 { X = Area.TexCoord0.X - texture.TexCoord1.X, Y = Area.TexCoord0.Y - texture.TexCoord1.Y },
                        new Vector2 { X = Area.TexCoord0.X - texture.TexCoord2.X, Y = Area.TexCoord0.Y - texture.TexCoord2.Y },
                        new Vector2 { X = Area.TexCoord0.X - texture.TexCoord3.X, Y = Area.TexCoord0.Y - texture.TexCoord3.Y }
                    }));
            }

            // Returns absolute child coordinates
            public static Vector2[] GetAbsChildCoordinates(ParentTextureArea parent, Vector2[] relativeCoordinates)
            {
                if(relativeCoordinates.Length != 4)
                {
                    logger.Error("GetAbsChildCoordinates: Weird coordinates count encountered!");
                    return new Vector2[] { Vector2.Zero };
                }

                return new Vector2[] { new Vector2(parent.Area.TexCoord0.X + relativeCoordinates[0].X, parent.Area.TexCoord0.Y + relativeCoordinates[0].Y),
                                       new Vector2(parent.Area.TexCoord0.X + relativeCoordinates[1].X, parent.Area.TexCoord0.Y + relativeCoordinates[1].Y),
                                       new Vector2(parent.Area.TexCoord0.X + relativeCoordinates[2].X, parent.Area.TexCoord0.Y + relativeCoordinates[2].Y),
                                       new Vector2(parent.Area.TexCoord0.X + relativeCoordinates[3].X, parent.Area.TexCoord0.Y + relativeCoordinates[3].Y), };
            }
        }

        // Gets free TexInfo index
        private int GetNewTexInfoIndex()
        {
            // Do we really need to check for free index in existing children lists?
            // Because texture generation is a linear one-off operation, I think we may
            // not care about that.

            int result = TexInfoCount;
            TexInfoCount++;
            return result;
        }

        // Try to find potential parent (larger texture) and add texture to its children
        public int FindAndAddToParent(TextureArea texture)
        {
            foreach (var parent in ParentTextures)
            {
                if (parent.IsPotentialChild(texture))
                {
                    var newTexIndex = GetNewTexInfoIndex();
                    parent.AddChild(texture, newTexIndex);
                    return newTexIndex;
                }
            }
            return -1;
        }

        public struct Result
        {
            // TexInfoIndex is saved as int for forward compatibility with engines such as TR5Main.
            public int TexInfoIndex;

            // Rotation value indicate that incoming TextureArea should be rotated N times. 
            // This approach allows to tightly pack TexInfos in same manner as tom2pc does.
            // As result, CreateFace3/4 should return a face with changed index order.
            public byte Rotation;

            public tr_face3 CreateFace3(ushort[] indices, bool doubleSided, ushort lightingEffect)
            {
                if(indices.Length != 3)
                    throw new ArgumentOutOfRangeException(nameof(indices.Length));

                ushort objectTextureIndex = (ushort)(GetLegacyIndex() | (doubleSided ? 0x8000 : 0));
                ushort[] transformedIndices = new ushort[3] { indices[0], indices[1], indices[2] };

                if(Rotation > 0)
                {
                    for (int i = 0; i < Rotation; i++)
                    {
                        ushort tempIndex = indices[3];
                        indices[3] = indices[2];
                        indices[2] = indices[1];
                        indices[1] = indices[0];
                        indices[0] = tempIndex;
                    }
                }

                return new tr_face3 { Vertices = new ushort[3] { transformedIndices[0], transformedIndices[1], transformedIndices[2] }, Texture = objectTextureIndex, LightingEffect = lightingEffect };
            }

            public tr_face4 CreateFace4(ushort[] indices, bool doubleSided, ushort lightingEffect)
            {
                if (indices.Length != 4)
                    throw new ArgumentOutOfRangeException(nameof(indices.Length));

                ushort objectTextureIndex = (ushort)(GetLegacyIndex() | (doubleSided ? 0x8000 : 0));
                ushort[] transformedIndices = new ushort[4] { indices[0], indices[1], indices[2], indices[3] };

                if (Rotation > 0)
                {
                    for (int i = 0; i < Rotation; i++)
                    {
                        ushort tempIndex = indices[3];
                        indices[3] = indices[2];
                        indices[2] = indices[1];
                        indices[1] = indices[0];
                        indices[0] = tempIndex;
                    }
                }

                return new tr_face4 { Vertices = new ushort[4] { transformedIndices[0], transformedIndices[1], transformedIndices[2], transformedIndices[3] }, Texture = objectTextureIndex, LightingEffect = lightingEffect };
            }

            // Silly measure to prevent overflowing legacy texinfo count
            private ushort GetLegacyIndex() => (ushort)((TexInfoIndex <= short.MaxValue) ? TexInfoIndex : short.MaxValue);

            // Custom implementation of these because default implementation is *insanely* slow.
            // Its not just a quite a bit slow, it really is *insanely* *crazy* slow so we need those functions :/
            public static unsafe bool operator ==(Result first, Result second)
            {
                return *((uint*)&first) == *((uint*)&second);
            }

            public static bool operator !=(Result first, Result second) => !(first == second);
            public bool Equals(Result other) => this == other;
            public override bool Equals(object other) => other is Result && this == (Result)other;
            public override unsafe int GetHashCode()
            {
                Result this2 = this;
                return unchecked(-368200913 * *((int*)&this2)); // Random prime
            }
        }

        public Result AddTexture(TextureArea texture, bool isTriangle, bool isUsedInRoomMesh, int packPriorityClass = 0)
        {
            // Try to get existing TexInfo
            byte rotation = 0;
            int  texInfoIndex = GetTexInfo(texture, out rotation);

            // Similar texture found
            if(texInfoIndex != -1)
                return new Result() { TexInfoIndex = texInfoIndex, Rotation = rotation };
            


            // Add object textures
            int textureID = _textureAllocator.GetOrAllocateTextureID(ref texture, isTriangle, packPriorityClass);
            bool isNew;
            byte firstTexCoordToEmit;
            ushort objTexIndex = AddOrGetObjectTexture(new SavedObjectTexture((ushort)textureID, texture, textureSpaceIdentifier,
                _textureAllocator.GetTextureFromID(textureID), isTriangle, isUsedInRoomMesh, canRotate, out firstTexCoordToEmit), supportsUpTo65536, out isNew);
            return new Result
            {
                ObjectTextureIndex = objTexIndex,
                FirstVertexIndexToEmit = firstTexCoordToEmit,
                Flags = (texture.DoubleSided ? ResultFlags.DoubleSided : ResultFlags.None) | (isNew ? ResultFlags.IsNew : ResultFlags.None)
            };
        }

        // Adds new TexInfo either to enclosed parent area or creates new one.
        // Additionally peforms checks if added TexInfo encloses any existing
        // parents, and if it does, merges the parent and becomes parent itself.

        private int MakeTexInfo(TextureArea areaToMake, out byte rotation)
        {
            rotation = 0;

            foreach(var parentTexInfo in ParentTextures)
            {


            }
        }

        // Gets existing TexInfo index if there is such one in ParentTextures list.
        // If parent itself is similar, its TexInfo index is returned.
        // If parent's child is similar, child TexInfo index is returned.

        private int GetTexInfo(TextureArea areaToLook, out byte rotation)
        {
            rotation = 0;

            foreach (var parent in ParentTextures)
            {
                // Parents with different attributes are quickly discarded
                if(!parent.Area.ParametersSimilar(areaToLook))
                    continue;

                var lookupCoordinates = new Vector2[] { new Vector2(areaToLook.TexCoord0.X, areaToLook.TexCoord0.Y),
                                                        new Vector2(areaToLook.TexCoord1.X, areaToLook.TexCoord1.Y),
                                                        new Vector2(areaToLook.TexCoord2.X, areaToLook.TexCoord2.Y),
                                                        new Vector2(areaToLook.TexCoord3.X, areaToLook.TexCoord3.Y), };

                // Extract each children's absolute coordinates and compare them to incoming texture coordinates.
                foreach (var child in parent.Children)
                {
                    var result = TestUVSimilarity(ParentTextureArea.GetAbsChildCoordinates(parent, child.Value), lookupCoordinates);
                    if(result != -1)
                    {
                        // Child is rotation-wise equal to incoming area
                        rotation = (byte)result;
                        return child.Key;
                    }
                }
            }

            return -1; // No equal entry, new should be created
        }

        // Tests if all UV coordinates are similar with different rotations.
        // If all coordinates are equal for one of the rotation factors, rotation factor is returned,
        // otherwise -1 is returned (not similar). If coordinates are 100% equal, 0 is returned.

        private int TestUVSimilarity(Vector2[] first, Vector2[] second)
        {
            int Result = -1; // Not similar

            // If first/second coordinates are not mutually quads/tris, quickly return -1.
            // Also discard out of bounds cases without exception.
            if (first.Length == second.Length && first.Length >= 3 && first.Length <= 4)
            {
                int baseCoord = -1;  // Used to keep first-found similar coordinate

                // Now scroll through all texture coords and find one which is similar to 
                // first one in current texture
                for (byte i = 0; i < first.Length; i++)
                {
                    if (first[0] == second[i])
                    {
                        baseCoord = i;
                        break;
                    }
                }

                // No first-found coordinate, discard further comparison
                if (baseCoord != -1)
                {
                    for (int i = 0; i < first.Length; i++)
                    {
                        // Shifted coord is different, discard further comparison
                        if (first[i] != second[((i + baseCoord) % first.Length)])
                            break;
                        // Last coord reached, comparison succeeded
                        if (i == first.Length - 1)
                            Result = baseCoord;
                    }
                }
            }
            return Result;
        }

        private int TestUVAlignment(TextureArea texture, out TextureArea rotatedTexture)
        {
            var testTexture = texture;
            int result = 0;

            while(testTexture.TexCoord0.Y < testTexture.TexCoord1.Y ||
                  testTexture.TexCoord0.Y < testTexture.TexCoord2.Y ||
                  testTexture.TexCoord0.Y < testTexture.TexCoord3.Y)
            {
                testTexture.Rotate();
                result++;
            }

            rotatedTexture = testTexture;
            return result;
        }
    }

        // ==================================== NON-REFACTORED CODE DOWN THE LINE ==========================


        private static readonly Vector2[] _quad0 = new Vector2[4] { new Vector2(0.5f, 0.5f), new Vector2(-0.5f, 0.5f), new Vector2(-0.5f, -0.5f), new Vector2(0.5f, -0.5f) };
        private static readonly Vector2[] _quad1 = new Vector2[4] { new Vector2(-0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, -0.5f), new Vector2(-0.5f, -0.5f) };
        private static readonly Vector2[] _zero = new Vector2[4] { new Vector2(0.0f, 0.0f), new Vector2(0.0f, 0.0f), new Vector2(0.0f, 0.0f), new Vector2(0.0f, 0.0f) };
        private static readonly Vector2[] _triangle0 = new Vector2[3] { new Vector2(0.5f, 0.5f), new Vector2(-0.5f, 0.5f), new Vector2(0.5f, -0.5f) };
        private static readonly Vector2[] _triangle1 = new Vector2[3] { new Vector2(-0.5f, 0.5f), new Vector2(-0.5f, -0.5f), new Vector2(0.5f, 0.5f) };
        private static readonly Vector2[] _triangle2 = new Vector2[3] { new Vector2(-0.5f, -0.5f), new Vector2(0.5f, -0.5f), new Vector2(-0.5f, 0.5f) };
        private static readonly Vector2[] _triangle3 = new Vector2[3] { new Vector2(0.5f, -0.5f), new Vector2(0.5f, 0.5f), new Vector2(-0.5f, -0.5f) };
        private static readonly Vector2[] _triangle4 = new Vector2[3] { new Vector2(-0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(-0.5f, -0.5f) };
        private static readonly Vector2[] _triangle5 = new Vector2[3] { new Vector2(0.5f, 0.5f), new Vector2(0.5f, -0.5f), new Vector2(-0.5f, 0.5f) };
        private static readonly Vector2[] _triangle6 = new Vector2[3] { new Vector2(0.5f, -0.5f), new Vector2(-0.5f, -0.5f), new Vector2(0.5f, 0.5f) };
        private static readonly Vector2[] _triangle7 = new Vector2[3] { new Vector2(-0.5f, -0.5f), new Vector2(-0.5f, 0.5f), new Vector2(0.5f, -0.5f) };

        public static Vector2[] GetTexCoordModificationFromNewFlags(ushort NewFlags, bool isTriangular)
        {
            if (isTriangular)
            {
                switch (NewFlags & 0x7)
                {
                    case 0x0:
                        return _triangle0;
                    case 0x1:
                        return _triangle1;
                    case 0x2:
                        return _triangle2;
                    case 0x3:
                        return _triangle3;
                    case 0x4:
                        return _triangle4;
                    case 0x5:
                        return _triangle5;
                    case 0x6:
                        return _triangle6;
                    case 0x7:
                        return _triangle7;
                }
                throw new NotImplementedException();
            }
            else
            {
                switch (NewFlags & 0x7)
                {
                    case 0x0:
                        return _quad0;
                    case 0x1:
                        return _quad1;
                    default:
                        return _zero; //Remaining cases do not do anything...
                }
            }
        }

        private static Vector2 HeuristcallyFixTexCoordUpperBound(Vector2 textureCoord)
        {
            // Add (2.01 / 256.0) to all vertices coordinates that are exactly at (127.0 / 256.0)
            Vector2 fractCoord = textureCoord - new Vector2((float)Math.Floor(textureCoord.X), (float)Math.Floor(textureCoord.Y));
            if (fractCoord == new Vector2(127.0f / 256.0f))
                return textureCoord + new Vector2(2.01f / 256.0f);
            return textureCoord;
        }

        private static ushort GetNewFlag(TextureArea texture, bool isTriangular, bool isUsedInRoomMesh, bool canRotate, out byte firstTexCoordToEmit)
        {
            firstTexCoordToEmit = 0;
            ushort result = isUsedInRoomMesh ? (ushort)0x8000 : (ushort)0;

            if (isTriangular)
            {
                texture.TexCoord0 = HeuristcallyFixTexCoordUpperBound(texture.TexCoord0);
                texture.TexCoord1 = HeuristcallyFixTexCoordUpperBound(texture.TexCoord1);
                texture.TexCoord2 = HeuristcallyFixTexCoordUpperBound(texture.TexCoord2);

                bool isClockwise = !(texture.TriangleArea > 0.0); //'Not' because Y-axis is flipped in texture space
                Vector2 midPoint = (texture.TexCoord0 + texture.TexCoord1 + texture.TexCoord2) * (1.0f / 3.0f);

                // Determine closest edge to the mid
                float distance0 = (texture.TexCoord0 - midPoint).LengthSquared();
                float distance1 = (texture.TexCoord1 - midPoint).LengthSquared();
                float distance2 = (texture.TexCoord2 - midPoint).LengthSquared();
                byte closeEdgeIndex = 0;
                if (distance1 < Math.Min(distance0, distance2))
                    closeEdgeIndex = 1;
                if (distance2 < Math.Min(distance0, distance1))
                    closeEdgeIndex = 2;

                // Determine case
                Vector2 toClosestEdge = texture.GetTexCoord(closeEdgeIndex) - midPoint;
                if (toClosestEdge.X < 0)
                    if (toClosestEdge.Y < 0)
                    { // Negative X, Negative Y
                        // +---+
                        // |  /
                        // | /
                        // |/
                        // +
                        if (isClockwise)
                            result |= 0; //static constexpr Diverse::Vec<2, float> Triangle0[3] = { { 0.5f, 0.5f }, { -0.5f, 0.5f }, { 0.5f, -0.5f } };
                        else
                            result |= 5; //static constexpr Diverse::Vec<2, float> Triangle5[3] = { { 0.5f, 0.5f }, { 0.5f, -0.5f }, { -0.5f, 0.5f } };
                    }
                    else
                    { // Negative X, Postive Y
                        // +
                        // |\
                        // | \
                        // |  \
                        // +---+
                        if (isClockwise)
                            result |= 3; //static constexpr Diverse::Vec<2, float> Triangle3[3] = { { 0.5f, -0.5f }, { 0.5f, 0.5f }, { -0.5f, -0.5f } };
                        else
                            result |= 6; //static constexpr Diverse::Vec<2, float> Triangle6[3] = { { 0.5f, -0.5f }, { -0.5f, -0.5f }, { 0.5f, 0.5f } };
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
                            result |= 1; //static constexpr Diverse::Vec<2, float> Triangle1[3] = { { -0.5f, 0.5f }, { -0.5f, -0.5f }, { 0.5f, 0.5f } };
                        else
                            result |= 4; //static constexpr Diverse::Vec<2, float> Triangle4[3] = { { -0.5f, 0.5f }, { 0.5f, 0.5f }, { -0.5f, -0.5f } };
                    }
                    else
                    { // Postive X, Postive Y
                      //     +
                      //    /|
                      //   / |
                      //  /  |
                      // +---+
                        if (isClockwise)
                            result |= 2; //static constexpr Diverse::Vec<2, float> Triangle2[3] = { { -0.5f, -0.5f }, { 0.5f, -0.5f }, { -0.5f, 0.5f } };
                        else
                            result |= 7; //static constexpr Diverse::Vec<2, float> Triangle7[3] = { { -0.5f, -0.5f }, { -0.5f, 0.5f }, { 0.5f, -0.5f } };
                    }

                // Rotate closest edge to the first place
                if (canRotate)
                    firstTexCoordToEmit = closeEdgeIndex;
            }
            else
            {
                texture.TexCoord0 = HeuristcallyFixTexCoordUpperBound(texture.TexCoord0);
                texture.TexCoord1 = HeuristcallyFixTexCoordUpperBound(texture.TexCoord1);
                texture.TexCoord2 = HeuristcallyFixTexCoordUpperBound(texture.TexCoord2);
                texture.TexCoord3 = HeuristcallyFixTexCoordUpperBound(texture.TexCoord3);

                bool isClockwise = !(texture.QuadArea > 0.0); // 'Not' because Y-axis is flipped in texture space

                // Determine upper left edge
                byte upperLeftIndex = 0;
                float upperLeftScore = texture.TexCoord0.Y + texture.TexCoord0.X;
                for (byte i = 1; i < 4; ++i)
                {
                    float iScore = texture.TexCoord1.Y + texture.GetTexCoord(i).X;
                    if (iScore < upperLeftScore)
                    {
                        upperLeftIndex = i;
                        upperLeftScore = iScore;
                    }
                }

                // Unify orientation depending on orientation
                if (isClockwise)
                { // Unify to mode 0
                    if (canRotate)
                        firstTexCoordToEmit = upperLeftIndex;
                }
                else
                { // Unify to mode 1
                    result |= 1;
                    if (canRotate)
                        firstTexCoordToEmit = (byte)((upperLeftIndex + 3) % 4);
                }
            }
            return result;
        }


        [StructLayout(LayoutKind.Sequential, Pack = 2)]
        private unsafe struct SavedObjectTexture : IEquatable<SavedObjectTexture>
        {
            public readonly ushort TextureID;
            public readonly ushort IsTriangularAndPadding;
            public ushort BlendMode;
            //Bit 0 - 2     mapping correction. It seems that these bits change the way the texture is applied...
            //Bit 11 - 12   the bump mapping type. Can be 0x01 or 0x10. It's 0x00 if not bump mapped. Only textures for room or animated textures can be bump mapped, not meshes
            //Bit 15        if set, the texture is for a tri/quad from a room or animated texture. If not set, the texture is for a mesh
            public readonly ushort NewFlags;
            public ushort TexCoord0X; // C# sucks!!!!!
            public ushort TexCoord0Y;
            public ushort TexCoord1X;
            public ushort TexCoord1Y;
            public ushort TexCoord2X;
            public ushort TexCoord2Y;
            public ushort TexCoord3X;
            public ushort TexCoord3Y;
            public readonly uint TextureSpaceIdentifier;
            private readonly uint Unused;

            public SavedObjectTexture(ushort textureID, TextureArea texture, uint textureSpaceIdentifier, TextureAllocator.TextureView view, bool isTriangular, bool isUsedInRoomMesh, bool canRotate, out byte firstTexCoordToEmit)
            {
                TextureID = textureID;
                IsTriangularAndPadding = isTriangular ? (ushort)1 : (ushort)0;
                BlendMode = (ushort)texture.BlendMode;
                NewFlags = GetNewFlag(texture, isTriangular, isUsedInRoomMesh, canRotate, out firstTexCoordToEmit);
                TextureSpaceIdentifier = textureSpaceIdentifier;
                Unused = 0;

                // C# sucks!!!!!
                TexCoord0X = TexCoord0Y = 0;
                TexCoord1X = TexCoord1Y = 0;
                TexCoord2X = TexCoord2Y = 0;
                TexCoord3X = TexCoord3Y = 0;

                //Save UV coordinates...
                int edgeCount = isTriangular ? 3 : 4;
                Vector2 lowerBound = new Vector2(127.0f / 256.0f);
                Vector2 upperBound = new Vector2(view.Area.Width, view.Area.Height) - new Vector2(129.0f / 256.0f);
                Vector2 lowerBountUint16 = new Vector2(0.0f);
                Vector2 upperBoundUint16 = new Vector2(view.Area.Width, view.Area.Height) * 256.0f - new Vector2(1.0f);
                Vector2[] texCoordModification = GetTexCoordModificationFromNewFlags(NewFlags, isTriangular);
                for (int i = 0; i < edgeCount; ++i)
                {
                    Vector2 value = texture.GetTexCoord((i + (canRotate ? firstTexCoordToEmit : 0)) % edgeCount);
                    value = Vector2.Max(lowerBound, Vector2.Min(upperBound, value));
                    value -= texCoordModification[i];
                    value *= 256.0f;
                    value = new Vector2((float)Math.Round(value.X), (float)Math.Round(value.Y));
                    value = Vector2.Max(lowerBountUint16, Vector2.Min(upperBoundUint16, value));

                    // C# sucks!!!!!
                    ushort x = (ushort)value.X;
                    ushort y = (ushort)value.Y;
                    switch (i)
                    {
                        case 0:
                            TexCoord0X = x;
                            TexCoord0Y = y;
                            break;
                        case 1:
                            TexCoord1X = x;
                            TexCoord1Y = y;
                            break;
                        case 2:
                            TexCoord2X = x;
                            TexCoord2Y = y;
                            break;
                        case 3:
                            TexCoord3X = x;
                            TexCoord3Y = y;
                            break;
                    }
                }
            }

            public void GetRealTexCoords(out Vector2 texCoord0, out Vector2 texCoord1, out Vector2 texCoord2, out Vector2 texCoord3)
            {
                bool isTriangle = IsTriangularAndPadding != 0;
                Vector2[] texCoordModification = GetTexCoordModificationFromNewFlags(NewFlags, isTriangle);
                texCoord0 = new Vector2(TexCoord0X, TexCoord0Y) * (1.0f / 256.0f) + texCoordModification[0];
                texCoord1 = new Vector2(TexCoord1X, TexCoord1Y) * (1.0f / 256.0f) + texCoordModification[1];
                texCoord2 = new Vector2(TexCoord2X, TexCoord2Y) * (1.0f / 256.0f) + texCoordModification[2];
                texCoord3 = isTriangle ? new Vector2() : new Vector2(TexCoord3X, TexCoord3Y) * (1.0f / 256.0f) + texCoordModification[3];
            }

            public void GetRealTexRect(out Vector2 minTexCoord, out Vector2 maxTexCoord)
            {
                bool isTriangle = IsTriangularAndPadding != 0;
                Vector2 texCoord0, texCoord1, texCoord2, texCoord3;
                GetRealTexCoords(out texCoord0, out texCoord1, out texCoord2, out texCoord3);
                minTexCoord = Vector2.Min(Vector2.Min(texCoord0, texCoord1), isTriangle ? texCoord2 : Vector2.Min(texCoord2, texCoord3));
                maxTexCoord = Vector2.Max(Vector2.Max(texCoord0, texCoord1), isTriangle ? texCoord2 : Vector2.Max(texCoord2, texCoord3));
            }

            // Custom implementation of these because default implementation is *insanely* slow.
            // Its not just a quite a bit slow, it really is *insanely* *crazy* slow so we need those functions :/
            public static bool operator ==(SavedObjectTexture first, SavedObjectTexture second)
            {
                ulong* firstPtr = (ulong*)&first;
                ulong* secondPtr = (ulong*)&second;
                return firstPtr[0] == secondPtr[0] && firstPtr[1] == secondPtr[1] && firstPtr[2] == secondPtr[2];
            }

            public static bool operator !=(SavedObjectTexture first, SavedObjectTexture second) => !(first == second);
            public bool Equals(SavedObjectTexture other) => this == other;
            public override bool Equals(object other) => other is SavedObjectTexture && this == (SavedObjectTexture)other;
            public override int GetHashCode() => base.GetHashCode();
        }
        private readonly List<SavedObjectTexture> _objectTextures = new List<SavedObjectTexture>();
        private readonly Dictionary<SavedObjectTexture, ushort> _objectTexturesLookup = new Dictionary<SavedObjectTexture, ushort>();
        private uint _textureSpaceIdentifier;
        private int _supportsUpTo65536TextureCount;

        private readonly TextureAllocator _textureAllocator = new TextureAllocator();

        private ushort AddObjectTextureWithoutLookup(SavedObjectTexture newEntry, bool supportsUpTo65536)
        {
            int newID = _objectTextures.Count;
            if (supportsUpTo65536)
            {
                if (newID > 0xffff)
                    throw new ApplicationException("More than 0xffff object textures are not possible for animated textures.");
                _supportsUpTo65536TextureCount += 1;
            }
            else
            {
                if (newID > 0x7fff)
                    throw new ApplicationException("More than 0x7fff object textures that are used for meshes in rooms/movables/statics are not possible.");
            }
            _objectTextures.Add(newEntry);
            return (ushort)newID;
        }

        public Result AddTexture(TextureArea texture, bool isTriangle, bool isUsedInRoomMesh, int packPriorityClass = 0, bool supportsUpTo65536 = false, bool canRotate = true, uint textureSpaceIdentifier = 0)
        {
            // Add object textures
            int textureID = _textureAllocator.GetOrAllocateTextureID(ref texture, isTriangle, packPriorityClass);
            bool isNew;
            byte firstTexCoordToEmit;
            ushort objTexIndex = AddOrGetObjectTexture(new SavedObjectTexture((ushort)textureID, texture, textureSpaceIdentifier,
                _textureAllocator.GetTextureFromID(textureID), isTriangle, isUsedInRoomMesh, canRotate, out firstTexCoordToEmit), supportsUpTo65536, out isNew);
            return new Result
            {
                ObjectTextureIndex = objTexIndex,
                FirstVertexIndexToEmit = firstTexCoordToEmit,
                Flags = (texture.DoubleSided ? ResultFlags.DoubleSided : ResultFlags.None) | (isNew ? ResultFlags.IsNew : ResultFlags.None)
            };
        }

        protected virtual void OnPackingTextures(IProgressReporter progressReporter)
        {}

        private volatile int _alphaBlendingTextureCount;
        public List<ImageC> PackTextures(IProgressReporter progressReporter)
        {
            //Add not yet required object textures to texture animations...
            OnPackingTextures(progressReporter);

            // Enable alpha blending for faces whose textures are not completely opaque.
            Parallel.For(0, _objectTextures.Count, objectTextureIndex =>
            {
                SavedObjectTexture objectTexture = _objectTextures[objectTextureIndex];
                if (objectTexture.BlendMode != (ushort)BlendMode.Normal) // Only consider alpha blending when blend mode is 0.
                    return;

                bool isTriangle = objectTexture.IsTriangularAndPadding != 0;
                TextureAllocator.TextureView textureView = _textureAllocator.GetTextureFromID(objectTexture.TextureID);

                // To simplify the test just use the rectangular region around. (We could do a polygonal thing but I am not sure its worth it)
                Vector2 minTexCoord, maxTexCoord;
                objectTexture.GetRealTexRect(out minTexCoord, out maxTexCoord);
                int startX = textureView.Area.X0 + (int)Math.Floor(minTexCoord.X);
                int startY = textureView.Area.Y0 + (int)Math.Floor(minTexCoord.Y);
                int endX = textureView.Area.X0 + (int)Math.Ceiling(maxTexCoord.X);
                int endY = textureView.Area.Y0 + (int)Math.Ceiling(maxTexCoord.Y);

                // Check for alpha
                bool hasAlpha = textureView.Texture.Image.HasAlpha(startX, startY, endX - startX, endY - startY);

                // Enable alpha if necessary
                if (hasAlpha)
                {
                    objectTexture.BlendMode = 1;
                    _objectTextures[objectTextureIndex] = objectTexture;
                    Interlocked.Increment(ref _alphaBlendingTextureCount); // Thread safe increment because Parallel.For
                }
            });
            progressReporter.ReportInfo("Alpha blending information built. ( Alpha blending enabled for " + _alphaBlendingTextureCount + " of " + _objectTextures.Count + " object textures)");
            progressReporter.ReportInfo("Count of general purpose object textures: " + (_objectTextures.Count - _supportsUpTo65536TextureCount) + " (The maximum is 32768)\n");
            progressReporter.ReportInfo("Count of extended object textures: " + _objectTextures.Count + " (The maximum is 65536)\n");

            //progressReporter.ReportInfo("Count of animated textures: " + AnimationsCache.Count);

            // Pack textures...
            List<ImageC> result = _textureAllocator.PackTextures();
            progressReporter.ReportInfo("Packed all level and wad textures into " + result.Count + " pages.");
            return result;
        }

        public uint GetNewTextureSpaceIdentifier() => ++_textureSpaceIdentifier;

        public int ObjectTextureCount => _objectTextures.Count;

        public void WriteObjectTextures(BinaryWriterEx stream, Level level)
        {
            // Write object textures
            stream.Write((uint)_objectTextures.Count);
            for (int i = 0; i < _objectTextures.Count; ++i)
            {
                SavedObjectTexture objectTexture = _objectTextures[i];
                TextureAllocator.Result UsedTexturePackInfo = _textureAllocator.GetPackInfo(objectTexture.TextureID);
                ushort Tile = UsedTexturePackInfo.OutputTextureID;
                if (level.Settings.GameVersion != GameVersion.TR2)
                    Tile |= objectTexture.IsTriangularAndPadding != 0 ? (ushort)0x8000 : (ushort)0;

                stream.Write(objectTexture.BlendMode);
                stream.Write(Tile);

                if (level.Settings.GameVersion == GameVersion.TR4 || level.Settings.GameVersion == GameVersion.TRNG ||
                                    level.Settings.GameVersion == GameVersion.TR5|| level.Settings.GameVersion == GameVersion.TR5Main)
                    stream.Write(objectTexture.NewFlags);

                UsedTexturePackInfo.TransformTexCoord(ref objectTexture.TexCoord0X, ref objectTexture.TexCoord0Y);
                UsedTexturePackInfo.TransformTexCoord(ref objectTexture.TexCoord1X, ref objectTexture.TexCoord1Y);
                UsedTexturePackInfo.TransformTexCoord(ref objectTexture.TexCoord2X, ref objectTexture.TexCoord2Y);
                UsedTexturePackInfo.TransformTexCoord(ref objectTexture.TexCoord3X, ref objectTexture.TexCoord3Y);

                stream.Write(objectTexture.TexCoord0X);
                stream.Write(objectTexture.TexCoord0Y);
                stream.Write(objectTexture.TexCoord1X);
                stream.Write(objectTexture.TexCoord1Y);
                stream.Write(objectTexture.TexCoord2X);
                stream.Write(objectTexture.TexCoord2Y);
                stream.Write(objectTexture.TexCoord3X);
                stream.Write(objectTexture.TexCoord3Y);

                if (level.Settings.GameVersion == GameVersion.TR4 || level.Settings.GameVersion == GameVersion.TRNG || level.Settings.GameVersion == GameVersion.TR5 || level.Settings.GameVersion == GameVersion.TR5Main)
                {
                    stream.Write((uint)0);
                    stream.Write((uint)0);
                    stream.Write((uint)0);
                    stream.Write((uint)0);
                }

                if (level.Settings.GameVersion == GameVersion.TR5 || level.Settings.GameVersion == GameVersion.TR5Main)
                    stream.Write((ushort)0);
            }
        }
    }
}
