using NLog;
using SharpDX.Toolkit.Graphics;
using System;
using System.Collections.Generic;
using System.Numerics;
using TombLib.LevelData;
using TombLib.Utils;
using Buffer = SharpDX.Toolkit.Graphics.Buffer;

namespace TombLib.Graphics
{
    public class ImportedGeometryRenderableMesh : Mesh<ImportedGeometryVertex>
    {
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();

        public bool HasVertexColors { get; set; }

        public ImportedGeometryRenderableMesh(GraphicsDevice device, ImportedGeometryMesh source)
            : base(device, source.Name)
        {
            HasVertexColors = source.HasVertexColors;
            BoundingBox = source.BoundingBox;
            Vertices.AddRange(source.Vertices);

            foreach (var submesh in source.Submeshes)
            {
                var newSubmesh = new Submesh(submesh.Key)
                {
                    BaseIndex = submesh.Value.BaseIndex,
                    MeshBaseIndex = submesh.Value.MeshBaseIndex
                };
                newSubmesh.Indices.AddRange(submesh.Value.Indices);
                Submeshes.Add(submesh.Key, newSubmesh);
            }
        }

        public void UpdateBuffers(Vector3? position = null)
        {
            if (Vertices.Count == 0)
                return;

            DepthSort(position);
            UpdateBoundingBox();

            VertexBuffer?.Dispose();
            IndexBuffer?.Dispose();

            VertexBuffer = Buffer.Vertex.New(GraphicsDevice, Vertices.ToArray(), SharpDX.Direct3D11.ResourceUsage.Immutable);
            InputLayout = VertexInputLayout.FromBuffer(0, VertexBuffer);
            IndexBuffer = Buffer.Index.New(GraphicsDevice, Indices.ToArray(), SharpDX.Direct3D11.ResourceUsage.Immutable);

            if (VertexBuffer == null)
                logger.Error("Vertex Buffer of Imported Geometry " + Name + " could not be created!");
            if (InputLayout == null)
                logger.Error("Input Layout of Imported Geometry " + Name + " could not be created!");
            if (IndexBuffer == null)
                logger.Error("Index Buffer of Imported Geometry " + Name + " could not be created!");
        }
    }

    public class ImportedGeometryRenderableModel : Model<ImportedGeometryRenderableMesh, ImportedGeometryVertex>
    {
        public float Scale { get; }

        public ImportedGeometryRenderableModel(GraphicsDevice device, float scale)
            : base(device, ModelType.RoomGeometry)
        {
            Scale = scale;
        }

        public override void UpdateBuffers(Vector3? position = null)
        {
            foreach (var mesh in Meshes)
                mesh.UpdateBuffers(position);
        }
    }

    public class ImportedGeometryRenderer : IDisposable
    {
        private sealed class CachedModel
        {
            public DataVersion Version { get; init; }
            public ImportedGeometryRenderableModel Model { get; init; }
        }

        private sealed class CachedTexture
        {
            public DataVersion Version { get; init; }
            public Texture2D Texture { get; init; }
        }

        public GraphicsDevice GraphicsDevice { get; }

        private Dictionary<ImportedGeometry, CachedModel> Models { get; } = new Dictionary<ImportedGeometry, CachedModel>();
        private Dictionary<ImportedGeometryTexture, CachedTexture> Textures { get; } = new Dictionary<ImportedGeometryTexture, CachedTexture>();

        public ImportedGeometryRenderer(GraphicsDevice graphicsDevice)
        {
            GraphicsDevice = graphicsDevice;
        }

        public ImportedGeometryRenderableModel GetModel(ImportedGeometry geometry)
        {
            if (geometry?.DirectXModel == null)
                return null;

            if (Models.TryGetValue(geometry, out var cachedModel))
            {
                if (cachedModel.Version >= geometry.DirectXModel.Version)
                    return cachedModel.Model;

                cachedModel.Model.Dispose();
                Models.Remove(geometry);
            }

            var model = BuildModel(geometry.DirectXModel);
            Models.Add(geometry, new CachedModel { Model = model, Version = geometry.DirectXModel.Version });
            return model;
        }

        public Texture2D GetTexture(ImportedGeometryTexture texture)
        {
            if (texture == null)
                return null;

            if (Textures.TryGetValue(texture, out var cachedTexture))
            {
                if (cachedTexture.Version >= texture.Version)
                    return cachedTexture.Texture;

                cachedTexture.Texture.Dispose();
                Textures.Remove(texture);
            }

            var directXTexture = TextureLoad.Load(GraphicsDevice, texture.Image);
            Textures.Add(texture, new CachedTexture { Texture = directXTexture, Version = texture.Version });
            return directXTexture;
        }

        public void GarbageCollect() => Dispose();

        public void Dispose()
        {
            foreach (var model in Models.Values)
                model.Model.Dispose();
            Models.Clear();

            foreach (var texture in Textures.Values)
                texture.Texture.Dispose();
            Textures.Clear();
        }

        private ImportedGeometryRenderableModel BuildModel(ImportedGeometry.Model source)
        {
            var model = new ImportedGeometryRenderableModel(GraphicsDevice, source.Scale)
            {
                BoundingBox = source.BoundingBox,
                Version = source.Version
            };

            model.Materials.AddRange(source.Materials);

            foreach (var mesh in source.Meshes)
                model.Meshes.Add(new ImportedGeometryRenderableMesh(GraphicsDevice, mesh));

            model.UpdateBuffers();
            return model;
        }
    }
}
