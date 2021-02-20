using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace TombLib.Script
{
    public class LevelScriptEntry
    {
        public LevelScriptCommand Command { get; set; }
        public List<object> Parameters { get; private set; } = new List<object>();

        public LevelScriptEntry(LevelScriptCommand command)
        {
            Command = command;
        }
    }

    public class LevelScript
    {
        public string FileName { get; set; }
        public string Name { get; set; }
        public byte AudioTrack { get; set; }
        public bool IsTitle { get; set; }
        public List<LevelScriptEntry> Entries { get; private set; } = new List<LevelScriptEntry>();
        public bool Horizon { get; set; }
        public bool ResetInventory { get; set; }
        public bool ColAddHorizon { get; set; }
        public bool Lightning { get; set; }
        public byte UVRotate { get; set; }

        // TR5Main only
        public bool Sky { get; set; }
        public int LaraType { get; set; } = 0;
        public int LevelFarView { get; set; } = 200;
        public string LoadScreen { get; set; } = "";
        public bool UnlimitedAir { get; set; }
        public bool Rumble { get; set; }
        public Weather Weather { get; set; }
        public string Background { get; set; } = "";

        public LevelScript(bool isTitle)
        {
            IsTitle = isTitle;
        }
    }
}
