using System;
using System.Collections.Generic;
using System.IO;
using System.Numerics;
using TombLib.GeometryIO;
using TombLib.IO;
using TombLib.Utils;
using System.Linq;
using TombLib.GeometryIO.Importers;
using NLog;

namespace TombLib.Wad
{
    public class WadMesh : ICloneable
    {
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();

        public string Name { get; set; }
        public List<Vector3> VerticesPositions { get; set; } = new List<Vector3>();
        public List<Vector3> VerticesNormals { get; set; } = new List<Vector3>();
        public List<short> VerticesShades { get; set; } = new List<short>();
        public List<WadPolygon> Polys { get; set; } = new List<WadPolygon>();
        public Hash Hash { get; private set; }
        public BoundingSphere BoundingSphere { get; set; }
        public BoundingBox BoundingBox { get; set; }
        public WadMeshLightingType LightingType { get; set; }

        public void CalculateNormals()
        {
            VerticesNormals.Clear();

            var tempNormals = new List<VertexNormalAverageHelper>();
            for (int i = 0; i < VerticesPositions.Count; i++)
                tempNormals.Add(new VertexNormalAverageHelper());

            foreach (var poly in Polys)
            {
                var p0 = VerticesPositions[poly.Index0];
                var p1 = VerticesPositions[poly.Index1];
                var p2 = VerticesPositions[poly.Index2];

                var v1 = p0 - p2;
                var v2 = p1 - p2;
                var normal = Vector3.Cross(v1, v2);

                tempNormals[poly.Index0].Normal += normal;
                tempNormals[poly.Index0].NumVertices++;

                tempNormals[poly.Index1].Normal += normal;
                tempNormals[poly.Index1].NumVertices++;

                tempNormals[poly.Index2].Normal += normal;
                tempNormals[poly.Index2].NumVertices++;

                if (poly.Shape == WadPolygonShape.Quad)
                {
                    tempNormals[poly.Index3].Normal += normal;
                    tempNormals[poly.Index3].NumVertices++;
                }
            }

            for (int i = 0; i < tempNormals.Count; i++)
            {
                var normal = tempNormals[i].Normal / Math.Max(1, tempNormals[i].NumVertices);
                normal = Vector3.Normalize(normal);
                VerticesNormals.Add(normal);
            }
        }

        public WadMesh Clone()
        {
            var mesh = (WadMesh)MemberwiseClone();
            mesh.VerticesPositions = new List<Vector3>(VerticesPositions);
            mesh.VerticesNormals = new List<Vector3>(VerticesNormals);
            mesh.VerticesShades = new List<short>(VerticesShades);
            mesh.Polys = new List<WadPolygon>(Polys);
            return mesh;
        }
        object ICloneable.Clone() => Clone();

        public byte[] ToByteArray()
        {
            using (var ms = new MemoryStream())
            {
                var writer = new BinaryWriterEx(ms);
                writer.Write(BoundingSphere.Center.X);
                writer.Write(BoundingSphere.Center.Y);
                writer.Write(BoundingSphere.Center.Z);
                writer.Write(BoundingSphere.Radius);

                int numVertices = VerticesPositions.Count;
                writer.Write(numVertices);

                for (int i = 0; i < VerticesPositions.Count; i++)
                    writer.Write(VerticesPositions[i]);

                if (VerticesNormals.Count > 0)
                    for (int i = 0; i < VerticesNormals.Count; i++)
                        writer.Write(VerticesNormals[i]);

                if (VerticesShades.Count > 0)
                    for (int i = 0; i < VerticesShades.Count; i++)
                        writer.Write(VerticesShades[i]);

                int numPolygons = Polys.Count;
                writer.Write(numPolygons);
                for (int i = 0; i < Polys.Count; i++)
                {
                    WadPolygon poly = Polys[i];
                    writer.Write((ushort)Polys[i].Shape);
                    writer.Write(Polys[i].Index0);
                    writer.Write(Polys[i].Index1);
                    writer.Write(Polys[i].Index2);
                    if (Polys[i].Shape == WadPolygonShape.Quad)
                        writer.Write(Polys[i].Index3);
                    writer.Write(((WadTexture)Polys[i].Texture.Texture).Hash);
                    writer.Write(Polys[i].Texture.DoubleSided);
                    writer.Write((short)Polys[i].Texture.BlendMode);
                    writer.Write(Polys[i].Texture.TexCoord0);
                    writer.Write(Polys[i].Texture.TexCoord1);
                    writer.Write(Polys[i].Texture.TexCoord2);
                    if (Polys[i].Shape == WadPolygonShape.Quad)
                        writer.Write(Polys[i].Texture.TexCoord3);
                    writer.Write(Polys[i].ShineStrength);
                }

                return ms.ToArray();
            }
        }

