using NLog;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Threading.Tasks;
using TombLib.IO;
using TombLib.LevelData.Compilers.TombEngine;
using TombLib.Utils;
using TombLib.Wad;

namespace TombLib.LevelData.Compilers
{
    public enum TextureDestination
    {
        RoomOrAggressive,
        Moveable,
        Static
    }

    public class TombEngineTexInfoManager
    {
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();

        public const int AtlasSize = 4096;
        public const int PageSize = AtlasSize;
        public const int PagesPerRowInAtlas = AtlasSize / PageSize;
        public const int PagesPerAtlas = PagesPerRowInAtlas * PagesPerRowInAtlas;

        private const int _noTexInfo = -1;
        private const int _dummyTexInfo = -2;
        private const int _minimumPadding = 1;
        private const int _minimumTileSize = AtlasSize;
        private const float _animTextureLookupMargin = 5.0f;

        // We need to keep level reference for padding and bumpmap references.

        protected readonly Level _level;
        protected readonly IProgressReporter _progressReporter;

        // Defines if texinfo manager should actually start generating texinfo indexes.
        // Needed for anim lookup table generation.

        private readonly bool _generateTexInfos = false;

        // Defines if all data has been laid out and texture manager is in finalized state.
        // If user tries to add textures after that, texture manager will throw an exception.

        private bool _dataHasBeenLaidOut = false;

        // Two lists of animated textures contain reference animation versions for each sequence
        // and actual found animated texture sequences in rooms. When compiler encounters a tile
        // which is similar to one of the reference animation versions, it copies it into actual
        // animations list with all respective frames. Any new comparison is made with actual
        // animation sequences at first, so next found similar tile will refer to already existing
        // animation version.
        // On packing, list of actual anim textures is added to main ParentTextures list, so only
        // versions existing in level file are added to texinfo list. Rest from reference list
        // is ignored.

        private readonly List<ParentAnimatedTexture> _referenceAnimTextures = new List<ParentAnimatedTexture>();
        private List<ParentAnimatedTexture> _actualAnimTextures = new List<ParentAnimatedTexture>();

        // UVRotate count should be placed after anim texture data to identify how many first anim seqs
        // should be processed using UVRotate engine function

        public int UvRotateCount => _actualAnimTextures.Count(seq => seq.Origin.IsUvRotate);

        // List of parent textures should contain all "ancestor" texture areas in which all variations
        // are placed, including mirrored and rotated ones.

        private readonly List<ParentTextureArea> _parentTextures = new List<ParentTextureArea>();

        // MaxTileSize defines maximum size to which parent can be inflated by incoming child, if
        // inflation is allowed.

        private readonly ushort MaxTileSize = _minimumTileSize;

        // If padding value is 0, 1 px padding will be forced on object textures anyway,
        // because yet we don't have a mechanism to specify UV adjustment in converted WADs.

        private readonly ushort _padding = 8;

        // TexInfoCount is internally a "reference counter" which is also used to get new TexInfo IDs.
        // Since generation of TexInfos is an one-off serialized process, we can safely use it in
        // serial manner as well.

        public int TexInfoCount { get; private set; } = 0;

        // Final texture pages and its counters

        public int NumRoomPages { get; private set; }
        public ImageC RoomsPagesPacked { get; private set; }
        public List<TombEngineAtlas> RoomsAtlas { get; private set; }

        public int NumObjectsPages { get; private set; }
        public ImageC ObjectsPagesPacked { get; private set; }

        public int NumMoveablesPages { get; private set; }
        public List<TombEngineAtlas> MoveablesAtlas { get; private set; }

        public int NumStaticsPages { get; private set; }
        public List<TombEngineAtlas> StaticsAtlas { get; private set; }

        public int NumBumpPages { get; private set; }
        public ImageC BumpPagesPacked { get; private set; }

        public int NumAnimatedPages { get; private set; }
        public List<TombEngineAtlas> AnimatedAtlas { get; private set; }

        // Precompiled object textures are kept in this dictionary.

        private SortedDictionary<int, ObjectTexture> _objectTextures;

        // Precompiled anim texture indices are kept separately to avoid
        // messing up after texture page cleanup.

        private List<List<ushort>> _animTextureIndices;

        // Expose the latter publicly, because TRNG compiler needs it
        public ReadOnlyCollection<KeyValuePair<AnimatedTextureSet, ReadOnlyCollection<ushort>>> AnimatedTextures
        {
            get
            {
                var result = new List<KeyValuePair<AnimatedTextureSet, ReadOnlyCollection<ushort>>>();
                for (int i = 0; i < _animTextureIndices.Count; i++)
                {
                    result.Add(new KeyValuePair<AnimatedTextureSet, ReadOnlyCollection<ushort>>
                        (_actualAnimTextures[i].Origin, _animTextureIndices[i].AsReadOnly()));
                }

                return result.AsReadOnly();
            }
        }

        public class TexturePage
        {
            public int Atlas { get; set; }
            public VectorInt2 Position { get; set; }
            public ImageC ColorMap { get; set; }
            public ImageC NormalMap { get; set; }
            public bool HasNormalMap;
        }

        // ChildTextureArea is a simple enclosed relative texture area with stripped down parameters
        // which should be the same among all children of same parent.
        // Stripped down parameters include BumpLevel and IsForTriangle, because if these are
        // different, it automatically means we should assign new parent for this child.

        public class ChildTextureArea
        {
            public int TexInfoIndex;
            public Vector2[] RelCoord; // Relative to parent!
            public Vector2[] AbsCoord; // Absolute

            public BlendMode BlendMode;
            public bool IsForTriangle;

            public Rectangle2 GetRect()
            {
                if (AbsCoord.Length == 3)
                {
                    return Rectangle2.FromCoordinates(AbsCoord[0], AbsCoord[1], AbsCoord[2]);
                }
                else
                {
                    return Rectangle2.FromCoordinates(AbsCoord[0], AbsCoord[1], AbsCoord[2], AbsCoord[3]);
                }
            }
        }

        // ParentTextureArea is a texture area which contains all other texture areas which are
        // completely inside current one. Bumpmapping parameter define that parent is different,
        // hence two TextureAreas with same UV coordinates but with different BumpLevel will be 
        // saved as different parents.

        public class ParentTextureArea
        {
            public VectorInt2 PositionInPage { get; set; }
            public VectorInt2 PositionInAtlas { get; set; }
            public int Page { get; set; }
            public int AtlasIndex { get; set; }
            public VectorInt2 AtlasDimensions { get; set; }
            public int[] Padding { get; set; } = new int[4]; // LTRB
            public Texture Texture { get; private set; }
            public TextureDestination Destination { get; set; }

            // We try to group together textures for alpha blend so the engine can 
            // optimize draw calls
            private bool _forSortedTransparency;
            public bool ForSortedTransparency
            {
                get { return _forSortedTransparency; }
                set
                {
                    if (value)
                    {
                        _forSortedTransparency = value;
                    }
                }
            }

            private Rectangle2 _area;
            public Rectangle2 Area
            {
                get { return _area; }
                set
                {
                    // Disallow reducing area size, cause it corrupts children.
                    if (value == _area || !value.Contains(_area))
                    {
                        return;
                    }

                    // Calculate startpoint delta and fix up children to comply with new parent area.
                    if (Children != null && Children.Count > 0)
                    {
                        Vector2 delta = _area.Start - value.Start;
                        foreach (ChildTextureArea child in Children)
                        {
                            for (int i = 0; i < child.RelCoord.Length; i++)
                            {
                                child.RelCoord[i] += delta;
                            }
                        }
                    }

                    _area = value;
                }
            }

            public List<ChildTextureArea> Children;

            // Generates new ParentTextureArea from raw texture coordinates.
            public ParentTextureArea(TextureArea texture, TextureDestination destination)
            {
                // Round area to nearest pixel to prevent rounding errors further down the line.
                // Use ParentArea to create a parent for textures which were applied with group texturing tools.
                _area = texture.ParentArea.IsZero ? texture.GetRect().Round() : texture.ParentArea.Round();
                Initialize(texture.Texture, destination);
            }

