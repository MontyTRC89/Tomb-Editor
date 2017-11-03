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
                        {
                            numFaces++;
                            if (room.GetFaceIndices(x, z, (BlockFace)f).Count > 3) numFaces++;
                        }

            var scale = 1024.0f;

            using (var writer = new StreamWriter(File.OpenWrite(filename)))
            {
                var textureName = Path.GetFileName(_editor.Level.Settings.MakeAbsolute(_editor.Level.Settings.Textures[0].Path)).Replace(".", "_");

                // Write XML header
                writer.WriteLine("<?xml version=\"1.0\" encoding=\"utf-8\"?>");
                writer.WriteLine("<COLLADA xmlns=\"http://www.collada.org/2005/11/COLLADASchema\" version=\"1.4.1\" xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\">");

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
                                 "<up_axis>Y_UP</up_axis>\n" +
                                 "</asset>");

                // Write texture file
                writer.WriteLine("<library_images>\n" +
                                 "<image id=\"" + textureName + "\" name=\"" + textureName + "\">\n" +
                                 "<init_from>" + _editor.Level.Settings.MakeAbsolute(_editor.Level.Settings.Textures[0].Path) + "</init_from>\n" +
                                 "</image>\n" +
                                 "</library_images>");

                // Write effect
                writer.WriteLine("<library_effects>\n" +
                                 "<effect id =\"" + textureName + "-effect\">\n" +
                                 "<profile_COMMON>\n" +
                                 "<newparam sid =\"" + textureName + "-surface\">\n" +
                                 "<surface type=\"2D\">\n" +
                                 "<init_from>" + textureName + "</init_from>\n" +
                                 "</surface>\n" +
                                 "</newparam>\n" +
                                 "<newparam sid=\"" + textureName + "-sampler\">\n" +
                                 "<sampler2D>\n" +
                                 "<source>" + textureName + "-surface</source>\n" +
                                 "</sampler2D>\n" +
                                 "</newparam>\n" +
                                 "<technique sid=\"common\">\n" +
                                 "<phong>\n" +
                                 "<emission>\n" +
                                 "<color sid=\"emission\">0 0 0 1</color>\n" +
                                 "</emission>\n" +
                                 "<ambient>\n" +
                                 "<color sid=\"ambient\">0 0 0 1</color>\n" +
                                 "</ambient>\n" +
                                 "<diffuse>\n" +
                                 "<texture texture=\"" + textureName + "-sampler\"/>\n" +
                                 "</diffuse>\n" +
                                 "<specular >\n" +
                                 "<color sid=\"specular\">0.5 0.5 0.5 1</color>\n" +
                                 "</specular>\n" +
                                 "<shininess>\n" +
                                 "<float sid=\"shininess\">50</float>\n" +
                                 "</shininess>\n" +
                                 "<index_of_refraction>\n" +
                                 "<float sid=\"index_of_refraction\">1</float>\n" +
                                 "</index_of_refraction>\n" +
                                 "</phong>\n" +
                                 "</technique>\n" +
                                 "</profile_COMMON>\n" +
                                 "</effect>\n" +
                                 "</library_effects>");

                // Write material
                writer.WriteLine("<library_materials>\n" +
                                 "<material id=\"" + textureName + "-material\" name=\"" + textureName + "\">\n" +
                                 "<instance_effect url=\"#" + textureName + "-effect\" />\n" +
                                 "</material>\n" +
                                 "</library_materials>\n");

                // Write geometry
                writer.WriteLine("<library_geometries>\n" +
                                 "<geometry id=\"Room-mesh\" name=\"Room\">\n" +
                                 "<mesh>");
                // Positions
                writer.WriteLine("<source id=\"Room-mesh-positions\">");
                writer.Write("<float_array id=\"Room-mesh-positions-array\" count=\"" + vertices.Count * 3 + "\">");
                foreach (var vertex in vertices)
                    writer.Write((vertex.Position.X / scale).ToString(CultureInfo.InvariantCulture) + " " +
                                 (vertex.Position.Y / scale).ToString(CultureInfo.InvariantCulture) + " " +
                                 (-vertex.Position.Z / scale).ToString(CultureInfo.InvariantCulture) + " ");
                writer.WriteLine("</float_array>");
                writer.WriteLine("<technique_common>\n" +
                                 "<accessor source=\"#Room-mesh-positions-array\" count=\"" + vertices.Count + "\" stride=\"3\">\n" +
                                 "<param name=\"X\" type=\"float\" />\n" +
                                 "<param name=\"Y\" type=\"float\" />\n" +
                                 "<param name=\"Z\" type=\"float\" />\n" +
                                 "</accessor>\n" +
                                 "</technique_common>");
                writer.WriteLine("</source>");

                // UV
                writer.WriteLine("<source id=\"Room-mesh-map-0\">");
                writer.Write("<float_array id=\"Room-mesh-map-0-array\" count=\"" + vertices.Count * 2 + "\">");
                foreach (var vertex in vertices)
                    writer.Write(((vertex.UV.X / _editor.Level.Settings.Textures[0].Image.Size.X)).ToString(CultureInfo.InvariantCulture) + " " +
                                 (1.0f - (vertex.UV.Y / _editor.Level.Settings.Textures[0].Image.Size.Y)).ToString(CultureInfo.InvariantCulture) + " ");
                writer.WriteLine("</float_array>");
                writer.WriteLine("<technique_common>\n" +
                                 "<accessor source=\"#Room-mesh-map-0-array\" count=\"" + vertices.Count + "\" stride=\"2\">\n" +
                                 "<param name=\"S\" type=\"float\" />\n" +
                                 "<param name=\"T\" type=\"float\" />\n" +
                                 "</accessor>\n" +
                                 "</technique_common>");
                writer.WriteLine("</source>");

                // Color
                writer.WriteLine("<source id=\"Room-mesh-colors-Col\">");
                writer.Write("<float_array id=\"Room-mesh-colors-Col-array\" count=\"" + vertices.Count * 3 + "\">");
                foreach (var vertex in vertices)
                    writer.Write(vertex.FaceColor.X.ToString(CultureInfo.InvariantCulture) + " " + 
                                 vertex.FaceColor.Y.ToString(CultureInfo.InvariantCulture) + " " + 
                                 vertex.FaceColor.Z.ToString(CultureInfo.InvariantCulture) + " ");
                writer.WriteLine("</float_array>");
                writer.WriteLine("<technique_common>\n" +
                                 "<accessor source=\"#Room-mesh-colors-Col-array\" count=\"" + vertices.Count + "\" stride=\"3\">\n" +
                                 "<param name=\"R\" type=\"float\" />\n" +
                                 "<param name=\"G\" type=\"float\" />\n" +
                                 "<param name=\"B\" type=\"float\" />\n" +
                                 "</accessor>\n" +
                                 "</technique_common>");
                writer.WriteLine("</source>");

                writer.WriteLine("<vertices id=\"Room-mesh-vertices\">\n" +
                                 "<input semantic=\"POSITION\" source=\"#Room-mesh-positions\" />\n" +
                                 "</vertices>");

                writer.WriteLine("<triangles material=\"" + textureName + "-material\" count=\"" + numFaces + "\">\n" +
                                 "<input semantic=\"VERTEX\" source=\"#Room-mesh-vertices\" offset=\"0\" />\n" +
                                 "<input semantic=\"TEXCOORD\" source=\"#Room-mesh-map-0\" offset=\"1\" set=\"0\" />\n" +
                                 "<input semantic=\"COLOR\" source=\"#Room-mesh-colors-Col\" offset=\"2\" set=\"0\" />\n" + 
                                 "<p>");
                for (var z = 0; z < room.NumZSectors; z++)
                {
                    for (var x = 0; x < room.NumXSectors; x++)
                    {
                        for (var f = 0; f < 29; f++)
                        {
                            if (room.IsFaceDefined(x, z, (BlockFace)f) && !room.Blocks[x, z].GetFaceTexture((BlockFace)f).TextureIsInvisble &&
                                !room.Blocks[x, z].GetFaceTexture((BlockFace)f).TextureIsUnavailable)
                            {
                                /* var indices = room.GetFaceIndices(x, z, (BlockFace)f);
                                 var v1 = indices[0];
                                 var v2 = indices[1];
                                 var v3 = indices[2];

                                 if (indices.Count == 3)
                                 {
                                     writer.Write(v1 + " " + v1 + " " + v1 + " " + v2 + " " + v2 + " " + v2 + " " + v3 + " " + v3 + " " + v3 + " ");
                                 }
                                 else
                                 {
                                     var v4 = indices[3];
                                     var v5 = indices[4];
                                     var v6 = indices[5];
                                     writer.Write(v1 + " " + v1 + " " + v1 + " " + v2 + " " + v2 + " " + v2 + " " + v3 + " " + v3 + " " + v3 + " ");
                                     writer.Write(v4 + " " + v4 + " " + v4 + " " + v5 + " " + v5 + " " + v5 + " " + v6 + " " + v6 + " " + v6 + " ");
                                 }*/

                                var vertexRange = room.GetFaceVertexRange(x, z, (BlockFace)f);
                                var v1 = vertexRange.Start + 0;
                                var v2 = vertexRange.Start + 1;
                                var v3 = vertexRange.Start + 2;

                                if (vertexRange.Count == 3)
                                {
                                    writer.Write(v1 + " " + v1 + " " + v1 + " " + v2 + " " + v2 + " " + v2 + " " + v3 + " " + v3 + " " + v3 + " ");
                                }
                                else
                                {
                                    var v4 = vertexRange.Start + 3;
                                    var v5 = vertexRange.Start + 4;
                                    var v6 = vertexRange.Start + 5;
                                    writer.Write(v1 + " " + v1 + " " + v1 + " " + v2 + " " + v2 + " " + v2 + " " + v3 + " " + v3 + " " + v3 + " ");
                                    writer.Write(v4 + " " + v4 + " " + v4 + " " + v5 + " " + v5 + " " + v5 + " " + v6 + " " + v6 + " " + v6 + " ");
                                }
                            }
                        }
                    }
                }
                writer.WriteLine("</p>\n" + 
                                 "</triangles>\n" +       
                                 "</mesh>\n" +
                                 "</geometry>\n" +
                                 "</library_geometries>");

                // Write scene
                writer.WriteLine("<library_visual_scenes>\n" +
                                 "<visual_scene id=\"Scene\" name=\"Scene\">\n" +
                                 "<node id=\"Room\" name=\"Room\" type=\"NODE\">\n" +
                                 "<instance_geometry url=\"#Room-mesh\" name=\"Room\">\n" +
                                 "<bind_material>\n" +
                                 "<technique_common>\n" +
                                 "<instance_material symbol=\"" + textureName + "-material\" target=\"#" + textureName + "-material\" />\n" +
                                 "</technique_common>\n" +
                                 "</bind_material>\n" +
                                 "</instance_geometry>\n" +
                                 "</node>\n" +
                                 "</visual_scene>\n" +
                                 "</library_visual_scenes>");

                writer.WriteLine("<scene>\n" +
                                 "<instance_visual_scene url=\"#Scene\" />\n" +
                                 "</scene>");

                writer.WriteLine("</COLLADA>");
            }

            return true;
        }
    }
}
