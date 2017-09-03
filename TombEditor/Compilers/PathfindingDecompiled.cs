using System;
using SharpDX;
using TombEditor.Geometry;
using System.Runtime.CompilerServices;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;

namespace TombEditor.Compilers
{
    [SuppressMessage("ReSharper", "UnusedMember.Global")]
    [SuppressMessage("ReSharper", "UnassignedField.Global")]
    public struct DecTrBoxAux
    {
        public int Zmin;
        public int Zmax;
        public int Xmin;
        public int Xmax;
        public short TrueFloor;
        public short Clicks;
        public short OverlapIndex;
        public byte NumXSectors;
        public byte NumZSectors;
        public bool IsolatedBox;
        public bool NotWalkableBox;
        public bool Monkey;
        public bool Jump;
        public Room Room;
        public bool Water;
        public int IsBaseRoom;
        public int IsAlternateRoom;
        public bool Flipped;
        public short ZoneId;
    }

    public sealed partial class LevelCompilerTr4
    {
        private bool _decGraybox;
        private bool _decWater = true;
        private bool _decMonkey;
        private bool _decFlipped;
        private bool _decJump;
        private Room _decCurrentRoom;
        private short _decQ0 = -1;
        private short _decQ1 = -1;
        private short _decQ2 = -1;
        private short _decQ3 = -1;
        private DecTrBoxAux[] _decBoxes;
        private ushort[] _decOverlaps;
        private int _decNumBoxes;
        private int _decNumOverlaps;
        private bool _decBoxExtendsInAnotherRoom;

        private void Dec_BuildBoxesAndOverlaps()
        {
            _decCurrentRoom = Level.Rooms[0];
            _decBoxes = new DecTrBoxAux[2040];

            var watch = new Stopwatch();
            watch.Start();

            for (int flipped = 0; flipped < 2; flipped++)
            {
                foreach (var room in Level.Rooms)
                {
                    // Room must be defined and also must be base room or the flipped version
                    if (room == null || ((flipped != 0 || room.AlternateBaseRoom != null) &&
                                         (flipped != 1 || room.AlternateBaseRoom == null)))
                        continue;

                    var tempRoom = _tempRooms[room];
                    for (int z = 0; z < room.NumZSectors; z++)
                    {
                        for (int x = 0; x < room.NumXSectors; x++)
                        {
                            if (!room.ExcludeFromPathFinding)
                            {
                                var box = new DecTrBoxAux();

                                // First create the box...
                                int boxIndex;
                                if (x != 0 &&
                                    z != 0 &&
                                    x != room.NumXSectors - 1 &&
                                    z != room.NumZSectors - 1 &&
                                    Dec_CreateNewBox(ref box, x, z, room))
                                {
                                    // ...then try to add it to the box array
                                    boxIndex = Dec_AddBox(ref box);
                                    if (boxIndex < 0) return;
                                }
                                else
                                {
                                    boxIndex = 0x7ff;
                                }

                                // Assign the box index to the sector
                                tempRoom.Sectors[tempRoom.NumZSectors * x + z].BoxIndex =
                                    (short)((boxIndex << 4) | (int)GetTextureSound(room, x, z));
                            }
                            else
                            {
                                tempRoom.Sectors[tempRoom.NumZSectors * x + z].BoxIndex =
                                    (short)((0x7ff << 4) | (int)GetTextureSound(room, x, z));
                            }
                        }
                    }
                }

                // Originally a FlipAllRooms() function was called. This function swap all base rooms with flipped room. 
                // I've decided to simply set this global variable to true and, in each case, take the correct room.
                _decFlipped = true;
            }

            _decFlipped = false;

            watch.Stop();
            Console.WriteLine(
                $@"Dec_BuildBoxesAndOverlaps() -> Build boxes: {watch.ElapsedMilliseconds} ms, Count = {_decNumBoxes}");

            watch.Restart();

            // Build the overlaps
            Dec_BuildOverlaps();

            watch.Stop();
            Console.WriteLine(
                $@"Dec_BuildBoxesAndOverlaps() -> Build overlaps: {watch.ElapsedMilliseconds} ms, Count = {
                        _decNumOverlaps
                    }");
        }

