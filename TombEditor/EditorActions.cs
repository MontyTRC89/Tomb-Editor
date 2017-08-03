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

        public static void EditSectorGeometry(Room room, Rectangle area, EditorArrowType arrow, int face, short increment, bool smooth)
        {
            if (smooth)
            {
                // Choose between QA, WS, ED and RF array
                Func<int, int, short[]> getFaces;
                switch (face)
                {
                    case 0:
                        getFaces = (x, z) => room.Blocks[x, z].QAFaces;
                        break;
                    case 1:
                        getFaces = (x, z) => room.Blocks[x, z].WSFaces;
                        break;
                    case 2:
                        getFaces = (x, z) => room.Blocks[x, z].EDFaces;
                        break;
                    case 3:
                        getFaces = (x, z) => room.Blocks[x, z].RFFaces;
                        break;
                    default:
                        throw new NotSupportedException();
                }

                // Get coordinates
                int xMin = area.X;
                int xMax = area.Right;
                int zMin = area.Y;
                int zMax = area.Bottom;
                int xMinSpecial = Math.Max(0, xMin - 1);
                int zMinSpecial = Math.Max(0, zMin - 1);
                int xMaxSpecial = Math.Min(_editor.SelectedRoom.NumXSectors - 1, xMax + 1);
                int zMaxSpecial = Math.Min(_editor.SelectedRoom.NumXSectors - 1, zMax + 1);

                // Build smooth edges
                getFaces(xMinSpecial, zMaxSpecial)[2] += increment;
                getFaces(xMaxSpecial, zMaxSpecial)[3] += increment;
                getFaces(xMaxSpecial, zMinSpecial)[0] += increment;
                getFaces(xMinSpecial, zMinSpecial)[1] += increment;

                for (int x = xMin; x <= xMax; x++)
                {
                    getFaces(x, zMinSpecial)[0] += increment;
                    getFaces(x, zMinSpecial)[1] += increment;

                    getFaces(x, zMaxSpecial)[3] += increment;
                    getFaces(x, zMaxSpecial)[2] += increment;
                }

                for (int z = zMin; z <= zMax; z++)
                {
                    getFaces(xMinSpecial, z)[1] += increment;
                    getFaces(xMinSpecial, z)[2] += increment;

                    getFaces(xMaxSpecial, z)[0] += increment;
                    getFaces(xMaxSpecial, z)[3] += increment;
                }
            }


            for (int x = area.X; x <= area.Right; x++)
                for (int z = area.Y; z <= area.Bottom; z++)
                {
                    Block block = room.Blocks[x, z];

                    switch (arrow)
                    {
                        case EditorArrowType.EntireFace:
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

                        case EditorArrowType.EdgeN:
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

                        case EditorArrowType.EdgeE:
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

                        case EditorArrowType.EdgeS:
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

                        case EditorArrowType.EdgeW:
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

                        case EditorArrowType.CornerNW:
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

                        case EditorArrowType.CornerNE:
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

                        case EditorArrowType.CornerSE:
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

                        case EditorArrowType.CornerSW:
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

                        case EditorArrowType.DiagonalFloorCorner:
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

                        case EditorArrowType.DiagonalCeilingCorner:
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
            _editor.DrawPanel3D();
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
            TriggerInstance trigger = new TriggerInstance(_editor.Level.GetNewTriggerId(), room);
            using (FormTrigger formTrigger = new FormTrigger(_editor.Level, trigger))
            {
                if (formTrigger.ShowDialog(parent) != DialogResult.OK)
                    return;
                trigger.Room = room;
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

        public static Vector3 GetMovementPrecision(Keys modifierKeys)
        {
            if (modifierKeys.HasFlag(Keys.Shift | Keys.Control))
                return new Vector3(0.0f);
            if (modifierKeys.HasFlag(Keys.Shift))
                return new Vector3(64.0f);
            return new Vector3(512.0f, 128.0f, 512.0f);
        }

        public static void MoveObject(Room room, ObjectPtr objectPtr, Vector3 pos, Keys modifierKeys)
        {
            MoveObject(room, objectPtr, pos, GetMovementPrecision(modifierKeys), modifierKeys.HasFlag(Keys.Alt));
        }

        public static void MoveObject(Room room, ObjectPtr objectPtr, Vector3 pos, Vector3 precision = new Vector3(), bool canGoOutsideRoom = false)
        {
            switch (objectPtr.Type)
            {
                case ObjectInstanceType.Moveable:
                case ObjectInstanceType.StaticMesh:
                case ObjectInstanceType.SoundSource:
                case ObjectInstanceType.Sink:
                case ObjectInstanceType.Camera:
                case ObjectInstanceType.FlyByCamera:
                case ObjectInstanceType.Light:
                    // Limit movement precision
                    for (int i = 0; i < 3; ++i)
                        if (precision[i] != 0)
                            pos[i] = ((float)Math.Round(pos[i] / precision[i])) * precision[i];

                    // Limit movement area ...
                    if (!canGoOutsideRoom)
                    {
                        float x = (float)Math.Floor(pos.X / 1024.0f);
                        float z = (float)Math.Floor(pos.Z / 1024.0f);

                        if ((x < 0.0f) || (x > (room.NumXSectors - 1)) ||
                            (z < 0.0f) || (z > (room.NumZSectors - 1)))
                            return;

                        if (room.Blocks[(int)x, (int)z].IsAnyWall)
                            return;

                        var lowest = room.GetLowestFloorCorner((int)x, (int)z);
                        var highest = room.GetHighestCeilingCorner((int)x, (int)z);

                        // Don't go outside room boundaries
                        if ((pos.X < 1024.0f) || (pos.X > (room.NumXSectors - 1) * 1024.0f) ||
                            (pos.Z < 1024.0f) || (pos.Z > (room.NumZSectors - 1) * 1024.0f) ||
                            (pos.Y < lowest * 256.0f) || (pos.Y > highest * 256.0f))
                            return;
                    }

                    // Update position
                    if (objectPtr.Type == ObjectInstanceType.Light)
                        room.Lights[objectPtr.Id].Position = pos;
                    else
                        _editor.Level.Objects[objectPtr.Id].Position = pos;

                    // Update state
                    if (objectPtr.Type == ObjectInstanceType.Light)
                    {
                        _editor.SelectedRoom.CalculateLightingForThisRoom();
                        _editor.SelectedRoom.UpdateBuffers();
                    }
                    _editor.DrawPanel3D();
                    break;
            }
        }

        public static void RotateObject(Room room, ObjectPtr objectPtr, int sign, bool smoothMove)
        {
            switch (objectPtr.Type)
            {
                case ObjectInstanceType.Moveable:
                case ObjectInstanceType.StaticMesh:
                case ObjectInstanceType.SoundSource:
                case ObjectInstanceType.Sink:
                case ObjectInstanceType.Camera:
                case ObjectInstanceType.FlyByCamera:
                    _editor.Level.Objects[objectPtr.Id].Rotation += (short)(sign * (smoothMove ? 5 : 45));

                    if (_editor.Level.Objects[objectPtr.Id].Rotation == 360)
                        _editor.Level.Objects[objectPtr.Id].Rotation = 0;

                    if (_editor.Level.Objects[objectPtr.Id].Rotation < 0)
                        _editor.Level.Objects[objectPtr.Id].Rotation += 360;
                    break;
            }
        }

        public static void EditObject(Room room, ObjectPtr objectPtr, IWin32Window owner)
        {
            switch (objectPtr.Type)
            {
                case ObjectInstanceType.Moveable:
                    using (FormMoveable formMoveable = new FormMoveable((MoveableInstance)(_editor.Level.Objects[objectPtr.Id])))
                        formMoveable.ShowDialog(owner);
                    break;
                case ObjectInstanceType.FlyByCamera:
                    using (FormFlybyCamera formFlyby = new FormFlybyCamera((FlybyCameraInstance)(_editor.Level.Objects[objectPtr.Id])))
                        formFlyby.ShowDialog(owner);
                    break;
                case ObjectInstanceType.Sink:
                    using (FormSink formSink = new FormSink((SinkInstance)(_editor.Level.Objects[objectPtr.Id])))
                        formSink.ShowDialog(owner);
                    break;
                case ObjectInstanceType.SoundSource:
                    using (FormSound formSound = new FormSound((SoundSourceInstance)(_editor.Level.Objects[objectPtr.Id])))
                        formSound.ShowDialog(owner);
                    break;
                case ObjectInstanceType.Trigger:
                    using (FormTrigger formSound = new FormTrigger(_editor.Level, (TriggerInstance)(_editor.Level.Triggers[objectPtr.Id])))
                        formSound.ShowDialog(owner);
                    break;
            }
        }

        public static void DeleteObjectWithWarning(Room room, ObjectPtr objectPtr, IWin32Window owner)
        {
            if (room.Flipped)
            {
                DarkUI.Forms.DarkMessageBox.ShowError("You can't delete portals of a flipped room", "Error");
                return;
            }

            if (DarkUI.Forms.DarkMessageBox.ShowWarning("Do you really want to delete " + objectPtr.ToString() + "?",
                    "Confirm delete", DarkUI.Forms.DarkDialogButton.YesNo) != DialogResult.Yes)
                return;

            DeleteObject(room, objectPtr);
        }

        public static void DeleteObject(Room room, ObjectPtr objectPtr)
        {
            switch (objectPtr.Type)
            {
                case ObjectInstanceType.Portal:
                    if (room.Flipped)
                        return;

                    Portal current = _editor.Level.Portals.First(p => p.Id == objectPtr.Id);
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

                    // Update state
                    room.BuildGeometry();
                    room.CalculateLightingForThisRoom();
                    room.UpdateBuffers();

                    other.Room.BuildGeometry();
                    other.Room.CalculateLightingForThisRoom();
                    other.Room.UpdateBuffers();

                    if (_editor.SelectedObject == objectPtr)
                        _editor.SelectedObject = null;
                    _editor.DrawPanelGrid();
                    break;

                case ObjectInstanceType.Trigger:
                    _editor.Level.DeleteTrigger(objectPtr.Id);
                    _editor.Level.Objects.Remove(objectPtr.Id);
                    room.Moveables.Remove(objectPtr.Id);

                    // Update state
                    if (_editor.SelectedObject == objectPtr)
                        _editor.SelectedObject = null;
                    _editor.DrawPanelGrid();
                    _editor.LoadTriggersInUI();
                    room.UpdateBuffers();
                    break;

                case ObjectInstanceType.Light:
                    _editor.Level.Objects.Remove(objectPtr.Id);
                    room.Lights.RemoveAt(objectPtr.Id);

                    room.BuildGeometry();
                    room.CalculateLightingForThisRoom();
                    room.UpdateBuffers();
                    break;

                case ObjectInstanceType.Moveable:
                    _editor.Level.Objects.Remove(objectPtr.Id);
                    room.Moveables.Remove(objectPtr.Id);
                    break;

                case ObjectInstanceType.StaticMesh:
                    _editor.Level.Objects.Remove(objectPtr.Id);
                    room.Moveables.Remove(objectPtr.Id);
                    break;

                case ObjectInstanceType.SoundSource:
                    _editor.Level.Objects.Remove(objectPtr.Id);
                    room.Moveables.Remove(objectPtr.Id);
                    break;

                case ObjectInstanceType.Sink:
                    _editor.Level.Objects.Remove(objectPtr.Id);
                    room.Moveables.Remove(objectPtr.Id);
                    break;

                case ObjectInstanceType.Camera:
                    _editor.Level.Objects.Remove(objectPtr.Id);
                    room.Moveables.Remove(objectPtr.Id);
                    break;

                case ObjectInstanceType.FlyByCamera:
                    _editor.Level.Objects.Remove(objectPtr.Id);
                    room.Moveables.Remove(objectPtr.Id);
                    break;
            }

            // Avoid having the removed object still selected
            if (_editor.SelectedObject == objectPtr)
                _editor.SelectedObject = null;

            // Update state ...
            _editor.UpdateStatusStrip();
            _editor.DrawPanel3D();
            if (_editor.SelectedObject == objectPtr)
                _editor.SelectedObject = null;
        }
        
        public static void RotateCone(Room room, ObjectPtr objectPtr, Vector2 delta)
        {
            switch (objectPtr.Type)
            {
                case ObjectInstanceType.Light:
                    {
                        Light light = room.Lights[objectPtr.Id];

                        light.DirectionX += delta.X;
                        light.DirectionY += delta.Y;

                        if (light.DirectionX >= 360)
                            light.DirectionX -= 360;
                        if (light.DirectionX < 0)
                            light.DirectionX += 360;
                        if (light.DirectionY < 0)
                            light.DirectionY += 360;
                        if (light.DirectionY >= 360)
                            light.DirectionY -= 360;
                        
                        room.BuildGeometry();
                        room.CalculateLightingForThisRoom();
                        room.UpdateBuffers();
                        _editor.UpdateStatusStrip();
                        _editor.DrawPanel3D();
                    }
                    break;

                case ObjectInstanceType.FlyByCamera:
                    {
                        FlybyCameraInstance flyby = (FlybyCameraInstance)_editor.Level.Objects[objectPtr.Id];

                        flyby.DirectionX += (short)Math.Round(delta.X);
                        flyby.DirectionY += (short)Math.Round(delta.Y);

                        if (flyby.DirectionX >= 360)
                            flyby.DirectionX -= 360;
                        if (flyby.DirectionX < 0)
                            flyby.DirectionX += 360;
                        if (flyby.DirectionY < 0)
                            flyby.DirectionY += 360;
                        if (flyby.DirectionY >= 360)
                            flyby.DirectionY -= 360;

                        _editor.DrawPanel3D();
                    }
                    break;
            }
        }

        public static void PlaceTexture(Room room, DrawingPoint pos, BlockFaces faceType)
        {
            BlockFace face = room.GetBlock(pos).Faces[(int)faceType];

            ApplyTexture(room, pos, faceType);

            face.Flipped = false;

            room.BuildGeometry(pos.X, pos.X, pos.Y, pos.Y);
            room.CalculateLightingForThisRoom();
            room.UpdateBuffers();
            _editor.DrawPanel3D();
        }

        public static void RotateTexture(Room room, DrawingPoint pos, BlockFaces faceType)
        {
            BlockFace face = room.GetBlock(pos).Faces[(int)faceType];
            if (_editor.InvisiblePolygon || face.Invisible)
                return;

            if (face.Shape == BlockFaceShape.Triangle)
            {
                Vector2 temp3 = face.TriangleUV[2];
                face.TriangleUV[2] = face.TriangleUV[1];
                face.TriangleUV[1] = face.TriangleUV[0];
                face.TriangleUV[0] = temp3;

                if (faceType == BlockFaces.FloorTriangle2)
                {
                    Vector2 temp4 = face.TriangleUV2[2];
                    face.TriangleUV2[2] = face.TriangleUV2[1];
                    face.TriangleUV2[1] = face.TriangleUV2[0];
                    face.TriangleUV2[0] = temp4;
                }

                face.Rotation += 1;
                if (face.Rotation == 3)
                    face.Rotation = 0;
            }
            else
            {
                Vector2 temp2 = face
                    .RectangleUV[3];
                face.RectangleUV[3] = face.RectangleUV[2];
                face.RectangleUV[2] = face.RectangleUV[1];
                face.RectangleUV[1] = face.RectangleUV[0];
                face.RectangleUV[0] = temp2;

                face.Rotation += 1;
                if (face.Rotation == 4)
                    face.Rotation = 0;
            }

            room.BuildGeometry();
            room.CalculateLightingForThisRoom();
            room.UpdateBuffers();
            _editor.DrawPanel3D();
        }

        public static void FlipTexture(Room room, DrawingPoint pos, BlockFaces faceType)
        {
            BlockFace face = room.GetBlock(pos).Faces[(int)faceType];

            if (_editor.InvisiblePolygon || face.Invisible || face.Texture == -1)
                return;

            if (face.Shape == BlockFaceShape.Triangle)
            {
                Vector2[] UV = new Vector2[4];

                // Calculate the new UV
                LevelTexture texture = _editor.Level.TextureSamples[face.Texture];

                UV[0] = new Vector2(texture.X / 256.0f, texture.Y / 256.0f);
                UV[1] = new Vector2((texture.X + texture.Width) / 256.0f, texture.Y / 256.0f);
                UV[2] = new Vector2((texture.X + texture.Width) / 256.0f, (texture.Y + texture.Height) / 256.0f);
                UV[3] = new Vector2(texture.X / 256.0f, (texture.Y + texture.Height) / 256.0f);

                if (_editor.SeletedTextureTriangle == TextureTileType.TriangleNW)
                {
                    face.TriangleUV[0] = UV[1];
                    face.TriangleUV[1] = UV[0];
                    face.TriangleUV[2] = UV[2];
                    if (faceType == BlockFaces.FloorTriangle2 || faceType == BlockFaces.CeilingTriangle2)
                    {
                        face.TriangleUV2[0] = UV[1];
                        face.TriangleUV2[1] = UV[0];
                        face.TriangleUV2[2] = UV[2];
                    }
                }

                if (_editor.SeletedTextureTriangle == TextureTileType.TriangleNE)
                {
                    face.TriangleUV[0] = UV[0];
                    face.TriangleUV[1] = UV[3];
                    face.TriangleUV[2] = UV[1];

                    if (faceType == BlockFaces.FloorTriangle2 || faceType == BlockFaces.CeilingTriangle2)
                    {
                        face.TriangleUV2[0] = UV[0];
                        face.TriangleUV2[1] = UV[3];
                        face.TriangleUV2[2] = UV[1];
                    }
                }

                if (_editor.SeletedTextureTriangle == TextureTileType.TriangleSE)
                {
                    face.TriangleUV[0] = UV[3];
                    face.TriangleUV[1] = UV[2];
                    face.TriangleUV[2] = UV[0];

                    if (faceType == BlockFaces.FloorTriangle2 || faceType == BlockFaces.CeilingTriangle2)
                    {
                        face.TriangleUV2[0] = UV[3];
                        face.TriangleUV2[1] = UV[2];
                        face.TriangleUV2[2] = UV[0];
                    }
                }

                if (_editor.SeletedTextureTriangle == TextureTileType.TriangleSW)
                {
                    face.TriangleUV[0] = UV[2];
                    face.TriangleUV[1] = UV[1];
                    face.TriangleUV[2] = UV[3];

                    if (faceType == BlockFaces.FloorTriangle2 || faceType == BlockFaces.CeilingTriangle2)
                    {
                        face.TriangleUV2[0] = UV[2];
                        face.TriangleUV2[1] = UV[1];
                        face.TriangleUV2[2] = UV[3];
                    }
                }

                for (int k = 0; k < face.Rotation; k++)
                {
                    Vector2 temp3 = face
                        .TriangleUV[2];
                    face.TriangleUV[2] = face.TriangleUV[1];
                    face.TriangleUV[1] = face.TriangleUV[0];
                    face.TriangleUV[0] = temp3;

                    if (faceType == BlockFaces.FloorTriangle2)
                    {
                        Vector2 temp4 = face.TriangleUV2[2];
                        face.TriangleUV2[2] = face.TriangleUV2[1];
                        face.TriangleUV2[1] = face.TriangleUV2[0];
                        face.TriangleUV2[0] = temp4;
                    }
                }
            }
            else
            {
                Vector2 temp2 = face.RectangleUV[1];
                face.RectangleUV[1] = face.RectangleUV[0];
                face.RectangleUV[0] = temp2;

                temp2 = face.RectangleUV[3];
                face.RectangleUV[3] = face.RectangleUV[2];
                face.RectangleUV[2] = temp2;
            }

            face.Flipped = !face.Flipped;

            room.BuildGeometry();
            room.CalculateLightingForThisRoom();
            room.UpdateBuffers();
            _editor.DrawPanel3D();
        }

        public static void PlaceNoCollision(Room room, DrawingPoint pos, BlockFaces faceType)
        {
            if (faceType == BlockFaces.Floor || faceType == BlockFaces.FloorTriangle2)
                room.GetBlock(pos).NoCollisionFloor = !room.GetBlock(pos).NoCollisionFloor;

            if (faceType == BlockFaces.Ceiling || faceType == BlockFaces.CeilingTriangle2)
                room.GetBlock(pos).NoCollisionCeiling = !room.GetBlock(pos).NoCollisionCeiling;

            room.BuildGeometry();
            room.CalculateLightingForThisRoom();
            room.UpdateBuffers();
            _editor.DrawPanel3D();
        }

        public static void ApplyTexture(Room room, DrawingPoint pos, BlockFaces faceType)
        {
            if (_editor == null || (_editor.SelectedTextureIndex == -1 && !_editor.InvisiblePolygon))
                return;

            BlockFace face = room.GetBlock(pos).Faces[(int)faceType];

            if (_editor.InvisiblePolygon)
            {
                room.GetBlock(pos).Faces[(int)faceType].Invisible = true;
                room.GetBlock(pos).Faces[(int)faceType].Transparent = false;
                room.GetBlock(pos).Faces[(int)faceType].DoubleSided = false;

                int tid = room.GetBlock(pos).Faces[(int)faceType].Texture;

                room.GetBlock(pos).Faces[(int)faceType].Texture = -1;
            }
            else
            {
                // if face was invisible, then reset flag
                if (face.Invisible)
                    room.GetBlock(pos).Faces[(int)faceType].Invisible = false;

                // set trasparency of this face
                if (_editor.Transparent)
                    room.GetBlock(pos).Faces[(int)faceType].Transparent = true;
                else
                    room.GetBlock(pos).Faces[(int)faceType].Transparent = false;

                // set double sided flag of this face
                if (_editor.DoubleSided)
                    room.GetBlock(pos).Faces[(int)faceType].DoubleSided = true;
                else
                    room.GetBlock(pos).Faces[(int)faceType].DoubleSided = false;

                Vector2[] UV = new Vector2[4];

                LevelTexture texture = _editor.Level.TextureSamples[_editor.SelectedTextureIndex];

                int yBlock = (int)(texture.Page / 8);
                int xBlock = (int)(texture.Page % 8);

                UV[0] = new Vector2((xBlock * 256.0f + texture.X + 0.5f) / 2048.0f, (yBlock * 256.0f + texture.Y + 0.5f) / 2048.0f);
                UV[1] = new Vector2((xBlock * 256.0f + texture.X + texture.Width - 0.5f) / 2048.0f, (yBlock * 256.0f + texture.Y + 0.5f) / 2048.0f);
                UV[2] = new Vector2((xBlock * 256.0f + texture.X + texture.Width - 0.5f) / 2048.0f, (yBlock * 256.0f + texture.Y + texture.Height - 0.5f) / 2048.0f);
                UV[3] = new Vector2((xBlock * 256.0f + texture.X + 0.5f) / 2048.0f, (yBlock * 256.0f + texture.Y + texture.Height - 0.5f) / 2048.0f);

                room.GetBlock(pos).Faces[(int)faceType].RectangleUV[0] = UV[0];
                room.GetBlock(pos).Faces[(int)faceType].RectangleUV[1] = UV[1];
                room.GetBlock(pos).Faces[(int)faceType].RectangleUV[2] = UV[2];
                room.GetBlock(pos).Faces[(int)faceType].RectangleUV[3] = UV[3];

                /*
                *  1----2    Split 0: 231 413  
                *  | \  |    Split 1: 124 342
                *  |  \ |
                *  4----3
                */

                if (face.Shape == BlockFaceShape.Triangle)
                {
                    if (_editor.SeletedTextureTriangle == TextureTileType.TriangleNW)
                    {
                        room.GetBlock(pos).Faces[(int)faceType].TriangleUV[0] = UV[0];
                        room.GetBlock(pos).Faces[(int)faceType].TriangleUV[1] = UV[1];
                        room.GetBlock(pos).Faces[(int)faceType].TriangleUV[2] = UV[3];

                        if (faceType == BlockFaces.FloorTriangle2 || faceType == BlockFaces.CeilingTriangle2)
                        {
                            room.GetBlock(pos).Faces[(int)faceType].TriangleUV2[0] = UV[0];
                            room.GetBlock(pos).Faces[(int)faceType].TriangleUV2[1] = UV[1];
                            room.GetBlock(pos).Faces[(int)faceType].TriangleUV2[2] = UV[3];
                        }
                    }

                    if (_editor.SeletedTextureTriangle == TextureTileType.TriangleNE)
                    {
                        room.GetBlock(pos).Faces[(int)faceType].TriangleUV[0] = UV[1];
                        room.GetBlock(pos).Faces[(int)faceType].TriangleUV[1] = UV[2];
                        room.GetBlock(pos).Faces[(int)faceType].TriangleUV[2] = UV[0];

                        if (faceType == BlockFaces.FloorTriangle2 || faceType == BlockFaces.CeilingTriangle2)
                        {
                            room.GetBlock(pos).Faces[(int)faceType].TriangleUV2[0] = UV[1];
                            room.GetBlock(pos).Faces[(int)faceType].TriangleUV2[1] = UV[2];
                            room.GetBlock(pos).Faces[(int)faceType].TriangleUV2[2] = UV[0];
                        }
                    }

                    if (_editor.SeletedTextureTriangle == TextureTileType.TriangleSE)
                    {
                        room.GetBlock(pos).Faces[(int)faceType].TriangleUV[0] = UV[2];
                        room.GetBlock(pos).Faces[(int)faceType].TriangleUV[1] = UV[3];
                        room.GetBlock(pos).Faces[(int)faceType].TriangleUV[2] = UV[1];

                        if (faceType == BlockFaces.FloorTriangle2 || faceType == BlockFaces.CeilingTriangle2)
                        {
                            room.GetBlock(pos).Faces[(int)faceType].TriangleUV2[0] = UV[2];
                            room.GetBlock(pos).Faces[(int)faceType].TriangleUV2[1] = UV[3];
                            room.GetBlock(pos).Faces[(int)faceType].TriangleUV2[2] = UV[1];
                        }
                    }

                    if (_editor.SeletedTextureTriangle == TextureTileType.TriangleSW)
                    {
                        room.GetBlock(pos).Faces[(int)faceType].TriangleUV[0] = UV[3];
                        room.GetBlock(pos).Faces[(int)faceType].TriangleUV[1] = UV[0];
                        room.GetBlock(pos).Faces[(int)faceType].TriangleUV[2] = UV[2];

                        if (faceType == BlockFaces.FloorTriangle2 || faceType == BlockFaces.CeilingTriangle2)
                        {
                            room.GetBlock(pos).Faces[(int)faceType].TriangleUV2[0] = UV[3];
                            room.GetBlock(pos).Faces[(int)faceType].TriangleUV2[1] = UV[0];
                            room.GetBlock(pos).Faces[(int)faceType].TriangleUV2[2] = UV[2];
                        }
                    }
                }

                if (face.Shape == BlockFaceShape.Triangle)
                {
                    room.GetBlock(pos).Faces[(int)faceType].TextureTriangle = _editor.SeletedTextureTriangle;
                }

                room.GetBlock(pos).Faces[(int)faceType].Texture = (short)_editor.SelectedTextureIndex;
                room.GetBlock(pos).Faces[(int)faceType].Rotation = 0;
            }
        }

        public static void PlaceObject(Room room, DrawingPoint pos, ObjectInstanceType type)
        {
            switch (type)
            {
                case ObjectInstanceType.Light:
                    {
                        Light instance = new Light();

                        Block block = room.GetBlock(pos);
                        int y = (block.QAFaces[0] + block.QAFaces[1] + block.QAFaces[2] + block.QAFaces[3]) / 4;

                        instance.Position = new Vector3(pos.X * 1024 + 512, y * 256 + 128.0f, pos.Y * 1024 + 512);
                        instance.Color = System.Drawing.Color.White;
                        instance.Active = true;
                        instance.Intensity = 0.5f;
                        instance.In = 1.0f;
                        instance.Out = 5.0f;
                        instance.Type = _editor.ActionPlaceLight_LightType;

                        switch (_editor.ActionPlaceLight_LightType)
                        {
                            case LightType.Shadow:
                                instance.Intensity *= -1;
                                break;
                            case LightType.Spot:
                                instance.Len = 2.0f;
                                instance.Cutoff = 3.0f;
                                instance.DirectionX = 0.0f;
                                instance.DirectionY = 0.0f;
                                instance.In = 20.0f;
                                instance.Out = 25.0f;
                                break;
                            case LightType.Sun:
                                instance.DirectionX = 0.0f;
                                instance.DirectionY = 0.0f;
                                break;
                            case LightType.Effect:
                                instance.In = 0.0f;
                                instance.Out = 1024.0f;
                                break;
                        }
                        room.Lights.Add(instance);

                        room.BuildGeometry();
                        room.CalculateLightingForThisRoom();
                        room.UpdateBuffers();
                    }
                    break;
                case ObjectInstanceType.Moveable:
                    if (!_editor.ActionPlaceItem_Item.IsStatic)
                    {
                        MoveableInstance instance = new MoveableInstance(_editor.Level.GetNewObjectId(), room);

                        Block block = room.GetBlock(pos);
                        int y = (block.QAFaces[0] + block.QAFaces[1] + block.QAFaces[2] + block.QAFaces[3]) / 4;

                        instance.Position = new Vector3(pos.X * 1024 + 512, y * 256, pos.Y * 1024 + 512);
                        instance.Model = _editor.Level.Wad.Moveables[(uint)(_editor.ActionPlaceItem_Item.Id)];

                        _editor.Level.Objects.Add(instance.Id, instance);
                        room.Moveables.Add(instance.Id);
                    }
                    break;
                case ObjectInstanceType.StaticMesh:
                    if (_editor.ActionPlaceItem_Item.IsStatic)
                    {
                        StaticMeshInstance instance = new StaticMeshInstance(_editor.Level.GetNewObjectId(), room);

                        Block block = room.GetBlock(pos);
                        int y = (block.QAFaces[0] + block.QAFaces[1] + block.QAFaces[2] + block.QAFaces[3]) / 4;

                        instance.Position = new Vector3(pos.X * 1024 + 512, y * 256, pos.Y * 1024 + 512);
                        instance.Model = _editor.Level.Wad.StaticMeshes[(uint)(_editor.ActionPlaceItem_Item.Id)];
                        instance.Color = System.Drawing.Color.FromArgb(255, 128, 128, 128);

                        _editor.Level.Objects.Add(instance.Id, instance);
                        room.StaticMeshes.Add(instance.Id);
                    }
                    break;
                case ObjectInstanceType.Camera:
                  {   
                        CameraInstance instance = new CameraInstance(_editor.Level.GetNewObjectId(), room);

                        Block block = room.GetBlock(pos);
                        int y = (block.QAFaces[0] + block.QAFaces[1] + block.QAFaces[2] + block.QAFaces[3]) / 4;

                        instance.Position = new Vector3(pos.X * 1024 + 512, y * 256, pos.Y * 1024 + 512);

                        _editor.Action = EditorAction.None;
                        _editor.Level.Objects.Add(instance.Id, instance);
                        room.Cameras.Add(instance.Id);
                    }
                    break;
                case ObjectInstanceType.FlyByCamera:
                    {
                        FlybyCameraInstance instance = new FlybyCameraInstance(_editor.Level.GetNewObjectId(), room);

                        Block block = room.GetBlock(pos);
                        int y = (block.QAFaces[0] + block.QAFaces[1] + block.QAFaces[2] + block.QAFaces[3]) / 4;

                        instance.Position = new Vector3(pos.X * 1024 + 512, y * 256, pos.Y * 1024 + 512);

                        _editor.Action = EditorAction.None;
                        _editor.Level.Objects.Add(instance.Id, instance);
                        room.FlyByCameras.Add(instance.Id);
                    }
                    break;
                case ObjectInstanceType.SoundSource:
                    {
                        SoundSourceInstance instance = new SoundSourceInstance(_editor.Level.GetNewObjectId(), room);

                        Block block = room.GetBlock(pos);
                        int y = (block.QAFaces[0] + block.QAFaces[1] + block.QAFaces[2] + block.QAFaces[3]) / 4;

                        instance.Position = new Vector3(pos.X * 1024 + 512, y * 256, pos.Y * 1024 + 512);

                        _editor.Action = EditorAction.None;
                        _editor.Level.Objects.Add(instance.Id, instance);
                        room.SoundSources.Add(instance.Id);
                    }
                    break;
                case ObjectInstanceType.Sink:
                    {
                        SinkInstance instance = new SinkInstance(_editor.Level.GetNewObjectId(), room);

                        Block block = room.GetBlock(pos);
                        int y = (block.QAFaces[0] + block.QAFaces[1] + block.QAFaces[2] + block.QAFaces[3]) / 4;

                        instance.Position = new Vector3(pos.X * 1024 + 512, y * 256, pos.Y * 1024 + 512);

                        _editor.Action = EditorAction.None;
                        _editor.Level.Objects.Add(instance.Id, instance);
                        room.Sinks.Add(instance.Id);
                    }
                    break;
            }
            _editor.UpdateStatusStrip();
            _editor.DrawPanel3D();
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

            var newRoom = new Room(_editor.Level, numXSectors, numZSectors, "Unnamed");
            newRoom.Position = new Vector3(newX, room.Position.Y, newZ);

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
                _editor.SelectedSector = new Rectangle(1, 1, numXSectors - 2, numZSectors - 2);
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
                    var currentRoomPortal = new Portal(room);
                    var otherRoomPortal = new Portal(found);

                    int xPortalOther = (int)(xPortalWorld - found.Position.X);
                    int zPortalOther = (int)(zPortalWorld - found.Position.Z);

                    currentRoomPortal.NumXBlocks = 1;
                    currentRoomPortal.NumZBlocks = (byte)(area.Bottom - area.Y + 1);
                    currentRoomPortal.Direction = PortalDirection.West;
                    currentRoomPortal.X = 0;
                    currentRoomPortal.Z = (byte)area.Y;
                    currentRoomPortal.AdjoiningRoom = found;
                    currentRoomPortal.MemberOfFlippedRoom = room.Flipped;

                    otherRoomPortal.NumXBlocks = 1;
                    otherRoomPortal.NumZBlocks = (byte)(area.Bottom - area.Y + 1);
                    otherRoomPortal.Direction = PortalDirection.East;
                    otherRoomPortal.X = (byte)(found.NumXSectors - 1);
                    otherRoomPortal.Z = (byte)zPortalOther;
                    otherRoomPortal.AdjoiningRoom = room;
                    otherRoomPortal.MemberOfFlippedRoom = found.Flipped;

                    currentRoomPortal.Other = otherRoomPortal;
                    otherRoomPortal.Other = currentRoomPortal;

                    // Set the portal ID in sectors
                    for (int z = area.Y; z <= area.Bottom; z++)
                    {
                        room.Blocks[0, z].WallPortal = currentRoomPortal;
                        found.Blocks[found.NumXSectors - 1, z + (int)(room.Position.Z - found.Position.Z)].WallPortal = otherRoomPortal;
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
                    var currentRoomPortal = new Portal(room);
                    var otherRoomPortal = new Portal(found);

                    int xPortalOther = (int)(xPortalWorld - found.Position.X);
                    int zPortalOther = (int)(zPortalWorld - found.Position.Z);

                    currentRoomPortal.NumXBlocks = 1;
                    currentRoomPortal.NumZBlocks = (byte)(area.Bottom - area.Y + 1);
                    currentRoomPortal.Direction = PortalDirection.East;
                    currentRoomPortal.X = (byte)(numXblocks - 1);
                    currentRoomPortal.Z = (byte)area.Y;
                    currentRoomPortal.AdjoiningRoom = found;
                    currentRoomPortal.MemberOfFlippedRoom = room.Flipped;

                    otherRoomPortal.NumXBlocks = 1;
                    otherRoomPortal.NumZBlocks = (byte)(area.Bottom - area.Y + 1);
                    otherRoomPortal.Direction = PortalDirection.West;
                    otherRoomPortal.X = 0;
                    otherRoomPortal.Z = (byte)zPortalOther;
                    otherRoomPortal.AdjoiningRoom = room;
                    otherRoomPortal.MemberOfFlippedRoom = found.Flipped;

                    currentRoomPortal.Other = otherRoomPortal;
                    otherRoomPortal.Other = currentRoomPortal;

                    // Set the portal ID in sectors
                    for (int z = area.Y; z <= area.Bottom; z++)
                    {
                        room.Blocks[numXblocks - 1, z].WallPortal = currentRoomPortal;
                        found.Blocks[0, z + (int)(room.Position.Z - found.Position.Z)].WallPortal = otherRoomPortal;
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
                    var currentRoomPortal = new Portal(room);
                    var otherRoomPortal = new Portal(found);

                    byte xPortalOther = (byte)(xPortalWorld - found.Position.X);
                    byte zPortalOther = (byte)(zPortalWorld - found.Position.Z);

                    currentRoomPortal.NumXBlocks = (byte)(area.Right - area.X + 1);
                    currentRoomPortal.NumZBlocks = 1;
                    currentRoomPortal.Direction = PortalDirection.North;
                    currentRoomPortal.X = (byte)area.X;
                    currentRoomPortal.Z = (byte)(numZblocks - 1);
                    currentRoomPortal.AdjoiningRoom = found;
                    currentRoomPortal.MemberOfFlippedRoom = room.Flipped;

                    otherRoomPortal.NumXBlocks = (byte)(area.Right - area.X + 1);
                    otherRoomPortal.NumZBlocks = 1;
                    otherRoomPortal.Direction = PortalDirection.South;
                    otherRoomPortal.X = xPortalOther;
                    otherRoomPortal.Z = 0;
                    otherRoomPortal.AdjoiningRoom = room;
                    otherRoomPortal.MemberOfFlippedRoom = found.Flipped;

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
                    var currentRoomPortal = new Portal(room);
                    var otherRoomPortal = new Portal(found);

                    int xPortalOther = (int)(xPortalWorld - found.Position.X);
                    int zPortalOther = (int)(zPortalWorld - found.Position.Z);

                    currentRoomPortal.NumXBlocks = (byte)(area.Right - area.X + 1);
                    currentRoomPortal.NumZBlocks = 1;
                    currentRoomPortal.Direction = PortalDirection.South;
                    currentRoomPortal.X = (byte)area.X;
                    currentRoomPortal.Z = 0;
                    currentRoomPortal.AdjoiningRoom = found;
                    currentRoomPortal.MemberOfFlippedRoom = room.Flipped;

                    otherRoomPortal.NumXBlocks = (byte)(area.Right - area.X + 1);
                    otherRoomPortal.NumZBlocks = 1;
                    otherRoomPortal.Direction = PortalDirection.North;
                    otherRoomPortal.X = (byte)xPortalOther;
                    otherRoomPortal.Z = (byte)(found.NumZSectors - 1);
                    otherRoomPortal.AdjoiningRoom = room;
                    otherRoomPortal.MemberOfFlippedRoom = found.Flipped;

                    currentRoomPortal.Other = otherRoomPortal;
                    otherRoomPortal.Other = currentRoomPortal;

                    // Set portal ID in sectors
                    for (int x = area.X; x <= area.Right; x++)
                    {
                        room.Blocks[x, 0].WallPortal = currentRoomPortal;
                        found.Blocks[x + xPortalOther - area.X, found.NumZSectors - 1].WallPortal = otherRoomPortal;
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
                            int h1 = otherRoom.Blocks[x, z].WSFaces[0];
                            int h2 = otherRoom.Blocks[x, z].WSFaces[1];
                            int h3 = otherRoom.Blocks[x, z].WSFaces[2];
                            int h4 = otherRoom.Blocks[x, z].WSFaces[3];

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

                            int lh1 = otherRoom.Blocks[lowX, lowZ].WSFaces[0];
                            int lh2 = otherRoom.Blocks[lowX, lowZ].WSFaces[1];
                            int lh3 = otherRoom.Blocks[lowX, lowZ].WSFaces[2];
                            int lh4 = otherRoom.Blocks[lowX, lowZ].WSFaces[3];

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

                    var currentRoomPortal = new Portal(room);
                    var otherRoomPortal = new Portal(found);

                    currentRoomPortal.NumXBlocks = (byte)(area.Right - area.X + 1);
                    currentRoomPortal.NumZBlocks = (byte)(area.Bottom - area.Y + 1);
                    currentRoomPortal.Direction = PortalDirection.Floor;
                    currentRoomPortal.X = (byte)area.X;
                    currentRoomPortal.Z = (byte)area.Y;
                    currentRoomPortal.AdjoiningRoom = found;
                    currentRoomPortal.MemberOfFlippedRoom = room.Flipped;

                    int lowXmin = area.X + (int)(room.Position.X - otherRoom.Position.X);
                    int lowXmax = area.Right + (int)(room.Position.X - otherRoom.Position.X);
                    int lowZmin = area.Y + (int)(room.Position.Z - otherRoom.Position.Z);
                    int lowZmax = area.Bottom + (int)(room.Position.Z - otherRoom.Position.Z);

                    otherRoomPortal.NumXBlocks = (byte)(lowXmax - lowXmin + 1);
                    otherRoomPortal.NumZBlocks = (byte)(lowZmax - lowZmin + 1);
                    otherRoomPortal.Direction = PortalDirection.Ceiling;
                    otherRoomPortal.X = (byte)lowXmin;
                    otherRoomPortal.Z = (byte)lowZmin;
                    otherRoomPortal.AdjoiningRoom = room;
                    otherRoomPortal.MemberOfFlippedRoom = otherRoom.Flipped;

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
                                block.WSFaces[i] = (short)Math.Round((verticalArea.FloorY * 1.0f + verticalArea.CeilingY * 2.0f) / 3.0f);
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
                                block.WSFaces[i] = (short)Math.Round((verticalArea.FloorY * 2.0f + verticalArea.CeilingY * 3.0f) / 5.0f);
                                block.RFFaces[i] = (short)Math.Round((verticalArea.FloorY * 1.0f + verticalArea.CeilingY * 4.0f) / 5.0f);
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
    }
}
