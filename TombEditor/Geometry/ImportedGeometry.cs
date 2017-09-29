using Assimp;
using NLog;
using SharpDX;
using SharpDX.Toolkit.Graphics;
using System;
using System.Collections.Generic;
using System.IO;
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

                float xMin = float.MaxValue;
                float yMin = float.MaxValue;
                float zMin = float.MaxValue;
                float xMax = float.MinValue;
                float yMax = float.MinValue;
                float zMax = float.MinValue;
                
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

                        if (v.Position.X < xMin)
                            xMin = v.Position.X;
                        if (v.Position.Y < yMin)
                            yMin = v.Position.Y;
                        if (v.Position.Z < zMin)
                            zMin = v.Position.Z;

                        if (v.Position.X > xMax)
                            xMax = v.Position.X;
                        if (v.Position.Y > yMax)
                            yMax = v.Position.Y;
                        if (v.Position.Z > zMax)
                            zMax = v.Position.Z;

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

                minVertex = new Vector3(xMin, yMin, zMin);
                maxVertex = new Vector3(xMax, yMax, zMax);

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

        public static string SupportedFormats
        {
            get
            {
                return "Any supported 3D model format|*.fbx;*.dae;*.gltf;*.glb;*.blend;*.3ds;*.ase;*.obj;.ifc;*.xgl;*.zgl;*.ply;*.dxf;*.lwo;*.lws;*.lxo;*.stl;*.x;*.ac;*.ms3d;*.cob;*.scn;*.ogex;*.x3d;*.3mf;*.bvh;*.csm;*.xml;*.irrmesh;*.*.irr;*.mdl;*.md2;*.md3;*.pk3;*.mdc;*.md5;*.smd;*.vta;*.ogex;*.3d;*.b3d;*.q3d;*.q3s;*.nff;*.nff;*.off;*.raw;*.ter;*.mdl;*.hmp;*.ndo|" +
                    "Autodesk (*.fbx)|*.fbx|" +
                    "Collada (*.dae)|*.dae|" +
                    "glTF (*.gltf, *.glb)|*.gltf;*.glb|" +
                    "Blender 3D (*.blend)|*.blend|" +
                    "3ds Max 3DS (*.3ds)|*.3ds|" +
                    "3ds Max ASE (*.ase)|*.ase|" +
                    "Wavefront Object (*.obj)|*.obj|" +
                    "Industry Foundation Classes (IFC / Step) (*.ifc)|*.ifc|" +
                    "XGL (*.xgl, *.zgl)|*.xgl;*.zgl|" +
                    "Stanford Polygon Library (*.ply)|*.ply|" +
                    "AutoCAD DXF (*.dxf)|*.dxf|" +
                    "LightWave (*.lwo)|*.lwo|" +
                    "LightWave Scene (*.lws)|*.lws|" +
                    "Modo (*.lxo)|*.lxo|" +
                    "Stereolithography (*.stl)|*.stl|" +
                    "DirectX X (*.x)|*.x|" +
                    "AC3D (*.ac)|*.ac|" +
                    "Milkshape 3D (*.ms3d)|*.ms3d|" +
                    "TrueSpace (*.cob, *.scn)|*.cob;*.scn|" +
                    "OpenGEX (*.ogex)|*.ogex|" +
                    "X3D (*.x3d)|*.x3d|" +
                    "3MF(*.3mf)|*.3mf|" +
                    "Biovision BVH (*.bvh)|*.bvh|" +
                    "CharacterStudio Motion (*.csm)|*.csm|" +
                    "Ogre XML (*.xml)|*.xml|" +
                    "Irrlicht Mesh (*.irrmesh)|*.irrmesh|" +
                    "Irrlicht Scene (*.irr)|*.irr|" +
                    "Quake I (*.mdl)|*.mdl|" +
                    "Quake II (*.md2)|*.md2|" +
                    "Quake III Mesh (*.md3)|*.md3|" +
                    "Quake III Map / BSP (*.pk3)|*.pk3|" +
                    "Return to Castle Wolfenstein (*.mdc)|*.mdc|" +
                    "Doom 3 (*.md5)|*.md5|" +
                    "Valve Model (*.smd, *.vta)|*.smd;*.vta|" +
                    "Open Game Engine Exchange (*.ogex)|*.ogex|" +
                    "Unreal(*.3d)|*.3d|" +
                    "BlitzBasic 3D (*.b3d)|*.b3d|" +
                    "Quick3D (*.q3d, *.q3s)|*.q3d;*.q3s|" +
                    "Neutral File Format (*.nff)|*.nff|" +
                    "Sense8 WorldToolKit (*.nff)|*.nff|" +
                    "Object File Format (*.off)|*.off|" +
                    "PovRAY Raw (*.raw)|*.raw|" +
                    "Terragen Terrain (*.ter)|*.ter|" +
                    "3D GameStudio(3DGS) (*.mdl)|*.mdl|" +
                    "3D GameStudio(3DGS) Terrain (*.hmp)|*.hmp|" +
                    "Izware Nendo (*.ndo)|*.ndo";
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