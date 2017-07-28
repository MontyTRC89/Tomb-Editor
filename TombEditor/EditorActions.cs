using SharpDX;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using NLog;
using TombEditor.Geometry;

namespace TombEditor
{
    public class EditorActions
    {
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();

        public enum FaceEditorActions
        {
            EntireFace,
            EdgeN,
            EdgeE,
            EdgeS,
            EdgeW,
            CornerNW,
            CornerNE,
            CornerSE,
            CornerSW,
            DiagonalFloorCorner,
            DiagonalCeilingCorner
        }

        public enum FaceSubdivisions
        {
            Q,
            A,
            W,
            S,
            E,
            D,
            R,
            F
        }

        public enum MoveObjectDirections
        {
            North,
            South,
            East,
            West,
            Up,
            Down
        }

        public enum ObjectType
        {
            Moveable,
            StaticMesh,
            Light,
            SoundSource,
            Sink,
            Camera,
            FlybyCamera
        }

        private static Editor _editor = Editor.Instance;

        public static void EditFace(int xMin, int xMax, int zMin, int zMax, FaceEditorActions action, FaceSubdivisions sub)
        {
            int face = 0;
            short increment = 0;

            switch (sub)
            {
                case FaceSubdivisions.Q:
                    face = 0;
                    increment = 1;
                    break;

                case FaceSubdivisions.A:
                    face = 0;
                    increment = -1;
                    break;

                case FaceSubdivisions.W:
                    face = 1;
                    increment = 1;
                    break;

                case FaceSubdivisions.S:
                    face = 1;
                    increment = -1;
                    break;

                case FaceSubdivisions.E:
                    face = 2;
                    increment = 1;
                    break;

                case FaceSubdivisions.D:
                    face = 2;
                    increment = -1;
                    break;

                case FaceSubdivisions.R:
                    face = 3;
                    increment = 1;
                    break;

                case FaceSubdivisions.F:
                    face = 3;
                    increment = -1;
                    break;

                default:
                    return;
            }

            for (int x = xMin; x <= xMax; x++)
            {
                for (int z = zMin; z <= zMax; z++)
                {
                    Block block = _editor.Level.Rooms[_editor.RoomIndex].Blocks[x, z];

                    switch (action)
                    {
                        case FaceEditorActions.EntireFace:
                            if (face == 0)
                            {
                                if (block.FloorDiagonalSplit == DiagonalSplit.NW && block.QAFaces[2] == block.QAFaces[0] && increment < 0)
                                    continue;
                                if (block.FloorDiagonalSplit == DiagonalSplit.NE && block.QAFaces[3] == block.QAFaces[1] && increment < 0)
                                    continue;
                                if (block.FloorDiagonalSplit == DiagonalSplit.SE && block.QAFaces[0] == block.QAFaces[2] && increment < 0)
                                    continue;
                                if (block.FloorDiagonalSplit == DiagonalSplit.SW && block.QAFaces[1] == block.QAFaces[3] && increment < 0)
                                    continue;

                                if (block.FloorDiagonalSplit != DiagonalSplit.NW)
                                    _editor.Level.Rooms[_editor.RoomIndex].Blocks[x, z].QAFaces[0] += increment;
                                if (block.FloorDiagonalSplit != DiagonalSplit.NE)
                                    _editor.Level.Rooms[_editor.RoomIndex].Blocks[x, z].QAFaces[1] += increment;
                                if (block.FloorDiagonalSplit != DiagonalSplit.SE)
                                    _editor.Level.Rooms[_editor.RoomIndex].Blocks[x, z].QAFaces[2] += increment;
                                if (block.FloorDiagonalSplit != DiagonalSplit.SW)
                                    _editor.Level.Rooms[_editor.RoomIndex].Blocks[x, z].QAFaces[3] += increment;

                                if (block.FloorPortal != -1 && !block.IsFloorSolid)
                                    continue;
                            }
                            else if (face == 1)
                            {
                                if (block.CeilingDiagonalSplit == DiagonalSplit.NW && block.WSFaces[2] == block.WSFaces[0] && increment > 0)
                                    continue;
                                if (block.CeilingDiagonalSplit == DiagonalSplit.NE && block.WSFaces[3] == block.WSFaces[1] && increment > 0)
                                    continue;
                                if (block.CeilingDiagonalSplit == DiagonalSplit.SE && block.WSFaces[0] == block.WSFaces[2] && increment > 0)
                                    continue;
                                if (block.CeilingDiagonalSplit == DiagonalSplit.SW && block.WSFaces[1] == block.WSFaces[3] && increment > 0)
                                    continue;

                                if (block.CeilingDiagonalSplit != DiagonalSplit.NW)
                                    _editor.Level.Rooms[_editor.RoomIndex].Blocks[x, z].WSFaces[0] += increment;
                                if (block.CeilingDiagonalSplit != DiagonalSplit.NE)
                                    _editor.Level.Rooms[_editor.RoomIndex].Blocks[x, z].WSFaces[1] += increment;
                                if (block.CeilingDiagonalSplit != DiagonalSplit.SE)
                                    _editor.Level.Rooms[_editor.RoomIndex].Blocks[x, z].WSFaces[2] += increment;
                                if (block.CeilingDiagonalSplit != DiagonalSplit.SW)
                                    _editor.Level.Rooms[_editor.RoomIndex].Blocks[x, z].WSFaces[3] += increment;
                            }
                            else if (face == 2)
                            {
                                _editor.Level.Rooms[_editor.RoomIndex].Blocks[x, z].EDFaces[0] += increment;
                                _editor.Level.Rooms[_editor.RoomIndex].Blocks[x, z].EDFaces[1] += increment;
                                _editor.Level.Rooms[_editor.RoomIndex].Blocks[x, z].EDFaces[2] += increment;
                                _editor.Level.Rooms[_editor.RoomIndex].Blocks[x, z].EDFaces[3] += increment;

                                if (block.FloorPortal != -1 && !block.IsFloorSolid)
                                    continue;
                            }
                            else if (face == 3)
                            {
                                _editor.Level.Rooms[_editor.RoomIndex].Blocks[x, z].RFFaces[0] += increment;
                                _editor.Level.Rooms[_editor.RoomIndex].Blocks[x, z].RFFaces[1] += increment;
                                _editor.Level.Rooms[_editor.RoomIndex].Blocks[x, z].RFFaces[2] += increment;
                                _editor.Level.Rooms[_editor.RoomIndex].Blocks[x, z].RFFaces[3] += increment;
                            }
                            break;

                        case FaceEditorActions.EdgeN:
                            if (face == 0)
                            {
                                if (block.Type != BlockType.Wall && block.FloorDiagonalSplit != DiagonalSplit.None)
                                    continue;

                                if (block.FloorDiagonalSplit != DiagonalSplit.NW)
                                    _editor.Level.Rooms[_editor.RoomIndex].Blocks[x, z].QAFaces[0] += increment;
                                if (block.FloorDiagonalSplit != DiagonalSplit.NE)
                                    _editor.Level.Rooms[_editor.RoomIndex].Blocks[x, z].QAFaces[1] += increment;

                                if (block.FloorPortal != -1 && !block.IsFloorSolid)
                                    continue;
                            }
                            else if (face == 1)
                            {
                                if (block.Type != BlockType.Wall && block.FloorDiagonalSplit != DiagonalSplit.None)
                                    continue;

                                if (block.CeilingDiagonalSplit != DiagonalSplit.SE)
                                    _editor.Level.Rooms[_editor.RoomIndex].Blocks[x, z].WSFaces[2] += increment;
                                if (block.CeilingDiagonalSplit != DiagonalSplit.SW)
                                    _editor.Level.Rooms[_editor.RoomIndex].Blocks[x, z].WSFaces[3] += increment;
                            }
                            else if (face == 2)
                            {
                                if (block.Type != BlockType.Wall && block.FloorDiagonalSplit != DiagonalSplit.None)
                                    continue;

                                _editor.Level.Rooms[_editor.RoomIndex].Blocks[x, z].EDFaces[0] += increment;
                                _editor.Level.Rooms[_editor.RoomIndex].Blocks[x, z].EDFaces[1] += increment;

                                if (block.FloorPortal != -1 && !block.IsFloorSolid)
                                    continue;
                            }
                            else if (face == 3)
                            {
                                if (block.Type != BlockType.Wall && block.FloorDiagonalSplit != DiagonalSplit.None)
                                    continue;

                                _editor.Level.Rooms[_editor.RoomIndex].Blocks[x, z].RFFaces[2] += increment;
                                _editor.Level.Rooms[_editor.RoomIndex].Blocks[x, z].RFFaces[3] += increment;
                            }
                            break;

                        case FaceEditorActions.EdgeE:
                            if (face == 0)
                            {
                                if (block.Type != BlockType.Wall && block.FloorDiagonalSplit != DiagonalSplit.None)
                                    continue;

                                if (block.FloorDiagonalSplit != DiagonalSplit.NE)
                                    _editor.Level.Rooms[_editor.RoomIndex].Blocks[x, z].QAFaces[1] += increment;
                                if (block.FloorDiagonalSplit != DiagonalSplit.SE)
                                    _editor.Level.Rooms[_editor.RoomIndex].Blocks[x, z].QAFaces[2] += increment;

                                if (block.FloorPortal != -1 && !block.IsFloorSolid)
                                    continue;
                            }
                            else if (face == 1)
                            {
                                if (block.Type != BlockType.Wall && block.FloorDiagonalSplit != DiagonalSplit.None)
                                    continue;

                                if (block.CeilingDiagonalSplit != DiagonalSplit.NW)
                                    _editor.Level.Rooms[_editor.RoomIndex].Blocks[x, z].WSFaces[0] += increment;
                                if (block.CeilingDiagonalSplit != DiagonalSplit.SW)
                                    _editor.Level.Rooms[_editor.RoomIndex].Blocks[x, z].WSFaces[3] += increment;
                            }
                            else if (face == 2)
                            {
                                _editor.Level.Rooms[_editor.RoomIndex].Blocks[x, z].EDFaces[1] += increment;
                                _editor.Level.Rooms[_editor.RoomIndex].Blocks[x, z].EDFaces[2] += increment;

                                if (block.FloorPortal != -1 && !block.IsFloorSolid)
                                    continue;
                            }
                            else if (face == 3)
                            {
                                _editor.Level.Rooms[_editor.RoomIndex].Blocks[x, z].RFFaces[0] += increment;
                                _editor.Level.Rooms[_editor.RoomIndex].Blocks[x, z].RFFaces[3] += increment;
                            }
                            break;

                        case FaceEditorActions.EdgeS:
                            if (face == 0)
                            {
                                if (block.Type != BlockType.Wall && block.FloorDiagonalSplit != DiagonalSplit.None)
                                    continue;

                                if (block.FloorDiagonalSplit != DiagonalSplit.SE)
                                    _editor.Level.Rooms[_editor.RoomIndex].Blocks[x, z].QAFaces[2] += increment;
                                if (block.FloorDiagonalSplit != DiagonalSplit.SW)
                                    _editor.Level.Rooms[_editor.RoomIndex].Blocks[x, z].QAFaces[3] += increment;

                                if (block.FloorPortal != -1 && !block.IsFloorSolid)
                                    continue;
                            }
                            else if (face == 1)
                            {
                                if (block.Type != BlockType.Wall && block.FloorDiagonalSplit != DiagonalSplit.None)
                                    continue;

                                if (block.CeilingDiagonalSplit != DiagonalSplit.NW)
                                    _editor.Level.Rooms[_editor.RoomIndex].Blocks[x, z].WSFaces[0] += increment;
                                if (block.CeilingDiagonalSplit != DiagonalSplit.NE)
                                    _editor.Level.Rooms[_editor.RoomIndex].Blocks[x, z].WSFaces[1] += increment;
                            }
                            else if (face == 2)
                            {
                                _editor.Level.Rooms[_editor.RoomIndex].Blocks[x, z].EDFaces[0] += increment;
                                _editor.Level.Rooms[_editor.RoomIndex].Blocks[x, z].EDFaces[1] += increment;

                                if (block.FloorPortal != -1 && !block.IsFloorSolid)
                                    continue;
                            }
                            else if (face == 3)
                            {
                                _editor.Level.Rooms[_editor.RoomIndex].Blocks[x, z].RFFaces[2] += increment;
                                _editor.Level.Rooms[_editor.RoomIndex].Blocks[x, z].RFFaces[3] += increment;
                            }
                            break;

                        case FaceEditorActions.EdgeW:
                            if (face == 0)
                            {
                                if (block.Type != BlockType.Wall && block.FloorDiagonalSplit != DiagonalSplit.None)
                                    continue;

                                if (block.FloorDiagonalSplit != DiagonalSplit.NW)
                                    _editor.Level.Rooms[_editor.RoomIndex].Blocks[x, z].QAFaces[0] += increment;
                                if (block.FloorDiagonalSplit != DiagonalSplit.SW)
                                    _editor.Level.Rooms[_editor.RoomIndex].Blocks[x, z].QAFaces[3] += increment;

                                if (block.FloorPortal != -1 && !block.IsFloorSolid)
                                    continue;
                            }
                            else if (face == 1)
                            {
                                if (block.Type != BlockType.Wall && block.FloorDiagonalSplit != DiagonalSplit.None)
                                    continue;

                                if (block.CeilingDiagonalSplit != DiagonalSplit.NE)
                                    _editor.Level.Rooms[_editor.RoomIndex].Blocks[x, z].WSFaces[1] += increment;
                                if (block.CeilingDiagonalSplit != DiagonalSplit.SE)
                                    _editor.Level.Rooms[_editor.RoomIndex].Blocks[x, z].WSFaces[2] += increment;
                            }
                            else if (face == 2)
                            {
                                _editor.Level.Rooms[_editor.RoomIndex].Blocks[x, z].EDFaces[0] += increment;
                                _editor.Level.Rooms[_editor.RoomIndex].Blocks[x, z].EDFaces[3] += increment;

                                if (block.FloorPortal != -1 && !block.IsFloorSolid)
                                    continue;
                            }
                            else if (face == 3)
                            {
                                _editor.Level.Rooms[_editor.RoomIndex].Blocks[x, z].RFFaces[1] += increment;
                                _editor.Level.Rooms[_editor.RoomIndex].Blocks[x, z].RFFaces[2] += increment;
                            }
                            break;

                        case FaceEditorActions.CornerNW:
                            if (face == 0)
                            {
                                if (block.Type != BlockType.Wall && block.FloorDiagonalSplit != DiagonalSplit.SE && block.FloorDiagonalSplit != DiagonalSplit.None)
                                    continue;

                                if (block.Type != BlockType.Wall && block.FloorDiagonalSplit == DiagonalSplit.SE)
                                {
                                    if (block.QAFaces[0] == block.QAFaces[1] && increment < 0)
                                        continue;
                                }

                                _editor.Level.Rooms[_editor.RoomIndex].Blocks[x, z].QAFaces[0] += increment;

                                if (block.FloorPortal != -1 && !block.IsFloorSolid)
                                    continue;
                            }
                            else if (face == 1)
                            {
                                if (block.CeilingDiagonalSplit != DiagonalSplit.NE && block.CeilingDiagonalSplit != DiagonalSplit.None)
                                    continue;

                                if (block.CeilingDiagonalSplit == DiagonalSplit.NE)
                                {
                                    if (block.WSFaces[3] == block.WSFaces[0] && increment > 0)
                                        continue;
                                }

                                _editor.Level.Rooms[_editor.RoomIndex].Blocks[x, z].WSFaces[3] += increment;
                            }
                            else if (face == 2)
                            {
                                _editor.Level.Rooms[_editor.RoomIndex].Blocks[x, z].EDFaces[0] += increment;

                                if (block.FloorPortal != -1 && !block.IsFloorSolid)
                                    continue;
                            }
                            else if (face == 3)
                            {
                                _editor.Level.Rooms[_editor.RoomIndex].Blocks[x, z].RFFaces[3] += increment;
                            }
                            break;

                        case FaceEditorActions.CornerNE:
                            if (face == 0)
                            {
                                if (block.Type != BlockType.Wall && block.FloorDiagonalSplit != DiagonalSplit.SW && block.FloorDiagonalSplit != DiagonalSplit.None)
                                    continue;

                                if (block.Type != BlockType.Wall && block.FloorDiagonalSplit == DiagonalSplit.SW)
                                {
                                    if (block.QAFaces[1] == block.QAFaces[2] && increment < 0)
                                        continue;
                                }

                                _editor.Level.Rooms[_editor.RoomIndex].Blocks[x, z].QAFaces[1] += increment;

                                if (block.FloorPortal != -1 && !block.IsFloorSolid)
                                    continue;
                            }
                            else if (face == 1)
                            {
                                if (block.CeilingDiagonalSplit != DiagonalSplit.NW && block.CeilingDiagonalSplit != DiagonalSplit.None)
                                    continue;

                                if (block.CeilingDiagonalSplit == DiagonalSplit.NW)
                                {
                                    if (block.WSFaces[1] == block.WSFaces[2] && increment > 0)
                                        continue;
                                }

                                _editor.Level.Rooms[_editor.RoomIndex].Blocks[x, z].WSFaces[2] += increment;
                            }
                            else if (face == 2)
                            {
                                _editor.Level.Rooms[_editor.RoomIndex].Blocks[x, z].EDFaces[1] += increment;

                                if (block.FloorPortal != -1 && !block.IsFloorSolid)
                                    continue;
                            }
                            else if (face == 3)
                            {
                                _editor.Level.Rooms[_editor.RoomIndex].Blocks[x, z].RFFaces[2] += increment;
                            }
                            break;

                        case FaceEditorActions.CornerSE:
                            if (face == 0)
                            {
                                if (block.Type != BlockType.Wall && block.FloorDiagonalSplit != DiagonalSplit.NW && block.FloorDiagonalSplit != DiagonalSplit.None)
                                    continue;

                                if (block.Type != BlockType.Wall && block.FloorDiagonalSplit == DiagonalSplit.NW)
                                {
                                    if (block.QAFaces[2] == block.QAFaces[3] && increment < 0)
                                        continue;
                                }

                                _editor.Level.Rooms[_editor.RoomIndex].Blocks[x, z].QAFaces[2] += increment;

                                if (block.FloorPortal != -1 && !block.IsFloorSolid)
                                    continue;
                            }
                            else if (face == 1)
                            {
                                if (block.CeilingDiagonalSplit != DiagonalSplit.SW && block.CeilingDiagonalSplit != DiagonalSplit.None)
                                    continue;

                                if (block.CeilingDiagonalSplit == DiagonalSplit.SW)
                                {
                                    if (block.WSFaces[1] == block.WSFaces[2] && increment > 0)
                                        continue;
                                }

                                _editor.Level.Rooms[_editor.RoomIndex].Blocks[x, z].WSFaces[1] += increment;
                            }
                            else if (face == 2)
                            {
                                _editor.Level.Rooms[_editor.RoomIndex].Blocks[x, z].EDFaces[2] += increment;

                                if (block.FloorPortal != -1 && !block.IsFloorSolid)
                                    continue;
                            }
                            else if (face == 3)
                            {
                                _editor.Level.Rooms[_editor.RoomIndex].Blocks[x, z].RFFaces[1] += increment;
                            }
                            break;

                        case FaceEditorActions.CornerSW:
                            if (face == 0)
                            {
                                if (block.Type != BlockType.Wall && block.FloorDiagonalSplit != DiagonalSplit.NE && block.FloorDiagonalSplit != DiagonalSplit.None)
                                    continue;

                                if (block.Type != BlockType.Wall && block.FloorDiagonalSplit == DiagonalSplit.NE)
                                {
                                    if (block.QAFaces[3] == block.QAFaces[0] && increment < 0)
                                        continue;
                                }

                                _editor.Level.Rooms[_editor.RoomIndex].Blocks[x, z].QAFaces[3] += increment;

                                if (block.FloorPortal != -1 && !block.IsFloorSolid)
                                    continue;
                            }
                            else if (face == 1)
                            {
                                if (block.CeilingDiagonalSplit != DiagonalSplit.SE && block.CeilingDiagonalSplit != DiagonalSplit.None)
                                    continue;

                                if (block.CeilingDiagonalSplit == DiagonalSplit.SE)
                                {
                                    if (block.WSFaces[3] == block.WSFaces[0] && increment > 0)
                                        continue;
                                }

                                _editor.Level.Rooms[_editor.RoomIndex].Blocks[x, z].WSFaces[0] += increment;
                            }
                            else if (face == 2)
                            {
                                _editor.Level.Rooms[_editor.RoomIndex].Blocks[x, z].EDFaces[3] += increment;

                                if (block.FloorPortal != -1 && !block.IsFloorSolid)
                                    continue;
                            }
                            else if (face == 3)
                            {
                                _editor.Level.Rooms[_editor.RoomIndex].Blocks[x, z].RFFaces[0] += increment;
                            }
                            break;

                        case FaceEditorActions.DiagonalFloorCorner:
                            if (block.FloorDiagonalSplit == DiagonalSplit.NW)
                            {
                                if (block.QAFaces[0] == block.QAFaces[1] && increment > 0)
                                    continue;
                                _editor.Level.Rooms[_editor.RoomIndex].Blocks[x, z].QAFaces[0] += increment;
                            }

                            if (block.FloorDiagonalSplit == DiagonalSplit.NE)
                            {
                                if (block.QAFaces[1] == block.QAFaces[2] && increment > 0)
                                    continue;
                                _editor.Level.Rooms[_editor.RoomIndex].Blocks[x, z].QAFaces[1] += increment;
                            }

                            if (block.FloorDiagonalSplit == DiagonalSplit.SE)
                            {
                                if (block.QAFaces[2] == block.QAFaces[3] && increment > 0)
                                    continue;
                                _editor.Level.Rooms[_editor.RoomIndex].Blocks[x, z].QAFaces[2] += increment;
                            }

                            if (block.FloorDiagonalSplit == DiagonalSplit.SW)
                            {
                                if (block.QAFaces[3] == block.QAFaces[0] && increment > 0)
                                    continue;
                                _editor.Level.Rooms[_editor.RoomIndex].Blocks[x, z].QAFaces[3] += increment;
                            }

                            break;

                        case FaceEditorActions.DiagonalCeilingCorner:
                            if (block.CeilingDiagonalSplit == DiagonalSplit.NW)
                            {
                                if (block.WSFaces[0] == block.WSFaces[1] && increment < 0)
                                    continue;
                                _editor.Level.Rooms[_editor.RoomIndex].Blocks[x, z].WSFaces[0] += increment;
                            }

                            if (block.CeilingDiagonalSplit == DiagonalSplit.NE)
                            {
                                if (block.WSFaces[1] == block.WSFaces[2] && increment < 0)
                                    continue;
                                _editor.Level.Rooms[_editor.RoomIndex].Blocks[x, z].WSFaces[1] += increment;
                            }

                            if (block.CeilingDiagonalSplit == DiagonalSplit.SE)
                            {
                                if (block.WSFaces[2] == block.WSFaces[3] && increment < 0)
                                    continue;
                                _editor.Level.Rooms[_editor.RoomIndex].Blocks[x, z].WSFaces[2] += increment;
                            }

                            if (block.CeilingDiagonalSplit == DiagonalSplit.SW)
                            {
                                if (block.WSFaces[3] == block.WSFaces[0] && increment < 0)
                                    continue;
                                _editor.Level.Rooms[_editor.RoomIndex].Blocks[x, z].WSFaces[3] += increment;
                            }

                            break;
                    }
                }
            }

            SmartBuildGeometry(_editor.RoomIndex, xMin, xMax + 1, zMin, zMax + 1);
        }

