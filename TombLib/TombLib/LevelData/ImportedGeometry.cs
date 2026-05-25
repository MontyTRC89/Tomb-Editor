using NLog;
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
        public ImportedGeometryTexture(string absolutePath)
        {
            AbsolutePath = absolutePath;
            Image = ImageC.FromFile(absolutePath);

            // Replace magenta with transparent color
            Image.ReplaceColor(new ColorC(255, 0, 255, 255), new ColorC(0, 0, 0, 0));
        }

        private ImportedGeometryTexture(ImportedGeometryTexture other)
        {
            AbsolutePath = other.AbsolutePath;
            Image = other.Image;
        }

        public void Assign(ImportedGeometryTexture other)
        {
            AbsolutePath = other.AbsolutePath;
            Image = other.Image;
        }

        public override Texture Clone() => new ImportedGeometryTexture(this);

        public override int GetHashCode() => AbsolutePath.GetHashCode();
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct ImportedGeometryVertex
    {
        public Vector3 Position;
        public Vector2 UV;
        public Vector3 Color;
        public Vector3 Normal;
    }

    /// <summary>
    /// CPU-only mesh data for an <see cref="ImportedGeometry.Model"/>. The V2
    /// renderer reads <see cref="Vertices"/> + <see cref="Indices"/> +
    /// <see cref="Submeshes"/> directly when it builds its own atlas and
    /// vertex buffers.
    /// </summary>
    public class ImportedGeometryMesh
    {
        public string Name { get; set; }
        public List<ImportedGeometryVertex> Vertices { get; } = new List<ImportedGeometryVertex>();
        public List<int> Indices { get; } = new List<int>();
        public Dictionary<Material, Submesh> Submeshes { get; } = new Dictionary<Material, Submesh>();
        public BoundingBox BoundingBox { get; set; }
        public bool HasVertexColors { get; set; }

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

        /// <summary>
        /// Lays out submesh BaseIndex values + populates the flat
        /// <see cref="Indices"/> stream from each submesh's per-tri indices.
        /// Replaces the legacy GPU depth-sort path.
        /// </summary>
        public void RebuildIndices()
        {
            int lastBaseIndex = 0;
            Indices.Clear();
            foreach (var submesh in Submeshes)
            {
                submesh.Value.BaseIndex = lastBaseIndex;
                Indices.AddRange(submesh.Value.Indices);
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
            public DataVersion Version { get; set; } = DataVersion.GetNext();

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

            /// <summary>
            /// CPU-only equivalent of the legacy GPU-buffer rebuild — rebuilds
            /// each mesh's bounding box + index stream. The V2 renderer rebuilds
            /// its atlas/vertex buffers from this data on demand, so no GPU
            /// upload happens here.
            /// </summary>
            public void UpdateBuffers()
            {
                foreach (var mesh in Meshes)
                {
                    mesh.UpdateBoundingBox();
                    mesh.RebuildIndices();
                }
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
                    InvertFaces = info.InvertFaces,
                    UseVertexColor = true
                };

                BaseGeometryImporter importer = BaseGeometryImporter.CreateForFile(importedGeometryPath, settingsIO, absoluteTexturePath =>
                {
                    return GetOrAddTexture(absolutePathTextureLookup, importedGeometryDirectory, absoluteTexturePath);
                });
                var tmpModel = importer.ImportFromFile(importedGeometryPath);

                // Integrity checks
                if (tmpModel.Materials.Count == 0)
                    throw new Exception("No valid materials found");
                if (tmpModel.Meshes.Count == 0)
                    throw new Exception("No valid mesh data found");

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
            // Create a new static model (CPU-only data; the V2 renderer
            // builds GPU buffers from this on demand).
            DirectXModel = new Model(info.Scale);
            DirectXModel.BoundingBox = tmpModel.BoundingBox;

            // Create materials
            foreach (var tmpMaterial in tmpModel.Materials)
            {
                var material = new Material(tmpMaterial.Name);
                material.Texture = tmpMaterial.Texture;
                material.AdditiveBlending = tmpMaterial.AdditiveBlending;
                material.DoubleSided = tmpMaterial.DoubleSided;
                DirectXModel.Materials.Add(material);
            }

            // Loop for each mesh loaded in scene
            foreach (var mesh in tmpModel.Meshes)
            {
                // Make sure we always have correct normals
                if (mesh.Normals.Count == 0)
                    mesh.CalculateNormals();

                var modelMesh = new ImportedGeometryMesh(mesh.Name);

                modelMesh.HasVertexColors = (mesh.Colors.Count != 0);

                var currentIndex = 0;
                var currPoly = 0;
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
                            {
                                var vertex = new ImportedGeometryVertex();
                                vertex.Position = mesh.Positions[tmpPoly.Indices[i]];
                                vertex.Color = tmpPoly.Indices[i] < mesh.Colors.Count ? mesh.Colors[tmpPoly.Indices[i]].To3() : Vector3.One;
                                vertex.UV = tmpPoly.Indices[i] < mesh.UV.Count ? mesh.UV[tmpPoly.Indices[i]] : Vector2.Zero;
                                vertex.Normal = tmpPoly.Indices[i] < mesh.Normals.Count ? mesh.Normals[tmpPoly.Indices[i]] : Vector3.Zero;
                                vertexList.Add(vertex);
                            }

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
                                var vertex = new ImportedGeometryVertex();
                                vertex.Position = mesh.Positions[tmpPoly.Indices[i]];
                                vertex.Color = tmpPoly.Indices[i] < mesh.Colors.Count ? mesh.Colors[tmpPoly.Indices[i]].To3() : Vector3.One;
                                vertex.UV = tmpPoly.Indices[i] < mesh.UV.Count ? mesh.UV[tmpPoly.Indices[i]] : Vector2.Zero;
                                vertex.Normal = tmpPoly.Indices[i] < mesh.Normals.Count ? mesh.Normals[tmpPoly.Indices[i]] : Vector3.Zero;
                                modelMesh.Vertices.Add(vertex);
                                submesh.Indices.Add(currentIndex);
                                currentIndex++;
                            }
                        }

                        currPoly++;
                    }

                    modelMesh.Submeshes.Add(material, submesh);
                }

                DirectXModel.Meshes.Add(modelMesh);
            }

            DirectXModel.UpdateBuffers();

            return true;
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
                    if (importedGeometryTexture != null && !Textures.Contains(importedGeometryTexture))
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
            return (info.GetHashCode());
        }
    }
}
