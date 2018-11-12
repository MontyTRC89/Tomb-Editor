using NLog;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Numerics;
using System.Threading.Tasks;
using TombLib.IO;
using TombLib.Utils;

namespace TombLib.LevelData.Compilers.Util
{
    public class TexInfoManager
    {
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();

        private const int NoTexInfo = -1;
        private const int DummyTexInfo = -2;
        private const int MinimumPadding = 1;
        private const float AnimTextureLookupMargin = 5.0f;

        // We need to keep level reference for padding and bumpmap references.

        protected readonly Level _level;
        protected readonly IProgressReporter _progressReporter;

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
        public List<ParentAnimatedTexture> ActualAnimTextures { get; private set; } = new List<ParentAnimatedTexture>();

        // UVRotate count should be placed after anim texture data to identify how many first anim seqs
        // should be processed using UVRotate engine function

        public int UvRotateCount => ActualAnimTextures.Count(seq => seq.Origin.IsUvRotate);

        // List of parent textures should contain all "ancestor" texture areas in which all variations
        // are placed, including mirrored and rotated ones.

        private List<ParentTextureArea> ParentTextures = new List<ParentTextureArea>();

        // MaxTileSize defines maximum size to which parent can be inflated by incoming child, if
        // inflation is allowed.

        private ushort MaxTileSize = 256;

        // If padding value is 0, 1 px padding will be forced on object textures anyway,
        // because yet we don't have a mechanism to specify UV adjustment in converted WADs.

        private ushort Padding = 8;

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
            public bool SqueezeAndDuplicate { get; set; } // Needed for UVRotate

            public Texture Texture { get; private set; }
            public bool IsForRoom { get; set; }

            // Waterfall textures need to stay on top of texture page without
            // padding, because of extremely ugly Core Design waterfall UVRotate hack.
            private bool _topmostAndUnpadded;
            public bool TopmostAndUnpadded
            {
                get { return _topmostAndUnpadded; }
                set { if (value) _topmostAndUnpadded = value; }
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
                // Round area to nearest pixel to prevent rounding errors further down the line.
                // Use ParentArea to create a parent for textures which were applied with group texturing tools.
                _area = texture.ParentArea.IsZero ? texture.GetRect().Round() : texture.ParentArea.Round();
                Initialize(texture.Texture, isForRoom);
            }

            public ParentTextureArea(Rectangle2 area, Texture texture, bool isForRoom)
            {
                _area = area;
                Initialize(texture, isForRoom);
            }

            private void Initialize(Texture texture, bool isForRoom)
            {
                Children = new List<ChildTextureArea>();

                Texture = texture;
                IsForRoom = isForRoom;
                SqueezeAndDuplicate = false;
            }

            // Compare parent's properties with incoming texture properties.
            public bool ParametersSimilar(TextureArea incomingTexture, bool isForRoom)
            {
                if (IsForRoom != isForRoom)
                    return false;

                TextureHashed incoming = incomingTexture.Texture as TextureHashed;
                TextureHashed current  = Texture as TextureHashed;

                // First case here should never happen, unless we find a way to texture rooms
                // with WAD textures or vice versa.
                if ((incoming == null) != (current == null))
                    return false;
                else if (incoming != null)
                    return incoming.Hash == current.Hash;
                else
                    return incomingTexture.Texture.GetHashCode() == Texture.GetHashCode();
            }

            // Check if bumpmapping could be assigned to parent.
            // NOTE: This function is only used to check if bumpmap is possible, DO NOT use it to check ACTUAL bumpmap level!
            public BumpMappingLevel BumpLevel()
            {
                if(Texture is LevelTexture)
                {
                    var tex = Texture as LevelTexture;
                    if (!String.IsNullOrEmpty(tex.BumpPath))
                        return BumpMappingLevel.Level1; // tomb4 doesn't care about specific flag value
                    else
                    {
                        var bumpLevel = tex.GetBumpMappingLevelFromTexCoord(Area.GetMid());
                        return bumpLevel.HasValue ? bumpLevel.Value : BumpMappingLevel.None; 
                    }
                }
                return BumpMappingLevel.None;
            }

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
                        var intersection = rect.Intersect(_area);
                        if (!_area.Contains(rect) && intersection.Width > 0 && intersection.Height > 0)
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
            public void AddChild(TextureArea texture, int newTextureID, bool isForTriangle, bool topmostAndUnpadded)
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

