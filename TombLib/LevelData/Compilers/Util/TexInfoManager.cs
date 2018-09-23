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
            private TextureArea _area;
            public TextureArea Area
            {
                get { return _area; }
                set
                {
                    // Disallow reducing area size, cause it corrupts children.
                    if (value == _area || Math.Abs(value.QuadArea) < Math.Abs(_area.QuadArea))
                        return;

                    // Calculate startpoint delta and fix up children to comply with new parent area.
                    if (Children != null && Children.Count > 0)
                    {
                        var delta = _area.TexCoord0 - value.TexCoord0;
                        foreach (var child in Children)
                        {
                            for (int i = 0; i < 4; i++)
                                child.Value[i] += delta;
                        }
                    }

                    // Set new parent area to min-max quad of incoming TextureArea
                    _area = GetQuad(value);
                }
            }
            public List<KeyValuePair<int, Vector2[]>> Children;

            // Generates new ParentTextureArea from raw texture coordinates.
            public ParentTextureArea(TextureArea newTextureArea)
            {
                Area = GetQuad(newTextureArea);
                Children = new List<KeyValuePair<int, Vector2[]>>();
            }

            // Gets canonical quad area in which texture is enclosed
            public static TextureArea GetQuad(TextureArea texture)
            {
                var minMax = texture.GetMinMax();
                return new TextureArea()
                {
                    BlendMode = texture.BlendMode,
                    BumpLevel = texture.BumpLevel,
                    DoubleSided = texture.DoubleSided,
                    Texture = texture.Texture,

                    // Please note that resulting parent area is a min-max quad, because
                    // generally it makes more sense to operate on it than on raw tex coords.
                    TexCoord0 = new Vector2(minMax[0].X, minMax[0].Y),
                    TexCoord1 = new Vector2(minMax[1].X, minMax[0].Y),
                    TexCoord2 = new Vector2(minMax[1].X, minMax[1].Y),
                    TexCoord3 = new Vector2(minMax[0].X, minMax[1].Y)
                };
            }

            // Checks if parameters are similar to another texture area, and if so,
            // also checks if texture area is enclosed in parent's area.
            public bool IsPotentialChild(TextureArea texture)
            {
                var minMax = texture.GetMinMax();

                var diff1 = Area.TexCoord0 - minMax[0];
                var diff2 = Area.TexCoord0 - minMax[0];

                return (Area.ParametersSimilar(texture) &&
                       (diff1.X <= 0.0f && diff1.Y <= 0.0f) &&
                       (diff2.X >= 0.0f && diff2.Y >= 0.0f));
            }

            // Checks if incoming texture is similar in parameters and encloses parent area.
            public bool IsPotentialParent(TextureArea texture)
            {
                var minMax = texture.GetMinMax();

                var diff1 = Area.TexCoord0 - minMax[0];
                var diff2 = Area.TexCoord0 - minMax[0];

                return (Area.ParametersSimilar(texture) &&
                       (diff1.X > 0.0f && diff1.Y > 0.0f) &&
                       (diff2.X < 0.0f && diff2.Y < 0.0f));
            }

            // Adds texture as a child to existing parent, with recalculating coordinates to relative.
            public void AddChild(TextureArea texture, int newTextureID)
            {
                Children.Add(new KeyValuePair<int, Vector2[]>(newTextureID,
                    new Vector2[4]
                    {
                        new Vector2 { X = texture.TexCoord0.X - Area.TexCoord0.X, Y = texture.TexCoord0.Y - Area.TexCoord0.Y },
                        new Vector2 { X = texture.TexCoord1.X - Area.TexCoord0.X, Y = texture.TexCoord1.Y - Area.TexCoord0.Y },
                        new Vector2 { X = texture.TexCoord2.X - Area.TexCoord0.X, Y = texture.TexCoord2.Y - Area.TexCoord0.Y },
                        new Vector2 { X = texture.TexCoord3.X - Area.TexCoord0.X, Y = texture.TexCoord3.Y - Area.TexCoord0.Y }
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

        public int TryToAddToExisting(TextureArea texture)
        {
            var textureQuad = ParentTextureArea.GetQuad(texture);
            foreach (var parent in ParentTextures)
            {
                // Try to find potential child (smaller texture) and make itself a child
                if (parent.IsPotentialParent(textureQuad))
                    parent.Area = textureQuad;
                // Try to find potential parent (larger texture) and add itself to children
                else if (!parent.IsPotentialChild(textureQuad))
                    continue;

                var newTexIndex = GetNewTexInfoIndex();
                parent.AddChild(texture, newTexIndex);
                return newTexIndex;
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

        public Result AddTexture(TextureArea texture, bool isTriangle, bool isUsedInRoomMesh, int packPriorityClass = 0)
        {
            // Try to get existing TexInfo
            byte rotation = 0;
            int texInfoIndex = GetTexInfo(texture, out rotation);

            // Try to create new canonical texture as child or parent
            if (texInfoIndex == -1)
            {
                var canonicalTexture = texture.GetCanonicalTexture(out rotation);
                texInfoIndex = TryToAddToExisting(canonicalTexture);

                // No any potential parents or children, create as new parent
                if(texInfoIndex == -1)
                {
                    var newParent = new ParentTextureArea(canonicalTexture);
                    newParent.AddChild(canonicalTexture, GetNewTexInfoIndex());
                    ParentTextures.Add(newParent);
                }
            }

            return new Result() { TexInfoIndex = texInfoIndex, Rotation = rotation };
        }
    }
}
