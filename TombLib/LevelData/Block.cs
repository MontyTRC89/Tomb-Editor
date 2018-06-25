using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Numerics;
using System.Runtime.CompilerServices;
using TombLib.Utils;

namespace TombLib.LevelData
{
    public enum BlockType : byte
    {
        Floor, Wall, BorderWall
    }

    [Flags]
    public enum BlockFlags : short
    {
        None = 0,
        Monkey = 1,
        Box = 2,
        DeathFire = 4,
        DeathLava = 8,
        DeathElectricity = 16,
        Beetle = 32,
        TriggerTriggerer = 64,
        NotWalkableFloor = 128,
        ClimbPositiveZ = 256,
        ClimbNegativeZ = 512,
        ClimbPositiveX = 1024,
        ClimbNegativeX = 2048,
        ClimbAny = ClimbPositiveZ | ClimbNegativeZ | ClimbPositiveX | ClimbNegativeX
    }

    [SuppressMessage("ReSharper", "InconsistentNaming")]
    public enum DiagonalSplit : byte
    {
        None = 0,
        XnZp = 1,
        XpZp = 2,
        XpZn = 3,
        XnZn = 4
    }

    [SuppressMessage("ReSharper", "InconsistentNaming")]
    public enum BlockFace : byte
    {
        PositiveZ_QA = 0, NegativeZ_QA = 1, NegativeX_QA = 2, PositiveX_QA = 3, DiagonalQA = 4,
        PositiveZ_ED = 5, NegativeZ_ED = 6, NegativeX_ED = 7, PositiveX_ED = 8, DiagonalED = 9,
        PositiveZ_Middle = 10, NegativeZ_Middle = 11, NegativeX_Middle = 12, PositiveX_Middle = 13, DiagonalMiddle = 14,
        PositiveZ_WS = 15, NegativeZ_WS = 16, NegativeX_WS = 17, PositiveX_WS = 18, DiagonalWS = 19,
        PositiveZ_RF = 20, NegativeZ_RF = 21, NegativeX_RF = 22, PositiveX_RF = 23, DiagonalRF = 24,
        Floor = 25, FloorTriangle2 = 26, Ceiling = 27, CeilingTriangle2 = 28
    }

    public enum Direction : byte
    {
        None = 0, PositiveZ = 1, PositiveX = 2, NegativeZ = 3, NegativeX = 4, Diagonal = 5
    }

    [Serializable]
    public struct BlockSurface
    {
        public bool SplitDirectionToggled;
        public DiagonalSplit DiagonalSplit;
        public short XnZp;
        public short XpZp;
        public short XpZn;
        public short XnZn;

        public bool IsQuad => DiagonalSplit == DiagonalSplit.None && IsQuad2(XnZp, XpZp, XpZn, XnZn);
        public bool HasSlope => Max - Min > 2;
        public int IfQuadSlopeX => IsQuad ? XpZp - XnZp : 0;
        public int IfQuadSlopeZ => IsQuad ? XpZp - XpZn : 0;
        public short Max => Math.Max(Math.Max(XnZp, XpZp), Math.Max(XpZn, XnZn));
        public short Min => Math.Min(Math.Min(XnZp, XpZp), Math.Min(XpZn, XnZn));

