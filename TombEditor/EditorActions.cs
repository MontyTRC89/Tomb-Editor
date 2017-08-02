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
    public static class EditorActions
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

        public static void SmartBuildGeometry(Room room, Rectangle area)
        {
            var watch = new System.Diagnostics.Stopwatch();
            watch.Start();

            room.BuildGeometry(Math.Max(0, area.X - 1), Math.Min(room.NumXSectors, area.Right + 1),
                                                                 Math.Max(0, area.Y - 1), Math.Min(room.NumZSectors, area.Bottom + 1));
            room.CalculateLightingForThisRoom();
            room.UpdateBuffers();

            var portalsToTravel = new List<Portal>();

            for (int x = 0; x < room.NumXSectors; x++)
            {
                for (int z = 0; z < room.NumZSectors; z++)
                {
                    var wallPortal = room.Blocks[x, z].WallPortal;
                    var ceilingPortal = room.Blocks[x, z].CeilingPortal;
                    var floorPortal = room.Blocks[x, z].FloorPortal;

                    if (wallPortal != null && !portalsToTravel.Contains(wallPortal))
                        portalsToTravel.Add(wallPortal);

                    if (ceilingPortal != null && !portalsToTravel.Contains(ceilingPortal))
                        portalsToTravel.Add(ceilingPortal);

                    if (floorPortal != null && !portalsToTravel.Contains(floorPortal))
                        portalsToTravel.Add(floorPortal);
                }
            }

            //Parallel.ForEach(portalsToTravel, p =>
            // {
            List<Room> roomsProcessed = new List<Room>();

            foreach (var portal in portalsToTravel)
            {
                if (roomsProcessed.Contains(portal.AdjoiningRoom))
                    continue;
                roomsProcessed.Add(portal.AdjoiningRoom);

                // Calculate facing X and Z
                int facingXmin = 0;
                int facingXmax = 0;
                int facingZmin = 0;
                int facingZmax = 0;

                Room otherRoom = portal.AdjoiningRoom;

                if (portal.Direction == PortalDirection.North)
                {
                    if (area.Bottom < room.NumZSectors - 2)
                        continue;

                    facingXmin = portal.X + (int)(room.Position.X - otherRoom.Position.X) - 1;
                    facingXmin = portal.X + portal.NumXBlocks + (int)(room.Position.X - otherRoom.Position.X);
                    facingZmin = 0;
                    facingZmax = 1;
                }
                else if (portal.Direction == PortalDirection.South)
                {
                    if (area.Y > 1)
                        continue;

                    facingXmin = portal.X + (int)(room.Position.X - otherRoom.Position.X) - 1;
                    facingXmax = portal.X + portal.NumXBlocks + (int)(room.Position.X - otherRoom.Position.X);
                    facingZmin = otherRoom.NumZSectors - 2;
                    facingZmax = otherRoom.NumZSectors - 1;
                }
                else if (portal.Direction == PortalDirection.East)
                {
                    if (area.Right < room.NumXSectors - 2)
                        continue;

                    facingXmin = 0;
                    facingXmax = 1;
                    facingZmin = portal.Z + (int)(room.Position.Z - otherRoom.Position.Z) - 1;
                    facingZmax = portal.Z + portal.NumZBlocks + (int)(room.Position.Z - otherRoom.Position.Z);
                }
                else if (portal.Direction == PortalDirection.West)
                {
                    if (area.X > 1)
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

                portal.AdjoiningRoom.BuildGeometry(facingXmin,
                    facingXmax,
                    facingZmin,
                    facingZmax);

                portal.AdjoiningRoom.CalculateLightingForThisRoom();
                portal.AdjoiningRoom.UpdateBuffers();
            }

            //   });

            watch.Stop();
            logger.Debug("Edit geometry time: " + watch.ElapsedMilliseconds + "  ms");

            _editor.UpdateStatusStrip();
        }

        public static void EditFace(Room room, Rectangle area, FaceEditorActions action, FaceSubdivisions sub)
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

            for (int x = area.X; x <= area.Right; x++)
                for (int z = area.Y; z <= area.Bottom; z++)
                {
                    Block block = room.Blocks[x, z];

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
                                    room.Blocks[x, z].QAFaces[0] += increment;
                                if (block.FloorDiagonalSplit != DiagonalSplit.NE)
                                    room.Blocks[x, z].QAFaces[1] += increment;
                                if (block.FloorDiagonalSplit != DiagonalSplit.SE)
                                    room.Blocks[x, z].QAFaces[2] += increment;
                                if (block.FloorDiagonalSplit != DiagonalSplit.SW)
                                    room.Blocks[x, z].QAFaces[3] += increment;

                                if (block.FloorPortal != null && !block.IsFloorSolid)
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
                                    room.Blocks[x, z].WSFaces[0] += increment;
                                if (block.CeilingDiagonalSplit != DiagonalSplit.NE)
                                    room.Blocks[x, z].WSFaces[1] += increment;
                                if (block.CeilingDiagonalSplit != DiagonalSplit.SE)
                                    room.Blocks[x, z].WSFaces[2] += increment;
                                if (block.CeilingDiagonalSplit != DiagonalSplit.SW)
                                    room.Blocks[x, z].WSFaces[3] += increment;
                            }
                            else if (face == 2)
                            {
                                room.Blocks[x, z].EDFaces[0] += increment;
                                room.Blocks[x, z].EDFaces[1] += increment;
                                room.Blocks[x, z].EDFaces[2] += increment;
                                room.Blocks[x, z].EDFaces[3] += increment;

                                if (block.FloorPortal != null && !block.IsFloorSolid)
                                    continue;
                            }
                            else if (face == 3)
                            {
                                room.Blocks[x, z].RFFaces[0] += increment;
                                room.Blocks[x, z].RFFaces[1] += increment;
                                room.Blocks[x, z].RFFaces[2] += increment;
                                room.Blocks[x, z].RFFaces[3] += increment;
                            }
                            break;

                        case FaceEditorActions.EdgeN:
                            if (face == 0)
                            {
                                if (block.Type != BlockType.Wall && block.FloorDiagonalSplit != DiagonalSplit.None)
                                    continue;

                                if (block.FloorDiagonalSplit != DiagonalSplit.NW)
                                    room.Blocks[x, z].QAFaces[0] += increment;
                                if (block.FloorDiagonalSplit != DiagonalSplit.NE)
                                    room.Blocks[x, z].QAFaces[1] += increment;

                                if (block.FloorPortal != null && !block.IsFloorSolid)
                                    continue;
                            }
                            else if (face == 1)
                            {
                                if (block.Type != BlockType.Wall && block.FloorDiagonalSplit != DiagonalSplit.None)
                                    continue;

                                if (block.CeilingDiagonalSplit != DiagonalSplit.SE)
                                    room.Blocks[x, z].WSFaces[2] += increment;
                                if (block.CeilingDiagonalSplit != DiagonalSplit.SW)
                                    room.Blocks[x, z].WSFaces[3] += increment;
                            }
                            else if (face == 2)
                            {
                                if (block.Type != BlockType.Wall && block.FloorDiagonalSplit != DiagonalSplit.None)
                                    continue;

                                room.Blocks[x, z].EDFaces[0] += increment;
                                room.Blocks[x, z].EDFaces[1] += increment;

                                if (block.FloorPortal != null && !block.IsFloorSolid)
                                    continue;
                            }
                            else if (face == 3)
                            {
                                if (block.Type != BlockType.Wall && block.FloorDiagonalSplit != DiagonalSplit.None)
                                    continue;

                                room.Blocks[x, z].RFFaces[2] += increment;
                                room.Blocks[x, z].RFFaces[3] += increment;
                            }
                            break;

                        case FaceEditorActions.EdgeE:
                            if (face == 0)
                            {
                                if (block.Type != BlockType.Wall && block.FloorDiagonalSplit != DiagonalSplit.None)
                                    continue;

                                if (block.FloorDiagonalSplit != DiagonalSplit.NE)
                                    room.Blocks[x, z].QAFaces[1] += increment;
                                if (block.FloorDiagonalSplit != DiagonalSplit.SE)
                                    room.Blocks[x, z].QAFaces[2] += increment;

                                if (block.FloorPortal != null && !block.IsFloorSolid)
                                    continue;
                            }
                            else if (face == 1)
                            {
                                if (block.Type != BlockType.Wall && block.FloorDiagonalSplit != DiagonalSplit.None)
                                    continue;

                                if (block.CeilingDiagonalSplit != DiagonalSplit.NW)
                                    room.Blocks[x, z].WSFaces[0] += increment;
                                if (block.CeilingDiagonalSplit != DiagonalSplit.SW)
                                    room.Blocks[x, z].WSFaces[3] += increment;
                            }
                            else if (face == 2)
                            {
                                room.Blocks[x, z].EDFaces[1] += increment;
                                room.Blocks[x, z].EDFaces[2] += increment;

                                if (block.FloorPortal != null && !block.IsFloorSolid)
                                    continue;
                            }
                            else if (face == 3)
                            {
                                room.Blocks[x, z].RFFaces[0] += increment;
                                room.Blocks[x, z].RFFaces[3] += increment;
                            }
                            break;

                        case FaceEditorActions.EdgeS:
                            if (face == 0)
                            {
                                if (block.Type != BlockType.Wall && block.FloorDiagonalSplit != DiagonalSplit.None)
                                    continue;

                                if (block.FloorDiagonalSplit != DiagonalSplit.SE)
                                    room.Blocks[x, z].QAFaces[2] += increment;
                                if (block.FloorDiagonalSplit != DiagonalSplit.SW)
                                    room.Blocks[x, z].QAFaces[3] += increment;

                                if (block.FloorPortal != null && !block.IsFloorSolid)
                                    continue;
                            }
                            else if (face == 1)
                            {
                                if (block.Type != BlockType.Wall && block.FloorDiagonalSplit != DiagonalSplit.None)
                                    continue;

                                if (block.CeilingDiagonalSplit != DiagonalSplit.NW)
                                    room.Blocks[x, z].WSFaces[0] += increment;
                                if (block.CeilingDiagonalSplit != DiagonalSplit.NE)
                                    room.Blocks[x, z].WSFaces[1] += increment;
                            }
                            else if (face == 2)
                            {
                                room.Blocks[x, z].EDFaces[0] += increment;
                                room.Blocks[x, z].EDFaces[1] += increment;

                                if (block.FloorPortal != null && !block.IsFloorSolid)
                                    continue;
                            }
                            else if (face == 3)
                            {
                                room.Blocks[x, z].RFFaces[2] += increment;
                                room.Blocks[x, z].RFFaces[3] += increment;
                            }
                            break;

                        case FaceEditorActions.EdgeW:
                            if (face == 0)
                            {
                                if (block.Type != BlockType.Wall && block.FloorDiagonalSplit != DiagonalSplit.None)
                                    continue;

                                if (block.FloorDiagonalSplit != DiagonalSplit.NW)
                                    room.Blocks[x, z].QAFaces[0] += increment;
                                if (block.FloorDiagonalSplit != DiagonalSplit.SW)
                                    room.Blocks[x, z].QAFaces[3] += increment;

                                if (block.FloorPortal != null && !block.IsFloorSolid)
                                    continue;
                            }
                            else if (face == 1)
                            {
                                if (block.Type != BlockType.Wall && block.FloorDiagonalSplit != DiagonalSplit.None)
                                    continue;

                                if (block.CeilingDiagonalSplit != DiagonalSplit.NE)
                                    room.Blocks[x, z].WSFaces[1] += increment;
                                if (block.CeilingDiagonalSplit != DiagonalSplit.SE)
                                    room.Blocks[x, z].WSFaces[2] += increment;
                            }
                            else if (face == 2)
                            {
                                room.Blocks[x, z].EDFaces[0] += increment;
                                room.Blocks[x, z].EDFaces[3] += increment;

                                if (block.FloorPortal != null && !block.IsFloorSolid)
                                    continue;
                            }
                            else if (face == 3)
                            {
                                room.Blocks[x, z].RFFaces[1] += increment;
                                room.Blocks[x, z].RFFaces[2] += increment;
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

                                room.Blocks[x, z].QAFaces[0] += increment;

                                if (block.FloorPortal != null && !block.IsFloorSolid)
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

                                room.Blocks[x, z].WSFaces[3] += increment;
                            }
                            else if (face == 2)
                            {
                                room.Blocks[x, z].EDFaces[0] += increment;

                                if (block.FloorPortal != null && !block.IsFloorSolid)
                                    continue;
                            }
                            else if (face == 3)
                            {
                                room.Blocks[x, z].RFFaces[3] += increment;
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

                                room.Blocks[x, z].QAFaces[1] += increment;

                                if (block.FloorPortal != null && !block.IsFloorSolid)
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

                                room.Blocks[x, z].WSFaces[2] += increment;
                            }
                            else if (face == 2)
                            {
                                room.Blocks[x, z].EDFaces[1] += increment;

                                if (block.FloorPortal != null && !block.IsFloorSolid)
                                    continue;
                            }
                            else if (face == 3)
                            {
                                room.Blocks[x, z].RFFaces[2] += increment;
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

                                room.Blocks[x, z].QAFaces[2] += increment;

                                if (block.FloorPortal != null && !block.IsFloorSolid)
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

                                room.Blocks[x, z].WSFaces[1] += increment;
                            }
                            else if (face == 2)
                            {
                                room.Blocks[x, z].EDFaces[2] += increment;

                                if (block.FloorPortal != null && !block.IsFloorSolid)
                                    continue;
                            }
                            else if (face == 3)
                            {
                                room.Blocks[x, z].RFFaces[1] += increment;
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

                                room.Blocks[x, z].QAFaces[3] += increment;

                                if (block.FloorPortal != null && !block.IsFloorSolid)
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

                                room.Blocks[x, z].WSFaces[0] += increment;
                            }
                            else if (face == 2)
                            {
                                room.Blocks[x, z].EDFaces[3] += increment;

                                if (block.FloorPortal != null && !block.IsFloorSolid)
                                    continue;
                            }
                            else if (face == 3)
                            {
                                room.Blocks[x, z].RFFaces[0] += increment;
                            }
                            break;

                        case FaceEditorActions.DiagonalFloorCorner:
                            if (block.FloorDiagonalSplit == DiagonalSplit.NW)
                            {
                                if (block.QAFaces[0] == block.QAFaces[1] && increment > 0)
                                    continue;
                                room.Blocks[x, z].QAFaces[0] += increment;
                            }

                            if (block.FloorDiagonalSplit == DiagonalSplit.NE)
                            {
                                if (block.QAFaces[1] == block.QAFaces[2] && increment > 0)
                                    continue;
                                room.Blocks[x, z].QAFaces[1] += increment;
                            }

                            if (block.FloorDiagonalSplit == DiagonalSplit.SE)
                            {
                                if (block.QAFaces[2] == block.QAFaces[3] && increment > 0)
                                    continue;
                                room.Blocks[x, z].QAFaces[2] += increment;
                            }

                            if (block.FloorDiagonalSplit == DiagonalSplit.SW)
                            {
                                if (block.QAFaces[3] == block.QAFaces[0] && increment > 0)
                                    continue;
                                room.Blocks[x, z].QAFaces[3] += increment;
                            }

                            break;

                        case FaceEditorActions.DiagonalCeilingCorner:
                            if (block.CeilingDiagonalSplit == DiagonalSplit.NW)
                            {
                                if (block.WSFaces[0] == block.WSFaces[1] && increment < 0)
                                    continue;
                                room.Blocks[x, z].WSFaces[0] += increment;
                            }

                            if (block.CeilingDiagonalSplit == DiagonalSplit.NE)
                            {
                                if (block.WSFaces[1] == block.WSFaces[2] && increment < 0)
                                    continue;
                                room.Blocks[x, z].WSFaces[1] += increment;
                            }

                            if (block.CeilingDiagonalSplit == DiagonalSplit.SE)
                            {
                                if (block.WSFaces[2] == block.WSFaces[3] && increment < 0)
                                    continue;
                                room.Blocks[x, z].WSFaces[2] += increment;
                            }

                            if (block.CeilingDiagonalSplit == DiagonalSplit.SW)
                            {
                                if (block.WSFaces[3] == block.WSFaces[0] && increment < 0)
                                    continue;
                                room.Blocks[x, z].WSFaces[3] += increment;
                            }

                            break;
                    }
                }

            SmartBuildGeometry(room, area);
            _editor.UpdateStatusStrip();
        }

        public static void FlipFloorSplit(Room room, Rectangle area)
        {
            for (int x = area.X; x <= area.Right; x++)
                for (int z = area.Y; z <= area.Bottom; z++)
                    room.Blocks[x, z].SplitFoorType = 
                        (byte)(room.Blocks[x, z].SplitFoorType == 0 ? 1 : 0);

            SmartBuildGeometry(room, area);
            _editor.UpdateStatusStrip();
        }

        public static void FlipCeilingSplit(Room room, Rectangle area)
        {
            for (int x = area.X; x <= area.Right; x++)
                for (int z = area.Y; z <= area.Bottom; z++)
                    room.Blocks[x, z].SplitCeilingType =
                        (byte)(room.Blocks[x, z].SplitCeilingType == 0 ? 1 : 0);

            SmartBuildGeometry(room, area);
            _editor.UpdateStatusStrip();
        }

        public static void AddTrigger(Room room, Rectangle area, IWin32Window parent)
        { 
            TriggerInstance trigger = null;
            using (FormTrigger formTrigger = new FormTrigger())
            {
                formTrigger.TriggerID = -1;
                if (formTrigger.ShowDialog(parent) != DialogResult.OK)
                    return;
                trigger = formTrigger.Trigger;
                trigger.Room = room;
                trigger.Id = _editor.Level.GetNewTriggerId();
                trigger.X = (byte)area.X;
                trigger.Z = (byte)area.Y;
                trigger.NumXBlocks = (byte)(area.Width + 1);
                trigger.NumZBlocks = (byte)(area.Height + 1);
                _editor.Level.Triggers.Add(trigger.Id, trigger);

                for (int x = area.X; x <= area.Right; x++)
                    for (int z = area.Y; z <= area.Bottom; z++)
                        room.Blocks[x, z].Triggers.Add(trigger.Id);
            }
            _editor.LoadTriggersInUI();
            _editor.UpdateRoomName();
            _editor.DrawPanelGrid();
        }

        public static void MoveObject(Room room, ObjectType type, int id, GizmoAxis axis, float delta, bool smooth)
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
            
            if (type != ObjectType.Light)
            {
                Vector3 newPosition = new Vector3(_editor.Level.Objects[id].Position.X,
                                              _editor.Level.Objects[id].Position.Y,
                                              _editor.Level.Objects[id].Position.Z);

                switch (axis)
                {
                    case GizmoAxis.X:
                        newPosition += new Vector3(delta, 0.0f, 0.0f);
                        break;

                    case GizmoAxis.Y:
                        newPosition += new Vector3(0.0f, delta, 0.0f);
                        break;

                    case GizmoAxis.Z:
                        newPosition += new Vector3(0.0f, 0.0f, delta);
                        break;
                }

                var x = (int)Math.Floor(newPosition.X / 1024.0f);
                var z = (int)Math.Floor(newPosition.Z / 1024.0f);

                if (x < 0.0f || z < 0.0f || x > room.NumXSectors - 1 || z > room.NumZSectors - 1)
                {
                    return;
                }

                var lowest = room.GetLowestFloorCorner(x, z);
                var highest = room.GetHighestCeilingCorner(x, z);

                // Don't go outside room boundaries
                if (newPosition.X < 1024.0f || newPosition.X > (room.NumXSectors - 1) * 1024.0f ||
                    newPosition.Z < 1024.0f || newPosition.Z > (room.NumZSectors - 1) * 1024.0f ||
                    newPosition.Y < lowest * 256.0f || newPosition.Y > (room.Ceiling + highest) * 256.0f)
                {
                    return;
                }
                else
                {
                    _editor.Level.Objects[id].Position = newPosition;
                }
            }
            else
            {
                Vector3 newPosition = new Vector3(room.Lights[id].Position.X,
                                                  room.Lights[id].Position.Y,
                                                  room.Lights[id].Position.Z);

                switch (axis)
                {
                    case GizmoAxis.X:
                        newPosition += new Vector3(delta, 0.0f, 0.0f);
                        break;

                    case GizmoAxis.Y:
                        newPosition += new Vector3(0.0f, delta, 0.0f);
                        break;

                    case GizmoAxis.Z:
                        newPosition += new Vector3(0.0f, 0.0f, delta);
                        break;
                }

                var x = (int)Math.Floor(newPosition.X / 1024.0f);
                var z = (int)Math.Floor(newPosition.Z / 1024.0f);

                if (x < 0.0f || z < 0.0f || x > room.NumXSectors - 1 || z > room.NumZSectors - 1)
                {
                    return;
                }

                var lowest = room.GetLowestFloorCorner(x, z);
                var highest = room.GetHighestCeilingCorner(x, z);

                // Don't go outside room boundaries
                if (newPosition.X < 1024.0f || newPosition.X > (room.NumXSectors - 1) * 1024.0f ||
                    newPosition.Z < 1024.0f || newPosition.Z > (room.NumZSectors - 1) * 1024.0f ||
                    newPosition.Y < lowest * 256.0f || newPosition.Y > (room.Ceiling + highest) * 256.0f)
                {
                    return;
                }
                else
                {
                    room.Lights[id].Position = newPosition;
                }
            }
        }
        
        public static void RotateObject(Room room, ObjectType type, int id, int sign, bool smoothMove)
        {
            _editor.Level.Objects[_editor.PickingResult.Element].Rotation += (short)(sign * (smoothMove ? 5 : 45));

            if (_editor.Level.Objects[_editor.PickingResult.Element].Rotation == 360)
                _editor.Level.Objects[_editor.PickingResult.Element].Rotation = 0;

            if (_editor.Level.Objects[_editor.PickingResult.Element].Rotation < 0)
                _editor.Level.Objects[_editor.PickingResult.Element].Rotation += 360;
        }

        public static void DeleteObject(Room room, ObjectType type, int id)
        {
            _editor.Level.Objects.Remove(id);

            switch (type)
            {
                case ObjectType.Moveable:
                    room.Moveables.Remove(id);
                    break;

                case ObjectType.StaticMesh:
                    room.StaticMeshes.Remove(id);
                    break;

                case ObjectType.SoundSource:
                    room.SoundSources.Remove(id);
                    break;

                case ObjectType.Sink:
                    room.Sinks.Remove(id);
                    break;

                case ObjectType.Camera:
                    room.Cameras.Remove(id);
                    break;

                case ObjectType.FlybyCamera:
                    room.FlyByCameras.Remove(id);
                    break;
            }

            _editor.Level.DeleteObject(id);
            _editor.UpdateStatusStrip();
        }

        public static void MoveLight(Room room, int id, MoveObjectDirections direction, bool smoothMove)
        {
            switch (direction)
            {
                case MoveObjectDirections.Up:
                    room.Lights[id].Position += new Vector3(0.0f, (smoothMove ? 32.0f : 128.0f), 0.0f);
                    break;

                case MoveObjectDirections.Down:
                    room.Lights[id].Position -= new Vector3(0.0f, (smoothMove ? 32.0f : 128.0f), 0.0f);
                    break;

                case MoveObjectDirections.West:
                    if (room.Lights[id].Position.X > 1024.0f)
                        room.Lights[id].Position -= new Vector3((smoothMove ? 64.0f : 1024.0f), 0.0f, 0.0f);
                    break;

                case MoveObjectDirections.East:
                    if (room.Lights[id].Position.X < (room.NumXSectors - 1) * 1024.0f)
                        room.Lights[id].Position += new Vector3((smoothMove ? 64.0f : 1024.0f), 0.0f, 0.0f);
                    break;

                case MoveObjectDirections.North:
                    if (room.Lights[id].Position.Z < (room.NumZSectors - 1) * 1024.0f)
                        room.Lights[id].Position += new Vector3(0.0f, 0.0f, (smoothMove ? 64.0f : 1024.0f));
                    break;

                case MoveObjectDirections.South:
                    if (room.Lights[id].Position.Z > 1024.0f)
                        room.Lights[id].Position -= new Vector3(0.0f, 0.0f, (smoothMove ? 64.0f : 1024.0f));
                    break;
            }

            room.BuildGeometry();
            room.CalculateLightingForThisRoom();
            room.UpdateBuffers();
            _editor.UpdateStatusStrip();
        }

        public static void DeleteLight(Room room, int id)
        {
            room.Lights.RemoveAt(id);

            room.BuildGeometry();
            room.CalculateLightingForThisRoom();
            room.UpdateBuffers();
            _editor.UpdateStatusStrip();
        }

        public static void MoveLightCone(Room room, int id, int x, int y)
        {
            Light light = room.Lights[id];

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

            room.Lights[id] = light;

            room.BuildGeometry();
            room.CalculateLightingForThisRoom();
            room.UpdateBuffers();
            _editor.UpdateStatusStrip();
        }

        public static void MoveFlybyCone(Room room, int id, short x, short y)
        {
            FlybyCameraInstance flyby = (FlybyCameraInstance)_editor.Level.Objects[id];

            flyby.DirectionX += x;
            flyby.DirectionY += y;

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

        public static void PlaceTexture(Room room, int x, int z, BlockFaces faceType)
        {
            BlockFace face = room.Blocks[x, z].Faces[(int)faceType];

            ApplyTexture(room, x, z, faceType);

            face.Flipped = false;

            room.BuildGeometry(x, x, z, z);
            room.CalculateLightingForThisRoom();
            room.UpdateBuffers();
        }

        public static void PlaceNoCollision(Room room, int x, int z, BlockFaces faceType)
        {
            if (faceType == BlockFaces.Floor || faceType == BlockFaces.FloorTriangle2)
                room.Blocks[x, z].NoCollisionFloor = !room.Blocks[x, z].NoCollisionFloor;

            if (faceType == BlockFaces.Ceiling || faceType == BlockFaces.CeilingTriangle2)
                room.Blocks[x, z].NoCollisionCeiling = !room.Blocks[x, z].NoCollisionCeiling;

            room.BuildGeometry();
            room.CalculateLightingForThisRoom();
            room.UpdateBuffers();
        }

        public static void ApplyTexture(Room room, int x, int z, BlockFaces faceType)
        {
            if (_editor == null || (_editor.SelectedTexture == -1 && !_editor.InvisiblePolygon))
                return;

            BlockFace face = room.Blocks[x, z].Faces[(int)faceType];

            if (_editor.InvisiblePolygon)
            {
                room.Blocks[x, z].Faces[(int)faceType].Invisible = true;
                room.Blocks[x, z].Faces[(int)faceType].Transparent = false;
                room.Blocks[x, z].Faces[(int)faceType].DoubleSided = false;

                int tid = room.Blocks[x, z].Faces[(int)faceType].Texture;

                room.Blocks[x, z].Faces[(int)faceType].Texture = -1;
            }
            else
            {
                // if face was invisible, then reset flag
                if (face.Invisible)
                    room.Blocks[x, z].Faces[(int)faceType].Invisible = false;

                // set trasparency of this face
                if (_editor.Transparent)
                    room.Blocks[x, z].Faces[(int)faceType].Transparent = true;
                else
                    room.Blocks[x, z].Faces[(int)faceType].Transparent = false;

                // set double sided flag of this face
                if (_editor.DoubleSided)
                    room.Blocks[x, z].Faces[(int)faceType].DoubleSided = true;
                else
                    room.Blocks[x, z].Faces[(int)faceType].DoubleSided = false;

                Vector2[] UV = new Vector2[4];

                LevelTexture texture = _editor.Level.TextureSamples[_editor.SelectedTexture];

                int yBlock = (int)(texture.Page / 8);
                int xBlock = (int)(texture.Page % 8);

                UV[0] = new Vector2((xBlock * 256.0f + texture.X + 0.5f) / 2048.0f, (yBlock * 256.0f + texture.Y + 0.5f) / 2048.0f);
                UV[1] = new Vector2((xBlock * 256.0f + texture.X + texture.Width - 0.5f) / 2048.0f, (yBlock * 256.0f + texture.Y + 0.5f) / 2048.0f);
                UV[2] = new Vector2((xBlock * 256.0f + texture.X + texture.Width - 0.5f) / 2048.0f, (yBlock * 256.0f + texture.Y + texture.Height - 0.5f) / 2048.0f);
                UV[3] = new Vector2((xBlock * 256.0f + texture.X + 0.5f) / 2048.0f, (yBlock * 256.0f + texture.Y + texture.Height - 0.5f) / 2048.0f);

                room.Blocks[x, z].Faces[(int)faceType].RectangleUV[0] = UV[0];
                room.Blocks[x, z].Faces[(int)faceType].RectangleUV[1] = UV[1];
                room.Blocks[x, z].Faces[(int)faceType].RectangleUV[2] = UV[2];
                room.Blocks[x, z].Faces[(int)faceType].RectangleUV[3] = UV[3];

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
                        room.Blocks[x, z].Faces[(int)faceType].TriangleUV[0] = UV[0];
                        room.Blocks[x, z].Faces[(int)faceType].TriangleUV[1] = UV[1];
                        room.Blocks[x, z].Faces[(int)faceType].TriangleUV[2] = UV[3];

                        if (faceType == BlockFaces.FloorTriangle2 || faceType == BlockFaces.CeilingTriangle2)
                        {
                            room.Blocks[x, z].Faces[(int)faceType].TriangleUV2[0] = UV[0];
                            room.Blocks[x, z].Faces[(int)faceType].TriangleUV2[1] = UV[1];
                            room.Blocks[x, z].Faces[(int)faceType].TriangleUV2[2] = UV[3];
                        }
                    }

                    if (_editor.TextureTriangle == TextureTileType.TriangleNE)
                    {
                        room.Blocks[x, z].Faces[(int)faceType].TriangleUV[0] = UV[1];
                        room.Blocks[x, z].Faces[(int)faceType].TriangleUV[1] = UV[2];
                        room.Blocks[x, z].Faces[(int)faceType].TriangleUV[2] = UV[0];

                        if (faceType == BlockFaces.FloorTriangle2 || faceType == BlockFaces.CeilingTriangle2)
                        {
                            room.Blocks[x, z].Faces[(int)faceType].TriangleUV2[0] = UV[1];
                            room.Blocks[x, z].Faces[(int)faceType].TriangleUV2[1] = UV[2];
                            room.Blocks[x, z].Faces[(int)faceType].TriangleUV2[2] = UV[0];
                        }
                    }

                    if (_editor.TextureTriangle == TextureTileType.TriangleSE)
                    {
                        room.Blocks[x, z].Faces[(int)faceType].TriangleUV[0] = UV[2];
                        room.Blocks[x, z].Faces[(int)faceType].TriangleUV[1] = UV[3];
                        room.Blocks[x, z].Faces[(int)faceType].TriangleUV[2] = UV[1];

                        if (faceType == BlockFaces.FloorTriangle2 || faceType == BlockFaces.CeilingTriangle2)
                        {
                            room.Blocks[x, z].Faces[(int)faceType].TriangleUV2[0] = UV[2];
                            room.Blocks[x, z].Faces[(int)faceType].TriangleUV2[1] = UV[3];
                            room.Blocks[x, z].Faces[(int)faceType].TriangleUV2[2] = UV[1];
                        }
                    }

                    if (_editor.TextureTriangle == TextureTileType.TriangleSW)
                    {
                        room.Blocks[x, z].Faces[(int)faceType].TriangleUV[0] = UV[3];
                        room.Blocks[x, z].Faces[(int)faceType].TriangleUV[1] = UV[0];
                        room.Blocks[x, z].Faces[(int)faceType].TriangleUV[2] = UV[2];

                        if (faceType == BlockFaces.FloorTriangle2 || faceType == BlockFaces.CeilingTriangle2)
                        {
                            room.Blocks[x, z].Faces[(int)faceType].TriangleUV2[0] = UV[3];
                            room.Blocks[x, z].Faces[(int)faceType].TriangleUV2[1] = UV[0];
                            room.Blocks[x, z].Faces[(int)faceType].TriangleUV2[2] = UV[2];
                        }
                    }
                }

                if (face.Shape == BlockFaceShape.Triangle)
                {
                    room.Blocks[x, z].Faces[(int)faceType].TextureTriangle = _editor.TextureTriangle;
                }

                room.Blocks[x, z].Faces[(int)faceType].Texture = (short)_editor.SelectedTexture;
                room.Blocks[x, z].Faces[(int)faceType].Rotation = 0;
            }
        }

        public static void PlaceObject(Room room, int x, int z, ObjectType type, int id)
        {
            if (type == ObjectType.Moveable)
            {
                MoveableInstance instance = new MoveableInstance(_editor.Level.GetNewObjectId(), room);

                Block block = room.Blocks[x, z];
                int y = (block.QAFaces[0] + block.QAFaces[1] + block.QAFaces[2] + block.QAFaces[3]) / 4;

                instance.Position = new Vector3(x * 1024 + 512, y * 256, z * 1024 + 512);
                instance.Model = _editor.Level.Wad.Moveables[(uint)id];
                instance.Invisible = false;

                _editor.Level.Objects.Add(instance.Id, instance);
                room.Moveables.Add(instance.Id);
            }
            else if (type == ObjectType.StaticMesh)
            {
                StaticMeshInstance instance = new StaticMeshInstance(_editor.Level.GetNewObjectId(), room);

                Block block = room.Blocks[x, z];
                int y = (block.QAFaces[0] + block.QAFaces[1] + block.QAFaces[2] + block.QAFaces[3]) / 4;

                instance.Position = new Vector3(x * 1024 + 512, y * 256, z * 1024 + 512);
                instance.Model = _editor.Level.Wad.StaticMeshes[(uint)id];
                instance.Invisible = false;
                instance.Color = System.Drawing.Color.FromArgb(255, 128, 128, 128);

                _editor.Level.Objects.Add(instance.Id, instance);
                room.StaticMeshes.Add(instance.Id);
            }
            else if (type == ObjectType.Camera)
            {
                CameraInstance instance = new CameraInstance(_editor.Level.GetNewObjectId(), room);

                Block block = room.Blocks[x, z];
                int y = (block.QAFaces[0] + block.QAFaces[1] + block.QAFaces[2] + block.QAFaces[3]) / 4;

                instance.Position = new Vector3(x * 1024 + 512, y * 256, z * 1024 + 512);
                instance.Invisible = false;

                _editor.Action = EditorAction.None;
                _editor.Level.Objects.Add(instance.Id, instance);
                room.Cameras.Add(instance.Id);
            }
            else if (type == ObjectType.FlybyCamera)
            {
                FlybyCameraInstance instance = new FlybyCameraInstance(_editor.Level.GetNewObjectId(), room);

                Block block = room.Blocks[x, z];
                int y = (block.QAFaces[0] + block.QAFaces[1] + block.QAFaces[2] + block.QAFaces[3]) / 4;

                instance.Position = new Vector3(x * 1024 + 512, y * 256, z * 1024 + 512);
                instance.Invisible = false;

                _editor.Action = EditorAction.None;
                _editor.Level.Objects.Add(instance.Id, instance);
                room.FlyByCameras.Add(instance.Id);
            }
            else if (type == ObjectType.SoundSource)
            {
                SoundInstance instance = new SoundInstance(_editor.Level.GetNewObjectId(), room);

                Block block = room.Blocks[x, z];
                int y = (block.QAFaces[0] + block.QAFaces[1] + block.QAFaces[2] + block.QAFaces[3]) / 4;

                instance.Position = new Vector3(x * 1024 + 512, y * 256, z * 1024 + 512);
                instance.Invisible = false;
                instance.SoundId = (short)id;

                _editor.Action = EditorAction.None;
                _editor.Level.Objects.Add(instance.Id, instance);
                room.SoundSources.Add(instance.Id);
            }
            else if (type == ObjectType.Sink)
            {
                SinkInstance instance = new SinkInstance(_editor.Level.GetNewObjectId(), room);

                Block block = room.Blocks[x, z];
                int y = (block.QAFaces[0] + block.QAFaces[1] + block.QAFaces[2] + block.QAFaces[3]) / 4;

                instance.Position = new Vector3(x * 1024 + 512, y * 256, z * 1024 + 512);
                instance.Invisible = false;

                _editor.Action = EditorAction.None;
                _editor.Level.Objects.Add(instance.Id, instance);
                room.Sinks.Add(instance.Id);
            }
            _editor.UpdateStatusStrip();
        }

        public static void PlaceLight(Room room, int x, int z, LightType type)
        {
            Light instance = new Light();

            Block block = room.Blocks[x, z];
            int y = (block.QAFaces[0] + block.QAFaces[1] + block.QAFaces[2] + block.QAFaces[3]) / 4;

            instance.Position = new Vector3(x * 1024 + 512, y * 256 + 128.0f, z * 1024 + 512);
            instance.Color = System.Drawing.Color.White;
            instance.Active = true;
            instance.Intensity = 0.5f;
            instance.In = 1.0f;
            instance.Out = 5.0f;
            instance.Type = type;

            if (type == LightType.Shadow)
            {
                instance.Intensity *= -1;
            }

            if (type == LightType.Spot)
            {
                instance.Len = 2.0f;
                instance.Cutoff = 3.0f;
                instance.DirectionX = 0.0f;
                instance.DirectionY = 0.0f;
                instance.In = 20.0f;
                instance.Out = 25.0f;
            }

            if (type == LightType.Sun)
            {
                instance.DirectionX = 0.0f;
                instance.DirectionY = 0.0f;
            }

            if (type == LightType.Effect)
            {
                instance.In = 0.0f;
                instance.Out = 1024.0f;
            }

            room.Lights.Add(instance);

            room.BuildGeometry();
            room.CalculateLightingForThisRoom();
            room.UpdateBuffers();
            _editor.UpdateStatusStrip();
        }

        public static void CropRoom(Room room, Rectangle newArea)
        {
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
                    if (room.Blocks[x, z].FloorPortal != null || room.Blocks[x, z].CeilingPortal != null ||
                        room.Blocks[x, z].WallPortal != null)
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

            byte numXSectors = (byte)(newArea.Width + 3);
            byte numZSectors = (byte)(newArea.Height + 3);
            int newX = (int)(room.Position.X + newArea.X - 1);
            int newZ = (int)(room.Position.Z + newArea.Y - 1);
            int worldX = newX * 1024;
            int worldZ = newZ * 1024;

            var newRoom = new Room(_editor.Level);
            newRoom.Init(newX, (int)room.Position.Y, newZ, numXSectors, numZSectors, room.Ceiling);

            // First collect all items to remove
            List<int> objectsToRemove = new List<int>();
            List<int> lightsToRemove = new List<int>();
            List<int> triggersToRemove = new List<int>();

            for (int i = 0; i < _editor.Level.Objects.Count; i++)
            {
                ObjectInstance obj = _editor.Level.Objects.ElementAt(i).Value;
                if (obj.Room == room)
                {
                    if (obj.Position.X < (newX + 1) * 1024 || obj.Position.X > (newX + numXSectors - 1) * 1024 ||
                        obj.Position.Z < (newZ + 1) * 1024 || obj.Position.Z > (newZ + numZSectors - 1) * 1024)
                    {
                        // We must remove that object. First try to find a trigger.
                        for (int j = 0; j < _editor.Level.Triggers.Count; j++)
                        {
                            TriggerInstance trigger = _editor.Level.Triggers.ElementAt(j).Value;

                            if (trigger.TargetType == TriggerTargetType.Camera && obj.Type == ObjectInstanceType.Camera &&
                                trigger.Target == obj.Id)
                            {
                                triggersToRemove.Add(trigger.Id);
                            }

                            if (trigger.TargetType == TriggerTargetType.FlyByCamera && obj.Type == ObjectInstanceType.FlyByCamera &&
                                trigger.Target == ((FlybyCameraInstance)obj).Sequence)
                            {
                                triggersToRemove.Add(trigger.Id);
                            }

                            if (trigger.TargetType == TriggerTargetType.Sink && obj.Type == ObjectInstanceType.Sink &&
                                trigger.Target == obj.Id)
                            {
                                triggersToRemove.Add(trigger.Id);
                            }

                            if (trigger.TargetType == TriggerTargetType.Object && obj.Type == ObjectInstanceType.Moveable &&
                                trigger.Target == obj.Id)
                            {
                                triggersToRemove.Add(trigger.Id);
                            }
                        }

                        // Remove the object
                        objectsToRemove.Add(obj.Id);
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
                    newRoom.Blocks[x, z] = room.Blocks[x + newArea.X - 1, z + newArea.Y - 1].Clone();

                    /* for (int f = 0; f < newRoom.Blocks[x, z].Faces.Length; f++)
                     {
                         if (newRoom.Blocks[x, z].Faces[f].Texture != -1)
                         {
                             _editor.Level.TextureSamples[newRoom.Blocks[x, z].Faces[f].Texture].UsageCount++;
                         }
                     }*/

                    for (int k = 0; k < room.Blocks[x + newArea.X - 1, z + newArea.Y - 1].Triggers.Count; k++)
                    {
                        int triggerId = room.Blocks[x + newArea.X - 1, z + newArea.Y - 1].Triggers[k];
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
                if (trigger.Room == room)
                {
                    trigger.X -= (byte)newX;
                    trigger.Z -= (byte)newZ;

                    _editor.Level.Triggers[trigger.Id] = trigger;
                }
            }

            // Build the geometry of the new room
            newRoom.BuildGeometry();
            newRoom.CalculateLightingForThisRoom();
            newRoom.UpdateBuffers();


            // Fix selection if necessary
            if (_editor.SelectedRoom == room)
            {
                _editor.SelectedRoom = newRoom;
                _editor.BlockSelection = new Rectangle(1, 1, numXSectors - 2, numZSectors - 2);
            }

            // Update state
            _editor.CenterCamera();
            _editor.DrawPanel3D();
            _editor.DrawPanelGrid();
            _editor.UpdateStatusStrip();
            _editor.UpdateRoomName();
        }

        public static void SetDiagonalFloorSplit(Room room, Rectangle area)
        {
            for (int x = area.X; x <= area.Right; x++)
                for (int z = area.Y; z <= area.Bottom; z++)
                {
                    if (room.Blocks[x, z].FloorPortal != null)
                    {
                        Room otherRoom = room.Blocks[x, z].FloorPortal.AdjoiningRoom;

                        int lowX = x + (int)(room.Position.X - otherRoom.Position.X);
                        int lowZ = z + (int)(room.Position.Z - otherRoom.Position.Z);

                        room.Blocks[x, z].FloorPortal.AdjoiningRoom.Blocks[lowX, lowZ].IsCeilingSolid = true;
                    }

                    if (room.Blocks[x, z].CeilingPortal != null)
                    {
                        Room otherRoom = room.Blocks[x, z].CeilingPortal.AdjoiningRoom;

                        int lowX = x + (int)(room.Position.X - otherRoom.Position.X);
                        int lowZ = z + (int)(room.Position.Z - otherRoom.Position.Z);

                        room.Blocks[x, z].CeilingPortal.AdjoiningRoom.Blocks[lowX, lowZ].IsFloorSolid = true;
                    }

                    // Now try to guess the floor split
                    short maxHeight = -32767;
                    byte theCorner = 0;

                    if (room.Blocks[x, z].QAFaces[0] > maxHeight)
                    {
                        maxHeight = room.Blocks[x, z].QAFaces[0];
                        theCorner = 0;
                    }

                    if (room.Blocks[x, z].QAFaces[1] > maxHeight)
                    {
                        maxHeight = room.Blocks[x, z].QAFaces[1];
                        theCorner = 1;
                    }

                    if (room.Blocks[x, z].QAFaces[2] > maxHeight)
                    {
                        maxHeight = room.Blocks[x, z].QAFaces[2];
                        theCorner = 2;
                    }

                    if (room.Blocks[x, z].QAFaces[3] > maxHeight)
                    {
                        maxHeight = room.Blocks[x, z].QAFaces[3];
                        theCorner = 3;
                    }

                    if (theCorner == 0)
                    {
                        room.Blocks[x, z].QAFaces[1] = maxHeight;
                        room.Blocks[x, z].QAFaces[3] = maxHeight;
                        room.Blocks[x, z].FloorDiagonalSplit = DiagonalSplit.SE;
                    }

                    if (theCorner == 1)
                    {
                        room.Blocks[x, z].QAFaces[0] = maxHeight;
                        room.Blocks[x, z].QAFaces[2] = maxHeight;
                        room.Blocks[x, z].FloorDiagonalSplit = DiagonalSplit.SW;
                    }

                    if (theCorner == 2)
                    {
                        room.Blocks[x, z].QAFaces[1] = maxHeight;
                        room.Blocks[x, z].QAFaces[3] = maxHeight;
                        room.Blocks[x, z].FloorDiagonalSplit = DiagonalSplit.NW;
                    }

                    if (theCorner == 3)
                    {
                        room.Blocks[x, z].QAFaces[0] = maxHeight;
                        room.Blocks[x, z].QAFaces[2] = maxHeight;
                        room.Blocks[x, z].FloorDiagonalSplit = DiagonalSplit.NE;
                    }


                    room.Blocks[x, z].FloorDiagonalSplitType = DiagonalSplitType.Floor;
                }

            SmartBuildGeometry(room, area);
            _editor.DrawPanel3D();
            _editor.DrawPanelGrid();
            _editor.UpdateStatusStrip();
        }

        public static void SetDiagonalCeilingSplit(Room room, Rectangle area)
        {
            for (int x = area.X; x <= area.Right; x++)
                for (int z = area.Y; z <= area.Bottom; z++)
                {
                    if (room.Blocks[x, z].FloorPortal != null)
                    {
                        Room otherRoom = room.Blocks[x, z].FloorPortal.AdjoiningRoom;

                        int lowX = x + (int)(room.Position.X - otherRoom.Position.X);
                        int lowZ = z + (int)(room.Position.Z - otherRoom.Position.Z);

                        room.Blocks[x, z].FloorPortal.AdjoiningRoom.Blocks[lowX, lowZ].IsCeilingSolid = true;
                    }

                    if (room.Blocks[x, z].CeilingPortal != null)
                    {
                        Room otherRoom = room.Blocks[x, z].CeilingPortal.AdjoiningRoom;

                        int lowX = x + (int)(room.Position.X - otherRoom.Position.X);
                        int lowZ = z + (int)(room.Position.Z - otherRoom.Position.Z);

                        room.Blocks[x, z].CeilingPortal.AdjoiningRoom.Blocks[lowX, lowZ].IsFloorSolid = true;
                    }

                    // Now try to guess the floor split
                    short minHeight = 32767;
                    byte theCorner = 0;

                    if (room.Blocks[x, z].WSFaces[0] < minHeight)
                    {
                        minHeight = room.Blocks[x, z].WSFaces[0];
                        theCorner = 0;
                    }

                    if (room.Blocks[x, z].WSFaces[1] < minHeight)
                    {
                        minHeight = room.Blocks[x, z].WSFaces[1];
                        theCorner = 1;
                    }

                    if (room.Blocks[x, z].WSFaces[2] < minHeight)
                    {
                        minHeight = room.Blocks[x, z].WSFaces[2];
                        theCorner = 2;
                    }

                    if (room.Blocks[x, z].WSFaces[3] < minHeight)
                    {
                        minHeight = room.Blocks[x, z].WSFaces[3];
                        theCorner = 3;
                    }

                    if (theCorner == 0)
                    {
                        room.Blocks[x, z].WSFaces[1] = minHeight;
                        room.Blocks[x, z].WSFaces[3] = minHeight;
                        room.Blocks[x, z].CeilingDiagonalSplit = DiagonalSplit.SE;
                    }

                    if (theCorner == 1)
                    {
                        room.Blocks[x, z].WSFaces[0] = minHeight;
                        room.Blocks[x, z].WSFaces[2] = minHeight;
                        room.Blocks[x, z].CeilingDiagonalSplit = DiagonalSplit.SW;
                    }

                    if (theCorner == 2)
                    {
                        room.Blocks[x, z].WSFaces[1] = minHeight;
                        room.Blocks[x, z].WSFaces[3] = minHeight;
                        room.Blocks[x, z].CeilingDiagonalSplit = DiagonalSplit.NW;
                    }

                    if (theCorner == 3)
                    {
                        room.Blocks[x, z].WSFaces[0] = minHeight;
                        room.Blocks[x, z].WSFaces[2] = minHeight;
                        room.Blocks[x, z].CeilingDiagonalSplit = DiagonalSplit.NE;
                    }


                    room.Blocks[x, z].CeilingDiagonalSplitType = DiagonalSplitType.Floor;
                }

            SmartBuildGeometry(room, area);
            _editor.DrawPanel3D();
            _editor.DrawPanelGrid();
            _editor.UpdateStatusStrip();
        }

        public static void SetDiagonalWallSplit(Room room, Rectangle area)
        {
            for (int x = area.X; x <= area.Right; x++)
                for (int z = area.Y; z <= area.Bottom; z++)
                {
                    if (room.Blocks[x, z].FloorPortal != null)
                    {
                        Room otherRoom = room.Blocks[x, z].FloorPortal.AdjoiningRoom;

                        int lowX = x + (int)(room.Position.X - otherRoom.Position.X);
                        int lowZ = z + (int)(room.Position.Z - otherRoom.Position.Z);

                        room.Blocks[x, z].FloorPortal.AdjoiningRoom.Blocks[lowX, lowZ].IsCeilingSolid = true;
                    }

                    if (room.Blocks[x, z].CeilingPortal != null)
                    {
                        Room otherRoom = room.Blocks[x, z].CeilingPortal.AdjoiningRoom;

                        int lowX = x + (int)(room.Position.X - otherRoom.Position.X);
                        int lowZ = z + (int)(room.Position.Z - otherRoom.Position.Z);

                        room.Blocks[x, z].CeilingPortal.AdjoiningRoom.Blocks[lowX, lowZ].IsFloorSolid = true;
                    }

                    // Now try to guess the floor split
                    short maxHeight = -32767;
                    byte theCorner = 0;

                    if (room.Blocks[x, z].QAFaces[0] > maxHeight)
                    {
                        maxHeight = room.Blocks[x, z].QAFaces[0];
                        theCorner = 0;
                    }

                    if (room.Blocks[x, z].QAFaces[1] > maxHeight)
                    {
                        maxHeight = room.Blocks[x, z].QAFaces[1];
                        theCorner = 1;
                    }

                    if (room.Blocks[x, z].QAFaces[2] > maxHeight)
                    {
                        maxHeight = room.Blocks[x, z].QAFaces[2];
                        theCorner = 2;
                    }

                    if (room.Blocks[x, z].QAFaces[3] > maxHeight)
                    {
                        maxHeight = room.Blocks[x, z].QAFaces[3];
                        theCorner = 3;
                    }

                    if (theCorner == 0)
                    {
                        room.Blocks[x, z].QAFaces[1] = maxHeight;
                        room.Blocks[x, z].QAFaces[3] = maxHeight;
                        room.Blocks[x, z].FloorDiagonalSplit = DiagonalSplit.SE;
                        room.Blocks[x, z].CeilingDiagonalSplit = DiagonalSplit.SE;
                    }

                    if (theCorner == 1)
                    {
                        room.Blocks[x, z].QAFaces[0] = maxHeight;
                        room.Blocks[x, z].QAFaces[2] = maxHeight;
                        room.Blocks[x, z].FloorDiagonalSplit = DiagonalSplit.SW;
                        room.Blocks[x, z].CeilingDiagonalSplit = DiagonalSplit.SW;
                    }

                    if (theCorner == 2)
                    {
                        room.Blocks[x, z].QAFaces[1] = maxHeight;
                        room.Blocks[x, z].QAFaces[3] = maxHeight;
                        room.Blocks[x, z].FloorDiagonalSplit = DiagonalSplit.NW;
                        room.Blocks[x, z].CeilingDiagonalSplit = DiagonalSplit.NW;
                    }

                    if (theCorner == 3)
                    {
                        room.Blocks[x, z].QAFaces[0] = maxHeight;
                        room.Blocks[x, z].QAFaces[2] = maxHeight;
                        room.Blocks[x, z].FloorDiagonalSplit = DiagonalSplit.NE;
                        room.Blocks[x, z].CeilingDiagonalSplit = DiagonalSplit.NE;
                    }

                    room.Blocks[x, z].Type = BlockType.Wall;
                    room.Blocks[x, z].FloorDiagonalSplitType = DiagonalSplitType.Wall;
                    room.Blocks[x, z].CeilingDiagonalSplitType = DiagonalSplitType.None;
                }

            SmartBuildGeometry(room, area);
            _editor.DrawPanel3D();
            _editor.DrawPanelGrid();
            _editor.UpdateStatusStrip();
        }
        
        public static void SetWall(Room room, Rectangle area)
        {
            for (int x = area.X; x <= area.Right; x++)
                for (int z = area.Y; z <= area.Bottom; z++)
                {
                    room.Blocks[x, z].Type = BlockType.Wall;

                    if (room.Blocks[x, z].FloorPortal != null)
                    {
                        Room otherRoom = room.Blocks[x, z].FloorPortal.AdjoiningRoom;

                        int lowX = x + (int)(room.Position.X - otherRoom.Position.X);
                        int lowZ = z + (int)(room.Position.Z - otherRoom.Position.Z);

                        room.Blocks[x, z].FloorPortal.AdjoiningRoom.Blocks[lowX, lowZ].IsCeilingSolid = true;
                    }

                    if (room.Blocks[x, z].CeilingPortal != null)
                    {
                        Room otherRoom = room.Blocks[x, z].CeilingPortal.AdjoiningRoom;

                        int lowX = x + (int)(room.Position.X - otherRoom.Position.X);
                        int lowZ = z + (int)(room.Position.Z - otherRoom.Position.Z);

                        room.Blocks[x, z].CeilingPortal.AdjoiningRoom.Blocks[lowX, lowZ].IsFloorSolid = true;
                    }
                }

            SmartBuildGeometry(room, area);
            _editor.DrawPanel3D();
            _editor.DrawPanelGrid();
            _editor.DrawPanelMap2D();
            _editor.UpdateStatusStrip();
        }

        public static void SetFloor(Room room, Rectangle area)
        {
            for (int x = area.X; x <= area.Right; x++)
                for (int z = area.Y; z <= area.Bottom; z++)
                {
                    if (room.Blocks[x, z].Type == BlockType.BorderWall)
                        continue;
                    room.Blocks[x, z].SplitFloor = false;
                    room.Blocks[x, z].Type = BlockType.Floor;
                    room.Blocks[x, z].FloorDiagonalSplit = DiagonalSplit.None;
                }

            SmartBuildGeometry(room, area);
            _editor.DrawPanel3D();
            _editor.DrawPanelGrid();
            _editor.DrawPanelMap2D();
        }

        public static void SetCeiling(Room room, Rectangle area)
        {
            for (int x = area.X; x <= area.Right; x++)
                for (int z = area.Y; z <= area.Bottom; z++)
                {
                    if (room.Blocks[x, z].Type == BlockType.BorderWall)
                        continue;
                    room.Blocks[x, z].SplitCeiling = false;
                    room.Blocks[x, z].CeilingDiagonalSplit = DiagonalSplit.None;
                }

            SmartBuildGeometry(room, area);
            _editor.DrawPanel3D();
            _editor.DrawPanelGrid();
            _editor.DrawPanelMap2D();
            _editor.UpdateStatusStrip();
        }

        public static void ToggleBlockFlag(Room room, Rectangle area, BlockFlags flag)
        {
            for (int x = area.X; x <= area.Right; x++)
                for (int z = area.Y; z <= area.Bottom; z++)
                    room.Blocks[x, z].Flags ^= flag;

            _editor.DrawPanel3D();
            _editor.DrawPanelGrid();
            _editor.UpdateStatusStrip();
        }

        public static void ToggleClimb(Room room, Rectangle area, int direction)
        {
            for (int x = area.X; x <= area.Right; x++)
                for (int z = area.Y; z <= area.Bottom; z++)
                    room.Blocks[x, z].Climb[direction] = !room.Blocks[x, z].Climb[direction];

            _editor.DrawPanel3D();
            _editor.DrawPanelGrid();
            _editor.UpdateStatusStrip();
        }

        public static bool AddPortal(Room room, Rectangle area)
        {
            // The size of the portal
            int numXblocks = room.NumXSectors;
            int numZblocks = room.NumZSectors;
            
            // West wall
            if (area.X == 0 && area.Right == 0 && area.Y != 0 && area.Bottom != numZblocks - 1)
            {
                // Check for portal overlaps
                for (int z = area.Y; z <= area.Bottom; z++)
                {
                    if (room.Blocks[area.X, z].WallPortal != null)
                    {
                        return false;
                    }
                }

                int xPortalWorld = Utils.GetWorldX(room, area.X);
                int zPortalWorld = Utils.GetWorldZ(room, area.Y);

                // Search a compatible neighbour
                Room found = null;
                for (int i = 0; i < Level.MaxNumberOfRooms; i++)
                {
                    found = null;
                    Room otherRoom = _editor.Level.Rooms[i];

                    if (ReferenceEquals(otherRoom, room) || otherRoom == null)
                        continue;
                    if (otherRoom.Flipped != room.Flipped)
                        continue;

                    if (otherRoom.Position.X + otherRoom.NumXSectors - 1 == room.Position.X + 1 && room.Position.Z + area.Y >= otherRoom.Position.Z + 1 &&
                        room.Position.Z + area.Bottom <= otherRoom.Position.Z + otherRoom.NumZSectors - 1)
                    {
                        for (int z = area.Y; z <= area.Bottom; z++)
                        {
                            int facingZ = z + (int)(room.Position.Z - otherRoom.Position.Z);
                            if (facingZ < 1 || facingZ > otherRoom.NumZSectors - 1)
                            {
                                found = null;
                                break;
                            }

                            if (otherRoom.Blocks[otherRoom.NumXSectors - 1, facingZ].WallPortal != null)
                            {
                                found = null;
                                break;
                            }
                            else
                            {
                                found = otherRoom;
                            }
                        }
                    }

                    if (found != null)
                        break;
                }

                if (found != null)
                {
                    var currentRoomPortal = new Portal(0, room);
                    var otherRoomPortal = new Portal(0, found);
                    Room otherRoom = found;

                    int xPortalOther = (int)(xPortalWorld - otherRoom.Position.X);
                    int zPortalOther = (int)(zPortalWorld - otherRoom.Position.Z);

                    currentRoomPortal.Id = _editor.Level.GetNewPortalId();
                    currentRoomPortal.NumXBlocks = 1;
                    currentRoomPortal.NumZBlocks = (byte)(area.Bottom - area.Y + 1);
                    currentRoomPortal.Direction = PortalDirection.West;
                    currentRoomPortal.X = 0;
                    currentRoomPortal.Z = (byte)area.Y;
                    currentRoomPortal.AdjoiningRoom = found;
                    currentRoomPortal.MemberOfFlippedRoom = room.Flipped;

                    _editor.Level.Portals.Add(currentRoomPortal.Id, currentRoomPortal);
                    room.Portals.Add(currentRoomPortal);

                    otherRoomPortal.Id = _editor.Level.GetNewPortalId();
                    otherRoomPortal.NumXBlocks = 1;
                    otherRoomPortal.NumZBlocks = (byte)(area.Bottom - area.Y + 1);
                    otherRoomPortal.Direction = PortalDirection.East;
                    otherRoomPortal.X = (byte)(otherRoom.NumXSectors - 1);
                    otherRoomPortal.Z = (byte)zPortalOther;
                    otherRoomPortal.AdjoiningRoom = room;
                    otherRoomPortal.MemberOfFlippedRoom = otherRoom.Flipped;

                    _editor.Level.Portals.Add(otherRoomPortal.Id, otherRoomPortal);
                    found.Portals.Add(otherRoomPortal);

                    currentRoomPortal.Other = otherRoomPortal;
                    otherRoomPortal.Other = currentRoomPortal;

                    // Set the portal ID in sectors
                    for (int z = area.Y; z <= area.Bottom; z++)
                    {
                        room.Blocks[0, z].WallPortal = currentRoomPortal;
                        found.Blocks[otherRoom.NumXSectors - 1, z + (int)(room.Position.Z - otherRoom.Position.Z)].WallPortal = otherRoomPortal;
                    }

                    // Build geometry for current room
                    room.BuildGeometry();
                    room.CalculateLightingForThisRoom();
                    room.UpdateBuffers();

                    // Build geometry for adjoining room
                    found.BuildGeometry();
                    found.CalculateLightingForThisRoom();
                    found.UpdateBuffers();

                    // Update state
                    _editor.DrawPanel3D();
                    _editor.DrawPanelGrid();
                    _editor.DrawPanelMap2D();
                    _editor.UpdateStatusStrip();

                    return true;
                }
                else
                {
                    return false;
                }
            }

            // East wall
            if (area.X == numXblocks - 1 && area.Right == area.X && area.Y != 0 && area.Bottom != numZblocks - 1)
            {
                // Check for portal overlaps
                for (int z = area.Y; z <= area.Bottom; z++)
                {
                    if (room.Blocks[area.X, z].WallPortal != null)
                    {
                        return false;
                    }
                }

                int xPortalWorld = Utils.GetWorldX(room, area.X);
                int zPortalWorld = Utils.GetWorldZ(room, area.Y);

                // Search a compatible neighbour
                Room found = null;
                for (int i = 0; i < Level.MaxNumberOfRooms; i++)
                {
                    found = null;
                    Room otherRoom = _editor.Level.Rooms[i];

                    if (ReferenceEquals(otherRoom, room) || otherRoom == null)
                        continue;
                    if (otherRoom.Flipped != room.Flipped)
                        continue;

                    if (room.Position.X + room.NumXSectors - 1 == otherRoom.Position.X + 1 && room.Position.Z + area.Y >= otherRoom.Position.Z + 1 &&
                        room.Position.Z + area.Bottom <= otherRoom.Position.Z + otherRoom.NumZSectors - 1)
                    {
                        for (int z = area.Y; z <= area.Bottom; z++)
                        {
                            int facingZ = z + (int)(room.Position.Z - otherRoom.Position.Z);
                            if (facingZ < 1 || facingZ > otherRoom.NumZSectors - 1)
                            {
                                found = null;
                                break;
                            }

                            if (otherRoom.Blocks[0, facingZ].WallPortal != null)
                            {
                                found = null;
                                break;
                            }
                            else
                            {
                                found = otherRoom;
                            }
                        }
                    }

                    if (found != null)
                        break;
                }

                if (found != null)
                {
                    var currentRoomPortal = new Portal(0, room);
                    var otherRoomPortal = new Portal(0, found);
                    Room otherRoom = found;

                    int xPortalOther = (int)(xPortalWorld - otherRoom.Position.X);
                    int zPortalOther = (int)(zPortalWorld - otherRoom.Position.Z);

                    currentRoomPortal.Id = _editor.Level.GetNewPortalId();
                    currentRoomPortal.NumXBlocks = 1;
                    currentRoomPortal.NumZBlocks = (byte)(area.Bottom - area.Y + 1);
                    currentRoomPortal.Direction = PortalDirection.East;
                    currentRoomPortal.X = (byte)(numXblocks - 1);
                    currentRoomPortal.Z = (byte)area.Y;
                    currentRoomPortal.AdjoiningRoom = found;
                    currentRoomPortal.MemberOfFlippedRoom = room.Flipped;

                    _editor.Level.Portals.Add(currentRoomPortal.Id, currentRoomPortal);
                    room.Portals.Add(currentRoomPortal);

                    otherRoomPortal.Id = _editor.Level.GetNewPortalId();
                    otherRoomPortal.NumXBlocks = 1;
                    otherRoomPortal.NumZBlocks = (byte)(area.Bottom - area.Y + 1);
                    otherRoomPortal.Direction = PortalDirection.West;
                    otherRoomPortal.X = 0;
                    otherRoomPortal.Z = (byte)zPortalOther;
                    otherRoomPortal.AdjoiningRoom = room;
                    otherRoomPortal.MemberOfFlippedRoom = otherRoom.Flipped;

                    _editor.Level.Portals.Add(otherRoomPortal.Id, otherRoomPortal);
                    found.Portals.Add(otherRoomPortal);

                    currentRoomPortal.Other = otherRoomPortal;
                    otherRoomPortal.Other = currentRoomPortal;

                    // Set the portal ID in sectors
                    for (int z = area.Y; z <= area.Bottom; z++)
                    {
                        room.Blocks[numXblocks - 1, z].WallPortal = currentRoomPortal;
                        found.Blocks[0, z + (int)(room.Position.Z - otherRoom.Position.Z)].WallPortal = otherRoomPortal;
                    }

                    // Build geometry for current room
                    room.BuildGeometry();
                    room.CalculateLightingForThisRoom();
                    room.UpdateBuffers();

                    // Build geometry for adjoining room
                    found.BuildGeometry();
                    found.CalculateLightingForThisRoom();
                    found.UpdateBuffers();

                    // Update state
                    _editor.DrawPanel3D();
                    _editor.DrawPanelGrid();
                    _editor.DrawPanelMap2D();
                    _editor.UpdateStatusStrip();

                    return true;
                }
                else
                {
                    return false;
                }
            }

            // North wall
            if (area.Y == numZblocks - 1 && area.Bottom == area.Y && area.X != 0 && area.Right != numXblocks - 1)
            {
                // Check for portal overlaps
                for (int x = area.X; x <= area.Right; x++)
                {
                    if (room.Blocks[x, area.Y].WallPortal != null)
                    {
                        return false;
                    }
                }

                int xPortalWorld = Utils.GetWorldX(room, area.X);
                int zPortalWorld = Utils.GetWorldZ(room, area.Y);

                // Search a compatible neighbour
                Room found = null;
                for (int i = 0; i < Level.MaxNumberOfRooms; i++)
                {
                    found = null;
                    Room otherRoom = _editor.Level.Rooms[i];

                    if (ReferenceEquals(otherRoom, room) || otherRoom == null)
                        continue;
                    if (otherRoom.Flipped != room.Flipped)
                        continue;

                    if (room.Position.Z + room.NumZSectors - 1 == otherRoom.Position.Z + 1 && room.Position.X + area.X >= otherRoom.Position.X + 1 &&
                        room.Position.X + area.Right <= otherRoom.Position.X + otherRoom.NumXSectors - 1)
                    {
                        for (int x = area.X; x <= area.Right; x++)
                        {
                            int facingX = x + (int)(room.Position.X - otherRoom.Position.X);
                            if (facingX < 1 || facingX > otherRoom.NumXSectors - 1)
                            {
                                found = null;
                                break;
                            }

                            if (otherRoom.Blocks[facingX, 0].WallPortal != null)
                            {
                                found = null;
                                break;
                            }
                            else
                            {
                                found = otherRoom;
                            }
                        }
                    }

                    if (found != null)
                        break;
                }

                if (found != null)
                {
                    var currentRoomPortal = new Portal(0, room);
                    var otherRoomPortal = new Portal(0, found);
                    Room otherRoom = found;

                    byte xPortalOther = (byte)(xPortalWorld - otherRoom.Position.X);
                    byte zPortalOther = (byte)(zPortalWorld - otherRoom.Position.Z);

                    currentRoomPortal.Id = _editor.Level.GetNewPortalId();
                    currentRoomPortal.NumXBlocks = (byte)(area.Right - area.X + 1);
                    currentRoomPortal.NumZBlocks = 1;
                    currentRoomPortal.Direction = PortalDirection.North;
                    currentRoomPortal.X = (byte)area.X;
                    currentRoomPortal.Z = (byte)(numZblocks - 1);
                    currentRoomPortal.AdjoiningRoom = found;
                    currentRoomPortal.MemberOfFlippedRoom = room.Flipped;

                    _editor.Level.Portals.Add(currentRoomPortal.Id, currentRoomPortal);
                    room.Portals.Add(currentRoomPortal);

                    otherRoomPortal.Id = _editor.Level.GetNewPortalId();
                    otherRoomPortal.NumXBlocks = (byte)(area.Right - area.X + 1);
                    otherRoomPortal.NumZBlocks = 1;
                    otherRoomPortal.Direction = PortalDirection.South;
                    otherRoomPortal.X = xPortalOther;
                    otherRoomPortal.Z = 0;
                    otherRoomPortal.AdjoiningRoom = room;
                    otherRoomPortal.MemberOfFlippedRoom = otherRoom.Flipped;

                    _editor.Level.Portals.Add(otherRoomPortal.Id, otherRoomPortal);
                    found.Portals.Add(otherRoomPortal);

                    currentRoomPortal.Other = otherRoomPortal;
                    otherRoomPortal.Other = currentRoomPortal;

                    // Set portal ID in sectors
                    for (int x = area.X; x <= area.Right; x++)
                    {
                        room.Blocks[x, numZblocks - 1].WallPortal = currentRoomPortal;
                        found.Blocks[x + xPortalOther - area.X, 0].WallPortal = otherRoomPortal;
                    }

                    // Build geometry for current room
                    room.BuildGeometry();
                    room.CalculateLightingForThisRoom();
                    room.UpdateBuffers();

                    // Build geometry for adjoining room
                    found.BuildGeometry();
                    found.CalculateLightingForThisRoom();
                    found.UpdateBuffers();

                    // Update state
                    _editor.DrawPanel3D();
                    _editor.DrawPanelGrid();
                    _editor.DrawPanelMap2D();
                    _editor.UpdateStatusStrip();

                    return true;
                }
                else
                {
                    return false;
                }
            }

            // South wall
            if (area.Y == 0 && area.Bottom == area.Y && area.X != 0 && area.Right != numXblocks - 1)
            {
                // Check for portal overlaps
                for (int x = area.X; x <= area.Right; x++)
                {
                    if (room.Blocks[x, area.Y].WallPortal != null)
                    {
                        return false;
                    }
                }

                int xPortalWorld = Utils.GetWorldX(room, area.X);
                int zPortalWorld = Utils.GetWorldZ(room, area.Y);

                // Search a compatible neighbour
                Room found = null;
                for (int i = 0; i < Level.MaxNumberOfRooms; i++)
                {
                    found = null;
                    Room otherRoom = _editor.Level.Rooms[i];

                    if (ReferenceEquals(otherRoom, room) || otherRoom == null)
                        continue;
                    if (otherRoom.Flipped != room.Flipped)
                        continue;

                    if (otherRoom.Position.Z + otherRoom.NumZSectors - 1 == room.Position.Z + 1 && room.Position.X + area.X >= otherRoom.Position.X + 1 &&
                        room.Position.X + area.Right <= otherRoom.Position.X + otherRoom.NumXSectors - 1)
                    {
                        for (int x = area.X; x <= area.Right; x++)
                        {
                            int facingX = x + (int)(room.Position.X - otherRoom.Position.X);
                            if (facingX < 1 || facingX > otherRoom.NumXSectors - 1)
                            {
                                found = null;
                                break;
                            }

                            if (otherRoom.Blocks[facingX, 0].WallPortal != null)
                            {
                                found = null;
                                break;
                            }
                            else
                            {
                                found = otherRoom;
                            }
                        }
                    }

                    if (found != null)
                        break;
                }

                if (found != null)
                {
                    var currentRoomPortal = new Portal(0, room);
                    var otherRoomPortal = new Portal(0, found);
                    Room otherRoom = found;

                    int xPortalOther = (int)(xPortalWorld - otherRoom.Position.X);
                    int zPortalOther = (int)(zPortalWorld - otherRoom.Position.Z);

                    currentRoomPortal.Id = _editor.Level.GetNewPortalId();
                    currentRoomPortal.NumXBlocks = (byte)(area.Right - area.X + 1);
                    currentRoomPortal.NumZBlocks = 1;
                    currentRoomPortal.Direction = PortalDirection.South;
                    currentRoomPortal.X = (byte)area.X;
                    currentRoomPortal.Z = 0;
                    currentRoomPortal.AdjoiningRoom = found;
                    currentRoomPortal.MemberOfFlippedRoom = room.Flipped;

                    _editor.Level.Portals.Add(currentRoomPortal.Id, currentRoomPortal);
                    room.Portals.Add(currentRoomPortal);

                    otherRoomPortal.Id = _editor.Level.GetNewPortalId();
                    otherRoomPortal.NumXBlocks = (byte)(area.Right - area.X + 1);
                    otherRoomPortal.NumZBlocks = 1;
                    otherRoomPortal.Direction = PortalDirection.North;
                    otherRoomPortal.X = (byte)xPortalOther;
                    otherRoomPortal.Z = (byte)(otherRoom.NumZSectors - 1);
                    otherRoomPortal.AdjoiningRoom = room;
                    otherRoomPortal.MemberOfFlippedRoom = otherRoom.Flipped;

                    _editor.Level.Portals.Add(otherRoomPortal.Id, otherRoomPortal);
                    found.Portals.Add(otherRoomPortal);

                    currentRoomPortal.Other = otherRoomPortal;
                    otherRoomPortal.Other = currentRoomPortal;

                    // Set portal ID in sectors
                    for (int x = area.X; x <= area.Right; x++)
                    {
                        room.Blocks[x, 0].WallPortal = currentRoomPortal;
                        found.Blocks[x + xPortalOther - area.X, otherRoom.NumZSectors - 1].WallPortal = otherRoomPortal;
                    }

                    // Build geometry for current room
                    room.BuildGeometry();
                    room.CalculateLightingForThisRoom();
                    room.UpdateBuffers();

                    // Build geometry for adjoining room
                    found.BuildGeometry();
                    found.CalculateLightingForThisRoom();
                    found.UpdateBuffers();

                    // Update state
                    _editor.DrawPanel3D();
                    _editor.DrawPanelGrid();
                    _editor.DrawPanelMap2D();
                    _editor.UpdateStatusStrip();

                    return true;
                }
                else
                {
                    return false;
                }
            }

            // Floor - ceiling portal
            if (area.X > 0 && area.Right < numXblocks - 1 && area.Y > 0 && area.Bottom < numZblocks - 1)
            {
                int lowest = room.GetLowestCorner();
                ;

                // Check for floor heights in selected area
                for (int x = area.X; x <= area.Right; x++)
                {
                    for (int z = area.Y; z <= area.Bottom; z++)
                    {
                        int h1 = room.Blocks[x, z].QAFaces[0];
                        int h2 = room.Blocks[x, z].QAFaces[1];
                        int h3 = room.Blocks[x, z].QAFaces[2];
                        int h4 = room.Blocks[x, z].QAFaces[3];

                        // Check if the sector already has a portal
                        if (room.Blocks[x, z].Type != BlockType.Floor || room.Blocks[x, z].FloorPortal != null)
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
                Room found = null;

                for (int i = 0; i < Level.MaxNumberOfRooms; i++)
                {
                    found = null;
                    Room otherRoom = _editor.Level.Rooms[i];

                    if (ReferenceEquals(otherRoom, room) || otherRoom == null)
                        continue;
                    if (otherRoom.Flipped != room.Flipped)
                        continue;

                    int distance = (int)room.Position.Y + room.GetLowestCorner() - ((int)otherRoom.Position.Y + otherRoom.GetHighestCorner());
                    if (distance < 0 || distance > 2)
                        continue;

                    int lowXmin = area.X + (int)(room.Position.X - otherRoom.Position.X);
                    int lowXmax = area.Right + (int)(room.Position.X - otherRoom.Position.X);
                    int lowZmin = area.Y + (int)(room.Position.Z - otherRoom.Position.Z);
                    int lowZmax = area.Bottom + (int)(room.Position.Z - otherRoom.Position.Z);

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
                                otherRoom.Blocks[x, z].CeilingPortal != null)
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
                        found = otherRoom;
                        break;
                    }
                }

                // We have found a compatible neighbour
                if (found != null)
                {
                    Room otherRoom = found;
                    int highest = found.GetHighestCorner();

                    for (int x = area.X; x <= area.Right; x++)
                    {
                        for (int z = area.Y; z <= area.Bottom; z++)
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

                            bool isCurrentWall = (room.Blocks[x, z].Type == BlockType.Wall);
                            bool isOtherWall = (found.Blocks[lowX, lowZ].Type == BlockType.Wall);

                            bool isCurrentDiagonal = (room.Blocks[x, z].FloorDiagonalSplit != DiagonalSplit.None);
                            bool isOtherDiagonal = ((isOtherWall && found.Blocks[lowX, lowZ].FloorDiagonalSplit != DiagonalSplit.None) ||
                                                    found.Blocks[lowX, lowZ].CeilingDiagonalSplit != DiagonalSplit.None);

                            // In some cases the surface of the floor must be solid
                            if (Room.IsQuad(x, z, h1, h2, h3, h4, true) && h1 == lowest)
                            {
                                if (isCurrentWall || isOtherWall || isCurrentDiagonal || isOtherDiagonal)
                                {
                                    room.Blocks[x, z].IsFloorSolid = true;
                                    defined = false;
                                }
                                else
                                {
                                    room.Blocks[x, z].IsFloorSolid = false;
                                    defined = true;
                                }
                            }
                            else
                            {
                                room.Blocks[x, z].IsFloorSolid = true;
                                defined = false;
                            }

                            // In some cases the surface of the ceiling must be solid
                            if (Room.IsQuad(x, z, lh1, lh2, lh3, lh4, true) && lh1 == highest && defined)
                            {
                                if (isCurrentWall || isOtherWall || isCurrentDiagonal || isOtherDiagonal)
                                {
                                    found.Blocks[lowX, lowZ].IsCeilingSolid = true;
                                }
                                else
                                {
                                    found.Blocks[lowX, lowZ].IsCeilingSolid = false;
                                }
                            }
                            else
                            {
                                found.Blocks[lowX, lowZ].IsCeilingSolid = true;
                            }
                        }
                    }

                    var currentRoomPortal = new Portal(0, room);
                    var otherRoomPortal = new Portal(0, found);

                    currentRoomPortal.Id = _editor.Level.GetNewPortalId();
                    currentRoomPortal.NumXBlocks = (byte)(area.Right - area.X + 1);
                    currentRoomPortal.NumZBlocks = (byte)(area.Bottom - area.Y + 1);
                    currentRoomPortal.Direction = PortalDirection.Floor;
                    currentRoomPortal.X = (byte)area.X;
                    currentRoomPortal.Z = (byte)area.Y;
                    currentRoomPortal.AdjoiningRoom = found;
                    currentRoomPortal.MemberOfFlippedRoom = room.Flipped;

                    _editor.Level.Portals.Add(currentRoomPortal.Id, currentRoomPortal);
                    room.Portals.Add(currentRoomPortal);

                    int lowXmin = area.X + (int)(room.Position.X - otherRoom.Position.X);
                    int lowXmax = area.Right + (int)(room.Position.X - otherRoom.Position.X);
                    int lowZmin = area.Y + (int)(room.Position.Z - otherRoom.Position.Z);
                    int lowZmax = area.Bottom + (int)(room.Position.Z - otherRoom.Position.Z);

                    otherRoomPortal.Id = _editor.Level.GetNewPortalId();
                    otherRoomPortal.NumXBlocks = (byte)(lowXmax - lowXmin + 1);
                    otherRoomPortal.NumZBlocks = (byte)(lowZmax - lowZmin + 1);
                    otherRoomPortal.Direction = PortalDirection.Ceiling;
                    otherRoomPortal.X = (byte)lowXmin;
                    otherRoomPortal.Z = (byte)lowZmin;
                    otherRoomPortal.AdjoiningRoom = room;
                    otherRoomPortal.MemberOfFlippedRoom = otherRoom.Flipped;

                    _editor.Level.Portals.Add(otherRoomPortal.Id, otherRoomPortal);
                    found.Portals.Add(otherRoomPortal);

                    currentRoomPortal.Other = otherRoomPortal;
                    otherRoomPortal.Other = currentRoomPortal;

                    // Set floor portal ID
                    for (int x = area.X; x <= area.Right; x++)
                        for (int z = area.Y; z <= area.Bottom; z++)
                            room.Blocks[x, z].FloorPortal = currentRoomPortal;

                    // Set ceiling portal ID
                    for (int x = otherRoomPortal.X; x <= otherRoomPortal.X + otherRoomPortal.NumXBlocks - 1; x++)
                        for (int z = otherRoomPortal.Z; z <= otherRoomPortal.Z + otherRoomPortal.NumZBlocks - 1; z++)
                            found.Blocks[x, z].CeilingPortal = otherRoomPortal;

                    // Build geometry for current room
                    room.BuildGeometry();
                    room.CalculateLightingForThisRoom();
                    room.UpdateBuffers();

                    // Build geometry for adjoining room
                    found.BuildGeometry();
                    found.CalculateLightingForThisRoom();
                    found.UpdateBuffers();

                    // Update state
                    _editor.DrawPanel3D();
                    _editor.DrawPanelGrid();
                    _editor.DrawPanelMap2D();
                    _editor.UpdateStatusStrip();

                    return true;
                }
            }

            return false;
        }

        public static void SpecialRaiseFloorOrCeiling(Room room, int face, short increment,
                                                      int xMinSpecial, int xMaxSpecial, int zMinSpecial, int zMaxSpecial,
                                                      int xMin, int xMax, int zMin, int zMax)
        {
            if (face == 0)
            {
                room.Blocks[xMinSpecial, zMaxSpecial].QAFaces[2] += increment;
                room.Blocks[xMaxSpecial, zMaxSpecial].QAFaces[3] += increment;
                room.Blocks[xMaxSpecial, zMinSpecial].QAFaces[0] += increment;
                room.Blocks[xMinSpecial, zMinSpecial].QAFaces[1] += increment;

                for (int x = xMin; x <= xMax; x++)
                {
                    room.Blocks[x, zMinSpecial].QAFaces[0] += increment;
                    room.Blocks[x, zMinSpecial].QAFaces[1] += increment;

                    room.Blocks[x, zMaxSpecial].QAFaces[3] += increment;
                    room.Blocks[x, zMaxSpecial].QAFaces[2] += increment;
                }

                for (int z = zMin; z <= zMax; z++)
                {
                    room.Blocks[xMinSpecial, z].QAFaces[1] += increment;
                    room.Blocks[xMinSpecial, z].QAFaces[2] += increment;

                    room.Blocks[xMaxSpecial, z].QAFaces[0] += increment;
                    room.Blocks[xMaxSpecial, z].QAFaces[3] += increment;
                }
            }
            else if (face == 1)
            {
                room.Blocks[xMinSpecial, zMaxSpecial].WSFaces[2] += increment;
                room.Blocks[xMaxSpecial, zMaxSpecial].WSFaces[3] += increment;
                room.Blocks[xMaxSpecial, zMinSpecial].WSFaces[0] += increment;
                room.Blocks[xMinSpecial, zMinSpecial].WSFaces[1] += increment;

                for (int x = xMin; x <= xMax; x++)
                {
                    room.Blocks[x, zMinSpecial].WSFaces[0] += increment;
                    room.Blocks[x, zMinSpecial].WSFaces[1] += increment;

                    room.Blocks[x, zMaxSpecial].WSFaces[3] += increment;
                    room.Blocks[x, zMaxSpecial].WSFaces[2] += increment;
                }

                for (int z = zMin; z <= zMax; z++)
                {
                    room.Blocks[xMinSpecial, z].WSFaces[1] += increment;
                    room.Blocks[xMinSpecial, z].WSFaces[2] += increment;

                    room.Blocks[xMaxSpecial, z].WSFaces[0] += increment;
                    room.Blocks[xMaxSpecial, z].WSFaces[3] += increment;
                }
            }
            
            _editor.DrawPanel3D();
            _editor.DrawPanelGrid();
            _editor.UpdateStatusStrip();
        }

        public static void SmoothRandomFloor(Room room, Rectangle area, float strengthDirection)
        {
            float[,] changes = new float[area.Width + 2, area.Height + 2];
            Random rng = new Random();
            for (int x = 1; x <= area.Width; x++)
                for (int z = 1; z <= area.Height; z++)
                    changes[x, z] = rng.NextFloat(0, 1) * strengthDirection;

            for (int x = 0; x <= area.Width; x++)
                for (int z = 0; z <= area.Height; z++)
                    for (int i = 0; i < 4; ++i)
                        room.Blocks[area.X + x, area.Y + z].QAFaces[i] += 
                            (short)Math.Round(changes[x + Block.FaceX[i], z + Block.FaceZ[i]]);

            SmartBuildGeometry(room, area);
            _editor.DrawPanel3D();
            _editor.UpdateStatusStrip();
        }

        public static void SmoothRandomCeiling(Room room, Rectangle area, float strengthDirection)
        {
            float[,] changes = new float[area.Width + 2, area.Height + 2];
            Random rng = new Random();
            for (int x = 1; x <= area.Width; x++)
                for (int z = 1; z <= area.Height; z++)
                    changes[x, z] = rng.NextFloat(0, 1) * strengthDirection;

            for (int x = 0; x <= area.Width; x++)
                for (int z = 0; z <= area.Height; z++)
                    for (int i = 0; i < 4; ++i)
                        room.Blocks[area.X + x, area.Y + z].WSFaces[i] +=
                            (short)Math.Round(changes[x + Block.FaceX[i], z + Block.FaceZ[i]]);

            SmartBuildGeometry(room, area);
            _editor.DrawPanel3D();
            _editor.UpdateStatusStrip();
        }

        public static void SharpRandomFloor(Room room, Rectangle area, float strengthDirection)
        {
            Random rng = new Random();
            for (int x = 0; x <= area.Width; x++)
                for (int z = 0; z <= area.Height; z++)
                    for (int i = 0; i < 4; ++i)
                        room.Blocks[area.X + x, area.Y + z].QAFaces[i] +=
                            (short)Math.Round(rng.NextFloat(0, 1) * strengthDirection);

            SmartBuildGeometry(room, area);
            _editor.DrawPanel3D();
            _editor.UpdateStatusStrip();
        }

        public static void SharpRandomCeiling(Room room, Rectangle area, float strengthDirection)
        {
            Random rng = new Random();
            for (int x = 0; x <= area.Width; x++)
                for (int z = 0; z <= area.Height; z++)
                    for (int i = 0; i < 4; ++i)
                        room.Blocks[area.X + x, area.Y + z].WSFaces[i] +=
                            (short)Math.Round(rng.NextFloat(0, 1) * strengthDirection);

            SmartBuildGeometry(room, area);
            _editor.DrawPanel3D();
            _editor.UpdateStatusStrip();
        }

        public static void FlattenFloor(Room room, Rectangle area)
        {
            for (int x = area.X; x <= area.Right; x++)
                for (int z = area.Y; z <= area.Bottom; z++)
                {
                    Block b = room.Blocks[x, z];

                    short mean = (short)((b.QAFaces[0] + b.QAFaces[1] + b.QAFaces[2] + b.QAFaces[3]) / 4);

                    for (int i = 0; i < 4; i++)
                    {
                        room.Blocks[x, z].QAFaces[i] = mean;
                    }
                }

            SmartBuildGeometry(room, area);
            _editor.DrawPanel3D();
            _editor.UpdateStatusStrip();
        }

        public static void FlattenCeiling(Room room, Rectangle area)
        {
            for (int x = area.X; x <= area.Right; x++)
                for (int z = area.Y; z <= area.Bottom; z++)
                {
                    Block b = room.Blocks[x, z];

                    short mean = (short)((b.WSFaces[0] + b.WSFaces[1] + b.WSFaces[2] + b.WSFaces[3]) / 4);

                    for (int i = 0; i < 4; i++)
                    {
                        room.Blocks[x, z].WSFaces[i] = mean;
                    }
                }

            SmartBuildGeometry(room, area);
            _editor.DrawPanel3D();
            _editor.UpdateStatusStrip();
        }

        public static void GridWalls3(Room room, Rectangle Area)
        {
            for (int x = Area.X; x <= Area.Right; x++)
                for (int z = Area.Y; z <= Area.Bottom; z++)
                {
                    Block block = room.Blocks[x, z];
                    if (block.IsAnyWall)
                    {
                        VerticalArea?[] verticalAreas = new VerticalArea?[4];
                        for (int i = 0; i < 4; ++i)
                            verticalAreas[i] = room.GetHeightAtPointMinSpace(x + Block.FaceX[i], z + Block.FaceZ[i]);
                        if (verticalAreas.Any((verticalArea) => verticalArea.HasValue)) // We can only do it if there is information available
                            for (int i = 0; i < 4; ++i)
                            {
                                // Use the closest available vertical area information and divide it equally
                                VerticalArea verticalArea = verticalAreas[i] ?? verticalAreas[(i + 1) % 4] ?? verticalAreas[(i + 3) % 4] ?? verticalAreas[(i + 2) % 4].Value;
                                block.EDFaces[i] = (short)Math.Round(verticalArea.FloorY);
                                block.QAFaces[i] = (short)Math.Round((verticalArea.FloorY * 2.0f + verticalArea.CeilingY * 1.0f) / 3.0f);
                                block.WSFaces[i] = (short)Math.Round((verticalArea.FloorY * 1.0f + verticalArea.CeilingY * 2.0f) / 3.0f - room.Ceiling);
                                block.RFFaces[i] = (short)Math.Round(verticalArea.CeilingY);
                            }
                    }
                }

            // Update the state ...
            room.BuildGeometry();
            room.CalculateLightingForThisRoom();
            room.UpdateBuffers();
            _editor.DrawPanel3D();
            _editor.UpdateStatusStrip();
        }

        public static void GridWalls5(Room room, Rectangle Area)
        {
            for (int x = Area.X; x <= Area.Right; x++)
                for (int z = Area.Y; z <= Area.Bottom; z++)
                {
                    Block block = room.Blocks[x, z];
                    if (block.IsAnyWall)
                    {
                        VerticalArea?[] verticalAreas = new VerticalArea?[4];
                        for (int i = 0; i < 4; ++i)
                            verticalAreas[i] = room.GetHeightAtPointMinSpace(x + Block.FaceX[i], z + Block.FaceZ[i]);
                        if (verticalAreas.Any(verticalArea => verticalArea.HasValue)) // We can only do it if there is information available
                            for (int i = 0; i < 4; ++i)
                            {
                                // Use the closest available vertical area information and divide it equally
                                VerticalArea verticalArea = verticalAreas[i] ?? verticalAreas[(i + 1) % 4] ?? verticalAreas[(i + 3) % 4] ?? verticalAreas[(i + 2) % 4].Value;
                                block.EDFaces[i] = (short)Math.Round((verticalArea.FloorY * 4.0f + verticalArea.CeilingY * 1.0f) / 5.0f);
                                block.QAFaces[i] = (short)Math.Round((verticalArea.FloorY * 3.0f + verticalArea.CeilingY * 2.0f) / 5.0f);
                                block.WSFaces[i] = (short)Math.Round((verticalArea.FloorY * 2.0f + verticalArea.CeilingY * 3.0f) / 5.0f - room.Ceiling);
                                block.RFFaces[i] = (short)Math.Round((verticalArea.FloorY * 1.0f + verticalArea.CeilingY * 4.0f) / 5.0f - room.Ceiling);
                            }
                    }
                }

            // Update the state ...
            room.BuildGeometry();
            room.CalculateLightingForThisRoom();
            room.UpdateBuffers();
            _editor.DrawPanel3D();
            _editor.UpdateStatusStrip();
        }

        public static void DeletePortal(Room room, Portal current)
        {
            var other = current.Other;

            for (int x = current.X; x < current.X + current.NumXBlocks; x++)
            {
                for (int z = current.Z; z < current.Z + current.NumZBlocks; z++)
                {
                    if (current.Direction == PortalDirection.Floor)
                    {
                        room.Blocks[x, z].FloorPortal = null;
                        room.Blocks[x, z].FloorOpacity = PortalOpacity.None;
                    }

                    if (current.Direction == PortalDirection.Ceiling)
                    {
                        room.Blocks[x, z].CeilingPortal = null;
                        room.Blocks[x, z].CeilingOpacity = PortalOpacity.None;
                    }

                    if (current.Direction == PortalDirection.North || current.Direction == PortalDirection.South ||
                        current.Direction == PortalDirection.West || current.Direction == PortalDirection.East)
                    {
                        room.Blocks[x, z].WallPortal = null;
                        room.Blocks[x, z].WallOpacity = PortalOpacity.None;
                    }
                }
            }

            for (int x = other.X; x < other.X + other.NumXBlocks; x++)
            {
                for (int z = other.Z; z < other.Z + other.NumZBlocks; z++)
                {
                    if (other.Direction == PortalDirection.Ceiling)
                    {
                        other.Room.Blocks[x, z].CeilingPortal = null;
                        other.Room.Blocks[x, z].CeilingOpacity = PortalOpacity.None;
                    }

                    if (other.Direction == PortalDirection.Floor)
                    {
                        other.Room.Blocks[x, z].FloorPortal = null;
                        other.Room.Blocks[x, z].FloorOpacity = PortalOpacity.None;
                    }

                    if (other.Direction == PortalDirection.North || other.Direction == PortalDirection.South ||
                        other.Direction == PortalDirection.West || other.Direction == PortalDirection.East)
                    {
                        other.Room.Blocks[x, z].WallPortal = null;
                        other.Room.Blocks[x, z].WallOpacity = PortalOpacity.None;
                    }
                }
            }

            current.Room.Portals.Remove(current);
            other.Room.Portals.Remove(other);

            _editor.Level.Portals.Remove(current.Id);
            _editor.Level.Portals.Remove(other.Id);

            room.BuildGeometry();
            room.CalculateLightingForThisRoom();
            room.UpdateBuffers();

            other.Room.BuildGeometry();
            other.Room.CalculateLightingForThisRoom();
            other.Room.UpdateBuffers();
            _editor.UpdateStatusStrip();
        }
    }
}