            // Generates new ParentTextureArea from given area in texture.
            public ParentTextureArea(Rectangle2 area, Texture texture, TextureDestination destination)
            {
                _area = area;
                Initialize(texture, destination);
            }

            private void Initialize(Texture texture, TextureDestination destination)
            {
                Children = new List<ChildTextureArea>();

                Texture = texture;
                Destination = destination;
            }

            // Compare parent's properties with incoming texture properties.
            public bool ParametersSimilar(TextureArea incomingTexture, TextureDestination destination)
            {
                if (Destination != destination)
                {
                    return false;
                }

                // See if texture is the same
                TextureHashed incoming = incomingTexture.Texture as TextureHashed;
                TextureHashed current = Texture as TextureHashed;

                // First case here should never happen, unless we find a way to texture rooms
                // with WAD textures or vice versa.
                if ((incoming == null) != (current == null))
                {
                    return false;
                }
                else if (incoming != null)
                {
                    return incoming.Hash == current.Hash;
                }
                else
                {
                    return incomingTexture.Texture.GetHashCode() == Texture.GetHashCode();
                }
            }

            // Compare raw bitmap data of given area with incoming texture
            public TextureArea? TextureSimilar(TextureArea texture)
            {
                // Only scan if:
                //  - Parent is room texture
                //  - Parent's texture isn't the same as incoming texture
                //  - Incoming texture is either from imported geometry or wad

                if (Texture != texture.Texture && Texture is LevelTexture &&
                   (texture.Texture is ImportedGeometryTexture || texture.Texture is WadTexture))
                {
                    Rectangle2 rr = texture.GetRect();
                    ColorC pp0 = texture.Texture.Image.GetPixel((int)rr.TopLeft.X, (int)rr.TopLeft.Y);
                    ColorC pp1 = texture.Texture.Image.GetPixel((int)rr.TopRight.X - 1, (int)rr.TopRight.Y);
                    ColorC pp2 = texture.Texture.Image.GetPixel((int)rr.BottomRight.X - 1, (int)rr.BottomRight.Y - 1);
                    ColorC pp3 = texture.Texture.Image.GetPixel((int)rr.BottomLeft.X, (int)rr.BottomLeft.Y - 1);
                    ColorC pp4 = texture.Texture.Image.GetPixel((int)rr.Width / 2, (int)rr.Height / 2);

                    foreach (ChildTextureArea child in Children)
                    {
                        // Compare size. If no match, it's not similar texture
                        Rectangle2 r = child.GetRect();
                        if (r.Width != rr.Width || r.Height != rr.Height)
                        {
                            continue;
                        }

                        // Compare 4 corner pixels and center to quickly filter out wrong results
                        ColorC p0 = Texture.Image.GetPixel((int)r.TopLeft.X, (int)r.TopLeft.Y);
                        ColorC p1 = Texture.Image.GetPixel((int)r.TopRight.X - 1, (int)r.TopRight.Y);
                        ColorC p2 = Texture.Image.GetPixel((int)r.BottomRight.X - 1, (int)r.BottomRight.Y - 1);
                        ColorC p3 = Texture.Image.GetPixel((int)r.BottomLeft.X, (int)r.BottomLeft.Y - 1);
                        ColorC p4 = texture.Texture.Image.GetPixel((int)r.Width / 2, (int)r.Height / 2);
                        if (p0 != pp0 || p1 != pp1 || p2 != pp2 || p3 != pp3 || p4 != pp4)
                        {
                            continue;
                        }

                        // All pixels match. Now compare all raw data
                        Hash hash1 = Hash.FromByteArray(texture.Texture.Image.ToByteArray(rr));
                        Hash hash2 = Hash.FromByteArray(Texture.Image.ToByteArray(r));

                        if (hash1 == hash2)
                        {
                            // Replace texture set with found one and substitute texture coordinates
                            TextureArea newtex = texture;
                            Vector2 shift = r.TopLeft - rr.TopLeft;
                            newtex.Texture = Texture;
                            newtex.TexCoord0 += shift;
                            newtex.TexCoord1 += shift;
                            newtex.TexCoord2 += shift;
                            if (child.AbsCoord.Length > 3)
                            {
                                newtex.TexCoord3 += shift;
                            }
                            else
                            {
                                newtex.TexCoord3 = newtex.TexCoord2;
                            }

                            return newtex;
                        }
                    }
                }

                return null;
            }

            // Check if bumpmapping could be assigned to parent.
            // NOTE: This function is only used to check if bumpmap is possible, DO NOT use it to check ACTUAL bumpmap level!
            public BumpMappingLevel BumpLevel()
            {
                if (Texture is LevelTexture)
                {
                    LevelTexture tex = Texture as LevelTexture;
                    if (!string.IsNullOrEmpty(tex.BumpPath))
                    {
                        return BumpMappingLevel.Level1; // tomb4 doesn't care about specific flag value
                    }
                    else
                    {
                        BumpMappingLevel? bumpLevel = tex.GetBumpMappingLevelFromTexCoord(Area.GetMid());
                        return bumpLevel.HasValue ? bumpLevel.Value : BumpMappingLevel.None;
                    }
                }
                return BumpMappingLevel.None;
            }

            // Checks if parameters are similar to another texture area, and if so,
            // also checks if texture area is enclosed in parent's area.
            public bool IsPotentialParent(TextureArea texture, TextureDestination destination, bool allowOverlaps, uint maxOverlappedSize)
            {
                Rectangle2 rect = texture.GetRect();

                if (ParametersSimilar(texture, destination))
                {
                    if (_area.Contains(rect))
                    {
                        return true;
                    }
                    else if (allowOverlaps)
                    {
                        Rectangle2 intersection = rect.Intersect(_area);
                        if (!_area.Contains(rect) && intersection.Width > 0 && intersection.Height > 0)
                        {
                            Rectangle2 potentialNewArea = rect.Union(_area);
                            return ((potentialNewArea.Width <= maxOverlappedSize) &&
                                    (potentialNewArea.Height <= maxOverlappedSize));
                        }
                    }
                }
                return false;
            }

            // Checks if incoming texture is similar in parameters and encloses parent area.
            public bool IsPotentialChild(TextureArea texture, TextureDestination destination)
            {
                return (ParametersSimilar(texture, destination) && texture.GetRect().Round().Contains(_area));
            }

            // Adds texture as a child to existing parent, with recalculating coordinates to relative.
            public void AddChild(TextureArea texture, int newTextureID, bool isForTriangle, bool requireSort)
            {
                Vector2[] relative = new Vector2[isForTriangle ? 3 : 4];
                Vector2[] absolute = new Vector2[isForTriangle ? 3 : 4];

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

                ForSortedTransparency |= requireSort;

                // Expand parent area, if needed
                Rectangle2 rect = texture.GetRect();
                if (!Area.Contains(rect))
                {
                    Area = Area.Union(rect).Round();
                }
            }

            // Moves child to another parent. This is intended to work only within same texture set.
            public void MoveChild(ChildTextureArea child, ParentTextureArea newParent)
            {
                Vector2[] newRelCoord = new Vector2[child.AbsCoord.Length];
                for (int i = 0; i < newRelCoord.Length; i++)
                {
                    newRelCoord[i] = child.AbsCoord[i] - newParent.Area.Start;
                }

                newParent.Children.Add(new ChildTextureArea()
                {
                    TexInfoIndex = child.TexInfoIndex,
                    BlendMode = child.BlendMode,
                    IsForTriangle = child.IsForTriangle,
                    RelCoord = newRelCoord,
                    AbsCoord = child.AbsCoord
                });
            }

