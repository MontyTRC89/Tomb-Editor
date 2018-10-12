using NLog;
using System;
using System.Collections.Generic;
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
        public  List<ParentAnimatedTexture> ActualAnimTextures { get; private set; } = new List<ParentAnimatedTexture>();

        // UVRotate count should be placed after anim texture data to identify how many first anim seqs
        // should be processed using UVRotate engine function

        public int UvRotateCount => ActualAnimTextures.Count(seq => seq.Origin.IsUvRotate);

        // List of parent textures should contain all "ancestor" texture areas in which all variations
        // are placed, including mirrored and rotated ones.

        private List<ParentTextureArea> ParentTextures = new List<ParentTextureArea>();

        // MaxTileSize defines maximum size to which parent can be inflated by incoming child, if
        // inflation is allowed.

        private ushort MaxTileSize = 256;

        // TexInfoCount is internally a "reference counter" which is also used to get new TexInfo IDs.
        // Since generation of TexInfos is an one-off serialized process, we can safely use it in
        // serial manner as well.

        public int TexInfoCount { get; private set; } = 0;

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
            public Vector2[] RelCoord;  // Relative to parent!
            public Vector2[] AbsCoord; // Absolute

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
            public int[] Padding { get; set; } = new int[4]; // LTRB

            public Texture Texture { get; private set; }
            public BumpLevel BumpLevel { get; set; }
            public bool IsForRoom { get; set; }

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
                            for (int i = 0; i < child.RelCoord.Length; i++)
                                child.RelCoord[i] += delta;
                    }

                    _area = value;
                }
            }
            public List<ChildTextureArea> Children;

            // Generates new ParentTextureArea from raw texture coordinates.
            public ParentTextureArea(TextureArea texture, bool isForRoom)
            {
                _area = texture.GetRect().Round(); // Round to nearest pixel to prevent rounding errors further down the line
                Initialize(texture.Texture, texture.BumpLevel, isForRoom);
            }

            public ParentTextureArea(Rectangle2 area, Texture texture, BumpLevel bumpLevel, bool isForRoom)
            {
                _area = area;
                Initialize(texture, bumpLevel, isForRoom);
            }

            private void Initialize(Texture texture, BumpLevel bumpLevel, bool isForRoom)
            {
                Children = new List<ChildTextureArea>();

                Texture = texture;
                BumpLevel = bumpLevel;
                IsForRoom = isForRoom;
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
                => (ParametersSimilar(texture, isForRoom) && texture.GetRect().Round().Contains(_area));

            // Adds texture as a child to existing parent, with recalculating coordinates to relative.
            public void AddChild(TextureArea texture, int newTextureID, bool isForTriangle)
            {
                var relative = new Vector2[isForTriangle ? 3 : 4];
                var absolute = new Vector2[isForTriangle ? 3 : 4];

                for (int i = 0; i < relative.Length; i++)
                {
                    absolute[i] = texture.GetTexCoord(i);
                    relative[i] = absolute[i] - Area.Start;
                }

                Children.Add(new ChildTextureArea()
                {
                    TexInfoIndex = newTextureID,
                    BlendMode = texture.BlendMode,
                    IsForTriangle = isForTriangle,
                    RelCoord = relative,
                    AbsCoord = absolute
                });

                // Expand parent area, if needed
                var rect = texture.GetRect();
                if (!Area.Contains(rect))
                    Area = Area.Union(rect).Round();
            }

            public void MoveChild(ChildTextureArea child, ParentTextureArea newParent)
            {
                var newCoordinates = child.AbsCoord;
                for (int i = 0; i < newCoordinates.Length; i++)
                    newCoordinates[i] -= newParent.Area.Start;

                newParent.Children.Add(new ChildTextureArea()
                {
                    TexInfoIndex = child.TexInfoIndex,
                    BlendMode = child.BlendMode,
                    IsForTriangle = child.IsForTriangle,
                    RelCoord = newCoordinates,
                    AbsCoord = child.AbsCoord
                });
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
                    var newParent = new ParentTextureArea(parent.Area, parent.Texture, parent.BumpLevel, parent.IsForRoom);
                    foreach(var child in parent.Children)
                    {
                        var newChild = new ChildTextureArea()
                        {
                            BlendMode = child.BlendMode,
                            IsForTriangle = child.IsForTriangle,
                            TexInfoIndex = child.TexInfoIndex
                        };

                        newChild.RelCoord = new Vector2[child.RelCoord.Length];
                        newChild.AbsCoord = new Vector2[child.AbsCoord.Length];

                        for (int i = 0; i < child.RelCoord.Length; i++)
                        {
                            newChild.RelCoord[i] = child.RelCoord[i];
                            newChild.AbsCoord[i] = child.AbsCoord[i];
                        }

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

            public ObjectTexture(ParentTextureArea parent, ChildTextureArea child, GameVersion version, float maxTextureSize)
            {
                BlendMode = child.BlendMode;
                BumpLevel = parent.BumpLevel;
                IsForRoom = parent.IsForRoom;
                IsForTriangle = child.IsForTriangle;
                Tile = parent.Page;

                for (int i = 0; i < child.RelCoord.Length; i++)
                {
                    var coord = new Vector2(child.RelCoord[i].X + (float)(parent.PositionInPage.X + parent.Padding[0]),
                                            child.RelCoord[i].Y + (float)(parent.PositionInPage.Y + parent.Padding[1]));

                    // Apply texture distortion as countermeasure for hardcoded TR4-5 mapping correction
                    if(version >= GameVersion.TR4)
                        coord -= IsForTriangle ? _distort3[i] : _distort4[i];

                    coord.X = (float)MathC.Clamp(coord.X, 0, maxTextureSize);
                    coord.Y = (float)MathC.Clamp(coord.Y, 0, maxTextureSize);

                    TexCoord[i] = new VectorInt2((((int)Math.Truncate(coord.X)) << 8) + (int)(Math.Floor(coord.X % 1.0f * 255.0f)),
                                                 (((int)Math.Truncate(coord.Y)) << 8) + (int)(Math.Floor(coord.Y % 1.0f * 255.0f)));
                }

                if (child.IsForTriangle)
                    TexCoord[3] = TexCoord[2];
            }

            public Rectangle2 GetRect(bool isTriangle)
            {
                if (isTriangle)
                    return Rectangle2.FromCoordinates(TexCoord[0], TexCoord[1], TexCoord[2]);
                else
                    return Rectangle2.FromCoordinates(TexCoord[0], TexCoord[1], TexCoord[2], TexCoord[3]);
            }
        }

        public TexInfoManager(ushort maxTileSize, List<AnimatedTextureSet> sets)
        {
            MaxTileSize = maxTileSize;
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

        public bool TryToAddToExisting(TextureArea texture, List<ParentTextureArea> parentList, bool isForRoom, bool isForTriangle, bool allowOverlaps = false)
        {
            // Try to find potential parent (larger texture) and add itself to children
            foreach (var parent in parentList)
            {
                if (!parent.IsPotentialParent(texture, isForRoom, allowOverlaps, MaxTileSize))
                    continue;

                parent.AddChild(texture, GetNewTexInfoIndex(), isForTriangle);
                return true;
            }

            // Try to find and merge parents which are enclosed in incoming texture area
            var childrenWannabes = parentList.Where(item => item.IsPotentialChild(texture, isForRoom)).ToList();
            if (childrenWannabes.Count > 0)
            {
                var newParent = new ParentTextureArea(texture, isForRoom);
                newParent.AddChild(texture, GetNewTexInfoIndex(), isForTriangle);
                newParent.MergeParents(parentList, childrenWannabes);
                parentList.Add(newParent);
                return true;
            }

            // No success
            return false;
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
                        transformedIndices[0] = transformedIndices[2];
                        transformedIndices[2] = transformedIndices[1];
                        transformedIndices[1] = tempIndex;
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
                        transformedIndices[0] = transformedIndices[3];
                        transformedIndices[3] = transformedIndices[2];
                        transformedIndices[2] = transformedIndices[1];
                        transformedIndices[1] = tempIndex;
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
                    if ((checkParameters && areaToLook.BlendMode != child.BlendMode) || child.IsForTriangle != isForTriangle)
                        continue;

                    // Test if coordinates are mutually equal and return resulting rotation if they are
                    var result = TestUVSimilarity(child.AbsCoord, lookupCoordinates, lookupMargin);
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
            // If first/second coordinates are not mutually quads/tris, quickly return NoTexInfo.
            // Also discard out of bounds cases without exception.
            if (first.Length == second.Length && first.Length >= 3 && first.Length <= 4)
            {
                for (int i = 0; i < first.Length; i++)
                    for (int j = 0; j < second.Length; j++)
                    {
                        var shift = (j + i) % second.Length;

                        if (!MathC.WithinEpsilon(first[j].X, second[shift].X, lookupMargin) ||
                            !MathC.WithinEpsilon(first[j].Y, second[shift].Y, lookupMargin))
                            break;

                        //Comparison was successful
                        if (j == second.Length - 1)
                            return i == 0 ? 0 : second.Length - i;
                    }
            }
            return NoTexInfo;
        }

        // Generate new parent with incoming texture and immediately add incoming texture as a child

        public void AddParent(TextureArea texture, List<ParentTextureArea> parentList, bool isForTriangle, bool isForRoom)
        {
            var newParent = new ParentTextureArea(texture, isForRoom);
            parentList.Add(newParent);
            newParent.AddChild(texture, GetNewTexInfoIndex(), isForTriangle);
        }

        // Only exposed variation of AddTexture that should be used outside of TexInfoManager itself
        
        public Result AddTexture(TextureArea texture, bool isForTriangle, bool isForRoom)
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
                            GenerateAnimTexture(refTex, texture, isForTriangle, isForRoom);
                            return AddTexture(texture, isForTriangle, isForRoom);
                        }
                    }
            }

            return AddTexture(texture, ParentTextures, isForTriangle, isForRoom);
        }

        // Internal AddTexture variation which is capable of adding texture to various ParentTextureArea lists
        // with customizable parameters.

        private Result AddTexture(TextureArea texture, List<ParentTextureArea> parentList, bool isForTriangle, bool isForRoom, bool packAnimations = false, bool makeCanonical = true)
        {
            // In case AddTexture is used with animated seq packing, we don't check frames for full similarity, because
            // frames can be dublicated with Repeat function or simply because of complex animator functions applied.
            var result = packAnimations ? null : GetTexInfo(texture, parentList, isForRoom, isForTriangle);

            if (!result.HasValue)
            {
                // Try to create new canonical (top-left-based) texture as child or parent.
                // makeCanonical parameter is necessary for animated textures, because animators may produce frames
                // with non-canonically rotated coordinates (e.g. spin animator).
                var canonicalTexture = makeCanonical ? texture.GetCanonicalTexture(isForTriangle) : texture;

                // If no any potential parents or children, create as new parent
                if (!TryToAddToExisting(canonicalTexture, parentList, isForRoom, isForTriangle, packAnimations))
                    AddParent(canonicalTexture, parentList, isForTriangle, isForRoom);

                // Try again to get texinfo
                if (packAnimations)
                    result = new Result { TexInfoIndex = DummyTexInfo, Rotation = 0 };
                else
                    result = GetTexInfo(texture, parentList, isForRoom, isForTriangle);
            }

            return result.Value;
        }

        private void SortOutAlpha(List<ParentTextureArea> parentList)
        {
            foreach (var parent in parentList)
            {
                var opaqueChildren = parent.Children.Where(child => child.BlendMode < BlendMode.Additive);
                if (opaqueChildren.Count() > 0 &&
                   parent.Texture.Image.HasAlpha((int)parent.Area.X0, (int)parent.Area.Y0, (int)parent.Area.Width, (int)parent.Area.Height))
                {
                    foreach (var children in opaqueChildren)
                        children.BlendMode = BlendMode.AlphaTest;
                }
            }
        }

        private int PlaceTexturesInMap(ref List<ParentTextureArea> textures, int padding)
        {
            if (textures.Count == 0)
                return 0;

            int currentPage = -1;
            List<RectPacker> texPackers = new List<RectPacker>();

            RectPacker packer = new RectPackerTree(new VectorInt2(256, 256));

            for (int i = 0; i < textures.Count; i++)
            {
                // Get the size of the quad surrounding texture area, typically should be the texture area itself
                int w = (int)(textures[i].Area.Width);
                int h = (int)(textures[i].Area.Height);

                // Calculate adaptive padding at all sides
                int lP = padding;
                int rP = padding;
                int tP = padding;
                int bP = padding;

                int horizontalPaddingSpace = MaxTileSize - w;
                int verticalPaddingSpace = MaxTileSize - h;

                // If hor/ver padding won't fully fit, get existing space and calculate padding out of it
                if (verticalPaddingSpace < padding * 2)
                {
                    tP = verticalPaddingSpace / 2;
                    bP = verticalPaddingSpace - tP;
                }
                if (horizontalPaddingSpace < padding * 2)
                {
                    lP = horizontalPaddingSpace / 2;
                    rP = horizontalPaddingSpace - lP;
                }

                w += lP + rP;
                h += tP + bP;

                // Pack texture
                int fitPage;
                VectorInt2? pos;

                for (ushort j = 0; j < currentPage; ++j)
                {
                    pos = texPackers[j].TryAdd(new VectorInt2(w, h));
                    if (pos.HasValue)
                    {
                        fitPage = j;
                        goto PackNextUsedTexture;
                    }
                }

                currentPage++;
                fitPage = currentPage;
                packer = new RectPackerTree(new VectorInt2(256, 256));
                pos = packer.TryAdd(new VectorInt2(w, h));
                texPackers.Add(packer);

                PackNextUsedTexture:
                    textures[i].Padding[0] = lP;
                    textures[i].Padding[1] = tP;
                    textures[i].Padding[2] = rP;
                    textures[i].Padding[3] = bP;
                    textures[i].Page = fitPage;
                    textures[i].PositionInPage = pos.Value;
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
                            AddTexture(newFrame, refAnim.CompiledAnimation, (triangleVariation > 0), true, true, set.IsUvRotate);
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

        private void GenerateAnimTexture(ParentAnimatedTexture reference, TextureArea origin, bool isForTriangle, bool isForRoom)
        {
            var refCopy = reference.Clone();
            foreach(var parent in refCopy.CompiledAnimation)
            {
                parent.BumpLevel = origin.BumpLevel;
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

                image.CopyFrom(p.PositionInPage.X + p.Padding[0], p.Page * 256 + p.PositionInPage.Y + p.Padding[1], p.Texture.Image,
                               x, y, width, height);
                AddPadding(p, p.Texture.Image, image, 0, padding);

                // Do the bump map if needed
                if (p.BumpLevel != BumpLevel.None)
                {
                    var bumpImage = ImageC.CreateNew(width, height);
                    bumpImage.CopyFrom(0, 0, image, p.PositionInPage.X + p.Padding[0], p.Page * 256 + p.PositionInPage.Y + p.Padding[1], (int)p.Area.Width, (int)p.Area.Height);

                    int effectSize = 0;

                    switch(p.BumpLevel)
                    {
                        case BumpLevel.Level1:
                            effectSize = 2;
                            break;
                        case BumpLevel.Level2:
                            effectSize = 3;
                            break;
                        case BumpLevel.Level3:
                            effectSize = 4;
                            break;
                    }
                    bumpImage.Emboss(0, 0, bumpImage.Width, bumpImage.Height, effectSize, -2);

                    var bumpX = p.PositionInPage.X + p.Padding[0];
                    var bumpY = (numPages + p.Page) * 256 + p.PositionInPage.Y + p.Padding[1];

                    image.CopyFrom(bumpX, bumpY, bumpImage);
                    AddPadding(p, image, image, numPages, padding, bumpX, bumpY);
                }
            }
            
            return image;
        }

        private void AddPadding(ParentTextureArea texture, ImageC from, ImageC to, int pageOffset, int padding, int? customX = null, int? customY = null)
        {
            var p = texture;
            var x = customX.HasValue ? customX.Value : (int)p.Area.Start.X;
            var y = customY.HasValue ? customY.Value : (int)p.Area.Start.Y;
            var width = (int)p.Area.Width;
            var height = (int)p.Area.Height;
            var dataOffset = (p.Page + pageOffset) * 256;

            // Add actual padding (ported code from OT bordered_texture_atlas.cpp)

            var topLeft = from.GetPixel(x, y);
            var topRight = from.GetPixel(x + width - 1, y);
            var bottomLeft = from.GetPixel(x, y + height - 1);
            var bottomRight = from.GetPixel(x + width - 1, y + height - 1);

            for (int xP = 0; xP < padding; xP++)
            {
                // copy left line
                if (xP < p.Padding[0])
                    to.CopyFrom(p.PositionInPage.X + xP, dataOffset + p.PositionInPage.Y + p.Padding[1], from,
                               x, y, 1, height);

                // copy right line
                if (xP < p.Padding[2])
                    to.CopyFrom(p.PositionInPage.X + xP + width + p.Padding[0], dataOffset + p.PositionInPage.Y + p.Padding[1], from,
                               x + width - 1, y, 1, height);

                for (int yP = 0; yP < padding; yP++)
                {
                    // copy top line
                    if (yP < p.Padding[1])
                        to.CopyFrom(p.PositionInPage.X + p.Padding[0], dataOffset + p.PositionInPage.Y + yP, from,
                                   x, y, width, 1);
                    // copy bottom line
                    if (yP < p.Padding[3])
                        to.CopyFrom(p.PositionInPage.X + p.Padding[0], dataOffset + p.PositionInPage.Y + yP + height + p.Padding[1], from,
                                   x, y + height - 1, width, 1);

                    // expand top-left pixel
                    if (xP < p.Padding[0] && yP < p.Padding[1])
                        to.SetPixel(p.PositionInPage.X + xP, dataOffset + p.PositionInPage.Y + yP, topLeft);
                    // expand top-right pixel
                    if (xP < p.Padding[2] && yP < p.Padding[1])
                        to.SetPixel(p.PositionInPage.X + xP + width + p.Padding[0], dataOffset + p.PositionInPage.Y + yP, topRight);
                    // expand bottom-left pixel
                    if (xP < p.Padding[0] && yP < p.Padding[3])
                        to.SetPixel(p.PositionInPage.X + xP, dataOffset + p.PositionInPage.Y + yP + height + p.Padding[1], bottomLeft);
                    // expand bottom-right pixel
                    if (xP < p.Padding[2] && yP < p.Padding[3])
                        to.SetPixel(p.PositionInPage.X + xP + width + p.Padding[0], dataOffset + p.PositionInPage.Y + yP + height + p.Padding[1], bottomRight);
                }
            }
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
            float maxSize = (float)MaxTileSize - (1.0f / 255.0f);

            _objectTextures = new SortedDictionary<int, ObjectTexture>();

            SortOutAlpha(ParentTextures);
            foreach (var parent in ParentTextures)
                foreach (var child in parent.Children)
                    if (!_objectTextures.ContainsKey(child.TexInfoIndex))
                        _objectTextures.Add(child.TexInfoIndex, new ObjectTexture(parent, child, version, maxSize));

            foreach (var animTexture in ActualAnimTextures)
            {
                SortOutAlpha(animTexture.CompiledAnimation);
                foreach (var parent in animTexture.CompiledAnimation)
                    foreach (var child in parent.Children)
                        if (!_objectTextures.ContainsKey(child.TexInfoIndex))
                            _objectTextures.Add(child.TexInfoIndex, new ObjectTexture(parent, child, version, maxSize));
            }
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