        public BoundingBox CalculateBoundingBox(Matrix4x4 transform)
        {
            float minX = float.MaxValue;
            float minY = float.MaxValue;
            float minZ = float.MaxValue;
            float maxX = float.MinValue;
            float maxY = float.MinValue;
            float maxZ = float.MinValue;
            
            foreach (Vector3 oldVertex in VerticesPositions)
            {
                var transformedVertex = MathC.HomogenousTransform(oldVertex, transform);

                minX = (float)Math.Min(transformedVertex.X, minX);
                minY = (float)Math.Min(transformedVertex.Y, minY);
                minZ = (float)Math.Min(transformedVertex.Z, minZ);
                maxX = (float)Math.Max(transformedVertex.X, maxX);
                maxY = (float)Math.Max(transformedVertex.Y, maxY);
                maxZ = (float)Math.Max(transformedVertex.Z, maxZ);
            }

            return new BoundingBox(new Vector3(minX, maxY, minZ), new Vector3(maxX, minY, maxZ));
        }

        public BoundingBox CalculateBoundingBox()
        {
            Vector3 min = new Vector3(float.MaxValue);
            Vector3 max = new Vector3(float.MinValue);
            foreach (Vector3 oldVertex in VerticesPositions)
            {
                min = Vector3.Min(oldVertex, min);
                max = Vector3.Max(oldVertex, max);
            }
            return new BoundingBox(min, max);
        }

        public BoundingSphere CalculateBoundingSphere()
        {
            BoundingBox boundingBox = CalculateBoundingBox();
            return new BoundingSphere(
                (boundingBox.Maximum + boundingBox.Minimum) * 0.5f,
                Vector3.Distance(boundingBox.Minimum, boundingBox.Maximum) * 0.5f);
        }

        public static WadMesh ImportFromExternalModel(string fileName, IOGeometrySettings settings)
        {
            var list = ImportFromExternalModel(fileName, settings, true);
            if (list != null && list.Count > 0)
                return list.First();
            else
                return null;
        }