        public short GetHeight(int face)
        {
            switch (face)
            {
                case Block.EdgeXnZp:
                    return XnZp;
                case Block.EdgeXpZp:
                    return XpZp;
                case Block.EdgeXpZn:
                    return XpZn;
                case Block.EdgeXnZn:
                    return XnZn;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public void SetHeight(int face, int value)
        {
            switch (face)
            {
                case Block.EdgeXnZp:
                    XnZp = checked((short)value);
                    return;
                case Block.EdgeXpZp:
                    XpZp = checked((short)value);
                    return;
                case Block.EdgeXpZn:
                    XpZn = checked((short)value);
                    return;
                case Block.EdgeXnZn:
                    XnZn = checked((short)value);
                    return;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public static bool IsQuad2(int hXnZp, int hXpZp, int hXpZn, int hXnZn)
        {
            return hXpZp - hXpZn == hXnZp - hXnZn &&
                hXpZp - hXnZp == hXpZn - hXnZn;
        }

        public bool SplitDirectionIsXEqualsZ
        {
            get
            {
                var p1 = new Vector3(0, XnZp, 0);
                var p2 = new Vector3(1, XpZp, 0);
                var p3 = new Vector3(1, XpZn, 1);
                var p4 = new Vector3(0, XnZn, 1);

                var plane = Plane.CreateFromVertices(p1, p2, p4);
                if (plane.Normal == Vector3.UnitY || plane.Normal == -Vector3.UnitY)
                    return !SplitDirectionToggled;

                plane = Plane.CreateFromVertices(p1, p2, p3);
                if (plane.Normal == Vector3.UnitY || plane.Normal == -Vector3.UnitY)
                    return SplitDirectionToggled;

                plane = Plane.CreateFromVertices(p2, p3, p4);
                if (plane.Normal == Vector3.UnitY || plane.Normal == -Vector3.UnitY)
                    return !SplitDirectionToggled;

                plane = Plane.CreateFromVertices(p3, p4, p1);
                if (plane.Normal == Vector3.UnitY || plane.Normal == -Vector3.UnitY)
                    return SplitDirectionToggled;

                // Otherwise
                int min = Math.Min(Math.Min(Math.Min(XnZp, XpZp), XpZn), XnZn);
                int max = Math.Max(Math.Max(Math.Max(XnZp, XpZp), XpZn), XnZn);

                if (XnZp == XpZn && XpZp == XnZn && XpZp != XpZn)
                    return SplitDirectionToggled;

                if (min == XnZp && min == XpZn)
                    return SplitDirectionToggled;
                if (min == XpZp && min == XnZn)
                    return !SplitDirectionToggled;

                if (min == XnZp && max == XpZn)
                    return !SplitDirectionToggled;
                if (min == XpZp && max == XnZn)
                    return SplitDirectionToggled;
                if (min == XpZn && max == XnZp)
                    return !SplitDirectionToggled;
                if (min == XnZn && max == XpZp)
                    return SplitDirectionToggled;

                return !SplitDirectionToggled;
            }
            set
            {
                if (value != SplitDirectionIsXEqualsZ)
                    SplitDirectionToggled = !SplitDirectionToggled;
            }
        }

        /// <summary>Checks for DiagonalSplit and takes priority</summary>
        public bool SplitDirectionIsXEqualsZWithDiagonalSplit
        {
            get
            {
                switch (DiagonalSplit)
                {
                    case DiagonalSplit.XnZn:
                    case DiagonalSplit.XpZp:
                        return false;
                    case DiagonalSplit.XpZn:
                    case DiagonalSplit.XnZp:
                        return true;
                    case DiagonalSplit.None:
                        return SplitDirectionIsXEqualsZ;
                    default:
                        throw new ApplicationException("\"DiagonalSplit\" in unknown state.");
                }
            }
        }

        /// <summary>Returns the height of the 4 edges if the sector is split</summary>
        public int GetActualMax(int edge)
        {
            switch (DiagonalSplit)
            {
                case DiagonalSplit.None:
                    return GetHeight(edge);
                case DiagonalSplit.XnZn:
                    if (edge == Block.EdgeXnZp || edge == Block.EdgeXpZn)
                        return Math.Max(GetHeight(edge), XpZp);
                    return GetHeight(edge);
                case DiagonalSplit.XpZp:
                    if (edge == Block.EdgeXnZp || edge == Block.EdgeXpZn)
                        return Math.Max(GetHeight(edge), XnZn);
                    return GetHeight(edge);
                case DiagonalSplit.XpZn:
                    if (edge == Block.EdgeXpZp || edge == Block.EdgeXnZn)
                        return Math.Max(GetHeight(edge), XnZp);
                    return GetHeight(edge);
                case DiagonalSplit.XnZp:
                    if (edge == Block.EdgeXpZp || edge == Block.EdgeXnZn)
                        return Math.Max(GetHeight(edge), XpZn);
                    return GetHeight(edge);
                default:
                    throw new ApplicationException("\"splitType\" in unknown state.");
            }
        }

        /// <summary>Returns the height of the 4 edges if the sector is split</summary>
        public int GetActualMin(int face)
        {
            switch (DiagonalSplit)
            {
                case DiagonalSplit.None:
                    return GetHeight(face);
                case DiagonalSplit.XnZn:
                    if (face == Block.EdgeXnZp || face == Block.EdgeXpZn)
                        return Math.Min(GetHeight(face), XpZp);
                    return GetHeight(face);
                case DiagonalSplit.XpZp:
                    if (face == Block.EdgeXnZp || face == Block.EdgeXpZn)
                        return Math.Min(GetHeight(face), XnZn);
                    return GetHeight(face);
                case DiagonalSplit.XpZn:
                    if (face == Block.EdgeXpZp || face == Block.EdgeXnZn)
                        return Math.Min(GetHeight(face), XnZp);
                    return GetHeight(face);
                case DiagonalSplit.XnZp:
                    if (face == Block.EdgeXpZp || face == Block.EdgeXnZn)
                        return Math.Min(GetHeight(face), XpZn);
                    return GetHeight(face);
                default:
                    throw new ApplicationException("\"splitType\" in unknown state.");
            }
        }
    }

    [Serializable]
    public class Block : ICloneable
    {
        public static Block Empty { get; } = new Block();

        public const BlockFace FaceCount = (BlockFace)29;

        /// <summary> Index of edges on the negative X and positive Z direction </summary>
        public const int EdgeXnZp = 0;
        /// <summary> Index of edges on the positive X and positive Z direction </summary>
        public const int EdgeXpZp = 1;
        /// <summary> Index of edges on the positive X and negative Z direction </summary>
        public const int EdgeXpZn = 2;
        /// <summary> Index of edges on the negative X and negative Z direction </summary>
        public const int EdgeXnZn = 3;

        /// <summary> Maximum normal height for non-slidable slopes </summary>
        public const float CriticalSlantComponent = 0.8f;
        /// <summary> The x offset of each face index in [0, 4). </summary>
        public static readonly int[] FaceX = new[] { 0, 1, 1, 0 };
        /// <summary> The x offset of each face index in [0, 4). </summary>
        public static readonly int[] FaceZ = new[] { 1, 1, 0, 0 };

        public BlockType Type { get; set; } = BlockType.Floor;
        public BlockFlags Flags { get; set; } = BlockFlags.None;
        public bool ForceFloorSolid { get; set; } // If this is set to true, portals are overwritten for this sector.
        // ReSharper disable once InconsistentNaming
        public short[] ED { get; } = new short[4];
        // ReSharper disable once InconsistentNaming
        public short[] RF { get; } = new short[4];
        private TextureArea[] _faceTextures { get; } = new TextureArea[(int)FaceCount];

        public BlockSurface Floor;
        public BlockSurface Ceiling;

        public List<TriggerInstance> Triggers { get; } = new List<TriggerInstance>(); // This array is not supposed to be modified here.
        public PortalInstance FloorPortal { get; internal set; } = null; // This is not supposed to be modified here.
        public PortalInstance WallPortal { get; internal set; } = null; // This is not supposed to be modified here.
        public PortalInstance CeilingPortal { get; internal set; } = null; // This is not supposed to be modified here.

        private Block()
        { }

        public Block(int floor, int ceiling)
        {
            Floor.XnZp = Floor.XpZp = Floor.XpZn = Floor.XnZn = (short)floor;
            ED[EdgeXnZp] = ED[EdgeXpZp] = ED[EdgeXpZn] = ED[EdgeXnZn] = (short)floor;
            RF[EdgeXnZp] = RF[EdgeXpZp] = RF[EdgeXpZn] = RF[EdgeXnZn] = (short)ceiling;
            Ceiling.XnZp = Ceiling.XpZp = Ceiling.XpZn = Ceiling.XnZn = (short)ceiling;
        }

        public Block Clone()
        {
            var result = new Block();
            result.Type = Type;
            result.Flags = Flags;
            result.ForceFloorSolid = ForceFloorSolid;
            for (int i = 0; i < (int)FaceCount; i++)
                result._faceTextures[i] = _faceTextures[i];
            for (int i = 0; i < 4; i++)
                result.ED[i] = ED[i];
            for (int i = 0; i < 4; i++)
                result.RF[i] = RF[i];
            result.Floor = Floor;
            result.Ceiling = Ceiling;
            return result;
        }

        object ICloneable.Clone() => Clone();

        public bool SetFaceTexture(BlockFace face, TextureArea texture)
        {
            if (_faceTextures[(int)face] != texture)
            {
                _faceTextures[(int)face] = texture;
                return true;
            }
            else
                return false;
        }

        public TextureArea GetFaceTexture(BlockFace face)
        {
            return _faceTextures[(int)face];
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

        public short GetHeight(int verticalSubdivision, int edge)
        {
            switch (verticalSubdivision)
            {
                case 0:
                    return Floor.GetHeight(edge);
                case 1:
                    return Ceiling.GetHeight(edge);
                case 2:
                    return ED[edge];
                case 3:
                    return RF[edge];
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public void SetHeight(int verticalSubdivision, int edge, int newValue)
        {
            switch (verticalSubdivision)
            {
                case 0:
                    Floor.SetHeight(edge, newValue);
                    return;
                case 1:
                    Ceiling.SetHeight(edge, newValue);
                    return;
                case 2:
                    ED[edge] = checked((short)newValue);
                    return;
                case 3:
                    RF[edge] = checked((short)newValue);
                    return;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public void ChangeHeight(int verticalSubdivision, int edge, short increment)
        {
            SetHeight(verticalSubdivision, edge, (short)(GetHeight(verticalSubdivision, edge) + increment));
        }

        public void Raise(int verticalSubdivision, bool diagonalStep, short increment)
        {
            var split = verticalSubdivision == 0 || verticalSubdivision == 2 ? Floor.DiagonalSplit : Ceiling.DiagonalSplit;
            if (diagonalStep)
            {
                switch (split)
                {
                    case DiagonalSplit.XpZn:
                        ChangeHeight(verticalSubdivision, Block.EdgeXnZp, increment);
                        break;
                    case DiagonalSplit.XnZn:
                        ChangeHeight(verticalSubdivision, Block.EdgeXpZp, increment);
                        break;
                    case DiagonalSplit.XnZp:
                        ChangeHeight(verticalSubdivision, Block.EdgeXpZn, increment);
                        break;
                    case DiagonalSplit.XpZp:
                        ChangeHeight(verticalSubdivision, Block.EdgeXnZn, increment);
                        break;
                }
            }
            else
            {
                for (int i = 0; i < 4; i++)
                {
                    if (i == 0 && split == DiagonalSplit.XpZn ||
                        i == 1 && split == DiagonalSplit.XnZn ||
                        i == 2 && split == DiagonalSplit.XnZp ||
                        i == 3 && split == DiagonalSplit.XpZp)
                        continue;
                    ChangeHeight(verticalSubdivision, i, increment);
                }
            }
        }

        public void RaiseStepWise(int verticalSubdivision, bool diagonalStep, short increment, bool autoSwitch = false)
        {
            var floor = verticalSubdivision % 2 == 0;
            var split = floor ? Floor.DiagonalSplit : Ceiling.DiagonalSplit;

            if (split != DiagonalSplit.None)
            {
                var stepIsLimited = increment != 0 && increment > 0 == (!floor ^ diagonalStep);

                if (split == DiagonalSplit.XpZn && GetHeight(verticalSubdivision, Block.EdgeXnZp) == GetHeight(verticalSubdivision, Block.EdgeXpZp) && stepIsLimited ||
                    split == DiagonalSplit.XnZn && GetHeight(verticalSubdivision, Block.EdgeXpZp) == GetHeight(verticalSubdivision, Block.EdgeXpZn) && stepIsLimited ||
                    split == DiagonalSplit.XnZp && GetHeight(verticalSubdivision, Block.EdgeXpZn) == GetHeight(verticalSubdivision, Block.EdgeXnZn) && stepIsLimited ||
                    split == DiagonalSplit.XpZp && GetHeight(verticalSubdivision, Block.EdgeXnZn) == GetHeight(verticalSubdivision, Block.EdgeXnZp) && stepIsLimited)
                {
                    if (IsAnyWall && autoSwitch)
                        Raise(verticalSubdivision, !diagonalStep, increment);
                    else
                    {
                        if (autoSwitch)
                        {
                            Transform(new RectTransformation { QuadrantRotation = 2 }, floor);
                            Raise(verticalSubdivision, !diagonalStep, increment);
                        }
                        return;
                    }
                }
            }
            Raise(verticalSubdivision, diagonalStep, increment);
        }

        public enum FaceShape
        {
            Triangle,
            Quad,
            Unknown
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

        private void MirrorWallTexture(BlockFace oldFace, Func<BlockFace, FaceShape> oldFaceIsTriangle)
        {
            switch (oldFaceIsTriangle(oldFace))
            {
                case FaceShape.Triangle:
                    Swap.Do(ref _faceTextures[(int)oldFace].TexCoord0, ref _faceTextures[(int)oldFace].TexCoord1);
                    break;
                case FaceShape.Quad:
                    Swap.Do(ref _faceTextures[(int)oldFace].TexCoord0, ref _faceTextures[(int)oldFace].TexCoord3);
                    Swap.Do(ref _faceTextures[(int)oldFace].TexCoord1, ref _faceTextures[(int)oldFace].TexCoord2);
                    break;
            }
        }

        public void Transform(RectTransformation transformation, bool? onlyFloor = null, Func<BlockFace, FaceShape> oldFaceIsTriangle = null)
        {
            // Rotate sector flags
            if (onlyFloor != null)
            {
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
            }

            // Rotate sector geometry
            bool diagonalChange = transformation.MirrorX != (transformation.QuadrantRotation % 2 != 0);
            bool oldFloorSplitDirectionIsXEqualsZReal = Floor.SplitDirectionIsXEqualsZWithDiagonalSplit;
            if (onlyFloor != false)
            {
                bool requiredFloorSplitDirectionIsXEqualsZ = Floor.SplitDirectionIsXEqualsZ != diagonalChange;
                Floor.DiagonalSplit = TransformDiagonalSplit(Floor.DiagonalSplit, transformation);
                transformation.TransformValueDiagonalQuad(ref Floor.XpZp, ref Floor.XnZp, ref Floor.XnZn, ref Floor.XpZn);
                transformation.TransformValueDiagonalQuad(ref ED[EdgeXpZp], ref ED[EdgeXnZp], ref ED[EdgeXnZn], ref ED[EdgeXpZn]);
                if (requiredFloorSplitDirectionIsXEqualsZ != Floor.SplitDirectionIsXEqualsZ)
                    Floor.SplitDirectionToggled = !Floor.SplitDirectionToggled;
            }
            bool oldCeilingSplitDirectionIsXEqualsZReal = Ceiling.SplitDirectionIsXEqualsZWithDiagonalSplit;
            if (onlyFloor != true)
            {
                bool requiredCeilingSplitDirectionIsXEqualsZ = Ceiling.SplitDirectionIsXEqualsZ != diagonalChange;
                Ceiling.DiagonalSplit = TransformDiagonalSplit(Ceiling.DiagonalSplit, transformation);
                transformation.TransformValueDiagonalQuad(ref Ceiling.XpZp, ref Ceiling.XnZp, ref Ceiling.XnZn, ref Ceiling.XpZn);
                transformation.TransformValueDiagonalQuad(ref RF[EdgeXpZp], ref RF[EdgeXnZp], ref RF[EdgeXnZn], ref RF[EdgeXpZn]);
                if (requiredCeilingSplitDirectionIsXEqualsZ != Ceiling.SplitDirectionIsXEqualsZ)
                    Ceiling.SplitDirectionToggled = !Ceiling.SplitDirectionToggled;
            }

            // Rotate applied textures
            if (onlyFloor != false)
            {
                // Fix lower wall textures
                if (transformation.MirrorX)
                {
                    MirrorWallTexture(BlockFace.PositiveX_QA, oldFaceIsTriangle);
                    MirrorWallTexture(BlockFace.PositiveZ_QA, oldFaceIsTriangle);
                    MirrorWallTexture(BlockFace.NegativeX_QA, oldFaceIsTriangle);
                    MirrorWallTexture(BlockFace.NegativeZ_QA, oldFaceIsTriangle);
                    MirrorWallTexture(BlockFace.DiagonalQA, oldFaceIsTriangle);

                    MirrorWallTexture(BlockFace.PositiveX_ED, oldFaceIsTriangle);
                    MirrorWallTexture(BlockFace.PositiveZ_ED, oldFaceIsTriangle);
                    MirrorWallTexture(BlockFace.NegativeX_ED, oldFaceIsTriangle);
                    MirrorWallTexture(BlockFace.NegativeZ_ED, oldFaceIsTriangle);
                    MirrorWallTexture(BlockFace.DiagonalED, oldFaceIsTriangle);
                }
                transformation.TransformValueQuad(
                    ref _faceTextures[(int)BlockFace.PositiveX_QA],
                    ref _faceTextures[(int)BlockFace.PositiveZ_QA],
                    ref _faceTextures[(int)BlockFace.NegativeX_QA],
                    ref _faceTextures[(int)BlockFace.NegativeZ_QA]);
                transformation.TransformValueQuad(
                    ref _faceTextures[(int)BlockFace.PositiveX_ED],
                    ref _faceTextures[(int)BlockFace.PositiveZ_ED],
                    ref _faceTextures[(int)BlockFace.NegativeX_ED],
                    ref _faceTextures[(int)BlockFace.NegativeZ_ED]);

                // Fix floor textures
                if (Floor.IsQuad)
                {
                    _faceTextures[(int)BlockFace.Floor] = _faceTextures[(int)BlockFace.Floor].Transform(transformation * new RectTransformation { QuadrantRotation = 2 });
                }
                else
                {
                    // Mirror
                    if (transformation.MirrorX)
                    {
                        Swap.Do(ref _faceTextures[(int)BlockFace.Floor], ref _faceTextures[(int)BlockFace.FloorTriangle2]);
                        Swap.Do(ref _faceTextures[(int)BlockFace.Floor].TexCoord0, ref _faceTextures[(int)BlockFace.Floor].TexCoord2);
                        Swap.Do(ref _faceTextures[(int)BlockFace.FloorTriangle2].TexCoord0, ref _faceTextures[(int)BlockFace.FloorTriangle2].TexCoord2);
                    }

                    // Rotation
                    for (int i = 0; i < transformation.QuadrantRotation; ++i)
                    {
                        if (!oldFloorSplitDirectionIsXEqualsZReal)
                            Swap.Do(ref _faceTextures[(int)BlockFace.Floor], ref _faceTextures[(int)BlockFace.FloorTriangle2]);
                        oldFloorSplitDirectionIsXEqualsZReal = !oldFloorSplitDirectionIsXEqualsZReal;
                    }
                }
            }
            if (onlyFloor != true)
            {
                // Fix upper wall textures
                if (transformation.MirrorX)
                {
                    MirrorWallTexture(BlockFace.PositiveX_WS, oldFaceIsTriangle);
                    MirrorWallTexture(BlockFace.PositiveZ_WS, oldFaceIsTriangle);
                    MirrorWallTexture(BlockFace.NegativeX_WS, oldFaceIsTriangle);
                    MirrorWallTexture(BlockFace.NegativeZ_WS, oldFaceIsTriangle);
                    MirrorWallTexture(BlockFace.DiagonalWS, oldFaceIsTriangle);

                    MirrorWallTexture(BlockFace.PositiveX_RF, oldFaceIsTriangle);
                    MirrorWallTexture(BlockFace.PositiveZ_RF, oldFaceIsTriangle);
                    MirrorWallTexture(BlockFace.NegativeX_RF, oldFaceIsTriangle);
                    MirrorWallTexture(BlockFace.NegativeZ_RF, oldFaceIsTriangle);
                    MirrorWallTexture(BlockFace.DiagonalRF, oldFaceIsTriangle);
                }
                transformation.TransformValueQuad(
                    ref _faceTextures[(int)BlockFace.PositiveX_WS],
                    ref _faceTextures[(int)BlockFace.PositiveZ_WS],
                    ref _faceTextures[(int)BlockFace.NegativeX_WS],
                    ref _faceTextures[(int)BlockFace.NegativeZ_WS]);
                transformation.TransformValueQuad(
                    ref _faceTextures[(int)BlockFace.PositiveX_RF],
                    ref _faceTextures[(int)BlockFace.PositiveZ_RF],
                    ref _faceTextures[(int)BlockFace.NegativeX_RF],
                    ref _faceTextures[(int)BlockFace.NegativeZ_RF]);

                // Fix ceiling textures
                if (Ceiling.IsQuad)
                {
                    _faceTextures[(int)BlockFace.Ceiling] = _faceTextures[(int)BlockFace.Ceiling].Transform(transformation * new RectTransformation { QuadrantRotation = 2 });
                }
                else
                {
                    // Mirror
                    if (transformation.MirrorX)
                    {
                        Swap.Do(ref _faceTextures[(int)BlockFace.Ceiling], ref _faceTextures[(int)BlockFace.CeilingTriangle2]);
                        Swap.Do(ref _faceTextures[(int)BlockFace.Ceiling].TexCoord0, ref _faceTextures[(int)BlockFace.Ceiling].TexCoord2);
                        Swap.Do(ref _faceTextures[(int)BlockFace.CeilingTriangle2].TexCoord0, ref _faceTextures[(int)BlockFace.CeilingTriangle2].TexCoord2);
                    }

                    // Rotation
                    for (int i = 0; i < transformation.QuadrantRotation; ++i)
                    {
                        if (!oldCeilingSplitDirectionIsXEqualsZReal)
                            Swap.Do(ref _faceTextures[(int)BlockFace.Ceiling], ref _faceTextures[(int)BlockFace.CeilingTriangle2]);
                        oldCeilingSplitDirectionIsXEqualsZReal = !oldCeilingSplitDirectionIsXEqualsZReal;
                    }
                }
            }
            if (onlyFloor == null)
            {
                if (transformation.MirrorX)
                {
                    MirrorWallTexture(BlockFace.PositiveX_Middle, oldFaceIsTriangle);
                    MirrorWallTexture(BlockFace.PositiveZ_Middle, oldFaceIsTriangle);
                    MirrorWallTexture(BlockFace.NegativeX_Middle, oldFaceIsTriangle);
                    MirrorWallTexture(BlockFace.NegativeZ_Middle, oldFaceIsTriangle);
                    MirrorWallTexture(BlockFace.DiagonalMiddle, oldFaceIsTriangle);
                }

                transformation.TransformValueQuad(
                    ref _faceTextures[(int)BlockFace.PositiveX_Middle],
                    ref _faceTextures[(int)BlockFace.PositiveZ_Middle],
                    ref _faceTextures[(int)BlockFace.NegativeX_Middle],
                    ref _faceTextures[(int)BlockFace.NegativeZ_Middle]);
            }
        }

        public void FixHeights(int verticalSubdivision = -1)
        {
            for (int i = 0; i < 4; i++)
            {
                ED[i] = Math.Min(ED[i], Floor.GetHeight(i));
                RF[i] = Math.Max(RF[i], Ceiling.GetHeight(i));

                if (verticalSubdivision == 0 || verticalSubdivision == 2 || verticalSubdivision == -1)
                    if (Floor.DiagonalSplit != DiagonalSplit.None)
                        Floor.SetHeight(i, Math.Min(Floor.GetHeight(i), Ceiling.Min));
                    else
                        Floor.SetHeight(i, Math.Min(Floor.GetHeight(i), Ceiling.GetHeight(i)));

                if (verticalSubdivision == 1 || verticalSubdivision == 3 || verticalSubdivision == -1)
                    if (Ceiling.DiagonalSplit != DiagonalSplit.None)
                        Ceiling.SetHeight(i, Math.Max(Ceiling.GetHeight(i), Floor.Max));
                    else
                        Ceiling.SetHeight(i, Math.Max(Ceiling.GetHeight(i), Floor.GetHeight(i)));
            }
        }

        public Vector3[] GetFloorTriangleNormals()
        {
            Plane[] tri = new Plane[2];

            var p0 = new Vector3(0, Floor.XnZp, 0);
            var p1 = new Vector3(4, Floor.XpZp, 0);
            var p2 = new Vector3(4, Floor.XpZn, -4);
            var p3 = new Vector3(0, Floor.XnZn, -4);

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

        public short GetTriangleMinimumFloorPoint(int triangle)
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
            var normals = GetFloorTriangleNormals();

            // Initialize slope directions as unslidable by default (EntireFace means unslidable in our case).

            Direction[] slopeDirections = new Direction[2] { Direction.None, Direction.None };

            if (Floor.HasSlope)
            {
                for (int i = 0; i < (Floor.IsQuad ? 1 : 2); i++) // If floor is quad, we don't solve second triangle
                {
                    if (Math.Abs(normals[i].Y) <= CriticalSlantComponent) // Triangle is slidable
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

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool HasFlag(BlockFlags flag)
        {
            return (Flags & flag) == flag;
        }

        public bool IsAnyWall => Type != BlockType.Floor;
        public bool IsAnyPortal => FloorPortal != null || CeilingPortal != null || WallPortal != null;
    }
}
