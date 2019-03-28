using Assimp;
using Assimp.Configs;
using NLog;
using System.Collections.Generic;
using System.IO;
using System.Numerics;
using TombLib.Utils;

namespace TombLib.GeometryIO.Importers
{
    public class Assimp : BaseGeometryImporter
    {
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();

        public Assimp(IOGeometrySettings settings, GetTextureDelegate getTextureCallback)
            : base(settings, getTextureCallback)
        {

        }

        public override IOModel ImportFromFile(string filename)
        {
            string path = Path.GetDirectoryName(filename);

            // Use Assimp.NET for importing model
            AssimpContext context = new AssimpContext();
            context.SetConfig(new NormalSmoothingAngleConfig(90.0f));
            Scene scene = context.ImportFile(filename, PostProcessPreset.TargetRealTimeMaximumQuality);

            var newModel = new IOModel();
            var textures = new Dictionary<int, Texture>();

            // Create the list of materials to load
            for (int i = 0; i < scene.Materials.Count; i++)
            {
                var mat = scene.Materials[i];
                var material = new IOMaterial(mat.HasName ? mat.Name : "Material_" + i);

                var diffusePath = mat.HasTextureDiffuse ? mat.TextureDiffuse.FilePath : null;
                if (string.IsNullOrWhiteSpace(diffusePath))
                    continue;

                textures.Add(i, GetTexture(path, diffusePath));

                // Create the new material
                material.Texture = textures[i];
                material.AdditiveBlending = mat.HasBlendMode && mat.BlendMode == global::Assimp.BlendMode.Additive;
                material.DoubleSided = mat.HasTwoSided && mat.IsTwoSided;
                newModel.Materials.Add(material);
            }

            var lastBaseVertex = 0;

            // Loop for each mesh loaded in scene
            foreach (var mesh in scene.Meshes)
            {
                // Import only textured meshes with valid materials
                Texture faceTexture;
                if (!textures.TryGetValue(mesh.MaterialIndex, out faceTexture))
                {
                    logger.Warn("Mesh \"" + (mesh.Name ?? "") + "\" does have material index " + mesh.MaterialIndex + " which can't be found.");
                    continue;
                }

                // Assimp's mesh is our IOSubmesh so we import meshes with just one submesh
                var material = newModel.Materials[mesh.MaterialIndex];
                var newMesh = new IOMesh(mesh.Name);
                var newSubmesh = new IOSubmesh(material);
                newMesh.Submeshes.Add(material, newSubmesh);

                bool hasTexCoords = mesh.HasTextureCoords(0);
                bool hasColors = mesh.HasVertexColors(0);
                bool hasNormals = mesh.HasNormals;

                // Source data
                var positions = mesh.Vertices;
                var normals = mesh.Normals;
                var texCoords = mesh.TextureCoordinateChannels[0];
                var colors = mesh.VertexColorChannels[0];

                for (int i = 0; i < mesh.VertexCount; i++)
                {
                    // Create position
                    var position = new Vector3(positions[i].X, positions[i].Y, positions[i].Z);
                    position = ApplyAxesTransforms(position);
                    newMesh.Positions.Add(position);

                    // Create normal
                    var normal = new Vector3(normals[i].X, normals[i].Y, normals[i].Z);
                    normal = ApplyAxesTransforms(normal);
                    newMesh.Normals.Add(normal);

                    // Create UV
                    var currentUV = new Vector2(texCoords[i].X, texCoords[i].Y);
                    currentUV = ApplyUVTransform(currentUV, faceTexture.Image.Width, faceTexture.Image.Height);
                    newMesh.UV.Add(currentUV);

                    // Create colors
                    if (hasColors)
                    {
                        var color = new Vector4(colors[i].R, colors[i].G, colors[i].B, colors[i].A);
                        newMesh.Colors.Add(color);
                    }
                }

                // Add polygons
                foreach (var face in mesh.Faces)
                {
                    if (face.IndexCount == 3)
                    {
                        var poly = new IOPolygon(IOPolygonShape.Triangle);

                        poly.Indices.Add(lastBaseVertex + face.Indices[0]);
                        poly.Indices.Add(lastBaseVertex + face.Indices[1]);
                        poly.Indices.Add(lastBaseVertex + face.Indices[2]);

                        if (_settings.InvertFaces)
                            poly.Indices.Reverse();

                        newSubmesh.Polygons.Add(poly);
                    }
                    else if (face.IndexCount == 4)
                    {
                        var poly = new IOPolygon(IOPolygonShape.Quad);

                        poly.Indices.Add(lastBaseVertex + face.Indices[0]);
                        poly.Indices.Add(lastBaseVertex + face.Indices[1]);
                        poly.Indices.Add(lastBaseVertex + face.Indices[2]);
                        poly.Indices.Add(lastBaseVertex + face.Indices[3]);

                        if (_settings.InvertFaces)
                            poly.Indices.Reverse();

                        newSubmesh.Polygons.Add(poly);
                    }
                }

                newModel.Meshes.Add(newMesh);
            }

            return newModel;
        }
    }
}
