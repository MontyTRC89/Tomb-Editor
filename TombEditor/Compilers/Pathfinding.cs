using System;
using System.Collections.Generic;
using System.Linq;
using TombEditor.Geometry;

namespace TombEditor.Compilers
{
    public partial class LevelCompilerTR4
    {
        private void BuildPathFindingData()
        {
            ReportProgress(50, "Building pathfinding data");

            // Fix monkey on portals
            for (int i = 0; i < Rooms.Length; i++)
            {
                tr_room fixRoom = Rooms[i];

                for (int x = 0; x < fixRoom.NumXSectors; x++)
                {
                    for (int z = 0; z < fixRoom.NumZSectors; z++)
                    {
                        if (fixRoom.AuxSectors[x, z].FloorPortal != -1)
                        {
                            Rooms[i].AuxSectors[x, z].Monkey = FindMonkeyFloor(i, x, z);
                        }
                    }
                }
            }

            // Build boxes
            tempBoxes = new List<tr_box_aux>();

            // First build boxes except portal boxes
            for (var i = 0; i < Rooms.Length; i++)
            {
                var room = Rooms[i];

                for (var x = 1; x < room.NumXSectors - 1; x++)
                {
                    for (var z = 1; z < room.NumZSectors - 1; z++)
                    {
                        var sector = GetSector(i, x, z);
                        var aux = room.AuxSectors[x, z];

                        // If this room is excluded from pathfinding or this sector is a Not Walkable Floor
                        if (_level.Rooms[room.OriginalRoomId].ExcludeFromPathFinding || aux.NotWalkableFloor)
                        {
                            sector.BoxIndex = (short)(0x7ff0 | (byte)room.TextureSounds[x, z]);
                            SaveSector(i, x, z, sector);

                            continue;
                        }

                        if (aux.Wall || aux.Portal || aux.HardSlope) continue;

                        // Build the box
                        tr_box_aux box;
                        var result = BuildBox(i, x, z, 0, 0, 0, 0, out box);
                        if (!result) continue;

                        // Check if box exists
                        var found = -1;
                        for (var j = 0; j < tempBoxes.Count; j++)
                        {
                            var box2 = tempBoxes[j];

                            var r1 = Rooms[box.Room];
                            var r2 = Rooms[box2.Room];

                            if (box.Xmin == box2.Xmin && box.Xmax == box2.Xmax && box.Zmin == box2.Zmin && box.Zmax == box2.Zmax &&
                                (box.Room == box2.Room || (box.Room != box2.Room && (r1.BaseRoom == r2.FlippedRoom || r1.FlippedRoom == r2.BaseRoom))) &&
                                box.TrueFloor == box2.TrueFloor)
                            {
                                found = j;
                                break;
                            }
                        }

                        // If box is not found, then add the new box
                        if (found == -1)
                        {
                            tempBoxes.Add(box);

                            for (int x2 = box.Xmin; x2 < box.Xmax; x2++)
                            {
                                for (int z2 = box.Zmin; z2 < box.Zmax; z2++)
                                {
                                    var xc = x2 - room.Info.X / 1024;
                                    var zc = z2 - room.Info.Z / 1024;

                                    var sect = GetSector(i, xc, zc);
                                    var aux2 = room.AuxSectors[xc, zc];

                                    if (aux2.Wall)
                                    {
                                        sect.BoxIndex = 0x7ff6;
                                    }
                                    else
                                    {
                                        sect.BoxIndex = (short)(((tempBoxes.Count - 1) << 4) | (byte)room.TextureSounds[xc, zc]);
                                    }

                                    SaveSector(i, xc, zc, sect);
                                }
                            }
                        }
                        else
                        {
                            for (int x2 = box.Xmin; x2 < box.Xmax; x2++)
                            {
                                for (int z2 = box.Zmin; z2 < box.Zmax; z2++)
                                {
                                    var xc = x2 - room.Info.X / 1024;
                                    var zc = z2 - room.Info.Z / 1024;

                                    var sect = GetSector(i, xc, zc);
                                    var aux2 = room.AuxSectors[xc, zc];

                                    if (aux2.Wall)
                                    {
                                        sect.BoxIndex = 0x7ff6;
                                    }
                                    else
                                    {
                                        sect.BoxIndex = (short)(((found) << 4) | (byte)room.TextureSounds[xc, zc]);
                                    }

                                    SaveSector(i, xc, zc, sect);
                                }
                            }
                        }
                    }
                }
            }

            // Now build only boxes of horizontal portals
            for (var i = 0; i < Rooms.Length; i++)
            {
                var room = Rooms[i];

                for (var x = 1; x < room.NumXSectors - 1; x++)
                {
                    for (var z = 1; z < room.NumZSectors - 1; z++)
                    {
                        var sector = GetSector(i, x, z);
                        var aux = room.AuxSectors[x, z];

                        // If this room is excluded from pathfinding or this sector is a Not Walkable Floor
                        if (_level.Rooms[room.OriginalRoomId].ExcludeFromPathFinding || aux.NotWalkableFloor)
                        {
                            sector.BoxIndex = (short)(0x7ff0 | (byte)room.TextureSounds[x, z]);
                            SaveSector(i, x, z, sector);

                            continue;
                        }

                        if (aux.FloorPortal == -1) continue;

                        var xMin = room.Info.X / 1024 + _level.Portals[aux.FloorPortal].X;
                        var xMax = room.Info.X / 1024 + _level.Portals[aux.FloorPortal].X + _level.Portals[aux.FloorPortal].NumXBlocks;
                        var zMin = room.Info.Z / 1024 + _level.Portals[aux.FloorPortal].Z;
                        var zMax = room.Info.Z / 1024 + _level.Portals[aux.FloorPortal].Z + _level.Portals[aux.FloorPortal].NumZBlocks;

                        var idRoom = i;

                        // Find the lowest room and floor
                        int room2;
                        short floor2;
                        var result = GetMostDownFloorAndRoom(idRoom, x, z, out room2, out floor2);
                        if (!result) continue;

                        // Build the box
                        tr_box_aux box;
                        result = BuildBox(room2, x + Rooms[i].Info.X / 1024 - Rooms[room2].Info.X / 1024,
                                                 z + Rooms[i].Info.Z / 1024 - Rooms[room2].Info.Z / 1024,
                                                 xMin, xMax, zMin, zMax, out box);
                        box.Room = (short)room2;
                        if (!result) continue;

                        // Check if box exists
                        var found = -1;
                        for (var j = 0; j < tempBoxes.Count; j++)
                        {
                            var box2 = tempBoxes[j];

                            var r1 = Rooms[box.Room];
                            var r2 = Rooms[box2.Room];

                            if (box.Xmin == box2.Xmin && box.Xmax == box2.Xmax && box.Zmin == box2.Zmin && box.Zmax == box2.Zmax &&
                                (box.Room == box2.Room || (box.Room != box2.Room && (r1.BaseRoom == r2.FlippedRoom || r1.FlippedRoom == r2.BaseRoom))) &&
                                box.TrueFloor == box2.TrueFloor)
                            {
                                found = j;
                                break;
                            }
                        }

                        // If box is not found, then add the new box
                        if (found == -1)
                        {
                            box.TrueFloor = GetMostDownFloor(i, x, z);

                            tempBoxes.Add(box);
                            found = tempBoxes.Count - 1;
                        }

                        for (int x2 = box.Xmin; x2 < box.Xmax; x2++)
                        {
                            for (int z2 = box.Zmin; z2 < box.Zmax; z2++)
                            {
                                var xc = x2 - room.Info.X / 1024;
                                var zc = z2 - room.Info.Z / 1024;

                                var sect = GetSector(i, xc, zc);
                                var aux2 = room.AuxSectors[xc, zc];

                                if (aux.FloorPortal == aux2.FloorPortal)
                                {
                                    sect.BoxIndex = (short)((found << 4) | (byte)room.TextureSounds[xc, zc]);
                                    SaveSector(i, xc, zc, sect);
                                }
                            }
                        }
                    }
                }
            }

            // Build overlaps
            var tempOverlaps = new List<tr_overlap_aux>();

            for (var i = 0; i < tempBoxes.Count; i++)
            {
                var box = tempBoxes[i];

                var foundOverlaps = false;
                var baseOverlaps = (short)tempOverlaps.Count;

                for (var j = 0; j < tempBoxes.Count; j++)
                {
                    // if they are the same box don't do anything
                    if (i == j) continue;

                    tr_box_aux box2 = tempBoxes[j];

                    // Now we have to find overlaps and edges
                    bool jump;

                    if (!BoxesOverlap(i, j, out jump))
                        continue;
                    
                    var overlap = new tr_overlap_aux();
                    overlap.Box = j;
                    overlap.IsEdge = (box.Xmax == box2.Xmin || box.Zmax == box2.Zmin || box.Xmin == box2.Xmax || box.Zmin == box2.Zmax);
                    overlap.Monkey = box2.Monkey;
                    overlap.MainBox = i;

                    tempOverlaps.Add(overlap);

                    if (box.Jump == false) box.Jump = jump;
                    if (box2.Jump == false) box2.Jump = jump;

                    tempBoxes[j] = box2;

                    foundOverlaps = true;
                }

                box.OverlapIndex = (short) (foundOverlaps ? baseOverlaps : 2047);

                tempBoxes[i] = box;

                if (!foundOverlaps) continue;

                if (box.IsolatedBox) box.OverlapIndex = (short)(box.OverlapIndex | 0x8000);

                // Mark the end of the list
                var last = tempOverlaps[tempOverlaps.Count - 1];
                last.EndOfList = true;
                tempOverlaps[tempOverlaps.Count - 1] = last;
            }

            // Build final overlaps
            Overlaps = new ushort[tempOverlaps.Count];
            for (var i = 0; i < tempOverlaps.Count; i++)
            {
                var ov = (ushort)tempOverlaps[i].Box;

                // Is the last overlap of the list?
                if (tempOverlaps[i].EndOfList) ov |= 0x8000;

                // Monkey flag
                var canMonkey = tempBoxes[tempOverlaps[i].Box].Monkey && tempBoxes[tempOverlaps[i].MainBox].Monkey;
                if (canMonkey) ov |= 0x2000;

                var canJump = tempBoxes[tempOverlaps[i].Box].Jump;
                if (canJump) ov |= 0x800;

                Overlaps[i] = ov;
            }

            Boxes = new tr_box[tempBoxes.Count];
            Zones = new tr_zone[tempBoxes.Count];

            // Convert boxes to TR format
            for (var i = 0; i < tempBoxes.Count; i++)
            {
                var newBox = new tr_box();
                var aux = tempBoxes[i];
                var zone = new tr_zone();

                newBox.Xmin = aux.Xmin;
                newBox.Xmax = aux.Xmax;
                newBox.Zmin = aux.Zmin;
                newBox.Zmax = aux.Zmax;
                newBox.TrueFloor = aux.TrueFloor;
                newBox.OverlapIndex = aux.OverlapIndex;

                Boxes[i] = newBox;
                Zones[i] = zone;
            }

            // Finally, build zones
            ushort groundZone1 = 1;
            ushort groundZone2 = 1;
            ushort groundZone3 = 1;
            ushort groundZone4 = 1;
            ushort flyZone = 1;
            ushort aGroundZone1 = 1;
            ushort aGroundZone2 = 1;
            ushort aGroundZone3 = 1;
            ushort aGroundZone4 = 1;
            ushort aFlyZone = 1;

            for (var i = 0; i < Boxes.Length; i++)
            {
                // Skeleton like enemis: in the future implement also jump
                if (Zones[i].GroundZone1_Normal == 0)
                {
                    Zones[i].GroundZone1_Normal = groundZone1;

                    foreach (var box in GetAllReachableBoxes(i, 1))
                    {
                        Zones[box].GroundZone1_Normal = groundZone1;
                    }

                    groundZone1++;
                }

                // Mummy like enemis: the simplest case
                if (Zones[i].GroundZone2_Normal == 0)
                {
                    Zones[i].GroundZone2_Normal = groundZone2;

                    foreach (var box in GetAllReachableBoxes(i, 2))
                    {
                        Zones[box].GroundZone2_Normal = groundZone2;
                    }

                    groundZone2++;
                }

                // Crocodile like enemis: like 1 & 2 but they can go inside water and swim
                if (Zones[i].GroundZone3_Normal == 0)
                {
                    Zones[i].GroundZone3_Normal = groundZone3;

                    foreach (var box in GetAllReachableBoxes(i, 3))
                    {
                        Zones[box].GroundZone3_Normal = groundZone3;
                    }

                    groundZone3++;
                }

                // Baddy like enemis: they can jump, grab and monkey
                if (Zones[i].GroundZone4_Normal == 0)
                {
                    Zones[i].GroundZone4_Normal = groundZone4;

                    foreach (var box in GetAllReachableBoxes(i, 4))
                    {
                        Zones[box].GroundZone4_Normal = groundZone4;
                    }

                    groundZone4++;
                }

                // Bat like enemis: they can fly everywhere, except into the water
                if (Zones[i].FlyZone_Normal == 0)
                {
                    Zones[i].FlyZone_Normal = flyZone;

                    foreach (var box in GetAllReachableBoxes(i, 5))
                    {
                        Zones[box].FlyZone_Normal = flyZone;
                    }

                    flyZone++;
                }

                // Flipped rooms------------------------------------------

                // Skeleton like enemis: in the future implement also jump
                if (Zones[i].GroundZone1_Alternate == 0)
                {
                    Zones[i].GroundZone1_Alternate = aGroundZone1;

                    foreach (var box in GetAllReachableBoxes(i, 101))
                    {
                        Zones[box].GroundZone1_Alternate = aGroundZone1;
                    }

                    aGroundZone1++;
                }

                // Mummy like enemis: the simplest case
                if (Zones[i].GroundZone2_Alternate == 0)
                {
                    Zones[i].GroundZone2_Alternate = aGroundZone2;

                    foreach (var box in GetAllReachableBoxes(i, 102))
                    {
                        Zones[box].GroundZone2_Alternate = aGroundZone2;
                    }

                    aGroundZone2++;
                }

                // Crocodile like enemis: like 1 & 2 but they can go inside water and swim
                if (Zones[i].GroundZone3_Alternate == 0)
                {
                    Zones[i].GroundZone3_Alternate = aGroundZone3;

                    foreach (var box in GetAllReachableBoxes(i, 103))
                    {
                        Zones[box].GroundZone3_Alternate = aGroundZone3;
                    }

                    aGroundZone3++;
                }

                // Baddy like enemis: they can jump, grab and monkey
                if (Zones[i].GroundZone4_Alternate == 0)
                {
                    Zones[i].GroundZone4_Alternate = aGroundZone4;

                    foreach (var box in GetAllReachableBoxes(i, 104))
                    {
                        Zones[box].GroundZone4_Alternate = aGroundZone4;
                    }

                    aGroundZone4++;
                }

                // Bat like enemis: they can fly everywhere, except into the water
                if (Zones[i].FlyZone_Alternate != 0)
                    continue;

                Zones[i].FlyZone_Alternate = aFlyZone;

                foreach (var box in GetAllReachableBoxes(i, 105))
                {
                    Zones[box].FlyZone_Alternate = aFlyZone;
                }

                aFlyZone++;
            }

            NumBoxes = (uint)Boxes.Length;
            NumOverlaps = (uint)Overlaps.Length;


            ReportProgress(60, "    Number of boxes: " + NumBoxes);
            ReportProgress(60, "    Number of overlaps: " + NumOverlaps);
            ReportProgress(60, "    Number of zones: " + NumBoxes);
        }
        
