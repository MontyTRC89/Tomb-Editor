using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Runtime.CompilerServices;
using TombLib.LevelData.SectorEnums;
using TombLib.LevelData.SectorEnums.Extensions;
using TombLib.LevelData.SectorStructs;
using TombLib.Utils;

namespace TombLib.LevelData
{
    [Serializable]
    public class Block : ICloneable
    {
        public const int MAX_EXTRA_SUBDIVISIONS = 8;
        public static Block Empty { get; } = new Block();

        public BlockType Type { get; set; } = BlockType.Floor;
        public BlockFlags Flags { get; set; } = BlockFlags.None;
        public bool ForceFloorSolid { get; set; } // If this is set to true, portals are overwritten for this sector.
        public List<Subdivision> ExtraFloorSubdivisions { get; } = new();
        public List<Subdivision> ExtraCeilingSubdivisions { get; } = new();
        private Dictionary<BlockFace, TextureArea> _faceTextures { get; } = new();

        public BlockSurface Floor;
        public BlockSurface Ceiling;

        public List<TriggerInstance> Triggers { get; } = new List<TriggerInstance>(); // This array is not supposed to be modified here.
        public PortalInstance FloorPortal { get; internal set; } = null; // This is not supposed to be modified here.
        public PortalInstance WallPortal { get; internal set; } = null; // This is not supposed to be modified here.
        public PortalInstance CeilingPortal { get; internal set; } = null; // This is not supposed to be modified here.

        public GhostBlockInstance GhostBlock { get; internal set; } = null; // If exists, adds invisible geometry to sector.

        private Block()
        { }

        public Block(int floor, int ceiling)
        {
            Floor.XnZp = Floor.XpZp = Floor.XpZn = Floor.XnZn = floor;
            Ceiling.XnZp = Ceiling.XpZp = Ceiling.XpZn = Ceiling.XnZn = ceiling;
        }

        public Block Clone()
        {
            var result = new Block
            {
                Type = Type,
                Flags = Flags,
                ForceFloorSolid = ForceFloorSolid,
                Floor = Floor,
                Ceiling = Ceiling
            };

            foreach (KeyValuePair<BlockFace, TextureArea> entry in _faceTextures)
                result._faceTextures[entry.Key] = entry.Value;

            foreach (Subdivision subdivision in ExtraFloorSubdivisions)
                result.ExtraFloorSubdivisions.Add((Subdivision)subdivision.Clone());

            foreach (Subdivision subdivision in ExtraCeilingSubdivisions)
                result.ExtraCeilingSubdivisions.Add((Subdivision)subdivision.Clone());

            return result;
        }

        object ICloneable.Clone() => Clone();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool HasFlag(BlockFlags flag)
        {
            return (Flags & flag) == flag;
        }

        public bool IsFullWall => Type == BlockType.BorderWall || (Type == BlockType.Wall && Floor.DiagonalSplit == DiagonalSplit.None);
        public bool IsAnyWall => Type != BlockType.Floor;
        public bool IsAnyPortal => FloorPortal != null || CeilingPortal != null || WallPortal != null;
        public bool HasGhostBlock => GhostBlock != null;

        public bool SetFaceTexture(BlockFace face, TextureArea texture)
        {
            if (texture == TextureArea.None)
            {
                if (_faceTextures.ContainsKey(face))
                    _faceTextures.Remove(face);

                return true;
            }

            if (texture.TextureIsDegenerate)
                texture = TextureArea.Invisible;

            if (!_faceTextures.ContainsKey(face) || _faceTextures[face] != texture)
            {
                _faceTextures[face] = texture;
                return true;
            }
            else
                return false;
        }

        public TextureArea GetFaceTexture(BlockFace face)
        {
            return _faceTextures.GetValueOrDefault(face);
        }

        public Dictionary<BlockFace, TextureArea> GetFaceTextures()
        {
            return _faceTextures;
        }

