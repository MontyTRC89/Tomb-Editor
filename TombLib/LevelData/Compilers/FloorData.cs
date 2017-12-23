using SharpDX;
using System;
using System.Collections.Generic;
using System.Linq;
using TombLib.LevelData;

namespace TombLib.LevelData.Compilers
{
    public sealed partial class LevelCompilerTr4
    {
        private bool IsWallSurroundedByWalls(int x, int z, Room room)
        {
            if (x > 0 && !room.Blocks[x - 1, z].IsAnyWall) return false;
            if (z > 0 && !room.Blocks[x, z - 1].IsAnyWall) return false;
            if (x < room.NumXSectors - 1 && !room.Blocks[x + 1, z].IsAnyWall) return false;
            if (z < room.NumZSectors - 1 && !room.Blocks[x, z + 1].IsAnyWall) return false;
            return true;
        }

        private void BuildFloorData()
        {
            ReportProgress(70, "Building floordata");

            // Initialize the floordata list and add the dummy entry for walls and sectors without particular things
            _floorData.Add(0x0000);

            for (var i = 0; i < _level.Rooms.Length; i++)
            {
                var room = _level.Rooms[i];
                if (room == null)
                    continue;
                var tempRoom = _tempRooms[room];

                // Get all portals
                var ceilingPortals = new List<PortalInstance>();
                for (var z = 0; z < room.NumZSectors; z++)
                    for (var x = 0; x < room.NumXSectors; x++)
                    {
                        var ceilingPortal = room.Blocks[x, z].CeilingPortal;
                        if ((ceilingPortal != null) && !ceilingPortals.Contains(ceilingPortal))
                            ceilingPortals.Add(ceilingPortal);
                    }

                for (var z = 0; z < room.NumZSectors; z++)
                {
                    for (var x = 0; x < room.NumXSectors; x++)
                    {
                        var sector = GetSector(tempRoom, x, z);
                        var block = room.Blocks[x, z];
                        Room.RoomConnectionInfo floorPortalInfo = room.GetFloorRoomConnectionInfo(new VectorInt2(x, z));
                        Room.RoomConnectionInfo ceilingPortalInfo = room.GetCeilingRoomConnectionInfo(new VectorInt2(x, z));

                        var baseFloorData = (ushort)_floorData.Count;

                        // If a sector is a wall and this room is a water room,
                        // It must be checked before if on the neighbour sector if there's a ceiling portal
                        // because eventually a vertical portal will be added
                        Room isWallWithCeilingPortal = null;
                        foreach (var portal in ceilingPortals)
                        {
                            // Check if x, z is inside the portal
                            if (!(x >= portal.Area.X0 - 1 && 
                                  z >= portal.Area.Y0 - 1 && 
                                  x <= portal.Area.X0 + portal.Area.Width + 1 && 
                                  z <= portal.Area.Y0 + portal.Area.Height + 1)) continue;

                            // Check if this is a wall
                            if (!block.IsAnyWall) continue;

                            // Check if current wall is surrounded by walls
                            if (IsWallSurroundedByWalls(x, z, room)) continue;

                            // Get new coordinates
                            var adjoining = portal.AdjoiningRoom;
                            var x2 = (int)(room.Position.X + x - adjoining.Position.X);
                            var z2 = (int)(room.Position.Z + z - adjoining.Position.Z);

                            // Check if we are outside the boundaries of adjoining room
                            if (x2 < 0 || z2 < 0 || x2 > adjoining.NumXSectors - 1 || z2 > adjoining.NumZSectors - 1) continue;

                            var adjoiningBlock = adjoining.Blocks[x2, z2];

                            // Now check for a ladder
                            if (block.Type == BlockType.Wall)
                            {
                                // Simplest case, just check for ceiling rooms
                                if (!adjoiningBlock.IsAnyWall)
                                {
                                    isWallWithCeilingPortal = portal.AdjoiningRoom;
                                    break;
                                }
                            }
                            else
                            {
                                // For border walls, we must consider also possible wall portals on ceiling room
                                if (adjoiningBlock.Type == BlockType.BorderWall && adjoiningBlock.WallPortal != null)
                                {
                                    isWallWithCeilingPortal = adjoiningBlock.WallPortal.AdjoiningRoom;
                                    break;
                                }
                                else if (adjoiningBlock.Type == BlockType.Floor)
                                {
                                    isWallWithCeilingPortal = portal.AdjoiningRoom;
                                    break;
                                }
                            }
                        }

                        // If sector is a wall with a ceiling portal on it or near it
                        if (block.WallPortal == null && isWallWithCeilingPortal != null &&
                            ((block.Type == BlockType.Wall && block.FloorDiagonalSplit == DiagonalSplit.None) ||
                             room.Blocks[x, z].Type == BlockType.BorderWall))
                        {
                            const ushort data1 = 0x8001;
                            var data2 = (ushort)_roomsRemappingDictionary[isWallWithCeilingPortal];

                            _floorData.Add(data1);
                            _floorData.Add(data2);

                            // Update current sector
                            sector.FloorDataIndex = baseFloorData;

                            SaveSector(tempRoom, x, z, sector);

                            continue;
                        }

                        // If sector is a border wall without portals or a normal wall
                        if ((block.Type == BlockType.Wall && block.FloorDiagonalSplit == DiagonalSplit.None) ||
                            (block.Type == BlockType.BorderWall && block.WallPortal == null))
                        {
                            sector.FloorDataIndex = 0;
                            sector.Floor = -127;
                            sector.Ceiling = -127;

                            SaveSector(tempRoom, x, z, sector);
                            continue;
                        }

                        // If sector is a floor portal
                        if (floorPortalInfo.TraversableType != Room.RoomConnectionType.NoPortal)
                        {
                            var portal = block.FloorPortal;
                            int roomIndex = _roomsRemappingDictionary[portal.AdjoiningRoom];
                            if (roomIndex >= 254)
                                throw new ApplicationException("Passable floor and ceiling portals are unfortunately only possible in the first 255 rooms. Portal " + portal + " can't be added.");
                            sector.RoomBelow = checked((byte)roomIndex);
                        }
                        else
                        {
                            sector.RoomBelow = 255;
                        }

                        // If sector is a ceiling portal
                        if (ceilingPortalInfo.TraversableType != Room.RoomConnectionType.NoPortal)
                        {
                            var portal = block.CeilingPortal;
                            int roomIndex = _roomsRemappingDictionary[portal.AdjoiningRoom];
                            if (roomIndex >= 254)
                                throw new ApplicationException("Passable floor and ceiling portals are unfortunately only possible in the first 255 rooms. Portal " + portal + " can't be added.");
                            sector.RoomAbove = checked((byte)roomIndex);
                        }
                        else
                        {
                            sector.RoomAbove = 255;
                        }

                        // If sector is a wall portal
                        if (block.WallPortal != null)
                        {
                            // Only if the portal is not a Toggle Opacity 1
                            if (block.WallPortal.Opacity != PortalOpacity.SolidFaces)
                            {
                                const ushort data1 = 0x8001;
                                var data2 = (ushort)_roomsRemappingDictionary[block.WallPortal.AdjoiningRoom];

                                _floorData.Add(data1);
                                _floorData.Add(data2);

                                if (block.WallPortal.Opacity == PortalOpacity.TraversableFaces)
                                {
                                    sector.Floor = -127;
                                    sector.Ceiling = -127;
                                }

                                // Update current sector
                                sector.FloorDataIndex = baseFloorData;
                                SaveSector(tempRoom, x, z, sector);
                            }
                            else
                            {
                                sector.Floor = -127;
                                sector.Ceiling = -127;
                                SaveSector(tempRoom, x, z, sector);
                            }

                            continue;
                        }

                        // From this point, the loop will never be bypassed, so surely add at least one
                        // floordata value
                        sector.FloorDataIndex = baseFloorData;
                        var tempCodes = new List<ushort>();
                        var lastFloorDataFunction = (ushort)tempCodes.Count;

                        // If sector has a floor slope
                        if (block.FloorIfQuadSlopeX != 0 || block.FloorIfQuadSlopeZ != 0)
                        {
                            lastFloorDataFunction = (ushort)tempCodes.Count;
                            tempCodes.Add(0x02);

                            sector.Floor = (sbyte)(-room.Position.Y - block.FloorMax);

                            var slope = (ushort)(((block.FloorIfQuadSlopeZ) << 8) | ((block.FloorIfQuadSlopeX) & 0xff));

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

                            // The real floor split of the sector
                            int function;

                            int t1;
                            int t2;

                            int h00;
                            int h01;
                            int h10;
                            int h11;

                            // First, we fix the sector height
                            if (block.Type == BlockType.Wall)
                                sector.Floor = (sbyte)-(room.Position.Y + 0x0f);
                            else
                                sector.Floor = (sbyte)-(room.Position.Y + block.FloorMax);

                            if (block.FloorDiagonalSplit == DiagonalSplit.XnZn ||
                                block.FloorDiagonalSplit == DiagonalSplit.XpZp)
                            {
                                lastFloorDataFunction = (ushort)tempCodes.Count;

                                if (floorPortalInfo.TraversableType != Room.RoomConnectionType.NoPortal)
                                {
                                    function = block.FloorDiagonalSplit == DiagonalSplit.XnZn ? 0x0c : 0x0b;
                                }
                                else
                                {
                                    function = 0x07;
                                }

                                // Diagonal steps and walls are the simplest case. All corner heights are zero
                                // except eventually the right angle on the top face. Corrections t1 and t2
                                // are simple to calculate
                                if (block.FloorDiagonalSplit == DiagonalSplit.XnZn)
                                {
                                    var lowCorner = q1;
                                    var highCorner = q3;

                                    if (block.Type == BlockType.Wall)
                                    {
                                        t1 = 0;
                                        t2 = 15 - block.QAFaces[
                                                 1]; // Diagonal wall max height minus the height of the lower right angle

                                        h00 = 0;
                                        h10 = 0;
                                        h01 = 0;
                                        h11 = 0;
                                    }
                                    else
                                    {
                                        t1 = q2 > q3 ? q3 - q2 : 0;
                                        t2 = (highCorner - lowCorner) & 0x1f;

                                        h00 = Math.Abs(q3 - q2);
                                        h10 = 0;
                                        h01 = 0;
                                        h11 = 0;
                                    }
                                }
                                else
                                {
                                    var lowCorner = q3;
                                    var highCorner = q1;

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
                                        t1 = (highCorner - lowCorner) & 0x1f;
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

                                if (floorPortalInfo.TraversableType != Room.RoomConnectionType.NoPortal)
                                {
                                    function = block.FloorDiagonalSplit == DiagonalSplit.XpZn ? 0x0d : 0x0e;
                                }
                                else
                                {
                                    function = 0x08;
                                }

                                if (block.FloorDiagonalSplit == DiagonalSplit.XpZn)
                                {
                                    var lowCorner = q0;
                                    var highCorner = q2;

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
                                        t1 = highCorner - lowCorner & 0x1f;
                                        t2 = 0;

                                        h00 = 0;
                                        h10 = q2 - q1;
                                        h01 = 0;
                                        h11 = 0;
                                    }
                                }
                                else
                                {
                                    var lowCorner = q2;
                                    var highCorner = q0;

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
                                        t2 = highCorner - lowCorner & 0x1f;

                                        h00 = 0;
                                        h10 = 0;
                                        h01 = q0 - q1;
                                        h11 = 0;
                                    }
                                }
                            }

                            var code1 = (ushort)(function | (t2 << 5) | (t1 << 10));
                            var code2 = (ushort)((h10) | (h00 << 4) | (h01 << 8) | (h11 << 12));

                            tempCodes.Add(code1);
                            tempCodes.Add(code2);
                        }
                        else
                        {
                            if (block.FloorIfQuadSlopeX == 0 && block.FloorIfQuadSlopeZ == 0)
                            {
                                int q0 = block.QAFaces[0];
                                int q1 = block.QAFaces[1];
                                int q2 = block.QAFaces[2];
                                int q3 = block.QAFaces[3];

                                // We have not a slope, so if this is not a horizontal square then we have triangulation
                                if (!Block.IsQuad(q0, q1, q2, q3))
                                {
                                    // First, we fix the sector height
                                    sector.Floor = (sbyte)-(room.Position.Y + block.FloorMax);

                                    // Then we have to find the axis of the triangulation
                                    var min = block.FloorMin;

                                    lastFloorDataFunction = (ushort)tempCodes.Count;

                                    // Corner heights
                                    var h10 = q2 - min;
                                    var h00 = q3 - min;
                                    var h01 = q0 - min;
                                    var h11 = q1 - min;

                                    var t1 = 0;
                                    var t2 = 0;

                                    // The real floor split of the sector
                                    int function;

                                    if (!block.FloorSplitDirectionIsXEqualsZ)
                                    {
                                        if (floorPortalInfo.TraversableType != Room.RoomConnectionType.NoPortal)
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
                                        var p00 = new Vector3(0, h00 * 256, 0);
                                        var p01 = new Vector3(0, h01 * 256, 1024);
                                        var p10 = new Vector3(1024, h10 * 256, 0);
                                        var p11 = new Vector3(1024, h11 * 256, 1024);

                                        // In triangle collisions, everything is relative to the highest corner
                                        var maxHeight = Math.Max(Math.Max(Math.Max(h01, h11), h00), h10);

                                        // Choose which triangle to adjust
                                        if (h00 < Math.Min(h10, h01) && h11 < Math.Min(h10, h01))
                                        {
                                            // Case 1: both triangles have their right angles below the diagonal ( /D\ )
                                            var pl1 = new Plane(p01, p10, p00);

                                            float distance1;

                                            // Find the 4th point
                                            var ray1 = new Ray(new Vector3(1024, 32768, 1024), -Vector3.UnitY);
                                            pl1.Intersects(ref ray1, out distance1);
                                            distance1 = 32768 - (float)Math.Round(distance1);
                                            distance1 /= 256;

                                            //int maxTriangle1 = Math.Max(Math.Max(h01, h00), h10);

                                            // Correction is the max height of the sector minus the height of the fourth point
                                            t2 = (int)(maxHeight - distance1) & 0x1f;

                                            var pl2 = new Plane(p10, p01, p11);

                                            float distance2;

                                            // Find the 4th point
                                            var ray2 = new Ray(new Vector3(0, 32768, 0), -Vector3.UnitY);
                                            pl2.Intersects(ref ray2, out distance2);
                                            distance2 = 32768 - (float)Math.Round(distance2);
                                            distance2 /= 256;

                                            //int maxTriangle2 = Math.Max(Math.Max(h11, h10), h01);

                                            // Correction is the max height of the sector minus the height of the fourth point
                                            t1 = (int)(maxHeight - distance2) & 0x1f;
                                        }
                                        else if ((h01 == maxHeight || h00 == maxHeight || h10 == maxHeight) &&
                                                 h11 < h00)
                                        {
                                            // Case 2: h00 is highest corner and h11 is lower than h00. Typical example, when you raise of one click
                                            // one corner of a sector (simplest case)
                                            var p = new Plane(p01, p11, p10);

                                            float distance;

                                            // Find the 4th point
                                            var ray = new Ray(new Vector3(0, 32768, 0), -Vector3.UnitY);
                                            p.Intersects(ref ray, out distance);
                                            distance = 32768 - (float)Math.Round(distance);
                                            distance /= 256;

                                            var maxTriangle = Math.Max(Math.Max(h01, h11), h10);

                                            // There are two cases (1.jpg and 2.jpg). The fourth point height can be lower than max height
                                            // of the triangle or higher.
                                            if (distance <= maxTriangle)
                                            {
                                                t1 = 0;
                                                t2 = maxHeight - maxTriangle & 0x1f;
                                            }
                                            else
                                            {
                                                t1 = 0;
                                                t2 = (int)(maxHeight - distance) & 0x1f;
                                            }
                                        }
                                        else if ((h01 == maxHeight || h11 == maxHeight || h10 == maxHeight) &&
                                                 h00 < h11)
                                        {
                                            // Case 3: similar to case 2, but the opposite
                                            var p = new Plane(p01, p10, p00);

                                            float distance;

                                            // Find the 4th point
                                            var ray = new Ray(new Vector3(1024, 32768, 1024), -Vector3.UnitY);
                                            p.Intersects(ref ray, out distance);
                                            distance = 32768 - (float)Math.Round(distance);
                                            distance /= 256;

                                            var maxTriangle = Math.Max(Math.Max(h01, h00), h10);

                                            if (distance <= maxTriangle)
                                            {
                                                t2 = 0;
                                                t1 = maxHeight - maxTriangle & 0x1f;
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
                                        if (floorPortalInfo.TraversableType != Room.RoomConnectionType.NoPortal)
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

                                        var p00 = new Vector3(0, h00 * 256, 0);
                                        var p01 = new Vector3(0, h01 * 256, 1024);
                                        var p10 = new Vector3(1024, h10 * 256, 0);
                                        var p11 = new Vector3(1024, h11 * 256, 1024);

                                        var maxHeight = Math.Max(Math.Max(Math.Max(h01, h11), h00), h10);

                                        // Choose which triangle to adjust
                                        if (h01 < Math.Min(h00, h11) && h10 < Math.Min(h00, h11))
                                        {
                                            var pl1 = new Plane(p11, p10, p00);

                                            float distance1;

                                            // Find the 4th point
                                            var ray1 = new Ray(new Vector3(0, 32768, 1024), -Vector3.UnitY);
                                            pl1.Intersects(ref ray1, out distance1);
                                            distance1 = 32768 - (float)Math.Round(distance1);
                                            distance1 /= 256;

                                            t2 = (int)(maxHeight - distance1) & 0x1f;

                                            var pl2 = new Plane(p00, p01, p11);

                                            float distance2;

                                            // Find the 4th point
                                            var ray2 = new Ray(new Vector3(1024, 32768, 0), -Vector3.UnitY);
                                            pl2.Intersects(ref ray2, out distance2);
                                            distance2 = 32768 - (float)Math.Round(distance2);
                                            distance2 /= 256;

                                            t1 = (int)(maxHeight - distance2) & 0x1f;
                                        }
                                        else if ((h11 == maxHeight || h00 == maxHeight || h10 == maxHeight) &&
                                                 h01 < h10)
                                        {
                                            var p = new Plane(p01, p11, p00);

                                            float distance;

                                            // Find the 4th point
                                            var ray = new Ray(new Vector3(1024, 32768, 0), -Vector3.UnitY);
                                            p.Intersects(ref ray, out distance);
                                            distance = 32768 - (float)Math.Round(distance);
                                            distance /= 256;

                                            var maxTriangle = Math.Max(Math.Max(h01, h11), h00);

                                            if (distance <= maxTriangle)
                                            {
                                                t1 = maxHeight - maxTriangle & 0x1f;
                                                t2 = 0;
                                            }
                                            else
                                            {
                                                t1 = (int)(maxHeight - distance) & 0x1f;
                                                t2 = 0;
                                            }
                                        }
                                        else if ((h11 == maxHeight || h00 == maxHeight || h01 == maxHeight) &&
                                                 h10 < h01)
                                        {
                                            var p = new Plane(p11, p10, p00);

                                            float distance;

                                            // Find the 4th point
                                            var ray = new Ray(new Vector3(0, 32768, 1024), -Vector3.UnitY);
                                            p.Intersects(ref ray, out distance);
                                            distance = 32768 - (float)Math.Round(distance);
                                            distance /= 256;

                                            var maxTriangle = Math.Max(Math.Max(h11, h00), h10);

                                            if (distance <= maxTriangle)
                                            {
                                                t1 = 0;
                                                t2 = maxHeight - maxTriangle & 0x1f;
                                            }
                                            else
                                            {
                                                t1 = 0;
                                                t2 = (int)(maxHeight - distance) & 0x1f;
                                            }
                                        }
                                    }

                                    // Now build the floordata codes
                                    var code1 = (ushort)(function | (t2 << 5) | (t1 << 10));
                                    var code2 = (ushort)((h10) | (h00 << 4) | (h01 << 8) | (h11 << 12));

                                    tempCodes.Add(code1);
                                    tempCodes.Add(code2);
                                }
                            }
                        }

                        // If sector has a ceiling slope
                        if (block.CeilingIfQuadSlopeX != 0 || block.CeilingIfQuadSlopeZ != 0)
                        {
                            lastFloorDataFunction = (ushort)tempCodes.Count;
                            tempCodes.Add(0x03);

                            var slope = (ushort)(((block.CeilingIfQuadSlopeZ) << 8) | ((block.CeilingIfQuadSlopeX) & 0xff));

                            tempCodes.Add(slope);
                        }

                        if (block.CeilingDiagonalSplit != DiagonalSplit.None)
                        {
                            if (block.Type != BlockType.Wall)
                            {
                                int w0 = block.WSFaces[0];
                                int w1 = block.WSFaces[1];
                                int w2 = block.WSFaces[2];
                                int w3 = block.WSFaces[3];

                                // The real floor split of the sector
                                int function;

                                int t1;
                                int t2;

                                int h00;
                                int h01;
                                int h10;
                                int h11;

                                // First, we fix the sector height
                                if (block.Type == BlockType.Wall)
                                    sector.Floor = (sbyte)-(room.Position.Y + 0x0f);
                                else
                                    sector.Floor = (sbyte)-(room.Position.Y + block.FloorMax);

                                if (block.CeilingDiagonalSplit == DiagonalSplit.XnZn ||
                                    block.CeilingDiagonalSplit == DiagonalSplit.XpZp)
                                {
                                    lastFloorDataFunction = (ushort)tempCodes.Count;

                                    if (ceilingPortalInfo.TraversableType != Room.RoomConnectionType.NoPortal)
                                    {
                                        function = block.CeilingDiagonalSplit == DiagonalSplit.XnZn ? 0x10 : 0x0f;
                                    }
                                    else
                                    {
                                        function = 0x09;
                                    }

                                    if (block.CeilingDiagonalSplit == DiagonalSplit.XnZn)
                                    {
                                        var lowCorner = w1;
                                        var highCorner = w3;


                                        t1 = 0;
                                        t2 = highCorner - lowCorner & 0x1f;

                                        h00 = w3 - w2;
                                        h10 = 0;
                                        h01 = 0;
                                        h11 = 0;
                                    }
                                    else
                                    {
                                        var lowCorner = w3;
                                        var highCorner = w1;

                                        t1 = highCorner - lowCorner & 0x1f;
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

                                    if (ceilingPortalInfo.TraversableType != Room.RoomConnectionType.NoPortal)
                                    {
                                        function = block.CeilingDiagonalSplit == DiagonalSplit.XpZn ? 0x11 : 0x12;
                                    }
                                    else
                                    {
                                        function = 0x0a;
                                    }

                                    if (block.CeilingDiagonalSplit == DiagonalSplit.XpZn)
                                    {
                                        var lowCorner = w0;
                                        var highCorner = w2;


                                        t1 = highCorner - lowCorner & 0x1f;
                                        t2 = 0;

                                        h00 = 0;
                                        h10 = w2 - w1;
                                        h01 = 0;
                                        h11 = 0;
                                    }
                                    else
                                    {
                                        var lowCorner = w2;
                                        var highCorner = w0;

                                        t1 = 0;
                                        t2 = highCorner - lowCorner & 0x1f;

                                        h00 = 0;
                                        h10 = 0;
                                        h01 = w0 - w1;
                                        h11 = 0;
                                    }
                                }

                                var code1 = (ushort)(function | (t2 << 5) | (t1 << 10));
                                var code2 = (ushort)((h10) | (h00 << 4) | (h01 << 8) | (h11 << 12));

                                tempCodes.Add(code1);
                                tempCodes.Add(code2);
                            }
                        }
                        else
                        {
                            if (block.CeilingIfQuadSlopeX == 0 && block.CeilingIfQuadSlopeZ == 0)
                            {
                                int w0 = block.WSFaces[0];
                                int w1 = block.WSFaces[1];
                                int w2 = block.WSFaces[2];
                                int w3 = block.WSFaces[3];

                                // We have not a slope, so if this is not a horizontal square then we have triangulation
                                if (!Block.IsQuad(w0, w1, w2, w3))
                                {
                                    // We have to find the axis of the triangulation
                                    var max = block.CeilingMax;

                                    lastFloorDataFunction = (ushort)tempCodes.Count;

                                    // Corner heights
                                    var h10 = max - w2;
                                    var h00 = max - w3;
                                    var h01 = max - w0;
                                    var h11 = max - w1;

                                    var t1 = 0;
                                    var t2 = 0;

                                    // The real ceiling split of the sector
                                    int function;

                                    // Now, for each of the two possible splits, apply the algorithm described by meta2tr and
                                    // TRosettaStone 3. I've simply managed some cases by hand. The difficult task is to
                                    // decide if apply the height correction to both triangles or just one of them.
                                    // Function must be decided looking at portals.

                                    if (!block.FloorSplitDirectionIsXEqualsZ)
                                    {
                                        if (ceilingPortalInfo.TraversableType != Room.RoomConnectionType.NoPortal)
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

                                        var p00 = new Vector3(0, h00 * 256, 1024);
                                        var p01 = new Vector3(0, h01 * 256, 0);
                                        var p10 = new Vector3(1024, h10 * 256, 1024);
                                        var p11 = new Vector3(1024, h11 * 256, 0);

                                        var maxHeight = Math.Max(Math.Max(Math.Max(h01, h11), h00), h10);

                                        // Choose which triangle to adjust
                                        if (h00 < Math.Min(h10, h01) && h11 < Math.Min(h10, h01))
                                        {
                                            var pl1 = new Plane(p01, p00, p10);

                                            float distance1;

                                            // Find the 4th point
                                            var ray1 = new Ray(new Vector3(1024, 32768, 0), -Vector3.UnitY);
                                            pl1.Intersects(ref ray1, out distance1);
                                            distance1 = 32768 - (float)Math.Round(distance1);
                                            distance1 /= 256;

                                            t2 = (int)(-maxHeight + distance1) & 0x1f;

                                            var pl2 = new Plane(p10, p11, p01);

                                            float distance2;

                                            // Find the 4th point
                                            var ray2 = new Ray(new Vector3(0, 32768, 1024), -Vector3.UnitY);
                                            pl2.Intersects(ref ray2, out distance2);
                                            distance2 = 32768 - (float)Math.Round(distance2);
                                            distance2 /= 256;

                                            t1 = (int)(-maxHeight + distance2) & 0x1f;
                                        }
                                        else if ((h01 == maxHeight || h00 == maxHeight || h10 == maxHeight) &&
                                                 h11 < h00)
                                        {
                                            var p = new Plane(p01, p10, p11);

                                            float distance;

                                            // Find the 4th point
                                            var ray = new Ray(new Vector3(0, 32768, 1024), -Vector3.UnitY);
                                            p.Intersects(ref ray, out distance);
                                            distance = 32768 - (float)Math.Round(distance);
                                            distance /= 256;

                                            var maxTriangle = Math.Max(Math.Max(h01, h11), h10);

                                            if (distance <= maxTriangle)
                                            {
                                                t1 = 0;
                                                t2 = -maxHeight + maxTriangle & 0x1f;
                                            }
                                            else
                                            {
                                                t1 = 0;
                                                t2 = (int)(-maxHeight + distance) & 0x1f;
                                            }
                                        }
                                        else if ((h01 == maxHeight || h11 == maxHeight || h10 == maxHeight) &&
                                                 h00 < h11)
                                        {
                                            var p = new Plane(p01, p00, p10);

                                            float distance;

                                            // Find the 4th point
                                            var ray = new Ray(new Vector3(1024, 32768, 0), -Vector3.UnitY);
                                            p.Intersects(ref ray, out distance);
                                            distance = 32768 - (float)Math.Round(distance);
                                            distance /= 256;

                                            var maxTriangle = Math.Max(Math.Max(h01, h00), h10);

                                            if (distance <= maxTriangle)
                                            {
                                                t2 = 0;
                                                t1 = -maxHeight + maxTriangle & 0x1f;
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
                                        if (ceilingPortalInfo.TraversableType != Room.RoomConnectionType.NoPortal)
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

                                        var p00 = new Vector3(0, h00 * 256, 1024);
                                        var p01 = new Vector3(0, h01 * 256, 0);
                                        var p10 = new Vector3(1024, h10 * 256, 1024);
                                        var p11 = new Vector3(1024, h11 * 256, 0);

                                        var maxHeight = Math.Max(Math.Max(Math.Max(h01, h11), h00), h10);

                                        // Choose which triangle to adjust
                                        if (h01 < Math.Min(h00, h11) && h10 < Math.Min(h00, h11))
                                        {
                                            var pl1 = new Plane(p11, p00, p10);

                                            float distance1;

                                            // Find the 4th point
                                            var ray1 = new Ray(new Vector3(0, 32768, 0), -Vector3.UnitY);
                                            pl1.Intersects(ref ray1, out distance1);
                                            distance1 = 32768 - (float)Math.Round(distance1);
                                            distance1 /= 256;

                                            t2 = (int)(-maxHeight + distance1) & 0x1f;

                                            var pl2 = new Plane(p00, p11, p01);

                                            float distance2;

                                            // Find the 4th point
                                            var ray2 = new Ray(new Vector3(1024, 32768, 1024), -Vector3.UnitY);
                                            pl2.Intersects(ref ray2, out distance2);
                                            distance2 = 32768 - (float)Math.Round(distance2);
                                            distance2 /= 256;

                                            t1 = (int)(-maxHeight + distance2) & 0x1f;
                                        }
                                        else if ((h11 == maxHeight || h00 == maxHeight || h10 == maxHeight) &&
                                                 h01 < h10)
                                        {
                                            var p = new Plane(p01, p00, p11);

                                            float distance;

                                            // Find the 4th point
                                            var ray = new Ray(new Vector3(1024, 32768, 1024), -Vector3.UnitY);
                                            p.Intersects(ref ray, out distance);
                                            distance = 32768 - (float)Math.Round(distance);
                                            distance /= 256;

                                            var maxTriangle = Math.Max(Math.Max(h01, h11), h00);

                                            if (distance <= maxTriangle)
                                            {
                                                t1 = -maxHeight + maxTriangle & 0x1f;
                                                t2 = 0;
                                            }
                                            else
                                            {
                                                t1 = (int)(-maxHeight + distance) & 0x1f;
                                                t2 = 0;
                                            }
                                        }
                                        else if ((h11 == maxHeight || h00 == maxHeight || h01 == maxHeight) &&
                                                 h10 < h01)
                                        {
                                            var p = new Plane(p11, p00, p10);

                                            float distance;

                                            // Find the 4th point
                                            var ray = new Ray(new Vector3(0, 32768, 0), -Vector3.UnitY);
                                            p.Intersects(ref ray, out distance);
                                            distance = 32768 - (float)Math.Round(distance);
                                            distance /= 256;

                                            var maxTriangle = Math.Max(Math.Max(h11, h00), h10);

                                            if (distance <= maxTriangle)
                                            {
                                                t1 = 0;
                                                t2 = -maxHeight + maxTriangle & 0x1f;
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
                        if ((block.Flags & BlockFlags.ClimbAny) != BlockFlags.None)
                        {
                            ushort climb = 0x06;

                            if (block.HasFlag(BlockFlags.ClimbPositiveZ)) climb |= (ushort)(0x01 << 8);
                            if (block.HasFlag(BlockFlags.ClimbPositiveX)) climb |= (ushort)(0x02 << 8);
                            if (block.HasFlag(BlockFlags.ClimbNegativeZ)) climb |= (ushort)(0x04 << 8);
                            if (block.HasFlag(BlockFlags.ClimbNegativeX)) climb |= (ushort)(0x08 << 8);

                            lastFloorDataFunction = (ushort)tempCodes.Count;
                            tempCodes.Add(climb);
                        }

                        // If sector is Death
                        if (block.HasFlag(BlockFlags.DeathFire))
                        {
                            lastFloorDataFunction = (ushort)tempCodes.Count;
                            tempCodes.Add(0x05);
                        }

                        // If sector is Monkey
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
                            TriggerInstance found = null;

                            // First, I search a special trigger, if exists
                            foreach (var trigger in room.Blocks[x, z].Triggers)
                            {
                                if (trigger.TriggerType == TriggerType.Trigger && found == null)
                                {
                                    // Normal trigger can be used only if in the sector there aren't special triggers
                                    found = trigger;
                                    continue;
                                }

                                if (trigger.TriggerType == TriggerType.Trigger)
                                    continue;

                                // For now I use the first special trigger of the chain, ignoring the following triggers
                                found = trigger;
                                break;
                            }

                            var tempTriggers = new List<TriggerInstance> { found };
                            tempTriggers.AddRange(room.Blocks[x, z].Triggers.Where(trigger => trigger != found));

                            {
                                lastFloorDataFunction = (ushort)tempCodes.Count;

                                // Trigger type and setup are coming from the found trigger. Other triggers are needed only for action.
                                ushort trigger1 = 0x04;
                                switch (found.TriggerType)
                                {
                                    case TriggerType.Trigger:
                                        trigger1 |= 0x00 << 8;
                                        break;
                                    case TriggerType.Pad:
                                        trigger1 |= 0x01 << 8;
                                        break;
                                    case TriggerType.Switch:
                                        trigger1 |= 0x02 << 8;
                                        break;
                                    case TriggerType.Key:
                                        trigger1 |= 0x03 << 8;
                                        break;
                                    case TriggerType.Pickup:
                                        trigger1 |= 0x04 << 8;
                                        break;
                                    case TriggerType.Heavy:
                                        trigger1 |= 0x05 << 8;
                                        break;
                                    case TriggerType.Antipad:
                                        trigger1 |= 0x06 << 8;
                                        break;
                                    case TriggerType.Combat:
                                        trigger1 |= 0x07 << 8;
                                        break;
                                    case TriggerType.Dummy:
                                        trigger1 |= 0x08 << 8;
                                        break;
                                    case TriggerType.Antitrigger:
                                        trigger1 |= 0x09 << 8;
                                        break;
                                    case TriggerType.HeavySwitch:
                                        trigger1 |= 0x0a << 8;
                                        break;
                                    case TriggerType.HeavyAntritrigger:
                                        trigger1 |= 0x0b << 8;
                                        break;
                                    case TriggerType.ConditionNg:
                                        trigger1 |= 0x0c << 8;
                                        break;
                                    default:
                                        throw new Exception("Unknown trigger type found '" + found.TriggerType + "' room '" + room + "' at trigger '" + found + "'");
                                }

                                ushort triggerSetup = 0;
                                if (_level.Settings.GameVersion == GameVersion.TRNG)
                                {
                                    // NG flipeffects store timer and extra in additional ushort
                                    if (found.TargetType == TriggerTargetType.FlipEffect)
                                        triggerSetup |= (ushort)0;
                                    // NG condition trigger uses timer in low byte and extra stored as bits in the high byte
                                    else if (found.TriggerType == TriggerType.ConditionNg)
                                        triggerSetup |= (ushort)found.Timer;
                                    // all other triggers work as usual
                                    else
                                        triggerSetup |= (ushort)(found.Timer & 0xff);
                                }
                                else
                                    triggerSetup |= (ushort)(found.Timer & 0xff);
                                triggerSetup |= (ushort)(found.OneShot ? 0x100 : 0);
                                triggerSetup |= (ushort)((found.CodeBits & 0x1f) << 9);

                                tempCodes.Add(trigger1);
                                tempCodes.Add(triggerSetup);
                            }

                            foreach (var trigger in tempTriggers)
                            {
                                ushort trigger2;

                                switch (trigger.TargetType)
                                {
                                    case TriggerTargetType.Object:
                                        // Trigger for object
                                        if (trigger.TargetObj == null)
                                            break;
                                        var object_ = trigger.CastTargetType<MoveableInstance>(room);
                                        bool isAI = object_.WadObjectId >= 398 && object_.WadObjectId <= 406;
                                        int item = (isAI ? _aiObjectsTable : _moveablesTable)[object_];
                                        trigger2 = (ushort)(item & 0x3ff | (0 << 10));
                                        tempCodes.Add(trigger2);
                                        break;
                                    case TriggerTargetType.Camera:
                                        // Trigger for camera
                                        if (trigger.TargetObj == null)
                                            break;
                                        var camera = trigger.CastTargetType<CameraInstance>(room);
                                         trigger2 = (ushort)(_cameraTable[camera] & 0x3ff | (1 << 10));
                                        tempCodes.Add(trigger2);

                                        // Additional short
                                        ushort trigger3 = 0;
                                        trigger3 |= (ushort)(trigger.Timer & 0xff);
                                        trigger3 |= (ushort)(trigger.OneShot ? 0x100 : 0);
                                        tempCodes.Add(trigger3);
                                        break;
                                    case TriggerTargetType.Sink:
                                        // Trigger for sink
                                        if (trigger.TargetObj == null)
                                            break;
                                        var sink = trigger.CastTargetType<SinkInstance>(room);
                                        trigger2 = (ushort)(_sinkTable[sink] & 0x3ff | (2 << 10));
                                        tempCodes.Add(trigger2);
                                        break;
                                    case TriggerTargetType.FlipMap:
                                        // Trigger for flip map
                                        trigger2 = (ushort)(trigger.TargetData & 0x3ff | (3 << 10));
                                        tempCodes.Add(trigger2);
                                        break;
                                    case TriggerTargetType.FlipOn:
                                        // Trigger for flip map on
                                        trigger2 = (ushort)(trigger.TargetData & 0x3ff | (4 << 10));
                                        tempCodes.Add(trigger2);
                                        break;
                                    case TriggerTargetType.FlipOff:
                                        // Trigger for flip map off
                                        trigger2 = (ushort)(trigger.TargetData & 0x3ff | (5 << 10));
                                        tempCodes.Add(trigger2);
                                        break;
                                    case TriggerTargetType.Target:
                                        // Trigger for look at item
                                        if (trigger.TargetObj == null)
                                            break;
                                        var target = trigger.CastTargetType<MoveableInstance>(room);
                                        trigger2 = (ushort)(_moveablesTable[target] & 0x3ff | (6 << 10));
                                        tempCodes.Add(trigger2);
                                        break;
                                    case TriggerTargetType.FinishLevel:
                                        // Trigger for finish level
                                        trigger2 = (ushort)(trigger.TargetData & 0x3ff | (7 << 10));
                                        tempCodes.Add(trigger2);
                                        break;
                                    case TriggerTargetType.PlayAudio:
                                        // Trigger for play soundtrack
                                        trigger2 = (ushort)(trigger.TargetData & 0x3ff | (8 << 10));
                                        tempCodes.Add(trigger2);
                                        break;
                                    case TriggerTargetType.FlipEffect:
                                        // Trigger for flip effect
                                        trigger2 = (ushort)(trigger.TargetData & 0x3ff | (9 << 10));
                                        tempCodes.Add(trigger2);

                                        // TRNG stores flipeffect timer as an extra ushort
                                        if (_level.Settings.GameVersion == GameVersion.TRNG)
                                        {
                                            trigger2 = (ushort)(trigger.Timer & 0x7fff);
                                            tempCodes.Add(trigger2);
                                        }

                                        break;
                                    case TriggerTargetType.Secret:
                                        // Trigger for secret found
                                        trigger2 = (ushort)(trigger.TargetData & 0x3ff | (10 << 10));
                                        tempCodes.Add(trigger2);
                                        break;
                                    case TriggerTargetType.ActionNg:
                                        // Trigger for action
                                        if (trigger.TargetObj == null)
                                            break;
                                        if (_level.Settings.GameVersion == GameVersion.TRNG)
                                        {
                                            var objectAction = trigger.CastTargetType<MoveableInstance>(room);
                                            bool isAIaction = objectAction.WadObjectId >= 398 && objectAction.WadObjectId <= 406;
                                            int itemAction = (isAIaction ? _aiObjectsTable : _moveablesTable)[objectAction];
                                            trigger2 = (ushort)(itemAction & 0x3ff | (11 << 10));
                                            tempCodes.Add(trigger2);

                                            trigger2 = (ushort)(trigger.Timer & 0x7fff);
                                            tempCodes.Add(trigger2);
                                        }
                                        else
                                            _progressReporter.ReportWarn("Level uses action trigger which is not supported in this game engine.");
                                        break;
                                    case TriggerTargetType.FlyByCamera:
                                        // Trigger for fly by
                                        if (trigger.TargetObj == null)
                                            break;
                                        var flyByCamera = trigger.CastTargetType<FlybyCameraInstance>(room);
                                        trigger2 = (ushort)(flyByCamera.Sequence & 0x3ff | (12 << 10));
                                        tempCodes.Add(trigger2);

                                        trigger2 = (ushort)(trigger.OneShot ? 0x0100 : 0x00);
                                        tempCodes.Add(trigger2);
                                        break;
                                    case TriggerTargetType.ParameterNg:
                                        // Trigger for secret found
                                        trigger2 = (ushort)(trigger.TargetData & 0x3ff | (13 << 10));
                                        tempCodes.Add(trigger2);
                                        break;
                                    case TriggerTargetType.FmvNg:
                                        // Trigger for secret found
                                        trigger2 = (ushort)(trigger.TargetData & 0x3ff | (14 << 10));
                                        tempCodes.Add(trigger2);
                                        break;
                                    default:
                                        throw new Exception("Unknown trigger type found '" + trigger.TargetType + "' in room '" + room + "' at trigger '" + trigger + "'");
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
                            _floorData.AddRange(tempCodes);
                        }

                        // Update the sector
                        SaveSector(tempRoom, x, z, sector);
                    }
                }
            }
            
            ReportProgress(80, "    Floordata size: " + _floorData.Count * 2 + " bytes");
        }
    }
}
