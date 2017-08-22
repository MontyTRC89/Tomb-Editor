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
        public static Dictionary<string, Texture2D> Textures;
        public static Dictionary<string, RoomGeometryModel> Models;

        private static Editor _editor;
        private static DeviceManager _manager;

        public static void Initialize(DeviceManager manager)
        {
            _manager = manager;
            _editor = Editor.Instance;

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
            RoomGeometryModel model = new RoomGeometryModel(_manager.Device);

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

                    v.Position = new Vector4(positions[i].X * scale, 
                                             positions[i].Y * scale,
                                             positions[i].Z * scale,
                                             1.0f);

                    if (v.Position.X <= minVertex.X && v.Position.Y <= minVertex.Y && v.Position.Z <= minVertex.Z)
                        minVertex = new Vector3(v.Position.X, v.Position.Y, v.Position.Z);

                    if (v.Position.X >= maxVertex.X && v.Position.Y >= maxVertex.Y && v.Position.Z >= maxVertex.Z)
                        maxVertex = new Vector3(v.Position.X, v.Position.Y, v.Position.Z);

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
            if (File.Exists(fileName)) File.Delete(fileName);

            using (var writer = new StreamWriter(File.OpenWrite(fileName)))
            {
                writer.WriteLine("# Exported by Tomb Editor");
                writer.WriteLine("o Room");

                // Export positions
                foreach (var vertex in room.Vertices)
                {
                    writer.WriteLine("v " + vertex.Position.X + " " + vertex.Position.Y + " " + vertex.Position.Z + " 1.0");
                }

                // TODO: to remove when materials will be exported
                Random r = new Random(new Random().Next());

                // Export UVs
                foreach (var vertex in room.Vertices)
                {
                    // TODO: to remove when materials will be exported
                    float randX = r.NextFloat(0, 1);
                    float randY = r.NextFloat(0, 1);

                    writer.WriteLine("vt " + /*vertex.UV.X*/ randX + " " + randY /* vertex.UV.Y*/);
                }

                writer.WriteLine("s 1");

                // Export faces
                for (int x = 0; x < room.NumXSectors; x++)
                {
                    for (int z = 0; z < room.NumZSectors; z++)
                    {
                        foreach (var face in room.Blocks[x, z].Faces)
                        {
                            if (!face.Defined) continue;

                            writer.Write("f");

                            for (int i = 0; i < face.IndicesForSolidBucketsRendering.Count;i++)
                            {
                                int index = face.IndicesForSolidBucketsRendering[i];

                                if ((index % 3) == 0 && i != 0)
                                {
                                    writer.WriteLine();
                                    writer.Write("f");
                                }

                                writer.Write(" " + (index + 1) + "/" + (index + 1));
                            }

                            writer.WriteLine();
                        }
                    }
                }

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