        public IEnumerable<BlockVertical> GetVerticals()
        {
            yield return BlockVertical.Floor;
            yield return BlockVertical.Ceiling;

            for (int i = 0; i < ExtraFloorSubdivisions.Count; i++)
                yield return BlockVerticalExtensions.GetExtraFloorSubdivision(i);

            for (int i = 0; i < ExtraCeilingSubdivisions.Count; i++)
                yield return BlockVerticalExtensions.GetExtraCeilingSubdivision(i);
        }

        public IEnumerable<PortalInstance> Portals
        {
            get
            {
                if (WallPortal != null)
                    yield return WallPortal;
                if (CeilingPortal != null)
                    yield return CeilingPortal;
                if (FloorPortal != null)
                    yield return FloorPortal;
            }
        }

        public void ReplaceGeometry(Level level, Block replacement)
        {
            if (Type != BlockType.BorderWall) Type = replacement.Type;

            Flags = replacement.Flags;
            ForceFloorSolid = replacement.ForceFloorSolid;

            foreach (BlockFace face in replacement.GetFaceTextures().Keys.Union(_faceTextures.Keys))
            {
                var texture = replacement.GetFaceTexture(face);
                if (texture.TextureIsInvisible || level.Settings.Textures.Contains(texture.Texture))
                    SetFaceTexture(face, texture);
            }

            Floor = replacement.Floor;
            Ceiling = replacement.Ceiling;

            ExtraFloorSubdivisions.Clear();
            ExtraFloorSubdivisions.AddRange(replacement.ExtraFloorSubdivisions);

            ExtraCeilingSubdivisions.Clear();
            ExtraCeilingSubdivisions.AddRange(replacement.ExtraCeilingSubdivisions);
        }

        public int GetHeight(BlockVertical vertical, BlockEdge edge)
        {
            switch (vertical)
            {
                case BlockVertical.Floor:
                    return Floor.GetHeight(edge);
                case BlockVertical.Ceiling:
                    return Ceiling.GetHeight(edge);
            }

            if (vertical.IsExtraFloorSubdivision())
            {
                int index = vertical.GetExtraSubdivisionIndex();
                Subdivision subdivision = ExtraFloorSubdivisions.ElementAtOrDefault(index);

                if (subdivision is null)
                    return int.MinValue;

                return subdivision.GetEdge(edge);
            }
            
            if (vertical.IsExtraCeilingSubdivision())
            {
                int index = vertical.GetExtraSubdivisionIndex();
                Subdivision subdivision = ExtraCeilingSubdivisions.ElementAtOrDefault(index);

                if (subdivision is null)
                    return int.MaxValue;

                return subdivision.GetEdge(edge);
            }

            throw new ArgumentOutOfRangeException();
        }

        public void SetHeight(BlockVertical vertical, BlockEdge edge, int newValue)
        {
            if (newValue is int.MinValue or int.MaxValue)
                return;

            switch (vertical)
            {
                case BlockVertical.Floor:
                    Floor.SetHeight(edge, newValue);
                    return;
                case BlockVertical.Ceiling:
                    Ceiling.SetHeight(edge, newValue);
                    return;
            }

            if (vertical.IsExtraFloorSubdivision())
            {
                Subdivision existingSubdivision = ExtraFloorSubdivisions.ElementAtOrDefault(vertical.GetExtraSubdivisionIndex());

                if (existingSubdivision is null)
                {
                    if (!IsValidNextSubdivision(vertical))
                        return;

                    existingSubdivision = ExtraFloorSubdivisions.AddAndReturn(new Subdivision(Floor.Min));
                }

                existingSubdivision.SetEdge(edge, newValue);
            }
            else if (vertical.IsExtraCeilingSubdivision())
            {
                Subdivision existingSubdivision = ExtraCeilingSubdivisions.ElementAtOrDefault(vertical.GetExtraSubdivisionIndex());

                if (existingSubdivision is null)
                {
                    if (!IsValidNextSubdivision(vertical))
                        return;

                    existingSubdivision = ExtraCeilingSubdivisions.AddAndReturn(new Subdivision(Ceiling.Max));
                }
                
                existingSubdivision.SetEdge(edge, newValue);
            }
        }

