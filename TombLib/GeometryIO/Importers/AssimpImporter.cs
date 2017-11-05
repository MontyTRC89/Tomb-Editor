using Assimp;
using SharpDX;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TombLib.Utils;

namespace TombLib.GeometryIO.Importers
{
    public class AssimpImporter : BaseGeometryImporter
    {
        public AssimpImporter(IOGeometrySettings settings)
            : base (settings)
        {

        }

        public override IOModel ImportFromFile(string filename)
        {
            // Use Assimp.NET for importing model
            AssimpContext context = new AssimpContext();
            Scene scene = context.ImportFile(filename, PostProcessPreset.TargetRealTimeMaximumQuality);

            var newModel = new IOModel();
            var textures = new Dictionary<int, IOTexture>();

            // Create the list of textures to load
            for (int i = 0; i < scene.Materials.Count; i++)
            {
                var mat = scene.Materials[i];

                var diffusePath = (mat.HasTextureDiffuse ? mat.TextureDiffuse.FilePath : null);
                if (diffusePath == null || diffusePath == "") continue;

                var found = false;
                for (var j = 0; j < textures.Count; j++)
                    if (textures.ElementAt(j).Value.Name == diffusePath)
                    {
                        found = true;
                        break;
                    }

                if (!found)
                {
                    var img = ImageC.FromFile(diffusePath);
                    textures.Add(i, new IOTexture(diffusePath, img.Width, img.Height));
                }
            }

            foreach (var text in textures)
                newModel.Textures.Add(text.Value);

            var lastBaseVertex = 0;

            // Loop for each mesh loaded in scene
            foreach (var mesh in scene.Meshes)
            {
                var newMesh = new IOMesh();

                if (!textures.ContainsKey(mesh.MaterialIndex)) continue;
                var faceTexture = textures[mesh.MaterialIndex];
                var hasTexCoords = mesh.HasTextureCoords(0);
                var hasColors = mesh.HasVertexColors(0);

                newMesh.Texture = faceTexture;

                // Source data
                var positions = mesh.Vertices;
                var texCoords = mesh.TextureCoordinateChannels[0];
                var colors = mesh.VertexColorChannels[0];

                for (int i = 0; i < mesh.VertexCount; i++)
                {
                    // Create position
                    var position = new Vector3(positions[i].X, positions[i].Y, positions[i].Z) * _settings.Scale;
                    position = ApplyAxesTransforms(position);
                    newMesh.Positions.Add(position);

                    // Create UV
                    var currentUV = new Vector2(texCoords[i].X, texCoords[i].Y);
                    currentUV = ApplyUVTransform(currentUV, faceTexture.Width, faceTexture.Height);
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

                        newMesh.Polygons.Add(poly);
                    }
                    else
                    {
                        var poly = new IOPolygon(IOPolygonShape.Quad);

                        poly.Indices.Add(lastBaseVertex + face.Indices[0]);
                        poly.Indices.Add(lastBaseVertex + face.Indices[1]);
                        poly.Indices.Add(lastBaseVertex + face.Indices[2]);
                        poly.Indices.Add(lastBaseVertex + face.Indices[3]);

                        newMesh.Polygons.Add(poly);
                    }
                }

                newModel.Meshes.Add(newMesh);
            }

            return newModel;
        }
    }
}
