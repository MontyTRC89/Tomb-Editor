﻿using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using TombLib.GeometryIO;
using TombLib.Graphics;
using TombLib.LevelData;
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

    public class Wad2
    {
        public bool HasUnknownData { get; set; } = false;
        public SoundSystem SoundSystem { get; set; }
        public TRVersion.Game GameVersion { get; set; } = TRVersion.Game.TR4;
        public SortedList<WadMoveableId, WadMoveable> Moveables { get; set; } = new SortedList<WadMoveableId, WadMoveable>();
        public SortedList<WadStaticId, WadStatic> Statics { get; set; } = new SortedList<WadStaticId, WadStatic>();
        public SortedList<WadSpriteSequenceId, WadSpriteSequence> SpriteSequences { get; set; } = new SortedList<WadSpriteSequenceId, WadSpriteSequence>();

        // DEPRECATED
        public SortedList<WadFixedSoundInfoId, WadFixedSoundInfo> FixedSoundInfosObsolete { get; set; } = new SortedList<WadFixedSoundInfoId, WadFixedSoundInfo>();
        // DEPRECATED
        public SortedList<WadAdditionalSoundInfoId, WadAdditionalSoundInfo> AdditionalSoundInfosObsolete { get; set; } = new SortedList<WadAdditionalSoundInfoId, WadAdditionalSoundInfo>();
        public Dictionary<long, WadSoundInfo> AllLoadedSoundInfos { get; set; } = new Dictionary<long, WadSoundInfo>();

        public WadSounds Sounds { get; set; }
        public string FileName { get; set; }

        public Wad2()
        {
            Sounds = new WadSounds();
        }

        public bool WadIsEmpty => String.IsNullOrEmpty(FileName) && Moveables.Count == 0 && Statics.Count == 0 && SpriteSequences.Count == 0;

        public HashSet<WadSoundInfo> SoundInfosUniqueObsolete
        {
            get
            {
                var soundInfos = new HashSet<WadSoundInfo>();
                foreach (WadFixedSoundInfo fixedSoundInfo in FixedSoundInfosObsolete.Values)
                    soundInfos.Add(fixedSoundInfo.SoundInfo);
                foreach (WadMoveable moveable in Moveables.Values)
                    foreach (WadAnimation animation in moveable.Animations)
                        foreach (WadAnimCommand animCommand in animation.AnimCommands)
                            if (animCommand.Type == WadAnimCommandType.PlaySound)
                                soundInfos.Add(animCommand.SoundInfoObsolete);
                foreach (var info in AdditionalSoundInfosObsolete.Values)
                    soundInfos.Add(info.SoundInfo);
                return soundInfos;
            }
        }

        public HashSet<WadTexture> MeshTexturesUnique
        {
            get
            {
                var textures = new HashSet<WadTexture>();
                foreach (var moveable in Moveables)
                    foreach (var mesh in moveable.Value.Meshes)
                        if (mesh != null)
                            foreach (WadPolygon polygon in mesh.Polys)
                                textures.Add((WadTexture)polygon.Texture.Texture);
                foreach (var stat in Statics)
                    if (stat.Value.Mesh != null)
                        foreach (WadPolygon polygon in stat.Value.Mesh.Polys)
                            textures.Add((WadTexture)polygon.Texture.Texture);
                return textures;
            }
        }

        public List<WadMesh> MeshesUnique
        {
            get
            {
                var meshes = new List<WadMesh>();
                foreach (WadMoveable moveable in Moveables.Values)
                    foreach (WadMesh mesh in moveable.Meshes)
                        meshes.Add(mesh);
                foreach (WadStatic @static in Statics.Values)
                    meshes.Add(@static.Mesh);
                return meshes;
            }
        }

        public WadStaticId GetFirstFreeStaticMesh()
        {
            for (int i = 0; i < Statics.Count; i++)
                if (!Statics.ContainsKey(new WadStaticId((uint)i)))
                    return new WadStaticId((uint)i);
            return new WadStaticId();
        }

        public static Wad2 ImportFromFile(string fileName, bool withSounds, IDialogHandler progressReporter, bool allowTRNGDecryption = false)
        {
            if (fileName.EndsWith(".wad2", StringComparison.InvariantCultureIgnoreCase))
                return Wad2Loader.LoadFromFile(fileName, withSounds);
            else if (fileName.EndsWith(".wad", StringComparison.InvariantCultureIgnoreCase) ||
                     fileName.EndsWith(".was", StringComparison.InvariantCultureIgnoreCase) ||
                     fileName.EndsWith(".sam", StringComparison.InvariantCultureIgnoreCase) ||
                     fileName.EndsWith(".sfx", StringComparison.InvariantCultureIgnoreCase) ||
                     fileName.EndsWith(".swd", StringComparison.InvariantCultureIgnoreCase))
            {
                if (!fileName.EndsWith(".wad", StringComparison.InvariantCultureIgnoreCase))
                    fileName = Path.ChangeExtension(fileName, "wad");

                var oldWad = new Tr4Wad.Tr4Wad();
                oldWad.LoadWad(fileName);
                return Tr4WadOperations.ConvertTr4Wad(oldWad, progressReporter);
            }
            else
            {
                var originalLevel = new TrLevel();
                originalLevel.LoadLevel(fileName, allowTRNGDecryption);
                return TrLevelOperations.ConvertTrLevel(originalLevel);
            }
        }

        public List<WadMoveable> GetAllMoveablesReferencingSound(int id)
        {
            var moveables = new List<WadMoveable>();
            foreach (var moveable in Moveables)
                foreach (var animation in moveable.Value.Animations)
                    foreach (var command in animation.AnimCommands)
                        if (command.Type == WadAnimCommandType.PlaySound)
                            if ((command.Parameter2 & 0x3FFF) == id)
                                if (!moveables.Contains(moveable.Value))
                                    moveables.Add(moveable.Value);
            return moveables;
        }

        public IWadObject TryGet(IWadObjectId wadObjectId)
        {
            if (wadObjectId is WadMoveableId)
                return Moveables.TryGetOrDefault((WadMoveableId)wadObjectId);
            else if (wadObjectId is WadStaticId)
                return Statics.TryGetOrDefault((WadStaticId)wadObjectId);
            else if (wadObjectId is WadSpriteSequenceId)
                return SpriteSequences.TryGetOrDefault((WadSpriteSequenceId)wadObjectId);
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
            else
                throw new ArgumentException("Argument not of a valid type.");
        }

        public bool Contains(IWadObjectId wadObjectId)
        {
            return TryGet(wadObjectId) != null;
        }

        public bool Contains(IWadObject wadObject)
        {
            if (wadObject is WadMoveable)
                return Moveables.Any(obj => obj.Value == (WadMoveable)wadObject);
            else if (wadObject is WadStatic)
                return Statics.Any(obj => obj.Value == (WadStatic)wadObject);
            else if (wadObject is WadSpriteSequence)
                return SpriteSequences.Any(obj => obj.Value == (WadSpriteSequence)wadObject);
            else
                throw new ArgumentException("Argument not of a valid type.");
        }

        public void AssignNewId(IWadObjectId oldId, IWadObjectId newId)
        {
            if (Contains(newId))
                throw new ArgumentException("Id " + newId.ToString(GameVersion) + " already exists.");
            IWadObject @object = TryGet(oldId);
            if (@object == null)
                throw new KeyNotFoundException("Id " + newId.ToString(GameVersion) + " not found.");
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

        public static List<WadTexture> PackTexturesForExport(Dictionary<Hash, WadTexture.AtlasReference> texturesToPack)
        {
            var textures = new List<WadTexture>();
            var scale = 256;

            var packer = new RectPackerTree(new TombLib.VectorInt2(scale, scale));
            var atlas = new WadTexture(ImageC.CreateNew(scale, scale));
            atlas.Image.Fill(new ColorC(0, 0, 0, 0));
            textures.Add(atlas);

            for (int i = 0; i < texturesToPack.Count; i++)
            {
                var textureRef = texturesToPack.ElementAt(i).Value;

                int paddingX = 4;
                if (textureRef.Texture.Image.Width + 2 * paddingX >= scale)
                    paddingX = (int)Math.Floor((float)(scale - textureRef.Texture.Image.Width) / 2);
                int paddingY = 4;
                if (textureRef.Texture.Image.Height + 2 * paddingY >= scale)
                    paddingY = (int)Math.Floor((float)(scale - textureRef.Texture.Image.Height) / 2);

                VectorInt2 size = textureRef.Texture.Image.Size + new VectorInt2(paddingX * 2, paddingY * 2);

                var result = packer.TryAdd(size);

                if (!result.HasValue)
                {
                    atlas = new WadTexture(ImageC.CreateNew(scale, scale));
                    atlas.Image.Fill(new ColorC(0, 0, 0, 0));
                    textures.Add(atlas);
                    packer = new RectPackerTree(new TombLib.VectorInt2(scale, scale));
                    result = packer.TryAdd(size);
                }

                // West
                for (int p = 0; p < paddingX; p++)
                    atlas.Image.CopyFrom(result.Value.X + p, result.Value.Y + paddingY, textureRef.Texture.Image, 0, 0, 1, textureRef.Texture.Image.Height);

                // East
                for (int p = 0; p < paddingX; p++)
                    atlas.Image.CopyFrom(result.Value.X + paddingX + textureRef.Texture.Image.Width + p, result.Value.Y + paddingY, textureRef.Texture.Image, textureRef.Texture.Image.Width - 1, 0, 1, textureRef.Texture.Image.Height);

                // North
                for (int p = 0; p < paddingY; p++)
                    atlas.Image.CopyFrom(result.Value.X + paddingX, result.Value.Y + p, textureRef.Texture.Image, 0, 0, textureRef.Texture.Image.Width, 1);

                // South
                for (int p = 0; p < paddingY; p++)
                    atlas.Image.CopyFrom(result.Value.X + paddingX, result.Value.Y + paddingY + textureRef.Texture.Image.Height + p, textureRef.Texture.Image, 0, textureRef.Texture.Image.Height - 1, textureRef.Texture.Image.Width, 1);

                // Corners
                var color = textureRef.Texture.Image.GetPixel(0, 0);
                for (int px = 0; px < paddingX; px++)
                    for (int py = 0; py < paddingY; py++)
                        atlas.Image.SetPixel(result.Value.X + px, result.Value.Y + py, color);

                color = textureRef.Texture.Image.GetPixel(textureRef.Texture.Image.Width - 1, 0);
                for (int px = 0; px < paddingX; px++)
                    for (int py = 0; py < paddingY; py++)
                        atlas.Image.SetPixel(result.Value.X + textureRef.Texture.Image.Width + paddingX + px, result.Value.Y + py, color);

                color = textureRef.Texture.Image.GetPixel(textureRef.Texture.Image.Width - 1, textureRef.Texture.Image.Height - 1);
                for (int px = 0; px < paddingX; px++)
                    for (int py = 0; py < paddingY; py++)
                        atlas.Image.SetPixel(result.Value.X + textureRef.Texture.Image.Width + paddingX + px, result.Value.Y + textureRef.Texture.Image.Height + paddingY + py, color);

                color = textureRef.Texture.Image.GetPixel(0, textureRef.Texture.Image.Height - 1);
                for (int px = 0; px < paddingX; px++)
                    for (int py = 0; py < paddingY; py++)
                        atlas.Image.SetPixel(result.Value.X + px, result.Value.Y + textureRef.Texture.Image.Height + paddingY + py, color);

                atlas.Image.CopyFrom(result.Value.X + paddingX, result.Value.Y + paddingY, textureRef.Texture.Image);

                textureRef.Position = new TombLib.VectorInt2(result.Value.X + paddingX, result.Value.Y + paddingY);
                textureRef.Atlas = textures.Count - 1;
            }

            return textures;
        }
    }
}
