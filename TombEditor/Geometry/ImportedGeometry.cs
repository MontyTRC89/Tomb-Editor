using NLog;
using SharpDX;
using SharpDX.Toolkit.Graphics;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using TombLib.GeometryIO;
using TombLib.GeometryIO.Importers;
using TombLib.Graphics;
using TombLib.Utils;
using Buffer = SharpDX.Toolkit.Graphics.Buffer;
using Texture = TombLib.Utils.Texture;

namespace TombEditor.Geometry
{
    public class ImportedGeometryTexture : Texture
    {
        public Texture2D DirectXTexture { get; }
        public string AbsolutePath { get; }

        public ImportedGeometryTexture(string absolutePath)
        {
            AbsolutePath = absolutePath;
            Image = ImageC.FromFile(absolutePath);
            DirectXTexture = TextureLoad.Load(DeviceManager.DefaultDeviceManager.Device, Image);
        }

        private ImportedGeometryTexture(ImportedGeometryTexture other)
        {
            DirectXTexture = other.DirectXTexture;
            AbsolutePath = other.AbsolutePath;
            Image = other.Image;
        }

        public override Texture Clone() => new ImportedGeometryTexture(this);
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct ImportedGeometryVertex : IVertex
    {
        [VertexElement("POSITION", 0, SharpDX.DXGI.Format.R32G32B32_Float, 0)]
        public Vector3 Position;
        private float _unusedPadding;
        [VertexElement("TEXCOORD", 0, SharpDX.DXGI.Format.R32G32_Float, 16)]
        public Vector2 UV;
        [VertexElement("NORMAL", 0, SharpDX.DXGI.Format.R32G32B32_Float, 24)]
        public Vector3 Normal;
        [VertexElement("TEXCOORD", 1, SharpDX.DXGI.Format.R32G32_Float, 36)]
        public Vector2 Shade;
        [VertexElement("COLOR", 0, SharpDX.DXGI.Format.R32G32B32A32_Float, 44)]
        public Vector4 Color;

        Vector3 IVertex.Position => Position;
    }

    public class ImportedGeometryMesh : Mesh<ImportedGeometryVertex>
    {
        public Texture Texture { get; set; }

        public ImportedGeometryMesh(GraphicsDevice device, string name)
            : base(device, name)
        { }

        public override void BuildBuffers()
        {
            BaseIndex = 0;
            NumIndices = Indices.Count;

            if (Vertices.Count == 0)
                return;

            Vector3 minVertex = new Vector3(float.MaxValue);
            Vector3 maxVertex = new Vector3(float.MinValue);
            foreach (var vertex in Vertices)
            {
                minVertex = Vector3.Min(minVertex, vertex.Position);
                maxVertex = Vector3.Max(maxVertex, vertex.Position);
            }
            BoundingBox = new BoundingBox(minVertex, maxVertex);

            VertexBuffer = Buffer.Vertex.New(GraphicsDevice, Vertices.ToArray(), SharpDX.Direct3D11.ResourceUsage.Default);
            IndexBuffer = Buffer.Index.New(GraphicsDevice, Indices.ToArray(), SharpDX.Direct3D11.ResourceUsage.Default);
        }
    }

    public struct ImportedGeometryInfo
    {
        public readonly static ImportedGeometryInfo Default = new ImportedGeometryInfo { Name = "Unnamed", Path = "", Scale = 1, FlipZ = true, FlipUV_V = true };

        public string Name { get; set; }
        public string Path { get; set; }
        public float Scale { get; set; }

        public bool SwapXY { get; set; }
        public bool SwapXZ { get; set; }
        public bool SwapYZ { get; set; }
        public bool FlipX { get; set; }
        public bool FlipY { get; set; }
        public bool FlipZ { get; set; }
        public bool FlipUV_V { get; set; }
        public bool InvertFaces { get; set; }
    }

    public class ImportedGeometry : ICloneable, IEquatable<ImportedGeometry>
    {
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();

        public class UniqueIDType { }

        public class Model : Model<ImportedGeometryMesh, ImportedGeometryVertex>
        {
            public float Scale { get; private set; }

            public Model(GraphicsDevice device, float scale)
                : base(device, ModelType.RoomGeometry)
            {
                Scale = scale;
            }

            public override void BuildBuffers() { }
        }

        public UniqueIDType UniqueID { get; } = new UniqueIDType();
        public Exception LoadException { get; private set; } = null;
        public ImportedGeometryInfo Info { get; private set; } = ImportedGeometryInfo.Default;
        public Model DirectXModel { get; private set; } = null;
        public List<ImportedGeometryTexture> Textures { get; private set; } = new List<ImportedGeometryTexture>();

