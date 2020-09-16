using System;
using System.Collections.Generic;

namespace TombLib.LevelData.Compilers.TR5Main
{
    public sealed partial class LevelCompilerTR5Main
    {
        private void BuildPathFindingData()
        {
            ReportProgress(48, "Building pathfinding data");

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

            // Use decompiled code for generation of boxes and overlaps
            Dec_BuildBoxesAndOverlaps();

            // Convert ovelaps to TR format
            _overlaps = new List<tr5main_overlap>();
            _overlaps.AddRange(dec_overlaps);

            // Convert boxes to TR format
            _boxes = new List<tr5main_box>();
            _zones = new List<tr5main_zone>();
            for (var i = 0; i < dec_boxes.Count; i++)
            {
                var box = new tr5main_box()
                {
                    Xmin = dec_boxes[i].Xmin,
                    Xmax = dec_boxes[i].Xmax,
                    Zmin = dec_boxes[i].Zmin,
                    Zmax = dec_boxes[i].Zmax,
                    TrueFloor = -(dec_boxes[i].TrueFloor * 256),
                    OverlapIndex = dec_boxes[i].OverlapIndex,
                    Flags = dec_boxes[i].IsolatedBox ? 0x8000 : 0
                };
                _boxes.Add(box);

                var zone = new tr5main_zone()
                {
                    GroundZone1_Normal = int.MaxValue,
                    GroundZone2_Normal = int.MaxValue,
                    GroundZone3_Normal = int.MaxValue,
                    GroundZone4_Normal = int.MaxValue,
                    GroundZone5_Normal = int.MaxValue,
                    FlyZone_Normal = int.MaxValue,
                    GroundZone1_Alternate = int.MaxValue,
                    GroundZone2_Alternate = int.MaxValue,
                    GroundZone3_Alternate = int.MaxValue,
                    GroundZone4_Alternate = int.MaxValue,
                    GroundZone5_Alternate = int.MaxValue,
                    FlyZone_Alternate = int.MaxValue
                };
                _zones.Add(zone);
            }

            // Create zones
            int groundZone1 = 1;
            int groundZone2 = 1;
            int groundZone3 = 1;
            int groundZone4 = 1;
            int groundZone5 = 1;
            int flyZone = 1;
            for (var i = 0; i < _zones.Count; i++)
            {
                // Skeleton like enemis: in the future implement also jump
                if (_zones[i].GroundZone1_Normal == int.MaxValue)
                {
                    _zones[i].GroundZone1_Normal = groundZone1;

                    foreach (var box in GetAllReachableBoxes(i, 1, false))
                    {
                        if (_zones[box].GroundZone1_Normal == int.MaxValue) _zones[box].GroundZone1_Normal = groundZone1;
                    }

                    groundZone1++;
                }

                // Mummy like enemis: the simplest case
                if (_zones[i].GroundZone2_Normal == int.MaxValue)
                {
                    _zones[i].GroundZone2_Normal = groundZone2;

                    foreach (var box in GetAllReachableBoxes(i, 2, false))
                    {
                        if (_zones[box].GroundZone2_Normal == int.MaxValue) _zones[box].GroundZone2_Normal = groundZone2;
                    }

                    groundZone2++;
                }

                // Crocodile like enemis: like 1 & 2 but they can go inside water and swim
                if (_zones[i].GroundZone3_Normal == int.MaxValue)
                {
                    _zones[i].GroundZone3_Normal = groundZone3;

                    foreach (var box in GetAllReachableBoxes(i, 3, false))
                    {
                        if (_zones[box].GroundZone3_Normal == int.MaxValue) _zones[box].GroundZone3_Normal = groundZone3;
                    }

                    groundZone3++;
                }

                // Baddy like enemis: they can jump, grab and monkey
                if (_zones[i].GroundZone4_Normal == int.MaxValue)
                {
                    _zones[i].GroundZone4_Normal = groundZone4;

                    foreach (var box in GetAllReachableBoxes(i, 4, false))
                    {
                        if (_zones[box].GroundZone4_Normal == int.MaxValue) _zones[box].GroundZone4_Normal = groundZone4;
                    }

                    groundZone4++;
                }

                // Von Croy like enemis: they can jump, grab and monkey and long jump
                if (_zones[i].GroundZone5_Normal == int.MaxValue)
                {
                    _zones[i].GroundZone5_Normal = groundZone5;

                    foreach (var box in GetAllReachableBoxes(i, 5, false))
                    {
                        if (_zones[box].GroundZone5_Normal == int.MaxValue) _zones[box].GroundZone5_Normal = groundZone5;
                    }

                    groundZone5++;
                }

                // Bat like enemis: they can fly everywhere, except into the water
                if (_zones[i].FlyZone_Normal == int.MaxValue)
                {
                    _zones[i].FlyZone_Normal = flyZone;

                    foreach (var box in GetAllReachableBoxes(i, 6, false))
                    {
                        if (_zones[box].FlyZone_Normal == int.MaxValue) _zones[box].FlyZone_Normal = flyZone;
                    }

                    flyZone++;
                }
            }

            // Flipped rooms------------------------------------------
            int aGroundZone1 = 1;
            int aGroundZone2 = 1;
            int aGroundZone3 = 1;
            int aGroundZone4 = 1;
            int aGroundZone5 = 1;
            int aFlyZone = 1;
            for (var i = 0; i < _zones.Count; i++)
            {
                // Skeleton like enemis: in the future implement also jump
                if (_zones[i].GroundZone1_Alternate == int.MaxValue)
                {
                    _zones[i].GroundZone1_Alternate = aGroundZone1;

                    foreach (var box in GetAllReachableBoxes(i, 1, true))
                    {
                        if (_zones[box].GroundZone1_Alternate == int.MaxValue) _zones[box].GroundZone1_Alternate = aGroundZone1;
                    }

                    aGroundZone1++;
                }

                // Mummy like enemis: the simplest case
                if (_zones[i].GroundZone2_Alternate == int.MaxValue)
                {
                    _zones[i].GroundZone2_Alternate = aGroundZone2;

                    foreach (var box in GetAllReachableBoxes(i, 2, true))
                    {
                        if (_zones[box].GroundZone2_Alternate == int.MaxValue) _zones[box].GroundZone2_Alternate = aGroundZone2;
                    }

                    aGroundZone2++;
                }

                // Crocodile like enemis: like 1 & 2 but they can go inside water and swim
                if (_zones[i].GroundZone3_Alternate == int.MaxValue)
                {
                    _zones[i].GroundZone3_Alternate = aGroundZone3;

                    foreach (var box in GetAllReachableBoxes(i, 3, true))
                    {
                        if (_zones[box].GroundZone3_Alternate == int.MaxValue) _zones[box].GroundZone3_Alternate = aGroundZone3;
                    }

                    aGroundZone3++;
                }

                // Baddy like enemis: they can jump, grab and monkey
                if (_zones[i].GroundZone4_Alternate == int.MaxValue)
                {
                    _zones[i].GroundZone4_Alternate = aGroundZone4;

                    foreach (var box in GetAllReachableBoxes(i, 4, true))
                    {
                        if (_zones[box].GroundZone4_Alternate == int.MaxValue) _zones[box].GroundZone4_Alternate = aGroundZone4;
                    }

                    aGroundZone4++;
                }

                // Von Croy like enemis: they can jump, grab and monkey and long jump
                if (_zones[i].GroundZone5_Alternate == int.MaxValue)
                {
                    _zones[i].GroundZone5_Alternate = aGroundZone5;

                    foreach (var box in GetAllReachableBoxes(i, 5, true))
                    {
                        if (_zones[box].GroundZone5_Alternate == int.MaxValue) _zones[box].GroundZone5_Alternate = aGroundZone5;
                    }

                    aGroundZone5++;
                }

                // Bat like enemis: they can fly everywhere, except into the water
                if (_zones[i].FlyZone_Alternate == int.MaxValue)
                {
                    _zones[i].FlyZone_Alternate = aFlyZone;

                    foreach (var box in GetAllReachableBoxes(i, 6, true))
                    {
                        if (_zones[box].FlyZone_Alternate == int.MaxValue) _zones[box].FlyZone_Alternate = aFlyZone;
                    }

                    aFlyZone++;
                }
            }

            ReportProgress(52, "    Number of boxes/zones: " + _boxes.Count);
            ReportProgress(52, "    Number of overlaps: " + _overlaps.Count);
        }

