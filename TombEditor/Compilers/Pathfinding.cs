using System;
using System.Collections.Generic;

namespace TombEditor.Compilers
{
    public sealed partial class LevelCompilerTr4
    {
        private List<TrBoxAux> _tempBoxes;

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
            _tempBoxes = new List<TrBoxAux>();

            // Use decompiled code for generation of boxes and overlaps
            Dec_BuildBoxesAndOverlaps();

            // Copy boxes from the decompile struct to editor struct. To remove in the future.
            for (int i = 0; i < _decNumBoxes; i++)
            {
                TrBoxAux box = new TrBoxAux
                {
                    Xmin = (byte)_decBoxes[i].Xmin,
                    Xmax = (byte)_decBoxes[i].Xmax,
                    Zmin = (byte)_decBoxes[i].Zmin,
                    Zmax = (byte)_decBoxes[i].Zmax,
                    Room = (short)_roomsRemappingDictionary[_decBoxes[i].Room],
                    IsolatedBox = _decBoxes[i].IsolatedBox,
                    Monkey = _decBoxes[i].Monkey,
                    Jump = _decBoxes[i].Jump,
                    FlipMap = _decBoxes[i].Flipped,
                    OverlapIndex = _decBoxes[i].OverlapIndex,
                    TrueFloor = (short)(_decBoxes[i].TrueFloor * -256)
                };


                _tempBoxes.Add(box);
            }

            _overlaps = new ushort[_decNumOverlaps];
            if (_decNumOverlaps != 0)
                Array.Copy(_decOverlaps, _overlaps, _decNumOverlaps);

            _boxes = new TrBox[_tempBoxes.Count];
            _zones = new TrZone[_tempBoxes.Count];

            // Convert boxes to TR format
            for (var i = 0; i < _tempBoxes.Count; i++)
            {
                var newBox = new TrBox();
                var aux = _tempBoxes[i];
                var zone = new TrZone();

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
                            ((!flipped && !_tempBoxes[boxIndex].FlipMap) ||
                             (flipped)))
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
                            ((!flipped && !_tempBoxes[boxIndex].FlipMap) ||
                             (flipped)))
                            add = true;
                    }

                    // Enemies like crocodiles. They can go on land and inside water, and climb 1 click step. In water they act like flying enemies.
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
                            ((!flipped && !_tempBoxes[boxIndex].FlipMap) ||
                             (flipped)))
                            add = true;
                    }

                    // Flying enemies. Here we just check if the water flag is the same.
                    if (zoneType == 5)
                    {
                        var water = (_tempRooms[_roomsUnmapping[_tempBoxes[boxIndex].Room]].Flags & 0x01) != 0;
                        if (water == isWater &&
                            ((!flipped && !_tempBoxes[boxIndex].FlipMap) ||
                             (flipped)))
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
