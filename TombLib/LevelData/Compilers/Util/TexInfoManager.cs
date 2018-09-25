using NLog;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Numerics;
using TombLib.IO;
using TombLib.Utils;

namespace TombLib.LevelData.Compilers.Util
{
    public class TexInfoManager
    {
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();

        // Two lists of animated textures contain reference animation versions for each sequence
        // and actual found animated texture sequences in rooms. When compiler encounters a tile
        // which is similar to one of the reference animation versions, it copies it into actual
        // animations list with all respective frames. Any new comparison is made with actual
        // animation sequences at first, so next found similar tile will refer to already existing
        // animation version.
        // On packing, list of actual anim textures is added to main ParentTextures list, so only
        // versions existing in level file are added to texinfo list. Rest from reference list
        // is ignored.

        public List<ParentAnimatedTexture> ReferenceAnimTextures = new List<ParentAnimatedTexture>();
        public List<ParentAnimatedTexture> ActualAnimTextures = new List<ParentAnimatedTexture>();

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

                return new Vector2[] { parent.Area.Start + child.TexCoord[0], parent.Area.Start + child.TexCoord[1],
                                       parent.Area.Start + child.TexCoord[2], parent.Area.Start + child.TexCoord[3] };
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

            public Texture Texture { get; private set; }
            public BumpLevel BumpLevel { get; private set; }
            public bool IsForRoom { get; private set; }
            public int PackPriority { get; private set; }

            private Rectangle2 _area;
            public Rectangle2 Area
            {
                get { return _area; }
                set
                {
                    // Disallow reducing area size, cause it corrupts children.
                    if (value == _area || !value.Contains(_area))
                        return;

                    // Calculate startpoint delta and fix up children to comply with new parent area.
                    if (Children != null && Children.Count > 0)
                    {
                        var delta = _area.Start - value.Start;
                        foreach (var child in Children)
                            for (int i = 0; i < 4; i++)
                                child.TexCoord[i] += delta;
                    }

                    _area = value;
                }
            }
            public List<ChildTextureArea> Children;

            // Generates new ParentTextureArea from raw texture coordinates.
            public ParentTextureArea(TextureArea texture, bool isForRoom, int packPriority)
            {
                _area = texture.GetRect();
                Children = new List<ChildTextureArea>();

                Texture = texture.Texture;
                BumpLevel = texture.BumpLevel;
                IsForRoom = isForRoom;
                PackPriority = packPriority;
            }

            // Compare parent's properties with incoming texture properties.
            public bool ParametersSimilar(TextureArea incomingTexture, bool isForRoom, int packPriority)
                => BumpLevel == incomingTexture.BumpLevel && Texture == incomingTexture.Texture && IsForRoom == isForRoom && PackPriority == packPriority;

            // Checks if parameters are similar to another texture area, and if so,
            // also checks if texture area is enclosed in parent's area.
            public bool IsPotentialParent(TextureArea texture, bool isForRoom, int packPriority)
                => (ParametersSimilar(texture, isForRoom, packPriority) && texture.GetRect().Contains(_area));

            // Checks if incoming texture is similar in parameters and encloses parent area.
            public bool IsPotentialChild(TextureArea texture, bool isForRoom, int packPriority, bool allowOverlaps = false, uint maxOverlappedSize = 256)
            {
                var rect = texture.GetRect();
                if (!allowOverlaps)
                    return (ParametersSimilar(texture, isForRoom, packPriority) && _area.Contains(rect));
                else
                {
                    var potentialNewArea = rect.Union(_area);
                    return (ParametersSimilar(texture, isForRoom, packPriority) &&
                           (potentialNewArea.Width <= maxOverlappedSize) &&
                           (potentialNewArea.Height <= maxOverlappedSize));
                }
            }

            // Adds texture as a child to existing parent, with recalculating coordinates to relative.
            public void AddChild(TextureArea texture, int newTextureID, bool isForTriangle)
            {
                Children.Add(new ChildTextureArea()
                {
                    TexInfoIndex = newTextureID,
                    BlendMode = texture.BlendMode,
                    IsForTriangle = isForTriangle,
                    TexCoord = new Vector2[4] { texture.TexCoord0 - Area.Start,
                                                texture.TexCoord1 - Area.Start,
                                                texture.TexCoord2 - Area.Start,
                                                texture.TexCoord3 - Area.Start }
                });

                // Expand parent area, if needed
                var rect = texture.GetRect();
                if (!Area.Contains(rect))
                    Area.Union(rect);
            }

