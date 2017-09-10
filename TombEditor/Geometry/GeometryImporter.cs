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
using System.IO;

namespace TombEditor.Geometry
{
    public static class GeometryImporterExporter
    {
        public static Dictionary<string, Texture2D> Textures { get; private set; }
        public static Dictionary<string, RoomGeometryModel> Models { get; private set; }
        
        private static DeviceManager _manager;

        public static void Initialize(DeviceManager manager)
        {
            _manager = manager;

            Textures = new Dictionary<string, Texture2D>();
            Models = new Dictionary<string, RoomGeometryModel>();
        }

        public static RoomGeometryModel LoadModel(string filename, float scale = 300.0f)
        {
            // Load the model just one time
            if (Models.ContainsKey(filename)) return Models[filename];

            // Use Assimp.NET for importing model
            AssimpContext context = new AssimpContext();
            Scene scene = context.ImportFile(filename, PostProcessPreset.TargetRealTimeMaximumQuality);
            
            // Create a new static model
            RoomGeometryModel model = new RoomGeometryModel(_manager.Device, scale);

            model.Name = Path.GetFileName(filename);

            // Load all textures
            foreach (var mat in scene.Materials)
            {
                var diffusePath = (mat.HasTextureDiffuse ? mat.TextureDiffuse.FilePath : null);
                if (diffusePath == null || diffusePath == "") continue;

                if (!Textures.ContainsKey(diffusePath))
                    Textures.Add(diffusePath, Texture2D.Load(_manager.Device, diffusePath));
            }

            Vector3 minVertex = new Vector3(float.MaxValue, float.MaxValue, float.MaxValue);
            Vector3 maxVertex = new Vector3(float.MinValue, float.MinValue, float.MinValue); ;
            
            // Loop for each mesh loaded in scene
            foreach (var mesh in scene.Meshes)
            {
                RoomGeometryMesh modelMesh = new RoomGeometryMesh(_manager.Device, "Imported");

                //if mesh has a material extract the diffuse texture, if present
                Assimp.Material material = scene.Materials[mesh.MaterialIndex];
                if (material != null && material.HasTextureDiffuse)
                {
                    TextureSlot texture = material.TextureDiffuse;

                    modelMesh.Texture = Textures[texture.FilePath]; 
                    modelMesh.TextureFileName = texture.FilePath;
                }
                else
                {
                    modelMesh.TextureFileName = "";
                }

                bool hasTexCoords = mesh.HasTextureCoords(0);

                List<Vector3D> positions = mesh.Vertices;
                List<Vector3D> texCoords = mesh.TextureCoordinateChannels[0];

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

                    v.Position = new Vector3(positions[i].X, positions[i].Y, positions[i].Z) * scale;

                    if (v.Position.X <= minVertex.X && v.Position.Y <= minVertex.Y && v.Position.Z <= minVertex.Z)
                        minVertex = v.Position;

                    if (v.Position.X >= maxVertex.X && v.Position.Y >= maxVertex.Y && v.Position.Z >= maxVertex.Z)
                        maxVertex = v.Position;

                    if (hasTexCoords)
                    {
                        v.UV = new Vector2(texCoords[i].X,
                                           1.0f - texCoords[i].Y);
                    }

                    vertices.Add(v);
                }
                
                modelMesh.VertexCount = mesh.VertexCount;
                modelMesh.PrimitiveCount = mesh.FaceCount;

                // Add indices
                int[] indices = mesh.GetIndices();

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

            // Set the bounding box
            model.BoundingBox = new BoundingBox(minVertex, maxVertex);

            // Add the model to global loaded models
            Models.Add(filename, model);

            return model;
        }

        public static void UnloadModel(string name)
        {
            // TODO: check in rooms

            // Remove the model and clean garbage
            Models[name].Dispose();
            Models.Remove(name);
            CleanUpGarbage();
        }

        public static bool ExportRoomToObj(Room room, string fileName)
        {
            var roomVertices = room.GetRoomVertices();

            using (var writer = new StreamWriter(new FileStream(fileName, FileMode.Create, FileAccess.Write, FileShare.Write)))
            {
                writer.WriteLine("# Exported by Tomb Editor");
                writer.WriteLine("o Room");

                // Export positions
                foreach (var vertex in roomVertices)
                {
                    writer.WriteLine("v " + vertex.Position.X + " " + vertex.Position.Y + " " + vertex.Position.Z + " 1.0");
                }

                // TODO: to remove when materials will be exported
                Random r = new Random(new Random().Next());

                // Export UVs
                foreach (var vertex in roomVertices)
                {
                    // TODO: to remove when materials will be exported
                    float randX = r.NextFloat(0, 1);
                    float randY = r.NextFloat(0, 1);

                    writer.WriteLine("vt " + /*vertex.UV.X*/ randX + " " + randY /* vertex.UV.Y*/);
                }

                writer.WriteLine("s 1");

                // Export faces
                for (int i = 0; i < roomVertices.Count; ++i)
                    writer.WriteLine("f " + i + " " + (i + 1) + " " + (i + 2));

                writer.Flush();
            }

            return true;
        }

        public static void CleanUpGarbage()
        {
            List<string> texturesToRemove = new List<string>();

            // Loop for each model and found used/unused textures
            for (int t = 0; t < Textures.Count; t++)
            {
                bool used = false;

                for (int i = 0; i < Models.Count; i++)
                {
                    RoomGeometryModel model = Models.ElementAt(i).Value;

                    foreach (RoomGeometryMesh mesh in model.Meshes)
                    {
                        if (mesh.TextureFileName == Textures.ElementAt(t).Key)
                        {
                            used = true;
                            break;
                        }
                    }

                    if (used) break;
                }

                if (used) continue;

                texturesToRemove.Add(Textures.ElementAt(t).Key);
            }

            // Dispose textures and remove them from the dictionary
            foreach (var texture in texturesToRemove)
            {
                Textures[texture].Dispose();
                Textures.Remove(texture);
            }

            // Collect garbage
            GC.Collect();
        }
    }
}
