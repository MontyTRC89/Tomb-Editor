using System;
using System.Collections.Generic;
using System.Linq;
using SharpDX;
using TombEditor.Geometry;
using System.Runtime.CompilerServices;
using System.Diagnostics;

namespace TombEditor.Compilers
{
    public enum Dec_BoxFlag
    {
        None = 0,
        Water = 1,
        Monkey = 2,
        Jump = 4,
        FlipMap = 8,
        IsolatedBox = 16,
        NotWalkableBox = 32
    }

    public struct dec_tr_box_aux
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
        public short ZoneID;
    }

    public sealed partial class LevelCompilerTr4
    {
        private bool dec_graybox = false;
        private bool dec_water = true;
        private bool dec_monkey = false;
        private bool dec_flipped = false;
        private bool dec_jump = false;
        private Room dec_currentRoom = null;
        private short dec_q0 = -1;
        private short dec_q1 = -1;
        private short dec_q2 = -1;
        private short dec_q3 = -1;
        private dec_tr_box_aux[] dec_boxes;
        private ushort[] dec_overlaps;
        private int dec_numBoxes = 0;
        private int dec_numOverlaps = 0;
        private bool dec_boxExtendsInAnotherRoom = false;
        private Room[] dec_rooms;
        private Room[] dec_baseRooms;
        private Room[] dec_alternateRooms;
        private short[,] dec_zones;

        private void Dec_BuildBoxesAndOverlaps()
        {
            dec_currentRoom = _level.Rooms[0];
            dec_boxes = new dec_tr_box_aux[2040];

            int boxIndex = 0x7ff;

            Stopwatch watch = new Stopwatch();
            watch.Start();

            // TODO: for now I replicate the flipping method of winroomedit
            // dec_rooms = new Room[_level.Rooms.Length];
            // dec_baseRooms = new Room[_level.Rooms.Length];
            // dec_alternateRooms = new Room[_level.Rooms.Length];

            /* for (int n = 0; n < _level.Rooms.Length; n++)
             {
                 dec_rooms[n] = null;
                 dec_baseRooms[n] = null;
                 dec_alternateRooms[n] = null;
             }

             for (int n = 0; n < _level.Rooms.Length; n++)
             {
                 Room room = _level.Rooms[n];
                 if (room != null && room.BaseRoom==null)
                 {
                     dec_rooms[n] = room;
                     dec_baseRooms[n] = room;
                 }
             }

             for (int n=0;n<_level.Rooms.Length;n++)
             {
                 Room room = _level.Rooms[n];
                 if (room != null && room.BaseRoom != null)
                 {
                     dec_alternateRooms[dec_rooms.ReferenceIndexOf(room.BaseRoom)] = room;
                 } 
             }
             */

            //dec_rooms = _level.Rooms;

            for (int flipped = 0; flipped < 2; flipped++)
            {
                for (int i = 0; i < _level.Rooms.Length; i++)
                {
                    Room room = _level.Rooms[i];

                    // Room must be defined and also must be base room or the flipped version
                    if (room != null && (flipped == 0 && room.BaseRoom == null || flipped == 1 && room.BaseRoom != null))
                    {
                        for (int z = 0; z < room.NumZSectors; z++)
                        {
                            for (int x = 0; x < room.NumXSectors; x++)
                            {
                                //   Console.WriteLine("Room: " + i + ", X: " + x + ", Z: " + z);
                                dec_tr_box_aux box = new dec_tr_box_aux();

                                // First create the box...
                                if (x != 0 &&
                                    z != 0 &&
                                    x != room.NumXSectors - 1 &&
                                    z != room.NumZSectors - 1 &&
                                    Dec_CreateNewBox(ref box, x, z, ref room))
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
                                room._compiled.Sectors[room._compiled.NumZSectors * x + z].BoxIndex = (short)(boxIndex << 4);
                            }
                        }
                    }
                }

                dec_flipped = true;
            }

            dec_flipped = false;

            watch.Stop();
            Console.WriteLine("Dec_BuildBoxesAndOverlaps() -> Build boxes: " + watch.ElapsedMilliseconds + " ms, Count = " + dec_numBoxes);

            watch.Restart();

            // Build the overlaps
            Dec_BuildOverlaps();

            watch.Stop();
            Console.WriteLine("Dec_BuildBoxesAndOverlaps() -> Build overlaps: " + watch.ElapsedMilliseconds + " ms, Count = " + dec_numOverlaps);

            watch.Restart();

            // Build the overlaps
            Dec_BuildZones();

            watch.Stop();
            Console.WriteLine("Dec_BuildBoxesAndOverlaps() -> Build zones: " + watch.ElapsedMilliseconds + " ms, Count = " + dec_numBoxes);

            /*// Now put the boxes in the final array
           if (dec_numBoxes > 0)
           {
               int currentBoxIndex = dec_numBoxes - 1;

               do
               {
                   if (dec_boxes[currentBoxIndex].OverlapIndex != 0x7ff)
                   {
                       if (dec_boxes[currentBoxIndex].IsolatedBox)
                           dec_boxes[currentBoxIndex].OverlapIndex = (short)(dec_boxes[currentBoxIndex].OverlapIndex | 0x8000);
                   }

                   /* dec_boxes[currentBoxIndex].Xmin <<= 10;
                    dec_boxes[currentBoxIndex].Zmin <<= 10;
                    dec_boxes[currentBoxIndex].Xmax = (byte)(dec_boxes[currentBoxIndex].Xmax << 10) - 1;
                   dec_boxes[currentBoxIndex].TrueFloor *= -256;

                   currentBoxIndex--;
               }
               while (currentBoxIndex > 0);
           }*/
        }

        /* private void Dec_FlipAllRooms()
         {
             for (int i=0;i<dec_rooms.Length;i++)
             {
                 if ()
             }
         }*/

        private bool Dec_BuildOverlaps()
        {
            int numBoxes = dec_numBoxes;
            int numOverlaps = 0;
            dec_numOverlaps = 0;
            dec_overlaps = new ushort[16384];

            int i = 0;
            int j = 0;

            if (dec_numBoxes > 0)
            {
                i = 0;

                do
                {
                    dec_tr_box_aux box1 = dec_boxes[i];
                    dec_boxes[i].OverlapIndex = 0x7ff;

                    if (!box1.Flipped)
                    {
                        if (dec_flipped)
                        {
                            dec_flipped = false;
                            numOverlaps = dec_numOverlaps;
                        }

                        numBoxes = dec_numBoxes;

                        if (dec_numBoxes > 0)
                        {
                            j = 0;
                            do
                            {
                                if (i != j)
                                {
                                    if (i % 50 == 0 && j % 50 == 0) Console.WriteLine("CHecking overlap " + i + " vs " + j);
                                    dec_tr_box_aux box2 = dec_boxes[j];

                                    if (!box2.Flipped)
                                    {
                                        bool overlap = Dec_BoxesOverlap(ref box1, ref box2);
                                        numOverlaps = dec_numOverlaps;

                                        if (overlap)
                                        {
                                            if (dec_numOverlaps == 16384) return false;
                                            if (dec_boxes[i].OverlapIndex == 0x7ff) dec_boxes[i].OverlapIndex = (short)dec_numOverlaps;

                                            dec_overlaps[dec_numOverlaps] = (ushort)j;
                                            // dec_numOverlaps = numOverlaps;

                                            if (dec_jump) dec_overlaps[dec_numOverlaps] |= 0x800;
                                            if (dec_monkey) dec_overlaps[dec_numOverlaps] |= 0x2000;

                                            dec_numOverlaps++;
                                        }
                                    }
                                }

                                numBoxes = dec_numBoxes;
                                j++;
                            }
                            while (j < dec_numBoxes);
                        }
                    }

                    if (box1.Flipped)
                    {
                        if (!dec_flipped)
                        {
                            dec_flipped = true;
                            numOverlaps = dec_numOverlaps;
                        }

                        numBoxes = dec_numBoxes;

                        if (dec_numBoxes > 0)
                        {
                            j = 0;
                            do
                            {
                                if (i != j)
                                {
                                    dec_tr_box_aux box2 = dec_boxes[j];

                                    if (!box2.Flipped)
                                    {
                                        bool overlap = Dec_BoxesOverlap(ref box1, ref box2);
                                        numOverlaps = dec_numOverlaps;

                                        if (overlap)
                                        {
                                            if (dec_numOverlaps == 16384) return false;
                                            if (dec_boxes[i].OverlapIndex == 0x7ff) dec_boxes[i].OverlapIndex = (short)dec_numOverlaps;

                                            dec_overlaps[dec_numOverlaps] = (ushort)j;
                                            // dec_numOverlaps = numOverlaps;

                                            if (dec_jump) dec_overlaps[dec_numOverlaps] |= 0x800;
                                            if (dec_monkey) dec_overlaps[dec_numOverlaps] |= 0x2000;

                                            dec_numOverlaps++;
                                        }
                                    }
                                }

                                numBoxes = dec_numBoxes;
                                j++;
                            }
                            while (j < dec_numBoxes);
                        }
                    }

                    i++;

                    dec_overlaps[dec_numOverlaps - 1] |= 0x8000;
                }
                while (i < dec_numBoxes);

            }

            return true;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private int Dec_AddBox(ref dec_tr_box_aux box)
        {
            if (dec_numBoxes == 2040) return -1;

            int boxIndex = -1;

            for (int i = 0; i < dec_numBoxes; i++)
            {
                if (dec_boxes[i].Xmin == box.Xmin &&
                    dec_boxes[i].Xmax == box.Xmax &&
                    dec_boxes[i].Zmin == box.Zmin &&
                    dec_boxes[i].Zmax == box.Zmax &&
                    dec_boxes[i].TrueFloor == box.TrueFloor &&/*
                    dec_boxes[i].IsBaseRoom == box.IsBaseRoom &&
                    dec_boxes[i].IsAlternateRoom == box.IsAlternateRoom &&
                    dec_boxes[i].IsolatedBox == box.IsolatedBox &&
                    dec_boxes[i].Jump == box.Jump &&
                    dec_boxes[i].Monkey == box.Monkey &&
                    dec_boxes[i].NotWalkableBox == box.NotWalkableBox &&*/
                    dec_boxes[i].Water == box.Water)
                {
                    boxIndex = i;
                    break;
                }
            }

            if (boxIndex == -1)
            {
                boxIndex = dec_numBoxes;
                box.OverlapIndex = 0x7ff;
                dec_boxes[dec_numBoxes] = box;
                dec_numBoxes++;
            }

            return boxIndex;
        }

        private bool Dec_CreateNewBox(ref dec_tr_box_aux box, int x, int z, ref Room theRoom)
        {
            bool monkey = false;

            Room room = theRoom;
            Block block = room.Blocks[x, z];

            if (block.Type == BlockType.Wall ||
                block.Type == BlockType.BorderWall ||
                block.FloorOpacity == PortalOpacity.Opacity1 ||
                block.CeilingOpacity == PortalOpacity.Opacity1)
            {
                return false;
            }

            dec_q0 = block.QAFaces[0];
            dec_q1 = block.QAFaces[1];
            dec_q2 = block.QAFaces[2];
            dec_q3 = block.QAFaces[3];

            int currentX = (int)room.Position.X + x;
            int currentZ = (int)room.Position.Z + z;

            dec_currentRoom = theRoom;

            Dec_CanSectorBeReachedAndIsSolid(currentX, currentZ);

            dec_graybox = false;
            dec_water = true;
            dec_monkey = false;

            short floor = (short)Dec_GetBoxFloorHeight(currentX, currentZ);
            box.TrueFloor = floor;

            if (floor == 0x7fff) return false;

            box.Room = dec_currentRoom;
            box.Water = room.FlagWater;

            if (dec_flipped)
            {
                box.Flipped = true;
            }
            else
            {
                box.Flipped = false;
            }

            if (dec_monkey)
            {
                box.Monkey = true;
                monkey = true;
            }

            if (!dec_water)
            {
                box.Water = false;
            }

            if (dec_graybox)
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
                dec_graybox = true;

                int direction = 0x0f;
                int directionBase = 0x0f;

                int xMin = currentX;
                int xMax = currentX;
                int zMin = currentZ;
                int zMax = currentZ;

                Room currentRoom = theRoom;
                Room currentRoom1 = theRoom;
                Room currentRoom2 = theRoom;
                Room currentRoom3 = theRoom;
                Room currentRoom4 = theRoom;

                int searchX = xMin;
                int searchZ = zMin;

                while (true)
                {
                    if ((directionBase & 0x04) == 0x04)
                    {
                        dec_boxExtendsInAnotherRoom = false;
                        dec_currentRoom = currentRoom1;
                        currentRoom = currentRoom1;

                        searchX = xMin;

                        if (xMin <= xMax)
                        {
                            bool finishedDirection = true;

                            while (floor == Dec_GetBoxFloorHeight(searchX, zMin) &&
                                   floor == Dec_GetBoxFloorHeight(searchX, zMin - 1) &&
                                   dec_monkey == monkey)
                            {
                                if (searchX == xMin) currentRoom1 = dec_currentRoom;

                                if (dec_boxExtendsInAnotherRoom)
                                {
                                    // If the box goes in another room and one of current rooms has a flipped room that stop now
                                    if (dec_currentRoom != currentRoom &&
                                        (dec_currentRoom.AlternateRoom != null ||
                                         currentRoom.AlternateRoom != null))
                                    {
                                        break;
                                    }

                                    // Reset current room index to start room index
                                    dec_currentRoom = currentRoom;

                                    // If floor of starting block is != floor of block (X, Z - 1) exit loop
                                    if (floor != Dec_GetBoxFloorHeight(searchX, zMin - 1)) break;

                                    // Reset flag of box extended in another room
                                    dec_boxExtendsInAnotherRoom = false;
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

                    if ((directionBase & 0x02) == 0x02)
                    {
                        dec_boxExtendsInAnotherRoom = false;
                        dec_currentRoom = currentRoom2;
                        currentRoom = currentRoom2;

                        searchZ = zMin;

                        if (zMin <= zMax)
                        {
                            bool finishedDirection = true;

                            while (floor == Dec_GetBoxFloorHeight(xMax, searchZ) &&
                                   floor == Dec_GetBoxFloorHeight(xMax + 1, searchZ) &&
                                   dec_monkey == monkey)
                            {
                                if (searchZ == zMin) currentRoom2 = dec_currentRoom;

                                if (dec_boxExtendsInAnotherRoom)
                                {
                                    // If the box goes in another room and one of current rooms has a flipped room that stop now
                                    if (dec_currentRoom != currentRoom &&
                                        (dec_currentRoom.AlternateRoom != null ||
                                         currentRoom.AlternateRoom != null))
                                    {
                                        break;
                                    }

                                    // Reset current room index to start room index
                                    dec_currentRoom = currentRoom;

                                    // If floor of starting block is != floor of block (X, Z - 1) exit loop
                                    if (floor != Dec_GetBoxFloorHeight(xMax + 1, searchZ)) break;

                                    // Reset flag of box extended in another room
                                    dec_boxExtendsInAnotherRoom = false;
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
                        dec_boxExtendsInAnotherRoom = false;
                        dec_currentRoom = currentRoom3;
                        currentRoom = currentRoom3;

                        searchX = xMax;

                        if (xMax >= xMin)
                        {
                            bool finishedDirection = true;

                            while (floor == Dec_GetBoxFloorHeight(searchX, zMax) &&
                                   floor == Dec_GetBoxFloorHeight(searchX, zMax + 1) &&
                                   dec_monkey == monkey)
                            {
                                if (searchX == xMax) currentRoom3 = dec_currentRoom;

                                if (dec_boxExtendsInAnotherRoom)
                                {
                                    // If the box goes in another room and one of current rooms has a flipped room that stop now
                                    if (dec_currentRoom != currentRoom &&
                                        (dec_currentRoom.AlternateRoom != null ||
                                         currentRoom.AlternateRoom != null))
                                    {
                                        break;
                                    }

                                    // Reset current room index to start room index
                                    dec_currentRoom = currentRoom;

                                    // If floor of starting block is != floor of block (X, Z + 1) exit loop
                                    if (floor != Dec_GetBoxFloorHeight(searchX, zMax + 1)) break;

                                    // Reset flag of box extended in another room
                                    dec_boxExtendsInAnotherRoom = false;
                                }

                                searchX--;

                                if (searchX < xMin)
                                {
                                    finishedDirection = false;
                                    break;
                                }
                            }

                            if (finishedDirection) direction -= 0x08;
                        }

                        directionBase = direction;
                        if ((directionBase & 0x08) == 0x08) zMax++;
                    }

                    if ((directionBase & 0x01) == 0x01)
                    {
                        dec_boxExtendsInAnotherRoom = false;
                        dec_currentRoom = currentRoom4;
                        currentRoom = currentRoom4;

                        searchZ = zMax;

                        if (zMax >= zMin)
                        {
                            bool finishedDirection = true;

                            while (floor == Dec_GetBoxFloorHeight(xMin, searchZ) &&
                                   floor == Dec_GetBoxFloorHeight(xMin - 1, searchZ) &&
                                   dec_monkey == monkey)
                            {
                                if (searchZ == zMax) currentRoom4 = dec_currentRoom;

                                if (dec_boxExtendsInAnotherRoom)
                                {
                                    // If the box goes in another room and one of current rooms has a flipped room that stop now
                                    if (dec_currentRoom != currentRoom &&
                                        (dec_currentRoom.AlternateRoom != null ||
                                         currentRoom.AlternateRoom != null))
                                    {
                                        break;
                                    }

                                    // Reset current room index to start room index
                                    dec_currentRoom = currentRoom;

                                    // If floor of starting block is != floor of block (X, Z - 1) exit loop
                                    if (floor != Dec_GetBoxFloorHeight(xMin - 1, searchZ)) break;

                                    // Reset flag of box extended in another room
                                    dec_boxExtendsInAnotherRoom = false;
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

                    currentX = xMin;
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

            Room theRoom = dec_currentRoom;

            int xInRoom = 0;
            int zInRoom = 0;

            Room room;
            Block block;

            if (borderOrOutside)
            {
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

                    bool isWallPortal = block.WallPortal != -1;
                    if (!isWallPortal) break;

                    Portal portal = _level.Portals[block.WallPortal];

                    Room adjoiningRoom = portal.AdjoiningRoom;
                    if (adjoiningRoom.AlternateRoom != null && dec_flipped) adjoiningRoom = adjoiningRoom.AlternateRoom;

                    dec_currentRoom = adjoiningRoom;
                    theRoom = adjoiningRoom;

                    if (block.WallOpacity == PortalOpacity.Opacity1) return false;

                    if (!Dec_IsOutsideOrdBorderRoom(x, z)) break;
                }

                room = dec_currentRoom;

                xInRoom = x - (int)room.Position.X;
                zInRoom = z - (int)room.Position.Z;

                block = room.Blocks[xInRoom, zInRoom];

                // After having probed that we can reach X, Z from the original room, do the following
                while (block.FloorPortal != -1 && !block.IsFloorSolid)
                {
                    Portal portal = _level.Portals[block.FloorPortal];

                    Room adjoiningRoom = portal.AdjoiningRoom;
                    if (adjoiningRoom.AlternateRoom != null && dec_flipped) adjoiningRoom = adjoiningRoom.AlternateRoom;

                    if (block.FloorOpacity == PortalOpacity.Opacity1 &&
                        !(room.FlagWater ^ adjoiningRoom.FlagWater))
                    {
                        break;
                    }

                    dec_currentRoom = adjoiningRoom;

                    room = dec_currentRoom;

                    xInRoom = x - (int)room.Position.X;
                    zInRoom = z - (int)room.Position.Z;

                    block = room.Blocks[xInRoom, zInRoom];
                }
            }

            return true;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private bool Dec_IsOutsideOrdBorderRoom(int x, int z)
        {
            Room room = dec_currentRoom;
            return (x < 0 || z < 0 || x > room.NumXSectors - 1 || z > room.NumZSectors - 1);
        }

        private int Dec_GetBoxFloorHeight(int x, int z)
        {
            Room adjoiningRoom = dec_currentRoom;
            Room room = dec_currentRoom;

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

            Block block = room.Blocks[xInRoom, zInRoom];

            // If block is a wall or is a vertical toggle opacity 1
            // Note that is & 8 because wall and border wall are the only blocks with bit 4 (0x08) set
            if (((block.Type == BlockType.Wall ||
                block.Type == BlockType.BorderWall) && block.WallPortal == -1) ||
                block.WallOpacity == PortalOpacity.Opacity1)
            {
                dec_q0 = -1;
                dec_q1 = -1;
                dec_q2 = -1;
                dec_q3 = -1;

                return 0x7fff;
            }

            // If it's not a wall portal or is vertical toggle opacity 1
            if ((block.WallPortal == -1 || block.WallOpacity == PortalOpacity.Opacity1))
            {

            }
            else
            {
                if (block.WallPortal == -1) return 0x7fff;

                Portal portal = _level.Portals[block.WallPortal];
                adjoiningRoom = portal.AdjoiningRoom;
                if (adjoiningRoom.AlternateRoom != null && dec_flipped) adjoiningRoom = adjoiningRoom.AlternateRoom;

                dec_currentRoom = adjoiningRoom;
                dec_boxExtendsInAnotherRoom = true;

                room = dec_currentRoom;

                posXblocks = (int)room.Position.X;
                posZblocks = (int)room.Position.Z;

                xInRoom = x - posXblocks;
                zInRoom = z - posZblocks;

                block = room.Blocks[xInRoom, zInRoom];
            }

            Room oldRoom = adjoiningRoom;

            while (block.FloorPortal != -1 && !block.IsFloorSolid)
            {
                Portal portal = _level.Portals[block.FloorPortal];

                Room adjoiningRoom2 = portal.AdjoiningRoom;
                if (adjoiningRoom2.AlternateRoom != null && dec_flipped) adjoiningRoom2 = adjoiningRoom2.AlternateRoom;

                if (block.FloorOpacity == PortalOpacity.Opacity1)
                {
                    if (!(room.FlagWater ^ adjoiningRoom2.FlagWater))
                    {
                        break;
                    }
                }

                dec_currentRoom = adjoiningRoom2;
                room = dec_currentRoom;

                posXblocks = (int)room.Position.X;
                posZblocks = (int)room.Position.Z;

                xInRoom = x - posXblocks;
                zInRoom = z - posZblocks;

                block = room.Blocks[xInRoom, zInRoom];
            }

            int sumHeights = block.QAFaces[0] + block.QAFaces[1] + block.QAFaces[2] + block.QAFaces[3];
            int meanFloorCornerHeight = sumHeights >> 2;

            dec_q0 = block.QAFaces[0];
            dec_q1 = block.QAFaces[1];
            dec_q2 = block.QAFaces[2];
            dec_q3 = block.QAFaces[3];

            int slope1 = (Math.Abs(dec_q0 - dec_q1) >= 3 ? 1 : 0);
            int slope2 = (Math.Abs(dec_q1 - dec_q2) >= 3 ? 1 : 0);
            int slope3 = (Math.Abs(dec_q2 - dec_q3) >= 3 ? 1 : 0);
            int slope4 = (Math.Abs(dec_q3 - dec_q0) >= 3 ? 1 : 0);

            bool someFlag = false;

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
                        block.QAFaces[0] < block.QAFaces[1] && block.QAFaces[0] < block.QAFaces[3] ||
                        block.QAFaces[2] < block.QAFaces[1] && block.QAFaces[2] < block.QAFaces[3])
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
            int ceiling = room.GetHighestCeilingCorner(xInRoom, zInRoom) + (int)room.Position.Y;

            if (dec_water && room.FlagWater && (ceiling - meanFloorCornerHeight) <= 1 && block.CeilingPortal != -1 && !block.IsCeilingSolid)
            {
                Portal portal = _level.Portals[block.CeilingPortal];
                Room adjoiningRoom3 = portal.AdjoiningRoom;
                if (adjoiningRoom3.AlternateRoom != null && dec_flipped) adjoiningRoom3 = adjoiningRoom3.AlternateRoom;

                if (!adjoiningRoom3.FlagWater)
                {
                    dec_water = false;
                }
            }

            dec_currentRoom = oldRoom;

            if (slope1 + slope2 + slope4 + slope3 >= 3 || slope1 + slope3 == 2 || slope2 + slope4 == 2)
            {
                if (dec_water && !room.FlagWater) return 0x7fff;
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
                        if (dec_water && !room.FlagWater) return 0x7fff;
                    }
                }
                else
                {
                    if (slope1 + slope4 == 2 || slope2 + slope3 == 2)
                    {
                        if (dec_water && !room.FlagWater) return 0x7fff;
                    }
                }
            }

            if ((block.Flags & BlockFlags.Box) == 0)
            {
                dec_monkey = (block.Flags & BlockFlags.Monkey) != 0;
                return floorHeight;
            }
            else
            {
                if (!dec_graybox)
                {
                    dec_graybox = true;
                    dec_monkey = (block.Flags & BlockFlags.Monkey) != 0;
                    return floorHeight;
                }
                else
                {
                    return 0x7fff;
                }
            }
        }

        private bool Dec_CheckIfCanJumpX(ref dec_tr_box_aux a, ref dec_tr_box_aux b)
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

            int floor = 0;

            if (zMax == zMin - 1)
            {
                dec_currentRoom = b.Room;

                if (!Dec_CanSectorBeReachedAndIsSolid(currentX, zMax - 1)) return false;

                if (Dec_CanSectorBeReachedAndIsSolid(currentX, zMax))
                {
                    floor = Dec_GetBoxFloorHeight(currentX, zMax);
                    if (floor <= b.TrueFloor - 2 && floor != 0x7fff) return true;

                    return false;
                }

                return false;
            }

            if (zMax == zMin - 2)
            {
                dec_currentRoom = b.Room;

                if (Dec_CanSectorBeReachedAndIsSolid(currentX, zMax - 1))
                {
                    if (Dec_CanSectorBeReachedAndIsSolid(currentX, zMax))
                    {
                        floor = Dec_GetBoxFloorHeight(currentX, zMax);
                        if (floor <= b.TrueFloor - 2 && floor != 0x7fff)
                        {
                            if (Dec_CanSectorBeReachedAndIsSolid(currentX, zMax + 1))
                            {
                                floor = Dec_GetBoxFloorHeight(currentX, zMax + 1);
                                if (floor <= b.TrueFloor - 2 && floor != 0x7fff) return true;
                            }
                        }
                    }
                }

                return false;
            }

            zMin = b.Zmin;
            zMax = a.Zmax;

            if (zMin != zMax + 1)
            {
                if (zMin != zMax + 2) return false;

                dec_currentRoom = a.Room;

                if (!Dec_CanSectorBeReachedAndIsSolid(currentX, zMax - 1)) return false;

                if (!Dec_CanSectorBeReachedAndIsSolid(currentX, zMax)) return false;

                floor = Dec_GetBoxFloorHeight(currentX, zMax);
                if (floor > b.TrueFloor || floor == 0x7fff) return false;

                if (Dec_CanSectorBeReachedAndIsSolid(currentX, zMax + 1))
                {
                    floor = Dec_GetBoxFloorHeight(currentX, zMax + 1);
                    if (floor <= b.TrueFloor - 2 && floor != 0x7fff) return true;

                    return false;
                }

                return false;
            }

            dec_currentRoom = a.Room;

            if (Dec_CanSectorBeReachedAndIsSolid(currentX, zMax - 1) && Dec_CanSectorBeReachedAndIsSolid(currentX, zMax))
            {
                floor = Dec_GetBoxFloorHeight(currentX, zMax);
                if (floor <= b.TrueFloor - 2 && floor != 0x7fff) return true;

                return false;
            }

            return false;
        }

        private bool Dec_CheckIfCanJumpZ(ref dec_tr_box_aux a, ref dec_tr_box_aux b)
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

            int floor = 0;

            if (xMax == xMin - 1)
            {
                dec_currentRoom = b.Room;

                if (!Dec_CanSectorBeReachedAndIsSolid(xMax - 1, currentZ)) return false;

                if (Dec_CanSectorBeReachedAndIsSolid(xMax, currentZ))
                {
                    floor = Dec_GetBoxFloorHeight(xMax, currentZ);
                    if (floor <= b.TrueFloor - 2 && floor != 0x7fff) return true;

                    return false;
                }

                return false;
            }

            if (xMax == xMin - 2)
            {
                dec_currentRoom = b.Room;

                if (Dec_CanSectorBeReachedAndIsSolid(xMax - 1, currentZ))
                {
                    if (Dec_CanSectorBeReachedAndIsSolid(xMax, currentZ))
                    {
                        floor = Dec_GetBoxFloorHeight(xMax, currentZ);
                        if (floor <= b.TrueFloor - 2 && floor != 0x7fff)
                        {
                            if (Dec_CanSectorBeReachedAndIsSolid(xMax + 1, currentZ))
                            {
                                floor = Dec_GetBoxFloorHeight(xMax + 1, currentZ);
                                if (floor <= b.TrueFloor - 2 && floor != 0x7fff) return true;
                            }
                        }
                    }
                }

                return false;
            }

            xMin = b.Xmin;
            xMax = a.Xmax;

            if (xMin != xMax + 1)
            {
                if (xMin != xMax + 2) return false;

                dec_currentRoom = a.Room;

                if (!Dec_CanSectorBeReachedAndIsSolid(xMax - 1, currentZ)) return false;

                if (!Dec_CanSectorBeReachedAndIsSolid(xMax, currentZ)) return false;

                floor = Dec_GetBoxFloorHeight(xMax, currentZ);
                if (floor > b.TrueFloor || floor == 0x7fff) return false;

                if (Dec_CanSectorBeReachedAndIsSolid(xMax + 1, currentZ))
                {
                    floor = Dec_GetBoxFloorHeight(xMax + 1, currentZ);
                    if (floor <= b.TrueFloor - 2 && floor != 0x7fff) return true;

                    return false;
                }

                return false;
            }

            dec_currentRoom = a.Room;

            if (Dec_CanSectorBeReachedAndIsSolid(xMax - 1, currentZ) && Dec_CanSectorBeReachedAndIsSolid(xMax, currentZ))
            {
                floor = Dec_GetBoxFloorHeight(xMax, currentZ);
                if (floor <= b.TrueFloor - 2 && floor != 0x7fff) return true;

                return false;
            }

            return false;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Dec_OverlapXmax(ref dec_tr_box_aux a, ref dec_tr_box_aux b)
        {
            int startZ = b.Zmin;
            if (a.Zmin > startZ)
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
                    dec_currentRoom = a.Room;

                    if (!Dec_CanSectorBeReachedAndIsSolid(a.Xmax - 1, startZ)) break;

                    dec_graybox = false;

                    if (b.TrueFloor != Dec_GetBoxFloorHeight(a.Xmax, startZ)) break;

                    startZ++;

                    if (startZ >= endZ) return true;
                }
            }

            return false;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Dec_OverlapXmin(ref dec_tr_box_aux a, ref dec_tr_box_aux b)
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
                    dec_currentRoom = a.Room;

                    if (!Dec_CanSectorBeReachedAndIsSolid(a.Xmin, startZ)) break;

                    dec_graybox = false;

                    if (b.TrueFloor != Dec_GetBoxFloorHeight(a.Xmin - 1, startZ)) break;

                    startZ++;

                    if (startZ >= endZ) return true;
                }
            }

            return false;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Dec_OverlapZmax(ref dec_tr_box_aux a, ref dec_tr_box_aux b)
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
            else
            {
                while (true)
                {
                    dec_currentRoom = a.Room;

                    if (!Dec_CanSectorBeReachedAndIsSolid(startX, a.Zmax - 1)) break;

                    dec_graybox = false;

                    if (b.TrueFloor != Dec_GetBoxFloorHeight(startX, a.Zmax)) break;

                    startX++;

                    if (startX >= endX) return true;
                }
            }

            return false;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Dec_OverlapZmin(ref dec_tr_box_aux a, ref dec_tr_box_aux b)
        {
            int startX = b.Xmin;
            if (a.Xmin > startX)
                startX = a.Xmin;

            int endX = a.Xmax;
            if (a.Xmax >= b.Xmax)
                endX = b.Xmax;

            if (startX >= endX)
            {
                return true;
            }
            else
            {
                while (true)
                {
                    dec_currentRoom = a.Room;

                    if (!Dec_CanSectorBeReachedAndIsSolid(startX, a.Zmin)) break;

                    dec_graybox = false;

                    if (b.TrueFloor != Dec_GetBoxFloorHeight(startX, a.Zmin - 1)) break;

                    startX++;

                    if (startX >= endX) return true;
                }
            }

            return false;
        }

        private bool Dec_BoxesOverlap(ref dec_tr_box_aux a, ref dec_tr_box_aux b)
        {
            dec_jump = false;
            dec_monkey = false;

            dec_tr_box_aux box1 = a;
            dec_tr_box_aux box2 = b;

            if (b.TrueFloor > a.TrueFloor)
            {
                box1 = b;
                box2 = a;
            }

            if (box1.Xmax <= box2.Xmin || box1.Xmin >= box2.Xmax)
            {
                if (box1.Zmax > box2.Zmin && box1.Zmin < box2.Zmax && Dec_CheckIfCanJumpZ(ref box1, ref box2))
                {
                    dec_jump = true;
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

                if (box1.Monkey && box2.Monkey) dec_monkey = true;
                return true;
            }

            if (box1.Zmax > box2.Zmin && box1.Zmin < box2.Zmax)
            {
                if (box1.TrueFloor != box2.TrueFloor) return false;

                if (box1.Monkey && box2.Monkey) dec_monkey = true;
                return true;
            }

            if (Dec_CheckIfCanJumpX(ref box2, ref box1))
            {
                dec_jump = true;
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
                if (box1.Monkey && box2.Monkey) dec_monkey = true;
                return true;
            }

            if (Dec_OverlapZmin(ref box1, ref box2))
            {
                if (box1.Monkey && box2.Monkey) dec_monkey = true;
                return true;
            }

            return false;
        }

        private void Dec_BuildZones()
        {
            return;

            dec_zones = new short[10, dec_numBoxes];

            for (int i = 0; i < 5; i++)
            {
                Dec_BuildZonesData(i, false);
            }

            for (int i = 0; i < 5; i++)
            {
                Dec_BuildZonesData(i, true);
            }
        }

        private void Dec_BuildZonesData(int zone, bool flipped)
        {
            // This array is used for making a non-recursive version of the algorithm
            short[] nextBoxes = new short[dec_numBoxes];

            // Basing on zone type, choose the right step difference
            int maxStep = 0;
            switch (zone)
            {
                case 0:
                case 1:
                case 2:
                    maxStep = 256;
                    break;
                case 3:
                    maxStep = 1024;
                    break;
                case 4:
                    maxStep = 20480;
                    break;
            }

            int numBoxes = dec_numBoxes;

            // Initialize arrays to 0x7ff (2047)
            for (int j = 0; j < dec_numBoxes; j++)
            {
                dec_zones[zone, j] = 0x7ff;
                nextBoxes[j] = 0x7ff;
            }

            int lastZoneID = 0;
            int currentBoxIndex = 0;
            int zoneID = 0;
            int startBoxIndex = 0;
            int currentZoneID = 0;
            int linkedBox;
            bool jump;
            bool monkey;
            int nextBoxIndex = 0;
            int oldBoxIndex = 0;
            bool isWater = false;
            int step = 0;
            dec_tr_box_aux box;
            dec_tr_box_aux startBox;

            bool thereAreBoxesToProcess = true;



            box = dec_boxes[currentBoxIndex];
            startBox = dec_boxes[currentBoxIndex];



            while (thereAreBoxesToProcess)
            {
                // Do loop while the current zone is != 0x7ff or room is not flipped or current overlapindex is == 0x7ff
                // I think that this loop tries to search a new box without zone from which starting
                while (dec_zones[zone, currentBoxIndex] != 0x7ff || box.Flipped != flipped || (box.OverlapIndex & 0x3fff) == 0x7ff)
                {
                    // Select next zone and box
                    currentBoxIndex++;
                    box = dec_boxes[currentBoxIndex];
                    startBox = dec_boxes[currentBoxIndex];
                    oldBoxIndex = currentBoxIndex;

                    // If I've reached the end of the boxes, then quit
                    if (currentBoxIndex >= numBoxes) return;
                }

                thereAreBoxesToProcess = true;

                zoneID = lastZoneID + 1;
                int v23 = currentBoxIndex;

                // Store the current zone ID
                dec_boxes[currentBoxIndex].ZoneID = (short)zoneID;

                startBoxIndex = currentBoxIndex;
                currentZoneID = zoneID;
                isWater = box.Water;

                bool endOfOverlapList = false;

                int currentOverlapIndex = (dec_boxes[startBoxIndex].OverlapIndex & 0x3fff);
                int currentOverlap = dec_overlaps[currentOverlapIndex];
                //int nextOverlap = 0;

                // Walk through overlaps
                while (!endOfOverlapList && currentOverlapIndex < dec_numOverlaps)
                {
                    //nextOverlap = currentOverlap + 1;

                    // Check if is the end of the overlaps list
                    if ((currentOverlap & 0x8000) != 0) endOfOverlapList = true;

                    // Get real box index plus some flags
                    linkedBox = currentOverlap & 0x7ff;
                    jump = ((currentOverlap & 0x800) != 0);
                    monkey = ((currentOverlap & 0x2000) != 0);

                    currentOverlapIndex++;
                    currentOverlap = dec_overlaps[currentOverlapIndex];

                    if (linkedBox > dec_numBoxes)
                    {
                        nextBoxIndex = nextBoxes[startBoxIndex];
                        nextBoxes[startBoxIndex] = 0x7ff;
                        startBoxIndex = nextBoxIndex;

                        // It's time to take a decision. 
                        // a) If next box is 0x7ff, then I have no other boxes in the tree to traverse, then I'm done
                        // b) If next box is not 0x7ff, then this becomes the new start box
                        if (nextBoxIndex == 0x7ff)
                        {
                            numBoxes = dec_numBoxes;
                            lastZoneID = currentZoneID;
                            box = startBox;
                            currentBoxIndex = oldBoxIndex;

                            // Select next zone and box
                            currentBoxIndex++;
                            box = dec_boxes[currentBoxIndex];
                            startBox = dec_boxes[currentBoxIndex];
                            oldBoxIndex = currentBoxIndex;

                            // If I've reached the end of the boxes, then quit
                            if (currentBoxIndex >= numBoxes) return;

                            //thereAreBoxesToProcess = false;
                            break;
                        }

                        endOfOverlapList = false;
                        //currentOverlap = (dec_boxes[startBoxIndex].OverlapIndex & 0x3fff);
                        //nextOverlap = 0;

                        continue;
                    }

                    // If the linked box has not a zone yet, then exit overlaps loop and assign a zone to it
                    if (dec_zones[zone, linkedBox] == 0x7ff)
                    {
                        // IF 
                        // a) current (flipped, water) status is != from start box (flipped, water)  
                        // b) zone type is not crocodile or fly
                        // c) is not monkey or zone type is not baddy
                        if (dec_boxes[linkedBox].Flipped != flipped || dec_boxes[linkedBox].Water != isWater &&
                            zone != 2 &&
                            zone != 4 &&
                            (!monkey || zone != 3))
                        {
                            continue;
                        }

                        // Calculate step height between boxes
                        step = dec_boxes[linkedBox].TrueFloor - dec_boxes[startBoxIndex].TrueFloor;

                        // If step is outside the max step range
                        if (step < -maxStep || step > maxStep)
                        {

                        }
                        else if (!jump ||
                                zone == 0 ||
                                zone == 3)// if not jump or zone is baddy or zone is skeleton
                        {
                            // Stores zone ID for linked box
                            dec_zones[zone, linkedBox] = (short)currentZoneID;

                            // If there isn't a next box, then store current box as next block
                            if (nextBoxes[linkedBox] == 0x7ff)
                            {
                                int nextZonesBoxIndex = v23;
                                v23 = linkedBox;
                                nextBoxes[nextZonesBoxIndex] = (short)linkedBox;
                            }

                            continue;
                        }

                        if ((!monkey || zone != 3) &&
                            (zone != 2 || !(dec_boxes[linkedBox].Water) || !(dec_boxes[startBoxIndex].Water)))
                        {
                            continue;
                        }

                        // Stores zone ID for linked box
                        dec_zones[zone, linkedBox] = (short)currentZoneID;

                        // If there isn't a next box, then store current box as next block
                        if (nextBoxes[linkedBox] == 0x7ff)
                        {
                            int nextZonesBoxIndex = v23;
                            v23 = linkedBox;
                            nextBoxes[nextZonesBoxIndex] = (short)linkedBox;
                        }

                        continue;
                    }

                    // If I've reached the end of the overlap list, then quit loop
                    if (endOfOverlapList)
                    {
                        nextBoxIndex = nextBoxes[startBoxIndex];
                        nextBoxes[startBoxIndex] = 0x7ff;
                        startBoxIndex = nextBoxIndex;

                        // It's time to take a decision. 
                        // a) If next box is 0x7ff, then I have no other boxes in the tree to traverse, then I'm done
                        // b) If next box is not 0x7ff, then this becomes the new start box
                        if (nextBoxIndex == 0x7ff)
                        {
                            numBoxes = dec_numBoxes;
                            lastZoneID = currentZoneID;
                            box = startBox;
                            currentBoxIndex = oldBoxIndex;

                            // Select next zone and box
                            currentBoxIndex++;
                            box = dec_boxes[currentBoxIndex];
                            startBox = dec_boxes[currentBoxIndex];
                            oldBoxIndex = currentBoxIndex;

                            // If I've reached the end of the boxes, then quit
                            if (currentBoxIndex >= numBoxes) return;

                            //thereAreBoxesToProcess = false;
                            break;
                        }

                        endOfOverlapList = false;
                        //currentOverlap = (dec_boxes[startBoxIndex].OverlapIndex & 0x3fff);
                        //nextOverlap = 0;

                        continue;
                    }
                }
            }





            //dec_tr_box_aux box = dec_boxes[currentBoxIndex];
            //dec_tr_box_aux startBox = dec_boxes[currentBoxIndex];

            // Do loop while the current zone is != 0x7ff or room is not flipped or current overlapindex is == 0x7ff
            // I think that this loop tries to search a new box without zone from which starting
            /* while (dec_zones[zone, currentBoxIndex] != 0x7ff || !box.Flipped || (box.OverlapIndex & 0x3fff) == 0x7ff)
             {
                 // Select next zone and box
                 currentBoxIndex++;
                 box = dec_boxes[currentBoxIndex];
                 startBox = dec_boxes[currentBoxIndex];
                 oldBoxIndex = currentBoxIndex;

                 // If I've reached the end of the boxes, then quit
                 if (currentBoxIndex >= numBoxes) return;
             }

             // Generate new zone ID
             zoneID = lastZoneID + 1;
             int v23 = currentBoxIndex;

             dec_boxes[currentBoxIndex].ZoneID = (short)zoneID;

             startBoxIndex = currentBoxIndex;
             currentZoneID = zoneID;
             isWaterOrFlipped = flipped || box.Water;
             isWater = box.Water;

             // BEFORE_OVERLAPS_LOOP
             bool endOfOverlapList = false;

             // Now the overlaps loop
             int currentOverlap = (dec_boxes[startBoxIndex].OverlapIndex & 0x3fff);
             int nextOverlap = 0;

             for (int i=currentOverlap;i<dec_numOverlaps;i++)
             {
                 nextOverlap = currentOverlap + 1;

                 // Check if is the end of the overlaps list
                 if ((currentOverlap & 0x8000) != 0) endOfOverlapList = true;

                 // Get real box index plus some flags
                 linkedBox = currentOverlap & 0x7ff;
                 jump = ((currentOverlap & 0x800) != 0);
                 monkey = ((currentOverlap & 0x2000) != 0);

                 // Box index not valid
                 if (linkedBox > dec_numBoxes)
                 {
                     nextBoxIndex = nextBoxes[startBoxIndex];
                     nextBoxes[startBoxIndex] = 0x7ff;
                     startBoxIndex = nextBoxIndex;

                     // It's time to take a decision. 
                     // a) If next box is 0x7ff, then I have no other boxes in the tree to traverse, then I'm done
                     // b) If next box is not 0x7ff, then this becomes the new start box
                     if (nextBoxIndex == 0x7ff)
                     {
                         numBoxes = dec_numBoxes;
                         lastZoneID = currentZoneID;
                         box = startBox;
                         currentBoxIndex = oldBoxIndex;

                         // goto ZONE_LOOP_START;
                     }

                     // goto BEFORE_OVERLAPS_LOOP;
                 }

                 // If the linked box has not a zone yet, then exit overlaps loop and assign a zone to it
                 if (dec_zones[zone, linkedBox] == 0x7ff) break;

                 // If I've reached the end of the overlap list, then quit loop
                 if (endOfOverlapList)
                 {
                     nextBoxIndex = nextBoxes[startBoxIndex];
                     nextBoxes[startBoxIndex] = 0x7ff;
                     startBoxIndex = nextBoxIndex;

                     // It's time to take a decision. 
                     // a) If next box is 0x7ff, then I have no other boxes in the tree to traverse, then I'm done
                     // b) If next box is not 0x7ff, then this becomes the new start box
                     if (nextBoxIndex == 0x7ff)
                     {
                         numBoxes = dec_numBoxes;
                         lastZoneID = currentZoneID;
                         box = startBox;
                         currentBoxIndex = oldBoxIndex;

                         // goto ZONE_LOOP_START;
                     }

                     // goto BEFORE_OVERLAPS_LOOP;
                 }
             }

             // IF 
             // a) current (flipped, water) status is != from start box (flipped, water)  
             // b) zone type is not crocodile or fly
             // c) is not monkey or zone type is not baddy
             if (dec_boxes[linkedBox].Flipped != flipped || dec_boxes[linkedBox].Water != isWater && 
                 zone != 2 &&
                 zone != 4 &&
                 (!monkey || zone != 3))
             {
                 //goto BEFORE_JUMP_END_OVERLAPS_LOOP;
             }

             // Calculate step height between boxes
             step = dec_boxes[linkedBox].TrueFloor - dec_boxes[startBoxIndex].TrueFloor;

             int tempZoneType = 0;

             // If step is outside the max step range
             if (step < -maxStep || step > maxStep)
             {
                 tempZoneType = zone;
             }
             else if (!jump || 
                     (tempZoneType = zone) == 0 || 
                     zone == 3)// if not jump or zone is baddy or zone is skeleton
             {
                 //STORE_ZONE_ID_AND_SAVE_NEXT_BOX:

                 // Stores zone ID for linked box
                 dec_zones[zone, linkedBox] = (short)currentZoneID;

                 // If there isn't a next box, then store current box as next block
                 if (nextBoxes[linkedBox] == 0x7ff)
                 {
                     int nextZonesBoxIndex = v23;
                     v23 = linkedBox;
                     nextBoxes[nextZonesBoxIndex] = (short)linkedBox;
                 }
                // goto END_OF_OVERLAPS_LOOP;
             }

             if ((!monkey || zone != 3) && 
                 (zone != 2 || !(dec_boxes[linkedBox].Water) || !(dec_boxes[startBoxIndex].Water)))
             {
                // BEFORE_JUMP_END_OVERLAPS_LOOP:
                 //v5 = pNextZonesCopy;
                 //pZones = pMemory;
                 //goto END_OF_OVERLAPS_LOOP;
             }*/
            //  goto STORE_ZONE_ID_AND_SAVE_NEXT_BOX;
        }
    }
}