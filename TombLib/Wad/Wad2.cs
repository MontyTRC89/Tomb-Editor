using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TombLib.GeometryIO;
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

    public class Wad2
    {
        public WadGameVersion SuggestedGameVersion { get; set; } = WadGameVersion.TR4_TRNG;
        public SortedList<WadMoveableId, WadMoveable> Moveables { get; set; } = new SortedList<WadMoveableId, WadMoveable>();
        public SortedList<WadStaticId, WadStatic> Statics { get; set; } = new SortedList<WadStaticId, WadStatic>();
        public SortedList<WadSpriteSequenceId, WadSpriteSequence> SpriteSequences { get; set; } = new SortedList<WadSpriteSequenceId, WadSpriteSequence>();
        public SortedList<WadFixedSoundInfoId, WadFixedSoundInfo> FixedSoundInfos { get; set; } = new SortedList<WadFixedSoundInfoId, WadFixedSoundInfo>();
        public SortedList<WadAdditionalSoundInfoId, WadAdditionalSoundInfo> AdditionalSoundInfos { get; set; } = new SortedList<WadAdditionalSoundInfoId, WadAdditionalSoundInfo>();

        public string FileName { get; set; }

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
                foreach (var info in AdditionalSoundInfos.Values)
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
                        foreach (WadPolygon polygon in mesh.Polys)
                            textures.Add((WadTexture)polygon.Texture.Texture);
                foreach (var stat in Statics)
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

        public WadSoundInfo TryGetSound(string soundName)
        {
            foreach (var soundInfo in SoundInfosUnique)
            {
                Console.WriteLine(soundInfo.Name);
                if (soundInfo.Name.Equals(soundName, StringComparison.InvariantCultureIgnoreCase))
                    return soundInfo;
            }
            return null;
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
            else if (wadObjectId is WadAdditionalSoundInfoId)
                return AdditionalSoundInfos.TryGetOrDefault((WadAdditionalSoundInfoId)wadObjectId);
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
            else if (wadObjectId is WadAdditionalSoundInfoId)
                AdditionalSoundInfos.Remove((WadAdditionalSoundInfoId)wadObjectId);
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
            else if (newId is WadAdditionalSoundInfoId)
                AdditionalSoundInfos.Add((WadAdditionalSoundInfoId)newId, (WadAdditionalSoundInfo)wadObject);
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
