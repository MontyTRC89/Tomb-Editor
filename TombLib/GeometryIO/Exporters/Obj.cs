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
    public class ObjExporter : BaseGeometryExporter
    {
        public ObjExporter(IOGeometrySettings settings, GetTextureDelegate getTexturePathCallback)
            : base(settings, getTexturePathCallback)
        {

        }

        public override bool ExportToFile(IOModel model, string filename)
        {
            /*var path = Path.GetDirectoryName(filename);
            var materialPath = path + "\\" + Path.GetFileNameWithoutExtension(filename) + ".mtl";

            var mesh = model.Meshes[0];
            var materials = new List<IOMaterial>();

            foreach (var pair in mesh.Submeshes)
            {
                var material = pair.Key;
                var submesh = pair.Value;

                // If no polygons, then no need to consider this material
                if (submesh.Polygons.Count == 0)
                    continue;

                material.Path = path + "\\" + Path.GetFileNameWithoutExtension(filename) + ".mtl";
                materials.Add(material);
            }

            using (var writer = new StreamWriter(filename, false))
            {
                writer.WriteLine("# Exported by Tomb Editor");
                writer.WriteLine("matlib " + Path.GetFileNameWithoutExtension(filename) + ".mtl");
                writer.WriteLine("o " + mesh.Name);

                // Save vertices
                foreach (var vertex in mesh.Positions)
                {
                    var position = ApplyAxesTransforms(vertex);
                    writer.WriteLine("v " + position.X.ToString(CultureInfo.InvariantCulture) + " " +
                                            position.Y.ToString(CultureInfo.InvariantCulture) + " " +
                                            position.Z.ToString(CultureInfo.InvariantCulture));
                }

                // Save UVs
                foreach (var uv in mesh.UV)
                {
                    var newUV = ApplyUVTransform(uv, mesh.Submeshes[0].Material.tex mesh.Texture.Image.Width, mesh.Texture.Image.Height);
                    writer.WriteLine("vt " + newUV.X.ToString(CultureInfo.InvariantCulture) + " " +
                                             newUV.Y.ToString(CultureInfo.InvariantCulture));
                }

                // Save faces
                writer.WriteLine("usemtl Room");
                writer.WriteLine("s off");
                foreach (var submesh in mesh.Submeshes)
                    foreach (var poly in submesh.Value.Polygons)
                    {
                        var indices = new List<int>();
                        indices.Add(poly.Indices[0] + 1);
                        indices.Add(poly.Indices[1] + 1);
                        indices.Add(poly.Indices[2] + 1);
                        if (poly.Shape == IOPolygonShape.Quad)
                            indices.Add(poly.Indices[3] + 1);

                        // Change vertex winding
                        if (_settings.InvertFaces)
                            indices.Reverse();

                        var v1 = indices[0];
                        var v2 = indices[1];
                        var v3 = indices[2];
                        var v4 = (poly.Shape == IOPolygonShape.Quad ? indices[3] : 0);

                        if (poly.Shape == IOPolygonShape.Triangle)
                        {
                            writer.WriteLine("f " + v1 + "/" + v1 + " " + v2 + "/" + v2 + " " + v3 + "/" + v3);
                        }
                        else
                        {
                            writer.WriteLine("f " + v1 + "/" + v1 + " " + v2 + "/" + v2 + " " + v3 + "/" + v3 + " " + v4 + "/" + v4);
                        }
                    }
            }

            using (var writer = new StreamWriter(materialPath, false))
            {
                writer.WriteLine("# Exported by Tomb Editor");

                foreach (var material in materials)
                {
                    writer.WriteLine("newmtl " + material.Name);
                    writer.WriteLine("Ka 1.000000 1.000000 1.000000");
                    writer.WriteLine("Ni 1.000000");
                    writer.WriteLine("d 1.000000");
                    writer.WriteLine("illum 2");
                    writer.WriteLine("map_Kd " + GetTexturePath(path, material.Texture));
                }
            }*/

            return true;
        }
    }
}
