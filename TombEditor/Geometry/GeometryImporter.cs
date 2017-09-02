using System;
using System.Collections.Generic;
using TombLib.Graphics;
using Assimp;
using SharpDX;
using SharpDX.Toolkit.Graphics;
using System.IO;

namespace TombEditor.Geometry
{
    public static class GeometryImporterExporter
    {
        private static Dictionary<string, Texture2D> _textures;
        public static Dictionary<string, RoomGeometryModel> Models;

        private static DeviceManager _manager;

        public static void Initialize(DeviceManager manager)
        {
            _manager = manager;

            _textures = new Dictionary<string, Texture2D>();
            Models = new Dictionary<string, RoomGeometryModel>();
        }

        public static void LoadModel(string filename, float scale = 300.0f)
        {
            // Load the model just one time
            if (Models.ContainsKey(filename)) return;

            // Use Assimp.NET for importing model
            var context = new AssimpContext();
            var scene = context.ImportFile(filename, PostProcessPreset.TargetRealTimeMaximumQuality);

            // Create a new static model
            var model = new RoomGeometryModel(_manager.Device) {Name = Path.GetFileName(filename)};

            // Load all textures
            foreach (var mat in scene.Materials)
            {
                var diffusePath = (mat.HasTextureDiffuse ? mat.TextureDiffuse.FilePath : null);
                if (string.IsNullOrEmpty(diffusePath)) continue;

                if (!_textures.ContainsKey(diffusePath))
                    _textures.Add(diffusePath, Texture2D.Load(_manager.Device, diffusePath));
            }

            var minVertex = new Vector3(float.MaxValue, float.MaxValue, float.MaxValue);
            var maxVertex = new Vector3(float.MinValue, float.MinValue, float.MinValue);

            // Loop for each mesh loaded in scene
            foreach (var mesh in scene.Meshes)
            {
                var modelMesh = new RoomGeometryMesh(_manager.Device, "Imported");

                //if mesh has a material extract the diffuse texture, if present
                var material = scene.Materials[mesh.MaterialIndex];
                if (material != null && material.HasTextureDiffuse)
                {
                    var texture = material.TextureDiffuse;

                    modelMesh.Texture = _textures[texture.FilePath];
                    modelMesh.TextureFileName = texture.FilePath;
                }
                else
                {
                    modelMesh.TextureFileName = "";
                }

                bool hasTexCoords = mesh.HasTextureCoords(0);

                var positions = mesh.Vertices;
                var texCoords = mesh.TextureCoordinateChannels[0];

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
                var vertices = new List<RoomGeometryVertex>();

                for (int i = 0; i < mesh.VertexCount; i++)
                {
                    var v = new RoomGeometryVertex
                    {
                        Position = new Vector3(positions[i].X, positions[i].Y, positions[i].Z) * scale
                    };


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
                var indices = mesh.GetIndices();

                var tempIndices = new List<int>();

                for (int i = 0; i < indices.GetLength(0); i++)
                {
                    tempIndices.Add(indices[i]);
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
        }

        public static void ExportRoomToObj(Room room, string fileName)
        {
            var roomVertices = room.GetRoomVertices();

            using (var writer =
                new StreamWriter(new FileStream(fileName, FileMode.Create, FileAccess.Write, FileShare.Write)))
            {
                writer.WriteLine("# Exported by Tomb Editor");
                writer.WriteLine("o Room");

                // Export positions
                foreach (var vertex in roomVertices)
                {
                    writer.WriteLine("v " + vertex.Position.X + " " + vertex.Position.Y + " " + vertex.Position.Z +
                                     " 1.0");
                }

                // TODO: to remove when materials will be exported
                var r = new Random(new Random().Next());

                // Export UVs
                foreach (var unused in roomVertices)
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
        }
    }
}
