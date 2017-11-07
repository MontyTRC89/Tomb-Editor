using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TombLib.Utils;

namespace TombLib.GeometryIO.Exporters
{
    public class RoomExporterPly : BaseGeometryExporter
    {
        public RoomExporterPly(IOGeometrySettings settings, GetTextureDelegate getTexturePathCallback)
            : base(settings, getTexturePathCallback)
        {

        }

        public override bool ExportToFile(IOModel model, string filename)
        {
            var path = Path.GetDirectoryName(filename);
            var material = path + "\\" + Path.GetFileNameWithoutExtension(filename) + ".mtl";

            using (var writer = new StreamWriter(filename, false))
            {
                var mesh = model.Meshes[0];

                writer.WriteLine("ply");
                writer.WriteLine("format ascii 1.0");
                writer.WriteLine("comment Created by Tomb Editor");
                writer.WriteLine("comment TextureFile " + GetTexturePath(path, mesh.Texture));
                writer.WriteLine("element vertex " + mesh.Positions.Count);
                writer.WriteLine("property float x");
                writer.WriteLine("property float y");
                writer.WriteLine("property float z");
                writer.WriteLine("property float s");
                writer.WriteLine("property float t");
                writer.WriteLine("property uchar red");
                writer.WriteLine("property uchar green");
                writer.WriteLine("property uchar blue");
                writer.WriteLine("element face " + mesh.Polygons.Count);
                writer.WriteLine("property list uchar uint vertex_indices");
                writer.WriteLine("end_header");

                // Save vertices
                for (var i = 0; i < mesh.Positions.Count; i++)
                {
                    var position = ApplyAxesTransforms(mesh.Positions[i]);
                    writer.Write(position.X.ToString(CultureInfo.InvariantCulture) + " " +
                                 position.Y.ToString(CultureInfo.InvariantCulture) + " " +
                                 position.Z.ToString(CultureInfo.InvariantCulture) + " ");

                    var uv = ApplyUVTransform(mesh.UV[i], mesh.Texture.Image.Width, mesh.Texture.Image.Height);
                    writer.Write(uv.X.ToString(CultureInfo.InvariantCulture) + " " +
                                 uv.Y.ToString(CultureInfo.InvariantCulture) + " ");

                    var color = ApplyColorTransform(mesh.Colors[i]);
                    writer.WriteLine((int)(color.X * 128.0f) + " " +
                                     (int)(color.Y * 128.0f) + " " +
                                     (int)(color.Z * 128.0f));
                }

                // Save faces
                foreach (var poly in mesh.Polygons)
                {
                    var indices = new List<int>();
                    indices.Add(poly.Indices[0]);
                    indices.Add(poly.Indices[1]);
                    indices.Add(poly.Indices[2]);
                    if (poly.Shape == IOPolygonShape.Quad) indices.Add(poly.Indices[3]);

                    // Change vertex winding
                    if (_settings.InvertFaces) indices.Reverse();

                    var v1 = indices[0];
                    var v2 = indices[1];
                    var v3 = indices[2];
                    var v4 = (poly.Shape == IOPolygonShape.Quad ? indices[3] : 0);

                    if (poly.Shape == IOPolygonShape.Triangle)
                    {
                        writer.WriteLine("3 " + v1 + " " + v2 + " " + v3);
                    }
                    else
                    {
                        writer.WriteLine("4 " + v1 + " " + v2 + " " + v3 + " " + v4);
                    }
                }
            }

            return true;
        }
    }
}
