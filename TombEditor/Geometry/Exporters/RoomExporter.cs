using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TombEditor.Geometry.Exporters
{
    public enum RoomImportExportFormat
    {
        OBJ,
        Metasequoia
    }

    public abstract class RoomExporter
    {
        protected Editor _editor;

        public RoomExporter()
        {
            _editor = Editor.Instance;
        }

        public abstract bool ExportToFile(Room room, string filename);

        public static RoomExporter GetExporter(RoomImportExportFormat format)
        {
            switch(format)
            {
                case RoomImportExportFormat.OBJ: return new RoomExporterOBJ();
                case RoomImportExportFormat.Metasequoia: return new RoomExporterMetasequoia();
                default: return null;
            }
        }
    }
}
