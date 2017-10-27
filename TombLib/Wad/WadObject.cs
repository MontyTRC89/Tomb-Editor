using System;
using System.Collections.Generic;

namespace TombLib.Wad
{
    public class WadObject
    {
        //public string Name { get; set; }
        public uint ObjectID { get; set; }
        public Wad2 Wad { get; set; }

        public WadObject(Wad2 wad)
        {
            Wad = wad;
        }
    }
}
