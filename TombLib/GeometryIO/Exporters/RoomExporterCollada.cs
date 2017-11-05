using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TombLib.GeometryIO.Exporters
{
    public class RoomExporterCollada : BaseGeometryExporter
    {
        public RoomExporterCollada(IOGeometrySettings settings)
            : base(settings)
        {

        }

        public override bool ExportToFile(IOModel model, string filename)
        {
            var path = Path.GetDirectoryName(filename);
            if (File.Exists(filename)) File.Delete(filename);
                 
            using (var writer = new StreamWriter(File.OpenWrite(filename)))
            {
                var roomMesh = model.Meshes[0];
                var textureName = Path.GetFileName(roomMesh.Texture.Name).Replace(".", "_");

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
                                 "<init_from>" + roomMesh.Texture + "</init_from>\n" +
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
                writer.Write("<float_array id=\"Room-mesh-positions-array\" count=\"" + roomMesh.Positions.Count * 3 + "\">");
                foreach (var vertex in roomMesh.Positions)
                {
                    var position = ApplyAxesTransforms(vertex);
                    writer.Write(position.X.ToString(CultureInfo.InvariantCulture) + " " +
                                 position.Y.ToString(CultureInfo.InvariantCulture) + " " +
                                 position.Z.ToString(CultureInfo.InvariantCulture) + " ");
                }
                writer.WriteLine("</float_array>");
                writer.WriteLine("<technique_common>\n" +
                                 "<accessor source=\"#Room-mesh-positions-array\" count=\"" + roomMesh.Positions.Count + "\" stride=\"3\">\n" +
                                 "<param name=\"X\" type=\"float\" />\n" +
                                 "<param name=\"Y\" type=\"float\" />\n" +
                                 "<param name=\"Z\" type=\"float\" />\n" +
                                 "</accessor>\n" +
                                 "</technique_common>");
                writer.WriteLine("</source>");

                // UV
                writer.WriteLine("<source id=\"Room-mesh-map-0\">");
                writer.Write("<float_array id=\"Room-mesh-map-0-array\" count=\"" + roomMesh.Positions.Count * 2 + "\">");
                foreach (var uv in roomMesh.UV)
                {
                    var newUV = ApplyUVTransform(uv, roomMesh.Texture.Width, roomMesh.Texture.Height);
                    writer.Write(newUV.X.ToString(CultureInfo.InvariantCulture) + " " +
                                 newUV.Y.ToString(CultureInfo.InvariantCulture) + " ");
                }
                writer.WriteLine("</float_array>");
                writer.WriteLine("<technique_common>\n" +
                                 "<accessor source=\"#Room-mesh-map-0-array\" count=\"" + roomMesh.Positions.Count + "\" stride=\"2\">\n" +
                                 "<param name=\"S\" type=\"float\" />\n" +
                                 "<param name=\"T\" type=\"float\" />\n" +
                                 "</accessor>\n" +
                                 "</technique_common>");
                writer.WriteLine("</source>");

                // Color
                writer.WriteLine("<source id=\"Room-mesh-colors-Col\">");
                writer.Write("<float_array id=\"Room-mesh-colors-Col-array\" count=\"" + roomMesh.Positions.Count * 3 + "\">");
                foreach (var color in roomMesh.Colors)
                {
                    var newColor = ApplyColorTransform(color);
                    writer.Write(newColor.X.ToString(CultureInfo.InvariantCulture) + " " +
                                 newColor.Y.ToString(CultureInfo.InvariantCulture) + " " +
                                 newColor.Z.ToString(CultureInfo.InvariantCulture) + " ");
                }
                writer.WriteLine("</float_array>");
                writer.WriteLine("<technique_common>\n" +
                                 "<accessor source=\"#Room-mesh-colors-Col-array\" count=\"" + roomMesh.Positions.Count + "\" stride=\"3\">\n" +
                                 "<param name=\"R\" type=\"float\" />\n" +
                                 "<param name=\"G\" type=\"float\" />\n" +
                                 "<param name=\"B\" type=\"float\" />\n" +
                                 "</accessor>\n" +
                                 "</technique_common>");
                writer.WriteLine("</source>");

                writer.WriteLine("<vertices id=\"Room-mesh-vertices\">\n" +
                                 "<input semantic=\"POSITION\" source=\"#Room-mesh-positions\" />\n" +
                                 "</vertices>");

                /*writer.WriteLine("<triangles material=\"" + textureName + "-material\" count=\"" + (roomMesh.NumQuads * 2 + roomMesh.NumTriangles) + "\">\n" +
                                 "<input semantic=\"VERTEX\" source=\"#Room-mesh-vertices\" offset=\"0\" />\n" +
                                 "<input semantic=\"TEXCOORD\" source=\"#Room-mesh-map-0\" offset=\"1\" set=\"0\" />\n" +
                                 "<input semantic=\"COLOR\" source=\"#Room-mesh-colors-Col\" offset=\"2\" set=\"0\" />\n" +
                                 "<p>");*/
                writer.WriteLine("<polylist material=\"" + textureName + "-material\" count=\"" + roomMesh.Polygons.Count + "\">\n" +
                                 "<input semantic=\"VERTEX\" source=\"#Room-mesh-vertices\" offset=\"0\" />\n" +
                                 "<input semantic=\"TEXCOORD\" source=\"#Room-mesh-map-0\" offset=\"1\" set=\"0\" />\n" +
                                 "<input semantic=\"COLOR\" source=\"#Room-mesh-colors-Col\" offset=\"2\" set=\"0\" />\n" +
                                 "<vcount>");
                foreach (var tmpPoly in roomMesh.Polygons)
                {
                    if (tmpPoly.Shape == IOPolygonShape.Quad)
                        writer.Write("4 ");
                    else
                        writer.Write("3 ");
                }
                writer.WriteLine("\n</vcount>\n" +
                                 "<p>");
                foreach (var tmpPoly in roomMesh.Polygons)
                {
                    foreach (var index in tmpPoly.Indices)
                        writer.Write(index + " " + index + " " + index + " ");
                }
                writer.WriteLine("\n</p>\n" +
                                 "</polylist>\n" +
                                 "</mesh>\n" +
                                 "</geometry>\n" +
                                 "</library_geometries>");
                /*writer.WriteLine("</p>\n" + 
                                 "</triangles>\n" +       
                                 "</mesh>\n" +
                                 "</geometry>\n" +
                                 "</library_geometries>");*/

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
