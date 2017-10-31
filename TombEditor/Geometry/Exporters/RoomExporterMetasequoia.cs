using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TombEditor.Geometry.Exporters
{
    public class RoomExporterMetasequoia : RoomExporter
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

                // Write material
                writer.WriteLine("Material 1 {");
                writer.WriteLine("\"RoomMaterial\" shader(3) vcol(1) col(" +
                                    (room.AmbientLight.X / 2.0f).ToString(CultureInfo.InvariantCulture) + " " +
                                    (room.AmbientLight.Y / 2.0f).ToString(CultureInfo.InvariantCulture) + " " +
                                    (room.AmbientLight.Z / 2.0f).ToString(CultureInfo.InvariantCulture) + ") " +
                                 "amb(1) dif(1) tex(\"" + _editor.Level.Settings.MakeAbsolute(_editor.Level.Settings.Textures[0].Path) + "\")");
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
                                if (vertexRange.Count != 3) numFaces++;
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
                                var vertexRange = room.GetFaceVertexRange(x, z, (BlockFace)f);
                                var v1 = vertexRange.Start + 0;
                                var v2 = vertexRange.Start + 1;
                                var v3 = vertexRange.Start + 2;
                                var v4 = vertexRange.Start + 3;
                                var v5 = vertexRange.Start + 4;
                                var v6 = vertexRange.Start + 5;

                                var uv1 = (vertices[v1].UV.X / _editor.Level.Settings.Textures[0].Image.Size.X).ToString(CultureInfo.InvariantCulture) + " " +
                                          (1.0f - (vertices[v1].UV.Y / _editor.Level.Settings.Textures[0].Image.Size.Y)).ToString(CultureInfo.InvariantCulture);
                                var uv2 = (vertices[v2].UV.X / _editor.Level.Settings.Textures[0].Image.Size.X).ToString(CultureInfo.InvariantCulture) + " " +
                                          (1.0f - (vertices[v2].UV.Y / _editor.Level.Settings.Textures[0].Image.Size.Y)).ToString(CultureInfo.InvariantCulture);
                                var uv3 = (vertices[v3].UV.X / _editor.Level.Settings.Textures[0].Image.Size.X).ToString(CultureInfo.InvariantCulture) + " " +
                                          (1.0f - (vertices[v3].UV.Y / _editor.Level.Settings.Textures[0].Image.Size.Y)).ToString(CultureInfo.InvariantCulture);

                                if (vertexRange.Count == 3)
                                {
                                    writer.Write("3 V(" + v1 + " " + v2 + " " + v3 + ") ");
                                    writer.Write("M(0) ");
                                    writer.WriteLine("UV(" + uv1 + " " + uv2 + " " + uv3 + ") ");
                                }
                                else
                                {
                                    writer.Write("3 V(" + v1 + " " + v2 + " " + v3 + ") ");
                                    writer.Write("M(0) ");
                                    writer.WriteLine("UV(" + uv1 + " " + uv2 + " " + uv3 + ") ");

                                    var uv4 = (vertices[v4].UV.X / _editor.Level.Settings.Textures[0].Image.Size.X).ToString(CultureInfo.InvariantCulture) + " " +
                                              (1.0f - (vertices[v4].UV.Y / _editor.Level.Settings.Textures[0].Image.Size.Y)).ToString(CultureInfo.InvariantCulture);
                                    var uv5 = (vertices[v5].UV.X / _editor.Level.Settings.Textures[0].Image.Size.X).ToString(CultureInfo.InvariantCulture) + " " +
                                              (1.0f - (vertices[v5].UV.Y / _editor.Level.Settings.Textures[0].Image.Size.Y)).ToString(CultureInfo.InvariantCulture);
                                    var uv6 = (vertices[v6].UV.X / _editor.Level.Settings.Textures[0].Image.Size.X).ToString(CultureInfo.InvariantCulture) + " " +
                                              (1.0f - (vertices[v6].UV.Y / _editor.Level.Settings.Textures[0].Image.Size.Y)).ToString(CultureInfo.InvariantCulture);

                                    writer.Write("3 V(" + v4 + " " + v5 + " " + v6 + ") ");
                                    writer.Write("M(0) ");
                                    writer.WriteLine("UV(" + uv4 + " " + uv5 + " " + uv6 + ") ");
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
    }
}
