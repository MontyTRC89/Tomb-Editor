using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Reflection;
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
            var path = Path.GetDirectoryName(filename);
            var materialPath = path + "\\" + Path.GetFileNameWithoutExtension(filename) + ".mtl";

            IOMesh mesh = model.Meshes[0];
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

            // Optimize all data
            var pos = new List<Vector3>();
            var posIndices = new Dictionary<int, int>();

            for (int i = 0; i < mesh.Positions.Count; i++)
            {
                var position = ApplyAxesTransforms(mesh.Positions[i]);
                int found = -1;
                for (int j = 0; j < pos.Count; j++)
                    if (pos[j] == position)
                    {
                        found = j;
                        break;
                    }

                if (found == -1)
                {
                    found = pos.Count;
                    pos.Add(position);
                }

                posIndices.Add(i, found);
            }

            using (var writer = new StreamWriter(filename, false))
            {
                writer.WriteLine("# Exported by Tomb Editor " + Assembly.GetExecutingAssembly().GetName().Version.ToString() + "\n\n");
                writer.WriteLine("mtllib " + Path.GetFileNameWithoutExtension(filename) + ".mtl" + "\n\n");

                // Mark object. On multiroom / multimesh export, this will result in new unique name.
                writer.WriteLine("o " + mesh.Name + "\n");

                // Save vertices
                foreach (var position in pos)
                {
                    writer.WriteLine("v " + position.X.ToString(CultureInfo.InvariantCulture) + " " +
                                            position.Y.ToString(CultureInfo.InvariantCulture) + " " +
                                            position.Z.ToString(CultureInfo.InvariantCulture));
                }

                writer.WriteLine("# " + pos.Count + " vertices total.\n");

                // Save normals
                mesh.CalculateNormals();
                foreach (var normal in mesh.Normals)
                {
                    writer.WriteLine("vn " + normal.X.ToString(CultureInfo.InvariantCulture) + " " +
                                             normal.Y.ToString(CultureInfo.InvariantCulture) + " " +
                                             normal.Z.ToString(CultureInfo.InvariantCulture));
                }

                writer.WriteLine("# " + mesh.Normals.Count + " normals total.\n"); 
                

                // Save UVs
                foreach (var uv in mesh.UV)
                {
                    var newUV = ApplyUVTransform(uv);
                    writer.WriteLine("vt " + uv.X.ToString(CultureInfo.InvariantCulture) + " " +
                                             (1.0f - uv.Y).ToString(CultureInfo.InvariantCulture) + " 0.000");
                }

                writer.WriteLine("# " + mesh.UV.Count + " UVs total.\n");

                // Save faces

                IOMaterial lastMaterial = null;
                int faceCount = 0;

                foreach (var submesh in mesh.Submeshes)
                {
                    if (submesh.Value.Polygons.Count == 0)
                        continue;

                    if (lastMaterial == null || lastMaterial != submesh.Key)
                    {
                        lastMaterial = submesh.Key;
                        writer.WriteLine("\n\n" + "usemtl " + lastMaterial.Name.ToString());
                        writer.WriteLine("s off");
                    }
                        
                    foreach (var poly in submesh.Value.Polygons)
                    {
                        var indices = new List<int>();
                        indices.Add(poly.Indices[0]);
                        indices.Add(poly.Indices[1]);
                        indices.Add(poly.Indices[2]);
                        if (poly.Shape == IOPolygonShape.Quad)
                            indices.Add(poly.Indices[3]);

                        // Change vertex winding
                        if (_settings.InvertFaces)
                            indices.Reverse();

                        var v1 = indices[0];
                        var v2 = indices[1];
                        var v3 = indices[2];
                        var v4 = (poly.Shape == IOPolygonShape.Quad ? indices[3] : 0);

                        if (poly.Shape == IOPolygonShape.Triangle)
                        {
                            writer.WriteLine("f  " + (posIndices[v1] + 1) + "/" 
                                                   + (v1 + 1) + "/"
                                                   + (v1 + 1) + " " 
                                                   + (posIndices[v2] + 1) + "/" 
                                                   + (v2 + 1) + "/"
                                                   + (v2 + 1) + " " 
                                                   + (posIndices[v3] + 1) + "/" 
                                                   + (v3 + 1) + "/"
                                                   + (v3 + 1));
                        }
                        else
                        {
                            writer.WriteLine("f  " + (posIndices[v1] + 1) + "/" 
                                                   + (v1 + 1) + "/"
                                                   + (v1 + 1) + " " 
                                                   + (posIndices[v2] + 1) + "/"
                                                   + (v2 + 1) + "/"
                                                   + (v2 + 1) + " "
                                                   + (posIndices[v3] + 1) + "/" 
                                                   + (v3 + 1) + "/"
                                                   + (v3 + 1) + " " 
                                                   + (posIndices[v4] + 1) + "/" 
                                                   + (v4 + 1) + "/"
                                                   + (v4 + 1));
                        }
                        faceCount++;
                    }
                }

                writer.WriteLine("# " + faceCount + " faces total.\n");

            }

            using (var writer = new StreamWriter(materialPath, false))
            {
                writer.WriteLine("# Exported by Tomb Editor " + Assembly.GetExecutingAssembly().GetName().Version.ToString());

                foreach (var material in materials)
                {
                    writer.WriteLine("newmtl " + material.Name);
                    writer.WriteLine("    Ka 1.000000 1.000000 1.000000");
                    writer.WriteLine("    Kd 1.000000 1.000000 1.000000");
                    writer.WriteLine("    Ks 1.000000 1.000000 1.000000");
                    writer.WriteLine("    Ke 1.000000 1.000000 1.000000");
                    writer.WriteLine("    Ni 1.000000");
                    writer.WriteLine("    Ns 10.000000");
                    writer.WriteLine("    d 1.000000");
                    writer.WriteLine("    illum 2");
                    writer.WriteLine("    map_Ka " + material.TexturePath);
                    writer.WriteLine("    map_Kd " + material.TexturePath);
                    writer.WriteLine("\n");
                }
            }

            return true;
        }
    }
}