        public bool IsValidNextSubdivision(BlockVertical vertical)
        {
            if (vertical.IsExtraFloorSubdivision())
                return vertical.GetExtraSubdivisionIndex() == ExtraFloorSubdivisions.Count;
            else if (vertical.IsExtraCeilingSubdivision())
                return vertical.GetExtraSubdivisionIndex() == ExtraCeilingSubdivisions.Count;
            else
                return false;
        }

        public bool SubdivisionExists(BlockVertical vertical)
        {
            if (vertical.IsExtraFloorSubdivision())
                return ExtraFloorSubdivisions.ElementAtOrDefault(vertical.GetExtraSubdivisionIndex()) is not null;
            else if (vertical.IsExtraCeilingSubdivision())
                return ExtraCeilingSubdivisions.ElementAtOrDefault(vertical.GetExtraSubdivisionIndex()) is not null;
            else
                return false;
        }

        private static DiagonalSplit TransformDiagonalSplit(DiagonalSplit split, RectTransformation transformation)
        {
            if (transformation.MirrorX)
                switch (split)
                {
                    case DiagonalSplit.XnZn:
                        split = DiagonalSplit.XpZn;
                        break;
                    case DiagonalSplit.XpZn:
                        split = DiagonalSplit.XnZn;
                        break;
                    case DiagonalSplit.XnZp:
                        split = DiagonalSplit.XpZp;
                        break;
                    case DiagonalSplit.XpZp:
                        split = DiagonalSplit.XnZp;
                        break;
                }

            for (int i = 0; i < transformation.QuadrantRotation; ++i)
                switch (split)
                {
                    case DiagonalSplit.XpZp:
                        split = DiagonalSplit.XnZp;
                        break;
                    case DiagonalSplit.XnZp:
                        split = DiagonalSplit.XnZn;
                        break;
                    case DiagonalSplit.XnZn:
                        split = DiagonalSplit.XpZn;
                        break;
                    case DiagonalSplit.XpZn:
                        split = DiagonalSplit.XpZp;
                        break;
                }

            return split;
        }

        private void MirrorWallTexture(BlockFace oldFace, Func<BlockFace, BlockFaceShape> oldFaceIsTriangle)
        {
            if (!_faceTextures.TryGetValue(oldFace, out TextureArea area))
                return;

            switch (oldFaceIsTriangle(oldFace))
            {
                case BlockFaceShape.Triangle:
                    Swap.Do(ref area.TexCoord0, ref area.TexCoord1);
                    break;
                case BlockFaceShape.Quad:
                    Swap.Do(ref area.TexCoord0, ref area.TexCoord3);
                    Swap.Do(ref area.TexCoord1, ref area.TexCoord2);
                    break;
            }

            _faceTextures[oldFace] = area;
        }

