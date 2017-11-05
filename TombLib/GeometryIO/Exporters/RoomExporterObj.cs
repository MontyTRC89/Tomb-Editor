using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TombLib.GeometryIO.Exporters
{
    public class RoomExporterObj : BaseGeometryExporter
    {
        public RoomExporterObj(IOGeometrySettings settings)
            : base(settings)
        {

        }

        public override bool ExportToFile(IOModel model, string filename)
        {
            var path = Path.GetDirectoryName(filename);
            var material = path + "\\" + Path.GetFileNameWithoutExtension(filename) + ".mtl";

            var mesh = model.Meshes[0];

            using (var writer = new StreamWriter(filename, false))
            {
                writer.WriteLine("# Exported by Tomb Editor");
                writer.WriteLine("mtllib " + Path.GetFileNameWithoutExtension(filename) + ".mtl");
                writer.WriteLine("o Room");

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
                    var newUV = ApplyUVTransform(uv, mesh.Texture.Width, mesh.Texture.Height);
                    writer.WriteLine("vt " + newUV.X.ToString(CultureInfo.InvariantCulture) + " " +
                                             newUV.Y.ToString(CultureInfo.InvariantCulture));
                }

                // Save faces
                writer.WriteLine("usemtl Room");
                writer.WriteLine("s off");
                foreach (var poly in mesh.Polygons)
                {
                    var v1 = poly.Indices[0] + 1;
                    var v2 = poly.Indices[1] + 1;
                    var v3 = poly.Indices[2] + 1;
                    var v4 = (poly.Shape == IOPolygonShape.Quad ? poly.Indices[3] + 1 : 0);

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

            using (var writer = new StreamWriter(material, false))
            {
                writer.WriteLine("# Exported by Tomb Editor");
                writer.WriteLine("newmtl Room");
                writer.WriteLine("Ka 1.000000 1.000000 1.000000");
                writer.WriteLine("Ni 1.000000");
                writer.WriteLine("d 1.000000");
                writer.WriteLine("illum 2");
                writer.WriteLine("map_Kd " + mesh.Texture.Name);
            }

            return true;
        }
    }
}
