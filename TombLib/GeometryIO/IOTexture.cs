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
        public ImageC Image { get; private set; }

        public IOTexture(string name, ImageC image)
        {
            this.Name = name;
            this.Image = image;
        }

        public int Width { get { return Image.Width; } }
        public int Height { get { return Image.Height; } }
    }
}