        private void Dec_BuildOverlaps()
        {
            _decNumOverlaps = 0;
            _decOverlaps = new ushort[16384];

            int i = 0;

            do
            {
                var box1 = _decBoxes[i];
                _decBoxes[i].OverlapIndex = 0x7ff;

                int j;
                if (!box1.Flipped)
                {
                    if (_decFlipped)
                    {
                        _decFlipped = false;
                    }

                    j = 0;
                    do
                    {
                        if (i != j)
                        {
                            if (i % 50 == 0 && j % 50 == 0) Console.WriteLine($@"Checking overlap {i} vs {j}");
                            var box2 = _decBoxes[j];

                            if (!box2.Flipped)
                            {
                                if (Dec_BoxesOverlap(ref box1, ref box2))
                                {
                                    if (_decNumOverlaps == 16384) return;
                                    if (_decBoxes[i].OverlapIndex == 0x7ff)
                                        _decBoxes[i].OverlapIndex = (short)_decNumOverlaps;

                                    _decOverlaps[_decNumOverlaps] = (ushort)j;

                                    if (_decJump) _decOverlaps[_decNumOverlaps] |= 0x800;
                                    if (_decMonkey) _decOverlaps[_decNumOverlaps] |= 0x2000;

                                    _decNumOverlaps++;
                                }
                            }
                        }

                        j++;
                    } while (j < _decNumBoxes);
                }

                if (box1.Flipped)
                {
                    if (!_decFlipped)
                    {
                        _decFlipped = true;
                    }

                    j = 0;
                    do
                    {
                        if (i != j)
                        {
                            DecTrBoxAux box2 = _decBoxes[j];

                            if (box2.Flipped)
                            {
                                if (Dec_BoxesOverlap(ref box1, ref box2))
                                {
                                    if (_decNumOverlaps == 16384) return;
                                    if (_decBoxes[i].OverlapIndex == 0x7ff)
                                        _decBoxes[i].OverlapIndex = (short)_decNumOverlaps;

                                    _decOverlaps[_decNumOverlaps] = (ushort)j;

                                    if (_decJump) _decOverlaps[_decNumOverlaps] |= 0x800;
                                    if (_decMonkey) _decOverlaps[_decNumOverlaps] |= 0x2000;

                                    _decNumOverlaps++;
                                }
                            }
                        }

                        j++;
                    } while (j < _decNumBoxes);
                }

                i++;

                _decOverlaps[_decNumOverlaps - 1] |= 0x8000;
            } while (i < _decNumBoxes);

            _decFlipped = false;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private int Dec_AddBox(ref DecTrBoxAux box)
        {
            if (_decNumBoxes == 2040) return -1;

            int boxIndex = -1;

            for (int i = 0; i < _decNumBoxes; i++)
            {
                if (_decBoxes[i].Xmin != box.Xmin || _decBoxes[i].Xmax != box.Xmax || _decBoxes[i].Zmin != box.Zmin ||
                    _decBoxes[i].Zmax != box.Zmax || _decBoxes[i].TrueFloor != box.TrueFloor ||
                    _decBoxes[i].Water != box.Water)
                    continue;

                boxIndex = i;
                break;
            }

            if (boxIndex != -1)
                return boxIndex;

            boxIndex = _decNumBoxes;
            box.OverlapIndex = 0x7ff;
            _decBoxes[_decNumBoxes] = box;
            _decNumBoxes++;

            return boxIndex;
        }

        private bool Dec_CreateNewBox(ref DecTrBoxAux box, int x, int z, Room theRoom)
        {
            bool monkey = false;

            var room = theRoom;
            var block = room.Blocks[x, z];

            // Check if current block is a not walkable sector
            if ((block.Flags & BlockFlags.NotWalkableFloor) != 0) return false;

            if (block.Type == BlockType.Wall ||
                block.Type == BlockType.BorderWall ||
                block.FloorOpacity == PortalOpacity.Opacity1 /*||
                block.CeilingOpacity == PortalOpacity.Opacity1*/)
            {
                return false;
            }

            _decQ0 = block.QAFaces[0];
            _decQ1 = block.QAFaces[1];
            _decQ2 = block.QAFaces[2];
            _decQ3 = block.QAFaces[3];

            int currentX = (int)room.Position.X + x;
            int currentZ = (int)room.Position.Z + z;

            _decCurrentRoom = theRoom;

            Dec_CanSectorBeReachedAndIsSolid(currentX, currentZ);

            _decGraybox = false;
            _decWater = true;
            _decMonkey = false;

            short floor = (short)Dec_GetBoxFloorHeight(currentX, currentZ);
            box.TrueFloor = floor;

            if (floor == 0x7fff) return false;

            box.Room = _decCurrentRoom;
            box.Water = room.FlagWater;

            box.Flipped = _decFlipped;

            if (_decMonkey)
            {
                box.Monkey = true;
                monkey = true;
            }

            if (!_decWater)
            {
                box.Water = false;
            }

            if (_decGraybox)
            {
                box.Xmin = currentX;
                box.Zmin = currentZ;
                box.Xmax = currentX + 1;
                box.Zmax = currentZ + 1;
                box.IsolatedBox = true;

                return true;
            }
            else
            {
                _decGraybox = true;

                int direction = 0x0f;
                int directionBase = 0x0f;

                int xMin = currentX;
                int xMax = currentX;
                int zMin = currentZ;
                int zMax = currentZ;

                var currentRoom1 = theRoom;
                var currentRoom2 = theRoom;
                var currentRoom3 = theRoom;
                var currentRoom4 = theRoom;

                while (true)
                {
                    Room currentRoom;
                    int searchX;
                    if ((directionBase & 0x04) == 0x04)
                    {
                        _decBoxExtendsInAnotherRoom = false;
                        _decCurrentRoom = currentRoom1;
                        currentRoom = currentRoom1;

                        searchX = xMin;

                        if (xMin <= xMax)
                        {
                            bool finishedDirection = true;

                            while (floor == Dec_GetBoxFloorHeight(searchX, zMin) &&
                                   floor == Dec_GetBoxFloorHeight(searchX, zMin - 1) &&
                                   _decMonkey == monkey)
                            {
                                if (searchX == xMin) currentRoom1 = _decCurrentRoom;

                                if (_decBoxExtendsInAnotherRoom)
                                {
                                    // If the box goes in another room and one of current rooms has a flipped room that stop now
                                    if (_decCurrentRoom != currentRoom &&
                                        (_decCurrentRoom.AlternateRoom != null ||
                                         currentRoom.AlternateRoom != null))
                                    {
                                        break;
                                    }

                                    // Reset current room index to start room index
                                    _decCurrentRoom = currentRoom;

                                    // If floor of starting block is != floor of block (X, Z - 1) exit loop
                                    if (floor != Dec_GetBoxFloorHeight(searchX, zMin - 1)) break;

                                    // Reset flag of box extended in another room
                                    _decBoxExtendsInAnotherRoom = false;
                                }

                                searchX++;

                                if (searchX > xMax)
                                {
                                    finishedDirection = false;
                                    break;
                                }
                            }

                            if (finishedDirection) direction -= 0x04;
                        }

                        directionBase = direction;
                        if ((directionBase & 0x04) == 0x04) zMin--;
                    }

                    int searchZ;
                    if ((directionBase & 0x02) == 0x02)
                    {
                        _decBoxExtendsInAnotherRoom = false;
                        _decCurrentRoom = currentRoom2;
                        currentRoom = currentRoom2;

                        searchZ = zMin;

                        if (zMin <= zMax)
                        {
                            bool finishedDirection = true;

                            while (floor == Dec_GetBoxFloorHeight(xMax, searchZ) &&
                                   floor == Dec_GetBoxFloorHeight(xMax + 1, searchZ) &&
                                   _decMonkey == monkey)
                            {
                                if (searchZ == zMin) currentRoom2 = _decCurrentRoom;

                                if (_decBoxExtendsInAnotherRoom)
                                {
                                    // If the box goes in another room and one of current rooms has a flipped room that stop now
                                    if (_decCurrentRoom != currentRoom &&
                                        (_decCurrentRoom.AlternateRoom != null ||
                                         currentRoom.AlternateRoom != null))
                                    {
                                        break;
                                    }

                                    // Reset current room index to start room index
                                    _decCurrentRoom = currentRoom;

                                    // If floor of starting block is != floor of block (X, Z - 1) exit loop
                                    if (floor != Dec_GetBoxFloorHeight(xMax + 1, searchZ)) break;

                                    // Reset flag of box extended in another room
                                    _decBoxExtendsInAnotherRoom = false;
                                }

                                searchZ++;

                                if (searchZ > zMax)
                                {
                                    finishedDirection = false;
                                    break;
                                }
                            }

                            if (finishedDirection) direction -= 0x02;
                        }

                        directionBase = direction;
                        if ((directionBase & 0x02) == 0x02) xMax++;
                    }

                    if ((directionBase & 0x08) == 0x08)
                    {
                        _decBoxExtendsInAnotherRoom = false;
                        _decCurrentRoom = currentRoom3;
                        currentRoom = currentRoom3;

                        searchX = xMax;

                        if (xMax >= xMin)
                        {
                            bool finishedDirection = true;

                            while (floor == Dec_GetBoxFloorHeight(searchX, zMax) &&
                                   floor == Dec_GetBoxFloorHeight(searchX, zMax + 1) &&
                                   _decMonkey == monkey)
                            {
                                if (searchX == xMax) currentRoom3 = _decCurrentRoom;

                                if (_decBoxExtendsInAnotherRoom)
                                {
                                    // If the box goes in another room and one of current rooms has a flipped room that stop now
                                    if (_decCurrentRoom != currentRoom &&
                                        (_decCurrentRoom.AlternateRoom != null ||
                                         currentRoom.AlternateRoom != null))
                                    {
                                        break;
                                    }

                                    // Reset current room index to start room index
                                    _decCurrentRoom = currentRoom;

                                    // If floor of starting block is != floor of block (X, Z + 1) exit loop
                                    if (floor != Dec_GetBoxFloorHeight(searchX, zMax + 1)) break;

                                    // Reset flag of box extended in another room
                                    _decBoxExtendsInAnotherRoom = false;
                                }

                                searchX--;

                                if (searchX >= xMin)
                                    continue;

                                finishedDirection = false;
                                break;
                            }

                            if (finishedDirection) direction -= 0x08;
                        }

                        directionBase = direction;
                        if ((directionBase & 0x08) == 0x08) zMax++;
                    }

                    if ((directionBase & 0x01) == 0x01)
                    {
                        _decBoxExtendsInAnotherRoom = false;
                        _decCurrentRoom = currentRoom4;
                        currentRoom = currentRoom4;

                        searchZ = zMax;

                        if (zMax >= zMin)
                        {
                            bool finishedDirection = true;

                            while (floor == Dec_GetBoxFloorHeight(xMin, searchZ) &&
                                   floor == Dec_GetBoxFloorHeight(xMin - 1, searchZ) &&
                                   _decMonkey == monkey)
                            {
                                if (searchZ == zMax) currentRoom4 = _decCurrentRoom;

                                if (_decBoxExtendsInAnotherRoom)
                                {
                                    // If the box goes in another room and one of current rooms has a flipped room that stop now
                                    if (_decCurrentRoom != currentRoom &&
                                        (_decCurrentRoom.AlternateRoom != null ||
                                         currentRoom.AlternateRoom != null))
                                    {
                                        break;
                                    }

                                    // Reset current room index to start room index
                                    _decCurrentRoom = currentRoom;

                                    // If floor of starting block is != floor of block (X, Z - 1) exit loop
                                    if (floor != Dec_GetBoxFloorHeight(xMin - 1, searchZ)) break;

                                    // Reset flag of box extended in another room
                                    _decBoxExtendsInAnotherRoom = false;
                                }

                                searchZ--;

                                if (searchZ < zMin)
                                {
                                    finishedDirection = false;
                                    break;
                                }
                            }

                            if (finishedDirection) direction -= 0x01;
                        }

                        directionBase = direction;
                        if ((directionBase & 0x01) == 0x01) xMin--;
                    }

                    if (directionBase == 0x00) break;
                }

                box.Xmin = xMin;
                box.Zmin = zMin;
                box.Xmax = xMax + 1;
                box.Zmax = zMax + 1;

                return true;
            }
        }

        private bool Dec_CanSectorBeReachedAndIsSolid(int x, int z)
        {
            bool borderOrOutside = Dec_IsOutsideOrdBorderRoom(x, z);
            if (!borderOrOutside)
                return true;

            var theRoom = _decCurrentRoom;

            int xInRoom;
            int zInRoom;
            Room room;
            Block block;
            while (true)
            {
                room = theRoom;

                xInRoom = x - (int)room.Position.X;
                zInRoom = z - (int)room.Position.Z;

                if (xInRoom >= 0)
                {
                    if (xInRoom >= room.NumXSectors)
                        xInRoom = room.NumXSectors - 1;
                }
                else
                {
                    xInRoom = 0;
                }

                if (zInRoom >= 0)
                {
                    if (zInRoom >= room.NumZSectors)
                        zInRoom = room.NumZSectors - 1;
                }
                else
                {
                    zInRoom = 0;
                }

                block = room.Blocks[xInRoom, zInRoom];

                // HACK: this code was not inside the original functions but the procedure fails if xInRoom and zInRoom are one of the 4 cornes.
                // This happen for example when there are 3 room connected together and the corner is inside the box. 
                // In this case, there are portals but the function can't travel to neighbour rooms because is stuck in the corner.
                // For now I assume that the dest X, Z can't be reached.
                if (xInRoom == 0 && zInRoom == 0 ||
                    xInRoom == 0 && zInRoom == room.NumZSectors - 1 ||
                    xInRoom == room.NumXSectors - 1 && zInRoom == 0 ||
                    xInRoom == room.NumXSectors - 1 && zInRoom == room.NumZSectors - 1) return false;

                if (block.WallPortal == null) break;

                var adjoiningRoom = block.WallPortal.AdjoiningRoom;
                if (adjoiningRoom.AlternateRoom != null && _decFlipped) adjoiningRoom = adjoiningRoom.AlternateRoom;

                _decCurrentRoom = adjoiningRoom;
                theRoom = adjoiningRoom;

                if (block.WallOpacity == PortalOpacity.Opacity1) return false;

                if (!Dec_IsOutsideOrdBorderRoom(x, z)) break;
            }

            room = _decCurrentRoom;

            xInRoom = x - (int)room.Position.X;
            zInRoom = z - (int)room.Position.Z;

            block = room.Blocks[xInRoom, zInRoom];

            // After having probed that we can reach X, Z from the original room, do the following
            while (!room.IsFloorSolid(new DrawingPoint(xInRoom, zInRoom)))
            {
                var adjoiningRoom = block.FloorPortal.AdjoiningRoom;
                if (adjoiningRoom.AlternateRoom != null && _decFlipped) adjoiningRoom = adjoiningRoom.AlternateRoom;

                if (block.FloorOpacity == PortalOpacity.Opacity1 &&
                    !(room.FlagWater ^ adjoiningRoom.FlagWater))
                {
                    break;
                }

                _decCurrentRoom = adjoiningRoom;

                room = _decCurrentRoom;

                xInRoom = x - (int)room.Position.X;
                zInRoom = z - (int)room.Position.Z;

                block = room.Blocks[xInRoom, zInRoom];
            }

            return true;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private bool Dec_IsOutsideOrdBorderRoom(int x, int z)
        {
            var room = _decCurrentRoom;
            return (x < 0 || z < 0 || x > room.NumXSectors - 1 || z > room.NumZSectors - 1);
        }

        private int Dec_GetBoxFloorHeight(int x, int z)
        {
            var adjoiningRoom = _decCurrentRoom;
            var room = _decCurrentRoom;

            // Ignore pathfinding for current room?
            if (_decCurrentRoom.ExcludeFromPathFinding) return 0x7fff;

            int posXblocks = (int)room.Position.X;
            int posZblocks = (int)room.Position.Z;

            int xInRoom = x - posXblocks;
            int zInRoom = z - posZblocks;

            if (xInRoom < 0 ||
                xInRoom > room.NumXSectors - 1 ||
                zInRoom < 0 ||
                zInRoom > room.NumZSectors - 1)
            {
                return 0x7fff;
            }

            var block = room.Blocks[xInRoom, zInRoom];

            // If block is a wall or is a vertical toggle opacity 1
            // Note that is & 8 because wall and border wall are the only blocks with bit 4 (0x08) set
            if (((block.Type == BlockType.Wall ||
                  block.Type == BlockType.BorderWall) && block.WallPortal == null) ||
                block.WallOpacity == PortalOpacity.Opacity1 ||
                (block.Flags & BlockFlags.NotWalkableFloor) != 0)
            {
                _decQ0 = -1;
                _decQ1 = -1;
                _decQ2 = -1;
                _decQ3 = -1;

                return 0x7fff;
            }

            // If it's not a wall portal or is vertical toggle opacity 1
            if ((block.WallPortal == null || block.WallOpacity == PortalOpacity.Opacity1))
            {
            }
            else
            {
                adjoiningRoom = block.WallPortal.AdjoiningRoom;
                if (adjoiningRoom.AlternateRoom != null && _decFlipped) adjoiningRoom = adjoiningRoom.AlternateRoom;

                _decCurrentRoom = adjoiningRoom;
                _decBoxExtendsInAnotherRoom = true;

                room = _decCurrentRoom;

                posXblocks = (int)room.Position.X;
                posZblocks = (int)room.Position.Z;

                xInRoom = x - posXblocks;
                zInRoom = z - posZblocks;

                block = room.Blocks[xInRoom, zInRoom];
            }

            var oldRoom = adjoiningRoom;

            while (!room.IsFloorSolid(new DrawingPoint(xInRoom, zInRoom)))
            {
                Room adjoiningRoom2 = block.FloorPortal.AdjoiningRoom;
                if (adjoiningRoom2.AlternateRoom != null && _decFlipped) adjoiningRoom2 = adjoiningRoom2.AlternateRoom;

                if (block.FloorOpacity == PortalOpacity.Opacity1)
                {
                    if (!(room.FlagWater ^ adjoiningRoom2.FlagWater))
                    {
                        break;
                    }
                }

                _decCurrentRoom = adjoiningRoom2;
                room = _decCurrentRoom;

                posXblocks = (int)room.Position.X;
                posZblocks = (int)room.Position.Z;

                xInRoom = x - posXblocks;
                zInRoom = z - posZblocks;

                block = room.Blocks[xInRoom, zInRoom];
            }

            if ((block.Flags & BlockFlags.NotWalkableFloor) != 0) return 0x7fff;

            int sumHeights = block.QAFaces[0] + block.QAFaces[1] + block.QAFaces[2] + block.QAFaces[3];
            int meanFloorCornerHeight = sumHeights >> 2;

            _decQ0 = block.QAFaces[0];
            _decQ1 = block.QAFaces[1];
            _decQ2 = block.QAFaces[2];
            _decQ3 = block.QAFaces[3];

            int slope1 = (Math.Abs(_decQ0 - _decQ1) >= 3 ? 1 : 0);
            int slope2 = (Math.Abs(_decQ1 - _decQ2) >= 3 ? 1 : 0);
            int slope3 = (Math.Abs(_decQ2 - _decQ3) >= 3 ? 1 : 0);
            int slope4 = (Math.Abs(_decQ3 - _decQ0) >= 3 ? 1 : 0);

            bool someFlag;

            if (block.QAFaces[0] == block.QAFaces[2])
            {
                someFlag = false;
            }
            else
            {
                if (block.QAFaces[1] != block.QAFaces[3])
                {
                    if (block.QAFaces[0] < block.QAFaces[1] && block.QAFaces[0] < block.QAFaces[3] ||
                        block.QAFaces[2] < block.QAFaces[1] && block.QAFaces[2] < block.QAFaces[3] ||
                        block.QAFaces[0] > block.QAFaces[1] && block.QAFaces[0] > block.QAFaces[3] ||
                        block.QAFaces[2] > block.QAFaces[1] && block.QAFaces[2] > block.QAFaces[3])
                    {
                        someFlag = true;
                    }
                    else
                    {
                        someFlag = false;
                    }
                }
                else
                {
                    someFlag = true;
                }
            }

            int floorHeight = meanFloorCornerHeight + (int)room.Position.Y;
            int ceiling = block.CeilingMax + (int)room.Position.Y;

            if (_decWater && room.FlagWater && (ceiling - meanFloorCornerHeight) <= 1 && block.CeilingPortal != null)
            {
                Room adjoiningRoom3 = block.CeilingPortal.AdjoiningRoom;
                if (adjoiningRoom3.AlternateRoom != null && _decFlipped) adjoiningRoom3 = adjoiningRoom3.AlternateRoom;

                if (!adjoiningRoom3.FlagWater)
                {
                    _decWater = false;
                }
            }

            _decCurrentRoom = oldRoom;

            if (slope1 + slope2 + slope4 + slope3 >= 3 || slope1 + slope3 == 2 || slope2 + slope4 == 2)
            {
                if (_decWater && !room.FlagWater) return 0x7fff;
            }
            else
            {
                if (someFlag)
                {
                    if ((slope1 == 0 || slope2 == 0) && (slope3 == 0 || slope4 == 0))
                    {
                    }
                    else
                    {
                        if (_decWater && !room.FlagWater) return 0x7fff;
                    }
                }
                else
                {
                    if (slope1 + slope4 == 2 || slope2 + slope3 == 2)
                    {
                        if (_decWater && !room.FlagWater) return 0x7fff;
                    }
                }
            }

            if ((block.Flags & BlockFlags.Box) == 0)
            {
                _decMonkey = (block.Flags & BlockFlags.Monkey) != 0;
                return floorHeight;
            }

            if (_decGraybox)
                return 0x7fff;

            _decGraybox = true;
            _decMonkey = (block.Flags & BlockFlags.Monkey) != 0;
            return floorHeight;
        }

        private bool Dec_CheckIfCanJumpX(ref DecTrBoxAux a, ref DecTrBoxAux b)
        {
            // Boxes must have the same height for jump
            if (a.TrueFloor != b.TrueFloor) return false;

            int xMin = a.Xmin;
            if (a.Xmin <= b.Xmin)
                xMin = b.Xmin;

            int xMax = a.Xmax;
            if (a.Xmax >= b.Xmax)
                xMax = b.Xmax;

            int zMin = a.Zmin;
            int zMax = b.Zmax;

            int currentX = (xMin + xMax) >> 1;

            int floor;

            if (zMax == zMin - 1)
            {
                _decCurrentRoom = b.Room;

                if (!Dec_CanSectorBeReachedAndIsSolid(currentX, zMax - 1))
                    return false;

                if (!Dec_CanSectorBeReachedAndIsSolid(currentX, zMax))
                    return false;

                floor = Dec_GetBoxFloorHeight(currentX, zMax);
                return floor <= b.TrueFloor - 2 && floor != 0x7fff;
            }

            if (zMax == zMin - 2)
            {
                _decCurrentRoom = b.Room;

                if (!Dec_CanSectorBeReachedAndIsSolid(currentX, zMax - 1))
                    return false;
                if (!Dec_CanSectorBeReachedAndIsSolid(currentX, zMax))
                    return false;
                floor = Dec_GetBoxFloorHeight(currentX, zMax);
                if (floor > b.TrueFloor - 2 || floor == 0x7fff)
                    return false;
                if (!Dec_CanSectorBeReachedAndIsSolid(currentX, zMax + 1))
                    return false;

                floor = Dec_GetBoxFloorHeight(currentX, zMax + 1);
                return floor <= b.TrueFloor - 2 && floor != 0x7fff;
            }

            zMin = b.Zmin;
            zMax = a.Zmax;

            if (zMin != zMax + 1)
            {
                if (zMin != zMax + 2) return false;

                _decCurrentRoom = a.Room;

                if (!Dec_CanSectorBeReachedAndIsSolid(currentX, zMax - 1)) return false;

                if (!Dec_CanSectorBeReachedAndIsSolid(currentX, zMax)) return false;

                floor = Dec_GetBoxFloorHeight(currentX, zMax);
                if (floor > b.TrueFloor - 2 || floor == 0x7fff) return false;

                if (!Dec_CanSectorBeReachedAndIsSolid(currentX, zMax + 1)) return false;
                floor = Dec_GetBoxFloorHeight(currentX, zMax + 1);
                return floor <= b.TrueFloor - 2 && floor != 0x7fff;
            }

            _decCurrentRoom = a.Room;

            if (!Dec_CanSectorBeReachedAndIsSolid(currentX, zMax - 1) ||
                !Dec_CanSectorBeReachedAndIsSolid(currentX, zMax)) return false;
            floor = Dec_GetBoxFloorHeight(currentX, zMax);
            return floor <= b.TrueFloor - 2 && floor != 0x7fff;
        }

        private bool Dec_CheckIfCanJumpZ(ref DecTrBoxAux a, ref DecTrBoxAux b)
        {
            // Boxes must have the same height for jump
            if (a.TrueFloor != b.TrueFloor) return false;

            int zMin = a.Zmin;
            if (a.Zmin <= b.Zmin)
                zMin = b.Zmin;

            int zMax = a.Zmax;
            if (a.Zmax >= b.Zmax)
                zMax = b.Zmax;

            int xMin = a.Xmin;
            int xMax = b.Xmax;

            int currentZ = (zMin + zMax) >> 1;

            int floor;

            if (xMax == xMin - 1)
            {
                _decCurrentRoom = b.Room;

                if (!Dec_CanSectorBeReachedAndIsSolid(xMax - 1, currentZ)) return false;

                if (!Dec_CanSectorBeReachedAndIsSolid(xMax, currentZ)) return false;
                floor = Dec_GetBoxFloorHeight(xMax, currentZ);
                return floor <= b.TrueFloor - 2 && floor != 0x7fff;
            }

            if (xMax == xMin - 2)
            {
                _decCurrentRoom = b.Room;

                if (!Dec_CanSectorBeReachedAndIsSolid(xMax - 1, currentZ)) return false;
                if (!Dec_CanSectorBeReachedAndIsSolid(xMax, currentZ)) return false;
                floor = Dec_GetBoxFloorHeight(xMax, currentZ);
                if (floor > b.TrueFloor - 2 || floor == 0x7fff) return false;
                if (!Dec_CanSectorBeReachedAndIsSolid(xMax + 1, currentZ)) return false;
                floor = Dec_GetBoxFloorHeight(xMax + 1, currentZ);
                if (floor <= b.TrueFloor - 2 && floor != 0x7fff) return true;

                return false;
            }

            xMin = b.Xmin;
            xMax = a.Xmax;

            if (xMin != xMax + 1)
            {
                if (xMin != xMax + 2) return false;

                _decCurrentRoom = a.Room;

                if (!Dec_CanSectorBeReachedAndIsSolid(xMax - 1, currentZ)) return false;

                if (!Dec_CanSectorBeReachedAndIsSolid(xMax, currentZ)) return false;

                floor = Dec_GetBoxFloorHeight(xMax, currentZ);
                if (floor > b.TrueFloor - 2 || floor == 0x7fff) return false;

                if (!Dec_CanSectorBeReachedAndIsSolid(xMax + 1, currentZ)) return false;
                floor = Dec_GetBoxFloorHeight(xMax + 1, currentZ);
                return floor <= b.TrueFloor - 2 && floor != 0x7fff;
            }

            _decCurrentRoom = a.Room;

            if (!Dec_CanSectorBeReachedAndIsSolid(xMax - 1, currentZ) ||
                !Dec_CanSectorBeReachedAndIsSolid(xMax, currentZ)) return false;
            floor = Dec_GetBoxFloorHeight(xMax, currentZ);
            return floor <= b.TrueFloor - 2 && floor != 0x7fff;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private bool Dec_OverlapXmax(ref DecTrBoxAux a, ref DecTrBoxAux b)
        {
            int startZ = b.Zmin;
            if (a.Zmin > b.Zmin)
                startZ = a.Zmin;

            int endZ = a.Zmax;
            if (a.Zmax >= b.Zmax)
                endZ = b.Zmax;

            if (startZ >= endZ)
            {
                return true;
            }

            while (true)
            {
                _decCurrentRoom = a.Room;

                if (!Dec_CanSectorBeReachedAndIsSolid(a.Xmax - 1, startZ)) break;

                _decGraybox = false;

                if (b.TrueFloor != Dec_GetBoxFloorHeight(a.Xmax, startZ)) break;

                startZ++;

                if (startZ >= endZ) return true;
            }

            return false;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private bool Dec_OverlapXmin(ref DecTrBoxAux a, ref DecTrBoxAux b)
        {
            int startZ = b.Zmin;
            if (a.Zmin > b.Zmin)
                startZ = a.Zmin;

            int endZ = a.Zmax;
            if (a.Zmax >= b.Zmax)
                endZ = b.Zmax;

            if (startZ >= endZ)
            {
                return true;
            }
            else
            {
                while (true)
                {
                    _decCurrentRoom = a.Room;

                    if (!Dec_CanSectorBeReachedAndIsSolid(a.Xmin, startZ)) break;

                    _decGraybox = false;

                    if (b.TrueFloor != Dec_GetBoxFloorHeight(a.Xmin - 1, startZ)) break;

                    startZ++;

                    if (startZ >= endZ) return true;
                }
            }

            return false;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private bool Dec_OverlapZmax(ref DecTrBoxAux a, ref DecTrBoxAux b)
        {
            int startX = b.Xmin;
            if (a.Xmin > b.Xmin)
                startX = a.Xmin;

            int endX = a.Xmax;
            if (a.Xmax >= b.Xmax)
                endX = b.Xmax;

            if (startX >= endX)
            {
                return true;
            }

            while (true)
            {
                _decCurrentRoom = a.Room;

                if (!Dec_CanSectorBeReachedAndIsSolid(startX, a.Zmax - 1)) break;

                _decGraybox = false;

                if (b.TrueFloor != Dec_GetBoxFloorHeight(startX, a.Zmax)) break;

                startX++;

                if (startX >= endX) return true;
            }

            return false;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private bool Dec_OverlapZmin(ref DecTrBoxAux a, ref DecTrBoxAux b)
        {
            int startX = b.Xmin;
            if (a.Xmin > b.Xmin)
                startX = a.Xmin;

            int endX = a.Xmax;
            if (a.Xmax >= b.Xmax)
                endX = b.Xmax;

            if (startX >= endX)
            {
                return true;
            }

            while (true)
            {
                _decCurrentRoom = a.Room;

                if (!Dec_CanSectorBeReachedAndIsSolid(startX, a.Zmin)) break;

                _decGraybox = false;

                if (b.TrueFloor != Dec_GetBoxFloorHeight(startX, a.Zmin - 1)) break;

                startX++;

                if (startX >= endX) return true;
            }

            return false;
        }

        private bool Dec_BoxesOverlap(ref DecTrBoxAux a, ref DecTrBoxAux b)
        {
            _decJump = false;
            _decMonkey = false;

            var box1 = a;
            var box2 = b;

            if (b.TrueFloor > a.TrueFloor)
            {
                box1 = b;
                box2 = a;
            }

            if (box1.Xmax <= box2.Xmin || box1.Xmin >= box2.Xmax)
            {
                if (box1.Zmax > box2.Zmin && box1.Zmin < box2.Zmax && Dec_CheckIfCanJumpZ(ref box1, ref box2))
                {
                    _decJump = true;
                    return true;
                }

                if (box1.Xmax < box2.Xmin ||
                    box1.Xmin > box2.Xmax ||
                    box1.Zmax <= box2.Zmin ||
                    box1.Zmin >= box2.Zmax ||
                    box1.Xmax == box2.Xmin && !Dec_OverlapXmax(ref box1, ref box2) ||
                    box1.Xmin == box2.Xmax && !Dec_OverlapXmin(ref box1, ref box2))
                {
                    return false;
                }

                if (box1.Monkey && box2.Monkey) _decMonkey = true;
                return true;
            }

            if (box1.Zmax > box2.Zmin && box1.Zmin < box2.Zmax)
            {
                if (box1.TrueFloor != box2.TrueFloor) return false;

                if (box1.Monkey && box2.Monkey) _decMonkey = true;
                return true;
            }

            if (Dec_CheckIfCanJumpX(ref box2, ref box1))
            {
                _decJump = true;
                return true;
            }

            if (box1.Zmax < box2.Zmin ||
                box1.Zmin > box2.Zmax ||
                box1.Zmax == box2.Zmin && !Dec_OverlapZmax(ref box1, ref box2))
            {
                return false;
            }

            if (box1.Zmin != box2.Zmax)
            {
                if (box1.Monkey && box2.Monkey) _decMonkey = true;
                return true;
            }

            if (!Dec_OverlapZmin(ref box1, ref box2))
                return false;

            if (box1.Monkey && box2.Monkey) _decMonkey = true;
            return true;
        }
    }
}
