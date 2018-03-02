using SharpDX.Toolkit.Graphics;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using TombLib.GeometryIO;
using TombLib.GeometryIO.Importers;
using TombLib.Graphics;
using TombLib.Utils;
using TombLib.Wad.Tr4Wad;
using TombLib.Wad.TrLevels;

namespace TombLib.Wad
{
    public class ComparerWadTextures : IComparer<WadTexture>
    {
        public int Compare(WadTexture x, WadTexture y)
        {
            if (x == null || y == null)
                return 0;

            return -x.Image.Height.CompareTo(y.Image.Height);
        }
    }

    public partial class Wad2 : IDisposable
    {
        public WadGameVersion SuggestedGameVersion { get; set; }
        public SortedList<WadMoveableId, WadMoveable> Moveables { get; set; } = new SortedList<WadMoveableId, WadMoveable>();
        public SortedList<WadStaticId, WadStatic> Statics { get; set; } = new SortedList<WadStaticId, WadStatic>();
        public SortedList<WadSpriteSequenceId, WadSpriteSequence> SpriteSequences { get; set; } = new SortedList<WadSpriteSequenceId, WadSpriteSequence>();
        public SortedList<WadFixedSoundInfoId, WadFixedSoundInfo> FixedSoundInfos { get; set; } = new SortedList<WadFixedSoundInfoId, WadFixedSoundInfo>();
        public string FileName { get; set; }

        // Data for rendering
        public GraphicsDevice GraphicsDevice { get; set; }
        public Texture2D DirectXTexture { get; private set; }
        public SortedDictionary<WadMoveableId, SkinnedModel> DirectXMoveables { get; } = new SortedDictionary<WadMoveableId, SkinnedModel>();
        public SortedDictionary<WadStaticId, StaticModel> DirectXStatics { get; } = new SortedDictionary<WadStaticId, StaticModel>();
        public List<WadTexture> PackedTextures { get; set; } = new List<WadTexture>();

        // Size of the atlas
        // DX10 requires minimum 8K textures support for hardware certification so we should be safe with this
        public const int TextureAtlasSize = 8192;

        public void Dispose()
        {
            DirectXTexture?.Dispose();
            DirectXTexture = null;

            foreach (var obj in DirectXMoveables.Values)
                obj.Dispose();
            DirectXMoveables.Clear();

            foreach (var obj in DirectXStatics.Values)
                obj.Dispose();
            DirectXStatics.Clear();
        }

        public HashSet<WadSoundInfo> SoundInfosUnique
        {
            get
            {
                var soundInfos = new HashSet<WadSoundInfo>();
                foreach (WadFixedSoundInfo fixedSoundInfo in FixedSoundInfos.Values)
                    soundInfos.Add(fixedSoundInfo.SoundInfo);
                foreach (WadMoveable moveable in Moveables.Values)
                    foreach (WadAnimation animation in moveable.Animations)
                        foreach (WadAnimCommand animCommand in animation.AnimCommands)
                            if (animCommand.Type == WadAnimCommandType.PlaySound)
                                soundInfos.Add(animCommand.SoundInfo);
                return soundInfos;
            }
        }

        public HashSet<WadTexture> MeshTexturesUnique
        {
            get
            {
                var textures = new HashSet<WadTexture>();
                foreach (WadMesh mesh in MeshesUnique)
                    foreach (WadPolygon polygon in mesh.Polys)
                        textures.Add((WadTexture)(polygon.Texture.Texture));
                return textures;
            }
        }

        public HashSet<WadMesh> MeshesUnique
        {
            get
            {
                var meshes = new HashSet<WadMesh>();
                foreach (WadMoveable moveable in Moveables.Values)
                    foreach (WadMesh mesh in moveable.Meshes)
                        meshes.Add(mesh);
                foreach (WadStatic @static in Statics.Values)
                    meshes.Add(@static.Mesh);
                return meshes;
            }
        }

        public WadSoundInfo TryGetSound(string soundName)
        {
            foreach (var soundInfo in SoundInfosUnique)
                if (soundInfo.Name.Equals(soundName, StringComparison.InvariantCultureIgnoreCase))
                    return soundInfo;
            return null;
        }