            public void MoveChild(ChildTextureArea child, ParentTextureArea newParent)
            {
                var absCoordinates = ChildTextureArea.GetAbsChildCoordinates(this, child);

                newParent.Children.Add(new ChildTextureArea()
                {
                    TexInfoIndex = child.TexInfoIndex,
                    BlendMode = child.BlendMode,
                    IsForTriangle = child.IsForTriangle,
                    TexCoord = new Vector2[4] { absCoordinates[0] - Area.Start,
                                                absCoordinates[1] - Area.Start,
                                                absCoordinates[2] - Area.Start,
                                                absCoordinates[3] - Area.Start }
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

        // add desc!

        public class ParentAnimatedTexture
        {
            public AnimatedTextureSet Origin;
            public List<ParentTextureArea> CompiledAnimation = new List<ParentTextureArea>();

            public ParentAnimatedTexture(AnimatedTextureSet origin)
            {
                Origin = origin;
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
            // Try to find and merge parents which are enclosed in incoming texture area
            var childrenWannabes = ParentTextures.Where(item => item.IsPotentialChild(texture, isForRoom, packPriority)).ToList();
            if(childrenWannabes.Count > 0)
            {
                var newParent = new ParentTextureArea(texture, isForRoom, packPriority);
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
                if (!parent.IsPotentialParent(texture, isForRoom, packPriority))
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
                    texInfoIndex = AddParent(canonicalTexture, isForTriangle, isForRoom, packPriority);
            }

            return new Result() { TexInfoIndex = texInfoIndex, Rotation = rotation };
        }

        public void PackTextures()
        {
            int padding = 16;
            int currentPage = 0;
            RectPacker packer = new RectPackerSimpleStack(new VectorInt2(256, 256));

            for (int i = 0; i < ParentTextures.Count; i++)
            {
                // Get the size of the quad surrounding texture area, typically should be the texture area itself
                int w = (int)(ParentTextures[i].Area.End.X - ParentTextures[i].Area.Start.X);
                int h = (int)(ParentTextures[i].Area.End.Y - ParentTextures[i].Area.Start.Y);

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
                var x = (int)p.Area.Start.X;
                var y = (int)p.Area.Start.Y;
                var width  = (int)(p.Area.End.X - p.Area.Start.X + 1);
                var height = (int)(p.Area.End.Y - p.Area.Start.Y + 1);

                image.CopyFrom(p.PositionInPage.X + p.Padding, p.Page * 256 + p.PositionInPage.Y + p.Padding, p.Texture.Image,
                               x, y, width, height);

                // Add actual padding (ported code from OT bordered_texture_atlas.cpp)

                var topLeft = p.Texture.Image.GetPixel(x, y);
                var topRight = p.Texture.Image.GetPixel(x + width - 1, y);
                var bottomLeft = p.Texture.Image.GetPixel(x, y + height - 1);
                var bottomRight = p.Texture.Image.GetPixel(x + width - 1, y + height - 1);

                for (int xP = 0; xP < padding; xP++)
                {
                    // copy left line
                    image.CopyFrom(p.PositionInPage.X + xP, p.Page * 256 + p.PositionInPage.Y + padding, p.Texture.Image,
                               x, y, 1, height - 1);
                    // copy right line
                    image.CopyFrom(p.PositionInPage.X + xP + width - 1 + padding, p.Page * 256 + p.PositionInPage.Y + padding, p.Texture.Image,
                               x + width - 1, y, 1, height - 1);

                    for (int yP = 0; yP < padding; yP++)
                    {
                        // copy top line
                        image.CopyFrom(p.PositionInPage.X + padding, p.Page * 256 + p.PositionInPage.Y + yP, p.Texture.Image,
                                   x, y, width - 1, 1);
                        // copy bottom line
                        image.CopyFrom(p.PositionInPage.X + padding, p.Page * 256 + p.PositionInPage.Y + yP + height - 1 + padding, p.Texture.Image,
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
