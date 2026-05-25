using System.Collections.Generic;
using System.Numerics;

namespace TombLib.GeometryIO
{
    public class IOBone
    {
        public IOBone Parent { get; set; }
        public List<IOBone> Children { get; set; } 
        public Vector3 Translation { get; set; }

        public IOBone()
        {
            Children = new List<IOBone>();
        }
    }
}