        private bool BoxesOverlap(int b1, int b2, out bool jump)
        {
            jump = false;

            var a = tempBoxes[b1];
            var b = tempBoxes[b2];

            // Check if there's overlapping and store edge and type

            var zOverlap = a.Zmin >= b.Zmin && a.Zmin <= b.Zmax || b.Zmin >= a.Zmin && b.Zmin <= a.Zmax;
            var xOverlap = a.Xmin >= b.Xmin && a.Xmin <= b.Xmax || b.Xmin >= a.Xmin && b.Xmin <= a.Xmax;

            var jumpX = CheckIfCanJumpX(b1, b2);
            var jumpZ = CheckIfCanJumpZ(b1, b2);
            
            if (jumpX || jumpZ)
            {
                jump = true;
                return true;
            }

            // If no overlapping then don't execute the rest of the function
            var overlapping = xOverlap && zOverlap;
            if (!overlapping) return false;

            // Boxes that are touching on corners are not overlapping
            if ((a.Xmax == b.Xmin && a.Zmax == b.Zmin) ||
                (a.Xmax == b.Xmin && a.Zmin == b.Zmax) ||
                (a.Xmin == b.Xmax && a.Zmax == b.Zmin) ||
                (a.Xmin == b.Xmax && a.Zmin == b.Zmax))
            {
                return false;
            }

            // If boxes are overlapping and the rooms are the same, then boxes overlap
            if (a.Room == b.Room)
                return true;
            
            // Otherwise, we must check if rooms are vertically reachable with a chain of rooms and portals

            for (var x = a.Xmin; x <= a.Xmax; x++)
            {
                for (var z = a.Zmin - 1; z <= a.Zmax - 1; z++)
                {
                    var r1 = Rooms[a.Room];
                    var r2 = Rooms[b.Room];

                    if (a.Room != b.Room && (IsVerticallyReachable(a.Room, b.Room) ||
                        r1.BaseRoom == r2.FlippedRoom || r1.FlippedRoom == r2.BaseRoom))
                    {
                        return true;
                    }
                }
            }

            return false;
        }
        
