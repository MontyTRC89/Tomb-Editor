using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TombLib.Graphics;
using Assimp;
using SharpDX;
using Assimp.Configs;
using SharpDX.Toolkit.Graphics;

namespace TombEditor.Geometry
{
    public static class GeometryImporter
    {
        public static Dictionary<string, Texture2D> Textures;

        private static Editor _editor;
        private static DeviceManager _manager;

        public static void Initialize(DeviceManager manager)
        {
            _manager = manager;
            _editor = Editor.Instance;

            Textures = new Dictionary<string, Texture2D>();
        }

        public static RoomGeometryModel ImportGeometry(string filename)
        {
            // Use Assimp.NET for importing model
            AssimpImporter importer = new AssimpImporter();
            importer.SetConfig(new NormalSmoothingAngleConfig(66.0f));
            Scene scene = importer.ImportFile(filename, PostProcessPreset.TargetRealTimeMaximumQuality);

            // Create a new static model
            RoomGeometryModel model = new RoomGeometryModel(_manager.Device);

            // Load all textures
            foreach (var mat in scene.Materials)
            {
                var diffusePath = mat.GetTexture(TextureType.Diffuse, 0).FilePath;
                if (diffusePath == null || diffusePath == "") continue;

                if (!Textures.ContainsKey(diffusePath))
                    Textures.Add(diffusePath, Texture2D.Load(_manager.Device, diffusePath));
            }

            // Loop for each mesh loaded in scene
            foreach (var mesh in scene.Meshes)
            {
                RoomGeometryMesh modelMesh = new RoomGeometryMesh(_manager.Device, "Imported");

                //if mesh has a material extract the diffuse texture, if present
                Assimp.Material material = scene.Materials[mesh.MaterialIndex];
                if (material != null && material.GetTextureCount(TextureType.Diffuse) > 0)
                {
                    TextureSlot texture = material.GetTexture(TextureType.Diffuse, 0);

                    modelMesh.Texture = Textures[texture.FilePath]; 
                    modelMesh.TextureFileName = texture.FilePath;
                }
                else
                {
                    modelMesh.TextureFileName = "";
                }

                bool hasTexCoords = mesh.HasTextureCoords(0);

                Vector3D[] positions = mesh.Vertices;
                Vector3D[] texCoords = mesh.GetTextureCoords(0);

                // Determine primitive type (should be always triangle)
                switch (mesh.PrimitiveType)
                {
                    case Assimp.PrimitiveType.Point:
                        modelMesh.PrimitiveType = SharpDX.Toolkit.Graphics.PrimitiveType.PointList;
                        break;
                    case Assimp.PrimitiveType.Line:
                        modelMesh.PrimitiveType = SharpDX.Toolkit.Graphics.PrimitiveType.LineList;
                        break;
                    case Assimp.PrimitiveType.Triangle:
                        modelMesh.PrimitiveType = SharpDX.Toolkit.Graphics.PrimitiveType.TriangleList;
                        break;
                    default:
                        throw new Exception("Unknown primitive type");
                }

                // Add vertices
                List<RoomGeometryVertex> vertices = new List<RoomGeometryVertex>();

                for (int i = 0; i < mesh.VertexCount; i++)
                {
                    RoomGeometryVertex v = new RoomGeometryVertex();

                    v.Position = new Vector4(positions[i].X * 300.0f, 
                                             positions[i].Y * 300.0f,
                                             positions[i].Z * 300.0f, 
                                             1.0f);
                    v.UV = new Vector2(texCoords[i].X, 
                                       1.0f - texCoords[i].Y);

                    vertices.Add(v);
                }
                
                modelMesh.VertexCount = mesh.VertexCount;
                modelMesh.PrimitiveCount = mesh.FaceCount;

                // Add indices
                uint[] indices = mesh.GetIndices();

                List<int> tempIndices = new List<int>();

                for (int i = 0; i < indices.GetLength(0); i++)
                {
                    tempIndices.Add((int)indices[i]);
                }
                
                modelMesh.IndexCount = indices.GetLength(0);

                // Prepare DirectX data for this mesh
                modelMesh.Vertices.AddRange(vertices);
                modelMesh.Indices.AddRange(tempIndices);
                modelMesh.BuildBuffers();

                // Add mesh to the model
                model.Meshes.Add(modelMesh);
            }

            return model;
        }
    }
}
