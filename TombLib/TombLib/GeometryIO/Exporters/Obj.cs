using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Reflection;

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

            using (var writer = new StreamWriter(filename, false))
            {
                writer.WriteLine("# Exported by Tomb Editor " + Assembly.GetExecutingAssembly().GetName().Version.ToString() + "\n");
                writer.WriteLine("mtllib " + Path.GetFileNameWithoutExtension(filename) + ".mtl" + "\n");

                // Optimize all data
                var poscol = new List<Vector3[]>();
                var poscolIndices = new Dictionary<int, int>();

                var normals = new List<Vector3>();
                var normalIndices = new Dictionary<int, int>();

                var uvs = new List<Vector2>();
                var uvIndices = new Dictionary<int, int>();

                // Pack positions and colours
                int meshCount = 0;
                foreach (var mesh in model.Meshes)
                {
                    if (mesh.Positions.Count == 0)
                        continue;

                    for (int i = 0; i < mesh.Positions.Count; i++)
                    {
                        var position = ApplyAxesTransforms(mesh.Positions[i]);
                        var colour = ApplyColorTransform(mesh.Colors.Count > i ? mesh.Colors[i] : new Vector4(2)).To3();

                        int found = -1;
                        for (int j = 0; j < poscol.Count; j++)
                            if (poscol[j][0] == position && 
                                poscol[j][1] == colour)
                            {
                                found = j;
                                break;
                            }

                        if (found == -1)
                        {
                            found = poscol.Count;
                            poscol.Add(new Vector3[2] { position, colour });
                        }

                        poscolIndices.Add(i + meshCount, found);
                    }
                    meshCount += mesh.Positions.Count;
                }

                // Pack normals
                meshCount = 0;
                foreach (var mesh in model.Meshes)
                {
                    if (mesh.Normals.Count == 0)
                        mesh.CalculateNormals();

                    for (int i = 0; i < mesh.Normals.Count; i++)
                    {
                        var normal = ApplyAxesTransforms(mesh.Normals[i]);
                        int found = -1;
                        for (int j = 0; j < normals.Count; j++)
                            if (normals[j] == normal)
                            {
                                found = j;
                                break;
                            }

                        if (found == -1)
                        {
                            found = normals.Count;
                            normals.Add(normal);
                        }

                        normalIndices.Add(i + meshCount, found);
                    }
                    meshCount += mesh.Normals.Count;
                }

                // Pack UV coords
                meshCount = 0;
                foreach (var mesh in model.Meshes)
                {
                    if (mesh.UV.Count == 0)
                        continue;

                    for (int i = 0; i < mesh.UV.Count; i++)
                    {
                        var uv = mesh.UV[i];
                        int found = -1;
                        for (int j = 0; j < uvs.Count; j++)
                            if (uvs[j] == uv)
                            {
                                found = j;
                                break;
                            }

                        if (found == -1)
                        {
                            found = uvs.Count;
                            uvs.Add(uv);
                        }

                        uvIndices.Add(i + meshCount, found);
                    }
                    meshCount += mesh.UV.Count;
                }

                // Save vertices
                if (!_settings.UseVertexColor)
                {
                    foreach (var pc in poscol)
                    {
                        writer.WriteLine("v " + pc[0].X.ToString(CultureInfo.InvariantCulture) + " " +
                                                pc[0].Y.ToString(CultureInfo.InvariantCulture) + " " +
                                                pc[0].Z.ToString(CultureInfo.InvariantCulture));
                    }
                }
                else
                {
                    // Include vertex colour, it may be parsed correctly by some software, e.g. MeshMixer and MeshLab.
                    foreach (var pc in poscol)
                    {
                        writer.WriteLine("v " + pc[0].X.ToString(CultureInfo.InvariantCulture) + " " +
                                                pc[0].Y.ToString(CultureInfo.InvariantCulture) + " " +
                                                pc[0].Z.ToString(CultureInfo.InvariantCulture) + " " +
                                                pc[1].X.ToString(CultureInfo.InvariantCulture) + " " +
                                                pc[1].Y.ToString(CultureInfo.InvariantCulture) + " " +
                                                pc[1].Z.ToString(CultureInfo.InvariantCulture));
                    }
                }
                writer.WriteLine("# " + poscol.Count + " vertices total.\n");

                // Save normals
                foreach (var normal in normals)
                {
                    writer.WriteLine("vn " + normal.X.ToString(CultureInfo.InvariantCulture) + " " +
                                             normal.Y.ToString(CultureInfo.InvariantCulture) + " " +
                                             normal.Z.ToString(CultureInfo.InvariantCulture));
                }
                writer.WriteLine("# " + normals.Count + " normals total.\n");

                // Save UVs
                foreach (var uv in uvs)
                {
                    writer.WriteLine("vt " + uv.X.ToString(CultureInfo.InvariantCulture) + " " +
                                    (1.0f - uv.Y).ToString(CultureInfo.InvariantCulture) + " 0.000");
                }
                writer.WriteLine("# " + uvs.Count + " UVs total.\n");

                int pCount = 0;
                int uCount = 0;
                int nCount = 0;

                foreach (var mesh in model.Meshes)
                {
                    writer.WriteLine("o " + mesh.Name + "\n");

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
                            writer.WriteLine("\n" + "usemtl " + lastMaterial.Name.ToString());
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
                                writer.WriteLine("f  " + (poscolIndices[v1 + pCount] + 1) + "/"
                                                       + (uvIndices    [v1 + uCount] + 1) + "/"
                                                       + (normalIndices[v1 + nCount] + 1) + " "
                                                       + (poscolIndices[v2 + pCount] + 1) + "/"
                                                       + (uvIndices    [v2 + uCount] + 1) + "/"
                                                       + (normalIndices[v2 + nCount] + 1) + " "
                                                       + (poscolIndices[v3 + pCount] + 1) + "/"
                                                       + (uvIndices    [v3 + uCount] + 1) + "/"
                                                       + (normalIndices[v3 + nCount] + 1));
                            }
                            else
                            {
                                writer.WriteLine("f  " + (poscolIndices[v1 + pCount] + 1) + "/"
                                                       + (uvIndices    [v1 + uCount] + 1) + "/"
                                                       + (normalIndices[v1 + nCount] + 1) + " "
                                                       + (poscolIndices[v2 + pCount] + 1) + "/"
                                                       + (uvIndices    [v2 + uCount] + 1) + "/"
                                                       + (normalIndices[v2 + nCount] + 1) + " "
                                                       + (poscolIndices[v3 + pCount] + 1) + "/"
                                                       + (uvIndices    [v3 + uCount] + 1) + "/"
                                                       + (normalIndices[v3 + nCount] + 1) + " "
                                                       + (poscolIndices[v4 + pCount] + 1) + "/"
                                                       + (uvIndices    [v4 + uCount] + 1) + "/"
                                                       + (normalIndices[v4 + nCount] + 1));
                            }
                            faceCount++;
                        }
                    }
                    pCount += mesh.Positions.Count;
                    uCount += mesh.UV.Count;
                    nCount += mesh.Normals.Count;

                    writer.WriteLine("# " + faceCount + " faces total.\n");
                }
            }

            using (var writer = new StreamWriter(materialPath, false))
            {
                writer.WriteLine("# Exported by Tomb Editor " +
                    Assembly.GetExecutingAssembly().GetName().Version.ToString() + "\n");

                foreach (var material in model.UsedMaterials)
                {
                    writer.WriteLine("newmtl " + material.Name);
                    writer.WriteLine("    Ka 1.000000 1.000000 1.000000");
                    writer.WriteLine("    Kd 1.000000 1.000000 1.000000");
                    writer.WriteLine("    Ni 1.000000");
                    writer.WriteLine("    Ns 0.000000");
                    writer.WriteLine("    d " + (material.AdditiveBlending ? "0.500000" : "1.000000"));
                    writer.WriteLine("    illum 2");
                    writer.WriteLine("    map_Ka " + Path.GetFileName(material.Path));
                    writer.WriteLine("    map_Kd " + Path.GetFileName(material.Path));
                    writer.WriteLine("\n");
                }
            }

            return true;
        }
    }
}
