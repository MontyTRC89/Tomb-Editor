using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TombLib.GeometryIO.Importers
{
    public class PlyRoomImporter : BaseGeometryImporter
    {
        public PlyRoomImporter(IOGeometrySettings settings, GetTextureDelegate getTextureCallback) 
            : base(settings, getTextureCallback)
        {
        }

        public override IOModel ImportFromFile(string filename)
        {
            throw new NotImplementedException();
        }
    }
}