        public static void SmartBuildGeometry(int r, int xMin, int xMax, int zMin, int zMax)
        {
            System.Diagnostics.Stopwatch watch = new System.Diagnostics.Stopwatch();
            watch.Start();

            _editor.Level.Rooms[r].BuildGeometry(Math.Max(0, xMin - 1), Math.Min(_editor.Level.Rooms[_editor.RoomIndex].NumXSectors, xMax + 1),
                                                                 Math.Max(0, zMin - 1), Math.Min(_editor.Level.Rooms[_editor.RoomIndex].NumZSectors, zMax + 1));
            _editor.Level.Rooms[r].CalculateLightingForThisRoom();
            _editor.Level.Rooms[r].UpdateBuffers();
            _editor.Level.Rooms[r].AdjustObjectsHeight();

            List<int> portalsToTravel = new List<int>();

            for (int x = 0; x < _editor.Level.Rooms[r].NumXSectors; x++)
            {
                for (int z = 0; z < _editor.Level.Rooms[r].NumZSectors; z++)
                {
                    int wallPortal = _editor.Level.Rooms[r].Blocks[x, z].WallPortal;
                    int ceilingPortal = _editor.Level.Rooms[r].Blocks[x, z].CeilingPortal;
                    int floorPortal = _editor.Level.Rooms[r].Blocks[x, z].FloorPortal;

                    if (wallPortal != -1 && !portalsToTravel.Contains(wallPortal))
                        portalsToTravel.Add(wallPortal);

                    if (ceilingPortal != -1 && !portalsToTravel.Contains(ceilingPortal))
                        portalsToTravel.Add(ceilingPortal);

                    if (floorPortal != -1 && !portalsToTravel.Contains(floorPortal))
                        portalsToTravel.Add(floorPortal);
                }
            }

            //Parallel.ForEach(portalsToTravel, p =>
            // {
            List<int> roomsProcessed = new List<int>();

            for (int p = 0; p < portalsToTravel.Count; p++)
            {
                Portal portal = _editor.Level.Portals[portalsToTravel[p]];

                if (roomsProcessed.Contains(portal.AdjoiningRoom))
                    continue;
                roomsProcessed.Add(portal.AdjoiningRoom);

                // Calculate facing X and Z
                int facingXmin = 0;
                int facingXmax = 0;
                int facingZmin = 0;
                int facingZmax = 0;

                Room room = _editor.Level.Rooms[r];
                Room otherRoom = _editor.Level.Rooms[portal.AdjoiningRoom];

                if (portal.Direction == PortalDirection.North)
                {
                    if (zMax < room.NumZSectors - 2)
                        continue;

                    facingXmin = portal.X + (int)(room.Position.X - otherRoom.Position.X) - 1;
                    facingXmin = portal.X + portal.NumXBlocks + (int)(room.Position.X - otherRoom.Position.X);
                    facingZmin = 0;
                    facingZmax = 1;
                }
                else if (portal.Direction == PortalDirection.South)
                {
                    if (zMin > 1)
                        continue;

                    facingXmin = portal.X + (int)(room.Position.X - otherRoom.Position.X) - 1;
                    facingXmax = portal.X + portal.NumXBlocks + (int)(room.Position.X - otherRoom.Position.X);
                    facingZmin = otherRoom.NumZSectors - 2;
                    facingZmax = otherRoom.NumZSectors - 1;
                }
                else if (portal.Direction == PortalDirection.East)
                {
                    if (xMax < room.NumXSectors - 2)
                        continue;

                    facingXmin = 0;
                    facingXmax = 1;
                    facingZmin = portal.Z + (int)(room.Position.Z - otherRoom.Position.Z) - 1;
                    facingZmax = portal.Z + portal.NumZBlocks + (int)(room.Position.Z - otherRoom.Position.Z);
                }
                else if (portal.Direction == PortalDirection.West)
                {
                    if (xMin > 1)
                        continue;

                    facingXmin = otherRoom.NumXSectors - 2;
                    facingXmax = otherRoom.NumXSectors - 1;
                    facingZmin = portal.Z + (int)(room.Position.Z - otherRoom.Position.Z) - 1;
                    facingZmax = portal.Z + portal.NumZBlocks + (int)(room.Position.Z - otherRoom.Position.Z);
                }
                else if (portal.Direction == PortalDirection.Floor)
                {
                    facingXmin = portal.X + (int)(room.Position.X - otherRoom.Position.X) - 1;
                    facingXmax = portal.X + portal.NumXBlocks + (int)(room.Position.X - otherRoom.Position.X);
                    facingZmin = portal.Z + (int)(room.Position.Z - otherRoom.Position.Z) - 1;
                    facingZmax = portal.Z + portal.NumZBlocks + (int)(room.Position.Z - otherRoom.Position.Z);
                }
                else if (portal.Direction == PortalDirection.Ceiling)
                {
                    facingXmin = portal.X + (int)(room.Position.X - otherRoom.Position.X) - 1;
                    facingXmax = portal.X + portal.NumXBlocks + (int)(room.Position.X - otherRoom.Position.X);
                    facingZmin = portal.Z + (int)(room.Position.Z - otherRoom.Position.Z) - 1;
                    facingZmax = portal.Z + portal.NumZBlocks + (int)(room.Position.Z - otherRoom.Position.Z);
                }

                _editor.Level.Rooms[portal.AdjoiningRoom].BuildGeometry(facingXmin,
                                                                        facingXmax,
                                                                        facingZmin,
                                                                        facingZmax);

                _editor.Level.Rooms[portal.AdjoiningRoom].CalculateLightingForThisRoom();
                _editor.Level.Rooms[portal.AdjoiningRoom].UpdateBuffers();
            }

            //   });

            watch.Stop();
            logger.Debug("Edit geometry time: " + watch.ElapsedMilliseconds + "  ms");
        }

        public static void FlipFloorSplit(int xMin, int xMax, int zMin, int zMax)
        {
            for (int x = xMin; x <= xMax; x++)
            {
                for (int z = zMin; z <= zMax; z++)
                {
                    byte split = _editor.Level.Rooms[_editor.RoomIndex].Blocks[x, z].SplitFoorType;

                    if (split == 0)
                        _editor.Level.Rooms[_editor.RoomIndex].Blocks[x, z].SplitFoorType = 1;
                    else
                        _editor.Level.Rooms[_editor.RoomIndex].Blocks[x, z].SplitFoorType = 0;
                }
            }

            SmartBuildGeometry(_editor.RoomIndex, xMin, xMax + 1, zMin, zMax + 1);
        }

        public static void FlipCeilingSplit(int xMin, int xMax, int zMin, int zMax)
        {
            for (int x = xMin; x <= xMax; x++)
            {
                for (int z = zMin; z <= zMax; z++)
                {
                    byte split = _editor.Level.Rooms[_editor.RoomIndex].Blocks[x, z].SplitCeilingType;

                    if (split == 0)
                        _editor.Level.Rooms[_editor.RoomIndex].Blocks[x, z].SplitCeilingType = 1;
                    else
                        _editor.Level.Rooms[_editor.RoomIndex].Blocks[x, z].SplitCeilingType = 0;
                }
            }

            SmartBuildGeometry(_editor.RoomIndex, xMin, xMax + 1, zMin, zMax + 1);
        }

        public static void AddTrigger(IWin32Window parent, int xMin, int xMax, int zMin, int zMax)
        {
            if (xMin == -1 || xMax == -1 || zMin == -1 || zMax == -1)
                return;

            TriggerInstance trigger = null;
            using (FormTrigger formTrigger = new FormTrigger())
            {
                formTrigger.TriggerID = -1;
                if (formTrigger.ShowDialog(parent) != DialogResult.OK)
                    return;
                trigger = formTrigger.Trigger;
                trigger.Room = _editor.RoomIndex;
                trigger.ID = _editor.Level.GetNewTriggerId();
                trigger.X = (byte)xMin;
                trigger.Z = (byte)zMin;
                trigger.NumXBlocks = (byte)(xMax - xMin + 1);
                trigger.NumZBlocks = (byte)(zMax - zMin + 1);
                _editor.Level.Triggers.Add(trigger.ID, trigger);

                for (int x = xMin; x <= xMax; x++)
                {
                    for (int z = zMin; z <= zMax; z++)
                    {
                        _editor.Level.Rooms[_editor.RoomIndex].Blocks[x, z].Triggers.Add(trigger.ID);
                    }
                }
            }
        }