        // Rotates and mirrors a sector according to a given transformation. The transformation can be combination of rotations and mirrors
        // but all possible transformation essentially are boiled down to a mirror on the x axis and a rotation afterwards.
        // Thus all that needs to be handled is mirroring on x and a single counterclockwise rotation (perhaps multiple times in a row).
        // When mirroring is done, a oldFaceIsTriangle must be provided that can return the previous shape of texture faces.
        // Set "onlyFloor" to true, to only modify the floor.
        // Set "onlyFloor" to false, to only modify the ceiling.
        public void Transform(RectTransformation transformation, bool? onlyFloor = null, Func<BlockFace, BlockFaceShape> oldFaceIsTriangle = null)
        {
            // Rotate sector flags
            if (transformation.MirrorX)
                Flags =
                    (Flags & ~(BlockFlags.ClimbPositiveX | BlockFlags.ClimbNegativeX)) |
                    ((Flags & BlockFlags.ClimbPositiveX) != BlockFlags.None ? BlockFlags.ClimbNegativeX : BlockFlags.None) |
                    ((Flags & BlockFlags.ClimbNegativeX) != BlockFlags.None ? BlockFlags.ClimbPositiveX : BlockFlags.None);

            for (int i = 0; i < transformation.QuadrantRotation; ++i)
                Flags =
                    (Flags & ~(BlockFlags.ClimbPositiveX | BlockFlags.ClimbPositiveZ | BlockFlags.ClimbNegativeX | BlockFlags.ClimbNegativeZ)) |
                    ((Flags & BlockFlags.ClimbPositiveX) != BlockFlags.None ? BlockFlags.ClimbPositiveZ : BlockFlags.None) |
                    ((Flags & BlockFlags.ClimbPositiveZ) != BlockFlags.None ? BlockFlags.ClimbNegativeX : BlockFlags.None) |
                    ((Flags & BlockFlags.ClimbNegativeX) != BlockFlags.None ? BlockFlags.ClimbNegativeZ : BlockFlags.None) |
                    ((Flags & BlockFlags.ClimbNegativeZ) != BlockFlags.None ? BlockFlags.ClimbPositiveX : BlockFlags.None);

            // Rotate sector geometry
            bool diagonalChange = transformation.MirrorX != (transformation.QuadrantRotation % 2 != 0);
            bool oldFloorSplitDirectionIsXEqualsZReal = Floor.SplitDirectionIsXEqualsZWithDiagonalSplit;
            if (onlyFloor != false)
            {
                bool requiredFloorSplitDirectionIsXEqualsZ = Floor.SplitDirectionIsXEqualsZ != diagonalChange;
                Floor.DiagonalSplit = TransformDiagonalSplit(Floor.DiagonalSplit, transformation);
                transformation.TransformValueDiagonalQuad(ref Floor.XpZp, ref Floor.XnZp, ref Floor.XnZn, ref Floor.XpZn);

                for (int i = 0; i < ExtraFloorSubdivisions.Count; i++)
                {
                    Subdivision subdivision = ExtraFloorSubdivisions[i];
                    transformation.TransformValueDiagonalQuad(ref subdivision.XpZp, ref subdivision.XnZp, ref subdivision.XnZn, ref subdivision.XpZn);
                }

                if (requiredFloorSplitDirectionIsXEqualsZ != Floor.SplitDirectionIsXEqualsZ)
                    Floor.SplitDirectionToggled = !Floor.SplitDirectionToggled;

                if (HasGhostBlock)
                    transformation.TransformValueDiagonalQuad(ref GhostBlock.Floor.XpZp, ref GhostBlock.Floor.XnZp, ref GhostBlock.Floor.XnZn, ref GhostBlock.Floor.XpZn);

            }

            bool oldCeilingSplitDirectionIsXEqualsZReal = Ceiling.SplitDirectionIsXEqualsZWithDiagonalSplit;
            if (onlyFloor != true)
            {
                bool requiredCeilingSplitDirectionIsXEqualsZ = Ceiling.SplitDirectionIsXEqualsZ != diagonalChange;
                Ceiling.DiagonalSplit = TransformDiagonalSplit(Ceiling.DiagonalSplit, transformation);
                transformation.TransformValueDiagonalQuad(ref Ceiling.XpZp, ref Ceiling.XnZp, ref Ceiling.XnZn, ref Ceiling.XpZn);

                for (int i = 0; i < ExtraCeilingSubdivisions.Count; i++)
                {
                    Subdivision subdivision = ExtraCeilingSubdivisions[i];
                    transformation.TransformValueDiagonalQuad(ref subdivision.XpZp, ref subdivision.XnZp, ref subdivision.XnZn, ref subdivision.XpZn);
                }

                if (requiredCeilingSplitDirectionIsXEqualsZ != Ceiling.SplitDirectionIsXEqualsZ)
                    Ceiling.SplitDirectionToggled = !Ceiling.SplitDirectionToggled;

                if (HasGhostBlock)
                    transformation.TransformValueDiagonalQuad(ref GhostBlock.Ceiling.XpZp, ref GhostBlock.Ceiling.XnZp, ref GhostBlock.Ceiling.XnZn, ref GhostBlock.Ceiling.XpZn);
            }

            // Rotate applied textures
            if (onlyFloor != false)
            {
                // Fix lower wall textures
                if (transformation.MirrorX)
                {
                    var faces = _faceTextures.Where(pair => pair.Key.IsFloorWall()).Select(pair => pair.Key).ToList();

                    for (int i = 0; i < faces.Count; i++)
                        MirrorWallTexture(faces[i], oldFaceIsTriangle);
                }

                transformation.TransformValueQuad(_faceTextures, BlockFace.Wall_PositiveX_QA, BlockFace.Wall_PositiveZ_QA, BlockFace.Wall_NegativeX_QA, BlockFace.Wall_NegativeZ_QA);

                var texturedSubdivisions = _faceTextures.Where(pair => pair.Key.IsExtraFloorSubdivision()).Select(pair => pair.Key).ToList();

                for (int i = 0; i < texturedSubdivisions.Count; i++)
                {
                    int index = texturedSubdivisions[i].GetVertical()?.GetExtraSubdivisionIndex()
                        ?? throw new InvalidOperationException("Invalid floor subdivision face.");

                    transformation.TransformValueQuad(_faceTextures,
                        BlockFaceExtensions.GetExtraFloorSubdivisionFace(Direction.PositiveX, index),
                        BlockFaceExtensions.GetExtraFloorSubdivisionFace(Direction.PositiveZ, index),
                        BlockFaceExtensions.GetExtraFloorSubdivisionFace(Direction.NegativeX, index),
                        BlockFaceExtensions.GetExtraFloorSubdivisionFace(Direction.NegativeZ, index));
                }

                // Fix floor textures
                if (Floor.IsQuad)
                {
                    if (_faceTextures.ContainsKey(BlockFace.Floor))
                        _faceTextures[BlockFace.Floor] = _faceTextures[BlockFace.Floor].Transform(transformation * new RectTransformation { QuadrantRotation = 2 });
                }
                else
                {
                    // Mirror
                    if (transformation.MirrorX)
                    {
                        _faceTextures.TrySwap(BlockFace.Floor, BlockFace.Floor_Triangle2);

                        if (_faceTextures.ContainsKey(BlockFace.Floor))
                        {
                            TextureArea floor = _faceTextures[BlockFace.Floor];
                            Swap.Do(ref floor.TexCoord0, ref floor.TexCoord2);
                            _faceTextures[BlockFace.Floor] = floor;
                        }

                        if (_faceTextures.ContainsKey(BlockFace.Floor_Triangle2))
                        {
                            TextureArea floorTriangle2 = _faceTextures[BlockFace.Floor_Triangle2];
                            Swap.Do(ref floorTriangle2.TexCoord0, ref floorTriangle2.TexCoord2);
                            _faceTextures[BlockFace.Floor_Triangle2] = floorTriangle2;
                        }

                        if (Floor.DiagonalSplit != DiagonalSplit.None) // REMOVE this when we have better diaognal steps.
                            _faceTextures.TrySwap(BlockFace.Floor, BlockFace.Floor_Triangle2);
                    }

                    // Rotation
                    for (int i = 0; i < transformation.QuadrantRotation; ++i)
                    {
                        if (!oldFloorSplitDirectionIsXEqualsZReal)
                            _faceTextures.TrySwap(BlockFace.Floor, BlockFace.Floor_Triangle2);
                        if (Floor.DiagonalSplit != DiagonalSplit.None) // REMOVE this when we have better diaognal steps.
                            _faceTextures.TrySwap(BlockFace.Floor, BlockFace.Floor_Triangle2);

                        oldFloorSplitDirectionIsXEqualsZReal = !oldFloorSplitDirectionIsXEqualsZReal;
                    }
                }
            }
            if (onlyFloor != true)
            {
                // Fix upper wall textures
                if (transformation.MirrorX)
                {
                    var faces = _faceTextures.Where(pair => pair.Key.IsCeilingWall()).Select(pair => pair.Key).ToList();

                    for (int i = 0; i < faces.Count; i++)
                        MirrorWallTexture(faces[i], oldFaceIsTriangle);
                }

                transformation.TransformValueQuad(_faceTextures, BlockFace.Wall_PositiveX_WS, BlockFace.Wall_PositiveZ_WS, BlockFace.Wall_NegativeX_WS, BlockFace.Wall_NegativeZ_WS);

                var texturedSubdivisions = _faceTextures.Where(pair => pair.Key.IsExtraCeilingSubdivision()).Select(pair => pair.Key).ToList();

                for (int i = 0; i < texturedSubdivisions.Count; i++)
                {
                    int index = texturedSubdivisions[i].GetVertical()?.GetExtraSubdivisionIndex()
                        ?? throw new InvalidOperationException("Invalid ceiling subdivision face.");

                    transformation.TransformValueQuad(_faceTextures,
                        BlockFaceExtensions.GetExtraCeilingSubdivisionFace(Direction.PositiveX, index),
                        BlockFaceExtensions.GetExtraCeilingSubdivisionFace(Direction.PositiveZ, index),
                        BlockFaceExtensions.GetExtraCeilingSubdivisionFace(Direction.NegativeX, index),
                        BlockFaceExtensions.GetExtraCeilingSubdivisionFace(Direction.NegativeZ, index));
                }

                // Fix ceiling textures
                if (Ceiling.IsQuad)
                {
                    if (_faceTextures.ContainsKey(BlockFace.Ceiling))
                        _faceTextures[BlockFace.Ceiling] = _faceTextures[BlockFace.Ceiling].Transform(transformation * new RectTransformation { QuadrantRotation = 2 });
                }
                else
                {
                    // Mirror
                    if (transformation.MirrorX)
                    {
                        _faceTextures.TrySwap(BlockFace.Ceiling, BlockFace.Ceiling_Triangle2);

                        if (_faceTextures.ContainsKey(BlockFace.Ceiling))
                        {
                            TextureArea ceiling = _faceTextures[BlockFace.Ceiling];
                            Swap.Do(ref ceiling.TexCoord0, ref ceiling.TexCoord2);
                            _faceTextures[BlockFace.Ceiling] = ceiling;
                        }

                        if (_faceTextures.ContainsKey(BlockFace.Ceiling_Triangle2))
                        {
                            TextureArea ceilingTriangle2 = _faceTextures[BlockFace.Ceiling_Triangle2];
                            Swap.Do(ref ceilingTriangle2.TexCoord0, ref ceilingTriangle2.TexCoord2);
                            _faceTextures[BlockFace.Ceiling_Triangle2] = ceilingTriangle2;
                        }

                        if (Ceiling.DiagonalSplit != DiagonalSplit.None) // REMOVE this when we have better diaognal steps.
                            _faceTextures.TrySwap(BlockFace.Ceiling, BlockFace.Ceiling_Triangle2);
                    }

                    // Rotation
                    for (int i = 0; i < transformation.QuadrantRotation; ++i)
                    {
                        if (!oldCeilingSplitDirectionIsXEqualsZReal)
                            _faceTextures.TrySwap(BlockFace.Ceiling, BlockFace.Ceiling_Triangle2);
                        if (Ceiling.DiagonalSplit != DiagonalSplit.None) // REMOVE this when we have better diaognal steps.
                            _faceTextures.TrySwap(BlockFace.Ceiling, BlockFace.Ceiling_Triangle2);

                        oldCeilingSplitDirectionIsXEqualsZReal = !oldCeilingSplitDirectionIsXEqualsZReal;
                    }
                }
            }
            if (onlyFloor == null)
            {
                if (transformation.MirrorX)
                {
                    MirrorWallTexture(BlockFace.Wall_PositiveX_Middle, oldFaceIsTriangle);
                    MirrorWallTexture(BlockFace.Wall_PositiveZ_Middle, oldFaceIsTriangle);
                    MirrorWallTexture(BlockFace.Wall_NegativeX_Middle, oldFaceIsTriangle);
                    MirrorWallTexture(BlockFace.Wall_NegativeZ_Middle, oldFaceIsTriangle);
                    MirrorWallTexture(BlockFace.Wall_Diagonal_Middle, oldFaceIsTriangle);
                }

                transformation.TransformValueQuad(_faceTextures, BlockFace.Wall_PositiveX_Middle, BlockFace.Wall_PositiveZ_Middle, BlockFace.Wall_NegativeX_Middle, BlockFace.Wall_NegativeZ_Middle);
            }
        }