            // Moves child to another parent by absolute coordinate. This is intended to work between texture sets.
            public void MoveChildWithoutRepositioning(ChildTextureArea child, ParentTextureArea newParent)
            {
                Vector2[] newAbsCoord = new Vector2[child.AbsCoord.Length];
                for (int i = 0; i < newAbsCoord.Length; i++)
                {
                    newAbsCoord[i] = child.RelCoord[i] + newParent.Area.Start;
                }

                newParent.Children.Add(new ChildTextureArea()
                {
                    TexInfoIndex = child.TexInfoIndex,
                    BlendMode = child.BlendMode,
                    IsForTriangle = child.IsForTriangle,
                    RelCoord = child.RelCoord,
                    AbsCoord = newAbsCoord
                });
            }

            public void MergeParents(List<ParentTextureArea> parentList, List<ParentTextureArea> parents)
            {
                foreach (ParentTextureArea parent in parents)
                {
                    Area = Area.Union(parent.Area);
                    ForSortedTransparency |= parent.ForSortedTransparency;

                    foreach (ChildTextureArea child in parent.Children)
                    {
                        parent.MoveChild(child, this);
                    }

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
                ParentAnimatedTexture result = new ParentAnimatedTexture(Origin)
                {
                    CompiledAnimation = new List<ParentTextureArea>()
                };

                foreach (ParentTextureArea parent in CompiledAnimation)
                {
                    var newParent = new ParentTextureArea(parent.Area, parent.Texture, parent.Destination);

                    foreach (ChildTextureArea child in parent.Children)
                    {
                        ChildTextureArea newChild = new ChildTextureArea()
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
            object ICloneable.Clone()
            {
                return Clone();
            }

            public AnimatedTextureSet Origin;
            public List<ParentTextureArea> CompiledAnimation = new List<ParentTextureArea>();

            public ParentAnimatedTexture(AnimatedTextureSet origin)
            {
                Origin = origin;
            }

            public int FrameCount()
            {
                int result = 0;
                foreach (ParentTextureArea parent in CompiledAnimation)
                {
                    result += parent.Children.Count;
                }

                return result;
            }
        }

        public class ObjectTexture
        {
            public int Tile;
            public Vector2[] TexCoord = new Vector2[4];
            public ushort UVAdjustmentFlag;

            public bool IsForTriangle;
            public TextureDestination Destination;
            public BlendMode BlendMode;
            public BumpMappingLevel BumpLevel;

            public int AtlasIndex;
            public VectorInt2 AtlasDimensions;
            public VectorInt2 PositionInAtlas;

            public ObjectTexture(ParentTextureArea parent, ChildTextureArea child, float maxTextureSize)
            {
                BlendMode = child.BlendMode;
                BumpLevel = parent.BumpLevel();
                Destination = parent.Destination;
                IsForTriangle = child.IsForTriangle;
                Tile = parent.Page;
                UVAdjustmentFlag = (ushort)TextureExtensions.GetTextureShapeType(child.RelCoord, IsForTriangle);

                for (int i = 0; i < child.RelCoord.Length; i++)
                {
                    Vector2 coord = new Vector2(child.RelCoord[i].X + (parent.PositionInPage.X + parent.Padding[0]),
                                                child.RelCoord[i].Y + (parent.PositionInPage.Y + parent.Padding[1]));

                    // Clamp coordinates that are possibly out of bounds
                    coord.X = MathC.Clamp(coord.X, 0, maxTextureSize);
                    coord.Y = MathC.Clamp(coord.Y, 0, maxTextureSize);

                    AtlasIndex = parent.AtlasIndex;
                    AtlasDimensions = parent.AtlasDimensions;
                    PositionInAtlas = parent.PositionInAtlas;

                    int atlasX = PositionInAtlas.X;
                    int atlasY = PositionInAtlas.Y;

                    // Float coordinates must be in 0.0f ... 1.0f range
                    TexCoord[i] = coord + new Vector2(atlasX * AtlasSize, atlasY * AtlasSize);
                    TexCoord[i].X /= (float)AtlasDimensions.X;
                    TexCoord[i].Y /= (float)AtlasDimensions.Y;
                }

                if (child.IsForTriangle)
                {
                    TexCoord[3] = TexCoord[2];
                }
            }

            public Rectangle2 GetRect()
            {
                if (IsForTriangle)
                {
                    return Rectangle2.FromCoordinates(TexCoord[0], TexCoord[1], TexCoord[2]);
                }
                else
                {
                    return Rectangle2.FromCoordinates(TexCoord[0], TexCoord[1], TexCoord[2], TexCoord[3]);
                }
            }
        }

        public TombEngineTexInfoManager(Level level, IProgressReporter progressReporter, int maxTileSize = -1)
        {
            _level = level;
            _padding = (ushort)level.Settings.TexturePadding;
            _progressReporter = progressReporter;

            if (maxTileSize > 0 && MathC.IsPowerOf2(maxTileSize))
            {
                MaxTileSize = (ushort)maxTileSize;
            }
            else
            {
                MaxTileSize = AtlasSize;
            }

            GenerateAnimLookups(_level.Settings.AnimatedTextureSets);  // Generate anim texture lookup table
            _generateTexInfos = true;    // Set manager ready state 
        }

        // Gets free TexInfo index
        private int GetNewTexInfoIndex()
        {
            if (_generateTexInfos)
            {
                int result = TexInfoCount;
                TexInfoCount++;
                return result;
            }
            else
            {
                return _dummyTexInfo;
            }
        }

        // Try to add texture to existing parent(s) either as a child of one, or as a parent, merging
        // enclosed parents.

        private bool TryToAddToExisting(TextureArea texture, List<ParentTextureArea> parentList, TextureDestination destination, bool isForTriangle, bool requireSort, int animFrameIndex = -1)
        {
            // Try to find potential parent (larger texture) and add itself to children
            foreach (ParentTextureArea parent in parentList)
            {
                if (!parent.IsPotentialParent(texture, destination, animFrameIndex >= 0, MaxTileSize))
                {
                    continue;
                }

                parent.AddChild(texture, animFrameIndex >= 0 ? animFrameIndex : GetNewTexInfoIndex(), isForTriangle, requireSort);
                return true;
            }

            // Try to find and merge parents which are enclosed in incoming texture area
            List<ParentTextureArea> childrenWannabes = parentList.Where(item => item.IsPotentialChild(texture, destination)).ToList();
            if (childrenWannabes.Count > 0)
            {
                ParentTextureArea newParent = new ParentTextureArea(texture, destination);
                newParent.AddChild(texture, animFrameIndex >= 0 ? animFrameIndex : GetNewTexInfoIndex(), isForTriangle, requireSort);
                newParent.MergeParents(parentList, childrenWannabes);
                parentList.Add(newParent);
                return true;
            }

            // No success
            return false;
        }

        public struct Result
        {
            // TexInfoIndex is saved as int for forward compatibility with engines such as TombEngine.
            public int TexInfoIndex;

            // Rotation value indicate that incoming TextureArea should be rotated N times. 
            // This approach allows to tightly pack TexInfos in same manner as tom2pc does.
            // As result, CreateFace3/4 should return a face with changed index order.
            public byte Rotation;

            public bool Animated;
            public int AnimatedSequence;
            public int AnimatedFrame;

            public TombEnginePolygon CreateTombEnginePolygon3(int[] indices, byte blendMode, List<TombEngineVertex> vertices)
            {
                if (indices.Length != 3)
                {
                    throw new ArgumentOutOfRangeException(nameof(indices.Length));
                }

                int objectTextureIndex = TexInfoIndex;
                int[] transformedIndices = new int[3] { indices[0], indices[1], indices[2] };

                if (Rotation > 0)
                {
                    for (int i = 0; i < Rotation; i++)
                    {
                        int tempIndex = transformedIndices[0];
                        transformedIndices[0] = transformedIndices[2];
                        transformedIndices[2] = transformedIndices[1];
                        transformedIndices[1] = tempIndex;
                    }
                }

                TombEnginePolygon polygon = new TombEnginePolygon
                {
                    Shape = TombEnginePolygonShape.Triangle
                };
                polygon.Indices.AddRange(transformedIndices);
                polygon.TextureId = objectTextureIndex;
                polygon.BlendMode = blendMode;
                polygon.Animated = Animated;

                if (vertices != null)
                {
                    // Calculate the normal
                    Vector3 e1 = vertices[polygon.Indices[1]].Position - vertices[polygon.Indices[0]].Position;
                    Vector3 e2 = vertices[polygon.Indices[2]].Position - vertices[polygon.Indices[0]].Position;
                    polygon.Normal = Vector3.Normalize(Vector3.Cross(e1, e2));
                }

                return polygon;
            }

            public TombEnginePolygon CreateTombEnginePolygon4(int[] indices, byte blendMode, List<TombEngineVertex> vertices)
            {
                if (indices.Length != 4)
                {
                    throw new ArgumentOutOfRangeException(nameof(indices.Length));
                }

                int objectTextureIndex = TexInfoIndex;
                int[] transformedIndices = new int[4] { indices[0], indices[1], indices[2], indices[3] };

                if (Rotation > 0)
                {
                    for (int i = 0; i < Rotation; i++)
                    {
                        int tempIndex = transformedIndices[0];
                        transformedIndices[0] = transformedIndices[3];
                        transformedIndices[3] = transformedIndices[2];
                        transformedIndices[2] = transformedIndices[1];
                        transformedIndices[1] = tempIndex;
                    }
                }

                TombEnginePolygon polygon = new TombEnginePolygon
                {
                    Shape = TombEnginePolygonShape.Quad
                };
                polygon.Indices.AddRange(transformedIndices);
                polygon.TextureId = objectTextureIndex;
                polygon.BlendMode = blendMode;
                polygon.Animated = Animated;

                if (vertices != null)
                {
                    // Calculate the normal
                    Vector3 e1 = vertices[polygon.Indices[1]].Position - vertices[polygon.Indices[0]].Position;
                    Vector3 e2 = vertices[polygon.Indices[2]].Position - vertices[polygon.Indices[0]].Position;
                    polygon.Normal = Vector3.Normalize(Vector3.Cross(e1, e2));
                }

                return polygon;
            }
        }

        // Gets existing TexInfo child index if there is similar one in parent textures list
        private Result? GetTexInfo(TextureArea areaToLook, List<ParentTextureArea> parentList, TextureDestination destination,
                                   bool isForTriangle, bool forSortedTransparency,
                                   bool checkParameters = true, bool scanOtherSets = false, float lookupMargin = 0.0f)
        {
            Vector2[] lookupCoordinates = new Vector2[isForTriangle ? 3 : 4];
            for (int i = 0; i < lookupCoordinates.Length; i++)
            {
                lookupCoordinates[i] = areaToLook.GetTexCoord(i);
            }

            foreach (ParentTextureArea parent in parentList)
            {
                // Parents with different attributes are quickly discarded
                if (!parent.ParametersSimilar(areaToLook, destination))
                {
                    // Try to identify if similar texture info from another texture set is present 
                    // by checking hash of the image area. If match is found, substitute lookup coordinates.

                    if (!scanOtherSets)
                    {
                        continue;
                    }

                    TextureArea? sr = parent.TextureSimilar(areaToLook);
                    if (!sr.HasValue)
                    {
                        continue;
                    }

                    for (int i = 0; i < lookupCoordinates.Length; i++)
                    {
                        lookupCoordinates[i] = sr.Value.GetTexCoord(i);
                    }
                }

                // Extract each children's absolute coordinates and compare them to incoming texture coordinates.
                foreach (ChildTextureArea child in parent.Children)
                {
                    // If parameters are different, children is quickly discarded from comparison.
                    if ((checkParameters && areaToLook.BlendMode != child.BlendMode) || child.IsForTriangle != isForTriangle)
                    {
                        continue;
                    }

                    // Test if coordinates are mutually equal and return resulting rotation if they are
                    int result = TestUVSimilarity(child.AbsCoord, lookupCoordinates, lookupMargin);
                    if (result != _noTexInfo)
                    {
                        // Refresh topmost flag, as same texture may be applied to faces with different topmost priority
                        parent.ForSortedTransparency |= forSortedTransparency;

                        // Refresh parent area (only in case it's from the same texture set, otherwise clashes are possible)
                        if (areaToLook.Texture == parent.Texture && !areaToLook.ParentArea.IsZero)
                        {
                            parent.Area = areaToLook.ParentArea;
                        }

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
                {
                    for (int j = 0; j < second.Length; j++)
                    {
                        int shift = (j + i) % second.Length;

                        if (!MathC.WithinEpsilon(first[j].X, second[shift].X, lookupMargin) ||
                            !MathC.WithinEpsilon(first[j].Y, second[shift].Y, lookupMargin))
                        {
                            break;
                        }

                        //Comparison was successful
                        if (j == second.Length - 1)
                        {
                            return i == 0 ? 0 : second.Length - i;
                        }
                    }
                }
            }
            return _noTexInfo;
        }

        // Generate new parent with incoming texture and immediately add incoming texture as a child

        private void AddParent(TextureArea texture, List<ParentTextureArea> parentList, TextureDestination destination, bool isForTriangle, bool requireSort, int frameIndex = -1)
        {
            ParentTextureArea newParent = new ParentTextureArea(texture, destination);
            parentList.Add(newParent);
            newParent.AddChild(texture, frameIndex >= 0 ? frameIndex : GetNewTexInfoIndex(), isForTriangle, requireSort);
        }

        // Only exposed variation of AddTexture that should be used outside of TexInfoManager itself

        public Result AddTexture(TextureArea texture, TextureDestination destination, bool isForTriangle, bool forSortedTransparency)
        {
            if (_dataHasBeenLaidOut)
            {
                throw new InvalidOperationException("Data has been already laid out for this TexInfoManager. Reinitialize it if you want to restart texture collection.");
            }

            // Only try to remap animated textures if fast mode is disabled
            bool remapAnimatedTextures = _level.Settings.RemapAnimatedTextures && !_level.Settings.FastMode;

            TextureArea refQuad = texture;

            // Try to compare incoming texture with existing anims and return animation frame
            if (_actualAnimTextures.Count > 0)
            {
                foreach (ParentAnimatedTexture actualTex in _actualAnimTextures)
                {
                    Result? existing = GetTexInfo(texture, actualTex.CompiledAnimation, destination, isForTriangle, forSortedTransparency, true, remapAnimatedTextures, _animTextureLookupMargin);
                    if (existing.HasValue)
                    {
                        Result result = new Result { Rotation = existing.Value.Rotation, TexInfoIndex = existing.Value.TexInfoIndex, Animated = true };
                        return result;
                    }
                }
            }

            // Now try to compare incoming texture with lookup anim seq table
            if (_referenceAnimTextures.Count > 0)
            {
                foreach (ParentAnimatedTexture refTex in _referenceAnimTextures)
                {
                    // If reference set found, generate actual one and immediately return fresh result
                    if (GetTexInfo(texture, refTex.CompiledAnimation, destination, isForTriangle, false, forSortedTransparency, remapAnimatedTextures, _animTextureLookupMargin).HasValue)
                    {
                        GenerateAnimTexture(refTex, refQuad, destination, isForTriangle);
                        Result result = AddTexture(texture, destination, isForTriangle, forSortedTransparency);
                        {
                            result.Animated = true;
                        }
                        return new Result() { Rotation = result.Rotation, TexInfoIndex = result.TexInfoIndex, Animated = true };
                    }
                }
            }

            // No animated textures identified, add texture as ordinary one
            return AddTexture(texture, _parentTextures, destination, isForTriangle, forSortedTransparency);
        }

        // Internal AddTexture variation which is capable of adding texture to various ParentTextureArea lists
        // with customizable parameters.
        // If animFrameIndex == -1, it means that ordinary texture is added, otherwise it indicates that specific anim
        // texture frame is being processed. If so, frame index is saved into TexInfoIndex field of resulting child.
        // Later on, on real anim texture creation, this index is used to sort frames in proper order.

        private Result AddTexture(TextureArea texture, List<ParentTextureArea> parentList, TextureDestination destination, bool isForTriangle, bool forSortedTransparency, int animFrameIndex = -1, bool makeCanonical = true)
        {
            // In case AddTexture is used with animated seq packing, we don't check frames for full similarity, because
            // frames can be duplicated with Repeat function or simply because of complex animator functions applied.
            Result? result = animFrameIndex >= 0 ? null : GetTexInfo(texture, parentList, destination, isForTriangle, forSortedTransparency);

            if (!result.HasValue)
            {
                // Try to create new canonical (top-left-based) texture as child or parent.
                // makeCanonical parameter is necessary for animated textures, because animators may produce frames
                // with non-canonically rotated coordinates (e.g. spin animator).
                TextureArea canonicalTexture = makeCanonical ? texture.GetCanonicalTexture(isForTriangle) : texture;

                // If no any potential parents or children, create as new parent
                if (!TryToAddToExisting(canonicalTexture, parentList, destination, isForTriangle, forSortedTransparency, animFrameIndex))
                {
                    AddParent(canonicalTexture, parentList, destination, isForTriangle, forSortedTransparency, animFrameIndex);
                }

                // Try again to get texinfo
                if (animFrameIndex >= 0)
                {
                    result = new Result { TexInfoIndex = _dummyTexInfo, Rotation = 0 };
                }
                else
                {
                    result = GetTexInfo(texture, parentList, destination, isForTriangle, forSortedTransparency);
                }
            }

            if (!result.HasValue)
            {
                _progressReporter.ReportWarn("Texture info manager couldn't fit texture into parent list. Please send your project to developers.");
                return new Result() { TexInfoIndex = _dummyTexInfo, Rotation = 0 };
            }
            else
            {
                return result.Value;
            }
        }

        // Scan and set alpha-test blending mode for opaque textures.
        // To speed up the process, all children whose parent region contains alpha, is also marked as
        // alpha.
        private void SortOutAlpha(List<ParentTextureArea> parentList)
        {
            Parallel.For(0, parentList.Count, i =>
            {
                IEnumerable<ChildTextureArea> opaqueChildren = parentList[i].Children.Where(child => child.BlendMode < BlendMode.Additive);
                if (opaqueChildren.Count() > 0)
                {
                    BlendMode realBlendMode = parentList[i].Texture.Image.HasAlpha(TRVersion.Game.TombEngine,
                        (int)parentList[i].Area.X0,
                        (int)parentList[i].Area.Y0,
                        (int)parentList[i].Area.Width,
                        (int)parentList[i].Area.Height);

                    if (realBlendMode != BlendMode.Normal)
                    {
                        foreach (ChildTextureArea children in opaqueChildren)
                        {
                            children.BlendMode = realBlendMode;
                        }
                    }
                }
            });
        }

        // Generates list of dummy lookup animated textures.

        private void GenerateAnimLookups(List<AnimatedTextureSet> sets)
        {
            foreach (AnimatedTextureSet set in sets)
            {
                // Ignore trivial (single-frame non-UVRotated) anims
                if (set.AnimationIsTrivial)
                {
                    continue;
                }

                int triangleVariation = 0;
                bool mirroredVariation = false;

                // Create all possible versions of current animation, including
                // mirrored and rotated ones. Later on, when parsing actual TextureAreas
                // from faces, we will compare them with this "lookup table" and will be
                // able to quickly return desired variation ID without complicated in-place
                // calculations.

                while (true)
                {
                    ParentAnimatedTexture refAnim = new ParentAnimatedTexture(set);
                    int index = 0;

                    foreach (AnimatedTextureFrame frame in set.Frames)
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
                        {
                            newFrame.TexCoord3 = newFrame.TexCoord2;
                        }

                        // Mirror if needed
                        if (mirroredVariation)
                        {
                            newFrame.Mirror(triangleVariation > 0);
                        }

                        // Make frame, including repeat versions
                        for (int i = 0; i < frame.Repeat; i++)
                        {
                            AddTexture(newFrame, refAnim.CompiledAnimation, TextureDestination.RoomOrAggressive, (triangleVariation > 0), newFrame.BlendMode == BlendMode.AlphaBlend, index, set.IsUvRotate);
                            index++;
                        }
                    }

                    _referenceAnimTextures.Add(refAnim);

                    triangleVariation++;
                    if (triangleVariation > 4)
                    {
                        if (!mirroredVariation)
                        {
                            triangleVariation = 0;
                            mirroredVariation = true;
                        }
                        else
                        {
                            break;
                        }
                    }
                }
            }
        }

        // Generates real animated texture sequence from reference lookup.

        private void GenerateAnimTexture(ParentAnimatedTexture reference, TextureArea origin, TextureDestination destination, bool isForTriangle)
        {
            ParentAnimatedTexture refCopy = reference.Clone();
            foreach (ParentTextureArea parent in refCopy.CompiledAnimation)
            {
                parent.Destination = destination;

                foreach (ChildTextureArea child in parent.Children)
                {
                    child.BlendMode = origin.BlendMode;
                }
            }

            // Sort and assign TexInfo indices for frames by the order they were created in reference animation
            IOrderedEnumerable<ChildTextureArea> orderedFrameList = refCopy.CompiledAnimation.SelectMany(x => x.Children).OrderBy(c => c.TexInfoIndex);
            foreach (ChildTextureArea frame in orderedFrameList)
            {
                frame.TexInfoIndex = GetNewTexInfoIndex();
            }

            _actualAnimTextures.Add(refCopy);
        }

        // Finds visually similar image areas across all parents and unifies them. This step drastically
        // reduces texture page usage if lots of objects with duplicated textures are used. This process is lossy.

        private void CleanUp(ref List<ParentTextureArea> textures)
        {
            Dictionary<Hash, ParentTextureArea> result = new Dictionary<Hash, ParentTextureArea>();

            foreach (ParentTextureArea tex in textures) // Do not parallelize this. For some reason it breaks everything.
            {
                Hash bmpHash = Hash.FromByteArray(tex.Texture.Image.ToByteArray(tex.Area));

                if (result.ContainsKey(bmpHash))
                {
                    tex.Children.ForEach(child => tex.MoveChildWithoutRepositioning(child, result[bmpHash]));
                    if (tex.ForSortedTransparency)
                    {
                        result[bmpHash].ForSortedTransparency = true;
                    }
                }
                else
                {
                    result.TryAdd(bmpHash, tex);
                }
            }

            textures = result.Select(entry => entry.Value).ToList();
        }

        // Maps parent texture areas on the proposed texture map.
        // This step only prepares for actual image data layout, actual layout is done in BuildTextureMap.

        private int PlaceTexturesInMap(ref List<ParentTextureArea> textures, bool forceMinimumPadding = false)
        {
            if (textures.Count == 0)
            {
                return 0;
            }

            int currentPage = -1;
            List<RectPacker> texPackers = new List<RectPacker>();

            for (int i = 0; i < textures.Count; i++)
            {
                // Get the size of the quad surrounding texture area, typically should be the texture area itself
                int w = (int)(textures[i].Area.Width);
                int h = (int)(textures[i].Area.Height);

                // Calculate adaptive padding at all sides
                int padding = (_padding == 0 && forceMinimumPadding) ? _minimumPadding : _padding;

                int tP = padding;
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
                texPackers.Add(new RectPackerTree(new VectorInt2(AtlasSize, AtlasSize)));
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

        private List<TombEngineAtlas> CreateAtlas(ref List<ParentTextureArea> textures, int numPages, bool bump, bool forceMinimumPadding, int baseIndex)
        {
            var customBumpmaps = new Dictionary<string, ImageC>();
            var texturePages = new List<TexturePage>();

            for (int i = 0; i < numPages; i++)
            {
                texturePages.Add(new TexturePage { ColorMap = ImageC.CreateNew(AtlasSize, AtlasSize) });
            }

            int actualPadding = (_padding == 0 && forceMinimumPadding) ? _minimumPadding : _padding;

            for (int b = 0; b < (bump ? 2 : 1); b++)
            {
                for (int i = 0; i < textures.Count; i++)
                {
                    ParentTextureArea p = textures[i];

                    if (p.Texture == null || p.Texture.IsUnavailable)
                    {
                        _progressReporter.ReportWarn("Texture null: " + i);
                        continue;
                    }

                    int x = (int)p.Area.Start.X;
                    int y = (int)p.Area.Start.Y;
                    int width = (int)p.Area.Width;
                    int height = (int)p.Area.Height;

                    TexturePage image = texturePages[p.Page];

                    int destX = p.PositionInPage.X + p.Padding[0];
                    int destY = p.PositionInPage.Y + p.Padding[1];

                    image.ColorMap.CopyFrom(destX, destY, p.Texture.Image, x, y, width, height);
                    AddPadding(p, p.Texture.Image, image.ColorMap, 0, actualPadding);

                    // Do the bump map if needed

                    if (p.Texture is LevelTexture && b == 1)
                    {
                        LevelTexture tex = (p.Texture as LevelTexture);
                        int bumpX = destX;
                        int bumpY = destY;

                        // Try to copy custom bumpmaps
                        if (!string.IsNullOrEmpty(tex.BumpPath))
                        {
                            if (!customBumpmaps.ContainsKey(tex.BumpPath))
                            {
                                var path = _level.Settings.MakeAbsolute(tex.BumpPath);

                                if (!File.Exists(path))
                                {
                                    _progressReporter.ReportWarn("External bumpmap file " + path + " was not found!");
                                    customBumpmaps.Add(tex.BumpPath, ImageC.Black);
                                }
                                else
                                {
                                    ImageC potentialBumpImage = ImageC.FromFile(path);

                                    // Only assign bumpmap image if size is equal to texture image size, otherwise use dummy

                                    if (potentialBumpImage != null && potentialBumpImage.Size == tex.Image.Size)
                                    {
                                        customBumpmaps.Add(tex.BumpPath, potentialBumpImage);
                                    }
                                    else
                                    {
                                        _progressReporter.ReportWarn("Texture file '" + tex + "' has external bumpmap assigned which has different size and was ignored.");
                                        customBumpmaps.Add(tex.BumpPath, ImageC.Black);
                                    }
                                }
                            }

                            // Init the normal map if not done yet
                            if (!image.HasNormalMap)
                            {
                                image.HasNormalMap = true;
                                image.NormalMap = ImageC.CreateNew(AtlasSize, AtlasSize);
                            }
                            image.NormalMap.CopyFrom(bumpX, bumpY, customBumpmaps[tex.BumpPath], x, y, width, height);
                            AddPadding(p, image.NormalMap, image.NormalMap, 0, actualPadding, bumpX, bumpY);
                        }
                        else
                        {
                            BumpMappingLevel? level = tex.GetBumpMappingLevelFromTexCoord(p.Area.GetMid());

                            ImageC bumpImage = ImageC.CreateNew(width, height);
                            bumpImage.CopyFrom(0, 0, p.Texture.Image, x, y, width, height);

                            float sobelLevel = 0;
                            float sobelStrength = 0;

                            switch (level)
                            {
                                case BumpMappingLevel.Level1:
                                    sobelLevel = 0.004f;
                                    sobelStrength = 0.003f;
                                    break;
                                case BumpMappingLevel.Level2:
                                    sobelLevel = 0.007f;
                                    sobelStrength = 0.004f;
                                    break;
                                case BumpMappingLevel.Level3:
                                    sobelLevel = 0.009f;
                                    sobelStrength = 0.008f;
                                    break;
                            }

                            // Init the normal map if not done yet
                            if (!image.HasNormalMap)
                            {
                                image.HasNormalMap = true;
                                image.NormalMap = ImageC.CreateNew(AtlasSize, AtlasSize);
                            }

                            if (level != BumpMappingLevel.None)
                            {
                                bumpImage = ImageC.GrayScaleFilter(bumpImage, true, 0, 0, bumpImage.Width, bumpImage.Height);
                                bumpImage = ImageC.SobelFilter(bumpImage, sobelStrength, sobelLevel, SobelFilterType.Sobel, 0, 0, bumpImage.Width, bumpImage.Height);
                            }
                            else
                            {
                                // Neutral Bump
                                bumpImage.Fill(new ColorC(128, 128, 255, 255));
                            }

                            image.NormalMap.CopyFrom(bumpX, bumpY, bumpImage, 0, 0, width, height);
                            AddPadding(p, image.NormalMap, image.NormalMap, 0, actualPadding, bumpX, bumpY);
                        }
                    }
                }
            }

            // Calculate how many atlases we need
            List<TombEngineAtlas> atlasList = new List<TombEngineAtlas>();
            int totalPages = numPages;
            int numAtlases = (int)Math.Floor((float)totalPages / PagesPerAtlas);
            if (totalPages % PagesPerAtlas != 0)
            {
                numAtlases++;
            }

            // Build a list of all packed pages in the previous step
            List<TexturePage> pages = texturePages;

            TombEngineAtlas currentAtlas = new TombEngineAtlas();
            int atlasX = 0;
            int atlasY = 0;
            int pagesProcessed = 0;
            VectorInt2 pagesPerAtlas = VectorInt2.Zero;

            foreach (TexturePage page in pages)
            {
                // At first iteration atlas is null, so create it
                if (pagesProcessed == 0)
                {
                    pagesPerAtlas = new VectorInt2(PagesPerAtlas, PagesPerAtlas); // GetOptimalSizeForAtlas(pages.Count - pagesProcessed);
                    currentAtlas.ColorMap = ImageC.CreateNew(pagesPerAtlas.X * PageSize, pagesPerAtlas.Y * PageSize);
                    if (page.HasNormalMap)
                    {
                        currentAtlas.HasNormalMap = true;
                        currentAtlas.NormalMap = ImageC.CreateNew(pagesPerAtlas.X * PageSize, pagesPerAtlas.Y * PageSize);
                    }
                    atlasList.Add(currentAtlas);
                }

                // Time to go to the next row? 
                if (atlasX == pagesPerAtlas.X)
                {
                    atlasX = 0;
                    atlasY++;
                }

                // Is atlas full and we need a new one?
                if (atlasY == pagesPerAtlas.Y)
                {
                    atlasX = 0;
                    atlasY = 0;
                    pagesPerAtlas = new VectorInt2(PagesPerAtlas, PagesPerAtlas); // GetOptimalSizeForAtlas(pages.Count - pagesProcessed);
                    currentAtlas = new TombEngineAtlas
                    {
                        ColorMap = ImageC.CreateNew(pagesPerAtlas.X * PageSize, pagesPerAtlas.Y * PageSize)
                    };
                    if (page.HasNormalMap)
                    {
                        currentAtlas.HasNormalMap = true;
                        currentAtlas.NormalMap = ImageC.CreateNew(pagesPerAtlas.X * PageSize, pagesPerAtlas.Y * PageSize);
                    }
                    atlasList.Add(currentAtlas);
                }

                // Store the atlas index and position
                page.Atlas = baseIndex + atlasList.Count - 1;
                page.Position = new VectorInt2(atlasX, atlasY);

                // Copy the texture into the atlas
                currentAtlas.ColorMap.CopyFrom(atlasX * PageSize, atlasY * PageSize, page.ColorMap, 0, 0, PageSize, PageSize);
                if (page.HasNormalMap)
                {
                    currentAtlas.NormalMap.CopyFrom(atlasX * PageSize, atlasY * PageSize, page.NormalMap, 0, 0, PageSize, PageSize);
                }

                // Increment atlas position
                atlasX++;

                pagesProcessed++;
            }

            for (int i = 0; i < textures.Count; i++)
            {
                textures[i].AtlasIndex = texturePages[textures[i].Page].Atlas;
                textures[i].AtlasDimensions = atlasList[textures[i].AtlasIndex - baseIndex].ColorMap.Size;
                textures[i].PositionInAtlas = texturePages[textures[i].Page].Position;
            }

            return atlasList;
        }

        private VectorInt2 GetOptimalSizeForAtlas(int remainingPages)
        {
            if (remainingPages > PagesPerAtlas)
            {
                return new VectorInt2(PagesPerRowInAtlas, PagesPerRowInAtlas);
            }
            else
            {
                VectorInt2 size;
                size.X = (int)Math.Ceiling(Math.Sqrt(remainingPages));
                size.Y = (int)Math.Ceiling(remainingPages / (float)size.X);
                return size;
            }
        }

        // Expands edge pixels to create padding which prevents border bleeding problems.

        private void AddPadding(ParentTextureArea texture, ImageC from, ImageC to, int pageOffset, int padding, int? customX = null, int? customY = null)
        {
            ParentTextureArea p = texture;
            int x = customX.HasValue ? customX.Value : (int)p.Area.Start.X;
            int y = customY.HasValue ? customY.Value : (int)p.Area.Start.Y;
            int width = (int)p.Area.Width;
            int height = (int)p.Area.Height;
            int dataOffset = 0;

            // Add actual padding (ported code from OT bordered_texture_atlas.cpp)

            ColorC topLeft = from.GetPixel(x, y);
            ColorC topRight = from.GetPixel(x + width - 1, y);
            ColorC bottomLeft = from.GetPixel(x, y + height - 1);
            ColorC bottomRight = from.GetPixel(x + width - 1, y + height - 1);

            for (int xP = 0; xP < padding; xP++)
            {
                // copy left line
                if (xP < p.Padding[0])
                {
                    to.CopyFrom(p.PositionInPage.X + xP, dataOffset + p.PositionInPage.Y + p.Padding[1], from,
                               x, y, 1, height);
                }

                // copy right line
                if (xP < p.Padding[2])
                {
                    to.CopyFrom(p.PositionInPage.X + xP + width + p.Padding[0], dataOffset + p.PositionInPage.Y + p.Padding[1], from,
                               x + width - 1, y, 1, height);
                }

                for (int yP = 0; yP < padding; yP++)
                {
                    // copy top line
                    if (yP < p.Padding[1])
                    {
                        to.CopyFrom(p.PositionInPage.X + p.Padding[0], dataOffset + p.PositionInPage.Y + yP, from,
                                   x, y, width, 1);
                    }
                    // copy bottom line
                    if (yP < p.Padding[3])
                    {
                        to.CopyFrom(p.PositionInPage.X + p.Padding[0], dataOffset + p.PositionInPage.Y + yP + height + p.Padding[1], from,
                                   x, y + height - 1, width, 1);
                    }

                    // expand top-left pixel
                    if (xP < p.Padding[0] && yP < p.Padding[1])
                    {
                        to.SetPixel(p.PositionInPage.X + xP, dataOffset + p.PositionInPage.Y + yP, topLeft);
                    }
                    // expand top-right pixel
                    if (xP < p.Padding[2] && yP < p.Padding[1])
                    {
                        to.SetPixel(p.PositionInPage.X + xP + width + p.Padding[0], dataOffset + p.PositionInPage.Y + yP, topRight);
                    }
                    // expand bottom-left pixel
                    if (xP < p.Padding[0] && yP < p.Padding[3])
                    {
                        to.SetPixel(p.PositionInPage.X + xP, dataOffset + p.PositionInPage.Y + yP + height + p.Padding[1], bottomLeft);
                    }
                    // expand bottom-right pixel
                    if (xP < p.Padding[2] && yP < p.Padding[3])
                    {
                        to.SetPixel(p.PositionInPage.X + xP + width + p.Padding[0], dataOffset + p.PositionInPage.Y + yP + height + p.Padding[1], bottomRight);
                    }
                }
            }
        }

        // Groups all textures by their attributes, cleans them up and builds actual texture data.

        public void LayOutAllData()
        {
            if (_dataHasBeenLaidOut)
            {
                return;
            }

            _dataHasBeenLaidOut = true;

            // Before any other action, lay out animated textures
            PrepareAnimatedTextures();

            // Subdivide textures in different blocks
            List<ParentTextureArea> roomTextures = new List<ParentTextureArea>();
            List<ParentTextureArea> moveablesTextures = new List<ParentTextureArea>();
            List<ParentTextureArea> staticsTextures = new List<ParentTextureArea>();
            List<List<ParentTextureArea>> animatedTextures = new List<List<ParentTextureArea>>();
            List<int> numAnimatedPages = new List<int>();

            for (int i = 0; i < _parentTextures.Count; i++)
            {
                if (_parentTextures[i].Destination == TextureDestination.RoomOrAggressive)
                {
                    roomTextures.Add(_parentTextures[i]);
                }
                else if (_parentTextures[i].Destination == TextureDestination.Moveable)
                {
                    moveablesTextures.Add(_parentTextures[i]);
                }
                else
                {
                    staticsTextures.Add(_parentTextures[i]);
                }
            }

            for (int n = 0; n < _actualAnimTextures.Count; n++)
            {
                List<ParentTextureArea> parentTextures = _actualAnimTextures[n].CompiledAnimation;

                animatedTextures.Add(new List<ParentTextureArea>());
                numAnimatedPages.Add(0);

                for (int i = 0; i < parentTextures.Count; i++)
                {
                    animatedTextures[n].Add(parentTextures[i]);
                }
            }

            // Cleanup duplicated parent areas.
            if (!_level.Settings.FastMode)
            {
                CleanUp(ref roomTextures);
                CleanUp(ref moveablesTextures);
                CleanUp(ref staticsTextures);
                for (int n = 0; n < _actualAnimTextures.Count; n++)
                {
                    List<ParentTextureArea> textures = animatedTextures[n];
                    CleanUp(ref textures);
                }
            }

            // Sort textures by size and blend mode
            roomTextures = roomTextures
                .OrderBy(item => item.ForSortedTransparency)
                .ThenByDescending(item => item.Area.Size.X * item.Area.Size.Y).ToList();
           
            moveablesTextures = moveablesTextures
                .OrderBy(item => item.ForSortedTransparency)
                .ThenByDescending(item => item.Area.Size.X * item.Area.Size.Y).ToList();
         
            staticsTextures = staticsTextures
                .OrderBy(item => item.ForSortedTransparency)
                .ThenByDescending(item => item.Area.Size.X * item.Area.Size.Y).ToList();

            // Calculate new X, Y of each texture area
            NumRoomPages = PlaceTexturesInMap(ref roomTextures);
            NumMoveablesPages = PlaceTexturesInMap(ref moveablesTextures, true);
            NumStaticsPages = PlaceTexturesInMap(ref staticsTextures, true);
            for (int n = 0; n < numAnimatedPages.Count; n++)
            {
                List<ParentTextureArea> textures = animatedTextures[n];
                numAnimatedPages[n] = PlaceTexturesInMap(ref textures, true);
            }

            // In TombEngine, we have only 4K texture atlases
            // We pack pages like in old games, but then we pack them quickly in big atlases
            RoomsAtlas = CreateAtlas(ref roomTextures, NumRoomPages, true, false, 0);
            MoveablesAtlas = CreateAtlas(ref moveablesTextures, NumMoveablesPages, false, true, 0);
            StaticsAtlas = CreateAtlas(ref staticsTextures, NumStaticsPages, false, true, 0);
            AnimatedAtlas = new List<TombEngineAtlas>();

            for (int n = 0; n < numAnimatedPages.Count; n++)
            {
                List<ParentTextureArea> textures = animatedTextures[n];
                AnimatedAtlas.AddRange(CreateAtlas(ref textures, numAnimatedPages[n], false, false, AnimatedAtlas.Count));
            }

#if DEBUG
            for (int n = 0; n < RoomsAtlas.Count; n++)
            {
                RoomsAtlas[n].ColorMap.Save("RoomsAtlas" + n + ".png");
            }

            for (int n = 0; n < MoveablesAtlas.Count; n++)
            {
                MoveablesAtlas[n].ColorMap.Save("MoveablesAtlas" + n + ".png");
            }

            for (int n = 0; n < StaticsAtlas.Count; n++)
            {
                StaticsAtlas[n].ColorMap.Save("StaticsAtlas" + n + ".png");
            }
#endif

            // Finally compile all texinfos
            BuildTextureInfos();
        }

        // Compiles all final texture infos into final list to be written into level file.

        private void BuildTextureInfos()
        {
            float maxSize = MaxTileSize - (1.0f / (AtlasSize - 1));

            _objectTextures = new SortedDictionary<int, ObjectTexture>();

            SortOutAlpha(_parentTextures);
            foreach (ParentTextureArea parent in _parentTextures)
            {
                foreach (ChildTextureArea child in parent.Children)
                {
                    if (!_objectTextures.ContainsKey(child.TexInfoIndex))
                    {
                        ObjectTexture newObjectTexture = new ObjectTexture(parent, child, maxSize);

                        if (newObjectTexture.TexCoord[0] == newObjectTexture.TexCoord[1] ||
                            newObjectTexture.TexCoord[0] == newObjectTexture.TexCoord[2] ||
                            newObjectTexture.TexCoord[1] == newObjectTexture.TexCoord[2] ||
                            newObjectTexture.TexCoord[0].X < 0 || newObjectTexture.TexCoord[0].Y < 0 ||
                            newObjectTexture.TexCoord[1].X < 0 || newObjectTexture.TexCoord[1].Y < 0 ||
                            newObjectTexture.TexCoord[2].X < 0 || newObjectTexture.TexCoord[2].Y < 0 ||
                            (!child.IsForTriangle && newObjectTexture.TexCoord[0] == newObjectTexture.TexCoord[3]) ||
                            (!child.IsForTriangle && newObjectTexture.TexCoord[1] == newObjectTexture.TexCoord[3]) ||
                            (!child.IsForTriangle && newObjectTexture.TexCoord[2] == newObjectTexture.TexCoord[3]) ||
                            (!child.IsForTriangle && (newObjectTexture.TexCoord[3].X < 0 || newObjectTexture.TexCoord[3].Y < 0)))
                        {
                            _progressReporter.ReportWarn("Compiled TexInfo " + child.TexInfoIndex + " is broken, coordinates are invalid.");
                        }

                        _objectTextures.Add(child.TexInfoIndex, newObjectTexture);
                    }
                }
            }

            foreach (ParentAnimatedTexture animTexture in _actualAnimTextures)
            {
                SortOutAlpha(animTexture.CompiledAnimation);
                foreach (ParentTextureArea parent in animTexture.CompiledAnimation)
                {
                    foreach (ChildTextureArea child in parent.Children)
                    {
                        if (!_objectTextures.ContainsKey(child.TexInfoIndex))
                        {
                            _objectTextures.Add(child.TexInfoIndex, new ObjectTexture(parent, child, maxSize));
                        }
                    }
                }
            }
        }

        // Sorts animated textures and prepares index table to layout them later.
        // We need to build index table in advance of CleanUp() call, because after that step animated texture order
        // gets shuffled.

        private void PrepareAnimatedTextures()
        {
            // Put UVRotate sequences first
            _actualAnimTextures = _actualAnimTextures.OrderBy(item => !item.Origin.IsUvRotate).ToList();

            // Build index table
            _animTextureIndices = new List<List<ushort>>();
            foreach (ParentAnimatedTexture compiledAnimatedTexture in _actualAnimTextures)
            {
                List<ushort> list = new List<ushort>();
                List<ChildTextureArea> orderedFrameList = compiledAnimatedTexture.CompiledAnimation.SelectMany(x => x.Children).OrderBy(c => c.TexInfoIndex).ToList();
                foreach (ChildTextureArea frame in orderedFrameList)
                {
                    list.Add((ushort)frame.TexInfoIndex);
                }

                _animTextureIndices.Add(list);
            }
        }

        public void WriteAnimatedTextures(BinaryWriterEx writer)
        {
            writer.Write(_actualAnimTextures.Count);
            for (int i = 0; i < _actualAnimTextures.Count; i++)
            {
                List<ParentTextureArea> sequence = _actualAnimTextures[i].CompiledAnimation;

                writer.Write(i);
                writer.Write((int)_actualAnimTextures[i].Origin.Fps);
                writer.Write(_animTextureIndices[i].Count); // Number of frames
                foreach (ushort frame in _animTextureIndices[i])
                {
                    ObjectTexture texture = _objectTextures[frame];

                    // Coordinates of each frame
                    writer.Write(texture.TexCoord[0].X);
                    writer.Write(texture.TexCoord[0].Y);
                    writer.Write(texture.TexCoord[1].X);
                    writer.Write(texture.TexCoord[1].Y);
                    writer.Write(texture.TexCoord[2].X);
                    writer.Write(texture.TexCoord[2].Y);
                    writer.Write(texture.TexCoord[3].X);
                    writer.Write(texture.TexCoord[3].Y);
                }
            }
        }

        public void WriteTextureInfosTombEngine(BinaryWriterEx writer, Level level)
        {
            writer.Write(_objectTextures.Count);
            for (int i = 0; i < _objectTextures.Count; i++)
            {
                ObjectTexture texture = _objectTextures.ElementAt(i).Value;

                // Tile and flags
                int tile = texture.AtlasIndex;
                if (texture.IsForTriangle)
                {
                    tile |= 0x8000;
                }

                // Blend mode
                int attribute = (int)texture.BlendMode;

                // Now write the texture
                writer.Write(attribute);
                writer.Write(tile);

                // Built-in TR4-5 mapping correction is not used. Dummy mapping type is used
                // together with compensation coordinate distortion.
                int newFlags = texture.UVAdjustmentFlag;

                if (texture.Destination == TextureDestination.RoomOrAggressive)
                {
                    newFlags |= 0x8000;
                }

                if (texture.BumpLevel == BumpMappingLevel.Level1)
                {
                    newFlags |= (1 << 9);
                }
                else if (texture.BumpLevel == BumpMappingLevel.Level2)
                {
                    newFlags |= (2 << 9);
                }
                else if (texture.BumpLevel == BumpMappingLevel.Level3)
                {
                    newFlags |= (3 << 9);
                }

                writer.Write(newFlags);

                for (int j = 0; j < 4; j++)
                {
                    if (texture.IsForTriangle && j == 3)
                    {
                        writer.Write((float)0);
                        writer.Write((float)0);
                    }
                    else
                    {
                        writer.Write(texture.TexCoord[j].X);
                        writer.Write(texture.TexCoord[j].Y);
                    }
                }

                writer.Write((int)texture.Destination);
            }
        }

        public void UpdateTiles(int numSpritesPages)
        {
            Parallel.For(0, _objectTextures.Count, i =>
            {
                ObjectTexture texture = _objectTextures.ElementAt(i).Value;
                if (texture.Destination == TextureDestination.RoomOrAggressive && texture.BumpLevel == BumpMappingLevel.None)
                {
                    // Tile is OK
                }
                else if (texture.Destination != TextureDestination.RoomOrAggressive)
                {
                    texture.Tile += NumRoomPages;
                }
                else if (texture.Destination == TextureDestination.RoomOrAggressive && texture.BumpLevel != BumpMappingLevel.None)
                {
                    texture.Tile += NumRoomPages + NumObjectsPages + numSpritesPages;
                }
            });

            NumObjectsPages += numSpritesPages;
        }

        public List<ObjectTexture> GetObjectTextures()
        {
            return _objectTextures.Values.ToList();
        }

        public Tuple<int, int> GetAnimatedTexture(int tid)
        {
            for (int i = 0; i < _animTextureIndices.Count; i++)
            {
                for (int j = 0; j < _animTextureIndices[i].Count; j++)
                {
                    if (_animTextureIndices[i][j] == tid)
                    {
                        return new Tuple<int, int>(i, j);
                    }
                }
            }

            return null;
        }

    }
}