        public static void MoveObject(ObjectType typ, int id, MoveObjectDirections direction, bool smoothMove)
        {
            if (typ != ObjectType.Light)
            {
                switch (direction)
                {
                    case MoveObjectDirections.Up:
                        _editor.Level.Objects[id].Position += new Vector3(0.0f, (smoothMove ? 32.0f : 128.0f), 0.0f);
                        break;

                    case MoveObjectDirections.Down:
                        _editor.Level.Objects[id].Position -= new Vector3(0.0f, (smoothMove ? 32.0f : 128.0f), 0.0f);
                        break;

                    case MoveObjectDirections.West:
                        _editor.Level.Objects[id].Position -= new Vector3((smoothMove ? 64.0f : 1024.0f), 0.0f, 0.0f);
                        break;

                    case MoveObjectDirections.East:
                        _editor.Level.Objects[id].Position += new Vector3((smoothMove ? 64.0f : 1024.0f), 0.0f, 0.0f);
                        break;

                    case MoveObjectDirections.North:
                        _editor.Level.Objects[id].Position += new Vector3(0.0f, 0.0f, (smoothMove ? 64.0f : 1024.0f));
                        break;

                    case MoveObjectDirections.South:
                        _editor.Level.Objects[id].Position -= new Vector3(0.0f, 0.0f, (smoothMove ? 64.0f : 1024.0f));
                        break;
                }
            }
            else
            {
                switch (direction)
                {
                    case MoveObjectDirections.Up:
                        _editor.Level.Rooms[_editor.RoomIndex].Lights[id].Position += new Vector3(0.0f, (smoothMove ? 32.0f : 128.0f), 0.0f);
                        break;

                    case MoveObjectDirections.Down:
                        _editor.Level.Rooms[_editor.RoomIndex].Lights[id].Position -= new Vector3(0.0f, (smoothMove ? 32.0f : 128.0f), 0.0f);
                        break;

                    case MoveObjectDirections.West:
                        _editor.Level.Rooms[_editor.RoomIndex].Lights[id].Position -= new Vector3((smoothMove ? 64.0f : 1024.0f), 0.0f, 0.0f);
                        break;

                    case MoveObjectDirections.East:
                        _editor.Level.Rooms[_editor.RoomIndex].Lights[id].Position += new Vector3((smoothMove ? 64.0f : 1024.0f), 0.0f, 0.0f);
                        break;

                    case MoveObjectDirections.North:
                        _editor.Level.Rooms[_editor.RoomIndex].Lights[id].Position += new Vector3(0.0f, 0.0f, (smoothMove ? 64.0f : 1024.0f));
                        break;

                    case MoveObjectDirections.South:
                        _editor.Level.Rooms[_editor.RoomIndex].Lights[id].Position -= new Vector3(0.0f, 0.0f, (smoothMove ? 64.0f : 1024.0f));
                        break;
                }
            }
        }

        public static void MoveObject(ObjectType typ, int id, GizmoAxis axis, float delta, bool smooth)
        {
            switch (axis)
            {
                case GizmoAxis.X:
                    if (smooth)
                        delta = (float)Math.Floor(delta / 64.0f) * 64.0f;
                    else
                        delta = (float)Math.Floor(delta / 512.0f) * 512.0f;
                    break;

                case GizmoAxis.Y:
                    if (smooth)
                        delta = (float)Math.Floor(delta / 64.0f) * 64.0f;
                    else
                        delta = (float)Math.Floor(delta / 128.0f) * 128.0f;
                    break;

                case GizmoAxis.Z:
                    if (smooth)
                        delta = (float)Math.Floor(delta / 64.0f) * 64.0f;
                    else
                        delta = (float)Math.Floor(delta / 512.0f) * 512.0f;
                    break;
            }
            if (typ != ObjectType.Light)
            {
                switch (axis)
                {
                    case GizmoAxis.X:
                        _editor.Level.Objects[id].Position += new Vector3(delta, 0.0f, 0.0f);
                        break;

                    case GizmoAxis.Y:
                        _editor.Level.Objects[id].Position += new Vector3(0.0f, delta, 0.0f);
                        break;                
                   
                    case GizmoAxis.Z:
                        _editor.Level.Objects[id].Position += new Vector3(0.0f, 0.0f, delta);
                        break;
                }
            }
            else
            {
                switch (axis)
                {
                    case GizmoAxis.X:
                        _editor.Level.Rooms[_editor.RoomIndex].Lights[id].Position += new Vector3(delta, 0.0f, 0.0f);
                        break;

                    case GizmoAxis.Y:
                        _editor.Level.Rooms[_editor.RoomIndex].Lights[id].Position += new Vector3(0.0f, delta, 0.0f);
                        break;

                    case GizmoAxis.Z:
                        _editor.Level.Rooms[_editor.RoomIndex].Lights[id].Position += new Vector3(0.0f, 0.0f, delta);
                        break;
                }
            }
        }


        public static void RotateObject(ObjectType typ, int id, int sign, bool smoothMove)
        {
            _editor.Level.Objects[_editor.PickingResult.Element].Rotation += (short)(sign * (smoothMove ? 5 : 45));

            if (_editor.Level.Objects[_editor.PickingResult.Element].Rotation == 360)
                _editor.Level.Objects[_editor.PickingResult.Element].Rotation = 0;

            if (_editor.Level.Objects[_editor.PickingResult.Element].Rotation < 0)
                _editor.Level.Objects[_editor.PickingResult.Element].Rotation += 360;
        }

        public static void DeleteObject(ObjectType typ, int id)
        {
            _editor.Level.Objects.Remove(id);

            switch (typ)
            {
                case ObjectType.Moveable:
                    _editor.Level.Rooms[_editor.RoomIndex].Moveables.Remove(id);
                    break;

                case ObjectType.StaticMesh:
                    _editor.Level.Rooms[_editor.RoomIndex].StaticMeshes.Remove(id);
                    break;

                case ObjectType.SoundSource:
                    _editor.Level.Rooms[_editor.RoomIndex].SoundSources.Remove(id);
                    break;

                case ObjectType.Sink:
                    _editor.Level.Rooms[_editor.RoomIndex].Sinks.Remove(id);
                    break;

                case ObjectType.Camera:
                    _editor.Level.Rooms[_editor.RoomIndex].Cameras.Remove(id);
                    break;

                case ObjectType.FlybyCamera:
                    _editor.Level.Rooms[_editor.RoomIndex].FlyByCameras.Remove(id);
                    break;
            }

            _editor.Level.DeleteObject(id);
        }        

        public static void MoveLight(int id, MoveObjectDirections direction, bool smoothMove)
        {
            switch (direction)
            {
                case MoveObjectDirections.Up:
                    _editor.Level.Rooms[_editor.RoomIndex].Lights[id].Position += new Vector3(0.0f, (smoothMove ? 32.0f : 128.0f), 0.0f);
                    break;

                case MoveObjectDirections.Down:
                    _editor.Level.Rooms[_editor.RoomIndex].Lights[id].Position -= new Vector3(0.0f, (smoothMove ? 32.0f : 128.0f), 0.0f);
                    break;

                case MoveObjectDirections.West:
                    if (_editor.Level.Rooms[_editor.RoomIndex].Lights[id].Position.X > 1024.0f)
                        _editor.Level.Rooms[_editor.RoomIndex].Lights[id].Position -= new Vector3((smoothMove ? 64.0f : 1024.0f), 0.0f, 0.0f);
                    break;

                case MoveObjectDirections.East:
                    if (_editor.Level.Rooms[_editor.RoomIndex].Lights[id].Position.X < (_editor.Level.Rooms[_editor.RoomIndex].NumXSectors - 1) * 1024.0f)
                        _editor.Level.Rooms[_editor.RoomIndex].Lights[id].Position += new Vector3((smoothMove ? 64.0f : 1024.0f), 0.0f, 0.0f);
                    break;

                case MoveObjectDirections.North:
                    if (_editor.Level.Rooms[_editor.RoomIndex].Lights[id].Position.Z < (_editor.Level.Rooms[_editor.RoomIndex].NumZSectors - 1) * 1024.0f)
                        _editor.Level.Rooms[_editor.RoomIndex].Lights[id].Position += new Vector3(0.0f, 0.0f, (smoothMove ? 64.0f : 1024.0f));
                    break;

                case MoveObjectDirections.South:
                    if (_editor.Level.Rooms[_editor.RoomIndex].Lights[id].Position.Z > 1024.0f)
                        _editor.Level.Rooms[_editor.RoomIndex].Lights[id].Position -= new Vector3(0.0f, 0.0f, (smoothMove ? 64.0f : 1024.0f));
                    break;
            }

            _editor.Level.Rooms[_editor.RoomIndex].BuildGeometry();
            _editor.Level.Rooms[_editor.RoomIndex].CalculateLightingForThisRoom();
            _editor.Level.Rooms[_editor.RoomIndex].UpdateBuffers();
        }

        public static void DeleteLight(int id)
        {
            _editor.Level.Rooms[_editor.RoomIndex].Lights.RemoveAt(id);

            _editor.Level.Rooms[_editor.RoomIndex].BuildGeometry();
            _editor.Level.Rooms[_editor.RoomIndex].CalculateLightingForThisRoom();
            _editor.Level.Rooms[_editor.RoomIndex].UpdateBuffers();
        }

        public static void MoveLightCone(int id, int x, int y)
        {
            Light light = _editor.Level.Rooms[_editor.RoomIndex].Lights[id];

            light.DirectionX += x;
            light.DirectionY += y;

            if (light.DirectionX >= 360)
                light.DirectionX -= 360;
            if (light.DirectionX < 0)
                light.DirectionX += 360;
            if (light.DirectionY < 0)
                light.DirectionY += 360;
            if (light.DirectionY >= 360)
                light.DirectionY -= 360;

            _editor.Level.Rooms[_editor.RoomIndex].Lights[id] = light;

            _editor.Level.Rooms[_editor.RoomIndex].BuildGeometry();
            _editor.Level.Rooms[_editor.RoomIndex].CalculateLightingForThisRoom();
            _editor.Level.Rooms[_editor.RoomIndex].UpdateBuffers();
        }

        public static void MoveFlybyCone(int id, short x, short y)
        {
            FlybyCameraInstance flyby = (FlybyCameraInstance)_editor.Level.Objects[id];

            flyby.DirectionX += x;
            flyby.DirectionX += y;

            if (flyby.DirectionX >= 360)
                flyby.DirectionX -= 360;
            if (flyby.DirectionX < 0)
                flyby.DirectionX += 360;
            if (flyby.DirectionY < 0)
                flyby.DirectionY += 360;
            if (flyby.DirectionY >= 360)
                flyby.DirectionY -= 360;

            _editor.Level.Objects[id] = flyby;
        }

        public static void PlaceTexture(int x, int z, BlockFaces faceType)
        {
            BlockFace face = _editor.Level.Rooms[_editor.RoomIndex].Blocks[x, z].Faces[(int)faceType];

            ApplyTexture(x, z, faceType);

            face.Flipped = false;

            _editor.Level.Rooms[_editor.RoomIndex].BuildGeometry(x, x, z, z);
            _editor.Level.Rooms[_editor.RoomIndex].CalculateLightingForThisRoom();
            _editor.Level.Rooms[_editor.RoomIndex].UpdateBuffers();
        }

        public static void PlaceNoCollision(int x, int z, BlockFaces faceType)
        {
            if (faceType == BlockFaces.Floor || faceType == BlockFaces.FloorTriangle2)
                _editor.Level.Rooms[_editor.RoomIndex].Blocks[x, z].NoCollisionFloor = !_editor.Level.Rooms[_editor.RoomIndex].Blocks[x, z].NoCollisionFloor;

            if (faceType == BlockFaces.Ceiling || faceType == BlockFaces.CeilingTriangle2)
                _editor.Level.Rooms[_editor.RoomIndex].Blocks[x, z].NoCollisionCeiling = !_editor.Level.Rooms[_editor.RoomIndex].Blocks[x, z].NoCollisionCeiling;

            _editor.Level.Rooms[_editor.RoomIndex].BuildGeometry();
            _editor.Level.Rooms[_editor.RoomIndex].CalculateLightingForThisRoom();
            _editor.Level.Rooms[_editor.RoomIndex].UpdateBuffers();
        }