        private bool CheckIfCanJumpX(int box1, int box2)
        {
            var a = tempBoxes[box1];
            var b = tempBoxes[box2];

            // Boxes must have the same height for jump
            if (a.TrueFloor != b.TrueFloor) return false;

            if (a.Xmax == b.Xmin || b.Xmax == a.Xmin) return false;

            if (a.Zmax < b.Zmin || a.Zmin > b.Zmax) return false;

            // Get min & max Z
            int zMin = a.Zmin;
            if (a.Zmin <= b.Zmin) zMin = b.Zmin;

            int zMax = a.Zmax;
            if (a.Zmax >= b.Zmax) zMax = b.Zmax;

            // Calculate mean Z
            int zMean = (zMin + zMax) >> 1;

            int roomIndex = 0;
            int destRoom = 0;
            int currentZ = zMean;

            int floor = 0;
            int xMax = 0;

            tr_room currentRoom;

            int xRoomPosition = 0;
            int zRoomPosition = 0;

            int xInRoom = 0;
            int zInRoom = 0;

            if (b.Xmax == a.Xmin - 1 || b.Xmax == a.Xmin - 2)
            {
                xMax = b.Xmax;
                roomIndex = b.Room;
            }

            if (a.Xmax == b.Xmin - 1 || a.Xmax == b.Xmin - 2)
            {
                xMax = a.Xmax;
                roomIndex = a.Room;
            }

            // If the gap is of 1 sector
            if (b.Xmax == a.Xmin - 1 || a.Xmax == b.Xmin - 1)
            {
                // If X, Zmax - 1 can't be reached then quit the function
                if (!CanSectorBeReachedAndIsSolid(roomIndex, xMax - 1, currentZ, out destRoom))
                    return false;

                if (!CanSectorBeReachedAndIsSolid(roomIndex, xMax, currentZ, out destRoom))
                    return false;
                
                currentRoom = Rooms[destRoom];

                xRoomPosition = (int)(currentRoom.Info.X / 1024.0f);
                zRoomPosition = (int)(currentRoom.Info.Z / 1024.0f);

                xInRoom = xMax - xRoomPosition;
                zInRoom = currentZ - zRoomPosition;

                floor = GetBoxFloorHeight(destRoom, xInRoom, zInRoom);

                // Enemy can jump to final box if its height is lower than the starting box
                return -floor <= -b.TrueFloor - 512 && floor != 0x7fff;
            }

            // If the gap is of 2 sectors
            if (b.Xmax != a.Xmin - 2 && a.Xmax != b.Xmin - 2)
                return false;

            if (!CanSectorBeReachedAndIsSolid(roomIndex, xMax - 1, currentZ, out destRoom))
                return false;

            if (!CanSectorBeReachedAndIsSolid(roomIndex, xMax, currentZ, out destRoom))
                return false;
            
            currentRoom = Rooms[destRoom];

            xRoomPosition = (int)(currentRoom.Info.X / 1024.0f);
            zRoomPosition = (int)(currentRoom.Info.Z / 1024.0f);

            xInRoom = xMax - xRoomPosition;
            zInRoom = currentZ - zRoomPosition;

            floor = GetBoxFloorHeight(destRoom, xInRoom, zInRoom);

            if (-floor > -b.TrueFloor - 512 || floor == 0x7fff)
                return false;

            if (!CanSectorBeReachedAndIsSolid(roomIndex, xMax + 1, currentZ, out destRoom))
                return false;
            
            currentRoom = Rooms[destRoom];

            xRoomPosition = (int)(currentRoom.Info.X / 1024.0f);
            zRoomPosition = (int)(currentRoom.Info.Z / 1024.0f);

            xInRoom = xMax + 1 - xRoomPosition;
            zInRoom = currentZ - zRoomPosition;

            floor = GetBoxFloorHeight(destRoom, xInRoom, zInRoom);

            //floor = GetBoxFloorHeight(destRoom, xMax + 1, currentZ);
            return -floor <= -b.TrueFloor - 512 && floor != 0x7FFF;
        }