        public void RebuildTextureAtlas()
        {
            if (DirectXTexture != null)
                DirectXTexture.Dispose();

            // Pack the textures in a single atlas
            PackedTextures = new List<WadTexture>(MeshTexturesUnique);
            PackedTextures.Sort(new ComparerWadTextures());

            var packer = new RectPackerSimpleStack(new VectorInt2(TextureAtlasSize, TextureAtlasSize));

            foreach (var texture in PackedTextures)
            {
                texture.PositionInTextureAtlas = packer.TryAdd(texture.Image.Size).Value;
            }

            // Copy the page in a temp bitmap.
            // I generate a texture atlas, putting all texture pages inside 2048x2048 pixel textures.
            var tempBitmap = ImageC.CreateNew(TextureAtlasSize, TextureAtlasSize);

            foreach (var texture in PackedTextures)
            {
                int startX = (int)texture.PositionInTextureAtlas.X;
                int startY = (int)texture.PositionInTextureAtlas.Y;
                tempBitmap.CopyFrom(startX, startY, texture.Image);
            }

            // Create the DirectX texture atlas
            DirectXTexture = TextureLoad.Load(GraphicsDevice, tempBitmap);
        }

        public void PrepareDataForDirectX()
        {
            Dispose();

            // Rebuild the texture atlas and covert it to a DirectX texture
            RebuildTextureAtlas();

            var tempMoveables = new ConcurrentDictionary<WadMoveableId, SkinnedModel>();
            var tempStatics = new ConcurrentDictionary<WadStaticId, StaticModel>();

            // Create moveable models
            Parallel.For(0, Moveables.Count, i =>
            {
                var mov = Moveables.ElementAt(i).Value;
                tempMoveables.TryAdd(mov.Id, SkinnedModel.FromWad2(GraphicsDevice, this, mov, PackedTextures));
            });

            // Create static meshes
            Parallel.For(0, Statics.Count, i =>
            {
                var staticMesh = Statics.ElementAt(i).Value;
                tempStatics.TryAdd(staticMesh.Id, StaticModel.FromWad2(GraphicsDevice, this, staticMesh, PackedTextures));
            });

            foreach (var mov in tempMoveables)
                DirectXMoveables.Add(mov.Key, mov.Value);

            foreach (var stat in tempStatics)
                DirectXStatics.Add(stat.Key, stat.Value);
        }

        // This function will be used for showing all textures of Wad2 in a single map
        public ImageC GetPackedTextureMap()
        {
            // Pack the textures in a single atlas
            var packedTextures = new List<WadTexture>(MeshTexturesUnique);
            PackedTextures.Sort(new ComparerWadTextures());
            var factor = TextureAtlasSize / 512;
            var height = TextureAtlasSize * factor;
            var packer = new RectPackerSimpleStack(new VectorInt2(512, height));
            VectorInt2 maxSize = new VectorInt2();
            foreach (var texture in PackedTextures)
            {
                VectorInt2 positionInPackedTextureMap = packer.TryAdd(texture.Image.Size).Value;
                maxSize = VectorInt2.Max(maxSize, positionInPackedTextureMap + texture.Image.Size);
            }

            // Copy the page in a temp bitmap.
            var tempBitmap = ImageC.CreateNew(maxSize.X, maxSize.Y);

            foreach (var texture in PackedTextures)
            {
                int startX = (int)texture.PositionInTextureAtlas.X;
                int startY = (int)texture.PositionInTextureAtlas.Y;
                tempBitmap.CopyFrom(startX, startY, texture.Image);
            }

            return tempBitmap;
        }

        public WadStaticId GetFirstFreeStaticMesh()
        {
            for (int i = 0; i < Statics.Count; i++)
                if (!Statics.ContainsKey(new WadStaticId((uint)i)))
                    return new WadStaticId((uint)i);
            return new WadStaticId();
        }

