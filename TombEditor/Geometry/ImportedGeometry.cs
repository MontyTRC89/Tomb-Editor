using Assimp;
using NLog;
using SharpDX;
using SharpDX.Toolkit.Graphics;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
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

            VertexBuffer = Buffer.Vertex.New(GraphicsDevice, Vertices.ToArray(), SharpDX.Direct3D11.ResourceUsage.Default);
            IndexBuffer = Buffer.Index.New(GraphicsDevice, Indices.ToArray(), SharpDX.Direct3D11.ResourceUsage.Default);
        }
    }

    public struct ImportedGeometryInfo
    {
        public readonly static ImportedGeometryInfo Default = new ImportedGeometryInfo { Name = "Unnamed", Path = "", Scale = 1 };

        public string Name { get; set; }
        public string Path { get; set; }
        public float Scale { get; set; }
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
                // Use Assimp.NET for importing model
                AssimpContext context = new AssimpContext();
                string importedGeometryPath = settings.MakeAbsolute(info.Path);
                Scene scene = context.ImportFile(importedGeometryPath, PostProcessPreset.TargetRealTimeMaximumQuality);
                string importedGeometryDirectory = Path.GetDirectoryName(importedGeometryPath);

                // Create a new static model
                DirectXModel = new Model(DeviceManager.DefaultDeviceManager.Device, info.Scale);

                Vector3 minVertex = new Vector3(float.MaxValue);
                Vector3 maxVertex = new Vector3(float.MinValue);

                // Loop for each mesh loaded in scene
                foreach (var mesh in scene.Meshes)
                {
                    ImportedGeometryMesh modelMesh = new ImportedGeometryMesh(DeviceManager.DefaultDeviceManager.Device, "Imported");

                    //if mesh has a material extract the diffuse texture, if present
                    Assimp.Material material = scene.Materials[mesh.MaterialIndex];
                    if (material != null && material.HasTextureDiffuse)
                        modelMesh.Texture = GetOrAddTexture(absolutePathTextureLookup, importedGeometryDirectory, material.TextureDiffuse.FilePath);

                    // Determine primitive type (should be always triangle)
                    if (mesh.PrimitiveType != Assimp.PrimitiveType.Triangle)
                    {
                        logger.Info("The imported model contains " + mesh.PrimitiveType + " primitives! Everything except triangles is ignored.");
                        continue;
                    }

                    // Prepare DirectX data for this mesh
                    List<Vector3D> positions = mesh.Vertices;
                    List<Vector3D> texCoords = mesh.TextureCoordinateChannels[0];
                    bool hasTexCoords = mesh.HasTextureCoords(0);
                    for (int i = 0; i < mesh.VertexCount; i++)
                    {
                        ImportedGeometryVertex v = new ImportedGeometryVertex();

                        v.Position = new Vector3(positions[i].X, positions[i].Y, positions[i].Z) * info.Scale;
                        minVertex = Vector3.Min(minVertex, v.Position);
                        maxVertex = Vector3.Max(maxVertex, v.Position);

                        if (hasTexCoords)
                        {
                            v.UV = new Vector2(texCoords[i].X, 1.0f - texCoords[i].Y);
                            if (modelMesh.Texture?.IsAvailable ?? false)
                                v.UV *= modelMesh.Texture.Image.Size;
                        }

                        modelMesh.Vertices.Add(v);
                    }

                    modelMesh.Indices.AddRange(mesh.GetIndices());
                    modelMesh.BuildBuffers();

                    // Add mesh to the model
                    DirectXModel.Meshes.Add(modelMesh);
                }

                // Set the bounding box
                DirectXModel.BoundingBox = new BoundingBox(minVertex, maxVertex);
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

        private struct FileFormat
        {
            public string Description;
            public string Extension;
        }

        public static class SupportedFormats
        {
            private static readonly List<FileFormat> _supportedFormats = new List<FileFormat>()
            {
                new FileFormat() { Description = "Autodesk (*.fbx)", Extension = "fbx" },
                new FileFormat() { Description = "Collada (*.dae)", Extension = "dae" },
                new FileFormat() { Description = "glTF (*.gltf)", Extension = "gltf" },
                new FileFormat() { Description = "glTF (*.glb)", Extension = "glb" },
                new FileFormat() { Description = "Blender 3D (*.blend)", Extension = "blend" },
                new FileFormat() { Description = "3ds Max 3DS (*.3ds)", Extension = "3ds" },
                new FileFormat() { Description = "3ds Max ASE (*.ase)", Extension = "ase" },
                new FileFormat() { Description = "Wavefront Object (*.obj)", Extension = "obj" },
                new FileFormat() { Description = "Industry Foundation Classes (IFC / Step) (*.ifc)", Extension = "ifc" },
                new FileFormat() { Description = "XGL (*.xgl)", Extension = "xgl" },
                new FileFormat() { Description = "XGL ZGL (*.zgl)", Extension = "zgl" },
                new FileFormat() { Description = "Stanford Polygon Library (*.ply)", Extension = "ply" },
                new FileFormat() { Description = "AutoCAD DXF (*.dxf)", Extension = "dxf" },
                new FileFormat() { Description = "LightWave (*.lwo)", Extension = "lwo" },
                new FileFormat() { Description = "LightWave Scene (*.lws)", Extension = "lws" },
                new FileFormat() { Description = "Modo (*.lxo)", Extension = "lxo" },
                new FileFormat() { Description = "Stereolithography (*.stl)", Extension = "stl" },
                new FileFormat() { Description = "DirectX X (*.x)", Extension = "x" },
                new FileFormat() { Description = "AC3D (*.ac)", Extension = "ac" },
                new FileFormat() { Description = "Milkshape 3D (*.ms3d)", Extension = "ms3d" },
                new FileFormat() { Description = "TrueSpace COB (*.cob)", Extension = "cob" },
                new FileFormat() { Description = "TrueSpace SCN (*.scn)", Extension = "scn" },
                new FileFormat() { Description = "OpenGEX (*.ogex)", Extension = "ogex" },
                new FileFormat() { Description = "X3D (*.x3d)", Extension = "x3d" },
                new FileFormat() { Description = "3MF(*.3mf)", Extension = "3mf" },
                new FileFormat() { Description = "Biovision BVH (*.bvh)", Extension = "bvh" },
                new FileFormat() { Description = "CharacterStudio Motion (*.csm)", Extension = "csm" },
                new FileFormat() { Description = "Ogre XML (*.xml)", Extension = "xml" },
                new FileFormat() { Description = "Irrlicht Mesh (*.irrmesh)", Extension = "irrmesh" },
                new FileFormat() { Description = "Irrlicht Scene (*.irr)", Extension = "irr" },
                new FileFormat() { Description = "Quake I (*.mdl)", Extension = "mdl" },
                new FileFormat() { Description = "Quake II (*.md2)", Extension = "md2" },
                new FileFormat() { Description = "Quake III Mesh (*.md3)", Extension = "md3" },
                new FileFormat() { Description = "Quake III Map / BSP (*.pk3)", Extension = "pk3" },
                new FileFormat() { Description = "Return to Castle Wolfenstein (*.mdc)", Extension = "mdc" },
                new FileFormat() { Description = "Doom 3 (*.md5)", Extension = "md5" },
                new FileFormat() { Description = "Valve Model SMD (*.smd)", Extension = "smd" },
                new FileFormat() { Description = "Valve Model VTA (*.vta)", Extension = "vta" },
                new FileFormat() { Description = "Open Game Engine Exchange (*.ogex)", Extension = "ogex" },
                new FileFormat() { Description = "Unreal(*.3d)", Extension = "3d" },
                new FileFormat() { Description = "BlitzBasic 3D (*.b3d)", Extension = "b3d" },
                new FileFormat() { Description = "Quick3D (*.q3d)", Extension = "q3d" },
                new FileFormat() { Description = "Quick3D Q3S (*.q3s)", Extension = "q3s" },
                new FileFormat() { Description = "Neutral File Format (*.nff)", Extension = "nff" },
                new FileFormat() { Description = "Object File Format (*.off)", Extension = "off" },
                new FileFormat() { Description = "PovRAY Raw (*.raw)", Extension = "raw" },
                new FileFormat() { Description = "Terragen Terrain (*.ter)", Extension = "ter" },
                new FileFormat() { Description = "3D GameStudio(3DGS) (*.mdl)", Extension = "mdl" },
                new FileFormat() { Description = "3D GameStudio(3DGS) Terrain (*.hmp)", Extension = "hmp" },
                new FileFormat() { Description = "Izware Nendo (*.ndo)", Extension = "ndo" }
            };

            public static string GetFilter()
            {
                int numExtensions = _supportedFormats.Count;
                string result = "Any supported 3D model format|";

                foreach(var ext in _supportedFormats)
                {
                    result = result + "*." + ext.Extension;
                    if (ext.Equals(_supportedFormats.Last()))
                        result = result + "|";
                    else
                        result = result + ";";
                }

                foreach (var ext in _supportedFormats)
                {
                    result = result + ext.Description + "|*." + ext.Extension;
                    if (!ext.Equals(_supportedFormats.Last()))
                        result = result + "|";
                }

                return result;
            }

            public static bool IsExtensionPresent(string srcExtension)
            {
                return _supportedFormats.Exists(item => srcExtension.EndsWith(item.Extension));
            }
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

        public static bool AreListsEqual(List<ImportedGeometry> first, List<ImportedGeometry> second)
        {
            if (first.Count != second.Count)
                return false;

            for (int i = 0; i < first.Count; ++i)
                if (!first[i].Equals(second[i]))
                    return false;

            return true;
        }
    }
}