        public static void ApplyTexture(int x, int z, BlockFaces faceType)
        {
            if (_editor == null || (_editor.SelectedTexture == -1 && !_editor.InvisiblePolygon))
                return;

            BlockFace face = (BlockFace)_editor.Level.Rooms[_editor.RoomIndex].Blocks[x, z].Faces[(int)faceType];

            if (_editor.InvisiblePolygon)
            {
                _editor.Level.Rooms[_editor.RoomIndex].Blocks[x, z].Faces[(int)faceType].Invisible = true;
                _editor.Level.Rooms[_editor.RoomIndex].Blocks[x, z].Faces[(int)faceType].Transparent = false;
                _editor.Level.Rooms[_editor.RoomIndex].Blocks[x, z].Faces[(int)faceType].DoubleSided = false;

                int tid = _editor.Level.Rooms[_editor.RoomIndex].Blocks[x, z].Faces[(int)faceType].Texture;

                _editor.Level.Rooms[_editor.RoomIndex].Blocks[x, z].Faces[(int)faceType].Texture = -1;
            }
            else
            {
                // if face was invisible, then reset flag
                if (face.Invisible)
                    _editor.Level.Rooms[_editor.RoomIndex].Blocks[x, z].Faces[(int)faceType].Invisible = false;

                // set trasparency of this face
                if (_editor.Transparent)
                    _editor.Level.Rooms[_editor.RoomIndex].Blocks[x, z].Faces[(int)faceType].Transparent = true;
                else
                    _editor.Level.Rooms[_editor.RoomIndex].Blocks[x, z].Faces[(int)faceType].Transparent = false;

                // set double sided flag of this face
                if (_editor.DoubleSided)
                    _editor.Level.Rooms[_editor.RoomIndex].Blocks[x, z].Faces[(int)faceType].DoubleSided = true;
                else
                    _editor.Level.Rooms[_editor.RoomIndex].Blocks[x, z].Faces[(int)faceType].DoubleSided = false;

                Vector2[] UV = new Vector2[4];
                
                LevelTexture texture = _editor.Level.TextureSamples[_editor.SelectedTexture];

                int yBlock = (int)(texture.Page / 8);
                int xBlock = (int)(texture.Page % 8);

                UV[0] = new Vector2((xBlock * 256 + texture.X) / 2048.0f, (yBlock * 256 + texture.Y) / 2048.0f);
                UV[1] = new Vector2((xBlock * 256 + texture.X + texture.Width) / 2048.0f, (yBlock * 256 + texture.Y) / 2048.0f);
                ;
                UV[2] = new Vector2((xBlock * 256 + texture.X + texture.Width) / 2048.0f, (yBlock * 256 + texture.Y + texture.Height) / 2048.0f);
                UV[3] = new Vector2((xBlock * 256 + texture.X) / 2048.0f, (yBlock * 256 + texture.Y + texture.Height) / 2048.0f);

                _editor.Level.Rooms[_editor.RoomIndex].Blocks[x, z].Faces[(int)faceType].RectangleUV[0] = UV[0];
                _editor.Level.Rooms[_editor.RoomIndex].Blocks[x, z].Faces[(int)faceType].RectangleUV[1] = UV[1];
                _editor.Level.Rooms[_editor.RoomIndex].Blocks[x, z].Faces[(int)faceType].RectangleUV[2] = UV[2];
                _editor.Level.Rooms[_editor.RoomIndex].Blocks[x, z].Faces[(int)faceType].RectangleUV[3] = UV[3];

                /*
                *  1----2    Split 0: 231 413  
                *  | \  |    Split 1: 124 342
                *  |  \ |
                *  4----3
                */

                if (face.Shape == BlockFaceShape.Triangle)
                {
                    if (_editor.TextureTriangle == TextureTileType.TriangleNW)
                    {
                        _editor.Level.Rooms[_editor.RoomIndex].Blocks[x, z].Faces[(int)faceType].TriangleUV[0] = UV[0];
                        _editor.Level.Rooms[_editor.RoomIndex].Blocks[x, z].Faces[(int)faceType].TriangleUV[1] = UV[1];
                        _editor.Level.Rooms[_editor.RoomIndex].Blocks[x, z].Faces[(int)faceType].TriangleUV[2] = UV[3];

                        if (faceType == BlockFaces.FloorTriangle2 || faceType == BlockFaces.CeilingTriangle2)
                        {
                            _editor.Level.Rooms[_editor.RoomIndex].Blocks[x, z].Faces[(int)faceType].TriangleUV2[0] = UV[0];
                            _editor.Level.Rooms[_editor.RoomIndex].Blocks[x, z].Faces[(int)faceType].TriangleUV2[1] = UV[1];
                            _editor.Level.Rooms[_editor.RoomIndex].Blocks[x, z].Faces[(int)faceType].TriangleUV2[2] = UV[3];
                        }
                    }

                    if (_editor.TextureTriangle == TextureTileType.TriangleNE)
                    {
                        _editor.Level.Rooms[_editor.RoomIndex].Blocks[x, z].Faces[(int)faceType].TriangleUV[0] = UV[1];
                        _editor.Level.Rooms[_editor.RoomIndex].Blocks[x, z].Faces[(int)faceType].TriangleUV[1] = UV[2];
                        _editor.Level.Rooms[_editor.RoomIndex].Blocks[x, z].Faces[(int)faceType].TriangleUV[2] = UV[0];

                        if (faceType == BlockFaces.FloorTriangle2 || faceType == BlockFaces.CeilingTriangle2)
                        {
                            _editor.Level.Rooms[_editor.RoomIndex].Blocks[x, z].Faces[(int)faceType].TriangleUV2[0] = UV[1];
                            _editor.Level.Rooms[_editor.RoomIndex].Blocks[x, z].Faces[(int)faceType].TriangleUV2[1] = UV[2];
                            _editor.Level.Rooms[_editor.RoomIndex].Blocks[x, z].Faces[(int)faceType].TriangleUV2[2] = UV[0];
                        }
                    }

                    if (_editor.TextureTriangle == TextureTileType.TriangleSE)
                    {
                        _editor.Level.Rooms[_editor.RoomIndex].Blocks[x, z].Faces[(int)faceType].TriangleUV[0] = UV[2];
                        _editor.Level.Rooms[_editor.RoomIndex].Blocks[x, z].Faces[(int)faceType].TriangleUV[1] = UV[3];
                        _editor.Level.Rooms[_editor.RoomIndex].Blocks[x, z].Faces[(int)faceType].TriangleUV[2] = UV[1];

                        if (faceType == BlockFaces.FloorTriangle2 || faceType == BlockFaces.CeilingTriangle2)
                        {
                            _editor.Level.Rooms[_editor.RoomIndex].Blocks[x, z].Faces[(int)faceType].TriangleUV2[0] = UV[2];
                            _editor.Level.Rooms[_editor.RoomIndex].Blocks[x, z].Faces[(int)faceType].TriangleUV2[1] = UV[3];
                            _editor.Level.Rooms[_editor.RoomIndex].Blocks[x, z].Faces[(int)faceType].TriangleUV2[2] = UV[1];
                        }
                    }

                    if (_editor.TextureTriangle == TextureTileType.TriangleSW)
                    {
                        _editor.Level.Rooms[_editor.RoomIndex].Blocks[x, z].Faces[(int)faceType].TriangleUV[0] = UV[3];
                        _editor.Level.Rooms[_editor.RoomIndex].Blocks[x, z].Faces[(int)faceType].TriangleUV[1] = UV[0];
                        _editor.Level.Rooms[_editor.RoomIndex].Blocks[x, z].Faces[(int)faceType].TriangleUV[2] = UV[2];

                        if (faceType == BlockFaces.FloorTriangle2 || faceType == BlockFaces.CeilingTriangle2)
                        {
                            _editor.Level.Rooms[_editor.RoomIndex].Blocks[x, z].Faces[(int)faceType].TriangleUV2[0] = UV[3];
                            _editor.Level.Rooms[_editor.RoomIndex].Blocks[x, z].Faces[(int)faceType].TriangleUV2[1] = UV[0];
                            _editor.Level.Rooms[_editor.RoomIndex].Blocks[x, z].Faces[(int)faceType].TriangleUV2[2] = UV[2];
                        }
                    }
                }

                if (face.Shape == BlockFaceShape.Triangle)
                {
                    _editor.Level.Rooms[_editor.RoomIndex].Blocks[x, z].Faces[(int)faceType].TextureTriangle = _editor.TextureTriangle;
                }

                _editor.Level.Rooms[_editor.RoomIndex].Blocks[x, z].Faces[(int)faceType].Texture = (short)_editor.SelectedTexture;
                _editor.Level.Rooms[_editor.RoomIndex].Blocks[x, z].Faces[(int)faceType].Rotation = 0;
            }
        }

        public static void PlaceObject(int x, int z, ObjectType typ, int id)
        {
            if (typ == ObjectType.Moveable)
            {
                MoveableInstance instance = new MoveableInstance(_editor.Level.GetNewObjectId(), _editor.RoomIndex);

                Block block = _editor.Level.Rooms[_editor.RoomIndex].Blocks[x, z];
                int y = (block.QAFaces[0] + block.QAFaces[1] + block.QAFaces[2] + block.QAFaces[3]) / 4;

                instance.Position = new Vector3(x * 1024 + 512, y * 256, z * 1024 + 512);
                instance.Model = _editor.Level.Wad.Moveables[(uint)id];
                instance.Invisible = false;

                _editor.Level.Objects.Add(instance.ID, instance);
                _editor.Level.Rooms[_editor.RoomIndex].Moveables.Add(instance.ID);
            }
            else if (typ == ObjectType.StaticMesh)
            {
                StaticMeshInstance instance = new StaticMeshInstance(_editor.Level.GetNewObjectId(), _editor.RoomIndex);

                Block block = _editor.Level.Rooms[_editor.RoomIndex].Blocks[x, z];
                int y = (block.QAFaces[0] + block.QAFaces[1] + block.QAFaces[2] + block.QAFaces[3]) / 4;

                instance.Position = new Vector3(x * 1024 + 512, y * 256, z * 1024 + 512);
                instance.Model = _editor.Level.Wad.StaticMeshes[(uint)id];
                instance.Invisible = false;
                instance.Color = System.Drawing.Color.FromArgb(255, 128, 128, 128);

                _editor.Level.Objects.Add(instance.ID, instance);
                _editor.Level.Rooms[_editor.RoomIndex].StaticMeshes.Add(instance.ID);
            }
            else if (typ == ObjectType.Camera)
            {
                CameraInstance instance = new CameraInstance(_editor.Level.GetNewObjectId(), _editor.RoomIndex);

                Block block = _editor.Level.Rooms[_editor.RoomIndex].Blocks[x, z];
                int y = (block.QAFaces[0] + block.QAFaces[1] + block.QAFaces[2] + block.QAFaces[3]) / 4;

                instance.Position = new Vector3(x * 1024 + 512, y * 256, z * 1024 + 512);
                instance.Invisible = false;

                _editor.Action = EditorAction.None;
                _editor.Level.Objects.Add(instance.ID, instance);
                _editor.Level.Rooms[_editor.RoomIndex].Cameras.Add(instance.ID);
            }
            else if (typ == ObjectType.FlybyCamera)
            {
                FlybyCameraInstance instance = new FlybyCameraInstance(_editor.Level.GetNewObjectId(), _editor.RoomIndex);

                Block block = _editor.Level.Rooms[_editor.RoomIndex].Blocks[x, z];
                int y = (block.QAFaces[0] + block.QAFaces[1] + block.QAFaces[2] + block.QAFaces[3]) / 4;

                instance.Position = new Vector3(x * 1024 + 512, y * 256, z * 1024 + 512);
                instance.Invisible = false;

                _editor.Action = EditorAction.None;
                _editor.Level.Objects.Add(instance.ID, instance);
                _editor.Level.Rooms[_editor.RoomIndex].FlyByCameras.Add(instance.ID);
            }
            else if (typ == ObjectType.SoundSource)
            {
                SoundInstance instance = new SoundInstance(_editor.Level.GetNewObjectId(), _editor.RoomIndex);

                Block block = _editor.Level.Rooms[_editor.RoomIndex].Blocks[x, z];
                int y = (block.QAFaces[0] + block.QAFaces[1] + block.QAFaces[2] + block.QAFaces[3]) / 4;

                instance.Position = new Vector3(x * 1024 + 512, y * 256, z * 1024 + 512);
                instance.Invisible = false;
                instance.SoundID = (short)id;

                _editor.Action = EditorAction.None;
                _editor.Level.Objects.Add(instance.ID, instance);
                _editor.Level.Rooms[_editor.RoomIndex].SoundSources.Add(instance.ID);
            }
            else if (typ == ObjectType.Sink)
            {
                SinkInstance instance = new SinkInstance(_editor.Level.GetNewObjectId(), _editor.RoomIndex);

                Block block = _editor.Level.Rooms[_editor.RoomIndex].Blocks[x, z];
                int y = (block.QAFaces[0] + block.QAFaces[1] + block.QAFaces[2] + block.QAFaces[3]) / 4;

                instance.Position = new Vector3(x * 1024 + 512, y * 256, z * 1024 + 512);
                instance.Invisible = false;

                _editor.Action = EditorAction.None;
                _editor.Level.Objects.Add(instance.ID, instance);
                _editor.Level.Rooms[_editor.RoomIndex].Sinks.Add(instance.ID);
            }
        }

        public static void PlaceLight(int x, int z, LightType typ)
        {
            Light instance = new Light();

            Block block = _editor.Level.Rooms[_editor.RoomIndex].Blocks[x, z];
            int y = (block.QAFaces[0] + block.QAFaces[1] + block.QAFaces[2] + block.QAFaces[3]) / 4;

            instance.Position = new Vector3(x * 1024 + 512, y * 256 + 128.0f, z * 1024 + 512);
            instance.Color = System.Drawing.Color.White;
            instance.Active = true;
            instance.Intensity = 0.5f;
            instance.In = 1.0f;
            instance.Out = 5.0f;
            instance.Type = typ;

            if (typ == LightType.Shadow)
            {
                instance.Intensity *= -1;
            }

            if (typ == LightType.Spot)
            {
                instance.Len = 2.0f;
                instance.Cutoff = 3.0f;
                instance.DirectionX = 0.0f;
                instance.DirectionY = 0.0f;
                instance.In = 20.0f;
                instance.Out = 25.0f;
            }

            if (typ == LightType.Sun)
            {
                instance.DirectionX = 0.0f;
                instance.DirectionY = 0.0f;
            }

            if (typ == LightType.Effect)
            {
                instance.In = 0.0f;
                instance.Out = 1024.0f;
            }

            _editor.Level.Rooms[_editor.RoomIndex].Lights.Add(instance);

            _editor.Level.Rooms[_editor.RoomIndex].BuildGeometry();
            _editor.Level.Rooms[_editor.RoomIndex].CalculateLightingForThisRoom();
            _editor.Level.Rooms[_editor.RoomIndex].UpdateBuffers();
        }

