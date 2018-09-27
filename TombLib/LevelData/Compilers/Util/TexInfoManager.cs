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

        public uint MaxParentSize = 256;
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
                var result = new Vector2[child.TexCoord.Length];
                for (int i = 0; i < child.TexCoord.Length; i++)
                    result[i] = parent.Area.Start + child.TexCoord[i];
                return result;
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
            public int PackPriority { get; set; }

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
                            for (int i = 0; i < child.TexCoord.Length; i++)
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
            public bool ParametersSimilar(TextureArea incomingTexture, bool isForRoom)
                => BumpLevel == incomingTexture.BumpLevel && Texture == incomingTexture.Texture && IsForRoom == isForRoom;

            // Checks if parameters are similar to another texture area, and if so,
            // also checks if texture area is enclosed in parent's area.
            public bool IsPotentialParent(TextureArea texture, bool isForRoom)
                => (ParametersSimilar(texture, isForRoom) && _area.Contains(texture.GetRect()));

            // Checks if incoming texture is similar in parameters and encloses parent area.
            public bool IsPotentialChild(TextureArea texture, bool isForRoom, bool allowOverlaps = false, uint maxOverlappedSize = 256)
            {
                var rect = texture.GetRect();
                if (!allowOverlaps)
                    return (ParametersSimilar(texture, isForRoom) && rect.Contains(_area));
                else
                {
                    var potentialNewArea = rect.Union(_area);
                    return (ParametersSimilar(texture, isForRoom) &&
                           (potentialNewArea.Width <= maxOverlappedSize) &&
                           (potentialNewArea.Height <= maxOverlappedSize));
                }
            }

            // Adds texture as a child to existing parent, with recalculating coordinates to relative.
            public void AddChild(TextureArea texture, int newTextureID, bool isForTriangle)
            {
                var result = new Vector2[isForTriangle ? 3 : 4];
                for (int i = 0; i < result.Length; i++)
                    result[i] = texture.GetTexCoord(i) - Area.Start;

                Children.Add(new ChildTextureArea()
                {
                    TexInfoIndex = newTextureID,
                    BlendMode = texture.BlendMode,
                    IsForTriangle = isForTriangle,
                    TexCoord = result
                });

                // Expand parent area, if needed
                var rect = texture.GetRect();
                if (!Area.Contains(rect))
                    Area.Union(rect);
            }

            public void MoveChild(ChildTextureArea child, ParentTextureArea newParent)
            {
                var newCoordinates = ChildTextureArea.GetAbsChildCoordinates(this, child);
                for (int i = 0; i < newCoordinates.Length; i++)
                    newCoordinates[i] -= Area.Start;

                newParent.Children.Add(new ChildTextureArea()
                {
                    TexInfoIndex = child.TexInfoIndex,
                    BlendMode = child.BlendMode,
                    IsForTriangle = child.IsForTriangle,
                    TexCoord = newCoordinates
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

        // ParentAnimatedTexture contains all precompiled frames of a specific anim texture set.
        // Since animated textures can overlap (especially with animators), we store list of frames
        // as a list of parents. Children and parents are added in sequential order, so no sorting
        // must be made on CompiledAnimation.

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

        public int TryToAddToExisting(TextureArea texture, List<ParentTextureArea> parentList, bool isForRoom, bool isForTriangle, int packPriority, bool allowOverlaps = false)
        {
            // Try to find and merge parents which are enclosed in incoming texture area
            var childrenWannabes = parentList.Where(item => item.IsPotentialChild(texture, isForRoom, allowOverlaps, MaxParentSize)).ToList();
            if (childrenWannabes.Count > 0)
            {
                var newPriority = Math.Max(packPriority, childrenWannabes.Max(item => item.PackPriority));
                var newParent = new ParentTextureArea(texture, isForRoom, newPriority);
                var texIndex = GetNewTexInfoIndex();
                newParent.AddChild(texture, texIndex, isForTriangle);
                newParent.MergeParents(childrenWannabes);
                childrenWannabes.ForEach(item => parentList.Remove(item));
                parentList.Add(newParent);
                return texIndex;
            }

            // Try to find potential parent (larger texture) and add itself to children
            foreach (var parent in parentList)
            {
                if (!parent.IsPotentialParent(texture, isForRoom))
                    continue;

                var newTexIndex = GetNewTexInfoIndex();
                parent.AddChild(texture, newTexIndex, isForTriangle);
                parent.PackPriority = Math.Max(parent.PackPriority, packPriority);
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
                if (indices.Length != 3)
                    throw new ArgumentOutOfRangeException(nameof(indices.Length));

                ushort objectTextureIndex = (ushort)(GetLegacyIndex() | (doubleSided ? 0x8000 : 0));
                ushort[] transformedIndices = new ushort[3] { indices[0], indices[1], indices[2] };

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

        // Gets existing TexInfo child index if there is similar one in parent textures list.

        private int GetTexInfo(TextureArea areaToLook, List<ParentTextureArea> parentList, bool isForRoom, bool isForTriangle, out byte rotation)
        {
            rotation = 0;

            foreach (var parent in parentList)
            {
                // Parents with different attributes are quickly discarded
                if (!parent.ParametersSimilar(areaToLook, isForRoom))
                    continue;

                var lookupCoordinates = new Vector2[isForTriangle ? 3 : 4];
                for (int i = 0; i < lookupCoordinates.Length; i++)
                    lookupCoordinates[i] = areaToLook.GetTexCoord(i);

                // Extract each children's absolute coordinates and compare them to incoming texture coordinates.
                foreach (var child in parent.Children)
                {
                    // If parameters are different, children is quickly discarded from comparison.
                    if (child.BlendMode != areaToLook.BlendMode || child.IsForTriangle != isForTriangle)
                        continue;

                    // Test if coordinates are mutually equal and return resulting rotation if they are
                    var result = TestUVSimilarity(ChildTextureArea.GetAbsChildCoordinates(parent, child), lookupCoordinates);
                    if (result != -1)
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

        public int AddParent(TextureArea texture, List<ParentTextureArea> parentList, bool isForTriangle, bool isForRoom, int packPriority)
        {
            var newParent = new ParentTextureArea(texture, isForRoom, packPriority);
            var texInfoIndex = GetNewTexInfoIndex();
            newParent.AddChild(texture, texInfoIndex, isForTriangle);
            parentList.Add(newParent);
            return texInfoIndex;
        }


        public Result AddTexture(TextureArea texture, bool isForTriangle, bool isForRoom, int packPriority = 0)
        {
            // TODO: add anim tex comparer here
            return AddTexture(texture, ParentTextures, isForTriangle, isForRoom, packPriority);
        }

        private Result AddTexture(TextureArea texture, List<ParentTextureArea> parentList, bool isForTriangle, bool isForRoom, int packPriority = 0, bool packAnimations = false, bool makeCanonical = true)
        {
            // Rotation receives incoming texture's rotation in relation to canonical texture which is possibly already
            // in the list. This is needed to prevent creation of extra TexInfos for similar rotated textures.
            byte rotation = 0;

            // In case AddTexture is used with animated seq packing, we don't check frames for full similarity, because
            // frames can be dublicated with Repeat function or simply because of complex animator functions applied.
            int texInfoIndex = packAnimations ? -1 : GetTexInfo(texture, parentList, isForRoom, isForTriangle, out rotation);

            if (texInfoIndex == -1)
            {
                // Try to create new canonical (top-left-based) texture as child or parent.
                // makeCanonical parameter is necessary for UVRotate animations, because in case texture is not top-left-based,
                // engine will incorrectly calculate texture split for UV panning.
                var canonicalTexture = makeCanonical ? texture : texture.GetCanonicalTexture(isForTriangle, out rotation);
                texInfoIndex = TryToAddToExisting(canonicalTexture, parentList, isForRoom, isForTriangle, packPriority, packAnimations);

                // No any potential parents or children, create as new parent
                if (texInfoIndex == -1)
                    texInfoIndex = AddParent(canonicalTexture, parentList, isForTriangle, isForRoom, packPriority);
            }

            return new Result() { TexInfoIndex = texInfoIndex, Rotation = rotation };
        }

        private int PlaceTexturesInMap(ref List<ParentTextureArea> textures, int padding)
        {
            int currentPage = 0;
            RectPacker packer = new RectPackerSimpleStack(new VectorInt2(256, 256));

            for (int i = 0; i < textures.Count; i++)
            {
                // Get the size of the quad surrounding texture area, typically should be the texture area itself
                int w = (int)(textures[i].Area.End.X - textures[i].Area.Start.X);
                int h = (int)(textures[i].Area.End.Y - textures[i].Area.Start.Y);

                // If texture is not too big we can pad it, otherwise we don't pad it
                // TODO: implement adaptative padding (for example 2 could be bad, but 1 could be fine instead of 0)
                if (w + 2 * padding < 256 && h + 2 * padding < 256)
                {
                    w += padding * 2;
                    h += padding * 2;
                    textures[i].Padding = padding;
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

                textures[i].Page = currentPage;
                textures[i].PositionInPage = result.Value;
            }

            return (currentPage + 1);
        }

        public void BuildAnimTextures(List<AnimatedTextureSet> sets)
        {
            foreach(var set in sets)
            {
                // Ignore trivial (single-frame non-UVRotated) anims
                if (set.AnimationIsTrivial)
                    continue;

                int  triangleVariation = 0;
                bool mirroredVariation = false;

                // Create all possible versions of current animation, including
                // mirrored and rotated ones. Later on, when parsing actual TextureAreas
                // from faces, we will compare them with this "lookup table" and will be
                // able to quickly return desired variation ID without complicated in-place
                // calculations.

                while(true)
                {
                    var refAnim = new ParentAnimatedTexture(set);

                    foreach (var frame in set.Frames)
                    {
                        // Create base frame
                        TextureArea newFrame = new TextureArea()
                        {
                            Texture = frame.Texture,
                            TexCoord0 = frame.TexCoord0,
                            TexCoord1 = frame.TexCoord1,
                            TexCoord2 = frame.TexCoord2,
                            TexCoord3 = (triangleVariation > 0 ? frame.TexCoord2 : frame.TexCoord3)
                        };

                        // Mirror if needed
                        if (mirroredVariation)
                            newFrame.Mirror();

                        // Rotate if needed
                        if(triangleVariation > 0)
                            newFrame.Rotate(triangleVariation - 1, true);

                        // Make frame, including repeat versions
                        for (int i = 0; i < frame.Repeat; i++)
                            AddTexture(newFrame, refAnim.CompiledAnimation, false, true, set.IsUvRotate ? 1 : 0, true, set.IsUvRotate);
                    }

                    ReferenceAnimTextures.Add(refAnim);

                    triangleVariation++;
                    if (triangleVariation == 5)
                    {
                        if (mirroredVariation == false)
                        {
                            triangleVariation = 0;
                            mirroredVariation = true;
                        }
                        else
                            break;
                    }
                }
            }
        }

        private ImageC BuildTextureMap(ref List<ParentTextureArea> textures, int numPages, int padding, BumpLevel bumpLevel)
        {
            var image = ImageC.CreateNew(256, numPages * 256 * (bumpLevel != BumpLevel.None ? 2 : 1));
            for (int i = 0; i < textures.Count; i++)
            {
                var p = textures[i];
                var x = (int)p.Area.Start.X;
                var y = (int)p.Area.Start.Y;
                var width = (int)(p.Area.End.X - p.Area.Start.X + 1);
                var height = (int)(p.Area.End.Y - p.Area.Start.Y + 1);

                image.CopyFrom(p.PositionInPage.X + p.Padding, p.Page * 256 + p.PositionInPage.Y + p.Padding, p.Texture.Image,
                               x, y, width, height);

                // Do the bump map if needed
                if (bumpLevel != BumpLevel.None)
                {
                    var bumpImage = ImageC.CreateNew((int)p.Area.Width, (int)p.Area.Height);
                    bumpImage.CopyFrom(0, 0, image, p.PositionInPage.X + p.Padding, p.Page * 256 + p.PositionInPage.Y + p.Padding, (int)p.Area.Width, (int)p.Area.Height);
                    bumpImage.Emboss(0, 0, bumpImage.Width, bumpImage.Height, 1, 3);
                    image.CopyFrom(p.PositionInPage.X + p.Padding, (numPages + p.Page) * 256 + p.PositionInPage.Y + p.Padding, bumpImage);
                }

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

            // Eventually generate bump maps
            /*if (bumpLevel != BumpLevel.None)
            {
                var bumpImage = ImageC.CreateNew(256, numPages * 256);
                bumpImage.CopyFrom(0, 0, image);
                bumpImage.Emboss(0, 0, bumpImage.Width, bumpImage.Height, 1, 3);

                var finalImage = ImageC.CreateNew(256, numPages * 2 * 256);
                finalImage.CopyFrom(0, 0, image);
                finalImage.CopyFrom(0, numPages * 256, bumpImage);

                image = finalImage;
            }*/

            return image;
        }

        public void PackTextures()
        {
            int padding = 8;

            // Subdivide textures in 3 blocks: room, objects, bump
            var roomTextures = new List<ParentTextureArea>();
            var objectsTextures = new List<ParentTextureArea>();
            var bumpedTextures = new List<ParentTextureArea>();

            for (int i = 0; i < ParentTextures.Count; i++)
            {
                if (ParentTextures[i].IsForRoom)
                {
                    if (ParentTextures[i].BumpLevel == BumpLevel.None)
                        bumpedTextures.Add(ParentTextures[i]);
                    else
                        roomTextures.Add(ParentTextures[i]);
                }
                else
                    objectsTextures.Add(ParentTextures[i]);
            }

            // Calculate new X, Y of each texture area
            int numRoomPages = PlaceTexturesInMap(ref roomTextures, padding);
            int numObjectsPages = PlaceTexturesInMap(ref objectsTextures, padding);
            int numBumpedPages = PlaceTexturesInMap(ref bumpedTextures, padding);

            // Place all the textures areas in the maps
            var roomPages = BuildTextureMap(ref roomTextures, numRoomPages, padding, BumpLevel.None);
            var objectsPages = BuildTextureMap(ref objectsTextures, numObjectsPages, padding, BumpLevel.None);
            var bumpedPages = BuildTextureMap(ref bumpedTextures, numBumpedPages, padding, BumpLevel.Level2);

            // Combine all maps in the final map
            numBumpedPages *= 2;
            int numPages = numRoomPages + numObjectsPages + numBumpedPages;
            var finalImage = ImageC.CreateNew(256, numPages * 256);
            finalImage.CopyFrom(0, 0, roomPages);
            finalImage.CopyFrom(0, numRoomPages * 256, objectsPages);
            finalImage.CopyFrom(0, (numRoomPages + numObjectsPages) * 256, bumpedPages);

            // DEBUG: Now for testing create a bitmap
            //finalImage.Save("h:\\testpack.png");
        }
    }
}
