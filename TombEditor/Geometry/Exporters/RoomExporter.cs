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
        Obj,
        Metasequoia,
        Fbx,
        Ply
    }

    public abstract class BaseRoomExporter
    {
        protected Editor _editor;

        public BaseRoomExporter()
        {
            _editor = Editor.Instance;
        }

        public abstract bool ExportToFile(Room room, string filename);

        public static BaseRoomExporter GetExporter(RoomImportExportFormat format)
        {
            switch(format)
            {
                case RoomImportExportFormat.Obj: return new RoomExporterObj();
                case RoomImportExportFormat.Metasequoia: return new RoomExporterMetasequoia();
                case RoomImportExportFormat.Fbx: return new RoomExporterFbx();
                case RoomImportExportFormat.Ply: return new RoomExporterPly();
                default: return null;
            }
        }
    }
}