        public static void CropRoom(int xMin, int xMax, int zMin, int zMax)
        {
            Room room = _editor.Level.Rooms[_editor.RoomIndex];

            if (room.Flipped)
            {
                DarkUI.Forms.DarkMessageBox.ShowError("You can't crop a flipped room", "Error");
                return;
            }

            // First check if there are portals
            for (int x = 0; x < room.NumXSectors; x++)
            {
                for (int z = 0; z < room.NumZSectors; z++)
                {
                    if (room.Blocks[x, z].FloorPortal != -1 || room.Blocks[x, z].CeilingPortal != -1 ||
                        room.Blocks[x, z].WallPortal != -1)
                    {
                        DarkUI.Forms.DarkMessageBox.ShowError("You can't crop a room with portals. Please delete all portals before doing this.",
                                                                "Error");
                        return;
                    }
                }
            }

            // Warning
            if (DarkUI.Forms.DarkMessageBox.ShowWarning("Warning: if you crop this room, all objects outside the new area will be deleted and " +
                                                        "triggers pointing to them will be removed. Do you want to continue?",
                                                        "Crop room", DarkUI.Forms.DarkDialogButton.YesNo) != DialogResult.Yes)
            {
                return;
            }

            byte numXSectors = (byte)(xMax - xMin + 3);
            byte numZSectors = (byte)(zMax - zMin + 3);
            int newX = (int)(room.Position.X + xMin - 1);
            int newZ = (int)(room.Position.Z + zMin - 1);
            int worldX = newX * 1024;
            int worldZ = newZ * 1024;

            Room newRoom = new Geometry.Room(_editor.Level, newX, (int)room.Position.Y, newZ, numXSectors, numZSectors, room.Ceiling);

            // First collect all items to remove
            List<int> objectsToRemove = new List<int>();
            List<int> lightsToRemove = new List<int>();
            List<int> triggersToRemove = new List<int>();

            for (int i = 0; i < _editor.Level.Objects.Count; i++)
            {
                IObjectInstance obj = _editor.Level.Objects.ElementAt(i).Value;
                if (obj.Room == _editor.RoomIndex)
                {
                    if (obj.Position.X < (newX + 1) * 1024 || obj.Position.X > (newX + numXSectors - 1) * 1024 ||
                        obj.Position.Z < (newZ + 1) * 1024 || obj.Position.Z > (newZ + numZSectors - 1) * 1024)
                    {
                        // We must remove that object. First try to find a trigger.
                        for (int j = 0; j < _editor.Level.Triggers.Count; j++)
                        {
                            TriggerInstance trigger = _editor.Level.Triggers.ElementAt(j).Value;

                            if (trigger.TargetType == TriggerTargetType.Camera && obj.Type == ObjectInstanceType.Camera &&
                                trigger.Target == obj.ID)
                            {
                                triggersToRemove.Add(trigger.ID);
                            }

                            if (trigger.TargetType == TriggerTargetType.FlyByCamera && obj.Type == ObjectInstanceType.FlyByCamera &&
                                trigger.Target == ((FlybyCameraInstance)obj).Sequence)
                            {
                                triggersToRemove.Add(trigger.ID);
                            }

                            if (trigger.TargetType == TriggerTargetType.Sink && obj.Type == ObjectInstanceType.Sink &&
                                trigger.Target == obj.ID)
                            {
                                triggersToRemove.Add(trigger.ID);
                            }

                            if (trigger.TargetType == TriggerTargetType.Object && obj.Type == ObjectInstanceType.Moveable &&
                                trigger.Target == obj.ID)
                            {
                                triggersToRemove.Add(trigger.ID);
                            }
                        }

                        // Remove the object
                        objectsToRemove.Add(obj.ID);
                    }
                }
            }

            // Now search for lights
            for (int i = 0; i < room.Lights.Count; i++)
            {
                Light light = room.Lights[i];
                if (light.Position.X < (newX + 1) * 1024 || light.Position.X > (newX + numXSectors) * 1024 ||
                        light.Position.Z < (newZ + 1) * 1024 || light.Position.Z > (newZ + numZSectors) * 1024)
                {
                    lightsToRemove.Add(i);
                }
            }

            // Now crop the room
            for (int x = 1; x < numXSectors - 1; x++)
            {
                for (int z = 1; z < numZSectors - 1; z++)
                {
                    newRoom.Blocks[x, z] = room.Blocks[x + xMin - 1, z + zMin - 1].Clone();
                    newRoom.Blocks[x, z].Room = newRoom;

                    /* for (int f = 0; f < newRoom.Blocks[x, z].Faces.Length; f++)
                     {
                         if (newRoom.Blocks[x, z].Faces[f].Texture != -1)
                         {
                             _editor.Level.TextureSamples[newRoom.Blocks[x, z].Faces[f].Texture].UsageCount++;
                         }
                     }*/

                    for (int k = 0; k < room.Blocks[x + xMin - 1, z + zMin - 1].Triggers.Count; k++)
                    {
                        int triggerId = room.Blocks[x + xMin - 1, z + zMin - 1].Triggers[k];
                        if (!triggersToRemove.Contains(triggerId))
                            newRoom.Blocks[x, z].Triggers.Add(triggerId);
                    }

                    // TODO: remove
                    /*
                    if (newRoom.Blocks[x, z].Type == BlockType.Portal ||
                        newRoom.Blocks[x, z].Type == BlockType.FloorPortal ||
                        newRoom.Blocks[x, z].Type == BlockType.CeilingPortal)
                    {
                        newRoom.Blocks[x, z].Type = BlockType.Floor;
                    }*/
                }
            }

            // Add everything except items to delete
            for (int i = 0; i < room.Lights.Count; i++)
            {
                if (!lightsToRemove.Contains(i))
                {
                    Light light = room.Lights[i];
                    newRoom.Lights.Add(light);
                    newRoom.Lights[newRoom.Lights.Count - 1].Position = new Vector3(light.Position.X - worldX,
                                                                                    light.Position.Y,
                                                                                    light.Position.Z - worldZ);
                }
            }

            for (int i = 0; i < room.Moveables.Count; i++)
            {
                if (!objectsToRemove.Contains(room.Moveables[i]))
                {
                    newRoom.Moveables.Add(room.Moveables[i]);
                    _editor.Level.Objects[room.Moveables[i]].Move(-worldX, 0, -worldZ);
                }
            }

            for (int i = 0; i < room.StaticMeshes.Count; i++)
            {
                if (!objectsToRemove.Contains(room.StaticMeshes[i]))
                {
                    newRoom.StaticMeshes.Add(room.StaticMeshes[i]);
                    _editor.Level.Objects[room.StaticMeshes[i]].Move(-worldX, 0, -worldZ);
                }
            }

            for (int i = 0; i < room.SoundSources.Count; i++)
            {
                if (!objectsToRemove.Contains(room.SoundSources[i]))
                {
                    newRoom.SoundSources.Add(room.SoundSources[i]);
                    _editor.Level.Objects[room.SoundSources[i]].Move(-worldX, 0, -worldZ);
                }
            }

            for (int i = 0; i < room.Sinks.Count; i++)
            {
                if (!objectsToRemove.Contains(room.Sinks[i]))
                {
                    newRoom.Sinks.Add(room.Sinks[i]);
                    _editor.Level.Objects[room.Sinks[i]].Move(-worldX, 0, -worldZ);
                }
            }

            for (int i = 0; i < room.Cameras.Count; i++)
            {
                if (!objectsToRemove.Contains(room.Cameras[i]))
                {
                    newRoom.Cameras.Add(room.Cameras[i]);
                    _editor.Level.Objects[room.Cameras[i]].Move(-worldX, 0, -worldZ);
                }
            }

            for (int i = 0; i < room.FlyByCameras.Count; i++)
            {
                if (!objectsToRemove.Contains(room.FlyByCameras[i]))
                {
                    newRoom.FlyByCameras.Add(room.FlyByCameras[i]);
                    _editor.Level.Objects[room.FlyByCameras[i]].Move(-worldX, 0, -worldZ);
                }
            }

            // Remove objects, triggers, lights
            for (int i = 0; i < objectsToRemove.Count; i++)
            {
                _editor.Level.Objects.Remove(objectsToRemove[i]);
            }

            for (int i = 0; i < triggersToRemove.Count; i++)
            {
                _editor.Level.Triggers.Remove(triggersToRemove[i]);
            }

            // Update triggers position
            for (int i = 0; i < _editor.Level.Triggers.Count; i++)
            {
                TriggerInstance trigger = _editor.Level.Triggers.ElementAt(i).Value;
                if (trigger.Room == _editor.RoomIndex)
                {
                    trigger.X -= (byte)newX;
                    trigger.Z -= (byte)newZ;

                    _editor.Level.Triggers[trigger.ID] = trigger;
                }
            }

            // Build the geometry of the new room
            newRoom.BuildGeometry();
            newRoom.CalculateLightingForThisRoom();
            newRoom.UpdateBuffers();

            _editor.Level.Rooms[_editor.RoomIndex] = newRoom;

            GC.Collect();
        }

        public static void SetDiagonalFloorSplit(int room, int xMin, int xMax, int zMin, int zMax)
        {
            Room currentRoom = _editor.Level.Rooms[room];

            if (_editor.BlockSelectionStartX != -1)
            {
                for (int x = xMin; x <= xMax; x++)
                {
                    for (int z = zMin; z <= zMax; z++)
                    {
                        if (currentRoom.Blocks[x, z].FloorPortal != -1)
                        {
                            Room otherRoom = _editor.Level.Rooms[_editor.Level.Portals[currentRoom.Blocks[x, z].FloorPortal].AdjoiningRoom];

                            int lowX = x + (int)(currentRoom.Position.X - otherRoom.Position.X);
                            int lowZ = z + (int)(currentRoom.Position.Z - otherRoom.Position.Z);

                            _editor.Level.Rooms[_editor.Level.Portals[currentRoom.Blocks[x, z].FloorPortal].AdjoiningRoom].Blocks[lowX, lowZ].IsCeilingSolid = true;
                        }

                        if (currentRoom.Blocks[x, z].CeilingPortal != -1)
                        {
                            Room otherRoom = _editor.Level.Rooms[_editor.Level.Portals[currentRoom.Blocks[x, z].CeilingPortal].AdjoiningRoom];

                            int lowX = x + (int)(currentRoom.Position.X - otherRoom.Position.X);
                            int lowZ = z + (int)(currentRoom.Position.Z - otherRoom.Position.Z);

                            _editor.Level.Rooms[_editor.Level.Portals[currentRoom.Blocks[x, z].CeilingPortal].AdjoiningRoom].Blocks[lowX, lowZ].IsFloorSolid = true;
                        }

                        // Now try to guess the floor split
                        short maxHeight = -32767;
                        byte theCorner = 0;

                        if (currentRoom.Blocks[x, z].QAFaces[0] > maxHeight)
                        {
                            maxHeight = currentRoom.Blocks[x, z].QAFaces[0];
                            theCorner = 0;
                        }

                        if (currentRoom.Blocks[x, z].QAFaces[1] > maxHeight)
                        {
                            maxHeight = currentRoom.Blocks[x, z].QAFaces[1];
                            theCorner = 1;
                        }

                        if (currentRoom.Blocks[x, z].QAFaces[2] > maxHeight)
                        {
                            maxHeight = currentRoom.Blocks[x, z].QAFaces[2];
                            theCorner = 2;
                        }

                        if (currentRoom.Blocks[x, z].QAFaces[3] > maxHeight)
                        {
                            maxHeight = currentRoom.Blocks[x, z].QAFaces[3];
                            theCorner = 3;
                        }

                        if (theCorner == 0)
                        {
                            _editor.Level.Rooms[room].Blocks[x, z].QAFaces[1] = maxHeight;
                            _editor.Level.Rooms[room].Blocks[x, z].QAFaces[3] = maxHeight;
                            _editor.Level.Rooms[room].Blocks[x, z].FloorDiagonalSplit = DiagonalSplit.SE;
                        }

                        if (theCorner == 1)
                        {
                            _editor.Level.Rooms[room].Blocks[x, z].QAFaces[0] = maxHeight;
                            _editor.Level.Rooms[room].Blocks[x, z].QAFaces[2] = maxHeight;
                            _editor.Level.Rooms[room].Blocks[x, z].FloorDiagonalSplit = DiagonalSplit.SW;
                        }

                        if (theCorner == 2)
                        {
                            _editor.Level.Rooms[room].Blocks[x, z].QAFaces[1] = maxHeight;
                            _editor.Level.Rooms[room].Blocks[x, z].QAFaces[3] = maxHeight;
                            _editor.Level.Rooms[room].Blocks[x, z].FloorDiagonalSplit = DiagonalSplit.NW;
                        }

                        if (theCorner == 3)
                        {
                            _editor.Level.Rooms[room].Blocks[x, z].QAFaces[0] = maxHeight;
                            _editor.Level.Rooms[room].Blocks[x, z].QAFaces[2] = maxHeight;
                            _editor.Level.Rooms[room].Blocks[x, z].FloorDiagonalSplit = DiagonalSplit.NE;
                        }


                        _editor.Level.Rooms[room].Blocks[x, z].FloorDiagonalSplitType = DiagonalSplitType.Floor;
                    }
                }

                SmartBuildGeometry(room, xMin, xMax + 1, zMin, zMax + 1);
            }
        }

        public static void SetDiagonalCeilingSplit(int room, int xMin, int xMax, int zMin, int zMax)
        {
            Room currentRoom = _editor.Level.Rooms[room];

            if (_editor.BlockSelectionStartX != -1)
            {
                for (int x = xMin; x <= xMax; x++)
                {
                    for (int z = zMin; z <= zMax; z++)
                    {
                        if (currentRoom.Blocks[x, z].FloorPortal != -1)
                        {
                            Room otherRoom = _editor.Level.Rooms[_editor.Level.Portals[currentRoom.Blocks[x, z].FloorPortal].AdjoiningRoom];

                            int lowX = x + (int)(currentRoom.Position.X - otherRoom.Position.X);
                            int lowZ = z + (int)(currentRoom.Position.Z - otherRoom.Position.Z);

                            _editor.Level.Rooms[_editor.Level.Portals[currentRoom.Blocks[x, z].FloorPortal].AdjoiningRoom].Blocks[lowX, lowZ].IsCeilingSolid = true;
                        }

                        if (currentRoom.Blocks[x, z].CeilingPortal != -1)
                        {
                            Room otherRoom = _editor.Level.Rooms[_editor.Level.Portals[currentRoom.Blocks[x, z].CeilingPortal].AdjoiningRoom];

                            int lowX = x + (int)(currentRoom.Position.X - otherRoom.Position.X);
                            int lowZ = z + (int)(currentRoom.Position.Z - otherRoom.Position.Z);

                            _editor.Level.Rooms[_editor.Level.Portals[currentRoom.Blocks[x, z].CeilingPortal].AdjoiningRoom].Blocks[lowX, lowZ].IsFloorSolid = true;
                        }

                        // Now try to guess the floor split
                        short minHeight = 32767;
                        byte theCorner = 0;

                        if (currentRoom.Blocks[x, z].WSFaces[0] < minHeight)
                        {
                            minHeight = currentRoom.Blocks[x, z].WSFaces[0];
                            theCorner = 0;
                        }

                        if (currentRoom.Blocks[x, z].WSFaces[1] < minHeight)
                        {
                            minHeight = currentRoom.Blocks[x, z].WSFaces[1];
                            theCorner = 1;
                        }

                        if (currentRoom.Blocks[x, z].WSFaces[2] < minHeight)
                        {
                            minHeight = currentRoom.Blocks[x, z].WSFaces[2];
                            theCorner = 2;
                        }

                        if (currentRoom.Blocks[x, z].WSFaces[3] < minHeight)
                        {
                            minHeight = currentRoom.Blocks[x, z].WSFaces[3];
                            theCorner = 3;
                        }

                        if (theCorner == 0)
                        {
                            _editor.Level.Rooms[room].Blocks[x, z].WSFaces[1] = minHeight;
                            _editor.Level.Rooms[room].Blocks[x, z].WSFaces[3] = minHeight;
                            _editor.Level.Rooms[room].Blocks[x, z].CeilingDiagonalSplit = DiagonalSplit.SE;
                        }

                        if (theCorner == 1)
                        {
                            _editor.Level.Rooms[room].Blocks[x, z].WSFaces[0] = minHeight;
                            _editor.Level.Rooms[room].Blocks[x, z].WSFaces[2] = minHeight;
                            _editor.Level.Rooms[room].Blocks[x, z].CeilingDiagonalSplit = DiagonalSplit.SW;
                        }

                        if (theCorner == 2)
                        {
                            _editor.Level.Rooms[room].Blocks[x, z].WSFaces[1] = minHeight;
                            _editor.Level.Rooms[room].Blocks[x, z].WSFaces[3] = minHeight;
                            _editor.Level.Rooms[room].Blocks[x, z].CeilingDiagonalSplit = DiagonalSplit.NW;
                        }

                        if (theCorner == 3)
                        {
                            _editor.Level.Rooms[room].Blocks[x, z].WSFaces[0] = minHeight;
                            _editor.Level.Rooms[room].Blocks[x, z].WSFaces[2] = minHeight;
                            _editor.Level.Rooms[room].Blocks[x, z].CeilingDiagonalSplit = DiagonalSplit.NE;
                        }


                        _editor.Level.Rooms[room].Blocks[x, z].CeilingDiagonalSplitType = DiagonalSplitType.Floor;
                    }
                }

                SmartBuildGeometry(room, xMin, xMax + 1, zMin, zMax + 1);
            }
        }

        public static void SetDiagonalWallSplit(int room, int xMin, int xMax, int zMin, int zMax)
        {
            Room currentRoom = _editor.Level.Rooms[room];

            if (_editor.BlockSelectionStartX != -1)
            {
                for (int x = xMin; x <= xMax; x++)
                {
                    for (int z = zMin; z <= zMax; z++)
                    {
                        if (currentRoom.Blocks[x, z].FloorPortal != -1)
                        {
                            Room otherRoom = _editor.Level.Rooms[_editor.Level.Portals[currentRoom.Blocks[x, z].FloorPortal].AdjoiningRoom];

                            int lowX = x + (int)(currentRoom.Position.X - otherRoom.Position.X);
                            int lowZ = z + (int)(currentRoom.Position.Z - otherRoom.Position.Z);

                            _editor.Level.Rooms[_editor.Level.Portals[currentRoom.Blocks[x, z].FloorPortal].AdjoiningRoom].Blocks[lowX, lowZ].IsCeilingSolid = true;
                        }

                        if (currentRoom.Blocks[x, z].CeilingPortal != -1)
                        {
                            Room otherRoom = _editor.Level.Rooms[_editor.Level.Portals[currentRoom.Blocks[x, z].CeilingPortal].AdjoiningRoom];

                            int lowX = x + (int)(currentRoom.Position.X - otherRoom.Position.X);
                            int lowZ = z + (int)(currentRoom.Position.Z - otherRoom.Position.Z);

                            _editor.Level.Rooms[_editor.Level.Portals[currentRoom.Blocks[x, z].CeilingPortal].AdjoiningRoom].Blocks[lowX, lowZ].IsFloorSolid = true;
                        }

                        // Now try to guess the floor split
                        short maxHeight = -32767;
                        byte theCorner = 0;

                        if (currentRoom.Blocks[x, z].QAFaces[0] > maxHeight)
                        {
                            maxHeight = currentRoom.Blocks[x, z].QAFaces[0];
                            theCorner = 0;
                        }

                        if (currentRoom.Blocks[x, z].QAFaces[1] > maxHeight)
                        {
                            maxHeight = currentRoom.Blocks[x, z].QAFaces[1];
                            theCorner = 1;
                        }

                        if (currentRoom.Blocks[x, z].QAFaces[2] > maxHeight)
                        {
                            maxHeight = currentRoom.Blocks[x, z].QAFaces[2];
                            theCorner = 2;
                        }

                        if (currentRoom.Blocks[x, z].QAFaces[3] > maxHeight)
                        {
                            maxHeight = currentRoom.Blocks[x, z].QAFaces[3];
                            theCorner = 3;
                        }

                        if (theCorner == 0)
                        {
                            _editor.Level.Rooms[room].Blocks[x, z].QAFaces[1] = maxHeight;
                            _editor.Level.Rooms[room].Blocks[x, z].QAFaces[3] = maxHeight;
                            _editor.Level.Rooms[room].Blocks[x, z].FloorDiagonalSplit = DiagonalSplit.SE;
                            _editor.Level.Rooms[room].Blocks[x, z].CeilingDiagonalSplit = DiagonalSplit.SE;
                        }

                        if (theCorner == 1)
                        {
                            _editor.Level.Rooms[room].Blocks[x, z].QAFaces[0] = maxHeight;
                            _editor.Level.Rooms[room].Blocks[x, z].QAFaces[2] = maxHeight;
                            _editor.Level.Rooms[room].Blocks[x, z].FloorDiagonalSplit = DiagonalSplit.SW;
                            _editor.Level.Rooms[room].Blocks[x, z].CeilingDiagonalSplit = DiagonalSplit.SW;
                        }

                        if (theCorner == 2)
                        {
                            _editor.Level.Rooms[room].Blocks[x, z].QAFaces[1] = maxHeight;
                            _editor.Level.Rooms[room].Blocks[x, z].QAFaces[3] = maxHeight;
                            _editor.Level.Rooms[room].Blocks[x, z].FloorDiagonalSplit = DiagonalSplit.NW;
                            _editor.Level.Rooms[room].Blocks[x, z].CeilingDiagonalSplit = DiagonalSplit.NW;
                        }

                        if (theCorner == 3)
                        {
                            _editor.Level.Rooms[room].Blocks[x, z].QAFaces[0] = maxHeight;
                            _editor.Level.Rooms[room].Blocks[x, z].QAFaces[2] = maxHeight;
                            _editor.Level.Rooms[room].Blocks[x, z].FloorDiagonalSplit = DiagonalSplit.NE;
                            _editor.Level.Rooms[room].Blocks[x, z].CeilingDiagonalSplit = DiagonalSplit.NE;
                        }

                        _editor.Level.Rooms[room].Blocks[x, z].Type = BlockType.Wall;
                        _editor.Level.Rooms[room].Blocks[x, z].FloorDiagonalSplitType = DiagonalSplitType.Wall;
                        _editor.Level.Rooms[room].Blocks[x, z].CeilingDiagonalSplitType = DiagonalSplitType.None;
                    }
                }

                SmartBuildGeometry(room, xMin, xMax + 1, zMin, zMax + 1);
            }
        }


