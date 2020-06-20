using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace TombLib.LevelData.Compilers.TR5Main
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct tr5main_sprite_texture
    {
        public int Tile;
        public float X1;
        public float Y1;
        public float X2;
        public float Y2;
        public float X3;
        public float Y3;
        public float X4;
        public float Y4;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct tr5main_room_sector
    {
        public int FloorDataIndex;
        public int BoxIndex;
        public int StepSound;
        public int RoomBelow;
        public int Stopper;
        public int Floor;
        public int RoomAbove;
        public int Ceiling;
    }
}
