using System;
using System.Collections.Generic;
using System.Linq;
using TombEditor.Geometry;

namespace TombEditor.Compilers
{
    public sealed partial class LevelCompilerTr4
    {
        private List<tr_box_aux> _tempBoxes;

        private void BuildPathFindingData()
        {
            ReportProgress(50, "Building pathfinding data");

            // Fix monkey on portals
            foreach (var fixRoom in _editor.Level.Rooms)
            {
                if (fixRoom == null) continue;

                for (int x = 0; x < fixRoom.NumXSectors; x++)
                {
                    for (int z = 0; z < fixRoom.NumZSectors; z++)
                    {
                        if (fixRoom._compiled.AuxSectors[x, z].FloorPortal != -1)
                        {
                            fixRoom._compiled.AuxSectors[x, z].Monkey = FindMonkeyFloor(fixRoom, x, z);
                        }
                    }
                }
            }

            // Build boxes
            _tempBoxes = new List<tr_box_aux>();

            // First build boxes except portal boxes
            /*foreach (var room in _editor.Level.Rooms)
            {
                if (room == null) continue;

                for (var x = 1; x < room.NumXSectors - 1; x++)
                {
                    for (var z = 1; z < room.NumZSectors - 1; z++)
                    {
                        var sector = GetSector(room, x, z);
                        var aux = room._compiled.AuxSectors[x, z];

                        // If this room is excluded from pathfinding or this sector is a Not Walkable Floor
                        if (room._compiled.OriginalRoom.ExcludeFromPathFinding || aux.NotWalkableFloor)
                        {
                            sector.BoxIndex = (short) (0x7ff0 | (byte) room._compiled.TextureSounds[x, z]);
                            SaveSector(room, x, z, sector);

                            continue;
                        }

                        if (aux.Wall || aux.Portal || aux.HardSlope) continue;

                        // Build the box
                        tr_box_aux box;
                        var result = BuildBox(room, x, z, 0, 0, 0, 0, out box);
                        if (!result) continue;

                        // Check if box exists
                        var found = -1;
                        for (var j = 0; j < _tempBoxes.Count; j++)
                        {
                            var box2 = _tempBoxes[j];

                            var r1 = _editor.Level.Rooms[box.Room];
                            var r2 = _editor.Level.Rooms[box2.Room];

                            if (box.Xmin == box2.Xmin && box.Xmax == box2.Xmax && box.Zmin == box2.Zmin &&
                                box.Zmax == box2.Zmax &&
                                (box.Room == box2.Room ||
                                 (box.Room != box2.Room &&
                                  (r1.BaseRoom == r2._compiled.FlippedRoom || r1._compiled.FlippedRoom == r2.BaseRoom))) &&
                                box.TrueFloor == box2.TrueFloor)
                            {
                                found = j;
                                break;
                            }
                        }

                        // If box is not found, then add the new box
                        if (found == -1)
                        {
                            _tempBoxes.Add(box);

                            for (int x2 = box.Xmin; x2 < box.Xmax; x2++)
                            {
                                for (int z2 = box.Zmin; z2 < box.Zmax; z2++)
                                {
                                    var xc = x2 - room._compiled.Info.X / 1024;
                                    var zc = z2 - room._compiled.Info.Z / 1024;

                                    var sect = GetSector(room, xc, zc);
                                    var aux2 = room._compiled.AuxSectors[xc, zc];

                                    if (aux2.Wall)
                                    {
                                        sect.BoxIndex = 0x7ff6;
                                    }
                                    else
                                    {
                                        sect.BoxIndex =
                                            (short) (((_tempBoxes.Count - 1) << 4) | (byte) room._compiled.TextureSounds[xc, zc]);
                                    }

                                    SaveSector(room, xc, zc, sect);
                                }
                            }
                        }
                        else
                        {
                            for (int x2 = box.Xmin; x2 < box.Xmax; x2++)
                            {
                                for (int z2 = box.Zmin; z2 < box.Zmax; z2++)
                                {
                                    var xc = x2 - room._compiled.Info.X / 1024;
                                    var zc = z2 - room._compiled.Info.Z / 1024;

                                    var sect = GetSector(room, xc, zc);
                                    var aux2 = room._compiled.AuxSectors[xc, zc];

                                    if (aux2.Wall)
                                    {
                                        sect.BoxIndex = 0x7ff6;
                                    }
                                    else
                                    {
                                        sect.BoxIndex = (short) (((found) << 4) | (byte) room._compiled.TextureSounds[xc, zc]);
                                    }

                                    SaveSector(room, xc, zc, sect);
                                }
                            }
                        }
                    }
                }
            }

            // Now build only boxes of horizontal portals
            foreach (var room in _editor.Level.Rooms)
            {
                if (room == null) continue;

                for (var x = 1; x < room.NumXSectors - 1; x++)
                {
                    for (var z = 1; z < room.NumZSectors - 1; z++)
                    {
                        var sector = GetSector(room, x, z);
                        var aux = room._compiled.AuxSectors[x, z];

                        // If this room is excluded from pathfinding or this sector is a Not Walkable Floor
                        if (room._compiled.OriginalRoom.ExcludeFromPathFinding || aux.NotWalkableFloor)
                        {
                            sector.BoxIndex = (short) (0x7ff0 | (byte) room._compiled.TextureSounds[x, z]);
                            SaveSector(room, x, z, sector);

                            continue;
                        }

                        if (aux.FloorPortal == -1) continue;

                        var xMin = room._compiled.Info.X / 1024 + _level.Portals[aux.FloorPortal].X;
                        var xMax = room._compiled.Info.X / 1024 + _level.Portals[aux.FloorPortal].X +
                                   _level.Portals[aux.FloorPortal].NumXBlocks;
                        var zMin = room._compiled.Info.Z / 1024 + _level.Portals[aux.FloorPortal].Z;
                        var zMax = room._compiled.Info.Z / 1024 + _level.Portals[aux.FloorPortal].Z +
                                   _level.Portals[aux.FloorPortal].NumZBlocks;

                        // Find the lowest room and floor
                        Room room2;
                        short floor2;
                        var result = GetMostDownFloorAndRoom(room, x, z, out room2, out floor2);
                        if (!result) continue;

                        // Build the box
                        tr_box_aux box;
                        result = BuildBox(room2, x + room._compiled.Info.X / 1024 - room2._compiled.Info.X / 1024,
                            z + room._compiled.Info.Z / 1024 - room2._compiled.Info.Z / 1024,
                            xMin, xMax, zMin, zMax, out box);
                        box.Room = (short) _level.Rooms.ReferenceIndexOf(room2);
                        if (!result) continue;

                        // Check if box exists
                        var found = -1;
                        for (var j = 0; j < _tempBoxes.Count; j++)
                        {
                            var box2 = _tempBoxes[j];

                            var r1 = _editor.Level.Rooms[box.Room];
                            var r2 = _editor.Level.Rooms[box2.Room];

                            if (box.Xmin == box2.Xmin && box.Xmax == box2.Xmax && box.Zmin == box2.Zmin &&
                                box.Zmax == box2.Zmax &&
                                (box.Room == box2.Room ||
                                 (box.Room != box2.Room &&
                                  (r1.BaseRoom == r2._compiled.FlippedRoom || r1._compiled.FlippedRoom == r2.BaseRoom))) &&
                                box.TrueFloor == box2.TrueFloor)
                            {
                                found = j;
                                break;
                            }
                        }

                        // If box is not found, then add the new box
                        if (found == -1)
                        {
                            box.TrueFloor = GetMostDownFloor(room, x, z);

                            _tempBoxes.Add(box);
                            found = _tempBoxes.Count - 1;
                        }

                        for (int x2 = box.Xmin; x2 < box.Xmax; x2++)
                        {
                            for (int z2 = box.Zmin; z2 < box.Zmax; z2++)
                            {
                                var xc = x2 - room._compiled.Info.X / 1024;
                                var zc = z2 - room._compiled.Info.Z / 1024;

                                var sect = GetSector(room, xc, zc);
                                var aux2 = room._compiled.AuxSectors[xc, zc];

                                if (aux.FloorPortal == aux2.FloorPortal)
                                {
                                    sect.BoxIndex = (short) ((found << 4) | (byte) room._compiled.TextureSounds[xc, zc]);
                                    SaveSector(room, xc, zc, sect);
                                }
                            }
                        }
                    }
                }
            }
            */

            Dec_BuildBoxesAndOverlaps();

            for (int i = 0; i < dec_numBoxes; i++)
            {
                tr_box_aux box = new tr_box_aux();

                box.Xmin = (byte)dec_boxes[i].Xmin;
                box.Xmax = (byte)dec_boxes[i].Xmax;
                box.Zmin = (byte)dec_boxes[i].Zmin;
                box.Zmax = (byte)dec_boxes[i].Zmax;
                box.Room = (short)_level.Rooms.ReferenceIndexOf(dec_boxes[i].Room);
                box.IsolatedBox = dec_boxes[i].IsolatedBox;
                box.Monkey = dec_boxes[i].Monkey;
                box.Jump = dec_boxes[i].Jump;
               // box.FlipMap = dec_boxes[i].FlipMap;
                box.OverlapIndex = dec_boxes[i].OverlapIndex;
               // box.Portal = dec_boxes[i].Portal;
               // box.RoomBelow = dec_boxes[i].RoomBelow;
                box.TrueFloor = (short)(dec_boxes[i].TrueFloor * -256);
               // box.Border = dec_boxes[i].Border;
                //box.AlternateRoom = dec_boxes[i].AlternateRoom;

                _tempBoxes.Add(box);
            }

            // Build overlaps
            var tempOverlaps = new List<tr_overlap_aux>();

            for (var i = 0; i < _tempBoxes.Count; i++)
            {
                var box = _tempBoxes[i];

                var foundOverlaps = false;
                var baseOverlaps = (short) tempOverlaps.Count;

                for (var j = 0; j < _tempBoxes.Count; j++)
                {
                    // if they are the same box don't do anything
                    if (i == j) continue;

                    var box2 = _tempBoxes[j];

                    // Now we have to find overlaps and edges
                    bool jump;

                    if (!BoxesOverlap(i, j, out jump))
                        continue;

                    var overlap = new tr_overlap_aux
                    {
                        Box = j,
                        IsEdge = (box.Xmax == box2.Xmin || box.Zmax == box2.Zmin || box.Xmin == box2.Xmax ||
                                  box.Zmin == box2.Zmax),
                        Monkey = box2.Monkey,
                        MainBox = i
                    };

                    tempOverlaps.Add(overlap);

                    if (box.Jump == false) box.Jump = jump;
                    if (box2.Jump == false) box2.Jump = jump;

                    _tempBoxes[j] = box2;

                    foundOverlaps = true;
                }

                box.OverlapIndex = (short) (foundOverlaps ? baseOverlaps : 2047);

                _tempBoxes[i] = box;

                if (!foundOverlaps) continue;

                if (box.IsolatedBox) box.OverlapIndex = (short) (box.OverlapIndex | 0x8000);

                // Mark the end of the list
                var last = tempOverlaps[tempOverlaps.Count - 1];
                last.EndOfList = true;
                tempOverlaps[tempOverlaps.Count - 1] = last;
            }

            // Build final overlaps
            _overlaps = new ushort[tempOverlaps.Count];
            for (var i = 0; i < tempOverlaps.Count; i++)
            {
                var ov = (ushort) tempOverlaps[i].Box;

                // Is the last overlap of the list?
                if (tempOverlaps[i].EndOfList) ov |= 0x8000;

                // Monkey flag
                var canMonkey = _tempBoxes[tempOverlaps[i].Box].Monkey && _tempBoxes[tempOverlaps[i].MainBox].Monkey;
                if (canMonkey) ov |= 0x2000;

                var canJump = _tempBoxes[tempOverlaps[i].Box].Jump;
                if (canJump) ov |= 0x800;

                _overlaps[i] = ov;
            }

            _boxes = new tr_box[_tempBoxes.Count];
            _zones = new tr_zone[_tempBoxes.Count];

            // Convert boxes to TR format
            for (var i = 0; i < _tempBoxes.Count; i++)
            {
                var newBox = new tr_box();
                var aux = _tempBoxes[i];
                var zone = new tr_zone();

                newBox.Xmin = aux.Xmin;
                newBox.Xmax = aux.Xmax;
                newBox.Zmin = aux.Zmin;
                newBox.Zmax = aux.Zmax;
                newBox.TrueFloor = aux.TrueFloor;
                newBox.OverlapIndex = aux.OverlapIndex;

                _boxes[i] = newBox;
                _zones[i] = zone;
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

            for (var i = 0; i < _boxes.Length; i++)
            {
                // Skeleton like enemis: in the future implement also jump
                if (_zones[i].GroundZone1_Normal == 0)
                {
                    _zones[i].GroundZone1_Normal = groundZone1;

                    foreach (var box in GetAllReachableBoxes(i, 1))
                    {
                        _zones[box].GroundZone1_Normal = groundZone1;
                    }

                    groundZone1++;
                }

                // Mummy like enemis: the simplest case
                if (_zones[i].GroundZone2_Normal == 0)
                {
                    _zones[i].GroundZone2_Normal = groundZone2;

                    foreach (var box in GetAllReachableBoxes(i, 2))
                    {
                        _zones[box].GroundZone2_Normal = groundZone2;
                    }

                    groundZone2++;
                }

                // Crocodile like enemis: like 1 & 2 but they can go inside water and swim
                if (_zones[i].GroundZone3_Normal == 0)
                {
                    _zones[i].GroundZone3_Normal = groundZone3;

                    foreach (var box in GetAllReachableBoxes(i, 3))
                    {
                        _zones[box].GroundZone3_Normal = groundZone3;
                    }

                    groundZone3++;
                }

                // Baddy like enemis: they can jump, grab and monkey
                if (_zones[i].GroundZone4_Normal == 0)
                {
                    _zones[i].GroundZone4_Normal = groundZone4;

                    foreach (var box in GetAllReachableBoxes(i, 4))
                    {
                        _zones[box].GroundZone4_Normal = groundZone4;
                    }

                    groundZone4++;
                }

                // Bat like enemis: they can fly everywhere, except into the water
                if (_zones[i].FlyZone_Normal == 0)
                {
                    _zones[i].FlyZone_Normal = flyZone;

                    foreach (var box in GetAllReachableBoxes(i, 5))
                    {
                        _zones[box].FlyZone_Normal = flyZone;
                    }

                    flyZone++;
                }

                // Flipped rooms------------------------------------------

                // Skeleton like enemis: in the future implement also jump
                if (_zones[i].GroundZone1_Alternate == 0)
                {
                    _zones[i].GroundZone1_Alternate = aGroundZone1;

                    foreach (var box in GetAllReachableBoxes(i, 101))
                    {
                        _zones[box].GroundZone1_Alternate = aGroundZone1;
                    }

                    aGroundZone1++;
                }

                // Mummy like enemis: the simplest case
                if (_zones[i].GroundZone2_Alternate == 0)
                {
                    _zones[i].GroundZone2_Alternate = aGroundZone2;

                    foreach (var box in GetAllReachableBoxes(i, 102))
                    {
                        _zones[box].GroundZone2_Alternate = aGroundZone2;
                    }

                    aGroundZone2++;
                }

                // Crocodile like enemis: like 1 & 2 but they can go inside water and swim
                if (_zones[i].GroundZone3_Alternate == 0)
                {
                    _zones[i].GroundZone3_Alternate = aGroundZone3;

                    foreach (var box in GetAllReachableBoxes(i, 103))
                    {
                        _zones[box].GroundZone3_Alternate = aGroundZone3;
                    }

                    aGroundZone3++;
                }

                // Baddy like enemis: they can jump, grab and monkey
                if (_zones[i].GroundZone4_Alternate == 0)
                {
                    _zones[i].GroundZone4_Alternate = aGroundZone4;

                    foreach (var box in GetAllReachableBoxes(i, 104))
                    {
                        _zones[box].GroundZone4_Alternate = aGroundZone4;
                    }

                    aGroundZone4++;
                }

                // Bat like enemis: they can fly everywhere, except into the water
                if (_zones[i].FlyZone_Alternate != 0)
                    continue;

                _zones[i].FlyZone_Alternate = aFlyZone;

                foreach (var box in GetAllReachableBoxes(i, 105))
                {
                    _zones[box].FlyZone_Alternate = aFlyZone;
                }

                aFlyZone++;
            }

            ReportProgress(60, "    Number of boxes: " + _boxes.Length);
            ReportProgress(60, "    Number of overlaps: " + _overlaps.Length);
            ReportProgress(60, "    Number of zones: " + _boxes.Length);
        }

        private bool BoxesOverlap(int b1, int b2, out bool jump)
        {
            jump = false;

            var a = _tempBoxes[b1];
            var b = _tempBoxes[b2];

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
                    var r1 = _editor.Level.Rooms[a.Room];
                    var r2 = _editor.Level.Rooms[b.Room];

                    if (a.Room != b.Room && (IsVerticallyReachable(_level.Rooms[a.Room], _level.Rooms[b.Room]) ||
                                             r1.BaseRoom == r2._compiled.FlippedRoom || r1._compiled.FlippedRoom == r2.BaseRoom))
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        private bool CheckIfCanJumpX(int box1, int box2)
        {
            var a = _tempBoxes[box1];
            var b = _tempBoxes[box2];

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

            Room roomIndex = null;
            Room destRoom;
            int currentZ = zMean;

            int floor;
            int xMax = 0;

            Room currentRoom;

            int xRoomPosition;
            int zRoomPosition;

            int xInRoom;
            int zInRoom;

            if (b.Xmax == a.Xmin - 1 || b.Xmax == a.Xmin - 2)
            {
                xMax = b.Xmax;
                roomIndex = _level.Rooms[b.Room];
            }

            if (a.Xmax == b.Xmin - 1 || a.Xmax == b.Xmin - 2)
            {
                xMax = a.Xmax;
                roomIndex = _level.Rooms[a.Room];
            }

            // If the gap is of 1 sector
            if (b.Xmax == a.Xmin - 1 || a.Xmax == b.Xmin - 1)
            {
                // If X, Zmax - 1 can't be reached then quit the function
                if (!CanSectorBeReachedAndIsSolid(roomIndex, xMax - 1, currentZ, out destRoom))
                    return false;

                if (!CanSectorBeReachedAndIsSolid(roomIndex, xMax, currentZ, out destRoom))
                    return false;

                currentRoom = destRoom;

                xRoomPosition = (int) (currentRoom._compiled.Info.X / 1024.0f);
                zRoomPosition = (int) (currentRoom._compiled.Info.Z / 1024.0f);

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

            currentRoom = destRoom;

            xRoomPosition = (int) (currentRoom._compiled.Info.X / 1024.0f);
            zRoomPosition = (int) (currentRoom._compiled.Info.Z / 1024.0f);

            xInRoom = xMax - xRoomPosition;
            zInRoom = currentZ - zRoomPosition;

            floor = GetBoxFloorHeight(destRoom, xInRoom, zInRoom);

            if (-floor > -b.TrueFloor - 512 || floor == 0x7fff)
                return false;

            if (!CanSectorBeReachedAndIsSolid(roomIndex, xMax + 1, currentZ, out destRoom))
                return false;

            currentRoom = destRoom;

            xRoomPosition = (int) (currentRoom._compiled.Info.X / 1024.0f);
            zRoomPosition = (int) (currentRoom._compiled.Info.Z / 1024.0f);

            xInRoom = xMax + 1 - xRoomPosition;
            zInRoom = currentZ - zRoomPosition;

            floor = GetBoxFloorHeight(destRoom, xInRoom, zInRoom);

            //floor = GetBoxFloorHeight(destRoom, xMax + 1, currentZ);
            return -floor <= -b.TrueFloor - 512 && floor != 0x7FFF;
        }

        private bool CheckIfCanJumpZ(int box1, int box2)
        {
            var a = _tempBoxes[box1];
            var b = _tempBoxes[box2];

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

            Room roomIndex = null;

            int currentX = xMean;

            int floor;
            int zMax = 0;

            if (b.Zmax == a.Zmin - 1 || b.Zmax == a.Zmin - 2)
            {
                zMax = b.Zmax;
                roomIndex = _level.Rooms[b.Room];
            }

            if (a.Zmax == b.Zmin - 1 || a.Zmax == b.Zmin - 2)
            {
                zMax = a.Zmax;
                roomIndex = _level.Rooms[a.Room];
            }

            Room destRoom;

            Room currentRoom;

            int xRoomPosition;
            int zRoomPosition;

            int xInRoom;
            int zInRoom;

            // If the gap is of 1 sector
            if (b.Zmax == a.Zmin - 1 || a.Zmax == b.Zmin - 1)
            {
                // If X, Zmax - 1 can't be reached then quit the function
                if (!CanSectorBeReachedAndIsSolid(roomIndex, currentX, zMax - 1, out destRoom))
                    return false;

                if (!CanSectorBeReachedAndIsSolid(roomIndex, currentX, zMax, out destRoom))
                    return false;

                currentRoom = destRoom;

                xRoomPosition = (int) (currentRoom._compiled.Info.X / 1024.0f);
                zRoomPosition = (int) (currentRoom._compiled.Info.Z / 1024.0f);

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

            currentRoom = destRoom;

            xRoomPosition = (int) (currentRoom._compiled.Info.X / 1024.0f);
            zRoomPosition = (int) (currentRoom._compiled.Info.Z / 1024.0f);

            xInRoom = currentX - xRoomPosition;
            zInRoom = zMax - zRoomPosition;

            floor = GetBoxFloorHeight(destRoom, xInRoom, zInRoom);

            // floorHeight = GetBoxFloorHeight(destRoom, currentX, zMax);
            if (-floor > -b.TrueFloor - 512 || floor == 0x7fff)
                return false;

            if (!CanSectorBeReachedAndIsSolid(roomIndex, currentX, zMax + 1, out destRoom))
                return false;

            currentRoom = destRoom;

            xRoomPosition = (int) (currentRoom._compiled.Info.X / 1024.0f);
            zRoomPosition = (int) (currentRoom._compiled.Info.Z / 1024.0f);

            xInRoom = currentX - xRoomPosition;
            zInRoom = zMax + 1 - zRoomPosition;

            floor = GetBoxFloorHeight(destRoom, xInRoom, zInRoom);

            //floorHeight = GetBoxFloorHeight(destRoom, currentX, zMax + 1);
            return -floor <= -b.TrueFloor - 512 && floor != 0x7FFF;
        }

        private int GetBoxFloorHeight(Room room, int x, int z) => GetMostDownFloor(room, x, z);

        private bool CanSectorBeReachedAndIsSolid(Room room, int x, int z, out Room destRoom)
        {
            destRoom = room;

            var currentRoom = room;
            Room editorRoom;

            var xRoomPosition = (int) (currentRoom._compiled.Info.X / 1024.0f);
            var zRoomPosition = (int) (currentRoom._compiled.Info.Z / 1024.0f);

            var xInRoom = x - xRoomPosition;
            var zInRoom = z - zRoomPosition;

            bool isOutside = IsXzInBorderOrOutsideRoom(room, xInRoom, zInRoom);

            while (isOutside)
            {
                currentRoom = room;
                editorRoom = currentRoom._compiled.OriginalRoom;

                xRoomPosition = (int) (currentRoom._compiled.Info.X / 1024.0f);
                zRoomPosition = (int) (currentRoom._compiled.Info.Z / 1024.0f);

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
                var portal = _editor.Level.Portals[editorRoom.Blocks[xInRoom, zInRoom].WallPortal];
                room = portal.AdjoiningRoom;
                destRoom = room;

                // If portal is a toggle opacity 1 then I can't go to original X, Z so quit the function
                if (editorRoom.Blocks[xInRoom, zInRoom].WallOpacity == PortalOpacity.Opacity1) return false;

                // Check if now I'm outside
                isOutside = IsXzInBorderOrOutsideRoom(room, x, z);
            }

            // If I am here, I've probed that I can reach the requested X, Z
            // Now I have to check if the floor under that sector is solid
            currentRoom = room;
            editorRoom = currentRoom._compiled.OriginalRoom;

            xRoomPosition = (int) (currentRoom._compiled.Info.X / 1024.0f);
            zRoomPosition = (int) (currentRoom._compiled.Info.Z / 1024.0f);

            xInRoom = x - xRoomPosition;
            zInRoom = z - zRoomPosition;

            bool isFloorPortal = (editorRoom.Blocks[xInRoom, zInRoom].FloorPortal != -1);

            // Navigate all floor portals until I come to a solid surface or to a water surface
            while (isFloorPortal)
            {
                // Get the floor portal
                var portal = _editor.Level.Portals[editorRoom.Blocks[xInRoom, zInRoom].FloorPortal];
                room = portal.AdjoiningRoom;
                destRoom = room;

                // If floor portal is toggle opacity 1 and not one of the two rooms are water rooms
                if (editorRoom.Blocks[xInRoom, zInRoom].FloorOpacity == PortalOpacity.Opacity1 &&
                    !(editorRoom.FlagWater ^ portal.AdjoiningRoom._compiled.OriginalRoom.FlagWater))
                {
                    return true;
                }

                currentRoom = room;
                editorRoom = currentRoom._compiled.OriginalRoom;

                xRoomPosition = (int) (currentRoom._compiled.Info.X / 1024.0f);
                zRoomPosition = (int) (currentRoom._compiled.Info.Z / 1024.0f);

                xInRoom = x - xRoomPosition;
                zInRoom = z - zRoomPosition;

                isFloorPortal = (editorRoom.Blocks[xInRoom, zInRoom].FloorPortal != -1);
            }

            return true;
        }

        private static bool IsXzInBorderOrOutsideRoom(Room room, int x, int z)
        {
            return (x <= 0 || z <= 0 || x >= room._compiled.NumXSectors - 1 || z >= room._compiled.NumZSectors - 1);
        }

        private IEnumerable<int> GetAllReachableBoxes(int box, int zoneType)
        {
            var boxes = new List<int>();

            // This is a non-recursive version of the algorithm for finding all reachable boxes. 
            // Avoid recursion all the times you can!
            var stack = new Stack<int>();
            stack.Push(box);

            // All reachable boxes must have the same water flag
            var isWater = (_editor.Level.Rooms[_tempBoxes[box].Room]._compiled.Flags & 0x01) != 0;

            while (stack.Count > 0)
            {
                var next = stack.Pop();
                var last = false;

                for (int i = _boxes[next].OverlapIndex; i < _overlaps.Length && !last; i++)
                {
                    int overlapIndex = _boxes[next].OverlapIndex & 0x3fff;
                    last = (_overlaps[overlapIndex] & 0x8000) != 0;

                    var boxIndex = _overlaps[overlapIndex] & 0x7ff;

                    var add = false;

                    // Enemies like scorpions, mummies, dogs, wild boars. They can go only on land, and climb 1 click step
                    if (zoneType == 1 || zoneType == 101)
                    {
                        var water = (_editor.Level.Rooms[_tempBoxes[boxIndex].Room]._compiled.Flags & 0x01) != 0;
                        var step = Math.Abs(_boxes[next].TrueFloor - _boxes[boxIndex].TrueFloor);
                        if (water == isWater && step <= 256) add = true;
                    }

                    // Enemies like skeletons. They can go only on land, and climb 1 click step. They can also jump 2 blocks.
                    if (zoneType == 2 || zoneType == 102)
                    {
                        var water = (_editor.Level.Rooms[_tempBoxes[boxIndex].Room]._compiled.Flags & 0x01) != 0;
                        var step = Math.Abs(_boxes[next].TrueFloor - _boxes[boxIndex].TrueFloor);

                        // Check all possibilities
                        var canJump = _tempBoxes[boxIndex].Jump;
                        var canClimb = Math.Abs(step) <= 256;

                        if (water == isWater && (canJump || canClimb)) add = true;
                    }

                    // Enemies like crocodiles. They can go on land and inside water, and climb 1 click step. In water they act like flying enemies.
                    if (zoneType == 3 || zoneType == 103)
                    {
                        var water = (_editor.Level.Rooms[_tempBoxes[boxIndex].Room]._compiled.Flags & 0x01) != 0;
                        var step = Math.Abs(_boxes[next].TrueFloor - _boxes[boxIndex].TrueFloor);
                        if (((water == isWater && step <= 256) || water)) add = true;
                    }

                    // Enemies like baddy 1 & 2. They can go only on land, and climb 4 clicks step. They can also jump 2 blocks and monkey.
                    if (zoneType == 4 || zoneType == 104)
                    {
                        var water = (_editor.Level.Rooms[_tempBoxes[boxIndex].Room]._compiled.Flags & 0x01) != 0;
                        var step = _boxes[boxIndex].TrueFloor - _boxes[next].TrueFloor;

                        // Check all possibilities
                        var canJump = _tempBoxes[boxIndex].Jump;
                        var canClimb = Math.Abs(step) <= 1024;
                        var canMonkey = _tempBoxes[boxIndex].Monkey;

                        if (water == isWater && (canJump || canClimb || canMonkey)) add = true;
                    }

                    // Flying enemies. Here we just check if the water flag is the same.
                    if (zoneType == 5 || zoneType == 105)
                    {
                        var water = (_editor.Level.Rooms[_tempBoxes[boxIndex].Room]._compiled.Flags & 0x01) != 0;
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

        private static bool IsVerticallyReachable(Room room, Room destRoom)
        {
            return room == destRoom || room._compiled.ReachableRooms.Any(r => r == destRoom);
        }
    }
}