        public static void SetWall(int roomIndex, int xMin, int xMax, int zMin, int zMax)
        {
            Room currentRoom = _editor.Level.Rooms[roomIndex];

            if (_editor.BlockSelectionStartX != -1)
            {
                for (int x = xMin; x <= xMax; x++)
                {
                    for (int z = zMin; z <= zMax; z++)
                    {
                        _editor.Level.Rooms[roomIndex].Blocks[x, z].Type = BlockType.Wall;

                        if (currentRoom.Blocks[x, z].FloorPortal != -1)
                        {
                            Room otherRoom = _editor.Level.Rooms[_editor.Level.Portals[currentRoom.Blocks[x, z].FloorPortal].AdjoiningRoom];

                            int lowX = x + (int)(currentRoom.Position.X - otherRoom.Position.X);
                            int lowZ = z + (int)(currentRoom.Position.Z - otherRoom.Position.Z);

                            _editor.Level.Rooms[_editor.Level.Portals[currentRoom.Blocks[x, z].FloorPortal].AdjoiningRoom].Blocks[lowX, lowZ].IsCeilingSolid = true;
                        }

                        if (currentRoom.Blocks[x, z].CeilingPortal != -1)
                        {
                            Room otherRoom = _editor.Level.Rooms[_editor.Level.Portals[currentRoom.Blocks[x, z].CeilingPortal].AdjoiningRoom];

                            int lowX = x + (int)(currentRoom.Position.X - otherRoom.Position.X);
                            int lowZ = z + (int)(currentRoom.Position.Z - otherRoom.Position.Z);

                            _editor.Level.Rooms[_editor.Level.Portals[currentRoom.Blocks[x, z].CeilingPortal].AdjoiningRoom].Blocks[lowX, lowZ].IsFloorSolid = true;
                        }
                    }
                }

                SmartBuildGeometry(roomIndex, xMin, xMax + 1, zMin, zMax + 1);
            }
        }

        public static void SetFloor(int roomIndex, int xMin, int xMax, int zMin, int zMax)
        {
            Room currentRoom = _editor.Level.Rooms[roomIndex];

            if (_editor.BlockSelectionStartX != -1)
            {
                for (int x = xMin; x <= xMax; x++)
                {
                    for (int z = zMin; z <= zMax; z++)
                    {
                        if (currentRoom.Blocks[x, z].Type != BlockType.Floor || currentRoom.Blocks[x, z].FloorPortal != -1)
                        {
                            DarkUI.Forms.DarkMessageBox.ShowError("Can't reset some blocks to floor because some of them are a portal or a border wall",
                                                                  "Error", DarkUI.Forms.DarkDialogButton.Ok);
                            return;
                        }
                    }
                }

                for (int x = xMin; x <= xMax; x++)
                {
                    for (int z = zMin; z <= zMax; z++)
                    {
                        _editor.Level.Rooms[roomIndex].Blocks[x, z].SplitFloor = false;
                        _editor.Level.Rooms[roomIndex].Blocks[x, z].Type = BlockType.Floor;
                        _editor.Level.Rooms[roomIndex].Blocks[x, z].FloorDiagonalSplit = DiagonalSplit.None;
                    }
                }

                SmartBuildGeometry(roomIndex, xMin, xMax + 1, zMin, zMax + 1);
            }
        }

        public static void SetCeiling(int roomIndex, int xMin, int xMax, int zMin, int zMax)
        {
            Room currentRoom = _editor.Level.Rooms[roomIndex];

            if (_editor.BlockSelectionStartX != -1)
            {
                for (int x = xMin; x <= xMax; x++)
                {
                    for (int z = zMin; z <= zMax; z++)
                    {
                        if (currentRoom.Blocks[x, z].Type != BlockType.Floor || currentRoom.Blocks[x, z].CeilingPortal != -1)
                        {
                            DarkUI.Forms.DarkMessageBox.ShowError("Can't reset some blocks to ceiling because some of them are a portal or a border wall",
                                                                  "Error", DarkUI.Forms.DarkDialogButton.Ok);
                            return;
                        }
                    }
                }

                for (int x = xMin; x <= xMax; x++)
                {
                    for (int z = zMin; z <= zMax; z++)
                    {
                        _editor.Level.Rooms[roomIndex].Blocks[x, z].SplitCeiling = false;
                        _editor.Level.Rooms[roomIndex].Blocks[x, z].CeilingDiagonalSplit = DiagonalSplit.None;
                    }
                }

                SmartBuildGeometry(roomIndex, xMin, xMax + 1, zMin, zMax + 1);
            }
        }

        public static void ToggleBlockFlag(BlockFlags flag)
        {
            Room currentRoom = _editor.Level.Rooms[_editor.RoomIndex];

            if (_editor.BlockSelectionStartX != -1)
            {
                int xMin = Math.Min(_editor.BlockSelectionStartX, _editor.BlockSelectionEndX);
                int xMax = Math.Max(_editor.BlockSelectionStartX, _editor.BlockSelectionEndX);
                int zMin = Math.Min(_editor.BlockSelectionStartZ, _editor.BlockSelectionEndZ);
                int zMax = Math.Max(_editor.BlockSelectionStartZ, _editor.BlockSelectionEndZ);

                for (int x = xMin; x <= xMax; x++)
                {
                    for (int z = zMin; z <= zMax; z++)
                    {
                        _editor.Level.Rooms[_editor.RoomIndex].Blocks[x, z].Flags ^= flag;
                    }
                }
            }
        }

        public static void ToggleClimb(int direction)
        {
            Room currentRoom = _editor.Level.Rooms[_editor.RoomIndex];

            if (_editor.BlockSelectionStartX != -1)
            {
                int xMin = Math.Min(_editor.BlockSelectionStartX, _editor.BlockSelectionEndX);
                int xMax = Math.Max(_editor.BlockSelectionStartX, _editor.BlockSelectionEndX);
                int zMin = Math.Min(_editor.BlockSelectionStartZ, _editor.BlockSelectionEndZ);
                int zMax = Math.Max(_editor.BlockSelectionStartZ, _editor.BlockSelectionEndZ);

                for (int x = xMin; x <= xMax; x++)
                {
                    for (int z = zMin; z <= zMax; z++)
                    {
                        _editor.Level.Rooms[_editor.RoomIndex].Blocks[x, z].Climb[direction] = !_editor.Level.Rooms[_editor.RoomIndex].Blocks[x, z].Climb[direction];
                    }
                }
            }
        }

