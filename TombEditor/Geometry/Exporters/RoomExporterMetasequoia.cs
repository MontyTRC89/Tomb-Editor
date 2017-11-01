using SharpDX;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TombEditor.Geometry.Exporters
{
    public class RoomExporterMetasequoia : BaseRoomExporter
    {
        public RoomExporterMetasequoia()
            : base()
        {

        }

        public override bool ExportToFile(Room room, string filename)
        {
            var path = Path.GetDirectoryName(filename);
            
            if (File.Exists(filename)) File.Delete(filename);

            using (var writer = new StreamWriter(File.OpenWrite(filename)))
            {
                writer.WriteLine("Metasequoia Document");
                writer.WriteLine("Format Text Ver 1.0");

                // Write scene
                writer.WriteLine("Scene {");
                writer.WriteLine("amb 0.250 0.250 0.250");
                writer.WriteLine("}");

                // Write material
                writer.WriteLine("Material 1 {");
                writer.Write("\"Room\" col(1.000 1.000 1.000 1.000) dif(0.000) amb(1.000) emi(1.000) spc(0.000) power(5.00) ");
                writer.Write("tex(\"" + _editor.Level.Settings.MakeAbsolute(_editor.Level.Settings.Textures[0].Path) + "\") ");
                writer.WriteLine("shader(4) vcol(1) ");

                writer.WriteLine("}");

                var scale = 1.0f;

                // Write mesh
                writer.WriteLine("Object Room {");

                // Save vertices
                var vertices = room.GetRoomVertices();
                writer.WriteLine("vertex " + vertices.Count + " {");
                foreach (var vertex in vertices)
                    writer.WriteLine((vertex.Position.X / scale).ToString(CultureInfo.InvariantCulture) + " " +
                                     (vertex.Position.Y / scale).ToString(CultureInfo.InvariantCulture) + " " +
                                     (-vertex.Position.Z / scale).ToString(CultureInfo.InvariantCulture));
                writer.WriteLine("}");

                // Save faces
                var numFaces = 0;
                for (var z = 0; z < room.NumZSectors; z++)
                {
                    for (var x = 0; x < room.NumXSectors; x++)
                    {
                        for (var f = 0; f < 29; f++)
                        {
                            if (room.IsFaceDefined(x, z, (BlockFace)f) && !room.Blocks[x, z].GetFaceTexture((BlockFace)f).TextureIsInvisble &&
                                !room.Blocks[x, z].GetFaceTexture((BlockFace)f).TextureIsUnavailable)
                            {
                                var vertexRange = room.GetFaceVertexRange(x, z, (BlockFace)f);
                                numFaces++;
                            }
                        }
                    }
                }

                writer.WriteLine("face " + numFaces + " { ");
                for (var z = 0; z < room.NumZSectors; z++)
                {
                    for (var x = 0; x < room.NumXSectors; x++)
                    {
                        for (var f = 0; f < 29; f++)
                        {
                            if (room.IsFaceDefined(x, z, (BlockFace)f) && !room.Blocks[x, z].GetFaceTexture((BlockFace)f).TextureIsInvisble &&
                                !room.Blocks[x, z].GetFaceTexture((BlockFace)f).TextureIsUnavailable)
                            {
                                var indices = room.GetFaceIndices(x, z, (BlockFace)f);

                                var v1 = indices[0];
                                var v2 = indices[1];
                                var v3 = indices[2];
                                var v4 = (indices.Count > 3 ? indices[3] : 0);

                                var uv1 = GetUV(vertices[v1].UV);
                                var uv2 = GetUV(vertices[v2].UV); 
                                var uv3 = GetUV(vertices[v3].UV);

                                var color1 = GetColor(vertices[v1].FaceColor);
                                var color2 = GetColor(vertices[v2].FaceColor);
                                var color3 = GetColor(vertices[v3].FaceColor);

                                if (indices.Count == 3)
                                {
                                    writer.Write("3 V(" + v1 + " " + v2 + " " + v3 + ") ");
                                    writer.Write("M(0) ");
                                    writer.Write("UV(" + uv1 + " " + uv2 + " " + uv3 + ") ");
                                    writer.WriteLine("COL(" + color1 + " " + color2 + " " + color3 + ") ");

                                }
                                else
                                {
                                    var uv4 = GetUV(vertices[v4].UV);
                                    var color4 = GetColor(vertices[v4].FaceColor);

                                    writer.Write("4 V(" + v1 + " " + v2 + " " + v3 + " " + v4 + ") ");
                                    writer.Write("M(0) ");
                                    writer.Write("UV(" + uv1 + " " + uv2 + " " + uv3 + " " + uv4 + ") ");
                                    writer.WriteLine("COL(" + color1 + " " + color2 + " " + color3 + " " + color4 + ") ");
                                }
                            }
                        }
                    }
                }
                writer.WriteLine("}");
                writer.WriteLine("}");
                writer.WriteLine("Eof");
            }

            return true;
        }

        private string GetUV(Vector2 uv)
        {
            return (uv.X / _editor.Level.Settings.Textures[0].Image.Size.X).ToString(CultureInfo.InvariantCulture) + " " +
                   ((uv.Y / _editor.Level.Settings.Textures[0].Image.Size.Y)).ToString(CultureInfo.InvariantCulture);
        }

        private uint GetColor(Vector4 color)
        {
            var r = (byte)((int)(color.X * 128.0f) & 0xFF);
            var g = (byte)((int)(color.Y * 128.0f) & 0xFF);
            var b = (byte)((int)(color.Z * 128.0f) & 0xFF);

            return (uint)(0xFF000000 + (r) + (g << 8) + (b << 16));
        }
    }
}
