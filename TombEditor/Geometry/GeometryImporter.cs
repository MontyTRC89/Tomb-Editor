using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TombLib.Graphics;
using Assimp;
using SharpDX;

namespace TombEditor.Geometry
{
    public static class GeometryImporter
    {
        private static Editor _editor;
        private static DeviceManager _manager;

        public static void Initialize(DeviceManager manager)
        {
            _manager = manager;
            _editor = Editor.Instance;
        }

        public static StaticModel ImportGeometry(string filename)
        {
            // Use Assimp.NET for importing model
            AssimpImporter importer = new AssimpImporter();
            Scene scene = importer.ImportFile(filename, PostProcessSteps.Triangulate | 
                                                        PostProcessSteps.SortByPrimitiveType | 
                                                        PostProcessSteps.OptimizeMeshes | 
                                                        PostProcessSteps.JoinIdenticalVertices);

            // Create a new static model
            StaticModel model = new StaticModel(_manager.Device);
                    
            // Loop for each mesh loaded in scene
            foreach (var mesh in scene.Meshes)
            {
                List<StaticVertex> vertices = new List<StaticVertex>();
                List<uint> indices = new List<uint>();

                StaticMesh importedMesh = new StaticMesh(_manager.Device, "IMPORTED");

                // Build the vertices array
                // HACK: the scale is 300.0f for sponza.obj but in the future or we add a scaling parameter in a 
                // window before import or we force users to build their models respecting the scale of TR world
                for (int i = 0; i < mesh.VertexCount; i++)
                {
                    StaticVertex vertex = new StaticVertex();
                    vertex.Position = new Vector4(mesh.Vertices[i].X * 300.0f, mesh.Vertices[i].Y * 300.0f, mesh.Vertices[i].Z * 300.0f, 1.0f);
                    vertex.UV = new Vector2(mesh.GetTextureCoords(0)[i].X, mesh.GetTextureCoords(0)[i].Y);
                    vertices.Add(vertex);
                }

                // Add vertices to the mesh
                importedMesh.Vertices.AddRange(vertices);

                // Add indices to the mesh
                var tempIndices = mesh.GetIndices();
                for (int i = 0; i < tempIndices.Length; i++)
                    importedMesh.Indices.Add((int)tempIndices[i]);

                // Add the mesh to the model
                model.Meshes.Add(importedMesh);
            }

            // Build the DirectX buffer for this model
            model.BuildBuffers();

            return model;
        }
    }
}
