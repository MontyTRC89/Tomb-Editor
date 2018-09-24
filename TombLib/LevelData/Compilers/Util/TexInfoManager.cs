using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
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

        public int MaxParentSize = 256;
        public int NumNonBumpedTexturePages = 0;
        public int NumBumpedTexturePages = 0;

        // Get page offset for bump-mapped texture pages
        private int PageOffset(ushort flags) => ((flags & 0x1800) != 0) ? NumNonBumpedTexturePages : 0;
        
        // ChildTextureArea is a simple enclosed relative texture area with stripped down parameters
        // which should be the same among all children of same parent.
        // Stripped down parameters include BumpLevel and Texture, because if these are different, it
        // automatically means we should assign new parent for this child.

        public class ChildTextureArea
        {
            public int TexInfoIndex;
            public Vector2[] TexCoord;  // Relative to parent!

            public BlendMode BlendMode;
            public bool IsForTriangle;
            
            // Returns absolute child coordinates
            public static Vector2[] GetAbsChildCoordinates(ParentTextureArea parent, ChildTextureArea child)
            {
                if (child.TexCoord.Length != 4)
                {
                    logger.Error("GetAbsChildCoordinates: Weird coordinates count encountered!");
                    return new Vector2[] { Vector2.Zero };
                }

                return new Vector2[] { new Vector2(parent.Area.TexCoord0.X + child.TexCoord[0].X, parent.Area.TexCoord0.Y + child.TexCoord[0].Y),
                                       new Vector2(parent.Area.TexCoord0.X + child.TexCoord[1].X, parent.Area.TexCoord0.Y + child.TexCoord[1].Y),
                                       new Vector2(parent.Area.TexCoord0.X + child.TexCoord[2].X, parent.Area.TexCoord0.Y + child.TexCoord[2].Y),
                                       new Vector2(parent.Area.TexCoord0.X + child.TexCoord[3].X, parent.Area.TexCoord0.Y + child.TexCoord[3].Y), };
            }
        }

        // ParentTextureArea is a texture area which contains all other texture areas which are
        // completely inside current one. Blending mode and bumpmapping parameters define that
        // parent is different, hence two TextureAreas with same UV coordinates but with different
        // BlendMode and BumpLevel will be saved as different parents.

        public class ParentTextureArea
        {
            public VectorInt2 PositionInPage { get; set; }
            public int Page { get; set; }
            public int Padding { get; set; }

            public bool IsForRoom { get; private set; }
            public int PackPriority { get; private set; }

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
                                child.TexCoord[i] += delta;
                        }
                    }

                    // Set new parent area to min-max quad of incoming TextureArea
                    _area = GetQuad(value);
                }
            }
            public List<ChildTextureArea> Children;

            // Generates new ParentTextureArea from raw texture coordinates.
            public ParentTextureArea(TextureArea newTextureArea, bool isForRoom, int packPriority)
            {
                Area = GetQuad(newTextureArea);
                Children = new List<ChildTextureArea>();
                IsForRoom = isForRoom;
                PackPriority = packPriority;
            }

            // Compare parent's properties with incoming texture properties.
            public bool ParametersSimilar(TextureArea incomingTexture, bool isForRoom, int packPriority)
            {
                return ( Area.BumpLevel == incomingTexture.BumpLevel &&
                         Area.Texture   == incomingTexture.Texture &&
                         IsForRoom      == isForRoom &&
                         PackPriority   == packPriority );
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
            public bool IsPotentialParent(TextureArea texture, bool isForRoom, int packPriority)
            {
                var minMax = texture.GetMinMax();

                var diff1 = Area.TexCoord0 - minMax[0];
                var diff2 = Area.TexCoord2 - minMax[1];

                return (ParametersSimilar(texture, isForRoom, packPriority) &&
                       (diff1.X <= 0.0f && diff1.Y <= 0.0f) &&
                       (diff2.X >= 0.0f && diff2.Y >= 0.0f));
            }

            // Checks if incoming texture is similar in parameters and encloses parent area.
            public bool IsPotentialChild(TextureArea texture, bool isForRoom, int packPriority)
            {
                var minMax = texture.GetMinMax();

                var diff1 = Area.TexCoord0 - minMax[0];
                var diff2 = Area.TexCoord2 - minMax[1];

                return (ParametersSimilar(texture, isForRoom, packPriority) &&
                       (diff1.X >= 0.0f && diff1.Y >= 0.0f) &&
                       (diff2.X <= 0.0f && diff2.Y <= 0.0f));
            }

            // Adds texture as a child to existing parent, with recalculating coordinates to relative.
            public void AddChild(TextureArea texture, int newTextureID, bool isForTriangle)
            {
                Children.Add(new ChildTextureArea()
                {
                    TexInfoIndex = newTextureID,
                    BlendMode = texture.BlendMode,
                    IsForTriangle = isForTriangle,
                    TexCoord = new Vector2[4]
                    {
                        new Vector2 { X = texture.TexCoord0.X - Area.TexCoord0.X, Y = texture.TexCoord0.Y - Area.TexCoord0.Y },
                        new Vector2 { X = texture.TexCoord1.X - Area.TexCoord0.X, Y = texture.TexCoord1.Y - Area.TexCoord0.Y },
                        new Vector2 { X = texture.TexCoord2.X - Area.TexCoord0.X, Y = texture.TexCoord2.Y - Area.TexCoord0.Y },
                        new Vector2 { X = texture.TexCoord3.X - Area.TexCoord0.X, Y = texture.TexCoord3.Y - Area.TexCoord0.Y }
                    }
                });
            }

            public void MoveChild(ChildTextureArea child, ParentTextureArea newParent)
            {
                var absCoordinates = ChildTextureArea.GetAbsChildCoordinates(this, child);

                newParent.Children.Add(new ChildTextureArea()
                {
                    TexInfoIndex = child.TexInfoIndex,
                    BlendMode = child.BlendMode,
                    IsForTriangle = child.IsForTriangle,
                    TexCoord = new Vector2[4]
                    {
                        new Vector2 { X = absCoordinates[0].X - Area.TexCoord0.X, Y = absCoordinates[0].Y - Area.TexCoord0.Y },
                        new Vector2 { X = absCoordinates[1].X - Area.TexCoord0.X, Y = absCoordinates[1].Y - Area.TexCoord0.Y },
                        new Vector2 { X = absCoordinates[2].X - Area.TexCoord0.X, Y = absCoordinates[2].Y - Area.TexCoord0.Y },
                        new Vector2 { X = absCoordinates[3].X - Area.TexCoord0.X, Y = absCoordinates[3].Y - Area.TexCoord0.Y }
                    }
                });
            }

            public void MergeParents(List<ParentTextureArea> parents)
            {
                foreach (var parent in parents)
                {

                    foreach (var child in parent.Children)
                        parent.MoveChild(child, this);

                    parent.Children.Clear();
                }
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

        public int TryToAddToExisting(TextureArea texture, bool isForRoom, bool isForTriangle, int packPriority)
        {
            var textureQuad = ParentTextureArea.GetQuad(texture);

            // Try to find and merge parents which are enclosed in incoming texture area
            var childrenWannabes = ParentTextures.Where(item => item.IsPotentialChild(textureQuad, isForRoom, packPriority)).ToList();
            if(childrenWannabes.Count > 0)
            {
                var newParent = new ParentTextureArea(textureQuad, isForRoom, packPriority);
                var texIndex = GetNewTexInfoIndex();
                newParent.AddChild(texture, texIndex, isForTriangle);
                newParent.MergeParents(childrenWannabes);
                childrenWannabes.ForEach(item => ParentTextures.Remove(item));
                ParentTextures.Add(newParent);
                return texIndex;
            }

            // Try to find potential parent (larger texture) and add itself to children
            foreach (var parent in ParentTextures)
            {
                if (!parent.IsPotentialParent(textureQuad, isForRoom, packPriority))
                    continue;

                var newTexIndex = GetNewTexInfoIndex();
                parent.AddChild(texture, newTexIndex, isForTriangle);
                return newTexIndex;
            }

            // No success
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

        // Gets existing TexInfo child index if there is similar one in ParentTextures list.

        private int GetTexInfo(TextureArea areaToLook, bool isForRoom, bool isForTriangle, int packPriority, out byte rotation)
        {
            rotation = 0;

            foreach (var parent in ParentTextures)
            {
                // Parents with different attributes are quickly discarded
                if(!parent.ParametersSimilar(areaToLook, isForRoom, packPriority))
                    continue;

                var lookupCoordinates = new Vector2[] { new Vector2(areaToLook.TexCoord0.X, areaToLook.TexCoord0.Y),
                                                        new Vector2(areaToLook.TexCoord1.X, areaToLook.TexCoord1.Y),
                                                        new Vector2(areaToLook.TexCoord2.X, areaToLook.TexCoord2.Y),
                                                        new Vector2(areaToLook.TexCoord3.X, areaToLook.TexCoord3.Y), };

                // Extract each children's absolute coordinates and compare them to incoming texture coordinates.
                foreach (var child in parent.Children)
                {
                    // If parameters are different, children is quickly discarded from comparison.
                    if (child.BlendMode != areaToLook.BlendMode || child.IsForTriangle != isForTriangle)
                        continue;

                    // Test if coordinates are mutually equal and return resulting rotation if they are
                    var result = TestUVSimilarity(ChildTextureArea.GetAbsChildCoordinates(parent, child), lookupCoordinates);
                    if(result != -1)
                    {
                        // Child is rotation-wise equal to incoming area
                        rotation = (byte)result;
                        return child.TexInfoIndex;
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

        public int AddParent(TextureArea texture, bool isForTriangle, bool isForRoom, int packPriority)
        {
            var newParent = new ParentTextureArea(texture, isForRoom, packPriority);
            var texInfoIndex = GetNewTexInfoIndex();
            newParent.AddChild(texture, texInfoIndex, isForTriangle);
            ParentTextures.Add(newParent);
            return texInfoIndex;
        }

        public Result AddTexture(TextureArea texture, bool isForTriangle, bool isForRoom, int packPriority = 0)
        {
            // Try to get existing TexInfo
            byte rotation = 0;
            int texInfoIndex = GetTexInfo(texture, isForRoom, isForTriangle, packPriority, out rotation);

            if (texInfoIndex == -1)
            {
                // Try to create new canonical texture as child or parent
                var canonicalTexture = texture.GetCanonicalTexture(out rotation);
                texInfoIndex = TryToAddToExisting(canonicalTexture, isForRoom, isForTriangle, packPriority);

                // No any potential parents or children, create as new parent
                if (texInfoIndex == -1)
                    AddParent(canonicalTexture, isForTriangle, isForRoom, packPriority);
            }

            return new Result() { TexInfoIndex = texInfoIndex, Rotation = rotation };
        }

        public void AddAnimatedTextures(List<AnimatedTextureSet> animatedTextures)
        {
            foreach(var set in animatedTextures)
            {

            }
        }

        public void PackTextures()
        {
            int padding = 16;
            int currentPage = 0;
            RectPacker packer = new RectPackerSimpleStack(new VectorInt2(256, 256));

            for (int i = 0; i < ParentTextures.Count; i++)
            {
                // Get the size of the quad surrounding texture area, typically should be the texture area itself
                int w = (int)(ParentTextures[i].Area.GetMinMax()[1].X - ParentTextures[i].Area.GetMinMax()[0].X);
                int h = (int)(ParentTextures[i].Area.GetMinMax()[1].Y - ParentTextures[i].Area.GetMinMax()[0].Y);

                // If texture is not too big we can pad it, otherwise we don't pad it
                // TODO: implement adaptative padding (for example 2 could be bad, but 1 could be fine instead of 0)
                if (w + 2 * padding < 256 && h + 2 * padding < 256)
                {
                    w += padding * 2;
                    h += padding * 2;
                    ParentTextures[i].Padding = padding;
                }

                // Pack texture
                // TODO: use tree packer for better packing
                var result = packer.TryAdd(new VectorInt2(w, h));
                if (result == null || !result.HasValue)
                {
                    packer = new RectPackerSimpleStack(new VectorInt2(256, 256));
                    currentPage++;
                    result = packer.TryAdd(new VectorInt2(w, h));
                }

                ParentTextures[i].Page = currentPage;
                ParentTextures[i].PositionInPage = result.Value;
            }

            // DEBUG: Now for testing create a bitmap
            var image = ImageC.CreateNew(256, (currentPage + 1) * 256);
            for (int i = 0; i < ParentTextures.Count; i++)
            {
                var p = ParentTextures[i];
                var x = (int)p.Area.GetMinMax()[0].X;
                var y = (int)p.Area.GetMinMax()[0].Y;
                var width  = (int)(p.Area.GetMinMax()[1].X - p.Area.GetMinMax()[0].X + 1);
                var height = (int)(p.Area.GetMinMax()[1].Y - p.Area.GetMinMax()[0].Y + 1);

                image.CopyFrom(p.PositionInPage.X + p.Padding, p.Page * 256 + p.PositionInPage.Y + p.Padding, p.Area.Texture.Image,
                               x, y, width, height);

                // Add actual padding (ported code from OT bordered_texture_atlas.cpp)

                var topLeft = p.Area.Texture.Image.GetPixel(x, y);
                var topRight = p.Area.Texture.Image.GetPixel(x + width - 1, y);
                var bottomLeft = p.Area.Texture.Image.GetPixel(x, y + height - 1);
                var bottomRight = p.Area.Texture.Image.GetPixel(x + width - 1, y + height - 1);

                for (int xP = 0; xP < padding; xP++)
                {
                    // copy left line
                    image.CopyFrom(p.PositionInPage.X + xP, p.Page * 256 + p.PositionInPage.Y + padding, p.Area.Texture.Image,
                               x, y, 1, height - 1);
                    // copy right line
                    image.CopyFrom(p.PositionInPage.X + xP + width - 1 + padding, p.Page * 256 + p.PositionInPage.Y + padding, p.Area.Texture.Image,
                               x + width - 1, y, 1, height - 1);

                    for (int yP = 0; yP < padding; yP++)
                    {
                        // copy top line
                        image.CopyFrom(p.PositionInPage.X + padding, p.Page * 256 + p.PositionInPage.Y + yP, p.Area.Texture.Image,
                                   x, y, width - 1, 1);
                        // copy bottom line
                        image.CopyFrom(p.PositionInPage.X + padding, p.Page * 256 + p.PositionInPage.Y + yP + height - 1 + padding, p.Area.Texture.Image,
                                   x, y + height - 1, width - 1, 1);

                        // expand top-left pixel
                        image.SetPixel(p.PositionInPage.X + xP, p.Page * 256 + p.PositionInPage.Y + yP, topLeft);
                        // expand top-right pixel
                        image.SetPixel(p.PositionInPage.X + xP + width - 1 + padding, p.Page * 256 + p.PositionInPage.Y + yP, topRight);
                        // expand bottom-left pixel
                        image.SetPixel(p.PositionInPage.X + xP, p.Page * 256 + p.PositionInPage.Y + yP + height - 1 + padding, bottomLeft);
                        // expand bottom-right pixel
                        image.SetPixel(p.PositionInPage.X + xP + width - 1 + padding, p.Page * 256 + p.PositionInPage.Y + yP + height - 1 + padding, bottomRight);
                    }
                }
            }
            image.Save("E:\\testpack.png");
        }
    }
}
