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
            foreach (var fixRoom in _tempRooms.Values)
            {
                for (int x = 0; x < fixRoom.NumXSectors; x++)
                {
                    for (int z = 0; z < fixRoom.NumZSectors; z++)
                    {
                        var sector = fixRoom.AuxSectors[x, z];
                        if (sector.FloorPortal != null)
                            sector.Monkey = FindMonkeyFloor(fixRoom.OriginalRoom, x, z);
                    }
                }
            }

            // Build boxes
            _tempBoxes = new List<tr_box_aux>();
            
            // Use decompiled code for generation of boxes and overlaps
            Dec_BuildBoxesAndOverlaps();

            // Copy boxes from the decompile struct to editor struct. To remove in the future.
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
                box.FlipMap = dec_boxes[i].Flipped;
                box.OverlapIndex = dec_boxes[i].OverlapIndex;
                box.TrueFloor = (short)(dec_boxes[i].TrueFloor * -256);

                _tempBoxes.Add(box);
            }

            _overlaps = new ushort[dec_numOverlaps];
            if (dec_numOverlaps != 0)
                Array.Copy(dec_overlaps, _overlaps, dec_numOverlaps);

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
                    if (_tempBoxes[i].FlipMap) continue;

                    _zones[i].GroundZone1_Normal = groundZone1;

                    foreach (var box in GetAllReachableBoxes(i, 1, false))
                    {
                        _zones[box].GroundZone1_Normal = groundZone1;
                    }

                    groundZone1++;
                }

                // Mummy like enemis: the simplest case
                if (_zones[i].GroundZone2_Normal == 0)
                {
                    if (_tempBoxes[i].FlipMap) continue;

                    _zones[i].GroundZone2_Normal = groundZone2;

                    foreach (var box in GetAllReachableBoxes(i, 2, false))
                    {
                        _zones[box].GroundZone2_Normal = groundZone2;
                    }

                    groundZone2++;
                }

                // Crocodile like enemis: like 1 & 2 but they can go inside water and swim
                if (_zones[i].GroundZone3_Normal == 0)
                {
                    if (_tempBoxes[i].FlipMap) continue;

                    _zones[i].GroundZone3_Normal = groundZone3;

                    foreach (var box in GetAllReachableBoxes(i, 3, false))
                    {
                        _zones[box].GroundZone3_Normal = groundZone3;
                    }

                    groundZone3++;
                }

                // Baddy like enemis: they can jump, grab and monkey
                if (_zones[i].GroundZone4_Normal == 0)
                {
                    if (_tempBoxes[i].FlipMap) continue;

                    _zones[i].GroundZone4_Normal = groundZone4;

                    foreach (var box in GetAllReachableBoxes(i, 4, false))
                    {
                        _zones[box].GroundZone4_Normal = groundZone4;
                    }

                    groundZone4++;
                }

                // Bat like enemis: they can fly everywhere, except into the water
                if (_zones[i].FlyZone_Normal == 0)
                {
                    if (_tempBoxes[i].FlipMap) continue;

                    _zones[i].FlyZone_Normal = flyZone;

                    foreach (var box in GetAllReachableBoxes(i, 5, false))
                    {
                        _zones[box].FlyZone_Normal = flyZone;
                    }

                    flyZone++;
                }

                // Flipped rooms------------------------------------------

                // Skeleton like enemis: in the future implement also jump
                if (_zones[i].GroundZone1_Alternate == 0)
                {
                    if (!_tempBoxes[i].FlipMap) continue;

                    _zones[i].GroundZone1_Alternate = aGroundZone1;

                    foreach (var box in GetAllReachableBoxes(i, 1, true))
                    {
                        _zones[box].GroundZone1_Alternate = aGroundZone1;
                    }

                    aGroundZone1++;
                }

                // Mummy like enemis: the simplest case
                if (_zones[i].GroundZone2_Alternate == 0)
                {
                    if (!_tempBoxes[i].FlipMap) continue;

                    _zones[i].GroundZone2_Alternate = aGroundZone2;

                    foreach (var box in GetAllReachableBoxes(i, 2, true))
                    {
                        _zones[box].GroundZone2_Alternate = aGroundZone2;
                    }

                    aGroundZone2++;
                }

                // Crocodile like enemis: like 1 & 2 but they can go inside water and swim
                if (_zones[i].GroundZone3_Alternate == 0)
                {
                    if (!_tempBoxes[i].FlipMap) continue;

                    _zones[i].GroundZone3_Alternate = aGroundZone3;

                    foreach (var box in GetAllReachableBoxes(i, 3, true))
                    {
                        _zones[box].GroundZone3_Alternate = aGroundZone3;
                    }

                    aGroundZone3++;
                }

                // Baddy like enemis: they can jump, grab and monkey
                if (_zones[i].GroundZone4_Alternate == 0)
                {
                    if (!_tempBoxes[i].FlipMap) continue;

                    _zones[i].GroundZone4_Alternate = aGroundZone4;

                    foreach (var box in GetAllReachableBoxes(i, 4, true))
                    {
                        _zones[box].GroundZone4_Alternate = aGroundZone4;
                    }

                    aGroundZone4++;
                }

                // Bat like enemis: they can fly everywhere, except into the water
                if (_zones[i].FlyZone_Alternate == 0)
                {
                    if (!_tempBoxes[i].FlipMap) continue;

                    _zones[i].FlyZone_Alternate = aFlyZone;

                    foreach (var box in GetAllReachableBoxes(i, 5, true))
                    {
                        _zones[box].FlyZone_Alternate = aFlyZone;
                    }

                    aFlyZone++;
                }
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
                    var r1 = _level.Rooms[a.Room];
                    var r2 = _level.Rooms[b.Room];
                    
                    if (a.Room != b.Room && (IsVerticallyReachable(_level.Rooms[a.Room], _level.Rooms[b.Room]) ||
                                             r1.BaseRoom == _tempRooms[r2].FlippedRoom || _tempRooms[r1].FlippedRoom == r2.BaseRoom))
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

            tr_room currentRoom;

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

                currentRoom = _tempRooms[destRoom];

                xRoomPosition = (int) (currentRoom.Info.X / 1024.0f);
                zRoomPosition = (int) (currentRoom.Info.Z / 1024.0f);

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

            currentRoom = _tempRooms[destRoom];

            xRoomPosition = (int) (currentRoom.Info.X / 1024.0f);
            zRoomPosition = (int) (currentRoom.Info.Z / 1024.0f);

            xInRoom = xMax - xRoomPosition;
            zInRoom = currentZ - zRoomPosition;

            floor = GetBoxFloorHeight(destRoom, xInRoom, zInRoom);

            if (-floor > -b.TrueFloor - 512 || floor == 0x7fff)
                return false;

            if (!CanSectorBeReachedAndIsSolid(roomIndex, xMax + 1, currentZ, out destRoom))
                return false;

            currentRoom = _tempRooms[destRoom];

            xRoomPosition = (int) (currentRoom.Info.X / 1024.0f);
            zRoomPosition = (int) (currentRoom.Info.Z / 1024.0f);

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

            tr_room currentRoom;

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

                currentRoom = _tempRooms[destRoom];

                xRoomPosition = (int) (currentRoom.Info.X / 1024.0f);
                zRoomPosition = (int) (currentRoom.Info.Z / 1024.0f);

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

            currentRoom = _tempRooms[destRoom];

            xRoomPosition = (int) (currentRoom.Info.X / 1024.0f);
            zRoomPosition = (int) (currentRoom.Info.Z / 1024.0f);

            xInRoom = currentX - xRoomPosition;
            zInRoom = zMax - zRoomPosition;

            floor = GetBoxFloorHeight(destRoom, xInRoom, zInRoom);

            // floorHeight = GetBoxFloorHeight(destRoom, currentX, zMax);
            if (-floor > -b.TrueFloor - 512 || floor == 0x7fff)
                return false;

            if (!CanSectorBeReachedAndIsSolid(roomIndex, currentX, zMax + 1, out destRoom))
                return false;

            currentRoom = _tempRooms[destRoom];

            xRoomPosition = (int) (currentRoom.Info.X / 1024.0f);
            zRoomPosition = (int) (currentRoom.Info.Z / 1024.0f);

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
            tr_room tempCurrentRoom = _tempRooms[currentRoom];
            Room editorRoom;

            var xRoomPosition = (int) (tempCurrentRoom.Info.X / 1024.0f);
            var zRoomPosition = (int) (tempCurrentRoom.Info.Z / 1024.0f);

            var xInRoom = x - xRoomPosition;
            var zInRoom = z - zRoomPosition;

            bool isOutside = IsXzInBorderOrOutsideRoom(tempCurrentRoom, xInRoom, zInRoom);

            while (isOutside)
            {
                currentRoom = room;
                tempCurrentRoom = _tempRooms[currentRoom];
                editorRoom = tempCurrentRoom.OriginalRoom;

                xRoomPosition = (int) (tempCurrentRoom.Info.X / 1024.0f);
                zRoomPosition = (int) (tempCurrentRoom.Info.Z / 1024.0f);

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
                if (editorRoom.Blocks[xInRoom, zInRoom].WallPortal == null) return false;

                // Get the wall portal
                room = editorRoom.Blocks[xInRoom, zInRoom].WallPortal.AdjoiningRoom;
                destRoom = room;

                // If portal is a toggle opacity 1 then I can't go to original X, Z so quit the function
                if (editorRoom.Blocks[xInRoom, zInRoom].WallOpacity == PortalOpacity.Opacity1) return false;

                // Check if now I'm outside
                isOutside = IsXzInBorderOrOutsideRoom(tempCurrentRoom, x, z);
            }

            // If I am here, I've probed that I can reach the requested X, Z
            // Now I have to check if the floor under that sector is solid
            currentRoom = room;
            tempCurrentRoom = _tempRooms[currentRoom];
            editorRoom = tempCurrentRoom.OriginalRoom;

            xRoomPosition = (int) (tempCurrentRoom.Info.X / 1024.0f);
            zRoomPosition = (int) (tempCurrentRoom.Info.Z / 1024.0f);

            xInRoom = x - xRoomPosition;
            zInRoom = z - zRoomPosition;

            bool isFloorPortal = (editorRoom.Blocks[xInRoom, zInRoom].FloorPortal != null);

            // Navigate all floor portals until I come to a solid surface or to a water surface
            while (isFloorPortal)
            {
                // Get the floor portal
                var portal = editorRoom.Blocks[xInRoom, zInRoom].FloorPortal;
                room = portal.AdjoiningRoom;
                destRoom = room;

                // If floor portal is toggle opacity 1 and not one of the two rooms are water rooms
                if (editorRoom.Blocks[xInRoom, zInRoom].FloorOpacity == PortalOpacity.Opacity1 &&
                    !(editorRoom.FlagWater ^ _tempRooms[portal.AdjoiningRoom].OriginalRoom.FlagWater))
                {
                    return true;
                }

                currentRoom = room;
                tempCurrentRoom = _tempRooms[currentRoom];
                editorRoom = tempCurrentRoom.OriginalRoom;

                xRoomPosition = (int) (tempCurrentRoom.Info.X / 1024.0f);
                zRoomPosition = (int) (tempCurrentRoom.Info.Z / 1024.0f);

                xInRoom = x - xRoomPosition;
                zInRoom = z - zRoomPosition;

                isFloorPortal = (editorRoom.Blocks[xInRoom, zInRoom].FloorPortal != null);
            }

            return true;
        }

        private static bool IsXzInBorderOrOutsideRoom(tr_room tempRoom, int x, int z)
        {
            return (x <= 0 || z <= 0 || x >= tempRoom.NumXSectors - 1 || z >= tempRoom.NumZSectors - 1);
        }

        private IEnumerable<int> GetAllReachableBoxes(int box, int zoneType, bool flipped)
        {
            var boxes = new List<int>();

            // This is a non-recursive version of the algorithm for finding all reachable boxes. 
            // Avoid recursion all the times you can!
            var stack = new Stack<int>();
            stack.Push(box);

            // All reachable boxes must have the same water flag and same flipped flag
            var isWater = (_tempRooms[_level.Rooms[_tempBoxes[box].Room]].Flags & 0x01) != 0;
            
            while (stack.Count > 0)
            {
                var next = stack.Pop();
                var last = false;

                for (int i = (_boxes[next].OverlapIndex & 0x3fff); i < _overlaps.Length && !last; i++)
                {
                    int overlapIndex = i;
                    last = (_overlaps[overlapIndex] & 0x8000) != 0;

                    var boxIndex = _overlaps[overlapIndex] & 0x7ff;

                    var add = false;

                    // Enemies like scorpions, mummies, dogs, wild boars. They can go only on land, and climb 1 click step
                    if (zoneType == 1)
                    {
                        var water = (_tempRooms[_level.Rooms[_tempBoxes[boxIndex].Room]].Flags & 0x01) != 0;
                        var step = Math.Abs(_boxes[next].TrueFloor - _boxes[boxIndex].TrueFloor);
                        if (water == isWater && step <= 256 && flipped == _tempBoxes[boxIndex].FlipMap) add = true;
                    }

                    // Enemies like skeletons. They can go only on land, and climb 1 click step. They can also jump 2 blocks.
                    if (zoneType == 2)
                    {
                        var water = (_tempRooms[_level.Rooms[_tempBoxes[boxIndex].Room]].Flags & 0x01) != 0;
                        var step = Math.Abs(_boxes[next].TrueFloor - _boxes[boxIndex].TrueFloor);

                        // Check all possibilities
                        var canJump = _tempBoxes[boxIndex].Jump;
                        var canClimb = Math.Abs(step) <= 256;

                        if (water == isWater && (canJump || canClimb) && flipped == _tempBoxes[boxIndex].FlipMap) add = true;
                    }

                    // Enemies like crocodiles. They can go on land and inside water, and climb 1 click step. In water they act like flying enemies.
                    if (zoneType == 3)
                    {
                        var water = (_tempRooms[_level.Rooms[_tempBoxes[boxIndex].Room]].Flags & 0x01) != 0;
                        var step = Math.Abs(_boxes[next].TrueFloor - _boxes[boxIndex].TrueFloor);
                        if (((water == isWater && step <= 256) || water)) add = true;
                    }

                    // Enemies like baddy 1 & 2. They can go only on land, and climb 4 clicks step. They can also jump 2 blocks and monkey.
                    if (zoneType == 4)
                    {
                        var water = (_tempRooms[_level.Rooms[_tempBoxes[boxIndex].Room]].Flags & 0x01) != 0;
                        var step = _boxes[boxIndex].TrueFloor - _boxes[next].TrueFloor;

                        // Check all possibilities
                        var canJump = _tempBoxes[boxIndex].Jump;
                        var canClimb = Math.Abs(step) <= 1024;
                        var canMonkey = _tempBoxes[boxIndex].Monkey;

                        if (water == isWater && (canJump || canClimb || canMonkey) && flipped == _tempBoxes[boxIndex].FlipMap) add = true;
                    }

                    // Flying enemies. Here we just check if the water flag is the same.
                    if (zoneType == 5)
                    {
                        var water = (_tempRooms[_level.Rooms[_tempBoxes[boxIndex].Room]].Flags & 0x01) != 0;
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

        private bool IsVerticallyReachable(Room room, Room destRoom)
        {
            return room == destRoom || _tempRooms[room].ReachableRooms.Any(r => r == destRoom);
        }
    }
}
