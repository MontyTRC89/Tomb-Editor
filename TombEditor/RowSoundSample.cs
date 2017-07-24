using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TombEditor
{
    public class RowSoundSample
    {
        public short ID { get; set; }
        public string Name { get; set; }
        public string File { get; set; }
        public bool Missing { get; set; }

        public RowSoundSample(short id, string name, string file)
        {
            this.ID = id;
            this.Name = name;
            this.File = file;
        }
    }
}
