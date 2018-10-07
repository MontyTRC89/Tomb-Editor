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

        private const int   NoTexInfo = -1;
        private const int   DummyTexInfo = -2;
        private const float AnimTextureLookupMargin = 5.0f;

        // Mapping correction compensation coordinate sets.
        // Used to counterbalance TR4/5 internal mapping correction applied in regard to NewFlags
        // value.

        private static readonly Vector2[] _distort4 = new Vector2[4] { new Vector2( 0.5f,  0.5f), new Vector2(-0.5f,  0.5f), new Vector2(-0.5f, -0.5f), new Vector2( 0.5f, -0.5f) };
        private static readonly Vector2[] _distort3 = new Vector2[3] { new Vector2( 0.5f,  0.5f), new Vector2(-0.5f,  0.5f), new Vector2( 0.5f, -0.5f)                            };
        
        // Defines if texinfo manager should actually start generating texinfo indexes.
        // Needed for anim lookup table generation.

        private bool GenerateTexInfos = false;

        // Two lists of animated textures contain reference animation versions for each sequence
        // and actual found animated texture sequences in rooms. When compiler encounters a tile
        // which is similar to one of the reference animation versions, it copies it into actual
        // animations list with all respective frames. Any new comparison is made with actual
        // animation sequences at first, so next found similar tile will refer to already existing
        // animation version.
        // On packing, list of actual anim textures is added to main ParentTextures list, so only
        // versions existing in level file are added to texinfo list. Rest from reference list
        // is ignored.

        private List<ParentAnimatedTexture> ReferenceAnimTextures = new List<ParentAnimatedTexture>();
        private List<ParentAnimatedTexture> ActualAnimTextures = new List<ParentAnimatedTexture>();

        // UVRotate count should be placed after anim texture data to identify how many first anim seqs
        // should be processed using UVRotate engine function

        public int UvRotateCount => ActualAnimTextures.Count(seq => seq.Origin.IsUvRotate);

        // List of parent textures should contain all "ancestor" texture areas in which all variations
        // are placed, including mirrored and rotated ones.

        private List<ParentTextureArea> ParentTextures = new List<ParentTextureArea>();

        // MaxParentSize defines maximum size to which parent can be inflated by incoming child, if
        // inflation is allowed.

        private uint MaxParentSize = 256;

        // TexInfoCount is internally a "reference counter" which is also used to get new TexInfo IDs.
        // Since generation of TexInfos is an one-off serialized process, we can safely use it in
        // serial manner as well.

        public int TexInfoCount { get; private set; } = 0;

        // Simple helper counters for final data structures, used in progress reporter

        public int ParentCount => ParentTextures.Count;
        public int ActualAnimTexturesCount => ActualAnimTextures.Count;
        public int ReferenceAnimTexturesCount => ReferenceAnimTextures.Count;

        // Final texture pages and its counters

        public int NumRoomPages { get; private set; }
        public ImageC RoomPages { get; private set; }

        public int NumObjectsPages { get; private set; }
        public ImageC ObjectsPages { get; private set; }

        public int NumBumpPages { get; private set; }
        public ImageC BumpPages { get; private set; }

        // Precompiled object textures are kept in this dictionary.

        private SortedDictionary<int, ObjectTexture> _objectTextures;

        // ChildTextureArea is a simple enclosed relative texture area with stripped down parameters
        // which should be the same among all children of same parent.
        // Stripped down parameters include BumpLevel and IsForTriangle, because if these are
        // different, it automatically means we should assign new parent for this child.

        public class ChildTextureArea
        {
            public int TexInfoIndex;
            public Vector2[] TexCoord;  // Relative to parent!

            public BlendMode BlendMode;
            public bool IsForTriangle;
        }

        // ParentTextureArea is a texture area which contains all other texture areas which are
        // completely inside current one. Bumpmapping parameter define that parent is different,
        // hence two TextureAreas with same UV coordinates but with different BumpLevel will be 
        // saved as different parents.

        public class ParentTextureArea
        {
            public VectorInt2 PositionInPage { get; set; }
            public int Page { get; set; }
            public int Padding { get; set; }

            public Texture Texture { get; private set; }
            public BumpLevel BumpLevel { get; set; }
            public bool IsForRoom { get; set; }

            private int _packPriority;
            public int PackPriority
            {
                get { return _packPriority; }
                set { _packPriority = Math.Max(value, _packPriority); }
            }

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

                // Expand to nearest pixel to prevent rounding errors further down the line
                _area.Start = new Vector2((float)Math.Floor(_area.Start.X), (float)Math.Floor(_area.Start.Y));
                _area.End = new Vector2((float)Math.Ceiling(_area.End.X), (float)Math.Ceiling(_area.End.Y));

                Initialize(texture.Texture, texture.BumpLevel, isForRoom, packPriority);
            }

            public ParentTextureArea(Rectangle2 area, Texture texture, BumpLevel bumpLevel, bool isForRoom, int packPriority)
            {
                _area = area;
                Initialize(texture, bumpLevel, isForRoom, packPriority);
            }

            private void Initialize(Texture texture, BumpLevel bumpLevel, bool isForRoom, int packPriority)
            {
                Children = new List<ChildTextureArea>();

                Texture = texture;
                BumpLevel = bumpLevel;
                IsForRoom = isForRoom;
                PackPriority = packPriority;
            }

            // Compare parent's properties with incoming texture properties.
            public bool ParametersSimilar(TextureArea incomingTexture, bool isForRoom)
                => BumpLevel == incomingTexture.BumpLevel && Texture.Image == incomingTexture.Texture.Image && IsForRoom == isForRoom;

            // Checks if parameters are similar to another texture area, and if so,
            // also checks if texture area is enclosed in parent's area.
            public bool IsPotentialParent(TextureArea texture, bool isForRoom, bool allowOverlaps = false, uint maxOverlappedSize = 256)
            {
                var rect = texture.GetRect();

                if (ParametersSimilar(texture, isForRoom))
                {
                    if (_area.Contains(rect))
                        return true;
                    else if (allowOverlaps)
                    {
                        if (!_area.Contains(rect) && rect.Intersects(_area))
                        {
                            var potentialNewArea = rect.Union(_area);
                            return ((potentialNewArea.Width <= maxOverlappedSize) &&
                                    (potentialNewArea.Height <= maxOverlappedSize));
                        }
                    }
                }
                return false;
            }

            // Checks if incoming texture is similar in parameters and encloses parent area.
            public bool IsPotentialChild(TextureArea texture, bool isForRoom)
                => (ParametersSimilar(texture, isForRoom) && texture.GetRect().Contains(_area));

            // Adds texture as a child to existing parent, with recalculating coordinates to relative.
            public void AddChild(TextureArea texture, int newTextureID, bool isForTriangle)
            {
                var result = new Vector2[isForTriangle ? 3 : 4];
                for (int i = 0; i < result.Length; i++)
                    result[i] = texture.GetTexCoord(i) - Area.Start;

                Children.Add(new ChildTextureArea()
                {
                    TexInfoIndex = newTextureID,
                    BlendMode = texture.GetRealBlendMode(),
                    IsForTriangle = isForTriangle,
                    TexCoord = result
                });

                // Expand parent area, if needed
                var rect = texture.GetRect();
                if (!Area.Contains(rect))
                    Area = Area.Union(rect);
            }

            public void MoveChild(ChildTextureArea child, ParentTextureArea newParent)
            {
                var newCoordinates = GetAbsChildCoordinates(child);
                for (int i = 0; i < newCoordinates.Length; i++)
                    newCoordinates[i] -= newParent.Area.Start;

                newParent.Children.Add(new ChildTextureArea()
                {
                    TexInfoIndex = child.TexInfoIndex,
                    BlendMode = child.BlendMode,
                    IsForTriangle = child.IsForTriangle,
                    TexCoord = newCoordinates
                });

            }

            // Returns absolute child coordinates
            public Vector2[] GetAbsChildCoordinates(ChildTextureArea child)
            {
                var result = new Vector2[child.TexCoord.Length];
                for (int i = 0; i < child.TexCoord.Length; i++)
                    result[i] = Area.Start + child.TexCoord[i];
                return result;
            }

            public void MergeParents(List<ParentTextureArea> parentList, List<ParentTextureArea> parents)
            {
                foreach (var parent in parents)
                {
                    Area = Area.Union(parent.Area);

                    foreach (var child in parent.Children)
                        parent.MoveChild(child, this);

                    parent.Children.Clear();
                }

                parents.ForEach(item => parentList.Remove(item));
            }
        }

        // ParentAnimatedTexture contains all precompiled frames of a specific anim texture set.
        // Since animated textures can overlap (especially with animators), we store list of frames
        // as a list of parents. Children and parents are added in sequential order, so no sorting
        // must be made on CompiledAnimation.

        public class ParentAnimatedTexture : ICloneable
        {
            public ParentAnimatedTexture Clone()
            {
                var result = new ParentAnimatedTexture(Origin);
                result.CompiledAnimation = new List<ParentTextureArea>();

                foreach(var parent in CompiledAnimation)
                {
                    var newParent = new ParentTextureArea(parent.Area, parent.Texture, parent.BumpLevel, parent.IsForRoom, parent.PackPriority);
                    foreach(var child in parent.Children)
                    {
                        var newChild = new ChildTextureArea()
                        {
                            BlendMode = child.BlendMode,
                            IsForTriangle = child.IsForTriangle,
                            TexInfoIndex = child.TexInfoIndex
                        };

                        newChild.TexCoord = new Vector2[child.TexCoord.Length];
                        for (int i = 0; i < child.TexCoord.Length; i++)
                            newChild.TexCoord[i] = child.TexCoord[i];

                        newParent.Children.Add(newChild);
                    }
                    result.CompiledAnimation.Add(newParent);
                }
                return result;
            }
            object ICloneable.Clone() => Clone();

            public AnimatedTextureSet Origin;
            public List<ParentTextureArea> CompiledAnimation = new List<ParentTextureArea>();

            public ParentAnimatedTexture(AnimatedTextureSet origin) 
            {
                Origin = origin;
            }

            public int FrameCount()
            {
                int result = 0;
                foreach(var parent in CompiledAnimation)
                    result += parent.Children.Count;
                return result;
            }
        }

        private class ObjectTexture
        {
            public int Tile;
            public VectorInt2[] TexCoord = new VectorInt2[4];

            public bool IsForTriangle;
            public bool IsForRoom;
            public BlendMode BlendMode;
            public BumpLevel BumpLevel;

            public ObjectTexture(ParentTextureArea parent, ChildTextureArea child, GameVersion version)
            {
                BlendMode = child.BlendMode;
                BumpLevel = parent.BumpLevel;
                IsForRoom = parent.IsForRoom;
                IsForTriangle = child.IsForTriangle;
                Tile = parent.Page;

                var coords = child.TexCoord;

                for (int i = 0; i < coords.Length; i++)
                {
                    coords[i].X += (float)(parent.PositionInPage.X + parent.Padding);
                    coords[i].Y += (float)(parent.PositionInPage.Y + parent.Padding);

                    // Apply texture distortion as countermeasure for hardcoded TR4-5 mapping correction
                    if(version >= GameVersion.TR4)
                        coords[i] -= IsForTriangle ? _distort3[i] : _distort4[i];
                }

                for (int i = 0; i < coords.Length; i++)
                    TexCoord[i] = new VectorInt2(((int)Math.Floor(coords[i].X) << 8) + (int)(Math.Ceiling(coords[i].X % 1.0f * 255.0f)),
                                                 ((int)Math.Floor(coords[i].Y) << 8) + (int)(Math.Ceiling(coords[i].Y % 1.0f * 255.0f)));

            }

            public Rectangle2 GetRect(bool isTriangle)
            {
                if (isTriangle)
                    return Rectangle2.FromCoordinates(TexCoord[0], TexCoord[1], TexCoord[2]);
                else
                    return Rectangle2.FromCoordinates(TexCoord[0], TexCoord[1], TexCoord[2], TexCoord[3]);
            }
        }

        public TexInfoManager(uint maxParentSize, List<AnimatedTextureSet> sets)
        {
            MaxParentSize = maxParentSize;
            GenerateAnimLookups(sets);  // Generate anim texture lookup table
            GenerateTexInfos = true;    // Set manager ready state 
        }

        // Gets free TexInfo index
        private int GetNewTexInfoIndex()
        {
            if (GenerateTexInfos)
            {
                int result = TexInfoCount;
                TexInfoCount++;
                return result;
            }
            else
                return DummyTexInfo;
        }

        // Try to add texture to existing parent(s) either as a child of one, or as a parent, merging
        // enclosed parents.

        public int TryToAddToExisting(TextureArea texture, List<ParentTextureArea> parentList, bool isForRoom, bool isForTriangle, int packPriority, bool allowOverlaps = false)
        {
            // Try to find potential parent (larger texture) and add itself to children
            foreach (var parent in parentList)
            {
                if (!parent.IsPotentialParent(texture, isForRoom, allowOverlaps, MaxParentSize))
                    continue;

                var newTexIndex = GetNewTexInfoIndex();
                parent.AddChild(texture, newTexIndex, isForTriangle);
                parent.PackPriority = packPriority;
                return newTexIndex;
            }

            // Try to find and merge parents which are enclosed in incoming texture area
            var childrenWannabes = parentList.Where(item => item.IsPotentialChild(texture, isForRoom)).ToList();
            if (childrenWannabes.Count > 0)
            {
                var newParent = new ParentTextureArea(texture, isForRoom, packPriority);
                var texIndex = GetNewTexInfoIndex();
                newParent.AddChild(texture, texIndex, isForTriangle);
                newParent.MergeParents(parentList, childrenWannabes);
                parentList.Add(newParent);
                return texIndex;
            }

            // No success
            return NoTexInfo;
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

                ushort objectTextureIndex = (ushort)(TexInfoIndex | (doubleSided ? 0x8000 : 0));
                ushort[] transformedIndices = new ushort[3] { indices[0], indices[1], indices[2] };

                if (Rotation > 0)
                {
                    for (int i = 0; i < Rotation; i++)
                    {
                        ushort tempIndex = transformedIndices[0];
                        transformedIndices[0] = transformedIndices[1];
                        transformedIndices[1] = transformedIndices[2];
                        transformedIndices[2] = tempIndex;
                    }
                }

                return new tr_face3 { Vertices = new ushort[3] { transformedIndices[0], transformedIndices[1], transformedIndices[2] }, Texture = objectTextureIndex, LightingEffect = lightingEffect };
            }

            public tr_face4 CreateFace4(ushort[] indices, bool doubleSided, ushort lightingEffect)
            {
                if (indices.Length != 4)
                    throw new ArgumentOutOfRangeException(nameof(indices.Length));

                ushort objectTextureIndex = (ushort)(TexInfoIndex | (doubleSided ? 0x8000 : 0));
                ushort[] transformedIndices = new ushort[4] { indices[0], indices[1], indices[2], indices[3] };

                if (Rotation > 0)
                {
                    for (int i = 0; i < Rotation; i++)
                    {
                        ushort tempIndex = transformedIndices[0];
                        transformedIndices[0] = transformedIndices[1];
                        transformedIndices[1] = transformedIndices[2];
                        transformedIndices[2] = transformedIndices[3];
                        transformedIndices[3] = tempIndex;
                    }
                }

                return new tr_face4 { Vertices = new ushort[4] { transformedIndices[0], transformedIndices[1], transformedIndices[2], transformedIndices[3] }, Texture = objectTextureIndex, LightingEffect = lightingEffect };
            }
        }

        // Gets existing TexInfo child index if there is similar one in parent textures list.

        private Result? GetTexInfo(TextureArea areaToLook, List<ParentTextureArea> parentList, bool isForRoom, bool isForTriangle, bool checkParameters = true, float lookupMargin = 0.0f)
        {
            foreach (var parent in parentList)
            {
                // Parents with different attributes are quickly discarded
                if (checkParameters && !parent.ParametersSimilar(areaToLook, isForRoom))
                    continue;

                var lookupCoordinates = new Vector2[isForTriangle ? 3 : 4];
                for (int i = 0; i < lookupCoordinates.Length; i++)
                    lookupCoordinates[i] = areaToLook.GetTexCoord(i);

                // Extract each children's absolute coordinates and compare them to incoming texture coordinates.
                foreach (var child in parent.Children)
                {
                    // If parameters are different, children is quickly discarded from comparison.
                    if ((checkParameters && !areaToLook.SameBlendMode(child.BlendMode)) || child.IsForTriangle != isForTriangle)
                        continue;

                    // Test if coordinates are mutually equal and return resulting rotation if they are
                    var result = TestUVSimilarity(parent.GetAbsChildCoordinates(child), lookupCoordinates, lookupMargin);
                    if (result != NoTexInfo)
                    {
                        // Child is rotation-wise equal to incoming area
                        return new Result() { TexInfoIndex = child.TexInfoIndex, Rotation = (byte)result };
                    }
                }
            }

            return null; // No equal entry, new should be created
        }

        // Tests if all UV coordinates are similar with different rotations.
        // If all coordinates are equal for one of the rotation factors, rotation factor is returned,
        // otherwise NoTexInfo is returned (not similar). If coordinates are 100% equal, 0 is returned.

        private int TestUVSimilarity(Vector2[] first, Vector2[] second, float lookupMargin)
        {
            int Result = NoTexInfo; // Not similar

            // If first/second coordinates are not mutually quads/tris, quickly return NoTexInfo.
            // Also discard out of bounds cases without exception.
            if (first.Length == second.Length && first.Length >= 3 && first.Length <= 4)
            {
                int baseCoord = -1;  // Used to keep first-found similar coordinate

                // Now scroll through all texture coords and find one which is similar to 
                // first one in current texture
                for (byte i = 0; i < first.Length; i++)
                {
                    if (MathC.WithinEpsilon(first[0].X, second[i].X, lookupMargin) &&
                        MathC.WithinEpsilon(first[0].Y, second[i].Y, lookupMargin))
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
                        var nextCoord = ((i + baseCoord) % first.Length);

                        // Shifted coord is different, discard further comparison
                        if (!MathC.WithinEpsilon(first[i].X, second[nextCoord].X, lookupMargin) ||
                            !MathC.WithinEpsilon(first[i].Y, second[nextCoord].Y, lookupMargin))

                            if (first[i] != second[nextCoord])
                            break;

                        // Last coord reached, comparison succeeded
                        if (i == first.Length - 1)
                            Result = baseCoord;
                    }
                }
            }
            return Result;
        }

        // Generate new parent with incoming texture and immediately add incoming texture as a child

        public int AddParent(TextureArea texture, List<ParentTextureArea> parentList, bool isForTriangle, bool isForRoom, int packPriority)
        {
            var newParent = new ParentTextureArea(texture, isForRoom, packPriority);
            var texInfoIndex = GetNewTexInfoIndex();
            newParent.AddChild(texture, texInfoIndex, isForTriangle);
            parentList.Add(newParent);
            return texInfoIndex;
        }

        // Only exposed variation of AddTexture that should be used outside of TexInfoManager itself
        
        public Result AddTexture(TextureArea texture, bool isForTriangle, bool isForRoom, int packPriority = 0)
        {
            if(isForRoom)
            {
                // Try to compare incoming texture with existing anims and return animation frame
                if (ActualAnimTextures.Count > 0)
                    foreach (var actualTex in ActualAnimTextures)
                    {
                        var existing = GetTexInfo(texture, actualTex.CompiledAnimation, isForRoom, isForTriangle, true, AnimTextureLookupMargin);
                        if (existing.HasValue)
                            return existing.Value;
                    }

                // Now try to compare incoming texture with lookup anim seq table
                if (ReferenceAnimTextures.Count > 0)
                    foreach (var refTex in ReferenceAnimTextures)
                    {
                        // If reference set found, generate actual one and immediately return fresh result
                        if (GetTexInfo(texture, refTex.CompiledAnimation, isForRoom, isForTriangle, false, AnimTextureLookupMargin).HasValue)
                        {
                            GenerateAnimTexture(refTex, texture, isForTriangle, isForRoom, packPriority);
                            return AddTexture(texture, isForTriangle, isForRoom, packPriority);
                        }
                    }
            }

            return AddTexture(texture, ParentTextures, isForTriangle, isForRoom, packPriority);
        }

        // Internal AddTexture variation which is capable of adding texture to various ParentTextureArea lists
        // with customizable parameters.

        private Result AddTexture(TextureArea texture, List<ParentTextureArea> parentList, bool isForTriangle, bool isForRoom, int packPriority = 0, bool packAnimations = false, bool makeCanonical = false)
        {
            // In case AddTexture is used with animated seq packing, we don't check frames for full similarity, because
            // frames can be dublicated with Repeat function or simply because of complex animator functions applied.
            var result = packAnimations ? null : GetTexInfo(texture, parentList, isForRoom, isForTriangle);

            if (!result.HasValue)
            {
                byte rotation = 0;

                // Try to create new canonical (top-left-based) texture as child or parent.
                // makeCanonical parameter is necessary for UVRotate animations, because in case texture is not top-left-based,
                // engine will incorrectly calculate texture split for UV panning.
                var canonicalTexture = makeCanonical ? texture.GetCanonicalTexture(isForTriangle, out rotation) : texture;
                var texInfoIndex = TryToAddToExisting(canonicalTexture, parentList, isForRoom, isForTriangle, packPriority, packAnimations);

                // No any potential parents or children, create as new parent
                if (texInfoIndex == NoTexInfo)
                    texInfoIndex = AddParent(canonicalTexture, parentList, isForTriangle, isForRoom, packPriority);

                result = new Result() { TexInfoIndex = texInfoIndex, Rotation = rotation };
            }

            return result.Value;
        }

        private int PlaceTexturesInMap(ref List<ParentTextureArea> textures, int padding)
        {
            int currentPage = 0;
            RectPacker packer = new RectPackerTree(new VectorInt2(256, 256));

            for (int i = 0; i < textures.Count; i++)
            {
                // Get the size of the quad surrounding texture area, typically should be the texture area itself
                int w = (int)(textures[i].Area.Width);
                int h = (int)(textures[i].Area.Height);

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

        private void GenerateAnimLookups(List<AnimatedTextureSet> sets)
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

                    foreach(var frame in set.Frames)
                    {
                        // Create base frame
                        TextureArea newFrame = new TextureArea() { Texture = frame.Texture };

                        // Rotate or cut 4nd coordinate if needed
                        switch(triangleVariation)
                        {
                            case 0:
                                newFrame.TexCoord0 = frame.TexCoord0;
                                newFrame.TexCoord1 = frame.TexCoord1;
                                newFrame.TexCoord2 = frame.TexCoord2;
                                newFrame.TexCoord3 = frame.TexCoord3;
                                break;
                            case 1:
                                newFrame.TexCoord0 = frame.TexCoord0;
                                newFrame.TexCoord1 = frame.TexCoord1;
                                newFrame.TexCoord2 = frame.TexCoord2;
                                break;
                            case 2:
                                newFrame.TexCoord0 = frame.TexCoord0;
                                newFrame.TexCoord1 = frame.TexCoord2;
                                newFrame.TexCoord2 = frame.TexCoord3;
                                break;
                            case 3:
                                newFrame.TexCoord0 = frame.TexCoord0;
                                newFrame.TexCoord1 = frame.TexCoord1;
                                newFrame.TexCoord2 = frame.TexCoord3;
                                break;
                            case 4:
                                newFrame.TexCoord0 = frame.TexCoord1;
                                newFrame.TexCoord1 = frame.TexCoord2;
                                newFrame.TexCoord2 = frame.TexCoord3;
                                break;
                        }
                        if(triangleVariation > 0)
                            newFrame.TexCoord3 = newFrame.TexCoord2;

                        // Mirror if needed
                        if (mirroredVariation)
                            newFrame.Mirror();

                        // Make frame, including repeat versions
                        for (int i = 0; i < frame.Repeat; i++)
                            AddTexture(newFrame, refAnim.CompiledAnimation, (triangleVariation > 0), true, set.IsUvRotate ? 1 : 0, true, set.IsUvRotate);
                    }

                    ReferenceAnimTextures.Add(refAnim);

                    triangleVariation++;
                    if (triangleVariation > 4)
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

        private void GenerateAnimTexture(ParentAnimatedTexture reference, TextureArea origin, bool isForTriangle, bool isForRoom, int packPriority)
        {
            var refCopy = reference.Clone();
            foreach(var parent in refCopy.CompiledAnimation)
            {
                parent.BumpLevel = origin.BumpLevel;
                parent.PackPriority = packPriority;
                parent.IsForRoom = isForRoom;

                foreach(var child in parent.Children)
                {
                    child.BlendMode = origin.BlendMode;
                    child.IsForTriangle = isForTriangle;
                    child.TexInfoIndex = GetNewTexInfoIndex();
                }
            }
            ActualAnimTextures.Add(refCopy);
        }

        private ImageC BuildTextureMap(ref List<ParentTextureArea> textures, int numPages, int padding, bool bump)
        {
            var image = ImageC.CreateNew(256, numPages * 256 * (bump ? 2 : 1));
            for (int i = 0; i < textures.Count; i++)
            {
                var p = textures[i];
                var x = (int)p.Area.Start.X;
                var y = (int)p.Area.Start.Y;
                var width = (int)p.Area.Width;
                var height = (int)p.Area.Height;

                image.CopyFrom(p.PositionInPage.X + p.Padding, p.Page * 256 + p.PositionInPage.Y + p.Padding, p.Texture.Image,
                               x, y, width, height);

                // Do the bump map if needed
                if (p.BumpLevel != BumpLevel.None)
                {
                    var bumpImage = ImageC.CreateNew(width, height);
                    bumpImage.CopyFrom(0, 0, image, p.PositionInPage.X + p.Padding, p.Page * 256 + p.PositionInPage.Y + p.Padding, (int)p.Area.Width, (int)p.Area.Height);
                    if (p.BumpLevel == BumpLevel.Level1)
                        bumpImage.Emboss(0, 0, bumpImage.Width, bumpImage.Height, 2, -2);
                    else
                        bumpImage.Emboss(0, 0, bumpImage.Width, bumpImage.Height, 2, -3);
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
            
            return image;
        }

        public void PackTextures(int padding = 8)
        {
            // Subdivide textures in 3 blocks: room, objects, bump
            var roomTextures = new List<ParentTextureArea>();
            var objectsTextures = new List<ParentTextureArea>();
            var bumpedTextures = new List<ParentTextureArea>();

            for (int i = 0; i < ParentTextures.Count; i++)
            {
                if (ParentTextures[i].IsForRoom)
                {
                    if (ParentTextures[i].BumpLevel != BumpLevel.None)
                        bumpedTextures.Add(ParentTextures[i]);
                    else
                        roomTextures.Add(ParentTextures[i]);
                }
                else
                    objectsTextures.Add(ParentTextures[i]);
            }

            for (int n = 0; n < ActualAnimTextures.Count; n++)
            {
                var parentTextures = ActualAnimTextures[n].CompiledAnimation;
                for (int i = 0; i < parentTextures.Count; i++)
                {
                    if (parentTextures[i].IsForRoom)
                    {
                        if (parentTextures[i].BumpLevel != BumpLevel.None)
                            bumpedTextures.Add(parentTextures[i]);
                        else
                            roomTextures.Add(parentTextures[i]);
                    }
                    else
                        objectsTextures.Add(parentTextures[i]);
                }
            }

            // Calculate new X, Y of each texture area
            NumRoomPages    = PlaceTexturesInMap(ref roomTextures, padding);
            NumObjectsPages = PlaceTexturesInMap(ref objectsTextures, padding);
            NumBumpPages    = PlaceTexturesInMap(ref bumpedTextures, padding);  

            // Place all the textures areas in the maps
            RoomPages    = BuildTextureMap(ref roomTextures, NumRoomPages, padding, false);
            ObjectsPages = BuildTextureMap(ref objectsTextures, NumObjectsPages, padding, false);
            BumpPages    = BuildTextureMap(ref bumpedTextures, NumBumpPages, padding, true);

            // NumBumpPages is doubled with bumpmap pages themselves
            NumBumpPages *= 2;

            // DEBUG: Combine all maps in the final map
            /*int numPages = numRoomPages + numObjectsPages + numBumpedPages * 2;
            var finalImage = ImageC.CreateNew(256, numPages * 256);
            finalImage.CopyFrom(0, 0, _roomPages);
            finalImage.CopyFrom(0, numRoomPages * 256, _objectsPages);
            finalImage.CopyFrom(0, (numRoomPages + numObjectsPages) * 256, _bumpPages);
            finalImage.Save("h:\\testpack.png");*/
        }

        public void BuildTextureInfos(GameVersion version)
        {
            _objectTextures = new SortedDictionary<int, ObjectTexture>();

            foreach (var parent in ParentTextures)
                foreach (var child in parent.Children)
                    if (!_objectTextures.ContainsKey(child.TexInfoIndex))
                        _objectTextures.Add(child.TexInfoIndex, new ObjectTexture(parent, child, version));

            foreach (var animTexture in ActualAnimTextures)
                foreach (var parent in animTexture.CompiledAnimation)
                    foreach (var child in parent.Children)
                        if (!_objectTextures.ContainsKey(child.TexInfoIndex))
                            _objectTextures.Add(child.TexInfoIndex, new ObjectTexture(parent, child, version));
        }

        public void WriteAnimatedTextures(BinaryWriterEx writer)
        {
            // Put UVRotate sequences first
            var SortedAnimTextures = ActualAnimTextures.OrderBy(item => !item.Origin.IsUvRotate).ToList();

            int numAnimatedTextures = 1;
            foreach (var compiledAnimatedTexture in SortedAnimTextures)
                numAnimatedTextures += compiledAnimatedTexture.FrameCount() + 1;
            writer.Write((uint)numAnimatedTextures);

            writer.Write((ushort)SortedAnimTextures.Count);
            foreach (var compiledAnimatedTexture in SortedAnimTextures)
            {
                writer.Write((ushort)(compiledAnimatedTexture.FrameCount() - 1));
                foreach (var parent in compiledAnimatedTexture.CompiledAnimation)
                    foreach(var child in parent.Children)
                        writer.Write((ushort)child.TexInfoIndex);
            }
        }

        public void WriteTextureInfos(BinaryWriterEx writer, Level level)
        {
            writer.Write((int)_objectTextures.Count);
            for (int i = 0; i < _objectTextures.Count; i++)
            {
                var texture = _objectTextures.ElementAt(i).Value;

                // Tile and flags
                ushort tile = (ushort)texture.Tile;
                if (texture.IsForTriangle && level.Settings.GameVersion > GameVersion.TR2) tile |= 0x8000;

                // Blend mode
                ushort attribute = (ushort)texture.BlendMode;

                // Now write the texture
                writer.Write(attribute);
                writer.Write(tile);

                // New flags from >= TR4
                if (level.Settings.GameVersion >= GameVersion.TR4)
                {
                    // Built-in TR4-5 mapping correction is not used. Dummy mapping type 0 is used
                    // together with compensation coordinate distortion.
                    ushort newFlags = 0;

                    if (texture.IsForRoom) newFlags |= 0x8000;

                         if (texture.BumpLevel == BumpLevel.Level1) newFlags |= (1 << 11);
                    else if (texture.BumpLevel == BumpLevel.Level2) newFlags |= (2 << 11);
                    else if (texture.BumpLevel == BumpLevel.Level3) newFlags |= (3 << 11);

                    writer.Write(newFlags);
                }

                for (int j = 0; j < 4; j++)
                {
                    if (texture.IsForTriangle && j == 3)
                    {
                        writer.Write((ushort)0);
                        writer.Write((ushort)0);
                    }
                    else
                    {
                        writer.Write((ushort)(texture.TexCoord[j].X));
                        writer.Write((ushort)(texture.TexCoord[j].Y));
                    }
                }

                if (level.Settings.GameVersion >= GameVersion.TR4)
                {
                    var rect = texture.GetRect(texture.IsForTriangle);
                    writer.Write((int)0);
                    writer.Write((int)0);
                    writer.Write(rect.Width - 1);
                    writer.Write(rect.Height - 1);
                }

                if (level.Settings.GameVersion == GameVersion.TR5 || level.Settings.GameVersion == GameVersion.TR5Main)
                    writer.Write((ushort)0);
            }
        }

        public void UpdateTiles(int numSpritesPages)
        {
            for (int i = 0; i < _objectTextures.Count; i++)
            {
                var texture = _objectTextures.ElementAt(i).Value;
                if (texture.IsForRoom && texture.BumpLevel== BumpLevel.None)
                {
                    // Tile is OK
                }
                else if (!texture.IsForRoom)
                {
                    texture.Tile += NumRoomPages;
                }
                else if (texture.IsForRoom && texture.BumpLevel != BumpLevel.None)
                {
                    texture.Tile += NumRoomPages + NumObjectsPages + numSpritesPages;
                }
            }
        }
    }
}