        private bool CheckIfCanJumpZ(int box1, int box2)
        {
            var a = tempBoxes[box1];
            var b = tempBoxes[box2];

            // Boxes must have the same height for jump
            if (a.TrueFloor != b.TrueFloor) return false;

            if (a.Zmax == b.Zmin || b.Zmax == a.Zmin) return false;

            if (a.Xmax < b.Xmin || a.Xmin > b.Xmax) return false;

            // Get min & max X
            int xMin = a.Xmin;
            if (a.Xmin <= b.Xmin) xMin = b.Xmin;

            int xMax = a.Xmax;
            if (a.Xmax >= b.Xmax) xMax = b.Xmax;

            // Calculate mean X
            int xMean = (xMin + xMax) >> 1;

            int roomIndex = 0; ;

            int currentX = xMean;

            int floor = 0;
            int zMax = 0;

            if (b.Zmax == a.Zmin - 1 || b.Zmax == a.Zmin - 2)
            {
                zMax = b.Zmax;
                roomIndex = b.Room;
            }

            if (a.Zmax == b.Zmin - 1 || a.Zmax == b.Zmin - 2)
            {
                zMax = a.Zmax;
                roomIndex = a.Room;
            }

            int destRoom = 0;

            tr_room currentRoom;

            int xRoomPosition = 0;
            int zRoomPosition = 0;

            int xInRoom = 0;
            int zInRoom = 0;

            // If the gap is of 1 sector
            if (b.Zmax == a.Zmin - 1 || a.Zmax == b.Zmin - 1)
            {
                // If X, Zmax - 1 can't be reached then quit the function
                if (!CanSectorBeReachedAndIsSolid(roomIndex, currentX, zMax - 1, out destRoom))
                    return false;

                if (!CanSectorBeReachedAndIsSolid(roomIndex, currentX, zMax, out destRoom))
                    return false;
                
                currentRoom = Rooms[destRoom];

                xRoomPosition = (int)(currentRoom.Info.X / 1024.0f);
                zRoomPosition = (int)(currentRoom.Info.Z / 1024.0f);

                xInRoom = currentX - xRoomPosition;
                zInRoom = zMax - zRoomPosition;

                floor = GetBoxFloorHeight(destRoom, xInRoom, zInRoom);

                //  floor = GetBoxFloorHeight(destRoom, currentX, zMax);

                // Enemy can jump to final box if its height is lower than the starting box
                return -floor <= -b.TrueFloor - 512 && floor != 0x7fff;
            }

            // If the gap is of 2 sectors
            if (b.Zmax != a.Zmin - 2 && a.Zmax != b.Zmin - 2)
                return false;

            if (!CanSectorBeReachedAndIsSolid(roomIndex, currentX, zMax - 1, out destRoom))
                return false;

            if (!CanSectorBeReachedAndIsSolid(roomIndex, currentX, zMax, out destRoom))
                return false;
            
            currentRoom = Rooms[destRoom];

            xRoomPosition = (int)(currentRoom.Info.X / 1024.0f);
            zRoomPosition = (int)(currentRoom.Info.Z / 1024.0f);

            xInRoom = currentX - xRoomPosition;
            zInRoom = zMax - zRoomPosition;

            floor = GetBoxFloorHeight(destRoom, xInRoom, zInRoom);

            // floorHeight = GetBoxFloorHeight(destRoom, currentX, zMax);
            if (-floor > -b.TrueFloor - 512 || floor == 0x7fff)
                return false;

            if (!CanSectorBeReachedAndIsSolid(roomIndex, currentX, zMax + 1, out destRoom))
                return false;
            
            currentRoom = Rooms[destRoom];

            xRoomPosition = (int)(currentRoom.Info.X / 1024.0f);
            zRoomPosition = (int)(currentRoom.Info.Z / 1024.0f);

            xInRoom = currentX - xRoomPosition;
            zInRoom = zMax + 1 - zRoomPosition;

            floor = GetBoxFloorHeight(destRoom, xInRoom, zInRoom);

            //floorHeight = GetBoxFloorHeight(destRoom, currentX, zMax + 1);
            return -floor <= -b.TrueFloor - 512 && floor != 0x7FFF;
        }
        