        private IEnumerable<int> GetAllReachableBoxes(int box, int zoneType, bool flipped)
        {
            var boxes = new List<int>();

            // HACK: boxes with no overlaps have overlapIndex = -1
            if (_boxes[box].OverlapIndex < 0)
                return boxes;

            // This is a non-recursive version of the algorithm for finding all reachable boxes.
            // Avoid recursion all the times you can!
            var stack = new Stack<int>();
            stack.Push(box);

            // All reachable boxes must have the same water flag and same flipped flag
            var isWater = (_tempRooms[dec_boxes[box].Room].Flags & 0x01) != 0;

            while (stack.Count > 0)
            {
                var next = stack.Pop();
                var last = false;

                for (int i = _boxes[next].OverlapIndex; i < _overlaps.Count && !last; i++)
                {
                    int overlapIndex = i;
                    last = (_overlaps[overlapIndex].Flags & 0x8000) != 0;

                    bool canJump = (_overlaps[overlapIndex].Flags & 0x800) != 0;
                    bool canMonkey = (_overlaps[overlapIndex].Flags & 0x2000) != 0;
                    bool canLongJump = (_overlaps[overlapIndex].Flags & 0x400) != 0;

                    var boxIndex = _overlaps[overlapIndex].Box;

                    var add = false;

                    // Enemies like skeletons. They can go only on land, and climb 1 click step. They can also jump 2 blocks.
                    if (zoneType == 1)
                    {
                        var water = (_tempRooms[dec_boxes[boxIndex].Room].Flags & 0x01) != 0;
                        var step = Math.Abs(_boxes[next].TrueFloor - _boxes[boxIndex].TrueFloor);

                        if (water == isWater && (canJump || step <= 256) &&
                            (!flipped && dec_boxes[boxIndex].Flag0x04 ||
                            flipped && dec_boxes[boxIndex].Flag0x02))
                            add = true;
                    }

                    // Enemies like scorpions, mummies, dogs, wild boars. They can go only on land, and climb 1 click step
                    if (zoneType == 2)
                    {
                        var water = (_tempRooms[dec_boxes[boxIndex].Room].Flags & 0x01) != 0;
                        var step = Math.Abs(_boxes[next].TrueFloor - _boxes[boxIndex].TrueFloor);
                        if (water == isWater && step <= 256 &&
                            (!flipped && dec_boxes[boxIndex].Flag0x04 ||
                            flipped && dec_boxes[boxIndex].Flag0x02))
                            add = true;
                    }

                    // Enemies like crocodiles. They can go on land and inside water, and climb 1 click step.
                    // In water they act like flying enemies. Guide seems to belong to this zone.
                    if (zoneType == 3)
                    {
                        var water = (_tempRooms[dec_boxes[boxIndex].Room].Flags & 0x01) != 0;
                        var step = Math.Abs(_boxes[next].TrueFloor - _boxes[boxIndex].TrueFloor);
                        if ((water == isWater && step <= 256 || water) &&
                            (!flipped && dec_boxes[boxIndex].Flag0x04 ||
                            flipped && dec_boxes[boxIndex].Flag0x02)) 
                            add = true;
                    }

                    // Enemies like baddy 1 & 2. They can go only on land, and climb 4 clicks step. They can also jump 2 blocks and monkey.
                    if (zoneType == 4)
                    {
                        var water = (_tempRooms[dec_boxes[boxIndex].Room].Flags & 0x01) != 0;
                        var step = Math.Abs(_boxes[boxIndex].TrueFloor - _boxes[next].TrueFloor);

                        if (water == isWater && (canJump || step <= 1024 || canMonkey) && !canLongJump &&
                            (!flipped && dec_boxes[boxIndex].Flag0x04 ||
                            flipped && dec_boxes[boxIndex].Flag0x02))
                            add = true;
                    }

                    // Enemies like Von Croy. They can go only on land, and climb 7 clicks step. They can also jump 3 blocks and monkey.
                    if (zoneType == 5)
                    {
                        var water = (_tempRooms[dec_boxes[boxIndex].Room].Flags & 0x01) != 0;
                        var step = Math.Abs(_boxes[boxIndex].TrueFloor - _boxes[next].TrueFloor);

                        if (water == isWater && (canJump || step <= 1792 || canMonkey || canLongJump) &&
                            (!flipped && dec_boxes[boxIndex].Flag0x04 ||
                            flipped && dec_boxes[boxIndex].Flag0x02))
                            add = true;
                    }

                    // Flying enemies. Here we just check if the water flag is the same.
                    if (zoneType == 6)
                    {
                        var water = (_tempRooms[dec_boxes[boxIndex].Room].Flags & 0x01) != 0;
                        if ((!flipped && dec_boxes[boxIndex].Flag0x04 ||
                            flipped && dec_boxes[boxIndex].Flag0x02))
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