                // Refresh topmost flag
                TopmostAndUnpadded = topmostAndUnpadded;

                // Expand parent area, if needed
                var rect = texture.GetRect();
                if (!Area.Contains(rect))
                    Area = Area.Union(rect).Round();
            }

            public void MoveChild(ChildTextureArea child, ParentTextureArea newParent)
            {
                var newRelCoord = new Vector2[child.AbsCoord.Length];
                for (int i = 0; i < newRelCoord.Length; i++)
                    newRelCoord[i] = child.AbsCoord[i] - newParent.Area.Start;

                newParent.Children.Add(new ChildTextureArea()
                {
                    TexInfoIndex = child.TexInfoIndex,
                    BlendMode = child.BlendMode,
                    IsForTriangle = child.IsForTriangle,
                    RelCoord = newRelCoord,
                    AbsCoord = child.AbsCoord
                });
            }

            public void MergeParents(List<ParentTextureArea> parentList, List<ParentTextureArea> parents)
            {
                foreach (var parent in parents)
                {
                    Area = Area.Union(parent.Area);
                    TopmostAndUnpadded = parent.TopmostAndUnpadded; // Refresh topmost flag

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

                foreach (var parent in CompiledAnimation)
                {
                    var newParent = new ParentTextureArea(parent.Area, parent.Texture, parent.IsForRoom);

                    // Squeeze and duplicate bitmap data for UVRotate texture sets
                    newParent.SqueezeAndDuplicate = Origin.IsUvRotate;

                    foreach (var child in parent.Children)
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
                foreach (var parent in CompiledAnimation)
                    result += parent.Children.Count;
                return result;
            }
        }

        private class ObjectTexture
        {
            public int Tile;
            public VectorInt2[] TexCoord = new VectorInt2[4];
            public ushort UVAdjustmentFlag;

            public bool IsForTriangle;
            public bool IsForRoom;
            public BlendMode BlendMode;
            public BumpMappingLevel BumpLevel;

            public ObjectTexture(ParentTextureArea parent, ChildTextureArea child, GameVersion version, float maxTextureSize)
            {
                BlendMode = child.BlendMode;
                BumpLevel = parent.BumpLevel();
                IsForRoom = parent.IsForRoom;
                IsForTriangle = child.IsForTriangle;
                Tile = parent.Page;
                UVAdjustmentFlag = (ushort)TextureExtensions.GetTextureShapeType(child.RelCoord, IsForTriangle);

                for (int i = 0; i < child.RelCoord.Length; i++)
                {
                    var coord = new Vector2(child.RelCoord[i].X + (float)(parent.PositionInPage.X + parent.Padding[0]),
                                            child.RelCoord[i].Y + (float)(parent.PositionInPage.Y + parent.Padding[1]));

                    // Apply texture distortion as countermeasure for hardcoded TR4-5 mapping correction
                    if (version >= GameVersion.TR4)
                        coord -= IsForTriangle ? TextureExtensions.CompensationTris[UVAdjustmentFlag, i] : TextureExtensions.CompensationQuads[UVAdjustmentFlag, i];

                    // Clamp coordinates that are possibly out of bounds
                    coord.X = (float)MathC.Clamp(coord.X, 0, maxTextureSize);
                    coord.Y = (float)MathC.Clamp(coord.Y, 0, maxTextureSize);

                    // Pack coordinates into 2-byte set (whole and frac parts)
                    TexCoord[i] = new VectorInt2((((int)Math.Truncate(coord.X)) << 8) + (int)(Math.Floor(coord.X % 1.0f * 255.0f)),
                                                 (((int)Math.Truncate(coord.Y)) << 8) + (int)(Math.Floor(coord.Y % 1.0f * 255.0f)));
                }

                if (child.IsForTriangle)
                    TexCoord[3] = TexCoord[2];
            }

            public Rectangle2 GetRect()
            {
                if (IsForTriangle)
                    return Rectangle2.FromCoordinates(TexCoord[0], TexCoord[1], TexCoord[2]);
                else
                    return Rectangle2.FromCoordinates(TexCoord[0], TexCoord[1], TexCoord[2], TexCoord[3]);
            }
        }

        public TexInfoManager(Level level, IProgressReporter progressReporter)
        {
            _level = level;
            Padding = (ushort)level.Settings.TexturePadding;
            _progressReporter = progressReporter;

            if(level.Settings.GameVersion == GameVersion.TR5Main)
                MaxTileSize = 256; // Change later...
            else
                MaxTileSize = 256;

            GenerateAnimLookups(_level.Settings.AnimatedTextureSets);  // Generate anim texture lookup table
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

        public bool TryToAddToExisting(TextureArea texture, List<ParentTextureArea> parentList, bool isForRoom, bool isForTriangle, bool topmostAndUnpadded = false, int animFrameIndex = -1)
        {
            // Try to find potential parent (larger texture) and add itself to children
            foreach (var parent in parentList)
            {
                if (!parent.IsPotentialParent(texture, isForRoom, animFrameIndex >= 0, MaxTileSize))
                    continue;

                parent.AddChild(texture, animFrameIndex >= 0 ? animFrameIndex : GetNewTexInfoIndex(), isForTriangle, topmostAndUnpadded);
                return true;
            }

            // Try to find and merge parents which are enclosed in incoming texture area
            var childrenWannabes = parentList.Where(item => item.IsPotentialChild(texture, isForRoom)).ToList();
            if (childrenWannabes.Count > 0)
            {
                var newParent = new ParentTextureArea(texture, isForRoom);
                newParent.AddChild(texture, animFrameIndex >= 0 ? animFrameIndex : GetNewTexInfoIndex(), isForTriangle, topmostAndUnpadded);
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

        private Result? GetTexInfo(TextureArea areaToLook, List<ParentTextureArea> parentList, bool isForRoom, bool isForTriangle, bool topmostAndUnpadded, bool checkParameters = true, float lookupMargin = 0.0f)
        {
            var lookupCoordinates = new Vector2[isForTriangle ? 3 : 4];
            for (int i = 0; i < lookupCoordinates.Length; i++)
                lookupCoordinates[i] = areaToLook.GetTexCoord(i);

            foreach (var parent in parentList)
            {
                // Parents with different attributes are quickly discarded
                if (checkParameters && !parent.ParametersSimilar(areaToLook, isForRoom))
                    continue;

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
                        // Refresh topmost flag, as same texture may be applied to faces with different topmost priority
                        parent.TopmostAndUnpadded = topmostAndUnpadded;

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

        public void AddParent(TextureArea texture, List<ParentTextureArea> parentList, bool isForRoom, bool isForTriangle, bool topmostAndUnpadded, int frameIndex = -1)
        {
            var newParent = new ParentTextureArea(texture, isForRoom);
            parentList.Add(newParent);
            newParent.AddChild(texture, frameIndex >= 0 ? frameIndex : GetNewTexInfoIndex(), isForTriangle, topmostAndUnpadded);
        }

        // Only exposed variation of AddTexture that should be used outside of TexInfoManager itself

        public Result AddTexture(TextureArea texture, bool isForRoom, bool isForTriangle, bool topmostAndUnpadded = false)
        {
            if (isForRoom)
            {
                // Try to compare incoming texture with existing anims and return animation frame
                if (ActualAnimTextures.Count > 0)
                    foreach (var actualTex in ActualAnimTextures)
                    {
                        var existing = GetTexInfo(texture, actualTex.CompiledAnimation, isForRoom, isForTriangle, false, true, AnimTextureLookupMargin);
                        if (existing.HasValue)
                            return existing.Value;
                    }

                // Now try to compare incoming texture with lookup anim seq table
                if (ReferenceAnimTextures.Count > 0)
                    foreach (var refTex in ReferenceAnimTextures)
                    {
                        // If reference set found, generate actual one and immediately return fresh result
                        if (GetTexInfo(texture, refTex.CompiledAnimation, isForRoom, isForTriangle, false, false, AnimTextureLookupMargin).HasValue)
                        {
                            GenerateAnimTexture(refTex, texture, isForRoom, isForTriangle);
                            return AddTexture(texture, isForRoom, isForTriangle);
                        }
                    }
            }

            return AddTexture(texture, ParentTextures, isForRoom, isForTriangle, topmostAndUnpadded);
        }

        // Internal AddTexture variation which is capable of adding texture to various ParentTextureArea lists
        // with customizable parameters.
        // If animFrameIndex == -1, it means that ordinary texture is added, otherwise it indicates that specific anim
        // texture frame is being processed. If so, frame index is saved into TexInfoIndex field of resulting child.
        // Later on, on real anim texture creation, this index is used to sort frames in proper order.

        private Result AddTexture(TextureArea texture, List<ParentTextureArea> parentList, bool isForRoom, bool isForTriangle, bool topmostAndUnpadded = false, int animFrameIndex = -1, bool makeCanonical = true)
        {
            // In case AddTexture is used with animated seq packing, we don't check frames for full similarity, because
            // frames can be dublicated with Repeat function or simply because of complex animator functions applied.
            var result = animFrameIndex >= 0 ? null : GetTexInfo(texture, parentList, isForRoom, isForTriangle, topmostAndUnpadded);

            if (!result.HasValue)
            {
                // Try to create new canonical (top-left-based) texture as child or parent.
                // makeCanonical parameter is necessary for animated textures, because animators may produce frames
                // with non-canonically rotated coordinates (e.g. spin animator).
                var canonicalTexture = makeCanonical ? texture.GetCanonicalTexture(isForTriangle) : texture;

                // If no any potential parents or children, create as new parent
                if (!TryToAddToExisting(canonicalTexture, parentList, isForRoom, isForTriangle, topmostAndUnpadded, animFrameIndex))
                    AddParent(canonicalTexture, parentList, isForRoom, isForTriangle, topmostAndUnpadded, animFrameIndex);

                // Try again to get texinfo
                if (animFrameIndex >= 0)
                    result = new Result { TexInfoIndex = DummyTexInfo, Rotation = 0 };
                else
                    result = GetTexInfo(texture, parentList, isForRoom, isForTriangle, topmostAndUnpadded);
            }

            return result.Value;
        }

        // Scan and set alpha-test blending mode for opaque textures.
        // To speed up the process, all children whose parent region contains alpha, is also marked as
        // alpha.

        private void SortOutAlpha(List<ParentTextureArea> parentList)
        {
            Parallel.For(0, parentList.Count, i =>
            {
                var opaqueChildren = parentList[i].Children.Where(child => child.BlendMode < BlendMode.Additive);
                if (opaqueChildren.Count() > 0 &&
                   parentList[i].Texture.Image.HasAlpha((int)parentList[i].Area.X0,
                                                        (int)parentList[i].Area.Y0,
                                                        (int)parentList[i].Area.Width,
                                                        (int)parentList[i].Area.Height))
                {
                    foreach (var children in opaqueChildren)
                        children.BlendMode = BlendMode.AlphaTest;
                }
            });
        }

        private int PlaceTexturesInMap(ref List<ParentTextureArea> textures, bool forceMinimumPadding = false)
        {
            if (textures.Count == 0)
                return 0;

            int currentPage = -1;
            List<RectPacker> texPackers = new List<RectPacker>();

            for (int i = 0; i < textures.Count; i++)
            {
                // Get the size of the quad surrounding texture area, typically should be the texture area itself
                int w = (int)(textures[i].Area.Width);
                int h = (int)(textures[i].Area.Height);

                // Calculate adaptive padding at all sides
                int padding = (Padding == 0 && forceMinimumPadding) ? MinimumPadding : Padding;

                int tP = textures[i].TopmostAndUnpadded ? 0 : padding; // Ugly, but needed for tomb4 UVRotate
                int bP = padding;
                int lP = padding;
                int rP = padding;

                int horizontalPaddingSpace = MaxTileSize - w;
                int verticalPaddingSpace = MaxTileSize - h;

                // If hor/ver padding won't fully fit, get existing space and calculate padding out of it
                if (verticalPaddingSpace < tP + bP)
                {
                    tP = (tP == 0) ? tP : verticalPaddingSpace / 2; // Ugly, but needed for tomb4 UVRotate
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

                for (ushort j = 0; j <= currentPage; ++j)
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
                texPackers.Add(new RectPackerTree(new VectorInt2(256, 256)));
                pos = texPackers.Last().TryAdd(new VectorInt2(w, h));

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
            foreach (var set in sets)
            {
                // Ignore trivial (single-frame non-UVRotated) anims
                if (set.AnimationIsTrivial)
                    continue;

                int triangleVariation = 0;
                bool mirroredVariation = false;

                // Create all possible versions of current animation, including
                // mirrored and rotated ones. Later on, when parsing actual TextureAreas
                // from faces, we will compare them with this "lookup table" and will be
                // able to quickly return desired variation ID without complicated in-place
                // calculations.

                while (true)
                {
                    var refAnim = new ParentAnimatedTexture(set);
                    int index = 0;

                    foreach (var frame in set.Frames)
                    {
                        // Create base frame
                        TextureArea newFrame = new TextureArea() { Texture = frame.Texture };

                        // Rotate or cut 4nd coordinate if needed
                        switch (triangleVariation)
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
                        if (triangleVariation > 0)
                            newFrame.TexCoord3 = newFrame.TexCoord2;

                        // Mirror if needed
                        if (mirroredVariation)
                            newFrame.Mirror();

                        // Make frame, including repeat versions
                        for (int i = 0; i < frame.Repeat; i++)
                        {
                            AddTexture(newFrame, refAnim.CompiledAnimation, true, (triangleVariation > 0), false, index, set.IsUvRotate);
                            index++;
                        }
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

        private void GenerateAnimTexture(ParentAnimatedTexture reference, TextureArea origin, bool isForRoom, bool isForTriangle)
        {
            var refCopy = reference.Clone();
            foreach (var parent in refCopy.CompiledAnimation)
            {
                parent.IsForRoom = isForRoom;

                foreach (var child in parent.Children)
                    child.BlendMode = origin.BlendMode;
            }

            // Sort and assign TexInfo indices for frames by the order they were created in reference animation
            var orderedFrameList = refCopy.CompiledAnimation.SelectMany(x => x.Children).OrderBy(c => c.TexInfoIndex);
            foreach(var frame in orderedFrameList)
                frame.TexInfoIndex = GetNewTexInfoIndex();

            ActualAnimTextures.Add(refCopy);
        }

        private ImageC BuildTextureMap(ref List<ParentTextureArea> textures, int numPages, bool bump, bool forceMinimumPadding = false)
        {
            var customBumpmaps = new Dictionary<string, ImageC>();
            var image = ImageC.CreateNew(256, numPages * 256 * (bump ? 2 : 1));

            var actualPadding = (Padding == 0 && forceMinimumPadding) ? MinimumPadding : Padding;

            for (int i = 0; i < textures.Count; i++)
            {
                var p = textures[i];
                var x = (int)p.Area.Start.X;
                var y = (int)p.Area.Start.Y;
                var width = (int)p.Area.Width;
                var height = (int)p.Area.Height;

                var destX = p.PositionInPage.X + p.Padding[0];
                var destY = p.Page * 256 + p.PositionInPage.Y + p.Padding[1];

                if (p.SqueezeAndDuplicate)
                {
                    // If squeeze-and-duplicate approach is needed (UVRotate), use system drawing routines
                    // to do high-quality bicubic resampling.

                    // Copy original region to new image
                    var originalImage = ImageC.CreateNew(width, height);
                    originalImage.CopyFrom(0, 0, p.Texture.Image, x, y, width, height);

                    // Make squeezed bitmap and put original one into it using bicubic resampling
                    var destBitmap = new Bitmap(width, height / 2);
                    using (var graphics = System.Drawing.Graphics.FromImage(destBitmap))
                    {
                        graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
                        graphics.DrawImage(originalImage.ToBitmap(), 0, 0, destBitmap.Width, destBitmap.Height);
                    }

                    // Twice copy squeezed image to original image
                    var squeezedImage = ImageC.FromSystemDrawingImage(destBitmap);
                    originalImage.CopyFrom(0, 0, squeezedImage, 0, 0, width, height / 2);
                    originalImage.CopyFrom(0, height / 2, squeezedImage, 0, 0, width, height / 2);

                    // Copy squeezed-and-duplicated image to texture map and add padding
                    image.CopyFrom(destX, destY, originalImage, 0, 0, width, height);
                    AddPadding(p, originalImage, image, 0, actualPadding, 0, 0);
                }
                else
                {
                    image.CopyFrom(destX, destY, p.Texture.Image, x, y, width, height);
                    AddPadding(p, p.Texture.Image, image, 0, actualPadding);
                }

                // Do the bump map if needed

                if (p.Texture is LevelTexture)
                {
                    var tex = (p.Texture as LevelTexture);
                    var bumpX = destX;
                    var bumpY = destY + (numPages * 256);

                    // Try to copy custom bumpmaps
                    if (!String.IsNullOrEmpty(tex.BumpPath))
                    {
                        if (!customBumpmaps.ContainsKey(tex.BumpPath))
                        {
                            var potentialBumpImage = ImageC.FromFile(_level.Settings.MakeAbsolute(tex.BumpPath));

                            // Only assign bumpmap image if size is equal to texture image size, otherwise use dummy

                            if (potentialBumpImage != null && potentialBumpImage.Size == tex.Image.Size)
                                customBumpmaps.Add(tex.BumpPath, potentialBumpImage);
                            else
                            {
                                _progressReporter.ReportWarn("Texture file '" + tex + "' has external bumpmap assigned which has different size and was ignored.");
                                customBumpmaps.Add(tex.BumpPath, ImageC.Black);
                            }
                        }

                        image.CopyFrom(bumpX, bumpY, customBumpmaps[tex.BumpPath], x, y, width, height);
                        AddPadding(p, image, image, numPages, actualPadding, bumpX, bumpY);
                    }
                    else
                    {
                        var level = tex.GetBumpMappingLevelFromTexCoord(p.Area.GetMid());

                        if (level != BumpMappingLevel.None)
                        {
                            var bumpImage = ImageC.CreateNew(width, height);
                            bumpImage.CopyFrom(0, 0, image, destX, destY, width, height);

                            int effectWeight = 0;
                            int effectSize = 0;

                            switch (level)
                            {
                                case BumpMappingLevel.Level1:
                                    effectWeight = -2;
                                    effectSize = 2;
                                    break;
                                case BumpMappingLevel.Level2:
                                    effectWeight = -2;
                                    effectSize = 3;
                                    break;
                                case BumpMappingLevel.Level3:
                                    effectWeight = -1;
                                    effectSize = 2;
                                    break;
                            }

                            bumpImage.Emboss(0, 0, bumpImage.Width, bumpImage.Height, effectWeight, effectSize);
                            image.CopyFrom(bumpX, bumpY, bumpImage, 0, 0, width, height);
                            AddPadding(p, image, image, numPages, actualPadding, bumpX, bumpY);
                        }
                    }
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

        public void PackTextures()
        {
            // Subdivide textures in 3 blocks: room, objects, bump
            var roomTextures = new List<ParentTextureArea>();
            var objectsTextures = new List<ParentTextureArea>();
            var bumpedTextures = new List<ParentTextureArea>();

            for (int i = 0; i < ParentTextures.Count; i++)
            {
                if (ParentTextures[i].IsForRoom)
                {
                    if (ParentTextures[i].BumpLevel() != BumpMappingLevel.None)
                        bumpedTextures.Add(ParentTextures[i]);
                    else
                        roomTextures.Add(ParentTextures[i]);
                }
                else
                    objectsTextures.Add(ParentTextures[i]);
            }

            // Sort textures by their TopmostAndUnpadded property (waterfalls first!)
            if(_level.Settings.AgressiveTexturePacking)
                roomTextures = roomTextures.OrderBy(item => !item.TopmostAndUnpadded).ToList();
            else
                objectsTextures = objectsTextures.OrderBy(item => !item.TopmostAndUnpadded).ToList();

            for (int n = 0; n < ActualAnimTextures.Count; n++)
            {
                var parentTextures = ActualAnimTextures[n].CompiledAnimation;
                for (int i = 0; i < parentTextures.Count; i++)
                {
                    if (parentTextures[i].IsForRoom)
                    {
                        if (parentTextures[i].BumpLevel() != BumpMappingLevel.None)
                            bumpedTextures.Add(parentTextures[i]);
                        else
                            roomTextures.Add(parentTextures[i]);
                    }
                    else
                        objectsTextures.Add(parentTextures[i]);
                }
            }

            // Calculate new X, Y of each texture area
            NumRoomPages = PlaceTexturesInMap(ref roomTextures);
            NumObjectsPages = PlaceTexturesInMap(ref objectsTextures, true);
            NumBumpPages = PlaceTexturesInMap(ref bumpedTextures);

            // Place all the textures areas in the maps
            RoomPages = BuildTextureMap(ref roomTextures, NumRoomPages, false);
            ObjectsPages = BuildTextureMap(ref objectsTextures, NumObjectsPages, false, true);
            BumpPages = BuildTextureMap(ref bumpedTextures, NumBumpPages, true);
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

        public void SortAnimatedTextures()
        {
            // Put UVRotate sequences first
            ActualAnimTextures = ActualAnimTextures.OrderBy(item => !item.Origin.IsUvRotate).ToList();
        }

        public void WriteAnimatedTextures(BinaryWriterEx writer)
        {
            int numAnimatedTextures = 1;
            foreach (var compiledAnimatedTexture in ActualAnimTextures)
                numAnimatedTextures += compiledAnimatedTexture.FrameCount() + 1;
            writer.Write((uint)numAnimatedTextures);

            writer.Write((ushort)ActualAnimTextures.Count);
            foreach (var compiledAnimatedTexture in ActualAnimTextures)
            {
                writer.Write((ushort)(compiledAnimatedTexture.FrameCount() - 1));

                // Write TexInfos in ascending order, in accordance with reference animation frame order
                var orderedFrameList = compiledAnimatedTexture.CompiledAnimation.SelectMany(x => x.Children).OrderBy(c => c.TexInfoIndex);
                foreach (var frame in orderedFrameList)
                    writer.Write((ushort)frame.TexInfoIndex);
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
                    // Built-in TR4-5 mapping correction is not used. Dummy mapping type is used
                    // together with compensation coordinate distortion.
                    ushort newFlags = texture.UVAdjustmentFlag;

                    if (texture.IsForRoom) newFlags |= 0x8000;

                    if      (texture.BumpLevel == BumpMappingLevel.Level1) newFlags |= (1 << 9);
                    else if (texture.BumpLevel == BumpMappingLevel.Level2) newFlags |= (2 << 9);
                    else if (texture.BumpLevel == BumpMappingLevel.Level3) newFlags |= (3 << 9);

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
                    var rect = texture.GetRect();
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
                if (texture.IsForRoom && texture.BumpLevel == BumpMappingLevel.None)
                {
                    // Tile is OK
                }
                else if (!texture.IsForRoom)
                {
                    texture.Tile += NumRoomPages;
                }
                else if (texture.IsForRoom && texture.BumpLevel != BumpMappingLevel.None)
                {
                    texture.Tile += NumRoomPages + NumObjectsPages + numSpritesPages;
                }
            }

            NumObjectsPages += numSpritesPages;
        }
    }
}