        public static bool AddPortal()
        {
            // Get selection's boundary
            int xMin = Math.Min(_editor.BlockSelectionStartX, _editor.BlockSelectionEndX);
            int xMax = Math.Max(_editor.BlockSelectionStartX, _editor.BlockSelectionEndX);
            int zMin = Math.Min(_editor.BlockSelectionStartZ, _editor.BlockSelectionEndZ);
            int zMax = Math.Max(_editor.BlockSelectionStartZ, _editor.BlockSelectionEndZ);

            Room room = _editor.Level.Rooms[_editor.RoomIndex];

            /*if (room.Flipped)
            {
                DarkUI.Forms.DarkMessageBox.ShowError("You can't add portals to a flipped room", "Error");
                return;
            }*/

            // The size of the portal
            int numXblocks = room.NumXSectors;
            int numZblocks = room.NumZSectors;

            // Probably to delete, because new controls. But for now I will leave it here.
            if (xMin < 0)
                xMin = 0;
            if (xMax > numXblocks - 1)
                xMax = numXblocks - 1;
            if (zMin < 0)
                zMin = 0;
            if (zMax > numZblocks - 1)
                zMax = numZblocks - 1;

            // West wall
            if (xMin == 0 && xMax == 0 && zMin != 0 && zMax != numZblocks - 1)
            {
                // Check for portal overlaps
                for (int z = zMin; z <= zMax; z++)
                {
                    if (room.Blocks[xMin, z].WallPortal != -1)
                    {
                        return false;
                    }
                }

                int xPortalWorld = Utils.GetWorldX(room, xMin);
                int zPortalWorld = Utils.GetWorldZ(room, zMin);

                // Search a compatible neighbour
                short found = -1;
                for (int i = 0; i < Level.MaxNumberOfRooms; i++)
                {
                    found = -1;
                    Room otherRoom = _editor.Level.Rooms[i];

                    if (i == _editor.RoomIndex || otherRoom == null)
                        continue;
                    if (otherRoom != null && otherRoom.Flipped != room.Flipped)
                        continue;

                    if (otherRoom.Position.X + otherRoom.NumXSectors - 1 == room.Position.X + 1 && room.Position.Z + zMin >= otherRoom.Position.Z + 1 &&
                        room.Position.Z + zMax <= otherRoom.Position.Z + otherRoom.NumZSectors - 1)
                    {
                        for (int z = zMin; z <= zMax; z++)
                        {
                            int facingZ = z + (int)(room.Position.Z - otherRoom.Position.Z);
                            if (facingZ < 1 || facingZ > otherRoom.NumZSectors - 1)
                            {
                                found = -1;
                                break;
                            }

                            if (otherRoom.Blocks[otherRoom.NumXSectors - 1, facingZ].WallPortal != -1)
                            {
                                found = -1;
                                break;
                            }
                            else
                            {
                                found = (short)i;
                            }
                        }
                    }

                    if (found != -1)
                        break;
                }

                if (found != -1)
                {
                    Portal currentRoomPortal = new Portal(0, _editor.RoomIndex);
                    Portal otherRoomPortal = new Portal(0, found);
                    Room otherRoom = _editor.Level.Rooms[found];

                    int xPortalOther = (int)(xPortalWorld - otherRoom.Position.X);
                    int zPortalOther = (int)(zPortalWorld - otherRoom.Position.Z);

                    currentRoomPortal.ID = _editor.Level.GetNewPortalId();
                    currentRoomPortal.NumXBlocks = 1;
                    currentRoomPortal.NumZBlocks = (byte)(zMax - zMin + 1);
                    currentRoomPortal.Direction = PortalDirection.West;
                    currentRoomPortal.X = 0;
                    currentRoomPortal.Z = (byte)zMin;
                    currentRoomPortal.AdjoiningRoom = found;
                    currentRoomPortal.MemberOfFlippedRoom = room.Flipped;

                    _editor.Level.Portals.Add(currentRoomPortal.ID, currentRoomPortal);
                    _editor.Level.Rooms[_editor.RoomIndex].Portals.Add(currentRoomPortal.ID);

                    otherRoomPortal.ID = _editor.Level.GetNewPortalId();
                    otherRoomPortal.NumXBlocks = 1;
                    otherRoomPortal.NumZBlocks = (byte)(zMax - zMin + 1);
                    otherRoomPortal.Direction = PortalDirection.East;
                    otherRoomPortal.X = (byte)(otherRoom.NumXSectors - 1);
                    otherRoomPortal.Z = (byte)zPortalOther;
                    otherRoomPortal.AdjoiningRoom = _editor.RoomIndex;
                    otherRoomPortal.MemberOfFlippedRoom = otherRoom.Flipped;

                    _editor.Level.Portals.Add(otherRoomPortal.ID, otherRoomPortal);
                    _editor.Level.Rooms[found].Portals.Add(otherRoomPortal.ID);

                    currentRoomPortal.OtherID = otherRoomPortal.ID;
                    otherRoomPortal.OtherID = currentRoomPortal.ID;

                    // Set the portal ID in sectors
                    for (int z = zMin; z <= zMax; z++)
                    {
                        _editor.Level.Rooms[_editor.RoomIndex].Blocks[0, z].WallPortal = currentRoomPortal.ID;
                        _editor.Level.Rooms[found].Blocks[otherRoom.NumXSectors - 1, z + (int)(room.Position.Z - otherRoom.Position.Z)].WallPortal = otherRoomPortal.ID;
                    }

                    // Build geometry for current room
                    _editor.Level.Rooms[_editor.RoomIndex].BuildGeometry();
                    _editor.Level.Rooms[_editor.RoomIndex].CalculateLightingForThisRoom();
                    _editor.Level.Rooms[_editor.RoomIndex].UpdateBuffers();

                    // Build geometry for adjoining room
                    _editor.Level.Rooms[found].BuildGeometry();
                    _editor.Level.Rooms[found].CalculateLightingForThisRoom();
                    _editor.Level.Rooms[found].UpdateBuffers();

                    return true;
                }
                else
                {
                    return false;
                }
            }

            // East wall
            if (xMin == numXblocks - 1 && xMax == xMin && zMin != 0 && zMax != numZblocks - 1)
            {
                // Check for portal overlaps
                for (int z = zMin; z <= zMax; z++)
                {
                    if (room.Blocks[xMin, z].WallPortal != -1)
                    {
                        return false;
                    }
                }

                int xPortalWorld = Utils.GetWorldX(room, xMin);
                int zPortalWorld = Utils.GetWorldZ(room, zMin);

                // Search a compatible neighbour
                short found = -1;
                for (int i = 0; i < Level.MaxNumberOfRooms; i++)
                {
                    found = -1;
                    Room otherRoom = _editor.Level.Rooms[i];

                    if (i == _editor.RoomIndex || otherRoom == null)
                        continue;
                    if (otherRoom != null && otherRoom.Flipped != room.Flipped)
                        continue;

                    if (room.Position.X + room.NumXSectors - 1 == otherRoom.Position.X + 1 && room.Position.Z + zMin >= otherRoom.Position.Z + 1 &&
                        room.Position.Z + zMax <= otherRoom.Position.Z + otherRoom.NumZSectors - 1)
                    {
                        for (int z = zMin; z <= zMax; z++)
                        {
                            int facingZ = z + (int)(room.Position.Z - otherRoom.Position.Z);
                            if (facingZ < 1 || facingZ > otherRoom.NumZSectors - 1)
                            {
                                found = -1;
                                break;
                            }

                            if (otherRoom.Blocks[0, facingZ].WallPortal != -1)
                            {
                                found = -1;
                                break;
                            }
                            else
                            {
                                found = (short)i;
                            }
                        }
                    }

                    if (found != -1)
                        break;
                }

                if (found != -1)
                {
                    Portal currentRoomPortal = new Portal(0, _editor.RoomIndex);
                    Portal otherRoomPortal = new Portal(0, found);
                    Room otherRoom = _editor.Level.Rooms[found];

                    int xPortalOther = (int)(xPortalWorld - otherRoom.Position.X);
                    int zPortalOther = (int)(zPortalWorld - otherRoom.Position.Z);

                    currentRoomPortal.ID = _editor.Level.GetNewPortalId();
                    currentRoomPortal.NumXBlocks = 1;
                    currentRoomPortal.NumZBlocks = (byte)(zMax - zMin + 1);
                    currentRoomPortal.Direction = PortalDirection.East;
                    currentRoomPortal.X = (byte)(numXblocks - 1);
                    currentRoomPortal.Z = (byte)zMin;
                    currentRoomPortal.AdjoiningRoom = found;
                    currentRoomPortal.MemberOfFlippedRoom = room.Flipped;

                    _editor.Level.Portals.Add(currentRoomPortal.ID, currentRoomPortal);
                    _editor.Level.Rooms[_editor.RoomIndex].Portals.Add(currentRoomPortal.ID);

                    otherRoomPortal.ID = _editor.Level.GetNewPortalId();
                    otherRoomPortal.NumXBlocks = 1;
                    otherRoomPortal.NumZBlocks = (byte)(zMax - zMin + 1);
                    otherRoomPortal.Direction = PortalDirection.West;
                    otherRoomPortal.X = 0;
                    otherRoomPortal.Z = (byte)zPortalOther;
                    otherRoomPortal.AdjoiningRoom = _editor.RoomIndex;
                    otherRoomPortal.MemberOfFlippedRoom = otherRoom.Flipped;

                    _editor.Level.Portals.Add(otherRoomPortal.ID, otherRoomPortal);
                    _editor.Level.Rooms[found].Portals.Add(otherRoomPortal.ID);

                    currentRoomPortal.OtherID = otherRoomPortal.ID;
                    otherRoomPortal.OtherID = currentRoomPortal.ID;

                    // Set the portal ID in sectors
                    for (int z = zMin; z <= zMax; z++)
                    {
                        _editor.Level.Rooms[_editor.RoomIndex].Blocks[numXblocks - 1, z].WallPortal = currentRoomPortal.ID;
                        _editor.Level.Rooms[found].Blocks[0, z + (int)(room.Position.Z - otherRoom.Position.Z)].WallPortal = otherRoomPortal.ID;
                    }

                    // Build geometry for current room
                    _editor.Level.Rooms[_editor.RoomIndex].BuildGeometry();
                    _editor.Level.Rooms[_editor.RoomIndex].CalculateLightingForThisRoom();
                    _editor.Level.Rooms[_editor.RoomIndex].UpdateBuffers();

                    // Build geometry for adjoining room
                    _editor.Level.Rooms[found].BuildGeometry();
                    _editor.Level.Rooms[found].CalculateLightingForThisRoom();
                    _editor.Level.Rooms[found].UpdateBuffers();

                    return true;
                }
                else
                {
                    return false;
                }
            }

            // North wall
            if (zMin == numZblocks - 1 && zMax == zMin && xMin != 0 && xMax != numXblocks - 1)
            {
                // Check for portal overlaps
                for (int x = xMin; x <= xMax; x++)
                {
                    if (room.Blocks[x, zMin].WallPortal != -1)
                    {
                        return false;
                    }
                }

                int xPortalWorld = Utils.GetWorldX(room, xMin);
                int zPortalWorld = Utils.GetWorldZ(room, zMin);

                // Search a compatible neighbour
                short found = -1;
                for (int i = 0; i < Level.MaxNumberOfRooms; i++)
                {
                    found = -1;
                    Room otherRoom = _editor.Level.Rooms[i];

                    if (i == _editor.RoomIndex || otherRoom == null)
                        continue;
                    if (otherRoom != null && otherRoom.Flipped != room.Flipped)
                        continue;

                    if (room.Position.Z + room.NumZSectors - 1 == otherRoom.Position.Z + 1 && room.Position.X + xMin >= otherRoom.Position.X + 1 &&
                        room.Position.X + xMax <= otherRoom.Position.X + otherRoom.NumXSectors - 1)
                    {
                        for (int x = xMin; x <= xMax; x++)
                        {
                            int facingX = x + (int)(room.Position.X - otherRoom.Position.X);
                            if (facingX < 1 || facingX > otherRoom.NumXSectors - 1)
                            {
                                found = -1;
                                break;
                            }

                            if (otherRoom.Blocks[facingX, 0].WallPortal != -1)
                            {
                                found = -1;
                                break;
                            }
                            else
                            {
                                found = (short)i;
                            }
                        }
                    }

                    if (found != -1)
                        break;
                }

                if (found != -1)
                {
                    Portal currentRoomPortal = new Portal(0, _editor.RoomIndex);
                    Portal otherRoomPortal = new Portal(0, found);
                    Room otherRoom = _editor.Level.Rooms[found];

                    byte xPortalOther = (byte)(xPortalWorld - otherRoom.Position.X);
                    byte zPortalOther = (byte)(zPortalWorld - otherRoom.Position.Z);

                    currentRoomPortal.ID = _editor.Level.GetNewPortalId();
                    currentRoomPortal.NumXBlocks = (byte)(xMax - xMin + 1);
                    currentRoomPortal.NumZBlocks = 1;
                    currentRoomPortal.Direction = PortalDirection.North;
                    currentRoomPortal.X = (byte)xMin;
                    currentRoomPortal.Z = (byte)(numZblocks - 1);
                    currentRoomPortal.AdjoiningRoom = found;
                    currentRoomPortal.MemberOfFlippedRoom = room.Flipped;

                    _editor.Level.Portals.Add(currentRoomPortal.ID, currentRoomPortal);
                    _editor.Level.Rooms[_editor.RoomIndex].Portals.Add(currentRoomPortal.ID);

                    otherRoomPortal.ID = _editor.Level.GetNewPortalId();
                    otherRoomPortal.NumXBlocks = (byte)(xMax - xMin + 1);
                    otherRoomPortal.NumZBlocks = 1;
                    otherRoomPortal.Direction = PortalDirection.South;
                    otherRoomPortal.X = xPortalOther;
                    otherRoomPortal.Z = 0;
                    otherRoomPortal.AdjoiningRoom = _editor.RoomIndex;
                    otherRoomPortal.MemberOfFlippedRoom = otherRoom.Flipped;

                    _editor.Level.Portals.Add(otherRoomPortal.ID, otherRoomPortal);
                    _editor.Level.Rooms[found].Portals.Add(otherRoomPortal.ID);

                    currentRoomPortal.OtherID = otherRoomPortal.ID;
                    otherRoomPortal.OtherID = currentRoomPortal.ID;

                    // Set portal ID in sectors
                    for (int x = xMin; x <= xMax; x++)
                    {
                        _editor.Level.Rooms[_editor.RoomIndex].Blocks[x, numZblocks - 1].WallPortal = currentRoomPortal.ID;
                        _editor.Level.Rooms[found].Blocks[x + xPortalOther - xMin, 0].WallPortal = otherRoomPortal.ID;
                    }

                    // Build geometry for current room
                    _editor.Level.Rooms[_editor.RoomIndex].BuildGeometry();
                    _editor.Level.Rooms[_editor.RoomIndex].CalculateLightingForThisRoom();
                    _editor.Level.Rooms[_editor.RoomIndex].UpdateBuffers();

                    // Build geometry for adjoining room
                    _editor.Level.Rooms[found].BuildGeometry();
                    _editor.Level.Rooms[found].CalculateLightingForThisRoom();
                    _editor.Level.Rooms[found].UpdateBuffers();

                    return true;
                }
                else
                {
                    return false;
                }
            }

            // South wall
            if (zMin == 0 && zMax == zMin && xMin != 0 && xMax != numXblocks - 1)
            {
                // Check for portal overlaps
                for (int x = xMin; x <= xMax; x++)
                {
                    if (room.Blocks[x, zMin].WallPortal != -1)
                    {
                        return false;
                    }
                }

                int xPortalWorld = Utils.GetWorldX(room, xMin);
                int zPortalWorld = Utils.GetWorldZ(room, zMin);

                // Search a compatible neighbour
                short found = -1;
                for (int i = 0; i < Level.MaxNumberOfRooms; i++)
                {
                    found = -1;
                    Room otherRoom = _editor.Level.Rooms[i];

                    if (i == _editor.RoomIndex || otherRoom == null)
                        continue;
                    if (otherRoom != null && otherRoom.Flipped != room.Flipped)
                        continue;

                    if (otherRoom.Position.Z + otherRoom.NumZSectors - 1 == room.Position.Z + 1 && room.Position.X + xMin >= otherRoom.Position.X + 1 &&
                        room.Position.X + xMax <= otherRoom.Position.X + otherRoom.NumXSectors - 1)
                    {
                        for (int x = xMin; x <= xMax; x++)
                        {
                            int facingX = x + (int)(room.Position.X - otherRoom.Position.X);
                            if (facingX < 1 || facingX > otherRoom.NumXSectors - 1)
                            {
                                found = -1;
                                break;
                            }

                            if (otherRoom.Blocks[facingX, 0].WallPortal != -1)
                            {
                                found = -1;
                                break;
                            }
                            else
                            {
                                found = (short)i;
                            }
                        }
                    }

                    if (found != -1)
                        break;
                }

                if (found != -1)
                {
                    Portal currentRoomPortal = new Portal(0, _editor.RoomIndex);
                    Portal otherRoomPortal = new Portal(0, found);
                    Room otherRoom = _editor.Level.Rooms[found];

                    int xPortalOther = (int)(xPortalWorld - otherRoom.Position.X);
                    int zPortalOther = (int)(zPortalWorld - otherRoom.Position.Z);

                    currentRoomPortal.ID = _editor.Level.GetNewPortalId();
                    currentRoomPortal.NumXBlocks = (byte)(xMax - xMin + 1);
                    currentRoomPortal.NumZBlocks = 1;
                    currentRoomPortal.Direction = PortalDirection.South;
                    currentRoomPortal.X = (byte)xMin;
                    currentRoomPortal.Z = 0;
                    currentRoomPortal.AdjoiningRoom = found;
                    currentRoomPortal.MemberOfFlippedRoom = room.Flipped;

                    _editor.Level.Portals.Add(currentRoomPortal.ID, currentRoomPortal);
                    _editor.Level.Rooms[_editor.RoomIndex].Portals.Add(currentRoomPortal.ID);

                    otherRoomPortal.ID = _editor.Level.GetNewPortalId();
                    otherRoomPortal.NumXBlocks = (byte)(xMax - xMin + 1);
                    otherRoomPortal.NumZBlocks = 1;
                    otherRoomPortal.Direction = PortalDirection.North;
                    otherRoomPortal.X = (byte)xPortalOther;
                    otherRoomPortal.Z = (byte)(otherRoom.NumZSectors - 1);
                    otherRoomPortal.AdjoiningRoom = _editor.RoomIndex;
                    otherRoomPortal.MemberOfFlippedRoom = otherRoom.Flipped;

                    _editor.Level.Portals.Add(otherRoomPortal.ID, otherRoomPortal);
                    _editor.Level.Rooms[found].Portals.Add(otherRoomPortal.ID);

                    currentRoomPortal.OtherID = otherRoomPortal.ID;
                    otherRoomPortal.OtherID = currentRoomPortal.ID;

                    // Set portal ID in sectors
                    for (int x = xMin; x <= xMax; x++)
                    {
                        _editor.Level.Rooms[_editor.RoomIndex].Blocks[x, 0].WallPortal = currentRoomPortal.ID;
                        _editor.Level.Rooms[found].Blocks[x + xPortalOther - xMin, otherRoom.NumZSectors - 1].WallPortal = otherRoomPortal.ID;
                    }

                    // Build geometry for current room
                    _editor.Level.Rooms[_editor.RoomIndex].BuildGeometry();
                    _editor.Level.Rooms[_editor.RoomIndex].CalculateLightingForThisRoom();
                    _editor.Level.Rooms[_editor.RoomIndex].UpdateBuffers();

                    // Build geometry for adjoining room
                    _editor.Level.Rooms[found].BuildGeometry();
                    _editor.Level.Rooms[found].CalculateLightingForThisRoom();
                    _editor.Level.Rooms[found].UpdateBuffers();

                    return true;
                }
                else
                {
                    return false;
                }
            }

            // Floor - ceiling portal
            if (xMin > 0 && xMax < numXblocks - 1 && zMin > 0 && zMax < numZblocks - 1)
            {
                int lowest = room.GetLowestCorner();
                ;

                // Check for floor heights in selected area
                for (int x = xMin; x <= xMax; x++)
                {
                    for (int z = zMin; z <= zMax; z++)
                    {
                        int h1 = room.Blocks[x, z].QAFaces[0];
                        int h2 = room.Blocks[x, z].QAFaces[1];
                        int h3 = room.Blocks[x, z].QAFaces[2];
                        int h4 = room.Blocks[x, z].QAFaces[3];

                        // Check if the sector already has a portal
                        if (room.Blocks[x, z].Type != BlockType.Floor || room.Blocks[x, z].FloorPortal != -1)
                        {
                            return false;
                        }

                        // The lowest corner of the floor must be the same of the lowest corner of the room
                        int min = Math.Min(Math.Min(Math.Min(h1, h2), h3), h4);
                        if (min < lowest)
                        {
                            return false;
                        }
                    }
                }

                // Search a compatible neighbour
                short found = -1;

                for (int i = 0; i < Level.MaxNumberOfRooms; i++)
                {
                    found = -1;
                    Room otherRoom = _editor.Level.Rooms[i];

                    if (i == _editor.RoomIndex || otherRoom == null)
                        continue;
                    if (otherRoom != null && otherRoom.Flipped != room.Flipped)
                        continue;

                    int distance = (int)room.Position.Y + room.GetLowestCorner() - ((int)otherRoom.Position.Y + otherRoom.GetHighestCorner());
                    if (distance < 0 || distance > 2)
                        continue;

                    int lowXmin = xMin + (int)(room.Position.X - otherRoom.Position.X);
                    int lowXmax = xMax + (int)(room.Position.X - otherRoom.Position.X);
                    int lowZmin = zMin + (int)(room.Position.Z - otherRoom.Position.Z);
                    int lowZmax = zMax + (int)(room.Position.Z - otherRoom.Position.Z);

                    // If one of the coordinates of the possible neighbour are out of range, then ignore this room
                    if (lowXmin < 1 || lowXmin > otherRoom.NumXSectors - 2 || lowXmax < 1 || lowXmax > otherRoom.NumXSectors - 2 ||
                        lowZmin < 1 || lowZmin > otherRoom.NumZSectors - 2 || lowZmax < 1 || lowZmax > otherRoom.NumZSectors - 2)
                        continue;

                    bool validRoom = true;
                    int highest = otherRoom.GetHighestCorner();

                    for (int x = lowXmin; x <= lowXmax; x++)
                    {
                        for (int z = lowZmin; z <= lowZmax; z++)
                        {
                            // Now I do the same checks already done before, but this time for ceiling
                            int h1 = otherRoom.Ceiling + otherRoom.Blocks[x, z].WSFaces[0];
                            int h2 = otherRoom.Ceiling + otherRoom.Blocks[x, z].WSFaces[1];
                            int h3 = otherRoom.Ceiling + otherRoom.Blocks[x, z].WSFaces[2];
                            int h4 = otherRoom.Ceiling + otherRoom.Blocks[x, z].WSFaces[3];

                            // Check if the sector already has a ceiling portal
                            if ((otherRoom.Blocks[x, z].Type != BlockType.Floor && otherRoom.Blocks[x, z].Type != BlockType.Wall) ||
                                otherRoom.Blocks[x, z].CeilingPortal != -1)
                            {
                                validRoom = false;
                                break;
                            }

                            // The highest corner of the sector must be the same of the highest corner of the room
                            int max = Math.Max(Math.Max(Math.Max(h1, h2), h3), h4);
                            if (max > highest)
                            {
                                validRoom = false;
                                break;
                            }
                        }

                        if (!validRoom)
                            break;
                    }

                    if (!validRoom)
                        continue;
                    else
                    {
                        found = (short)i;
                        break;
                    }
                }

                // We have found a compatible neighbour
                if (found > -1)
                {
                    Room otherRoom = _editor.Level.Rooms[found];
                    int highest = _editor.Level.Rooms[found].GetHighestCorner();

                    for (int x = xMin; x <= xMax; x++)
                    {
                        for (int z = zMin; z <= zMax; z++)
                        {
                            int lowX = x + (int)(room.Position.X - otherRoom.Position.X);
                            int lowZ = z + (int)(room.Position.Z - otherRoom.Position.Z);

                            int h1 = room.Blocks[x, z].QAFaces[0];
                            int h2 = room.Blocks[x, z].QAFaces[1];
                            int h3 = room.Blocks[x, z].QAFaces[2];
                            int h4 = room.Blocks[x, z].QAFaces[3];

                            int lh1 = otherRoom.Ceiling + otherRoom.Blocks[lowX, lowZ].WSFaces[0];
                            int lh2 = otherRoom.Ceiling + otherRoom.Blocks[lowX, lowZ].WSFaces[1];
                            int lh3 = otherRoom.Ceiling + otherRoom.Blocks[lowX, lowZ].WSFaces[2];
                            int lh4 = otherRoom.Ceiling + otherRoom.Blocks[lowX, lowZ].WSFaces[3];

                            bool defined = false;

                            bool isCurrentWall = (_editor.Level.Rooms[_editor.RoomIndex].Blocks[x, z].Type == BlockType.Wall);
                            bool isOtherWall = (_editor.Level.Rooms[found].Blocks[lowX, lowZ].Type == BlockType.Wall);

                            bool isCurrentDiagonal = (_editor.Level.Rooms[_editor.RoomIndex].Blocks[x, z].FloorDiagonalSplit != DiagonalSplit.None);
                            bool isOtherDiagonal = ((isOtherWall && _editor.Level.Rooms[found].Blocks[lowX, lowZ].FloorDiagonalSplit != DiagonalSplit.None) ||
                                                    _editor.Level.Rooms[found].Blocks[lowX, lowZ].CeilingDiagonalSplit != DiagonalSplit.None);

                            // In some cases the surface of the floor must be solid
                            if (Room.IsQuad(x, z, h1, h2, h3, h4, true) && h1 == lowest)
                            {
                                if (isCurrentWall || isOtherWall || isCurrentDiagonal || isOtherDiagonal)
                                {
                                    _editor.Level.Rooms[_editor.RoomIndex].Blocks[x, z].IsFloorSolid = true;
                                    defined = false;
                                }
                                else
                                {
                                    _editor.Level.Rooms[_editor.RoomIndex].Blocks[x, z].IsFloorSolid = false;
                                    defined = true;
                                }
                            }
                            else
                            {
                                _editor.Level.Rooms[_editor.RoomIndex].Blocks[x, z].IsFloorSolid = true;
                                defined = false;
                            }

                            // In some cases the surface of the ceiling must be solid
                            if (Room.IsQuad(x, z, lh1, lh2, lh3, lh4, true) && lh1 == highest && defined)
                            {
                                if (isCurrentWall || isOtherWall || isCurrentDiagonal || isOtherDiagonal)
                                {
                                    _editor.Level.Rooms[found].Blocks[lowX, lowZ].IsCeilingSolid = true;
                                }
                                else
                                {
                                    _editor.Level.Rooms[found].Blocks[lowX, lowZ].IsCeilingSolid = false;
                                }
                            }
                            else
                            {
                                _editor.Level.Rooms[found].Blocks[lowX, lowZ].IsCeilingSolid = true;
                            }
                        }
                    }

                    Portal currentRoomPortal = new Portal(0, _editor.RoomIndex);
                    Portal otherRoomPortal = new Portal(0, found);

                    currentRoomPortal.ID = _editor.Level.GetNewPortalId();
                    currentRoomPortal.NumXBlocks = (byte)(xMax - xMin + 1);
                    currentRoomPortal.NumZBlocks = (byte)(zMax - zMin + 1);
                    currentRoomPortal.Direction = PortalDirection.Floor;
                    currentRoomPortal.X = (byte)xMin;
                    currentRoomPortal.Z = (byte)zMin;
                    currentRoomPortal.AdjoiningRoom = found;
                    currentRoomPortal.MemberOfFlippedRoom = room.Flipped;

                    _editor.Level.Portals.Add(currentRoomPortal.ID, currentRoomPortal);
                    _editor.Level.Rooms[_editor.RoomIndex].Portals.Add(currentRoomPortal.ID);

                    int lowXmin = xMin + (int)(room.Position.X - otherRoom.Position.X);
                    int lowXmax = xMax + (int)(room.Position.X - otherRoom.Position.X);
                    int lowZmin = zMin + (int)(room.Position.Z - otherRoom.Position.Z);
                    int lowZmax = zMax + (int)(room.Position.Z - otherRoom.Position.Z);

                    otherRoomPortal.ID = _editor.Level.GetNewPortalId();
                    otherRoomPortal.NumXBlocks = (byte)(lowXmax - lowXmin + 1);
                    otherRoomPortal.NumZBlocks = (byte)(lowZmax - lowZmin + 1);
                    otherRoomPortal.Direction = PortalDirection.Ceiling;
                    otherRoomPortal.X = (byte)lowXmin;
                    otherRoomPortal.Z = (byte)lowZmin;
                    otherRoomPortal.AdjoiningRoom = _editor.RoomIndex;
                    otherRoomPortal.MemberOfFlippedRoom = otherRoom.Flipped;

                    _editor.Level.Portals.Add(otherRoomPortal.ID, otherRoomPortal);
                    _editor.Level.Rooms[found].Portals.Add(otherRoomPortal.ID);

                    currentRoomPortal.OtherID = otherRoomPortal.ID;
                    otherRoomPortal.OtherID = currentRoomPortal.ID;

                    // Set floor portal ID
                    for (int x = xMin; x <= xMax; x++)
                    {
                        for (int z = zMin; z <= zMax; z++)
                        {
                            _editor.Level.Rooms[_editor.RoomIndex].Blocks[x, z].FloorPortal = currentRoomPortal.ID;
                        }
                    }

                    // Set ceiling portal ID
                    for (int x = otherRoomPortal.X; x <= otherRoomPortal.X + otherRoomPortal.NumXBlocks - 1; x++)
                    {
                        for (int z = otherRoomPortal.Z; z <= otherRoomPortal.Z + otherRoomPortal.NumZBlocks - 1; z++)
                        {
                            _editor.Level.Rooms[found].Blocks[x, z].CeilingPortal = otherRoomPortal.ID;
                        }
                    }

                    // Build geometry for current room
                    _editor.Level.Rooms[_editor.RoomIndex].BuildGeometry();
                    _editor.Level.Rooms[_editor.RoomIndex].CalculateLightingForThisRoom();
                    _editor.Level.Rooms[_editor.RoomIndex].UpdateBuffers();

                    // Build geometry for adjoining room
                    _editor.Level.Rooms[found].BuildGeometry();
                    _editor.Level.Rooms[found].CalculateLightingForThisRoom();
                    _editor.Level.Rooms[found].UpdateBuffers();

                    return true;
                }
            }

            return false;
        }

