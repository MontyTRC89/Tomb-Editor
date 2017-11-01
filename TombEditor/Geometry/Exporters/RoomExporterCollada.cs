using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TombEditor.Geometry.Exporters
{
    public class RoomExporterCollada : BaseRoomExporter
    {
        public RoomExporterCollada()
            : base()
        {

        }

        public override bool ExportToFile(Room room, string filename)
        {
            var path = Path.GetDirectoryName(filename);
            var material = path + "\\" + Path.GetFileNameWithoutExtension(filename) + ".mtl";

            if (File.Exists(filename)) File.Delete(filename);
            if (File.Exists(material)) File.Delete(material);

            var vertices = room.GetRoomVertices();

            var numFaces = 0;
            for (var z = 0; z < room.NumZSectors; z++)
                for (var x = 0; x < room.NumXSectors; x++)
                    for (var f = 0; f < 29; f++)
                        if (room.IsFaceDefined(x, z, (BlockFace)f) && !room.Blocks[x, z].GetFaceTexture((BlockFace)f).TextureIsInvisble &&
                            !room.Blocks[x, z].GetFaceTexture((BlockFace)f).TextureIsUnavailable)
                            numFaces++;

            using (var writer = new StreamWriter(File.OpenWrite(filename)))
            {
                // Write XML header
                writer.WriteLine("<?xml version=\"1.0\" encoding=\"utf - 8\"?>");
                writer.WriteLine("<COLLADA xmlns=\"http://www.collada.org/2005/11/COLLADASchema\" version=\"1.4.1\" xmlns: xsi=\"http://www.w3.org/2001/XMLSchema-instance\">");

                // Write asset node
                var now = DateTime.Now;
                writer.WriteLine("<asset>\n" +
                                 "<contributor>\n" +
                                 "<author>Tomb Editor</author>\n" +
                                 "<authoring_tool>Tomb Editor</authoring_tool>\n" +
                                 "</contributor>\n" +
                                 "<created>" + now.Year + "-" + now.Month + "-" + now.Day + "T" + now.Hour + ":" + now.Minute + ":" + now.Second + "</created>\n" +
                                 "<modified>" + now.Year + "-" + now.Month + "-" + now.Day + "T" + now.Hour + ":" + now.Minute + ":" + now.Second + "</modified>\n" +
                                 "<unit name=\"meter\" meter=\"1\" />\n" +
                                 "<up_axis> Y_UP </up_axis>\n" +
                                 "</asset>");

                // Write texture file
                writer.WriteLine("<library_images>\n" +
                                 "<image id=\"texture\" name=\"texture\">\n" +
                                 "<init_from>" + _editor.Level.Settings.MakeAbsolute(_editor.Level.Settings.Textures[0].Path) + "</init_from>\n" +
                                 "</image>\n" +
                                 "</library_images>");



                writer.WriteLine("</COLLADA>");
            }

            return true;
        }
    }
}