        public void FixHeights(BlockVertical? vertical = null)
        {
            for (BlockEdge i = 0; i < BlockEdge.Count; i++)
            {
                for (int j = 0; j < ExtraFloorSubdivisions.Count; j++)
                {
                    BlockVertical subdivisionVertical = BlockVerticalExtensions.GetExtraFloorSubdivision(j);
                    BlockVertical lastBlockVertical = j == 0 ? BlockVertical.Floor : BlockVerticalExtensions.GetExtraFloorSubdivision(j - 1); 
                    SetHeight(subdivisionVertical, i, Math.Min(GetHeight(subdivisionVertical, i), GetHeight(lastBlockVertical, i)));
                }

                for (int j = 0; j < ExtraCeilingSubdivisions.Count; j++)
                {
                    BlockVertical subdivisionVertical = BlockVerticalExtensions.GetExtraCeilingSubdivision(j);
                    BlockVertical lastBlockVertical = j == 0 ? BlockVertical.Ceiling : BlockVerticalExtensions.GetExtraCeilingSubdivision(j - 1);
                    SetHeight(subdivisionVertical, i, Math.Max(GetHeight(subdivisionVertical, i), GetHeight(lastBlockVertical, i)));
                }

                if (vertical == null || vertical.Value.IsOnFloor())
                    if (Floor.DiagonalSplit != DiagonalSplit.None)
                        Floor.SetHeight(i, Math.Min(Floor.GetHeight(i), Ceiling.Min));
                    else
                        Floor.SetHeight(i, Math.Min(Floor.GetHeight(i), Ceiling.GetHeight(i)));

                if (vertical == null || vertical.Value.IsOnCeiling())
                    if (Ceiling.DiagonalSplit != DiagonalSplit.None)
                        Ceiling.SetHeight(i, Math.Max(Ceiling.GetHeight(i), Floor.Max));
                    else
                        Ceiling.SetHeight(i, Math.Max(Ceiling.GetHeight(i), Floor.GetHeight(i)));
            }
        }

