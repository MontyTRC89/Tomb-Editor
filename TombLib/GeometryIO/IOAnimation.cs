using System.Collections.Generic;

namespace TombLib.GeometryIO
{
    public class IOAnimation
    {
        public string Name { get; private set; }
        public int NumNodes { get; private set; }
        public List<IOFrame> Frames { get; private set; }

        public IOAnimation(string name, int numNodes)
        {
            Name = name;
            NumNodes = numNodes;
            Frames = new List<IOFrame>();
        }
    }
}