        public static void SpecialRaiseFloorOrCeiling(int face, short increment, 
                                                      int xMinSpecial, int xMaxSpecial, int zMinSpecial, int zMaxSpecial,
                                                      int xMin, int xMax, int zMin, int zMax)
        {
            if (face == 0)
            {
                _editor.Level.Rooms[_editor.RoomIndex].Blocks[xMinSpecial, zMaxSpecial].QAFaces[2] += increment;
                _editor.Level.Rooms[_editor.RoomIndex].Blocks[xMaxSpecial, zMaxSpecial].QAFaces[3] += increment;
                _editor.Level.Rooms[_editor.RoomIndex].Blocks[xMaxSpecial, zMinSpecial].QAFaces[0] += increment;
                _editor.Level.Rooms[_editor.RoomIndex].Blocks[xMinSpecial, zMinSpecial].QAFaces[1] += increment;

                for (int x = xMin; x <= xMax; x++)
                {
                    _editor.Level.Rooms[_editor.RoomIndex].Blocks[x, zMinSpecial].QAFaces[0] += increment;
                    _editor.Level.Rooms[_editor.RoomIndex].Blocks[x, zMinSpecial].QAFaces[1] += increment;

                    _editor.Level.Rooms[_editor.RoomIndex].Blocks[x, zMaxSpecial].QAFaces[3] += increment;
                    _editor.Level.Rooms[_editor.RoomIndex].Blocks[x, zMaxSpecial].QAFaces[2] += increment;
                }

                for (int z = zMin; z <= zMax; z++)
                {
                    _editor.Level.Rooms[_editor.RoomIndex].Blocks[xMinSpecial, z].QAFaces[1] += increment;
                    _editor.Level.Rooms[_editor.RoomIndex].Blocks[xMinSpecial, z].QAFaces[2] += increment;

                    _editor.Level.Rooms[_editor.RoomIndex].Blocks[xMaxSpecial, z].QAFaces[0] += increment;
                    _editor.Level.Rooms[_editor.RoomIndex].Blocks[xMaxSpecial, z].QAFaces[3] += increment;
                }
            }
            else if (face == 1)
            {
                _editor.Level.Rooms[_editor.RoomIndex].Blocks[xMinSpecial, zMaxSpecial].WSFaces[2] += increment;
                _editor.Level.Rooms[_editor.RoomIndex].Blocks[xMaxSpecial, zMaxSpecial].WSFaces[3] += increment;
                _editor.Level.Rooms[_editor.RoomIndex].Blocks[xMaxSpecial, zMinSpecial].WSFaces[0] += increment;
                _editor.Level.Rooms[_editor.RoomIndex].Blocks[xMinSpecial, zMinSpecial].WSFaces[1] += increment;

                for (int x = xMin; x <= xMax; x++)
                {
                    _editor.Level.Rooms[_editor.RoomIndex].Blocks[x, zMinSpecial].WSFaces[0] += increment;
                    _editor.Level.Rooms[_editor.RoomIndex].Blocks[x, zMinSpecial].WSFaces[1] += increment;

                    _editor.Level.Rooms[_editor.RoomIndex].Blocks[x, zMaxSpecial].WSFaces[3] += increment;
                    _editor.Level.Rooms[_editor.RoomIndex].Blocks[x, zMaxSpecial].WSFaces[2] += increment;
                }

                for (int z = zMin; z <= zMax; z++)
                {
                    _editor.Level.Rooms[_editor.RoomIndex].Blocks[xMinSpecial, z].WSFaces[1] += increment;
                    _editor.Level.Rooms[_editor.RoomIndex].Blocks[xMinSpecial, z].WSFaces[2] += increment;

                    _editor.Level.Rooms[_editor.RoomIndex].Blocks[xMaxSpecial, z].WSFaces[0] += increment;
                    _editor.Level.Rooms[_editor.RoomIndex].Blocks[xMaxSpecial, z].WSFaces[3] += increment;
                }
            }
        }

        public static void RandomFloor(short sign, int roomIndex, int xMin, int xMax, int zMin, int zMax)
        {
            Room room = _editor.Level.Rooms[roomIndex];

            Random r = new Random(new Random().Next());

            for (int x = xMin + 1; x <= xMax; x++)
            {
                for (int z = zMin + 1; z <= zMax; z++)
                {
                    short step = (short)(r.Next(0, 1000) < 500 ? 0 : 1 * sign);

                    _editor.Level.Rooms[roomIndex].Blocks[x, z].QAFaces[3] += step;

                    if (x > xMin)
                        _editor.Level.Rooms[roomIndex].Blocks[x - 1, z].QAFaces[2] = _editor.Level.Rooms[roomIndex].Blocks[x, z].QAFaces[3];
                    if (z > zMin)
                        _editor.Level.Rooms[roomIndex].Blocks[x, z - 1].QAFaces[0] = _editor.Level.Rooms[roomIndex].Blocks[x, z].QAFaces[3];

                    if (x > xMin && z > zMin)
                    {
                        _editor.Level.Rooms[roomIndex].Blocks[x - 1, z - 1].QAFaces[1] = _editor.Level.Rooms[roomIndex].Blocks[x, z].QAFaces[3];
                    }
                }
            }

            SmartBuildGeometry(roomIndex, xMin, xMax, zMin, zMax);
        }

        public static void RandomCeiling(short sign, int roomIndex, int xMin, int xMax, int zMin, int zMax)
        {
            Room room = _editor.Level.Rooms[roomIndex];

            Random r = new Random(new Random().Next());

            for (int x = xMin + 1; x <= xMax; x++)
            {
                for (int z = zMin + 1; z <= zMax; z++)
                {
                    short step = (short)(r.Next(0, 1000) < 500 ? 0 : 1 * sign);

                    _editor.Level.Rooms[roomIndex].Blocks[x, z].WSFaces[3] += step;

                    if (x > xMin)
                        _editor.Level.Rooms[roomIndex].Blocks[x - 1, z].WSFaces[2] = _editor.Level.Rooms[roomIndex].Blocks[x, z].WSFaces[3];
                    if (z > zMin)
                        _editor.Level.Rooms[roomIndex].Blocks[x, z - 1].WSFaces[0] = _editor.Level.Rooms[roomIndex].Blocks[x, z].WSFaces[3];

                    if (x > xMin && z > zMin)
                    {
                        _editor.Level.Rooms[roomIndex].Blocks[x - 1, z - 1].WSFaces[1] = _editor.Level.Rooms[roomIndex].Blocks[x, z].WSFaces[3];
                    }
                }
            }

            SmartBuildGeometry(roomIndex, xMin, xMax, zMin, zMax);
        }

        public static void AverageFloor(int roomIndex, int xMin, int xMax, int zMin, int zMax)
        {
            Room room = _editor.Level.Rooms[roomIndex];

            for (int x = xMin; x <= xMax; x++)
            {
                for (int z = zMin; z <= zMax; z++)
                {
                    Block b = room.Blocks[x, z];

                    short mean = (short)((b.QAFaces[0] + b.QAFaces[1] + b.QAFaces[2] + b.QAFaces[3]) / 4);

                    for (int i = 0; i < 4; i++)
                    {
                        _editor.Level.Rooms[roomIndex].Blocks[x, z].QAFaces[i] = mean;
                    }
                }
            }

            SmartBuildGeometry(roomIndex, xMin, xMax, zMin, zMax);
        }

        public static void AverageCeiling(int roomIndex, int xMin, int xMax, int zMin, int zMax)
        {
            Room room = _editor.Level.Rooms[roomIndex];

            for (int x = xMin; x <= xMax; x++)
            {
                for (int z = zMin; z <= zMax; z++)
                {
                    Block b = room.Blocks[x, z];

                    short mean = (short)((b.WSFaces[0] + b.WSFaces[1] + b.WSFaces[2] + b.WSFaces[3]) / 4);

                    for (int i = 0; i < 4; i++)
                    {
                        _editor.Level.Rooms[roomIndex].Blocks[x, z].WSFaces[i] = mean;
                    }
                }
            }

            SmartBuildGeometry(roomIndex, xMin, xMax, zMin, zMax);
        }
    }
}
