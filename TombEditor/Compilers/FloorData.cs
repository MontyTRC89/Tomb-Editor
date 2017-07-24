using System;
using System.Collections.Generic;
using SharpDX;
using TombEditor.Geometry;

namespace TombEditor.Compilers
{
    public partial class LevelCompilerTR4
    {
        private void BuildFloorData()
        {
            ReportProgress(70, "Building floordata");

            // Initialize the floordata list and add the dummy entry for walls and sectors without particular things
            List<ushort> tempFloorData = new List<ushort>();
            tempFloorData.Add(0 | 0x8000);

            for (int i = 0; i < _editor.Level.Rooms.Length; i++)
            {
                Room room = _editor.Level.Rooms[i];
                if (room == null) continue;
                int idNewRoom = _roomsIdTable[i];

                // Get all portals
                List<Portal> portals = new List<Portal>();
                for (int z = 0; z < room.NumZSectors; z++)
                {
                    for (int x = 0; x < room.NumXSectors; x++)
                    {
                        if (room.Blocks[x, z].CeilingPortal != -1 && room.Blocks[x, z].CeilingOpacity != PortalOpacity.Opacity1)
                        {
                            portals.Add(_level.Portals[room.Blocks[x, z].CeilingPortal]);
                        }
                    }
                }

                if (i==110)
                {
                    int ghghg = 0;
                }

                for (int z = 0; z < room.NumZSectors; z++)
                {
                    for (int x = 0; x < room.NumXSectors; x++)
                    {
                        tr_room_sector sector = GetSector(idNewRoom, x, z);
                        Block block = room.Blocks[x, z];

                        if (i==0 && x == 3 && z ==12)
                        {
                            int ggg = 0;
                        }

                        ushort baseFloorData = (ushort)tempFloorData.Count;

                        // If a sector is a wall and this room is a water room, 
                        // I must check before if on the neighbour sector there's a ceiling portal 
                        // because eventually I must add a vertical portal
                        int isWallWithCeilingPortal = -1;
                        if (portals.Count != 0)
                        {
                            // Find a suitable portal
                            for (int j = 0; j < portals.Count; j++)
                            {
                                if (x >= portals[j].X - 1 && x <= portals[j].X + portals[j].NumXBlocks + 1 &&
                                    z >= portals[j].Z - 1 && z <= portals[j].Z + portals[j].NumZBlocks + 1)
                                {
                                    Room adjoining = _level.Rooms[portals[j].AdjoiningRoom];
                                    int x2 = (int)(room.Position.X + x - adjoining.Position.X);
                                    int z2 = (int)(room.Position.Z + z - adjoining.Position.Z);

                                    if (x2 < 0 || x2 > adjoining.NumXSectors - 1 || z2 < 0 || z2 > adjoining.NumZSectors - 1) continue;

                                    BlockType blockType = adjoining.Blocks[x2, z2].Type;
                                    DiagonalSplit adjoiningSplit = adjoining.Blocks[x2, z2].FloorDiagonalSplit;
                                    DiagonalSplit currentSplit = block.FloorDiagonalSplit;

                                    if ((x2 > 1 || z2 > 1 || x2 < adjoining.NumXSectors - 1 || z2 < adjoining.NumZSectors - 1) &&
                                        !((blockType == BlockType.Wall && adjoiningSplit == DiagonalSplit.None)
                                        || blockType == BlockType.BorderWall))
                                    {
                                        isWallWithCeilingPortal = _roomsIdTable[portals[j].AdjoiningRoom];
                                        break;
                                    }
                                }
                            }
                        }

                        // If sector is a wall with a ceiling portal on it or near it
                        if (block.WallPortal == -1 && isWallWithCeilingPortal != -1 && ((block.Type == BlockType.Wall && block.FloorDiagonalSplit == DiagonalSplit.None) || room.Blocks[x, z].Type == BlockType.BorderWall))
                        {
                            ushort data1 = 0x8001;
                            ushort data2 = (ushort)isWallWithCeilingPortal;

                            tempFloorData.Add(data1);
                            tempFloorData.Add(data2);

                            // Update current sector
                            sector.FloorDataIndex = baseFloorData;

                            SaveSector(idNewRoom, x, z, sector);

                            continue;
                        }

                        // If sector is a border wall without portals or a normal wall
                        if ((block.Type == BlockType.Wall && block.FloorDiagonalSplit == DiagonalSplit.None) || (block.Type == BlockType.BorderWall && block.WallPortal == -1))
                        {
                            sector.FloorDataIndex = 0;
                            sector.Floor = -127;
                            sector.Ceiling = -127;

                            SaveSector(idNewRoom, x, z, sector);
                            continue;
                        }

                        // If sector is a floor portal
                        if (block.FloorPortal >= 0)
                        {
                            // I must setup portal only if current sector is not solid and opacity if different from 1
                            if ((!block.IsFloorSolid && block.FloorOpacity != PortalOpacity.Opacity1) || (block.IsFloorSolid && block.NoCollisionFloor))
                            {
                                Portal portal = _editor.Level.Portals[block.FloorPortal];
                                sector.RoomBelow = (byte)_roomsIdTable[portal.AdjoiningRoom];
                            }
                            else
                            {
                                sector.RoomBelow = 255;
                            }
                        }

                        // If sector is a ceiling portal
                        if (block.CeilingPortal >= 0)
                        {
                            // I must setup portal only if current sector is not solid and opacity if different from 1
                            if ((!block.IsCeilingSolid && block.CeilingOpacity != PortalOpacity.Opacity1) || (block.IsCeilingSolid && block.NoCollisionCeiling))
                            {
                                Portal portal = _editor.Level.Portals[block.CeilingPortal];
                                sector.RoomAbove = (byte)_roomsIdTable[portal.AdjoiningRoom];
                            }
                            else
                            {
                                sector.RoomAbove = 255;
                            }
                        }

                        // If sector is a wall portal
                        if (block.WallPortal >= 0)
                        {
                            Portal portal = _editor.Level.Portals[block.WallPortal];

                            // Only if the portal is not a Toggle Opacity 1
                            if (block.WallOpacity != PortalOpacity.Opacity1)
                            {
                                ushort data1 = 0x8001;
                                ushort data2 = (ushort)_roomsIdTable[portal.AdjoiningRoom];

                                tempFloorData.Add(data1);
                                tempFloorData.Add(data2);

                                // Update current sector
                                sector.FloorDataIndex = baseFloorData;
                                SaveSector(idNewRoom, x, z, sector);
                            }
                            else
                            {
                                sector.Floor = -127;
                                sector.Ceiling = -127;
                                SaveSector(idNewRoom, x, z, sector);
                            }

                            continue;
                        }

                        // From this point, I will never bypass the loop and something must be there so I surely add at least one
                        // floordata value
                        sector.FloorDataIndex = baseFloorData;
                        List<ushort> tempCodes = new List<ushort>();
                        ushort lastFloorDataFunction = (ushort)tempCodes.Count;

                        // If sector is Death
                        if ((block.Flags & BlockFlags.Death) == BlockFlags.Death)
                        {
                            lastFloorDataFunction = (ushort)tempCodes.Count;
                            tempCodes.Add(0x05);
                        }

                        // If sector has a floor slope
                        if (block.FloorSlopeX != 0 || block.FloorSlopeZ != 0)
                        {
                            lastFloorDataFunction = (ushort)tempCodes.Count;
                            tempCodes.Add(0x02);

                            sector.Floor = (sbyte)(-room.Position.Y - room.GetHighestFloorCorner(x, z));

                            ushort slope = (ushort)(((block.FloorSlopeZ) << 8) | ((block.FloorSlopeX) & 0xff));

                            tempCodes.Add(slope);
                        }

                        // If sector has a ceiling slope
                        if (block.CeilingSlopeX != 0 || block.CeilingSlopeZ != 0)
                        {
                            lastFloorDataFunction = (ushort)tempCodes.Count;
                            tempCodes.Add(0x03);

                            ushort slope = (ushort)(((block.CeilingSlopeZ) << 8) | ((block.CeilingSlopeX) & 0xff));

                            tempCodes.Add(slope);
                        }

                        // Now begins the triangulation for floor and ceiling
                        // It's a very long and hard task
                        if (block.FloorDiagonalSplit != DiagonalSplit.None)
                        {
                            int q0 = block.QAFaces[0];
                            int q1 = block.QAFaces[1];
                            int q2 = block.QAFaces[2];
                            int q3 = block.QAFaces[3];

                            int w0 = block.WSFaces[0];
                            int w1 = block.WSFaces[1];
                            int w2 = block.WSFaces[2];
                            int w3 = block.WSFaces[3];

                            // The real floor split of the sector
                            int split = block.RealSplitFloor;
                            int function = 0;

                            int t1 = 0;
                            int t2 = 0;

                            int h00 = 0;
                            int h01 = 0;
                            int h10 = 0;
                            int h11 = 0;

                            // First, we fix the sector height
                            if (block.Type == BlockType.Wall)
                                sector.Floor = (sbyte)(Rooms[idNewRoom].Info.YBottom / 256.0f - 0x0f);
                            else
                                sector.Floor = (sbyte)(Rooms[idNewRoom].Info.YBottom / 256.0f - room.GetHighestFloorCorner(x, z));

                            if (block.FloorDiagonalSplit == DiagonalSplit.NE || block.FloorDiagonalSplit == DiagonalSplit.SW)
                            {
                                lastFloorDataFunction = (ushort)tempCodes.Count;

                                if (block.FloorPortal >= 0 && block.NoCollisionFloor)
                                {
                                    if (block.FloorDiagonalSplit == DiagonalSplit.NE)
                                    {
                                        function = 0x0c;
                                    }
                                    else
                                    {
                                        function = 0x0b;
                                    }
                                }
                                else
                                {
                                    function = 0x07;
                                }

                                // Diagonal steps and walls are the simplest case. All corner heights are zero 
                                // except eventually the right angle on the top face. Corrections t1 and t2 
                                // are simple to calculate
                                if (block.FloorDiagonalSplit == DiagonalSplit.NE)
                                {
                                    int lowCorner = q1;
                                    int highCorner = q3;

                                    if (block.Type == BlockType.Wall)
                                    {
                                        t1 = 0;
                                        t2 = 15 - block.QAFaces[1]; // Diagonal wall max height minus the height of the lower right angle

                                        h00 = 0;
                                        h10 = 0;
                                        h01 = 0;
                                        h11 = 0;
                                    }
                                    else
                                    {
                                        t1 = (q2 > q3 ? q3 - q2 : 0);
                                        t2 = (int)(highCorner - lowCorner) & 0x1f;

                                        h00 = Math.Abs(q3 - q2);
                                        h10 = 0;
                                        h01 = 0;
                                        h11 = 0;
                                    }
                                }
                                else
                                {
                                    int lowCorner = q3;
                                    int highCorner = q1;

                                    if (block.Type == BlockType.Wall)
                                    {
                                        t1 = 15 - block.QAFaces[3];
                                        t2 = 0;

                                        h00 = 0;
                                        h10 = 0;
                                        h01 = 0;
                                        h11 = 0;
                                    }
                                    else
                                    {
                                        t1 = (int)(highCorner - lowCorner) & 0x1f;
                                        t2 = 0;

                                        h00 = 0;
                                        h10 = 0;
                                        h01 = 0;
                                        h11 = q1 - q2;
                                    }
                                }
                            }
                            else
                            {
                                lastFloorDataFunction = (ushort)tempCodes.Count;

                                if (block.FloorPortal >= 0 && block.NoCollisionFloor)
                                {
                                    if (block.FloorDiagonalSplit == DiagonalSplit.NW)
                                    {
                                        function = 0x0d;
                                    }
                                    else
                                    {
                                        function = 0x0e;
                                    }
                                }
                                else
                                {
                                    function = 0x08;
                                }

                                if (block.FloorDiagonalSplit == DiagonalSplit.NW)
                                {
                                    int lowCorner = q0;
                                    int highCorner = q2;

                                    if (block.Type == BlockType.Wall)
                                    {
                                        t1 = 15 - block.QAFaces[0];
                                        t2 = 0;

                                        h00 = 0;
                                        h10 = 0;
                                        h01 = 0;
                                        h11 = 0;
                                    }
                                    else
                                    {
                                        t1 = (int)(highCorner - lowCorner) & 0x1f;
                                        t2 = 0;

                                        h00 = 0;
                                        h10 = q2 - q1;
                                        h01 = 0;
                                        h11 = 0;
                                    }
                                }
                                else
                                {
                                    int lowCorner = q2;
                                    int highCorner = q0;

                                    if (block.Type == BlockType.Wall)
                                    {
                                        t1 = 0;
                                        t2 = 15 - block.QAFaces[2];

                                        h00 = 0;
                                        h10 = 0;
                                        h01 = 0;
                                        h11 = 0;
                                    }
                                    else
                                    {
                                        t1 = 0;
                                        t2 = (int)(highCorner - lowCorner) & 0x1f;

                                        h00 = 0;
                                        h10 = 0;
                                        h01 = q0 - q1;
                                        h11 = 0;
                                    }
                                }
                            }

                            ushort code1 = (ushort)(function | (t2 << 5) | (t1 << 10));
                            ushort code2 = (ushort)((h10) | (h00 << 4) | (h01 << 8) | (h11 << 12));

                            tempCodes.Add(code1);
                            tempCodes.Add(code2);
                        }
                        else
                        {
                            if (block.FloorSlopeX == 0 && block.FloorSlopeZ == 0)
                            {
                                int q0 = block.QAFaces[0];
                                int q1 = block.QAFaces[1];
                                int q2 = block.QAFaces[2];
                                int q3 = block.QAFaces[3];

                                // We have not a slope, so if this is not a horizontal square then we have triangulation
                                if (!Room.IsQuad(x, z, q0, q1, q2, q3, true))
                                {
                                    // First, we fix the sector height
                                    sector.Floor = (sbyte)(Rooms[idNewRoom].Info.YBottom / 256.0f - room.GetHighestFloorCorner(x, z));

                                    // Then we have to find the axis of the triangulation
                                    int min = room.GetLowestFloorCorner(x, z);
                                    int max = room.GetHighestFloorCorner(x, z);

                                    lastFloorDataFunction = (ushort)tempCodes.Count;

                                    // Corner heights
                                    int h10 = q2 - min;
                                    int h00 = q3 - min;
                                    int h01 = q0 - min;
                                    int h11 = q1 - min;

                                    int maxCorner = Math.Max(h00, Math.Max(h01, Math.Max(h10, h11)));

                                    // Flip the Y axis
                                    min = -min;
                                    max = -max;

                                    int t1 = 0;
                                    int t2 = 0;

                                    // The real floor split of the sector
                                    int split = block.RealSplitFloor;
                                    int function = 0;

                                    if (split == 0)
                                    {
                                        if (block.FloorPortal >= 0 && block.NoCollisionFloor)
                                        {
                                            if (q0 == q1 && q1 == q2 && q2 == q0)
                                            {
                                                function = 0x0c;
                                            }
                                            else
                                            {
                                                function = 0x0b;
                                            }
                                        }
                                        else
                                        {
                                            function = 0x07;
                                        }

                                        // Prepare four vectors that are vertices of a square from 0, 0 to 1024, 1024 and 
                                        // with variable corner heights. For calculating the right t1 and t2 values, we 
                                        // must know also the fourth point of the square that contains the triangle 
                                        // we are trying to correct. I simply intersect a very long ray with the plane 
                                        // passing through the triangle and I can obtain in this way the height of the fourth corner.
                                        // This height then is used in different ways
                                        Vector3 p00 = new Vector3(0, h00 * 256, 0);
                                        Vector3 p01 = new Vector3(0, h01 * 256, 1024);
                                        Vector3 p10 = new Vector3(1024, h10 * 256, 0);
                                        Vector3 p11 = new Vector3(1024, h11 * 256, 1024);

                                        // In triangle collisions, everything is relative to the highest corner
                                        int maxHeight = Math.Max(Math.Max(Math.Max(h01, h11), h00), h10);

                                        // Choose which triangle to adjust
                                        if (h00 < Math.Min(h10, h01) && h11 < Math.Min(h10, h01))
                                        {
                                            // Case 1: both triangles have their right angles below the diagonal ( /D\ )
                                            Plane pl1 = new Plane(p01, p10, p00);

                                            Ray ray1;
                                            float distance1;

                                            // Find the 4th point
                                            ray1 = new Ray(new Vector3(1024, 32768, 1024), -Vector3.UnitY);
                                            pl1.Intersects(ref ray1, out distance1);
                                            distance1 = 32768 - distance1;
                                            distance1 /= 256;

                                            //int maxTriangle1 = Math.Max(Math.Max(h01, h00), h10);

                                            // Correction is the max height of the sector minus the height of the fourth point
                                            t2 = (int)(maxHeight - distance1) & 0x1f;

                                            Plane pl2 = new Plane(p10, p01, p11);

                                            Ray ray2;
                                            float distance2;

                                            // Find the 4th point
                                            ray2 = new Ray(new Vector3(0, 32768, 0), -Vector3.UnitY);
                                            pl2.Intersects(ref ray2, out distance2);
                                            distance2 = 32768 - distance2;
                                            distance2 /= 256;

                                            //int maxTriangle2 = Math.Max(Math.Max(h11, h10), h01);

                                            // Correction is the max height of the sector minus the height of the fourth point
                                            t1 = (int)(maxHeight - distance2) & 0x1f;
                                        }
                                        else if ((h01 == maxHeight || h00 == maxHeight || h10 == maxHeight) && h11 < h00)
                                        {
                                            // Case 2: h00 is highest corner and h11 is lower than h00. Typical example, when you raise of one click 
                                            // one corner of a sector (simplest case)
                                            Plane p = new Plane(p01, p11, p10);

                                            Ray ray;
                                            float distance;

                                            // Find the 4th point
                                            ray = new Ray(new Vector3(0, 32768, 0), -Vector3.UnitY);
                                            p.Intersects(ref ray, out distance);
                                            distance = 32768 - distance;
                                            distance /= 256;

                                            Vector3 pt = new Vector3(0, distance, 0);

                                            int maxTriangle = Math.Max(Math.Max(h01, h11), h10);

                                            // There are two cases (1.jpg and 2.jpg). The fourth point height can be lower than max height 
                                            // of the triangle or higher.
                                            if (distance <= maxTriangle)
                                            {
                                                t1 = 0;
                                                t2 = (int)(maxHeight - maxTriangle) & 0x1f;
                                            }
                                            else
                                            {
                                                t1 = 0;
                                                t2 = (int)(maxHeight - distance) & 0x1f;
                                            }
                                        }
                                        else if ((h01 == maxHeight || h11 == maxHeight || h10 == maxHeight) && h00 < h11)
                                        {
                                            // Case 3: similar to case 2, but the opposite 
                                            Plane p = new Plane(p01, p10, p00);

                                            Ray ray;
                                            float distance;

                                            // Find the 4th point
                                            ray = new Ray(new Vector3(1024, 32768, 1024), -Vector3.UnitY);
                                            p.Intersects(ref ray, out distance);
                                            distance = 32768 - distance;
                                            distance /= 256;

                                            Vector3 pt = new Vector3(0, distance, 0);

                                            int maxTriangle = Math.Max(Math.Max(h01, h00), h10);

                                            if (distance <= maxTriangle)
                                            {
                                                t2 = 0;
                                                t1 = (int)(maxHeight - maxTriangle) & 0x1f;
                                            }
                                            else
                                            {
                                                t2 = 0;
                                                t1 = (int)(maxHeight - distance) & 0x1f;
                                            }
                                        }
                                    }
                                    else
                                    {
                                        if (block.FloorPortal >= 0 && block.NoCollisionFloor)
                                        {
                                            if (q3 == q0 && q0 == q1 && q1 == q3)
                                            {
                                                function = 0x0d;
                                            }
                                            else
                                            {
                                                function = 0x0e;
                                            }
                                        }
                                        else
                                        {
                                            function = 0x08;
                                        }

                                        Vector3 p00 = new Vector3(0, h00 * 256, 0);
                                        Vector3 p01 = new Vector3(0, h01 * 256, 1024);
                                        Vector3 p10 = new Vector3(1024, h10 * 256, 0);
                                        Vector3 p11 = new Vector3(1024, h11 * 256, 1024);

                                        int maxHeight = Math.Max(Math.Max(Math.Max(h01, h11), h00), h10);

                                        // Choose which triangle to adjust
                                        if (h01 < Math.Min(h00, h11) && h10 < Math.Min(h00, h11))
                                        {
                                            Plane pl1 = new Plane(p11, p10, p00);

                                            Ray ray1;
                                            float distance1;

                                            // Find the 4th point
                                            ray1 = new Ray(new Vector3(0, 32768, 1024), -Vector3.UnitY);
                                            pl1.Intersects(ref ray1, out distance1);
                                            distance1 = 32768 - distance1;
                                            distance1 /= 256;

                                            int maxTriangle1 = Math.Max(Math.Max(h11, h00), h10);

                                            t2 = (int)(maxHeight - distance1) & 0x1f;

                                            Plane pl2 = new Plane(p00, p01, p11);

                                            Ray ray2;
                                            float distance2;

                                            // Find the 4th point
                                            ray2 = new Ray(new Vector3(1024, 32768, 0), -Vector3.UnitY);
                                            pl2.Intersects(ref ray2, out distance2);
                                            distance2 = 32768 - distance2;
                                            distance2 /= 256;

                                            int maxTriangle2 = Math.Max(Math.Max(h11, h00), h01);

                                            t1 = (int)(maxHeight - distance2) & 0x1f;
                                        }
                                        else if ((h11 == maxHeight || h00 == maxHeight || h10 == maxHeight) && h01 < h10)
                                        {
                                            Plane p = new Plane(p01, p11, p00);

                                            Ray ray;
                                            float distance;

                                            // Find the 4th point
                                            ray = new Ray(new Vector3(1024, 32768, 0), -Vector3.UnitY);
                                            p.Intersects(ref ray, out distance);
                                            distance = 32768 - distance;
                                            distance /= 256;

                                            int maxTriangle = Math.Max(Math.Max(h01, h11), h00);

                                            if (distance <= maxTriangle)
                                            {
                                                t1 = (int)(maxHeight - maxTriangle) & 0x1f;
                                                t2 = 0;
                                            }
                                            else
                                            {
                                                t1 = (int)(maxHeight - distance) & 0x1f;
                                                t2 = 0;
                                            }
                                        }
                                        else if ((h11 == maxHeight || h00 == maxHeight || h01 == maxHeight) && h10 < h01)
                                        {
                                            Plane p = new Plane(p11, p10, p00);

                                            Ray ray;
                                            float distance;

                                            // Find the 4th point
                                            ray = new Ray(new Vector3(0, 32768, 1024), -Vector3.UnitY);
                                            p.Intersects(ref ray, out distance);
                                            distance = 32768 - distance;
                                            distance /= 256;

                                            Vector3 pt = new Vector3(0, distance, 0);

                                            int maxTriangle = Math.Max(Math.Max(h11, h00), h10);

                                            if (distance <= maxTriangle)
                                            {
                                                t1 = 0;
                                                t2 = (int)(maxHeight - maxTriangle) & 0x1f;
                                            }
                                            else
                                            {
                                                t1 = 0;
                                                t2 = (int)(maxHeight - distance) & 0x1f;
                                            }
                                        }
                                    }

                                    // Now build the floordata codes
                                    ushort code1 = (ushort)(function | (t2 << 5) | (t1 << 10));
                                    ushort code2 = (ushort)((h10) | (h00 << 4) | (h01 << 8) | (h11 << 12));

                                    tempCodes.Add(code1);
                                    tempCodes.Add(code2);
                                }
                            }
                        }

                        if (block.CeilingDiagonalSplit != DiagonalSplit.None)
                        {
                            if (block.Type != BlockType.Wall)
                            {
                                int q0 = block.QAFaces[0];
                                int q1 = block.QAFaces[1];
                                int q2 = block.QAFaces[2];
                                int q3 = block.QAFaces[3];

                                int w0 = block.WSFaces[0];
                                int w1 = block.WSFaces[1];
                                int w2 = block.WSFaces[2];
                                int w3 = block.WSFaces[3];

                                // The real floor split of the sector
                                int split = block.RealSplitCeiling;
                                int function = 0;

                                int t1 = 0;
                                int t2 = 0;

                                int h00 = 0;
                                int h01 = 0;
                                int h10 = 0;
                                int h11 = 0;

                                // First, we fix the sector height
                                if (block.Type == BlockType.Wall)
                                    sector.Floor = (sbyte)(Rooms[idNewRoom].Info.YBottom / 256.0f - 0x0f);
                                else
                                    sector.Floor = (sbyte)(Rooms[idNewRoom].Info.YBottom / 256.0f - room.GetHighestFloorCorner(x, z));

                                if (block.CeilingDiagonalSplit == DiagonalSplit.NE || block.CeilingDiagonalSplit == DiagonalSplit.SW)
                                {
                                    lastFloorDataFunction = (ushort)tempCodes.Count;

                                    if (block.CeilingPortal >= 0 && block.NoCollisionCeiling)
                                    {
                                        if (block.CeilingDiagonalSplit == DiagonalSplit.NE)
                                        {
                                            function = 0x10;
                                        }
                                        else
                                        {
                                            function = 0x0f;
                                        }
                                    }
                                    else
                                    {
                                        function = 0x09;
                                    }

                                    if (block.CeilingDiagonalSplit == DiagonalSplit.NE)
                                    {
                                        int lowCorner = w1;
                                        int highCorner = w3;


                                        t1 = 0;
                                        t2 = (int)(highCorner - lowCorner) & 0x1f;

                                        h00 = w3 - w2;
                                        h10 = 0;
                                        h01 = 0;
                                        h11 = 0;
                                    }
                                    else
                                    {
                                        int lowCorner = w3;
                                        int highCorner = w1;

                                        t1 = (int)(highCorner - lowCorner) & 0x1f;
                                        t2 = 0;

                                        h00 = 0;
                                        h10 = 0;
                                        h01 = 0;
                                        h11 = w1 - w2;
                                    }
                                }
                                else
                                {
                                    lastFloorDataFunction = (ushort)tempCodes.Count;

                                    if (block.CeilingPortal >= 0 && block.NoCollisionCeiling)
                                    {
                                        if (block.CeilingDiagonalSplit == DiagonalSplit.NW)
                                        {
                                            function = 0x11;
                                        }
                                        else
                                        {
                                            function = 0x12;
                                        }
                                    }
                                    else
                                    {
                                        function = 0x0a;
                                    }

                                    if (block.CeilingDiagonalSplit == DiagonalSplit.NW)
                                    {
                                        int lowCorner = w0;
                                        int highCorner = w2;


                                        t1 = (int)(highCorner - lowCorner) & 0x1f;
                                        t2 = 0;

                                        h00 = 0;
                                        h10 = w2 - w1;
                                        h01 = 0;
                                        h11 = 0;
                                    }
                                    else
                                    {
                                        int lowCorner = w2;
                                        int highCorner = w0;

                                        t1 = 0;
                                        t2 = (int)(highCorner - lowCorner) & 0x1f;

                                        h00 = 0;
                                        h10 = 0;
                                        h01 = w0 - w1;
                                        h11 = 0;
                                    }
                                }

                                ushort code1 = (ushort)(function | (t2 << 5) | (t1 << 10));
                                ushort code2 = (ushort)((h10) | (h00 << 4) | (h01 << 8) | (h11 << 12));

                                tempCodes.Add(code1);
                                tempCodes.Add(code2);
                            }
                        }
                        else
                        {
                            if (block.CeilingSlopeX == 0 && block.CeilingSlopeZ == 0)
                            {
                                int w0 = block.WSFaces[0];
                                int w1 = block.WSFaces[1];
                                int w2 = block.WSFaces[2];
                                int w3 = block.WSFaces[3];

                                // We have not a slope, so if this is not a horizontal square then we have triangulation
                                if (!Room.IsQuad(x, z, w0, w1, w2, w3, true))
                                {
                                    // We have to find the axis of the triangulation
                                    int min = room.GetLowestCeilingCorner(x, z);
                                    int max = room.GetHighestCeilingCorner(x, z);

                                    lastFloorDataFunction = (ushort)tempCodes.Count;

                                    // Corner heights
                                    int h10 = max - w2;
                                    int h00 = max - w3;
                                    int h01 = max - w0;
                                    int h11 = max - w1;

                                    int maxCorner = Math.Max(h00, Math.Max(h01, Math.Max(h10, h11)));

                                    // Flip the Y axis
                                    min = -min;
                                    max = -max;

                                    int t1 = 0;
                                    int t2 = 0;

                                    // The real ceiling split of the sector
                                    int split = block.RealSplitCeiling;
                                    int function = 0;

                                    // Now, for each of the two possible splits, apply the algorithm described by meta2tr and 
                                    // TRosettaStone 3. I've simply managed some cases by hand. The difficult task is to 
                                    // decide if apply the height correction to both triangles or just one of them.
                                    // Function must be decided looking at portals.

                                    if (split == 0)
                                    {
                                        if (block.CeilingPortal >= 0 && block.NoCollisionCeiling)
                                        {
                                            if (w0 == w1 && w1 == w2 && w2 == w0)
                                            {
                                                function = 0x10;
                                            }
                                            else
                                            {
                                                function = 0x0f;
                                            }
                                        }
                                        else
                                        {
                                            function = 0x09;
                                        }

                                        Vector3 p00 = new Vector3(0, h00 * 256, 1024);
                                        Vector3 p01 = new Vector3(0, h01 * 256, 0);
                                        Vector3 p10 = new Vector3(1024, h10 * 256, 1024);
                                        Vector3 p11 = new Vector3(1024, h11 * 256, 0);

                                        int maxHeight = Math.Max(Math.Max(Math.Max(h01, h11), h00), h10);

                                        // Choose which triangle to adjust
                                        if (h00 < Math.Min(h10, h01) && h11 < Math.Min(h10, h01))
                                        {
                                            Plane pl1 = new Plane(p01, p00, p10);

                                            Ray ray1;
                                            float distance1;

                                            // Find the 4th point
                                            ray1 = new Ray(new Vector3(1024, 32768, 0), -Vector3.UnitY);
                                            pl1.Intersects(ref ray1, out distance1);
                                            distance1 = 32768 - distance1;
                                            distance1 /= 256;

                                            int maxTriangle1 = Math.Max(Math.Max(h01, h00), h10);

                                            t2 = (int)(-maxHeight + distance1) & 0x1f;

                                            Plane pl2 = new Plane(p10, p11, p01);

                                            Ray ray2;
                                            float distance2;

                                            // Find the 4th point
                                            ray2 = new Ray(new Vector3(0, 32768, 1024), -Vector3.UnitY);
                                            pl2.Intersects(ref ray2, out distance2);
                                            distance2 = 32768 - distance2;
                                            distance2 /= 256;

                                            int maxTriangle2 = Math.Max(Math.Max(h11, h10), h01);

                                            t1 = (int)(-maxHeight + distance2) & 0x1f;
                                        }
                                        else if ((h01 == maxHeight || h00 == maxHeight || h10 == maxHeight) && h11 < h00)
                                        {
                                            Plane p = new Plane(p01, p10, p11);

                                            Ray ray;
                                            float distance;

                                            // Find the 4th point
                                            ray = new Ray(new Vector3(0, 32768, 1024), -Vector3.UnitY);
                                            p.Intersects(ref ray, out distance);
                                            distance = 32768 - distance;
                                            distance /= 256;

                                            Vector3 pt = new Vector3(0, distance, 0);

                                            int maxTriangle = Math.Max(Math.Max(h01, h11), h10);

                                            if (distance <= maxTriangle)
                                            {
                                                t1 = 0;
                                                t2 = (int)(-maxHeight + maxTriangle) & 0x1f;
                                            }
                                            else
                                            {
                                                t1 = 0;
                                                t2 = (int)(-maxHeight + distance) & 0x1f;
                                            }
                                        }
                                        else if ((h01 == maxHeight || h11 == maxHeight || h10 == maxHeight) && h00 < h11)
                                        {
                                            Plane p = new Plane(p01, p00, p10);

                                            Ray ray;
                                            float distance;

                                            // Find the 4th point
                                            ray = new Ray(new Vector3(1024, 32768, 0), -Vector3.UnitY);
                                            p.Intersects(ref ray, out distance);
                                            distance = 32768 - distance;
                                            distance /= 256;

                                            Vector3 pt = new Vector3(0, distance, 0);

                                            int maxTriangle = Math.Max(Math.Max(h01, h00), h10);

                                            if (distance <= maxTriangle)
                                            {
                                                t2 = 0;
                                                t1 = (int)(-maxHeight + maxTriangle) & 0x1f;
                                            }
                                            else
                                            {
                                                t2 = 0;
                                                t1 = (int)(-maxHeight + distance) & 0x1f;
                                            }
                                        }
                                    }
                                    else
                                    {
                                        if (block.CeilingPortal >= 0 && block.NoCollisionCeiling)
                                        {
                                            if (w3 == w0 && w0 == w1 && w1 == w3)
                                            {
                                                function = 0x11;
                                            }
                                            else
                                            {
                                                function = 0x12;
                                            }
                                        }
                                        else
                                        {
                                            function = 0x0a;
                                        }

                                        Vector3 p00 = new Vector3(0, h00 * 256, 1024);
                                        Vector3 p01 = new Vector3(0, h01 * 256, 0);
                                        Vector3 p10 = new Vector3(1024, h10 * 256, 1024);
                                        Vector3 p11 = new Vector3(1024, h11 * 256, 0);

                                        int maxHeight = Math.Max(Math.Max(Math.Max(h01, h11), h00), h10);

                                        // Choose which triangle to adjust
                                        if (h01 < Math.Min(h00, h11) && h10 < Math.Min(h00, h11))
                                        {
                                            Plane pl1 = new Plane(p11, p00, p10);

                                            Ray ray1;
                                            float distance1;

                                            // Find the 4th point
                                            ray1 = new Ray(new Vector3(0, 32768, 0), -Vector3.UnitY);
                                            pl1.Intersects(ref ray1, out distance1);
                                            distance1 = 32768 - distance1;
                                            distance1 /= 256;

                                            int maxTriangle1 = Math.Max(Math.Max(h11, h00), h10);

                                            t2 = (int)(-maxHeight + distance1) & 0x1f;

                                            Plane pl2 = new Plane(p00, p11, p01);

                                            Ray ray2;
                                            float distance2;

                                            // Find the 4th point
                                            ray2 = new Ray(new Vector3(1024, 32768, 1024), -Vector3.UnitY);
                                            pl2.Intersects(ref ray2, out distance2);
                                            distance2 = 32768 - distance2;
                                            distance2 /= 256;

                                            int maxTriangle2 = Math.Max(Math.Max(h11, h00), h01);

                                            t1 = (int)(-maxHeight + distance2) & 0x1f;
                                        }
                                        else if ((h11 == maxHeight || h00 == maxHeight || h10 == maxHeight) && h01 < h10)
                                        {
                                            Plane p = new Plane(p01, p00, p11);

                                            Ray ray;
                                            float distance;

                                            // Find the 4th point
                                            ray = new Ray(new Vector3(1024, 32768, 1024), -Vector3.UnitY);
                                            p.Intersects(ref ray, out distance);
                                            distance = 32768 - distance;
                                            distance /= 256;

                                            int maxTriangle = Math.Max(Math.Max(h01, h11), h00);

                                            if (distance <= maxTriangle)
                                            {
                                                t1 = (int)(-maxHeight + maxTriangle) & 0x1f;
                                                t2 = 0;
                                            }
                                            else
                                            {
                                                t1 = (int)(-maxHeight + distance) & 0x1f;
                                                t2 = 0;
                                            }
                                        }
                                        else if ((h11 == maxHeight || h00 == maxHeight || h01 == maxHeight) && h10 < h01)
                                        {
                                            Plane p = new Plane(p11, p00, p10);

                                            Ray ray;
                                            float distance;

                                            // Find the 4th point
                                            ray = new Ray(new Vector3(0, 32768, 0), -Vector3.UnitY);
                                            p.Intersects(ref ray, out distance);
                                            distance = 32768 - distance;
                                            distance /= 256;

                                            Vector3 pt = new Vector3(0, distance, 0);

                                            int maxTriangle = Math.Max(Math.Max(h11, h00), h10);

                                            if (distance <= maxTriangle)
                                            {
                                                t1 = 0;
                                                t2 = (int)(-maxHeight + maxTriangle) & 0x1f;
                                            }
                                            else
                                            {
                                                t1 = 0;
                                                t2 = (int)(-maxHeight + distance) & 0x1f;
                                            }
                                        }
                                    }

                                    // Now build the floordata codes
                                    ushort code1 = (ushort)(function | (t2 << 5) | (t1 << 10));
                                    ushort code2 = (ushort)((h11) | (h01 << 4) | (h00 << 8) | (h10 << 12));

                                    tempCodes.Add(code1);
                                    tempCodes.Add(code2);
                                }
                            }
                        }

                        // If sector is Climbable
                        if (block.Climb[0] || block.Climb[1] || block.Climb[2] || block.Climb[3])
                        {
                            ushort climb = 0x06;

                            if (block.Climb[0]) climb |= (0x01 << 8);
                            if (block.Climb[1]) climb |= (0x02 << 8);
                            if (block.Climb[2]) climb |= (0x04 << 8);
                            if (block.Climb[3]) climb |= (0x08 << 8);

                            lastFloorDataFunction = (ushort)tempCodes.Count;
                            tempCodes.Add(climb);
                        }

                        // If sector is Death
                        if ((block.Flags & BlockFlags.Monkey) == BlockFlags.Monkey)
                        {
                            lastFloorDataFunction = (ushort)tempCodes.Count;
                            tempCodes.Add(0x13);
                        }

                        // If sector is Beetle
                        if ((block.Flags & BlockFlags.Beetle) == BlockFlags.Beetle)
                        {
                            lastFloorDataFunction = (ushort)tempCodes.Count;
                            tempCodes.Add(0x15);
                        }

                        // If sector is Trigger triggerer
                        if ((block.Flags & BlockFlags.TriggerTriggerer) == BlockFlags.TriggerTriggerer)
                        {
                            lastFloorDataFunction = (ushort)tempCodes.Count;
                            tempCodes.Add(0x14);
                        }

                        // Triggers
                        if (room.Blocks[x, z].Triggers.Count > 0)
                        {
                            int found = -1;
                            TriggerInstance trigger;

                            // First, I search a special trigger, if exists
                            for (int j = 0; j < room.Blocks[x, z].Triggers.Count; j++)
                            {
                                trigger = _editor.Level.Triggers[room.Blocks[x, z].Triggers[j]];

                                if (trigger.TriggerType == TriggerType.Trigger && found == -1)
                                {
                                    // Normal trigger can be used only if in the sector there aren't special triggers
                                    found = j;
                                    continue;
                                }

                                if (trigger.TriggerType != TriggerType.Trigger)
                                {
                                    // For now I use the first special trigger of the chain, ignoring the following triggers
                                    found = j;
                                    break;
                                }
                            }

                            List<int> tempTriggers = new List<int>();
                            tempTriggers.Add(room.Blocks[x, z].Triggers[found]);
                            for (int j = 0; j < room.Blocks[x, z].Triggers.Count; j++)
                            {
                                if (j != found) tempTriggers.Add(room.Blocks[x, z].Triggers[j]);
                            }

                            trigger = _editor.Level.Triggers[room.Blocks[x, z].Triggers[found]];

                            lastFloorDataFunction = (ushort)tempCodes.Count;

                            // Trigger type and setup are coming from the found trigger. Other triggers are needed onlt for action.
                            ushort trigger1 = 0x04;
                            if (trigger.TriggerType == TriggerType.Trigger) trigger1 |= (ushort)(0x00 << 8);
                            if (trigger.TriggerType == TriggerType.Pad) trigger1 |= (ushort)(0x01 << 8);
                            if (trigger.TriggerType == TriggerType.Switch) trigger1 |= (ushort)(0x02 << 8);
                            if (trigger.TriggerType == TriggerType.Key) trigger1 |= (ushort)(0x03 << 8);
                            if (trigger.TriggerType == TriggerType.Pickup) trigger1 |= (ushort)(0x04 << 8);
                            if (trigger.TriggerType == TriggerType.Heavy) trigger1 |= (ushort)(0x05 << 8);
                            if (trigger.TriggerType == TriggerType.Antipad) trigger1 |= (ushort)(0x06 << 8);
                            if (trigger.TriggerType == TriggerType.Combat) trigger1 |= (ushort)(0x07 << 8);
                            if (trigger.TriggerType == TriggerType.Dummy) trigger1 |= (ushort)(0x08 << 8);
                            if (trigger.TriggerType == TriggerType.Antitrigger) trigger1 |= (ushort)(0x09 << 8);
                            if (trigger.TriggerType == TriggerType.HeavySwitch) trigger1 |= (ushort)(0x0a << 8);
                            if (trigger.TriggerType == TriggerType.HeavyAntritrigger) trigger1 |= (ushort)(0x0b << 8);
                            if (trigger.TriggerType == TriggerType.Monkey) trigger1 |= (ushort)(0x0c << 8);

                            ushort triggerSetup = 0;
                            triggerSetup |= (ushort)(trigger.Timer & 0xff);
                            triggerSetup |= (ushort)(trigger.OneShot ? 0x100 : 0);
                            triggerSetup |= (ushort)(trigger.Bits[0] ? (0x01 << 13) : 0);
                            triggerSetup |= (ushort)(trigger.Bits[1] ? (0x01 << 12) : 0);
                            triggerSetup |= (ushort)(trigger.Bits[2] ? (0x01 << 11) : 0);
                            triggerSetup |= (ushort)(trigger.Bits[3] ? (0x01 << 10) : 0);
                            triggerSetup |= (ushort)(trigger.Bits[4] ? (0x01 << 9) : 0);

                            tempCodes.Add(trigger1);
                            tempCodes.Add(triggerSetup);

                            for (int j = 0; j < tempTriggers.Count; j++)
                            {
                                trigger = _editor.Level.Triggers[tempTriggers[j]];

                                ushort trigger2 = 0;
                                ushort trigger3 = 0;

                                if (trigger.TargetType == TriggerTargetType.Object)
                                {
                                    // Trigger for object
                                    int item = trigger.Target;
                                    if (_editor.Level.Objects[trigger.Target].Type == ObjectInstanceType.Moveable)
                                    {
                                        MoveableInstance instance = (MoveableInstance)_editor.Level.Objects[trigger.Target];
                                        if (instance.Model.ObjectID >= 398 && instance.Model.ObjectID <= 406)
                                        {
                                            item = _aiObjectsTable[trigger.Target];
                                        }
                                        else
                                        {
                                            item = _moveablesTable[trigger.Target];
                                        }
                                    }

                                    trigger2 = (ushort)(item & 0x3ff | (0x00 << 10));
                                    tempCodes.Add(trigger2);
                                }
                                else if (trigger.TargetType == TriggerTargetType.Camera)
                                {
                                    // Trigger for camera
                                    trigger2 = (ushort)(_cameraTable[trigger.Target] & 0x3ff | (0x01 << 10));
                                    tempCodes.Add(trigger2);

                                    // Additional short
                                    trigger3 = 0;
                                    trigger3 |= (ushort)(trigger.Timer & 0xff);
                                    trigger3 |= (ushort)(trigger.OneShot ? 0x100 : 0);
                                    tempCodes.Add(trigger3);
                                }
                                else if (trigger.TargetType == TriggerTargetType.Sink)
                                {
                                    // Trigger for sink
                                    trigger2 = (ushort)(_sinkTable[trigger.Target] & 0x3ff | (0x02 << 10));
                                    tempCodes.Add(trigger2);
                                }
                                else if (trigger.TargetType == TriggerTargetType.FlipMap)
                                {
                                    // Trigger for flip map
                                    trigger2 = (ushort)(trigger.Target & 0x3ff | (0x03 << 10));
                                    tempCodes.Add(trigger2);
                                }
                                else if (trigger.TargetType == TriggerTargetType.FlipOn)
                                {
                                    // Trigger for flip map on
                                    trigger2 = (ushort)(trigger.Target & 0x3ff | (0x04 << 10));
                                    tempCodes.Add(trigger2);
                                }
                                else if (trigger.TargetType == TriggerTargetType.FlipOff)
                                {
                                    // Trigger for flip map off
                                    trigger2 = (ushort)(trigger.Target & 0x3ff | (0x05 << 10));
                                    tempCodes.Add(trigger2);
                                }
                                else if (trigger.TargetType == TriggerTargetType.Target)
                                {
                                    // Trigger for look at item
                                    trigger2 = (ushort)(_moveablesTable[trigger.Target] & 0x3ff | (0x06 << 10));
                                    tempCodes.Add(trigger2);
                                }
                                else if (trigger.TargetType == TriggerTargetType.FinishLevel)
                                {
                                    // Trigger for finish level
                                    trigger2 = (ushort)(trigger.Target & 0x3ff | (0x07 << 10));
                                    tempCodes.Add(trigger2);
                                }
                                else if (trigger.TargetType == TriggerTargetType.PlayAudio)
                                {
                                    // Trigger for play soundtrack
                                    trigger2 = (ushort)(trigger.Target & 0x3ff | (0x08 << 10));
                                    tempCodes.Add(trigger2);
                                }
                                else if (trigger.TargetType == TriggerTargetType.FlipEffect)
                                {
                                    // Trigger for flip effect
                                    trigger2 = (ushort)(trigger.Target & 0x3ff | (0x09 << 10));
                                    tempCodes.Add(trigger2);

                                    /*  trigger2 = (ushort)(trigger.Timer);
                                      tempCodes.Add(trigger2);*/
                                }
                                else if (trigger.TargetType == TriggerTargetType.Secret)
                                {
                                    // Trigger for secret found
                                    trigger2 = (ushort)(trigger.Target & 0x3ff | (0x0a << 10));
                                    tempCodes.Add(trigger2);
                                }
                                else if (trigger.TargetType == TriggerTargetType.FlyByCamera)
                                {
                                    // Trigger for fly by
                                    trigger2 = (ushort)(trigger.Target & 0x3ff | (0x0c << 10));
                                    tempCodes.Add(trigger2);

                                    trigger2 = (ushort)(trigger.OneShot ? 0x0100 : 0x00);
                                    tempCodes.Add(trigger2);
                                }
                            }

                            tempCodes[tempCodes.Count - 1] |= 0x8000; // End of the action list
                        }

                        if (tempCodes.Count == 0)
                        {
                            sector.FloorDataIndex = 0;
                        }
                        else
                        {
                            // Mark the end of the list
                            tempCodes[lastFloorDataFunction] |= 0x8000;
                            tempFloorData.AddRange(tempCodes);
                        }

                        // Update the sector
                        SaveSector(idNewRoom, x, z, sector);
                    }
                }
            }

            FloorData = tempFloorData.ToArray();

            ReportProgress(80, "    Floordata size: " + FloorData.Length * 2 + " bytes");
        }
    }
}