        public static List<WadMesh> ImportFromExternalModel(string fileName, IOGeometrySettings settings, bool mergeIntoOne)
        {
            IOModel tmpModel = null;
            var meshList = new List<WadMesh>();

            bool calculateNormals = false;

            // Import the model
            try
            {
                var importer = BaseGeometryImporter.CreateForFile(fileName, settings, absoluteTexturePath =>
                {
                    return new WadTexture(ImageC.FromFile(absoluteTexturePath));
                });
                tmpModel = importer.ImportFromFile(fileName);

                calculateNormals = importer is MetasequoiaImporter;
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Geometry import failed!");
                return null;
            }

            // Create a new mesh (all meshes from model will be joined)
            WadMesh mesh = null;
            var lastBaseVertex = 0;
            for (int i = 0; i < tmpModel.Meshes.Count; i++)
            {
                var tmpMesh = tmpModel.Meshes[i];

                if (mesh == null || !mergeIntoOne)
                {
                    mesh = new WadMesh();
                    mesh.Name = string.IsNullOrEmpty(tmpMesh.Name) ? "ImportedMesh" + i : tmpMesh.Name;

                    if (mergeIntoOne) lastBaseVertex = 0; // Reset if we're doing multi-mesh import
                }

                mesh.VerticesPositions.AddRange(tmpMesh.Positions);

                // Copy normals as well, if they are consistent
                if (tmpMesh.Normals.Count == tmpMesh.Positions.Count)
                    mesh.VerticesNormals.AddRange(tmpMesh.Normals);

                // FIXME: Why do we keep white intensity shades for wad2 meshes internally, and not vertex colors?
                if (tmpMesh.Colors.Count == tmpMesh.Positions.Count)
                    mesh.VerticesShades.AddRange(tmpMesh.Colors.Select(v => (short)(8191.0f - (v.To3().GetLuma() * 8191.0f))));

                foreach (var tmpSubmesh in tmpMesh.Submeshes)
                    foreach (var tmpPoly in tmpSubmesh.Value.Polygons)
                    {
                        if (tmpPoly.Shape == IOPolygonShape.Quad)
                        {
                            var poly = new WadPolygon { Shape = WadPolygonShape.Quad };
                            poly.Index0 = tmpPoly.Indices[0] + lastBaseVertex;
                            poly.Index1 = tmpPoly.Indices[1] + lastBaseVertex;
                            poly.Index2 = tmpPoly.Indices[2] + lastBaseVertex;
                            poly.Index3 = tmpPoly.Indices[3] + lastBaseVertex;

                            var area = new TextureArea();
                            area.TexCoord0 = tmpMesh.UV[tmpPoly.Indices[0]];
                            area.TexCoord1 = tmpMesh.UV[tmpPoly.Indices[1]];
                            area.TexCoord2 = tmpMesh.UV[tmpPoly.Indices[2]];
                            area.TexCoord3 = tmpMesh.UV[tmpPoly.Indices[3]];
                            area.Texture = tmpSubmesh.Value.Material.Texture;
                            area.DoubleSided = tmpSubmesh.Value.Material.DoubleSided;

                            poly.Texture = area;
                            poly.ShineStrength = (byte)Math.Round(tmpSubmesh.Value.Material.Shininess / 16.0f, MidpointRounding.ToEven);

                            mesh.Polys.Add(poly);
                        }
                        else
                        {
                            var poly = new WadPolygon { Shape = WadPolygonShape.Triangle };
                            poly.Index0 = tmpPoly.Indices[0] + lastBaseVertex;
                            poly.Index1 = tmpPoly.Indices[1] + lastBaseVertex;
                            poly.Index2 = tmpPoly.Indices[2] + lastBaseVertex;

                            var area = new TextureArea();
                            area.TexCoord0 = tmpMesh.UV[tmpPoly.Indices[0]];
                            area.TexCoord1 = tmpMesh.UV[tmpPoly.Indices[1]];
                            area.TexCoord2 = tmpMesh.UV[tmpPoly.Indices[2]];
                            area.TexCoord3 = area.TexCoord2;
                            area.Texture = tmpSubmesh.Value.Material.Texture;
                            area.DoubleSided = tmpSubmesh.Value.Material.DoubleSided;

                            poly.Texture = area;
                            poly.ShineStrength = (byte)Math.Round(tmpSubmesh.Value.Material.Shininess / 16.0f, MidpointRounding.ToEven);

                            mesh.Polys.Add(poly);
                        }
                    }

                if (!mergeIntoOne || i == tmpModel.Meshes.Count - 1)
                {
                    mesh.BoundingBox = mesh.CalculateBoundingBox();
                    mesh.BoundingSphere = mesh.CalculateBoundingSphere();

                    if (mesh.VerticesNormals.Count == 0 || calculateNormals)
                        mesh.CalculateNormals(); // MQO files rarely have normals
                    
                    if (mesh.VerticesPositions.Count != mesh.VerticesShades.Count)
                        mesh.VerticesShades.Clear(); // Reset vertex shades in case they got desynced from vertex count

                    lastBaseVertex = 0;
                    meshList.Add(mesh);
                }
                else
                    lastBaseVertex = mesh.VerticesPositions.Count;
            }

            return meshList;
        }

        public static bool operator ==(WadMesh first, WadMesh second) => ReferenceEquals(first, null) ? ReferenceEquals(second, null) : (ReferenceEquals(second, null) ? false : first.Hash == second.Hash);
        public static bool operator !=(WadMesh first, WadMesh second) => !(first == second);
        public bool Equals(WadMesh other) => Hash == other.Hash;
        public override bool Equals(object other) => other is WadMesh && Hash == ((WadMesh)other).Hash;
        public override int GetHashCode() => Hash.GetHashCode();

        public static WadMesh Empty { get; } = new WadMesh();
    }
}

