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
                box.Room = (short)_roomsRemappingDictionary[dec_boxes[i].Room];
                box.IsolatedBox = dec_boxes[i].IsolatedBox;
                box.Monkey = dec_boxes[i].Monkey;
                box.Jump = dec_boxes[i].Jump;
                box.OverlapIndex = dec_boxes[i].OverlapIndex;
                box.TrueFloor = (short)(dec_boxes[i].TrueFloor * -256);
                box.Flag0x04 = dec_boxes[i].Flag0x04;
                box.Flag0x02 = dec_boxes[i].Flag0x02;

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

                if (aux.IsolatedBox) newBox.OverlapIndex = (short)(newBox.OverlapIndex | 0x8000);

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

            // Init zones with default values
            for (var i = 0; i < _boxes.Length; i++)
            {
                _zones[i].GroundZone1_Normal = 0x7ff;
                _zones[i].GroundZone2_Normal = 0x7ff;
                _zones[i].GroundZone3_Normal = 0x7ff;
                _zones[i].GroundZone4_Normal = 0x7ff;
                _zones[i].FlyZone_Normal = 0x7ff;
                _zones[i].GroundZone1_Alternate = 0x7ff;
                _zones[i].GroundZone2_Alternate = 0x7ff;
                _zones[i].GroundZone3_Alternate = 0x7ff;
                _zones[i].GroundZone4_Alternate = 0x7ff;
                _zones[i].FlyZone_Alternate = 0x7ff;
            }

            // Create zones
            for (var i = 0; i < _boxes.Length; i++)
            {
                // Skeleton like enemis: in the future implement also jump
                if (_zones[i].GroundZone1_Normal == 0x7ff)
                {
                    _zones[i].GroundZone1_Normal = groundZone1;

                    foreach (var box in GetAllReachableBoxes(i, 1, false))
                    {
                        if (_zones[box].GroundZone1_Normal == 0x7ff) _zones[box].GroundZone1_Normal = groundZone1;
                    }

                    groundZone1++;
                }

                // Mummy like enemis: the simplest case
                if (_zones[i].GroundZone2_Normal == 0x7ff)
                {
                    _zones[i].GroundZone2_Normal = groundZone2;

                    foreach (var box in GetAllReachableBoxes(i, 2, false))
                    {
                        if (_zones[box].GroundZone2_Normal == 0x7ff) _zones[box].GroundZone2_Normal = groundZone2;
                    }

                    groundZone2++;
                }

                // Crocodile like enemis: like 1 & 2 but they can go inside water and swim
                if (_zones[i].GroundZone3_Normal == 0x7ff)
                {
                    _zones[i].GroundZone3_Normal = groundZone3;

                    foreach (var box in GetAllReachableBoxes(i, 3, false))
                    {
                        if (_zones[box].GroundZone3_Normal == 0x7ff) _zones[box].GroundZone3_Normal = groundZone3;
                    }

                    groundZone3++;
                }

                // Baddy like enemis: they can jump, grab and monkey
                if (_zones[i].GroundZone4_Normal == 0x7ff)
                {
                    _zones[i].GroundZone4_Normal = groundZone4;

                    foreach (var box in GetAllReachableBoxes(i, 4, false))
                    {
                        if (_zones[box].GroundZone4_Normal == 0x7ff) _zones[box].GroundZone4_Normal = groundZone4;
                    }

                    groundZone4++;
                }

                // Bat like enemis: they can fly everywhere, except into the water
                if (_zones[i].FlyZone_Normal == 0x7ff)
                {
                    _zones[i].FlyZone_Normal = flyZone;

                    foreach (var box in GetAllReachableBoxes(i, 5, false))
                    {
                        if (_zones[box].FlyZone_Normal == 0x7ff) _zones[box].FlyZone_Normal = flyZone;
                    }

                    flyZone++;
                }
            }

            // Flipped rooms------------------------------------------
            for (var i = 0; i < _boxes.Length; i++)
            {
                // Skeleton like enemis: in the future implement also jump
                if (_zones[i].GroundZone1_Alternate == 0x7ff)
                {
                    _zones[i].GroundZone1_Alternate = aGroundZone1;

                    foreach (var box in GetAllReachableBoxes(i, 1, true))
                    {
                        if (_zones[box].GroundZone1_Alternate == 0x7ff) _zones[box].GroundZone1_Alternate = aGroundZone1;
                    }

                    aGroundZone1++;
                }

                // Mummy like enemis: the simplest case
                if (_zones[i].GroundZone2_Alternate == 0x7ff)
                {
                    _zones[i].GroundZone2_Alternate = aGroundZone2;

                    foreach (var box in GetAllReachableBoxes(i, 2, true))
                    {
                        if (_zones[box].GroundZone2_Alternate == 0x7ff) _zones[box].GroundZone2_Alternate = aGroundZone2;
                    }

                    aGroundZone2++;
                }

                // Crocodile like enemis: like 1 & 2 but they can go inside water and swim
                if (_zones[i].GroundZone3_Alternate == 0x7ff)
                {
                    _zones[i].GroundZone3_Alternate = aGroundZone3;

                    foreach (var box in GetAllReachableBoxes(i, 3, true))
                    {
                        if (_zones[box].GroundZone3_Alternate == 0x7ff) _zones[box].GroundZone3_Alternate = aGroundZone3;
                    }

                    aGroundZone3++;
                }

                // Baddy like enemis: they can jump, grab and monkey
                if (_zones[i].GroundZone4_Alternate == 0x7ff)
                {
                    _zones[i].GroundZone4_Alternate = aGroundZone4;

                    foreach (var box in GetAllReachableBoxes(i, 4, true))
                    {
                        if (_zones[box].GroundZone4_Alternate == 0x7ff) _zones[box].GroundZone4_Alternate = aGroundZone4;
                    }

                    aGroundZone4++;
                }

                // Bat like enemis: they can fly everywhere, except into the water
                if (_zones[i].FlyZone_Alternate == 0x7ff)
                {
                    _zones[i].FlyZone_Alternate = aFlyZone;

                    foreach (var box in GetAllReachableBoxes(i, 5, true))
                    {
                        if (_zones[box].FlyZone_Alternate == 0x7ff) _zones[box].FlyZone_Alternate = aFlyZone;
                    }

                    aFlyZone++;
                }
            }

            ReportProgress(60, "    Number of boxes: " + _boxes.Length);
            ReportProgress(60, "    Number of overlaps: " + _overlaps.Length);
            ReportProgress(60, "    Number of zones: " + _boxes.Length);
        }

        private IEnumerable<int> GetAllReachableBoxes(int box, int zoneType, bool flipped)
        {
            var boxes = new List<int>();

            // This is a non-recursive version of the algorithm for finding all reachable boxes. 
            // Avoid recursion all the times you can!
            var stack = new Stack<int>();
            stack.Push(box);

            // All reachable boxes must have the same water flag and same flipped flag
            var isWater = (_tempRooms[_roomsUnmapping[_tempBoxes[box].Room]].Flags & 0x01) != 0;
            
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
                        var water = (_tempRooms[_roomsUnmapping[_tempBoxes[boxIndex].Room]].Flags & 0x01) != 0;
                        var step = Math.Abs(_boxes[next].TrueFloor - _boxes[boxIndex].TrueFloor);
                        if (water == isWater && step <= 256 && 
                            ((!flipped  && _tempBoxes[boxIndex].Flag0x04) || 
                            (flipped && _tempBoxes[boxIndex].Flag0x02))) 
                            add = true;
                    }

                    // Enemies like skeletons. They can go only on land, and climb 1 click step. They can also jump 2 blocks.
                    if (zoneType == 2)
                    {
                        var water = (_tempRooms[_roomsUnmapping[_tempBoxes[boxIndex].Room]].Flags & 0x01) != 0;
                        var step = Math.Abs(_boxes[next].TrueFloor - _boxes[boxIndex].TrueFloor);

                        // Check all possibilities
                        var canJump = _tempBoxes[boxIndex].Jump;
                        var canClimb = Math.Abs(step) <= 256;

                        if (water == isWater && (canJump || canClimb) &&
                            ((!flipped && _tempBoxes[boxIndex].Flag0x04) ||
                            (flipped && _tempBoxes[boxIndex].Flag0x02)))
                            add = true;
                    }

                    // Enemies like crocodiles. They can go on land and inside water, and climb 1 click step. 
                    // In water they act like flying enemies. Guide seems to belong to this zone.
                    if (zoneType == 3)
                    {
                        var water = (_tempRooms[_roomsUnmapping[_tempBoxes[boxIndex].Room]].Flags & 0x01) != 0;
                        var step = Math.Abs(_boxes[next].TrueFloor - _boxes[boxIndex].TrueFloor);
                        if (((water == isWater && step <= 256) || water)) add = true;
                    }

                    // Enemies like baddy 1 & 2. They can go only on land, and climb 4 clicks step. They can also jump 2 blocks and monkey.
                    if (zoneType == 4)
                    {
                        var water = (_tempRooms[_roomsUnmapping[_tempBoxes[boxIndex].Room]].Flags & 0x01) != 0;
                        var step = _boxes[boxIndex].TrueFloor - _boxes[next].TrueFloor;

                        // Check all possibilities
                        var canJump = _tempBoxes[boxIndex].Jump;
                        var canClimb = Math.Abs(step) <= 1024;
                        var canMonkey = _tempBoxes[boxIndex].Monkey;

                        if (water == isWater && (canJump || canClimb || canMonkey) &&
                            ((!flipped && _tempBoxes[boxIndex].Flag0x04) ||
                            (flipped && _tempBoxes[boxIndex].Flag0x02)))
                            add = true;
                    }

                    // Flying enemies. Here we just check if the water flag is the same.
                    if (zoneType == 5)
                    {
                        var water = (_tempRooms[_roomsUnmapping[_tempBoxes[boxIndex].Room]].Flags & 0x01) != 0;
                        if (water == isWater &&
                            ((!flipped && _tempBoxes[boxIndex].Flag0x04) ||
                            (flipped && _tempBoxes[boxIndex].Flag0x02)))
                            add = true;
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
    }
}
