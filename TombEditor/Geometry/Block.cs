using SharpDX;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Linq;
using System.Text;
using TombEditor.Geometry.IO;
using TombLib.Utils;
using TombLib;

namespace TombEditor.Geometry
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

    public class Block : ICloneable
    {
        public static Block Empty { get; } = new Block();

        public const BlockFace FaceCount = (BlockFace)29;
        /// <summary> Index of faces on the negative X and positive Z direction </summary>
        public const int FaceXnZp = 0;
        /// <summary> Index of faces on the positive X and positive Z direction </summary>
        public const int FaceXpZp = 1;
        /// <summary> Index of faces on the positive X and negative Z direction </summary>
        public const int FaceXpZn = 2;
        /// <summary> Index of faces on the negative X and negative Z direction </summary>
        public const int FaceXnZn = 3;
        /// <summary> Maximum normal height for non-slidable slopes </summary>
        public const float CriticalSlantComponent = 0.8f;
        /// <summary> The x offset of each face index in [0, 4). </summary>
        public static readonly int[] FaceX = new int[] { 0, 1, 1, 0 };
        /// <summary> The x offset of each face index in [0, 4). </summary>
        public static readonly int[] FaceZ = new int[] { 1, 1, 0, 0 };

        public BlockType Type { get; set; } = BlockType.Floor;
        public BlockFlags Flags { get; set; } = BlockFlags.None;
        public bool ForceFloorSolid { get; set; } = false; // If this is set to true, portals are overwritten for this sector.
        // ReSharper disable once InconsistentNaming
        public short[] EDFaces { get; } = new short[4];
        // ReSharper disable once InconsistentNaming
        public short[] QAFaces { get; } = new short[4];
        // ReSharper disable once InconsistentNaming
        public short[] WSFaces { get; } = new short[4];
        // ReSharper disable once InconsistentNaming
        public short[] RFFaces { get; } = new short[4];
        private TextureArea[] _faceTextures { get; } = new TextureArea[(int)FaceCount];

        public bool FloorSplitDirectionToggled { get; set; } = false;
        public bool CeilingSplitDirectionToggled { get; set; } = false;
        public DiagonalSplit FloorDiagonalSplit { get; set; } = DiagonalSplit.None;
        public DiagonalSplit CeilingDiagonalSplit { get; set; } = DiagonalSplit.None;

        public List<TriggerInstance> Triggers { get; } = new List<TriggerInstance>(); // This array is not supposed to be modified here.
        public PortalInstance FloorPortal { get; internal set; } = null; // This is not supposed to be modified here.
        public PortalInstance WallPortal { get; internal set; } = null; // This is not supposed to be modified here.
        public PortalInstance CeilingPortal { get; internal set; } = null; // This is not supposed to be modified here.

        private Block()
        { }

        public Block(int floor, int ceiling)
        {
            for (int i = 0; i < 4; i++)
            {
                QAFaces[i] = (short)floor;
                EDFaces[i] = (short)floor;
                WSFaces[i] = (short)ceiling;
                RFFaces[i] = (short)ceiling;
            }
        }

        public Block Clone()
        {
            var result = new Block();
            result.Flags = Flags;
            result.Type = Type;
            result.ForceFloorSolid = ForceFloorSolid;

            for (int i = 0; i < 4; i++)
                result.QAFaces[i] = QAFaces[i];
            for (int i = 0; i < 4; i++)
                result.EDFaces[i] = EDFaces[i];
            for (int i = 0; i < 4; i++)
                result.WSFaces[i] = WSFaces[i];
            for (int i = 0; i < 4; i++)
                result.RFFaces[i] = RFFaces[i];
            for (int i = 0; i < (int)FaceCount; i++)
                result._faceTextures[i] = _faceTextures[i];

            result.FloorSplitDirectionToggled = FloorSplitDirectionToggled;
            result.CeilingSplitDirectionToggled = CeilingSplitDirectionToggled;
            result.FloorDiagonalSplit = FloorDiagonalSplit;
            result.CeilingDiagonalSplit = CeilingDiagonalSplit;
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

        public short[] GetVerticalSubdivision(int verticalSubdivision)
        {
            switch (verticalSubdivision)
            {
                case 0:
                    return QAFaces;
                case 1:
                    return WSFaces;
                case 2:
                    return EDFaces;
                case 3:
                    return RFFaces;
                default:
                    throw new NotSupportedException();
            }
        }

        public short GetEdge(int verticalSubdivision, int edge)
        {
            return GetVerticalSubdivision(verticalSubdivision)[edge];
        }

        public void SetEdge(int verticalSubdivision, int edge, short newValue)
        {
            GetVerticalSubdivision(verticalSubdivision)[edge] = newValue;
            FixHeights(verticalSubdivision);
        }

        public void ChangeEdge(int verticalSubdivision, int edge, short increment, SmoothGeometryEditingType editingType = SmoothGeometryEditingType.Any)
        {
            if (((verticalSubdivision == 0 || verticalSubdivision == 2) && FloorDiagonalSplit == DiagonalSplit.None) ||
               ((verticalSubdivision == 1 || verticalSubdivision == 3) && CeilingDiagonalSplit == DiagonalSplit.None))
            {
                if (editingType == SmoothGeometryEditingType.Any ||
                   (Type == BlockType.Floor && editingType == SmoothGeometryEditingType.Floor) ||
                   (Type != BlockType.Floor && editingType == SmoothGeometryEditingType.Wall))
                {
                    GetVerticalSubdivision(verticalSubdivision)[edge] += increment;
                    FixHeights(verticalSubdivision);
                }
            }
        }
        public void Raise(int verticalSubdivision, bool diagonalStep, short increment)
        {
            var faces = GetVerticalSubdivision(verticalSubdivision);
            var split = (verticalSubdivision == 0 || verticalSubdivision == 2) ? FloorDiagonalSplit : CeilingDiagonalSplit;

            if (diagonalStep)
            {
                switch (split)
                {
                    case DiagonalSplit.XpZn:
                        faces[0] += increment;
                        break;
                    case DiagonalSplit.XnZn:
                        faces[1] += increment;
                        break;
                    case DiagonalSplit.XnZp:
                        faces[2] += increment;
                        break;
                    case DiagonalSplit.XpZp:
                        faces[3] += increment;
                        break;
                }
            }
            else
            {
                for (int i = 0; i < 4; i++)
                {
                    if ((i == 0 && split == DiagonalSplit.XpZn) ||
                        (i == 1 && split == DiagonalSplit.XnZn) ||
                        (i == 2 && split == DiagonalSplit.XnZp) ||
                        (i == 3 && split == DiagonalSplit.XpZp))
                        continue;
                    faces[i] += increment;
                }
            }
        }

        public void RaiseStepWise(int verticalSubdivision, bool diagonalStep, short increment, bool autoSwitch = false)
        {
            var floor = (verticalSubdivision % 2 == 0);
            var split = floor ? FloorDiagonalSplit : CeilingDiagonalSplit;

            if (split != DiagonalSplit.None)
            {
                var faces = GetVerticalSubdivision(verticalSubdivision);
                var stepIsLimited = increment != 0 && ((increment > 0) == (!floor ^ diagonalStep));

                if ((split == DiagonalSplit.XpZn && faces[0] == faces[1] && stepIsLimited) ||
                    (split == DiagonalSplit.XnZn && faces[1] == faces[2] && stepIsLimited) ||
                    (split == DiagonalSplit.XnZp && faces[2] == faces[3] && stepIsLimited) ||
                    (split == DiagonalSplit.XpZp && faces[3] == faces[0] && stepIsLimited))
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
            Unknown,
            Triangle,
            Quad
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
            bool diagonalChange = transformation.MirrorX != ((transformation.QuadrantRotation % 2) != 0);
            bool oldFloorSplitDirectionIsXEqualsZReal = FloorSplitDirectionIsXEqualsZReal;
            if (onlyFloor != false)
            {
                bool requiredFloorSplitDirectionIsXEqualsZ = FloorSplitDirectionIsXEqualsZ != diagonalChange;
                FloorDiagonalSplit = TransformDiagonalSplit(FloorDiagonalSplit, transformation);
                transformation.TransformValueDiagonalQuad(ref QAFaces[FaceXpZp], ref QAFaces[FaceXnZp], ref QAFaces[FaceXnZn], ref QAFaces[FaceXpZn]);
                transformation.TransformValueDiagonalQuad(ref EDFaces[FaceXpZp], ref EDFaces[FaceXnZp], ref EDFaces[FaceXnZn], ref EDFaces[FaceXpZn]);
                if (requiredFloorSplitDirectionIsXEqualsZ != FloorSplitDirectionIsXEqualsZ)
                    FloorSplitDirectionToggled = !FloorSplitDirectionToggled;
            }
            bool oldCeilingSplitDirectionIsXEqualsZReal = CeilingSplitDirectionIsXEqualsZReal;
            if (onlyFloor != true)
            {
                bool requiredCeilingSplitDirectionIsXEqualsZ = CeilingSplitDirectionIsXEqualsZ != diagonalChange;
                CeilingDiagonalSplit = TransformDiagonalSplit(CeilingDiagonalSplit, transformation);
                transformation.TransformValueDiagonalQuad(ref WSFaces[FaceXpZp], ref WSFaces[FaceXnZp], ref WSFaces[FaceXnZn], ref WSFaces[FaceXpZn]);
                transformation.TransformValueDiagonalQuad(ref RFFaces[FaceXpZp], ref RFFaces[FaceXnZp], ref RFFaces[FaceXnZn], ref RFFaces[FaceXpZn]);
                if (requiredCeilingSplitDirectionIsXEqualsZ != CeilingSplitDirectionIsXEqualsZ)
                    CeilingSplitDirectionToggled = !CeilingSplitDirectionToggled;
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
                if (FloorIsQuad)
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
                if (CeilingIsQuad)
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
                EDFaces[i] = Math.Min(EDFaces[i], QAFaces[i]);
                RFFaces[i] = Math.Max(RFFaces[i], WSFaces[i]);

                if (verticalSubdivision == 0 || verticalSubdivision == 2 || verticalSubdivision == -1)
                    if (FloorDiagonalSplit != DiagonalSplit.None)
                        QAFaces[i] = Math.Min(QAFaces[i], CeilingMin);
                    else
                        QAFaces[i] = Math.Min(QAFaces[i], WSFaces[i]);

                if (verticalSubdivision == 1 || verticalSubdivision == 3 || verticalSubdivision == -1)
                    if (CeilingDiagonalSplit != DiagonalSplit.None)
                        WSFaces[i] = Math.Max(WSFaces[i], FloorMax);
                    else
                        WSFaces[i] = Math.Max(WSFaces[i], QAFaces[i]);
            }
        }

        private static int FindHorizontalTriangle(int h1, int h2, int h3, int h4)
        {
            var p1 = new Vector3(0, h1, 0);
            var p2 = new Vector3(1, h2, 0);
            var p3 = new Vector3(1, h3, 1);
            var p4 = new Vector3(0, h4, 1);

            var plane = new Plane(p1, p2, p4);
            if (plane.Normal == Vector3.UnitY || plane.Normal == -Vector3.UnitY)
                return 0;

            plane = new Plane(p1, p2, p3);
            if (plane.Normal == Vector3.UnitY || plane.Normal == -Vector3.UnitY)
                return 1;

            plane = new Plane(p2, p3, p4);
            if (plane.Normal == Vector3.UnitY || plane.Normal == -Vector3.UnitY)
                return 2;

            plane = new Plane(p3, p4, p1);
            if (plane.Normal == Vector3.UnitY || plane.Normal == -Vector3.UnitY)
                return 3;

            return -1;
        }

        public Vector3[] GetFloorTriangleNormals()
        {
            Plane[] tri = new Plane[2];

            var p0 = new Vector3(0, QAFaces[0], 0);
            var p1 = new Vector3(4, QAFaces[1], 0);
            var p2 = new Vector3(4, QAFaces[2], -4);
            var p3 = new Vector3(0, QAFaces[3], -4);

            // Create planes based on floor split direction

            if (FloorSplitDirectionIsXEqualsZ)
            {
                tri[0] = new Plane(p1, p2, p3);
                tri[1] = new Plane(p1, p3, p0);
            }
            else
            {
                tri[0] = new Plane(p0, p1, p2);
                tri[1] = new Plane(p0, p2, p3);
            }

            return new Vector3[2] { tri[0].Normal, tri[1].Normal };
        }

        public short GetTriangleMinimumFloorPoint(int triangle)
        {
            if (triangle != 0 && triangle != 1)
                return 0;

            if (FloorSplitDirectionIsXEqualsZ)
            {
                if (triangle == 0)
                    return Math.Min(Math.Min(QAFaces[1], QAFaces[2]), QAFaces[3]);
                else
                    return Math.Min(Math.Min(QAFaces[1], QAFaces[3]), QAFaces[0]);
            }
            else
            {
                if (triangle == 0)
                    return Math.Min(Math.Min(QAFaces[0], QAFaces[1]), QAFaces[2]);
                else
                    return Math.Min(Math.Min(QAFaces[0], QAFaces[2]), QAFaces[3]);
            }
        }

        public Direction[] GetFloorTriangleSlopeDirections()
        {
            var normals = GetFloorTriangleNormals();

            // Initialize slope directions as unslidable by default (EntireFace means unslidable in our case).

            Direction[] slopeDirections = new Direction[2] { Direction.None, Direction.None };

            if (FloorHasSlope)
            {
                for (int i = 0; i < (FloorIsQuad ? 1 : 2); i++) // If floor is quad, we don't solve second triangle
                {
                    if (Math.Abs(normals[i].Y) <= CriticalSlantComponent) // Triangle is slidable
                    {
                        bool angleNotDefined = true;
                        var angle = MathUtilEx.RadiansToDegrees((float)Math.Atan2(normals[i].X, normals[i].Z));
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

                if (FloorDiagonalSplit != DiagonalSplit.None)
                {
                    switch (FloorDiagonalSplit)
                    {
                        case DiagonalSplit.XpZn:
                            slopeDirections[1] = slopeDirections[0];
                            slopeDirections[0] = Direction.None;
                            break;

                        case DiagonalSplit.XnZp:
                            if (!FloorIsQuad)
                            {
                                slopeDirections[0] = slopeDirections[1];
                                slopeDirections[1] = Direction.None;
                            }
                            break;

                        case DiagonalSplit.XnZn:
                            if (FloorIsQuad)
                            {
                                slopeDirections[1] = slopeDirections[0];
                                slopeDirections[0] = Direction.None;
                            }
                            else
                                slopeDirections[0] = Direction.None;
                            break;

                        case DiagonalSplit.XpZp:
                            if (!FloorIsQuad)
                                slopeDirections[1] = Direction.None;
                            break;
                    }
                }
            }

            return slopeDirections;
        }

        public short GetFaceMin(int verticalSubdivision, Direction direction = Direction.None)
        {
            var faces = GetVerticalSubdivision(verticalSubdivision);

            if (direction == Direction.None)
                return Math.Min(Math.Min(faces[0], faces[1]), Math.Min(faces[2], faces[3]));
            else
            {
                switch (direction)
                {
                    case Direction.PositiveZ:
                        return Math.Min(faces[0], faces[1]);
                    case Direction.NegativeZ:
                        return Math.Min(faces[2], faces[3]);
                    case Direction.PositiveX:
                        return Math.Min(faces[1], faces[2]);
                    case Direction.NegativeX:
                        return Math.Min(faces[0], faces[3]);
                    default:
                        return 0;
                }
            }
        }

        public short GetFaceMax(int verticalSubdivision, Direction direction = Direction.None)
        {
            var faces = GetVerticalSubdivision(verticalSubdivision);

            if (direction == Direction.None)
                return Math.Max(Math.Max(faces[0], faces[1]), Math.Max(faces[2], faces[3]));
            else
            {
                switch (direction)
                {
                    case Direction.PositiveZ:
                        return Math.Max(faces[0], faces[1]);
                    case Direction.NegativeZ:
                        return Math.Max(faces[2], faces[3]);
                    case Direction.PositiveX:
                        return Math.Max(faces[1], faces[2]);
                    case Direction.NegativeX:
                        return Math.Max(faces[0], faces[3]);
                    default:
                        return 0;
                }
            }
        }

        public bool FloorSplitDirectionIsXEqualsZ
        {
            get
            {
                int h1 = QAFaces[0], h2 = QAFaces[1], h3 = QAFaces[2], h4 = QAFaces[3];
                int horizontalTriangle = FindHorizontalTriangle(h1, h2, h3, h4);

                switch (horizontalTriangle)
                {
                    case 0:
                        return !FloorSplitDirectionToggled;
                    case 1:
                        return FloorSplitDirectionToggled;
                    case 2:
                        return !FloorSplitDirectionToggled;
                    case 3:
                        return FloorSplitDirectionToggled;
                    default:
                        int min = Math.Min(Math.Min(Math.Min(h1, h2), h3), h4);
                        int max = Math.Max(Math.Max(Math.Max(h1, h2), h3), h4);

                        if (h1 == h3 && h2 == h4 && h2 != h3)
                            return FloorSplitDirectionToggled;

                        if (min == h1 && min == h3)
                            return FloorSplitDirectionToggled;
                        if (min == h2 && min == h4)
                            return !FloorSplitDirectionToggled;

                        if (min == h1 && max == h3)
                            return !FloorSplitDirectionToggled;
                        if (min == h2 && max == h4)
                            return FloorSplitDirectionToggled;
                        if (min == h3 && max == h1)
                            return !FloorSplitDirectionToggled;
                        if (min == h4 && max == h2)
                            return FloorSplitDirectionToggled;

                        return !FloorSplitDirectionToggled;
                }
            }
            set
            {
                if (value != FloorSplitDirectionIsXEqualsZ)
                    FloorSplitDirectionToggled = !FloorSplitDirectionToggled;
            }
        }

        public bool CeilingSplitDirectionIsXEqualsZ
        {
            get
            {
                int h1 = WSFaces[0], h2 = WSFaces[1], h3 = WSFaces[2], h4 = WSFaces[3];
                int horizontalTriangle = FindHorizontalTriangle(h1, h2, h3, h4);

                switch (horizontalTriangle)
                {
                    case 0:
                        return !CeilingSplitDirectionToggled;
                    case 1:
                        return CeilingSplitDirectionToggled;
                    case 2:
                        return !CeilingSplitDirectionToggled;
                    case 3:
                        return CeilingSplitDirectionToggled;
                    default:
                        int min = Math.Min(Math.Min(Math.Min(h1, h2), h3), h4);
                        int max = Math.Max(Math.Max(Math.Max(h1, h2), h3), h4);

                        if (h1 == h3 && h2 == h4 && h2 != h3)
                            return FloorSplitDirectionToggled;

                        if (max == h1 && max == h3)
                            return CeilingSplitDirectionToggled;
                        if (max == h2 && max == h4)
                            return !CeilingSplitDirectionToggled;

                        if (min == h1 && max == h3)
                            return !CeilingSplitDirectionToggled;
                        if (min == h2 && max == h4)
                            return CeilingSplitDirectionToggled;
                        if (min == h3 && max == h1)
                            return !CeilingSplitDirectionToggled;
                        if (min == h4 && max == h2)
                            return CeilingSplitDirectionToggled;

                        return !CeilingSplitDirectionToggled;
                }
            }
            set
            {
                if (value != CeilingSplitDirectionIsXEqualsZ)
                    CeilingSplitDirectionToggled = !CeilingSplitDirectionToggled;
            }
        }

        /// <summary>Checks for FloorDiagonalSplit and takes priority</summary>
        public bool FloorSplitDirectionIsXEqualsZReal
        {
            get
            {
                switch (FloorDiagonalSplit)
                {
                    case DiagonalSplit.XnZn:
                    case DiagonalSplit.XpZp:
                        return false;
                    case DiagonalSplit.XpZn:
                    case DiagonalSplit.XnZp:
                        return true;
                    case DiagonalSplit.None:
                        return FloorSplitDirectionIsXEqualsZ;
                    default:
                        throw new ApplicationException("\"FloorDiagonalSplit\" in unknown state.");
                }
            }
        }

        /// <summary>Checks for CeilingDiagonalSplit and takes priority</summary>
        public bool CeilingSplitDirectionIsXEqualsZReal
        {
            get
            {
                switch (CeilingDiagonalSplit)
                {
                    case DiagonalSplit.XnZn:
                    case DiagonalSplit.XpZp:
                        return false;
                    case DiagonalSplit.XpZn:
                    case DiagonalSplit.XnZp:
                        return true;
                    case DiagonalSplit.None:
                        return CeilingSplitDirectionIsXEqualsZ;
                    default:
                        throw new ApplicationException("\"CeilingDiagonalSplit\" in unknown state.");
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool HasFlag(BlockFlags flag)
        {
            return (Flags & flag) == flag;
        }

        public static bool IsQuad(int hXnZp, int hXpZp, int hXpZn, int hXnZn)
        {
            return (hXpZp - hXpZn) == (hXnZp - hXnZn) &&
                (hXpZp - hXnZp) == (hXpZn - hXnZn);
        }

        public bool IsAnyWall => Type != BlockType.Floor;
        public bool IsAnyPortal => FloorPortal != null || CeilingPortal != null || WallPortal != null;
        public bool FloorIsQuad => FloorDiagonalSplit == DiagonalSplit.None && IsQuad(QAFaces[FaceXnZp], QAFaces[FaceXpZp], QAFaces[FaceXpZn], QAFaces[FaceXnZn]);
        public bool CeilingIsQuad => CeilingDiagonalSplit == DiagonalSplit.None && IsQuad(WSFaces[FaceXnZp], WSFaces[FaceXpZp], WSFaces[FaceXpZn], WSFaces[FaceXnZn]);
        public bool FloorHasSlope => FloorMax - FloorMin > 2;
        public bool CeilingHasSlope => CeilingMax - CeilingMin > 2;
        public int FloorIfQuadSlopeX => FloorIsQuad ? QAFaces[FaceXpZp] - QAFaces[FaceXnZp] : 0;
        public int FloorIfQuadSlopeZ => FloorIsQuad ? QAFaces[FaceXpZp] - QAFaces[FaceXpZn] : 0;
        public int CeilingIfQuadSlopeX => CeilingIsQuad ? WSFaces[FaceXpZp] - WSFaces[FaceXnZp] : 0;
        public int CeilingIfQuadSlopeZ => CeilingIsQuad ? WSFaces[FaceXpZn] - WSFaces[FaceXpZp] : 0;
        public short FloorMax => GetFaceMax(0);
        public short FloorMin => GetFaceMin(0);
        public short CeilingMax => GetFaceMax(1);
        public short CeilingMin => GetFaceMin(1);
    }
}