        public void Update(LevelSettings settings, Dictionary<string, Texture> absolutePathTextureLookup, ImportedGeometryInfo info)
        {
            Info = info;
            LoadException = null;
            DirectXModel = null;
            Textures.Clear();

            try
            {
                string importedGeometryPath = settings.MakeAbsolute(info.Path);
                string importedGeometryDirectory = Path.GetDirectoryName(importedGeometryPath);

                // Invoke the TombLib geometry import code
                var settingsIO = new IOGeometrySettings
                {
                    Scale = info.Scale,
                    SwapXY = info.SwapXY,
                    SwapXZ = info.SwapXZ,
                    SwapYZ = info.SwapYZ,
                    FlipX = info.FlipX,
                    FlipY = info.FlipY,
                    FlipZ = info.FlipZ,
                    FlipUV_V = info.FlipUV_V,
                    InvertFaces = info.InvertFaces
                };
                var importer = new AssimpImporter(settingsIO, (absoluteTexturePath) =>
                {
                    return GetOrAddTexture(absolutePathTextureLookup, importedGeometryDirectory, absoluteTexturePath);
                });
                var tmpModel = importer.ImportFromFile(importedGeometryPath);

                // Create a new static model
                DirectXModel = new Model(DeviceManager.DefaultDeviceManager.Device, info.Scale);
                DirectXModel.BoundingBox = tmpModel.BoundingBox;

                // Loop for each mesh loaded in scene
                foreach (var mesh in tmpModel.Meshes)
                {
                    ImportedGeometryMesh modelMesh = new ImportedGeometryMesh(DeviceManager.DefaultDeviceManager.Device, "Imported");

                    var currentIndex = 0;
                    foreach (var tmpPoly in mesh.Polygons)
                    {
                        if (tmpPoly.Shape == IOPolygonShape.Quad)
                        {
                            for (var i = 0; i < 4; i++)
                            {
                                var vertex = new ImportedGeometryVertex();
                                vertex.Position = mesh.Positions[tmpPoly.Indices[i]];
                                vertex.UV = mesh.UV[tmpPoly.Indices[i]];
                                modelMesh.Vertices.Add(vertex);
                                modelMesh.Indices.Add(currentIndex);
                                currentIndex++;
                            }
                        }
                        else
                        {
                            for (var i = 0; i < 3; i++)
                            {
                                var vertex = new ImportedGeometryVertex();
                                vertex.Position = mesh.Positions[tmpPoly.Indices[i]];
                                vertex.UV = mesh.UV[tmpPoly.Indices[i]];
                                modelMesh.Vertices.Add(vertex);
                                modelMesh.Indices.Add(currentIndex);
                                currentIndex++;
                            }
                        }
                    }

                    modelMesh.Texture = mesh.Texture;
                    modelMesh.BuildBuffers();

                    // Add mesh to the model
                    DirectXModel.Meshes.Add(modelMesh);
                }
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (Exception exc)
            {
                LoadException = exc;
                DirectXModel = null;
                logger.Warn(exc, "Unable to load model \"" + info.Name + "\" from \"" + info.Path + "\" because an exception occurred during loading.");
            }
        }

        private Texture GetOrAddTexture(Dictionary<string, Texture> absolutePathTextureLookup, string importedGeometryDirectory, string texturePath)
        {
            if (string.IsNullOrEmpty(texturePath))
                return null;
            string absolutePath = Path.GetFullPath(Path.Combine(importedGeometryDirectory, texturePath));

            // Is this texture already loaded?
            {
                Texture texture;
                if (absolutePathTextureLookup.TryGetValue(absolutePath, out texture))
                {
                    // Make sure the texture is already listed under this object
                    var importedGeometryTexture = texture as ImportedGeometryTexture;
                    if ((importedGeometryTexture != null) && !Textures.Contains(importedGeometryTexture))
                        Textures.Add(importedGeometryTexture);

                    // Use texture
                    return texture;
                }
            }

            // Add a new imported geometry texture
            var newTexture = new ImportedGeometryTexture(absolutePath);
            Textures.Add(newTexture);
            absolutePathTextureLookup.Add(absolutePath, newTexture);
            return newTexture;
        }

        public void Assign(ImportedGeometry other)
        {
            LoadException = other.LoadException;
            Info = other.Info;
            DirectXModel = other.DirectXModel;
            Textures = other.Textures;
        }

        public ImportedGeometry Clone() => (ImportedGeometry)MemberwiseClone();
        object ICloneable.Clone() => Clone();
        public bool Equals(ImportedGeometry other) => base.Equals(other);
    }
}