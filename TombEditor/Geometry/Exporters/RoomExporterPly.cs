using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TombEditor.Geometry.Exporters
{
    public class RoomExporterPly : BaseRoomExporter
    {
        public RoomExporterPly()
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

            using (var writer = new StreamWriter(File.OpenWrite(filename)))
            {
                writer.WriteLine("ply");
                writer.WriteLine("format ascii 1.0");
                writer.WriteLine("comment Created by Tomb Editor");
                writer.WriteLine("comment TextureFile " + _editor.Level.Settings.MakeAbsolute(_editor.Level.Settings.Textures[0].Path));
                writer.WriteLine("element vertex " + vertices.Count);
                writer.WriteLine("property float x");
                writer.WriteLine("property float y");
                writer.WriteLine("property float z");
                writer.WriteLine("property float s");
                writer.WriteLine("property float t");
                writer.WriteLine("property uchar red");
                writer.WriteLine("property uchar green");
                writer.WriteLine("property uchar blue");
                writer.WriteLine("element face " + numFaces);
                writer.WriteLine("property list uchar uint vertex_indices");
                writer.WriteLine("end_header");
                               
                var scale = 1024.0f;

                // Save vertices
                foreach (var vertex in vertices)
                {
                    writer.Write((vertex.Position.X / scale).ToString(CultureInfo.InvariantCulture) + " " +
                                 (vertex.Position.Z / scale).ToString(CultureInfo.InvariantCulture) + " " +
                                 (vertex.Position.Y / scale).ToString(CultureInfo.InvariantCulture) + " ");
                    writer.Write(((vertex.UV.X / _editor.Level.Settings.Textures[0].Image.Size.X)).ToString(CultureInfo.InvariantCulture) + " " +
                                 (1.0f - (vertex.UV.Y / _editor.Level.Settings.Textures[0].Image.Size.Y)).ToString(CultureInfo.InvariantCulture) + " ");
                    writer.WriteLine((int)(vertex.FaceColor.X * 128.0f) + " " + (int)(vertex.FaceColor.Y * 128.0f) + " " +
                                     (int)(vertex.FaceColor.Z * 128.0f));
                }

                // Save faces
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

                                if (indices.Count == 3)
                                {
                                    writer.WriteLine("3 " + v1 + " " + v2 + " " + v3);
                                }
                                else
                                {
                                    writer.WriteLine("4 " + v1 + " " + v2 + " " + v3 + " " + v4);
                                }
                            }
                        }
                    }
                }
            }

           /* using (var writer = new StreamWriter(File.OpenWrite(material)))
            {
                writer.WriteLine("# Exported by Tomb Editor");
                writer.WriteLine("newmtl Room");
                writer.WriteLine("Ka " + (room.AmbientLight.X / 2.0f).ToString(CultureInfo.InvariantCulture) + " " +
                                         (room.AmbientLight.Y / 2.0f).ToString(CultureInfo.InvariantCulture) + " " +
                                         (room.AmbientLight.Z / 2.0f).ToString(CultureInfo.InvariantCulture));
                writer.WriteLine("Ni 1.000000");
                writer.WriteLine("d 1.000000");
                writer.WriteLine("illum 2");
                writer.WriteLine("map_Kd " + _editor.Level.Settings.MakeAbsolute(_editor.Level.Settings.Textures[0].Path));
            }*/

            return true;
        }
    }
}
