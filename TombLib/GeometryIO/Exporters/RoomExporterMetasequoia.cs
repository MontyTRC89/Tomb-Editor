using SharpDX;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TombLib.GeometryIO.Exporters
{
    public class RoomExporterMetasequoia : BaseGeometryExporter
    {
        public RoomExporterMetasequoia(IOGeometrySettings settings)
            : base(settings)
        {

        }

        public override bool ExportToFile(IOModel model, string filename)
        {
            var path = Path.GetDirectoryName(filename);

            using (var writer = new StreamWriter(filename, false))
            {
                var mesh = model.Meshes[0];

                writer.WriteLine("Metasequoia Document");
                writer.WriteLine("Format Text Ver 1.0");

                // Write scene
                writer.WriteLine("Scene {");
                writer.WriteLine("amb 0.250 0.250 0.250");
                writer.WriteLine("}");

                // Write material
                writer.WriteLine("Material 1 {");
                writer.Write("\"Room\" col(1.000 1.000 1.000 1.000) dif(0.000) amb(1.000) emi(1.000) spc(0.000) power(5.00) ");
                writer.Write("tex(\"" + mesh.Texture.Name + "\") ");
                writer.WriteLine("shader(4) vcol(1) ");

                writer.WriteLine("}");

                // Write mesh
                writer.WriteLine("Object Room {");

                // Save vertices
                writer.WriteLine("vertex " + mesh.Positions.Count + " {");
                foreach (var vertex in mesh.Positions)
                {
                    var position = ApplyAxesTransforms(vertex);
                    writer.WriteLine(position.X.ToString(CultureInfo.InvariantCulture) + " " +
                                     position.Y.ToString(CultureInfo.InvariantCulture) + " " +
                                     position.Z.ToString(CultureInfo.InvariantCulture));
                }
                writer.WriteLine("}");

                // Save faces
                writer.WriteLine("face " + mesh.Polygons.Count + " { ");
                foreach (var poly in mesh.Polygons)
                {
                    var indices = poly.Indices;

                    var v1 = indices[0];
                    var v2 = indices[1];
                    var v3 = indices[2];
                    var v4 = (indices.Count > 3 ? indices[3] : 0);

                    var uv1 = GetUV(ApplyUVTransform(mesh.UV[v1], mesh.Texture.Width, mesh.Texture.Height));
                    var uv2 = GetUV(ApplyUVTransform(mesh.UV[v2], mesh.Texture.Width, mesh.Texture.Height));
                    var uv3 = GetUV(ApplyUVTransform(mesh.UV[v3], mesh.Texture.Width, mesh.Texture.Height));

                    var color1 = GetColor(ApplyColorTransform(mesh.Colors[v1]));
                    var color2 = GetColor(ApplyColorTransform(mesh.Colors[v2]));
                    var color3 = GetColor(ApplyColorTransform(mesh.Colors[v3]));

                    if (poly.Shape == IOPolygonShape.Triangle)
                    {
                        writer.Write("3 V(" + v1 + " " + v2 + " " + v3 + ") ");
                        writer.Write("M(0) ");
                        writer.Write("UV(" + uv1 + " " + uv2 + " " + uv3 + ") ");
                        writer.WriteLine("COL(" + color1 + " " + color2 + " " + color3 + ") ");

                    }
                    else
                    {
                        var uv4 = GetUV(ApplyUVTransform(mesh.UV[v4], mesh.Texture.Width, mesh.Texture.Height));
                        var color4 = GetColor(ApplyColorTransform(mesh.Colors[v4]));

                        writer.Write("4 V(" + v1 + " " + v2 + " " + v3 + " " + v4 + ") ");
                        writer.Write("M(0) ");
                        writer.Write("UV(" + uv1 + " " + uv2 + " " + uv3 + " " + uv4 + ") ");
                        writer.WriteLine("COL(" + color1 + " " + color2 + " " + color3 + " " + color4 + ") ");
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
            return uv.X.ToString(CultureInfo.InvariantCulture) + " " +
                   uv.Y.ToString(CultureInfo.InvariantCulture);
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