        private int GetBoxFloorHeight(int room, int x, int z)
        {
            return GetMostDownFloor(room, x, z);

            int roomIndex = room;

            tr_room currentRoom = Rooms[roomIndex];
            Room editorRoom = _level.Rooms[currentRoom.OriginalRoomId];

            int positionX = (int)(currentRoom.Info.X / 1024.0f);
            int positionZ = (int)(currentRoom.Info.Z / 1024.0f);

            int xInRoom = x - positionX;
            int zInRoom = z - positionZ;

            int height0 = -1;
            int height1 = -1;
            int height2 = -1;
            int height3 = -1;

            byte slope1 = 0;
            byte slope2 = 0;
            byte slope3 = 0;
            byte slope4 = 0;

            if (xInRoom < 0 || xInRoom > currentRoom.NumXSectors - 1 || zInRoom < 0 || zInRoom > currentRoom.NumZSectors - 1)
            {
                return 0x7fff;
            }

            Block block = editorRoom.Blocks[xInRoom, zInRoom];

            // If block is a wall or is a vertical toggle opacity 1
            if ((block.Type == BlockType.BorderWall || block.Type == BlockType.Wall) && block.WallOpacity == PortalOpacity.Opacity1)
            {
                return 0x7fff;
            }

            // If it's not a wall portal or is vertical toggle opacity 1
            if (!(block.WallPortal == -1 || block.WallOpacity == PortalOpacity.Opacity1))
            {
                // Is a wall portal if I'm here
                Portal portal = _editor.Level.Portals[editorRoom.Blocks[xInRoom, zInRoom].WallPortal];

                roomIndex = _roomsIdTable[portal.AdjoiningRoom];

                currentRoom = Rooms[roomIndex];
                editorRoom = _level.Rooms[currentRoom.OriginalRoomId];

                positionX = (int)(currentRoom.Info.X / 1024.0f);
                positionZ = (int)(currentRoom.Info.Z / 1024.0f);

                xInRoom = x - positionX;
                zInRoom = z - positionZ;

                block = editorRoom.Blocks[xInRoom, zInRoom];
            }

            bool isFloorPortal = block.FloorPortal != -1;

            while (isFloorPortal)
            {
                Portal portal = _level.Portals[block.FloorPortal];

                // If the floor is toggle opacity 1 then exit loop
                if (block.FloorOpacity == PortalOpacity.Opacity1 &&
                    !(editorRoom.FlagWater ^ _editor.Level.Rooms[Rooms[portal.AdjoiningRoom].OriginalRoomId].FlagWater))
                {
                    break;
                }

                roomIndex = _roomsIdTable[portal.AdjoiningRoom];

                currentRoom = Rooms[roomIndex];
                editorRoom = _level.Rooms[currentRoom.OriginalRoomId];

                positionX = (int)(currentRoom.Info.X / 1024.0f);
                positionZ = (int)(currentRoom.Info.Z / 1024.0f);

                xInRoom = x - positionX;
                zInRoom = z - positionZ;

                block = editorRoom.Blocks[xInRoom, zInRoom];

                isFloorPortal = block.FloorPortal != -1;
            }

            int sumHeights = block.QAFaces[0] + block.QAFaces[1] + block.QAFaces[2] + block.QAFaces[3];
            int meanFloorCornerHeight = sumHeights >> 2;

            height0 = block.QAFaces[0];
            height1 = block.QAFaces[1];
            height2 = block.QAFaces[2];
            height3 = block.QAFaces[3];

            meanFloorCornerHeight = sumHeights >> 2;

            slope1 = (byte)(Math.Abs(height0 - height1) >= 3 ? 1 : 0);
            slope2 = (byte)(Math.Abs(height1 - height2) >= 3 ? 1 : 0);
            slope3 = (byte)(Math.Abs(height2 - height3) >= 3 ? 1 : 0);
            slope4 = (byte)(Math.Abs(height3 - height0) >= 3 ? 1 : 0);

            bool xa = false;
            bool za = false;

            if (height0 == height2)
            {
                za = false;
            }
            else
            {
                if (height1 != height3)
                {
                    if ((height0 < height1 && height0 < height3) ||
                        (height2 < height1 && height2 < height3) ||
                        (height0 > height1 && height0 > height3) ||
                        (height2 > height1 && height2 > height3))
                    {
                        za = true;
                    }
                }
                else
                {
                    za = true;
                }
            }

            int height = currentRoom.Info.YBottom + meanFloorCornerHeight * -256;

            if (slope1 + slope2 + slope4 + slope3 >= 3 || slope1 == 1 && slope3 == 1 || slope2 == 1 && slope4 == 1)
            {
                if ((block.Flags & BlockFlags.Box) != BlockFlags.Box)            
                {
                    return meanFloorCornerHeight;
                }
            }
            else
            {
                if (za)
                {
                    if ((slope1 == 0 || slope2 == 0) && (slope3 == 0 || slope4 == 0))
                    {
                        if ((block.Flags & BlockFlags.Box) != BlockFlags.Box)
                        {
                            return meanFloorCornerHeight;
                        }
                    }
                }
            }

            return 0x7fff;
        }
        