        public Vector3[] GetFloorTriangleNormals()
        {
            Plane[] tri = new Plane[2];

            var p0 = new Vector3(0, Clicks.FromWorld(Floor.XnZp, RoundingMethod.Integer), 0);
            var p1 = new Vector3(4, Clicks.FromWorld(Floor.XpZp, RoundingMethod.Integer), 0);
            var p2 = new Vector3(4, Clicks.FromWorld(Floor.XpZn, RoundingMethod.Integer), -4);
            var p3 = new Vector3(0, Clicks.FromWorld(Floor.XnZn, RoundingMethod.Integer), -4);

            // Create planes based on floor split direction

            if (Floor.SplitDirectionIsXEqualsZ)
            {
                tri[0] = Plane.CreateFromVertices(p1, p2, p3);
                tri[1] = Plane.CreateFromVertices(p1, p3, p0);
            }
            else
            {
                tri[0] = Plane.CreateFromVertices(p0, p1, p2);
                tri[1] = Plane.CreateFromVertices(p0, p2, p3);
            }

            return new Vector3[2] { tri[0].Normal, tri[1].Normal };
        }

        public int GetTriangleMinimumFloorPoint(int triangle)
        {
            if (triangle != 0 && triangle != 1)
                return 0;

            if (Floor.SplitDirectionIsXEqualsZ)
            {
                if (triangle == 0)
                    return Math.Min(Math.Min(Floor.XpZp, Floor.XpZn), Floor.XnZn);
                else
                    return Math.Min(Math.Min(Floor.XpZp, Floor.XnZn), Floor.XnZp);
            }
            else
            {
                if (triangle == 0)
                    return Math.Min(Math.Min(Floor.XnZp, Floor.XpZp), Floor.XpZn);
                else
                    return Math.Min(Math.Min(Floor.XnZp, Floor.XpZn), Floor.XnZn);
            }
        }

