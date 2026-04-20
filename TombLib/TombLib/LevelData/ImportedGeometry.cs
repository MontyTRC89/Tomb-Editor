using NLog;
using SharpDX.Toolkit.Graphics;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Threading;
using TombLib.GeometryIO;
using TombLib.Graphics;
using TombLib.Utils;
using TombLib.Wad;
using Texture = TombLib.Utils.Texture;

namespace TombLib.LevelData
{
    public class ImportedGeometryTexture : Texture
    {
        public DataVersion Version { get; private set; } = DataVersion.GetNext();

        public ImportedGeometryTexture(string absolutePath)
        {
            AbsolutePath = absolutePath;
            Image = ImageC.FromFile(absolutePath);

            // Replace magenta with transparent color
            Image.ReplaceColor(new ColorC(255, 0, 255, 255), new ColorC(0, 0, 0, 0));
        }

        private ImportedGeometryTexture(ImportedGeometryTexture other)
        {
            Version = other.Version;
            AbsolutePath = other.AbsolutePath;
            Image = other.Image;
        }

        public void Assign(ImportedGeometryTexture other)
        {
            AbsolutePath = other.AbsolutePath;
            Image = other.Image;
            Version = DataVersion.GetNext();
        }

        public override Texture Clone() => new ImportedGeometryTexture(this);

        public override int GetHashCode() => AbsolutePath.GetHashCode();
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct ImportedGeometryVertex : IVertex
    {
        [VertexElement("POSITION", 0, SharpDX.DXGI.Format.R32G32B32_Float, 0)]
        public Vector3 Position;
        [VertexElement("TEXCOORD", 0, SharpDX.DXGI.Format.R32G32_Float, 12)]
        public Vector2 UV;
        [VertexElement("COLOR", 0, SharpDX.DXGI.Format.R32G32B32_Float, 20)]
        public Vector3 Color;
        [VertexElement("NORMAL", 0, SharpDX.DXGI.Format.R32G32B32_Float, 32)]
        public Vector3 Normal;

        Vector3 IVertex.Position => Position;
    }

    public class ImportedGeometryMesh
    {
        public string Name { get; }
        public bool HasVertexColors { get; set; }
        public List<ImportedGeometryVertex> Vertices { get; } = new List<ImportedGeometryVertex>();
        public List<int> Indices { get; } = new List<int>();
        public Dictionary<Material, Submesh> Submeshes { get; } = new Dictionary<Material, Submesh>();
        public BoundingBox BoundingBox { get; set; }

        public ImportedGeometryMesh(string name)
        {
            Name = name;
        }

        public void UpdateBoundingBox()
        {
            Vector3 minVertex = new Vector3(float.MaxValue);
            Vector3 maxVertex = new Vector3(float.MinValue);

            foreach (var vertex in Vertices)
            {
                minVertex = Vector3.Min(minVertex, vertex.Position);
                maxVertex = Vector3.Max(maxVertex, vertex.Position);
            }

            BoundingBox = new BoundingBox(minVertex, maxVertex);
        }

        public void DepthSort(Vector3? position)
        {
            int lastBaseIndex = 0;
            Indices.Clear();

            foreach (var submesh in Submeshes)
            {
                submesh.Value.BaseIndex = lastBaseIndex;
                if (submesh.Value.NumIndices != 0)
                {
                    var indexList = new List<int[]>();

                    for (int i = 0; i < submesh.Value.NumIndices; i += 3)
                    {
                        var tri = new[]
                        {
                            submesh.Value.Indices[i],
                            submesh.Value.Indices[i + 1],
                            submesh.Value.Indices[i + 2]
                        };
                        indexList.Add(tri);
                    }

                    if (position != null)
                        indexList = indexList.OrderByDescending(p => Vector3.Distance(position.Value,
                            (Vertices[p[0]].Position + Vertices[p[1]].Position + Vertices[p[2]].Position) / 3.0f)).ToList();

                    foreach (var tri in indexList)
                        Indices.AddRange(tri);
                }

                lastBaseIndex += submesh.Value.NumIndices;
            }
        }
    }

    public struct ImportedGeometryInfo
    {
        public static readonly ImportedGeometryInfo Default = new ImportedGeometryInfo { Name = "Unnamed", Path = "", Scale = 1, FlipZ = true, MappedUV = true, FlipUV_V = true };

        public string Name { get; set; }
        public string Path { get; set; }
        public float Scale { get; set; }

        public bool SwapXY { get; set; }
        public bool SwapXZ { get; set; }
        public bool SwapYZ { get; set; }
        public bool FlipX { get; set; }
        public bool FlipY { get; set; }
        public bool FlipZ { get; set; }
        public bool MappedUV { get; set; }
        public bool FlipUV_V { get; set; }
        public bool InvertFaces { get; set; }

