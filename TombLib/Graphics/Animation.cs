using System.Collections.Generic;

namespace TombLib.Graphics
{
    public class Animation
    {
        public string Name { get; set; }
        public short Framerate { get; set; }
        public float Speed { get; set; }
        public float Acceleration { get; set; }
        public List<KeyFrame> KeyFrames { get; set; } = new List<KeyFrame>();
    }
}