        public Direction[] GetFloorTriangleSlopeDirections()
        {
            const float CriticalSlantYComponent = 0.8f;

            var normals = GetFloorTriangleNormals();

            // Initialize slope directions as unslidable by default (EntireFace means unslidable in our case).

            Direction[] slopeDirections = new Direction[2] { Direction.None, Direction.None };

            if (Floor.HasSlope)
            {
                for (int i = 0; i < (Floor.IsQuad ? 1 : 2); i++) // If floor is quad, we don't solve second triangle
                {
                    if (Math.Abs(normals[i].Y) <= CriticalSlantYComponent) // Triangle is slidable
                    {
                        bool angleNotDefined = true;
                        var angle = (float)(Math.Atan2(normals[i].X, normals[i].Z) * (180 / Math.PI));
                        angle = angle < 0 ? angle + 360.0f : angle;

                        // Note about 45, 135, 225 and 315 degree steps:
                        // Core Design has used override instead of rounding for triangular slopes angled under
                        // 45-degree stride, to produce either east or west-oriented slide.

                        while (angleNotDefined)
                        {
                            switch ((int)angle)
                            {
                                case 0:
                                case 360:
                                    slopeDirections[i] = Direction.PositiveZ;
                                    angleNotDefined = false;
                                    break;
                                case 45:
                                case 90:
                                case 135:
                                    slopeDirections[i] = Direction.PositiveX;
                                    angleNotDefined = false;
                                    break;
                                case 180:
                                    slopeDirections[i] = Direction.NegativeZ;
                                    angleNotDefined = false;
                                    break;
                                case 225:
                                case 270:
                                case 315:
                                    slopeDirections[i] = Direction.NegativeX;
                                    angleNotDefined = false;
                                    break;
                                default:
                                    angle = (int)Math.Round(angle / 90.0f, MidpointRounding.AwayFromZero) * 90;
                                    break;
                            }
                        }
                    }
                }

                // We swap triangle directions for XpZn and XnZp cases, because in these cases
                // triangle indices are inverted.
                // For other cases, we move slide direction triangle to proper one accordingly to
                // step slant value encoded in corner heights.

                if (Floor.DiagonalSplit != DiagonalSplit.None)
                {
                    switch (Floor.DiagonalSplit)
                    {
                        case DiagonalSplit.XpZn:
                            slopeDirections[1] = slopeDirections[0];
                            slopeDirections[0] = Direction.None;
                            break;

                        case DiagonalSplit.XnZp:
                            if (!Floor.IsQuad)
                            {
                                slopeDirections[0] = slopeDirections[1];
                                slopeDirections[1] = Direction.None;
                            }
                            break;

                        case DiagonalSplit.XnZn:
                            if (Floor.IsQuad)
                            {
                                slopeDirections[1] = slopeDirections[0];
                                slopeDirections[0] = Direction.None;
                            }
                            else
                                slopeDirections[0] = Direction.None;
                            break;

                        case DiagonalSplit.XpZp:
                            if (!Floor.IsQuad)
                                slopeDirections[1] = Direction.None;
                            break;
                    }
                }
            }

            return slopeDirections;
        }
    }
}