        public static Wad2 ImportFromFile(string fileName, IEnumerable<string> oldWadSoundPaths, IDialogHandler progressReporter) // Last two parameters can be null.
        {
            if (fileName.EndsWith(".wad2", StringComparison.InvariantCultureIgnoreCase))
                return Wad2Loader.LoadFromFile(fileName);
            else if (fileName.EndsWith(".wad", StringComparison.InvariantCultureIgnoreCase))
            {
                var oldWad = new Tr4Wad.Tr4Wad();
                oldWad.LoadWad(fileName);
                return Tr4WadOperations.ConvertTr4Wad(oldWad, oldWadSoundPaths.ToList(), progressReporter);
            }
            else
            {
                var originalLevel = new TrLevel();
                originalLevel.LoadLevel(fileName);
                return TrLevelOperations.ConvertTrLevel(originalLevel);
            }
        }

        public List<WadMoveable> GetAllMoveablesReferencingSound(WadSoundInfo soundInfo)
        {
            var moveables = new List<WadMoveable>();
            foreach (var moveable in Moveables)
                foreach (var animation in moveable.Value.Animations)
                    foreach (var command in animation.AnimCommands)
                        if (command.Type == WadAnimCommandType.PlaySound)
                            if (command.SoundInfo == soundInfo)
                                if (!moveables.Contains(moveable.Value))
                                    moveables.Add(moveable.Value);
            return moveables;
        }

        public void CreateNewStaticMeshFromExternalModel(string fileName, IOGeometrySettings settings)
        {
            var staticMesh = new WadStatic(GetFirstFreeStaticMesh());
            staticMesh.Mesh = ImportWadMeshFromExternalModel(fileName, settings);
            staticMesh.VisibilityBox = staticMesh.Mesh.BoundingBox;
            staticMesh.CollisionBox = staticMesh.Mesh.BoundingBox;

            Statics.Add(staticMesh.Id, staticMesh);

            // Reload DirectX data
            PrepareDataForDirectX();
        }

        public WadMesh ImportWadMeshFromExternalModel(string fileName, IOGeometrySettings settings)
        {
            // Import the model
            var importer = new AssimpImporter(settings, (absoluteTexturePath) =>
            {
                return new WadTexture(ImageC.FromFile(absoluteTexturePath));
            });
            var tmpModel = importer.ImportFromFile(fileName);

            // Create a new mesh (all meshes from model will be joined)
            var mesh = new WadMesh();
            mesh.BoundingBox = tmpModel.BoundingBox;
            mesh.BoundingSphere = tmpModel.BoundingSphere;

            var lastBaseVertex = 0;
            foreach (var tmpMesh in tmpModel.Meshes)
            {
                mesh.VerticesPositions.AddRange(tmpMesh.Positions);
                foreach (var tmpSubmesh in tmpMesh.Submeshes)
                    foreach (var tmpPoly in tmpSubmesh.Value.Polygons)
                    {
                        if (tmpPoly.Shape == IOPolygonShape.Quad)
                        {
                            var poly = new WadPolygon { Shape = WadPolygonShape.Quad };
                            poly.Index0 = tmpPoly.Indices[0] + lastBaseVertex;
                            poly.Index1 = tmpPoly.Indices[1] + lastBaseVertex;
                            poly.Index2 = tmpPoly.Indices[2] + lastBaseVertex;
                            poly.Index3 = tmpPoly.Indices[3] + lastBaseVertex;

                            var area = new TextureArea();
                            area.TexCoord0 = tmpMesh.UV[tmpPoly.Indices[0]];
                            area.TexCoord1 = tmpMesh.UV[tmpPoly.Indices[1]];
                            area.TexCoord2 = tmpMesh.UV[tmpPoly.Indices[2]];
                            area.TexCoord3 = tmpMesh.UV[tmpPoly.Indices[3]];
                            area.Texture = tmpSubmesh.Value.Material.Texture;
                            poly.Texture = area;

                            mesh.Polys.Add(poly);
                        }
                        else
                        {
                            var poly = new WadPolygon { Shape = WadPolygonShape.Triangle };
                            poly.Index0 = tmpPoly.Indices[0] + lastBaseVertex;
                            poly.Index1 = tmpPoly.Indices[1] + lastBaseVertex;
                            poly.Index2 = tmpPoly.Indices[2] + lastBaseVertex;

                            var area = new TextureArea();
                            area.TexCoord0 = tmpMesh.UV[tmpPoly.Indices[0]];
                            area.TexCoord1 = tmpMesh.UV[tmpPoly.Indices[1]];
                            area.TexCoord2 = tmpMesh.UV[tmpPoly.Indices[2]];
                            area.Texture = tmpSubmesh.Value.Material.Texture;
                            poly.Texture = area;

                            mesh.Polys.Add(poly);
                        }
                    }

                lastBaseVertex = mesh.VerticesPositions.Count;
            }

            mesh.UpdateHash();
            return mesh;
        }