        public ImportedGeometryInfo(string path, IOGeometrySettings settings)
        {
            Name = PathC.GetFileNameWithoutExtensionTry(path);
            Path = path;
            Scale = settings.Scale;
            SwapXY = settings.SwapXY;
            SwapXZ = settings.SwapXZ;
            SwapYZ = settings.SwapYZ;
            InvertFaces = settings.InvertFaces;
            FlipX = settings.FlipX;
            FlipY = settings.FlipY;
            FlipZ = settings.FlipZ;
            MappedUV = settings.MappedUV;
            FlipUV_V = settings.FlipUV_V;
        }
    }

    // ImportedGeometry is not actually IWadObject. This interface here is a hack against TRTombalization
    // unless whole item selection workflow is fully rewritten.

    public class ImportedGeometry : IWadObject, ICloneable, IReloadableResource, IEquatable<ImportedGeometry>
    {
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();

        public class UniqueIDType { }

        public class Model
        {
            public BoundingBox BoundingBox { get; set; }
            public List<ImportedGeometryMesh> Meshes { get; } = new List<ImportedGeometryMesh>();
            public List<Material> Materials { get; } = new List<Material>();
            public float Scale { get; }
            public DataVersion Version { get; private set; } = DataVersion.GetNext();

            public int TotalTriangles
            {
                get
                {
                    int numTriangles = 0;

                    foreach (var mesh in Meshes)
                        foreach (var submesh in mesh.Submeshes)
                            numTriangles += submesh.Value.Indices.Count / 3;

                    return numTriangles;
                }
            }

            public Model(float scale)
            {
                Scale = scale;
            }

            public void UpdateBuffers(Vector3? position = null)
            {
                var boundingBox = new BoundingBox();
                bool hasMesh = false;

                foreach (var mesh in Meshes)
                {
                    if (mesh.Vertices.Count == 0)
                        continue;

                    mesh.UpdateBoundingBox();
                    mesh.DepthSort(position);

                    boundingBox = hasMesh ? boundingBox.Union(mesh.BoundingBox) : mesh.BoundingBox;
                    hasMesh = true;
                }

                if (hasMesh)
                    BoundingBox = boundingBox;

                Version = DataVersion.GetNext();
            }
        }

        public UniqueIDType UniqueID { get; } = new UniqueIDType();
        public ImportedGeometryInfo Info { get; private set; } = ImportedGeometryInfo.Default;
        public Model DirectXModel { get; private set; }
        public List<ImportedGeometryTexture> Textures { get; private set; } = new List<ImportedGeometryTexture>();

        public IWadObjectId Id => null;
        public string ToString(TRVersion.Game gameVersion) => Info.Name;

        public ReloadableResourceType ResourceType { get { return ReloadableResourceType.ImportedGeometry; } }
        public Exception LoadException { get; set; }
        public IEnumerable<FileFormat> FileExtensions => BaseGeometryImporter.FileExtensions;
        public List<IReloadableResource> GetResourceList(LevelSettings settings) => settings.ImportedGeometries.Select(i => i as IReloadableResource).ToList();

        public string GetPath() => Info.Path;
        public void SetPath(LevelSettings settings, string path)
        {
            var newInfo = Info;
            newInfo.Path = path;
            settings.ImportedGeometryUpdate(this, newInfo);
        }

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
                    InvertFaces = info.InvertFaces,
                    UseVertexColor = true
                };

                BaseGeometryImporter importer = BaseGeometryImporter.CreateForFile(importedGeometryPath, settingsIO, absoluteTexturePath =>
                {
                    return GetOrAddTexture(absolutePathTextureLookup, importedGeometryDirectory, absoluteTexturePath);
                });
                var tmpModel = importer.ImportFromFile(importedGeometryPath);

                if (tmpModel.Materials.Count == 0)
                    throw new Exception("No valid materials found");
                if (tmpModel.Meshes.Count == 0)
                    throw new Exception("No valid mesh data found");

                if (SynchronizationContext.Current != null)
                    SynchronizationContext.Current.Post(unused => Update(tmpModel, info), null);
                else
                    Update(tmpModel, info);
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

