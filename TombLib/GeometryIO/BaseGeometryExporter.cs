using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TombLib.GeometryIO
{
    public abstract class BaseGeometryExporter
    {
        public abstract bool ExportToFile(IOMesh mesh, string filename);
    }
}
