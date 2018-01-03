using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using TombLib.LevelData;
using TombLib.Utils;

namespace TombLib.GeometryIO.Exporters
{
    public class RoomExporterMetasequoia : BaseGeometryExporter
    {
        public RoomExporterMetasequoia(IOGeometrySettings settings, GetTextureDelegate getTexturePathCallback)
            : base(settings, getTexturePathCallback)
        {

        }

        public override bool ExportToFile(IOModel model, string filename)
        {
            var path = Path.GetDirectoryName(filename);

            using (var writer = new StreamWriter(filename, false))
            {
                writer.WriteLine("Metasequoia Document");
                writer.WriteLine("Format Text Ver 1.0");

                // Write scene
                writer.WriteLine("Scene {");
                writer.WriteLine("\tamb 0.250 0.250 0.250");
                writer.WriteLine("}");

                // Write materials
                writer.WriteLine("Material " + model.Materials.Count + " {");
                foreach (var material in model.Materials)
                {
                    writer.Write("\t\"" + material.Name + "\" col(1.000 1.000 1.000 1.000) dif(0.000) amb(1.000) emi(1.000) spc(0.000) power(5.00) ");
                    writer.Write("tex(\"" + GetTexturePath(path, material.Texture) + "\") ");
                    writer.WriteLine("shader(4) vcol(1) ");
                }

                writer.WriteLine("}");

                // Write mesh
                foreach (var mesh in model.Meshes)
                {
                    writer.WriteLine("Object " + mesh.Name + " {");

                    var translation = ApplyAxesTransforms(mesh.Position);

                    writer.WriteLine("\tdepth 0");
                    writer.WriteLine("\tfolding 0");
                    /*writer.WriteLine("\tscale 1.000000 1.000000 1.000000");
                    writer.WriteLine("\trotation 0.000000 0.000000 0.000000");
                    writer.WriteLine("\ttranslation " + translation.X.ToString(CultureInfo.InvariantCulture) + " " +
                                                        translation.Y.ToString(CultureInfo.InvariantCulture) + " " +
                                                        translation.Z.ToString(CultureInfo.InvariantCulture));*/
                    writer.WriteLine("\tvisible 15");
                    writer.WriteLine("\tlocking 0");
                    writer.WriteLine("\tshading 1");
                    writer.WriteLine("\tfacet 59.5");

                    // Save vertices
                    writer.WriteLine("\tvertex " + mesh.Positions.Count + " {");
                    foreach (var vertex in mesh.Positions)
                    {
                        var position = ApplyAxesTransforms(vertex);
                        writer.WriteLine("\t\t" + position.X.ToString(CultureInfo.InvariantCulture) + " " +
                                                  position.Y.ToString(CultureInfo.InvariantCulture) + " " +
                                                  position.Z.ToString(CultureInfo.InvariantCulture));
                    }
                    writer.WriteLine("\t}");

                    // Save faces
                    writer.WriteLine("\tface " + mesh.NumPolygons + " { ");

                    foreach (var submesh in mesh.Submeshes)
                        foreach (var poly in submesh.Value.Polygons)
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

                            var texture = submesh.Value.Material.Texture;
                            var uv1 = GetUV(ApplyUVTransform(mesh.UV[v1], texture.Image.Width, texture.Image.Height));
                            var uv2 = GetUV(ApplyUVTransform(mesh.UV[v2], texture.Image.Width, texture.Image.Height));
                            var uv3 = GetUV(ApplyUVTransform(mesh.UV[v3], texture.Image.Width, texture.Image.Height));

                            var color1 = GetColor(ApplyColorTransform(mesh.Colors[v1]));
                            var color2 = GetColor(ApplyColorTransform(mesh.Colors[v2]));
                            var color3 = GetColor(ApplyColorTransform(mesh.Colors[v3]));

                            if (poly.Shape == IOPolygonShape.Triangle)
                            {
                                writer.Write("\t\t3 V(" + v1 + " " + v2 + " " + v3 + ") ");
                                writer.Write("M(" + model.Materials.IndexOf(submesh.Value.Material) + ") ");
                                writer.Write("UV(" + uv1 + " " + uv2 + " " + uv3 + ") ");
                                writer.WriteLine("COL(" + color1 + " " + color2 + " " + color3 + ") ");
                            }
                            else
                            {
                                var uv4 = GetUV(ApplyUVTransform(mesh.UV[v4], texture.Image.Width, texture.Image.Height));
                                var color4 = GetColor(ApplyColorTransform(mesh.Colors[v4]));

                                writer.Write("\t\t4 V(" + v1 + " " + v2 + " " + v3 + " " + v4 + ") ");
                                writer.Write("M(" + model.Materials.IndexOf(submesh.Value.Material) + ") ");
                                writer.Write("UV(" + uv1 + " " + uv2 + " " + uv3 + " " + uv4 + ") ");
                                writer.WriteLine("COL(" + color1 + " " + color2 + " " + color3 + " " + color4 + ") ");
                            }
                        }

                    writer.WriteLine("\t}");
                    writer.WriteLine("}");
                }

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