        private bool CanSectorBeReachedAndIsSolid(int room, int x, int z, out int destRoom)
        {
            int roomIndex = room;
            int xInRoom = 0;
            int zInRoom = 0;
            int xRoomPosition = 0;
            int zRoomPosition = 0;
            Portal portal;

            destRoom = room;

            var currentRoom = Rooms[roomIndex];
            Room editorRoom;

            xRoomPosition = (int)(currentRoom.Info.X / 1024.0f);
            zRoomPosition = (int)(currentRoom.Info.Z / 1024.0f);

            xInRoom = x - xRoomPosition;
            zInRoom = z - zRoomPosition;

            bool isOutside = IsXzInBorderOrOutsideRoom(roomIndex, xInRoom, zInRoom);

            while (isOutside)
            {
                currentRoom = Rooms[roomIndex];
                editorRoom = _level.Rooms[currentRoom.OriginalRoomId];

                xRoomPosition = (int)(currentRoom.Info.X / 1024.0f);
                zRoomPosition = (int)(currentRoom.Info.Z / 1024.0f);

                xInRoom = x - xRoomPosition;
                zInRoom = z - zRoomPosition;

                // Limit the X, Z to current room
                if (xInRoom >= 0)
                {
                    if (xInRoom >= currentRoom.NumXSectors)
                        xInRoom = currentRoom.NumXSectors - 1;
                }
                else
                {
                    xInRoom = 0;
                }
                if (zInRoom >= 0)
                {
                    if (zInRoom >= currentRoom.NumZSectors)
                        zInRoom = currentRoom.NumZSectors - 1;
                }
                else
                {
                    zInRoom = 0;
                }

                // If current X, Z is not a block wall then exit the loop
                if (editorRoom.Blocks[xInRoom, zInRoom].WallPortal == -1) return false;

                // Get the wall portal
                portal = _editor.Level.Portals[editorRoom.Blocks[xInRoom, zInRoom].WallPortal];
                roomIndex = _roomsIdTable[portal.AdjoiningRoom];
                destRoom = roomIndex;

                // If portal is a toggle opacity 1 then I can't go to original X, Z so quit the function
                if (editorRoom.Blocks[xInRoom, zInRoom].WallOpacity == PortalOpacity.Opacity1) return false;

                // Check if now I'm outside
                isOutside = IsXzInBorderOrOutsideRoom(roomIndex, x, z);
            }

            // If I am here, I've probed that I can reach the requested X, Z
            // Now I have to check if the floor under that sector is solid
            currentRoom = Rooms[roomIndex];
            editorRoom = _level.Rooms[currentRoom.OriginalRoomId];

            xRoomPosition = (int)(currentRoom.Info.X / 1024.0f);
            zRoomPosition = (int)(currentRoom.Info.Z / 1024.0f);

            xInRoom = x - xRoomPosition;
            zInRoom = z - zRoomPosition;

            bool isFloorPortal = (editorRoom.Blocks[xInRoom, zInRoom].FloorPortal != -1);

            // Navigate all floor portals until I come to a solid surface or to a water surface
            while(isFloorPortal)
            {
                // Get the floor portal
                portal = _editor.Level.Portals[editorRoom.Blocks[xInRoom, zInRoom].FloorPortal];
                roomIndex = _roomsIdTable[portal.AdjoiningRoom];
                destRoom = roomIndex;

                // If floor portal is toggle opacity 1 and not one of the two rooms are water rooms
                if (editorRoom.Blocks[xInRoom, zInRoom].FloorOpacity == PortalOpacity.Opacity1 && 
                    !(editorRoom.FlagWater ^ _editor.Level.Rooms[Rooms[_roomsIdTable[portal.AdjoiningRoom]].OriginalRoomId].FlagWater))
                {
                    return true;
                }

                currentRoom = Rooms[roomIndex];
                editorRoom = _level.Rooms[currentRoom.OriginalRoomId];

                xRoomPosition = (int)(currentRoom.Info.X / 1024.0f);
                zRoomPosition = (int)(currentRoom.Info.Z / 1024.0f);

                xInRoom = x - xRoomPosition;
                zInRoom = z - zRoomPosition;

                isFloorPortal = (editorRoom.Blocks[xInRoom, zInRoom].FloorPortal != -1);
            }

            return true;
        }
        