        public IWadObject TryGet(IWadObjectId wadObjectId)
        {
            if (wadObjectId is WadMoveableId)
                return Moveables.TryGetOrDefault((WadMoveableId)wadObjectId);
            else if (wadObjectId is WadStaticId)
                return Statics.TryGetOrDefault((WadStaticId)wadObjectId);
            else if (wadObjectId is WadSpriteSequenceId)
                return SpriteSequences.TryGetOrDefault((WadSpriteSequenceId)wadObjectId);
            else if (wadObjectId is WadFixedSoundInfoId)
                return FixedSoundInfos.TryGetOrDefault((WadFixedSoundInfoId)wadObjectId);
            else
                throw new ArgumentException("Argument not of a valid type.");

        }

        public void Remove(IWadObjectId wadObjectId)
        {
            if (wadObjectId is WadMoveableId)
                Moveables.Remove((WadMoveableId)wadObjectId);
            else if (wadObjectId is WadStaticId)
                Statics.Remove((WadStaticId)wadObjectId);
            else if (wadObjectId is WadSpriteSequenceId)
                SpriteSequences.Remove((WadSpriteSequenceId)wadObjectId);
            else if (wadObjectId is WadFixedSoundInfoId)
                FixedSoundInfos.Remove((WadFixedSoundInfoId)wadObjectId);
            else
                throw new ArgumentException("Argument not of a valid type.");
        }

        public void Add(IWadObjectId newId, IWadObject wadObject)
        {
            // Change id if necessary
            if (newId != wadObject.Id)
            {
                var property = wadObject.GetType().GetProperty("Id");
                property.SetValue(wadObject, newId);
            }

            // Add object
            if (newId is WadMoveableId)
                Moveables.Add((WadMoveableId)newId, (WadMoveable)wadObject);
            else if (newId is WadStaticId)
                Statics.Add((WadStaticId)newId, (WadStatic)wadObject);
            else if (newId is WadSpriteSequenceId)
                SpriteSequences.Add((WadSpriteSequenceId)newId, (WadSpriteSequence)wadObject);
            else if (newId is WadFixedSoundInfoId)
                FixedSoundInfos.Add((WadFixedSoundInfoId)newId, (WadFixedSoundInfo)wadObject);
            else
                throw new ArgumentException("Argument not of a valid type.");
        }

        public bool Contains(IWadObjectId wadObjectId)
        {
            return TryGet(wadObjectId) != null;
        }

        public void AssignNewId(IWadObjectId oldId, IWadObjectId newId)
        {
            if (Contains(newId))
                throw new ArgumentException("Id " + newId.ToString(SuggestedGameVersion) + " exists already.");
            IWadObject @object = TryGet(oldId);
            if (@object == null)
                throw new KeyNotFoundException("Id " + newId.ToString(SuggestedGameVersion) + " not found.");
            Remove(oldId);
            Add(newId, @object);
        }

        public static IReadOnlyList<FileFormat> WadFormatExtensions { get; } = new List<FileFormat>()
        {
            new FileFormat("Winroomedit WAD", "wad"),
            new FileFormat("TombEditor WAD2", "wad2"),
            new FileFormat("Tomb Raider I level", "phd"),
            new FileFormat("Tomb Raider II/III level", "tr2"),
            new FileFormat("Tomb Raider The Last Revelation level", "tr4"),
            new FileFormat("Tomb Raider Chronicles level", "trc")
        };
    }
}
