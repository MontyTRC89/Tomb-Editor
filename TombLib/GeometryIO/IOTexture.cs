using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TombLib.Utils;

namespace TombLib.GeometryIO
{
    public class IOTexture
    {
        public string Name { get; private set; }
        public int Width { get; private set; }
        public int Height { get; private set; }

        public IOTexture(string name, int width, int height)
        {
            this.Name = name;
            this.Width = width;
            this.Height = height;
        }
    }
}