        private bool IsXzInBorderOrOutsideRoom(int room, int x, int z)
        {
            return (x <= 0 || z <= 0 || x >= Rooms[room].NumXSectors - 1 || z >= Rooms[room].NumZSectors - 1);
        }
        
        private IEnumerable<int> GetAllReachableBoxes(int box, int zoneType)
        {
            var boxes = new List<int>();

            // This is a non-recursive version of the algorithm for finding all reachable boxes. 
            // Avoid recursion all the times you can!
            var stack = new Stack<int>();
            stack.Push(box);

            // All reachable boxes must have the same water flag
            var isWater = (Rooms[tempBoxes[box].Room].Flags & 0x01) != 0;

            while (stack.Count > 0)
            {
                var next = stack.Pop();
                var last = false;

                for (int i = Boxes[next].OverlapIndex; i < Overlaps.Length && !last; i++)
                {
                    last = (Overlaps[i] & 0x8000) != 0;
                    var boxIndex = Overlaps[i] & 0x7ff;

                    var add = false;

                    // Enemies like scorpions, mummies, dogs, wild boars. They can go only on land, and climb 1 click step
                    if (zoneType == 1 || zoneType == 101)
                    {
                        var water = (Rooms[tempBoxes[boxIndex].Room].Flags & 0x01) != 0;
                        var step = Math.Abs(Boxes[next].TrueFloor - Boxes[boxIndex].TrueFloor);
                        if (water == isWater && step <= 256) add = true;
                    }

                    // Enemies like skeletons. They can go only on land, and climb 1 click step. They can also jump 2 blocks.
                    if (zoneType == 2 || zoneType == 102)
                    {
                        var water = (Rooms[tempBoxes[boxIndex].Room].Flags & 0x01) != 0;
                        var step = Math.Abs(Boxes[next].TrueFloor - Boxes[boxIndex].TrueFloor);

                        // Check all possibilities
                        var canJump = tempBoxes[boxIndex].Jump;
                        var canClimb = Math.Abs(step) <= 256;

                        if (water == isWater && (canJump || canClimb)) add = true;
                    }

                    // Enemies like crocodiles. They can go on land and inside water, and climb 1 click step. In water they act like flying enemies.
                    if (zoneType == 3 || zoneType == 103)
                    {
                        var water = (Rooms[tempBoxes[boxIndex].Room].Flags & 0x01) != 0;
                        var step = Math.Abs(Boxes[next].TrueFloor - Boxes[boxIndex].TrueFloor);
                        if (((water == isWater && step <= 256) || water)) add = true;
                    }

                    // Enemies like baddy 1 & 2. They can go only on land, and climb 4 clicks step. They can also jump 2 blocks and monkey.
                    if (zoneType == 4 || zoneType == 104)
                    {
                        var water = (Rooms[tempBoxes[boxIndex].Room].Flags & 0x01) != 0;
                        var step = Boxes[boxIndex].TrueFloor - Boxes[next].TrueFloor;

                        // Check all possibilities
                        var canJump = tempBoxes[boxIndex].Jump;
                        var canClimb = Math.Abs(step) <= 1024;
                        var canMonkey = tempBoxes[boxIndex].Monkey;

                        if (water == isWater && (canJump || canClimb || canMonkey)) add = true;
                    }

                    // Flying enemies. Here we just check if the water flag is the same.
                    if (zoneType == 5 || zoneType == 105)
                    {
                        var water = (Rooms[tempBoxes[boxIndex].Room].Flags & 0x01) != 0;
                        if (water == isWater) add = true;
                    }

                    if (stack.Contains(boxIndex) || !add)
                        continue;
                    
                    if (!boxes.Contains(boxIndex))
                        stack.Push(boxIndex);
                    
                    boxes.Add(boxIndex);
                }
            }

            return boxes;
        }
        
        private bool IsVerticallyReachable(int room, int destRoom)
        {
            return room == destRoom || Rooms[room].ReachableRooms.Any(r => r == destRoom);
        }
    }
}
