using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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

        public LevelScript(bool isTitle)
        {
            IsTitle = isTitle;
        }
    }
}
