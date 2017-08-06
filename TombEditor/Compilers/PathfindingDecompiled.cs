using System;
using System.Collections.Generic;
using System.Linq;
using SharpDX;
using TombEditor.Geometry;

namespace TombEditor.Compilers
{
    public sealed partial class LevelCompilerTr4
    {
        private bool dec_graybox = false;
        private bool dec_water = true;
        private bool dec_monkey = false;
        private int dec_roomIndex = -1;
        private short dec_q0 = -1;
        private short dec_q1 = -1;
        private short dec_q2 = -1;
        private short dec_q3 = -1;
        private tr_box_aux[] dec_boxes;
        private tr_overlap_aux[] dec_overlaps;
        private short dec_numBoxes = 0;
        private short dec_numOverlaps = 0;
        private bool dec_boxExtendsInAnotherRoom = false;

        private void dec_BuildBoxesAndOverlaps()
        {
            dec_roomIndex = 0;
            dec_boxes = new tr_box_aux[2040];

            int boxIndex = 0x7ff;

            for (int flipped = 0; flipped < 2; flipped++)
            {
                for (int i = 0; i < _level.Rooms.Length; i++)
                {
                    Room room = _level.Rooms[i];

                    // Room must be defined and also must be base room or the flipped version
                    if (room != null && (flipped == 0 && room.BaseRoom == null || flipped == 1 && room.BaseRoom != null))
                    {
                        for (int x = 0; x < room.NumXSectors; x++)
                        {
                            for (int z = 0; z < room.NumZSectors; z++)
                            {
                                tr_box_aux box = new tr_box_aux();

                                // First create the box...
                                if (x != 0 &&
                                    z != 0 &&
                                    x != room.NumXSectors - 1 &&
                                    z != room.NumZSectors - 1 &&
                                    Dec_CreateNewBox(ref box, x, z, i))
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
                                room._compiled.Sectors[room._compiled.NumZSectors * x + z].BoxIndex = (short)boxIndex;
                            }
                        }
                    }
                }
            }

            // Build the overlaps
            Dec_BuildOverlaps();

            // Now put the boxes in the final array
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
                     dec_boxes[currentBoxIndex].Xmax = (byte)(dec_boxes[currentBoxIndex].Xmax << 10) - 1;*/
                    dec_boxes[currentBoxIndex].TrueFloor *= -256;

                    currentBoxIndex--;
                }
                while (currentBoxIndex > 0);
            }
        }

        private bool Dec_BuildOverlaps()
        {
            throw new NotImplementedException();
        }

        // TODO: maybe better inline this
        private int Dec_AddBox(ref tr_box_aux box)
        {
            if (dec_numBoxes == 2040) return -1;

            int boxIndex = -1;

            for (int i = 0; i < dec_numBoxes; i++)
            {
                // TODO: need to work about flags comparing
                if (dec_boxes[i].Xmin != box.Xmin ||
                    dec_boxes[i].Xmax != box.Xmax ||
                    dec_boxes[i].Zmin != box.Zmin ||
                    dec_boxes[i].Zmax != box.Zmax ||
                    dec_boxes[i].TrueFloor != box.TrueFloor /*||
                    /*dec_boxes[i]. != box.Xmin || */)
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

        private bool Dec_CreateNewBox(ref tr_box_aux box, int x,int z, int roomIndex)
        {
            Room room = _level.Rooms[dec_roomIndex];
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
            
            dec_roomIndex = roomIndex;

            dec_graybox = false;
            dec_water = true;
            dec_monkey = false;

 
            

            short floor = Dec_GetBoxFloorHeight(currentX, currentZ);
            box.TrueFloor = floor;


            

            if (block.Type == BlockType.Wall || 
                block.Type == BlockType.BorderWall || 
                block.FloorOpacity == PortalOpacity.Opacity1 || 
                block.CeilingOpacity == PortalOpacity.Opacity1)
            {              

                currentX = (int)room.Position.X + x;
                currentZ = (int)room.Position.Z + z;

                if (Dec_CanSectorBeReachedAndIsSolid(currentX, currentZ))
                {                  

                    if (floor == 0x7fff) return false;
                }
            }
            else
            {

            }

            throw new NotImplementedException();
        }

        private bool Dec_CanSectorBeReachedAndIsSolid(int x, int z)
        {
            throw new NotImplementedException();
        }

        private short Dec_GetBoxFloorHeight(int x, int z)
        {
            int adjoiningRoom = dec_roomIndex;
            Room room = _level.Rooms[dec_roomIndex];

            int posXblocks = (int)room.Position.X;
            int posZblocks = (int)room.Position.Z;

            int xInRoom = x - posXblocks;
            int zInRoom = z - posZblocks;

            int currentX = 0;
            int currentZ = 0;

            Block block = room.Blocks[xInRoom, zInRoom];

            if (xInRoom < 0 ||
                xInRoom > room.NumXSectors - 1 ||
                zInRoom < 0 ||
                zInRoom > room.NumZSectors - 1)
            {
                return 0x7fff;
            }

            // If block is a wall or is a vertical toggle opacity 1
            // Note that is & 8 because wall and border wall are the only blocks with bit 4 (0x08) set
            if (((block.Type == BlockType.Wall ||
                block.Type == BlockType.BorderWall) && block.WallPortal == null) ||
                block.WallOpacity == PortalOpacity.Opacity1)
            {
                dec_q0 = -1;
                dec_q1 = -1;
                dec_q2 = -1;
                dec_q3 = -1;

                return 0x7fff;
            }

            // If it's not a wall portal or is vertical toggle opacity 1
            if (block.WallPortal == null || block.WallOpacity == PortalOpacity.Opacity1)
            {
                currentZ = z;
            }
            else
            {
                if (block.WallPortal == null) return 0x7fff;

                Portal portal = block.WallPortal;
                adjoiningRoom = _level.Rooms.ReferenceIndexOf(portal.AdjoiningRoom);

                dec_roomIndex = adjoiningRoom;
                dec_boxExtendsInAnotherRoom = true;

                currentZ = z;

                room = _level.Rooms[dec_roomIndex];

                posXblocks = (int)room.Position.X;
                posZblocks = (int)room.Position.Z;

                xInRoom = x - posXblocks;
                zInRoom = z - posZblocks;

                block = room.Blocks[xInRoom, zInRoom];
            }

            while (block.FloorPortal != null)
            {
                Portal portal = block.FloorPortal;

                adjoiningRoom = _level.Rooms.ReferenceIndexOf(portal.AdjoiningRoom);

                if (block.FloorOpacity == PortalOpacity.Opacity1)
                {
                    if (!(room.FlagWater ^ _level.Rooms[adjoiningRoom].FlagWater))
                    {
                        break;
                    }
                }

                dec_roomIndex = adjoiningRoom;
                room = _level.Rooms[dec_roomIndex];

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

            if (dec_water && room.FlagWater && (/*room.Ceiling*/ - meanFloorCornerHeight) <= 1 && block.CeilingPortal != null)
            {
                Portal portal = block.CeilingPortal;
                if (portal.AdjoiningRoom.FlagWater)
                {
                    dec_water = false;
                }
            }

            dec_roomIndex = adjoiningRoom;

            if (slope1 + slope2 + slope4 + slope3 >= 3 || slope1 + slope3 == 2 || slope2 + slope4 == 2)
            {
                if (dec_water && !room.FlagWater) return 0x7fff;
            }
            else
            {
                if (someFlag)
                {

                }
            }

            throw new NotImplementedException();
        }

      
    }
}