        private bool Update(IOModel tmpModel, ImportedGeometryInfo info)
        {
            DirectXModel = new Model(info.Scale);
            DirectXModel.BoundingBox = tmpModel.BoundingBox;

            foreach (var tmpMaterial in tmpModel.Materials)
            {
                var material = new Material(tmpMaterial.Name)
                {
                    Texture = tmpMaterial.Texture,
                    AdditiveBlending = tmpMaterial.AdditiveBlending,
                    DoubleSided = tmpMaterial.DoubleSided
                };
                DirectXModel.Materials.Add(material);
            }

            foreach (var mesh in tmpModel.Meshes)
            {
                if (mesh.Normals.Count == 0)
                    mesh.CalculateNormals();

                var modelMesh = new ImportedGeometryMesh(mesh.Name)
                {
                    HasVertexColors = mesh.Colors.Count != 0
                };

                var currentIndex = 0;
                foreach (var tmpSubmesh in mesh.Submeshes)
                {
                    var material = DirectXModel.Materials[tmpModel.Materials.IndexOf(tmpSubmesh.Value.Material)];
                    var submesh = new Submesh(material);

                    foreach (var tmpPoly in tmpSubmesh.Value.Polygons)
                    {
                        if (tmpPoly.Shape == IOPolygonShape.Quad)
                        {
                            var vertexList = new List<ImportedGeometryVertex>();

                            for (var i = 0; i < 4; i++)
                                vertexList.Add(CreateVertex(mesh, tmpPoly.Indices[i]));

                            // HACK: Triangulate and disjoint quad faces for imported geometry, because otherwise another hack which joints
                            // disjointed vertices together will fail in Rooms.cs

                            submesh.Indices.Add(currentIndex);
                            submesh.Indices.Add(currentIndex + 1);
                            submesh.Indices.Add(currentIndex + 2);
                            submesh.Indices.Add(currentIndex + 3);
                            submesh.Indices.Add(currentIndex + 4);
                            submesh.Indices.Add(currentIndex + 5);

                            modelMesh.Vertices.Add(vertexList[0]);
                            modelMesh.Vertices.Add(vertexList[1]);
                            modelMesh.Vertices.Add(vertexList[2]);
                            modelMesh.Vertices.Add(vertexList[0]);
                            modelMesh.Vertices.Add(vertexList[2]);
                            modelMesh.Vertices.Add(vertexList[3]);

                            currentIndex += 6;
                        }
                        else
                        {
                            for (var i = 0; i < 3; i++)
                            {
                                modelMesh.Vertices.Add(CreateVertex(mesh, tmpPoly.Indices[i]));
                                submesh.Indices.Add(currentIndex);
                                currentIndex++;
                            }
                        }
                    }

                    modelMesh.Submeshes.Add(material, submesh);
                }

                DirectXModel.Meshes.Add(modelMesh);
            }

            DirectXModel.UpdateBuffers();

            return true;
        }

        private static ImportedGeometryVertex CreateVertex(IOMesh mesh, int index)
        {
            return new ImportedGeometryVertex
            {
                Position = mesh.Positions[index],
                Color = index < mesh.Colors.Count ? mesh.Colors[index].To3() : Vector3.One,
                UV = index < mesh.UV.Count ? mesh.UV[index] : Vector2.Zero,
                Normal = index < mesh.Normals.Count ? mesh.Normals[index] : Vector3.Zero
            };
        }

        private Texture GetOrAddTexture(Dictionary<string, Texture> absolutePathTextureLookup, string importedGeometryDirectory, string texturePath)
        {
            if (string.IsNullOrEmpty(texturePath))
                return null;
            string absolutePath = Path.GetFullPath(Path.Combine(importedGeometryDirectory, texturePath));

            {
                if (absolutePathTextureLookup.TryGetValue(absolutePath, out Texture texture))
                {
                    var importedGeometryTexture = texture as ImportedGeometryTexture;
                    if (importedGeometryTexture != null && !Textures.Contains(importedGeometryTexture))
                        Textures.Add(importedGeometryTexture);

                    return texture;
                }
            }

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

    public class ImportedGeometryComparer : IEqualityComparer<ImportedGeometry>
    {
        private LevelSettings _settings;

        public ImportedGeometryComparer(LevelSettings settings)
        {
            _settings = settings;
        }

        public bool Equals(ImportedGeometry x, ImportedGeometry y)
        {
            return (x.Info.FlipUV_V == y.Info.FlipUV_V &&
                    x.Info.FlipX == y.Info.FlipX &&
                    x.Info.FlipY == y.Info.FlipY &&
                    x.Info.FlipZ == y.Info.FlipZ &&
                    x.Info.InvertFaces == y.Info.InvertFaces &&
                    _settings.MakeAbsolute(x.Info.Name).Equals(_settings.MakeAbsolute(y.Info.Name)) &&
                    x.Info.Path == y.Info.Path &&
                    x.Info.Scale == y.Info.Scale &&
                    x.Info.SwapXY == y.Info.SwapXY &&
                    x.Info.SwapXZ == y.Info.SwapXZ &&
                    x.Info.SwapYZ == y.Info.SwapYZ);
        }

        public int GetHashCode(ImportedGeometry obj)
        {
            string info = obj.Info.FlipUV_V.ToString() + "|" +
                          obj.Info.FlipX.ToString() + "|" +
                          obj.Info.FlipY.ToString() + "|" +
                          obj.Info.FlipZ.ToString() + "|" +
                          obj.Info.InvertFaces.ToString() + "|" +
                          obj.Info.Name.ToString() + "|" +
                          obj.Info.Path.ToString() + "|" +
                          obj.Info.Scale.ToString() + "|" +
                          obj.Info.SwapXY.ToString() + "|" +
                          obj.Info.SwapXZ.ToString() + "|" +
                          obj.Info.SwapYZ.ToString();
            return info.GetHashCode();
        }
    }
}
