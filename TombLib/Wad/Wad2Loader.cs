using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Reflection;
using TombLib.IO;
using TombLib.LevelData;
using TombLib.Utils;
using TombLib.Wad.Catalog;

namespace TombLib.Wad
{
    public static class Wad2Loader
    {
        public static Wad2 LoadFromFile(string fileName, bool withSounds)
        {
            Wad2 result;
            using (var fileStream = new FileStream(fileName, FileMode.Open, FileAccess.Read, FileShare.Read))
                result = LoadFromStream(fileStream);
            result.FileName = fileName;

            // Load additional XML file if it exists
            if (withSounds)
            {
                var xmlFile = Path.ChangeExtension(fileName, "xml");
                if (File.Exists(xmlFile))
                {
                    result.Sounds = WadSounds.ReadFromFile(xmlFile);
                }
            }

            return result;
        }

        public static Wad2 LoadFromStream(Stream stream)
        {
            byte[] magicNumber = new byte[4];
            stream.Read(magicNumber, 0, 4);
            stream.Seek(-4, SeekOrigin.Current);
            if (magicNumber.SequenceEqual(Wad2Chunks.MagicNumberObsolete))
            {
                // TODO In the long term it would be good to get rid of this obsolete code.
                using (var chunkIO = new ChunkReader(Wad2Chunks.MagicNumberObsolete, stream, Wad2Chunks.ChunkList))
                    return LoadWad2(chunkIO, true);
            }
            else
            {
                using (var chunkIO = new ChunkReader(Wad2Chunks.MagicNumber, stream, Wad2Chunks.ChunkList))
                    return LoadWad2(chunkIO, false);
            }
        }

        private static Wad2 LoadWad2(ChunkReader chunkIO, bool obsolete)
        {
            if (obsolete)
                LEB128.ReadUInt(chunkIO.Raw);
            var wad = new Wad2();

            Dictionary<long, WadTexture> textures = null;
            Dictionary<long, WadSample> samples = null;
            Dictionary<long, WadSoundInfo> soundInfos = null;
            Dictionary<long, WadSprite> sprites = null;

            wad.SoundSystem = SoundSystem.Dynamic;

            chunkIO.ReadChunks((id, chunkSize) =>
            {
                if (id == Wad2Chunks.GameVersion)
                {
                    wad.GameVersion = (TRVersion.Game)chunkIO.ReadChunkLong(chunkSize);
                    return true;
                }
                else if (id == Wad2Chunks.SoundSystem)
                {
                    wad.SoundSystem = (SoundSystem)chunkIO.ReadChunkLong(chunkSize);
                    return true;
                }
                else if (LoadTextures(chunkIO, id, wad, ref textures))
                    return true;
                else if (LoadSamples(chunkIO, id, wad, ref samples, obsolete))
                    return true;
                else if (LoadSoundInfos(chunkIO, id, wad, ref soundInfos, samples))
                    return true;
                else if (LoadFixedSoundInfos(chunkIO, id, wad, soundInfos))
                    return true;
                else if (LoadAdditionalSoundInfos(chunkIO, id, wad, soundInfos, samples))
                    return true;
                else if (LoadSprites(chunkIO, id, wad, ref sprites))
                    return true;
                else if (LoadSpriteSequences(chunkIO, id, wad, sprites))
                    return true;
                else if (LoadMoveables(chunkIO, id, wad, soundInfos, textures))
                    return true;
                else if (LoadStatics(chunkIO, id, wad, textures))
                    return true;
                return false;
            });

            if (obsolete)
                foreach (KeyValuePair<long, WadSoundInfo> soundInfo in soundInfos)
                    if (TrCatalog.IsSoundFixedByDefault(TRVersion.Game.TR4, checked((uint)soundInfo.Key)))
                    {
                        var Id = new WadFixedSoundInfoId(checked((uint)soundInfo.Key));
                        wad.FixedSoundInfosObsolete.Add(Id, new WadFixedSoundInfo(Id) { SoundInfo = soundInfo.Value });
                    }

            // XML_SOUND_SYSTEM: Used for conversion of Wad2 to new sound system
            wad.AllLoadedSoundInfos = soundInfos;

            // Force wad to be xml wad in case there's no sound infos at all
            if (wad.SoundSystem != SoundSystem.Xml && wad.AllLoadedSoundInfos?.Count == 0)
                wad.SoundSystem = SoundSystem.Xml;

            return wad;
        }

        private static bool LoadTextures(ChunkReader chunkIO, ChunkId idOuter, Wad2 wad, ref Dictionary<long, WadTexture> outTextures)
        {
            if (idOuter != Wad2Chunks.Textures)
                return false;

            Dictionary<long, WadTexture> textures = new Dictionary<long,  WadTexture>();
            long obsoleteIndex = 0; // Move this into each chunk once we got rid of old style *.wad2 files.

            chunkIO.ReadChunks((id, chunkSize) =>
            {
                if (id != Wad2Chunks.Texture)
                    return false;

                var width = LEB128.ReadInt(chunkIO.Raw);
                var height = LEB128.ReadInt(chunkIO.Raw);
                byte[] textureData = null;
                chunkIO.ReadChunks((id2, chunkSize2) =>
                {
                    if (id2 == Wad2Chunks.TextureIndex)
                        obsoleteIndex = chunkIO.ReadChunkLong(chunkSize2);
                    if (id2 == Wad2Chunks.TextureData)
                        textureData = chunkIO.ReadChunkArrayOfBytes(chunkSize2);
                    else
                        return false;
                    return true;
                });

                var texture = ImageC.FromByteArray(textureData, width, height);
                texture.ReplaceColor(new ColorC(255, 0, 255, 255), new ColorC(0, 0, 0, 0));

                textures.Add(obsoleteIndex++, new WadTexture(texture));
                return true;
            });

            outTextures = textures;
            return true;
        }

        private static bool LoadSamples(ChunkReader chunkIO, ChunkId idOuter, Wad2 wad, ref Dictionary<long, WadSample> outSamples, bool obsolete)
        {
            if (idOuter != Wad2Chunks.Samples)
                return false;

            var samples = new Dictionary<long, WadSample>();
            long obsoleteIndex = 0; // Move this into each chunk once we got rid of old style *.wad2 files.

            chunkIO.ReadChunks((id, chunkSize) =>
            {
                if (id != Wad2Chunks.Sample)
                    return false;

                string FilenameObsolete = null;
                byte[] data = null;

                chunkIO.ReadChunks((id2, chunkSize2) =>
                {
                    if (id2 == Wad2Chunks.SampleIndex)
                        obsoleteIndex = chunkIO.ReadChunkLong(chunkSize2);
                    else if (id2 == Wad2Chunks.SampleFilenameObsolete)
                        FilenameObsolete = chunkIO.ReadChunkString(chunkSize2);
                    else if (id2 == Wad2Chunks.SampleData)
                        data = chunkIO.ReadChunkArrayOfBytes(chunkSize2);
                    else
                        return false;
                    return true;
                });

                if (data == null && !string.IsNullOrEmpty(FilenameObsolete))
                {
                    string fullPath = Path.Combine(PathC.GetDirectoryNameTry(Assembly.GetEntryAssembly().Location), "Sounds\\TR4\\Samples", FilenameObsolete + ".wav");
                    data = File.ReadAllBytes(fullPath);
                }

                samples.Add(obsoleteIndex++, new WadSample("", WadSample.ConvertSampleFormat(data, 22050, 16)));
                return true;
            });

            outSamples = samples;
            return true;
        }

        private static bool LoadSoundInfo(ChunkReader chunkIO, Wad2 wad, Dictionary<long, WadSample> samples,
                                          out WadSoundInfo soundInfo, out long index)
        {
            var tempSoundInfo = new WadSoundInfo(0);
            long tempIndex = 0;
            float volume = 0;
            float chance = 0;
            float pitch = 0;
            float range = 0;

            chunkIO.ReadChunks((id2, chunkSize2) =>
            {
                // XML_SOUND_SYSTEM
                if (id2 == Wad2Chunks.SoundInfoIndex)
                    tempIndex = chunkIO.ReadChunkLong(chunkSize2);
                else if (id2 == Wad2Chunks.SoundInfoVolume)
                    volume = chunkIO.ReadChunkFloat(chunkSize2);
                else if (id2 == Wad2Chunks.SoundInfoRange)
                    range = chunkIO.ReadChunkFloat(chunkSize2);
                else if (id2 == Wad2Chunks.SoundInfoPitch)
                    pitch = chunkIO.ReadChunkFloat(chunkSize2);
                else if (id2 == Wad2Chunks.SoundInfoChance)
                    chance = chunkIO.ReadChunkFloat(chunkSize2);
                else if (id2 == Wad2Chunks.SoundInfoDisablePanning)
                    tempSoundInfo.DisablePanning = chunkIO.ReadChunkBool(chunkSize2);
                else if (id2 == Wad2Chunks.SoundInfoRandomizePitch)
                    tempSoundInfo.RandomizePitch = chunkIO.ReadChunkBool(chunkSize2);
                else if (id2 == Wad2Chunks.SoundInfoRandomizeVolume)
                    tempSoundInfo.RandomizeVolume = chunkIO.ReadChunkBool(chunkSize2);
                else if (id2 == Wad2Chunks.SoundInfoLoopBehaviour)
                    tempSoundInfo.LoopBehaviour = (WadSoundLoopBehaviour)(3 & chunkIO.ReadChunkByte(chunkSize2));
                else if (id2 == Wad2Chunks.SoundInfoName || id2 == Wad2Chunks.SoundInfoNameObsolete)
                    tempSoundInfo.Name = chunkIO.ReadChunkString(chunkSize2);
                else if (id2 == Wad2Chunks.SoundInfoSampleIndex)
                    tempSoundInfo.Samples.Add(samples[chunkIO.ReadChunkInt(chunkSize2)]); // Legacy
                else
                    return false;
                return true;
            });

            // Convert from floats to ints
            tempSoundInfo.Volume = (int)Math.Round(volume * 100.0f);
            tempSoundInfo.RangeInSectors = (int)range;
            tempSoundInfo.Chance = (int)Math.Round(chance * 100.0f);
            tempSoundInfo.PitchFactor = (int)Math.Round((pitch - 1.0f) * 100.0f);

            // Try to get the old ID
            tempSoundInfo.Id = TrCatalog.TryGetSoundInfoIdByDescription(wad.GameVersion, tempSoundInfo.Name);

            if (string.IsNullOrWhiteSpace(tempSoundInfo.Name))
                tempSoundInfo.Name = TrCatalog.GetOriginalSoundName(wad.GameVersion, unchecked((uint)tempIndex));

            index = tempIndex;
            soundInfo = tempSoundInfo;

            return true;
        }

        private static bool LoadSoundInfos(ChunkReader chunkIO, ChunkId idOuter, Wad2 wad, ref Dictionary<long, WadSoundInfo> outSoundInfos, Dictionary<long, WadSample> samples)
        {
            if (idOuter != Wad2Chunks.SoundInfos)
                return false;

            var soundInfos = new Dictionary<long, WadSoundInfo>();
            chunkIO.ReadChunks((id, chunkSize) =>
            {
                if (id != Wad2Chunks.SoundInfo)
                    return false;

                WadSoundInfo soundInfo;
                long index;
                LoadSoundInfo(chunkIO, wad, samples, out soundInfo, out index);
                soundInfos.Add(index, soundInfo);

                return true;
            });

            outSoundInfos = soundInfos;
            return true;
        }

        private static bool LoadFixedSoundInfos(ChunkReader chunkIO, ChunkId idOuter, Wad2 wad, Dictionary<long, WadSoundInfo> soundInfos)
        {
            if (idOuter != Wad2Chunks.FixedSoundInfos)
                return false;

            var fixedSoundInfos = new SortedList<WadFixedSoundInfoId, WadFixedSoundInfo>();
            chunkIO.ReadChunks((id, chunkSize) =>
            {
                if (id != Wad2Chunks.FixedSoundInfo)
                    return false;
                int soundId = -1;
                int SoundInfoId = -1;
                chunkIO.ReadChunks((id2, chunkSize2) =>
                {
                    if (id2 == Wad2Chunks.FixedSoundInfoId)
                        soundId = chunkIO.ReadChunkInt(chunkSize2);
                    else if (id2 == Wad2Chunks.FixedSoundInfoSoundInfoId)
                        SoundInfoId = chunkIO.ReadChunkInt(chunkSize2);
                    else
                        return false;
                    return true;
                });
                if (soundId == -1 || SoundInfoId == -1)
                    throw new Exception("Invalid fixed sound info.");

                var Id = new WadFixedSoundInfoId(checked((uint)soundId));
                fixedSoundInfos.Add(Id, new WadFixedSoundInfo(Id) { SoundInfo = soundInfos[SoundInfoId] });
                return true;
            });

            wad.FixedSoundInfosObsolete = fixedSoundInfos;
            return true;
        }

        private static bool LoadAdditionalSoundInfos(ChunkReader chunkIO, ChunkId idOuter, Wad2 wad, Dictionary<long, WadSoundInfo> soundInfos, Dictionary<long, WadSample> samples)
        {
            if (idOuter == Wad2Chunks.AdditionalSoundInfosObsolete)
            {
                chunkIO.ReadChunks((id, chunkSize) =>
                {
                    if (id != Wad2Chunks.SoundInfo)
                        return false;

                    WadSoundInfo soundInfo;
                    long index;
                    LoadSoundInfo(chunkIO, wad, samples, out soundInfo, out index);
                    var wId = new WadAdditionalSoundInfoId("Unnamed " + soundInfo.Name);
                    wad.AdditionalSoundInfosObsolete.Add(wId, new WadAdditionalSoundInfo(wId) { SoundInfo = soundInfo });

                    return true;
                });
                return true;
            }
            else if (idOuter == Wad2Chunks.AdditionalSoundInfos)
            {
                var additionalSoundInfos = new SortedList<WadAdditionalSoundInfoId, WadAdditionalSoundInfo>();
                chunkIO.ReadChunks((id, chunkSize) =>
                {
                    if (id != Wad2Chunks.AdditionalSoundInfo)
                        return false;
                    string soundName = null;
                    int SoundInfoId = -1;
                    chunkIO.ReadChunks((id2, chunkSize2) =>
                    {
                        if (id2 == Wad2Chunks.AdditionalSoundInfoName)
                            soundName = chunkIO.ReadChunkString(chunkSize2);
                        else if (id2 == Wad2Chunks.AdditionalSoundInfoSoundInfoId)
                            SoundInfoId = chunkIO.ReadChunkInt(chunkSize2);
                        else
                            return false;
                        return true;
                    });

                    var Id = new WadAdditionalSoundInfoId(soundName);
                    additionalSoundInfos.Add(Id, new WadAdditionalSoundInfo(Id) { SoundInfo = soundInfos[SoundInfoId] });
                    return true;
                });

                wad.AdditionalSoundInfosObsolete = additionalSoundInfos;
                return true;
            }
            return false;
        }

        private static WadMesh LoadMesh(ChunkReader chunkIO, long chunkSize, Dictionary<long, WadTexture> textures)
        {
            var mesh = new WadMesh();
            long obsoleteIndex = 0;

            chunkIO.ReadChunks((id2, chunkSize2) =>
            {
                if (id2 == Wad2Chunks.MeshIndex)
                    obsoleteIndex = chunkIO.ReadChunkLong(chunkSize2);
                else if (id2 == Wad2Chunks.MeshName)
                    mesh.Name = chunkIO.ReadChunkString(chunkSize2);
                else if (id2 == Wad2Chunks.MeshSphere)
                {
                    // Read bounding sphere
                    float radius = 0;
                    Vector3 center = Vector3.Zero;
                    chunkIO.ReadChunks((id3, chunkSize3) =>
                    {
                        if (id3 == Wad2Chunks.MeshSphereCenter)
                            center = chunkIO.ReadChunkVector3(chunkSize3);
                        else if (id3 == Wad2Chunks.MeshSphereRadius)
                            radius = chunkIO.ReadChunkFloat(chunkSize3);
                        else
                            return false;
                        return true;
                    });
                    mesh.BoundingSphere = new BoundingSphere(center, radius);
                }
                else if (id2 == Wad2Chunks.MeshBoundingBox)
                {
                    // Read bounding box
                    Vector3 min = Vector3.Zero;
                    Vector3 max = Vector3.Zero;
                    chunkIO.ReadChunks((id3, chunkSize3) =>
                    {
                        if (id3 == Wad2Chunks.MeshBoundingBoxMin)
                            min = chunkIO.ReadChunkVector3(chunkSize3);
                        else if (id3 == Wad2Chunks.MeshBoundingBoxMax)
                            max = chunkIO.ReadChunkVector3(chunkSize3);
                        else
                            return false;
                        return true;
                    });
                    mesh.BoundingBox = new BoundingBox(min, max);
                }
                else if (id2 == Wad2Chunks.MeshVertexPositions)
                {
                    chunkIO.ReadChunks((id3, chunkSize3) =>
                    {
                        if (id3 == Wad2Chunks.MeshVertexPosition)
                            mesh.VerticesPositions.Add(chunkIO.ReadChunkVector3(chunkSize3));
                        else
                            return false;
                        return true;
                    });
                }
                else if (id2 == Wad2Chunks.MeshVertexNormals)
                {
                    chunkIO.ReadChunks((id3, chunkSize3) =>
                    {
                        if (id3 == Wad2Chunks.MeshVertexNormal)
                            mesh.VerticesNormals.Add(chunkIO.ReadChunkVector3(chunkSize3));
                        else
                            return false;
                        return true;
                    });
                }
                else if (id2 == Wad2Chunks.MeshVertexShades)
                {
                    chunkIO.ReadChunks((id3, chunkSize3) =>
                    {
                        if (id3 == Wad2Chunks.MeshVertexShade)
                            mesh.VerticesShades.Add(chunkIO.ReadChunkShort(chunkSize3));
                        else
                            return false;
                        return true;
                    });
                }
                else if (id2 == Wad2Chunks.MeshPolygons)
                {
                    chunkIO.ReadChunks((id3, chunkSize3) =>
                    {
                        if (id3 == Wad2Chunks.MeshQuad ||
                            id3 == Wad2Chunks.MeshTriangle)
                        {
                            var polygon = new WadPolygon();
                            polygon.Shape = id3 == Wad2Chunks.MeshQuad ? WadPolygonShape.Quad : WadPolygonShape.Triangle;
                            polygon.Index0 = LEB128.ReadInt(chunkIO.Raw);
                            polygon.Index1 = LEB128.ReadInt(chunkIO.Raw);
                            polygon.Index2 = LEB128.ReadInt(chunkIO.Raw);
                            if (id3 == Wad2Chunks.MeshQuad)
                                polygon.Index3 = LEB128.ReadInt(chunkIO.Raw);
                            polygon.ShineStrength = LEB128.ReadByte(chunkIO.Raw);

                            TextureArea textureArea = new TextureArea();
                            textureArea.Texture = textures[LEB128.ReadInt(chunkIO.Raw)];
                            textureArea.TexCoord0 = chunkIO.Raw.ReadVector2();
                            textureArea.TexCoord1 = chunkIO.Raw.ReadVector2();
                            textureArea.TexCoord2 = chunkIO.Raw.ReadVector2();
                            if (id3 == Wad2Chunks.MeshQuad)
                                textureArea.TexCoord3 = chunkIO.Raw.ReadVector2();
                            else
                                textureArea.TexCoord3 = textureArea.TexCoord2;
                            textureArea.BlendMode = (BlendMode)LEB128.ReadLong(chunkIO.Raw);
                            textureArea.DoubleSided = chunkIO.Raw.ReadBoolean();
                            polygon.Texture = textureArea;

                            chunkIO.ReadChunks((id4, chunkSize4) =>
                            {
                                return false;
                            });

                            mesh.Polys.Add(polygon);
                        }
                        else
                        {
                            return false;
                        }
                        return true;
                    });
                }
                else
                {
                    return false;
                }

                return true;
            });

            return mesh;
        }

        private static bool LoadSprites(ChunkReader chunkIO, ChunkId idOuter, Wad2 wad, ref Dictionary<long, WadSprite> outSprites)
        {
            if (idOuter != Wad2Chunks.Sprites)
                return false;

            var sprites = new Dictionary<long, WadSprite>();
            long obsoleteIndex = 0; // Move this into each chunk once we got rid of old style *.wad2 files.
            chunkIO.ReadChunks((id, chunkSize) =>
            {
                if (id != Wad2Chunks.Sprite)
                    return false;

                int width = LEB128.ReadInt(chunkIO.Raw);
                int height = LEB128.ReadInt(chunkIO.Raw);
                byte[] imageData = null;
                RectangleInt2 rect = new RectangleInt2();

                chunkIO.ReadChunks((id2, chunkSize2) =>
                {
                    if (id2 == Wad2Chunks.SpriteIndex)
                        obsoleteIndex = chunkIO.ReadChunkLong(chunkSize2);
                    else if (id2 == Wad2Chunks.SpriteData)
                        imageData = chunkIO.ReadChunkArrayOfBytes(chunkSize2);
                    else if (id2 == Wad2Chunks.SpriteSides) {
                        rect.X0 = chunkIO.Raw.ReadInt32();
                        rect.Y0 = chunkIO.Raw.ReadInt32();
                        rect.X1 = chunkIO.Raw.ReadInt32();
                        rect.Y1 = chunkIO.Raw.ReadInt32();
                    }else return false;
                    return true;
                });

                sprites.Add(obsoleteIndex++, new WadSprite {
                    Texture = new WadTexture(ImageC.FromByteArray(imageData, width, height)),
                    Alignment = rect
                })
                ;
                return true;
            });

            outSprites = sprites;
            return true;
        }

        private static bool LoadSpriteSequences(ChunkReader chunkIO, ChunkId idOuter, Wad2 wad, Dictionary<long, WadSprite> sprites)
        {
            if (idOuter != Wad2Chunks.SpriteSequences)
                return false;

            var spriteSequences = new SortedList<WadSpriteSequenceId, WadSpriteSequence>();
            chunkIO.ReadChunks((id, chunkSize) =>
            {
                if (id != Wad2Chunks.SpriteSequence)
                    return false;

                var s = new WadSpriteSequence(new WadSpriteSequenceId(LEB128.ReadUInt(chunkIO.Raw)));
                chunkIO.ReadChunks((id2, chunkSize2) =>
                {
                    if (id2 == Wad2Chunks.SpriteSequenceSpriteIndex)
                        s.Sprites.Add(sprites[chunkIO.ReadChunkInt(chunkSize2)]);
                    else
                        return false;
                    return true;
                });

                spriteSequences.Add(s.Id, s);
                return true;
            });

            wad.SpriteSequences = spriteSequences;
            return true;
        }

        private static WadBone LoadBone(ChunkReader chunkIO, WadMoveable mov, List<WadMesh> meshes)
        {
            WadBone bone = new WadBone();

            chunkIO.ReadChunks((id, chunkSize) =>
            {
                if (id == Wad2Chunks.MoveableBoneName)
                {
                    bone.Name = chunkIO.ReadChunkString(chunkSize);
                    //Console.WriteLine("Processing " + bone.Name);
                }
                else if (id == Wad2Chunks.MoveableBoneTranslation)
                    bone.Translation = chunkIO.ReadChunkVector3(chunkSize);
                else if (id == Wad2Chunks.MoveableBoneMeshPointer)
                    bone.Mesh = meshes[chunkIO.ReadChunkInt(chunkSize)];
                else if (id == Wad2Chunks.MoveableBone)
                    bone.Children.Add(LoadBone(chunkIO, mov, meshes));
                else
                    return false;
                return true;
            });

            foreach (var childBone in bone.Children)
                childBone.Parent = bone;

            return bone;
        }

        private static void BuildNewMeshTree(WadBone bone, List<WadBone> meshTrees)
        {
            var newBone = new WadBone();
            newBone.Translation = bone.Translation;
            newBone.Mesh = bone.Mesh;
            newBone.Name = bone.Name;

            if (bone.Parent == null)
                newBone.OpCode = WadLinkOpcode.Push;
            else
            {
                if (bone.Parent.Children.Count == 1)
                    newBone.OpCode = WadLinkOpcode.NotUseStack;
                else
                {
                    int childrenCount = bone.Parent.Children.Count;
                    if (bone.Parent.Children.IndexOf(bone) == 0)
                        newBone.OpCode = WadLinkOpcode.Push;
                    else if (bone.Parent.Children.IndexOf(bone) == childrenCount - 1)
                        newBone.OpCode = WadLinkOpcode.Pop;
                    else
                        newBone.OpCode = WadLinkOpcode.Read;
                }
            }

            if (bone.Parent != null)
                meshTrees.Add(newBone);

            for (int i = 0; i < bone.Children.Count; i++)
                BuildNewMeshTree(bone.Children[i], meshTrees);
        }

        private static bool LoadMoveables(ChunkReader chunkIO, ChunkId idOuter, Wad2 wad,
                                          Dictionary<long, WadSoundInfo> soundInfos, 
                                          Dictionary<long, WadTexture> textures)
        {
            if (idOuter != Wad2Chunks.Moveables)
                return false;

            chunkIO.ReadChunks((id, chunkSize) =>
            {
                if (id != Wad2Chunks.Moveable)
                    return false;

                uint objTypeId = LEB128.ReadUInt(chunkIO.Raw);
                var mov = new WadMoveable(new WadMoveableId(objTypeId));
                var meshes = new List<WadMesh>();
                chunkIO.ReadChunks((id2, chunkSize2) =>
                {
                    if (id2 == Wad2Chunks.Mesh)
                    {
                        var mesh = LoadMesh(chunkIO, chunkSize2, textures);
                        meshes.Add(mesh);
                    }
                    else if (id2 == Wad2Chunks.MoveableBone)
                    {
                        var skeleton = LoadBone(chunkIO, mov, meshes);

                        // Now convert the skeleton in the new (?) format. Ugly system but this is required for 
                        // setting exact hardcoded ID for some moveables (i.e. gun mesh of an enemy, the engine 
                        // has hardcoded mesh indices for effects)
                        var bones = new List<WadBone>();

                        var root = new WadBone();
                        root.Name = skeleton.Name;
                        root.Translation = Vector3.Zero;
                        root.Mesh = skeleton.Mesh;
                        bones.Add(root);

                        BuildNewMeshTree(skeleton, bones);
                        mov.Bones.AddRange(bones);
                    }
                    else if (id2 == Wad2Chunks.MoveableBoneNew)
                    {
                        var bone = new WadBone();

                        bone.OpCode = (WadLinkOpcode)LEB128.ReadByte(chunkIO.Raw);
                        bone.Name = chunkIO.Raw.ReadStringUTF8();

                        chunkIO.ReadChunks((id3, chunkSize3) =>
                        {                            
                            if (id3 == Wad2Chunks.MoveableBoneTranslation)
                                bone.Translation = chunkIO.ReadChunkVector3(chunkSize);
                            else if (id3 == Wad2Chunks.MoveableBoneMeshPointer)
                                bone.Mesh = meshes[chunkIO.ReadChunkInt(chunkSize)];
                            else
                                return false;
                            return true;
                        });

                        mov.Bones.Add(bone);
                    }
                    else if (id2 == Wad2Chunks.AnimationObsolete || 
                             id2 == Wad2Chunks.Animation ||
                             id2 == Wad2Chunks.Animation2)
                    {
                        var animation = new WadAnimation();

                        animation.StateId = LEB128.ReadUShort(chunkIO.Raw);
                        animation.EndFrame = LEB128.ReadUShort(chunkIO.Raw);
                        animation.FrameRate = LEB128.ReadByte(chunkIO.Raw);

                        if (id2 == Wad2Chunks.AnimationObsolete)
                        {
                            LEB128.ReadUShort(chunkIO.Raw);
                            LEB128.ReadUShort(chunkIO.Raw);
                        }

                        int oldSpeed, oldAccel, oldLatSpeed, oldLatAccel;
                        oldSpeed = oldAccel = oldLatSpeed = oldLatAccel = 0;

                        if (id2 != Wad2Chunks.Animation2)
                        {
                            // Use old speeds/accels for legacy chunk versions
                            oldSpeed    = LEB128.ReadInt(chunkIO.Raw);
                            oldAccel    = LEB128.ReadInt(chunkIO.Raw);
                            oldLatSpeed = LEB128.ReadInt(chunkIO.Raw);
                            oldLatAccel = LEB128.ReadInt(chunkIO.Raw);

                            // Correct EndFrame for legacy chunk versions
                            if (animation.EndFrame > 0)
                                animation.EndFrame--;
                        }

                        // Fix possibly corrupted EndFrame value which was caused by bug introduced in 1.2.9
                        if (animation.EndFrame == ushort.MaxValue)
                            animation.EndFrame = 0;

                        animation.NextAnimation = LEB128.ReadUShort(chunkIO.Raw);
                        animation.NextFrame = LEB128.ReadUShort(chunkIO.Raw);

                        bool foundNewVelocitiesChunk = false;
                        chunkIO.ReadChunks((id3, chunkSize3) =>
                        {
                            if (id3 == Wad2Chunks.AnimationName)
                            {
                                animation.Name = chunkIO.ReadChunkString(chunkSize3);
                            }
                            else if (id3 == Wad2Chunks.AnimationVelocities)
                            {
                                foundNewVelocitiesChunk = true;
                                var velocities = chunkIO.ReadChunkVector4(chunkSize);
                                animation.StartVelocity = velocities.X;
                                animation.EndVelocity = velocities.Y;
                                animation.StartLateralVelocity = velocities.Z;
                                animation.EndLateralVelocity = velocities.W;
                            }
                            else if (id3 == Wad2Chunks.KeyFrame)
                            {
                                var keyframe = new WadKeyFrame();
                                chunkIO.ReadChunks((id4, chunkSize4) =>
                                {
                                    if (id4 == Wad2Chunks.KeyFrameOffset)
                                    {
                                        keyframe.Offset = chunkIO.ReadChunkVector3(chunkSize4);
                                    }
                                    else if (id4 == Wad2Chunks.KeyFrameBoundingBox)
                                    {
                                        var kfMin = Vector3.Zero;
                                        var kfMax = Vector3.Zero;
                                        chunkIO.ReadChunks((id5, chunkSize5) =>
                                        {
                                            if (id5 == Wad2Chunks.MeshBoundingBoxMin)
                                                kfMin = chunkIO.ReadChunkVector3(chunkSize5);
                                            else if (id5 == Wad2Chunks.MeshBoundingBoxMax)
                                                kfMax = chunkIO.ReadChunkVector3(chunkSize5);
                                            else
                                                return false;
                                            return true;
                                        });
                                        keyframe.BoundingBox = new BoundingBox(kfMin, kfMax);
                                    }
                                    else if (id4 == Wad2Chunks.KeyFrameAngle)
                                    {
                                        var angle = new WadKeyFrameRotation();
                                        angle.Rotations = chunkIO.ReadChunkVector3(chunkSize4);
                                        keyframe.Angles.Add(angle);
                                    }
                                    else
                                    {
                                        return false;
                                    }
                                    return true;
                                });
                                animation.KeyFrames.Add(keyframe);
                            }
                            else if (id3 == Wad2Chunks.StateChange)
                            {
                                var stateChange = new WadStateChange();
                                stateChange.StateId = LEB128.ReadUShort(chunkIO.Raw);
                                chunkIO.ReadChunks((id4, chunkSize4) =>
                                {
                                    if (id4 == Wad2Chunks.Dispatch)
                                    {
                                        var dispatch = new WadAnimDispatch();
                                        dispatch.InFrame = LEB128.ReadUShort(chunkIO.Raw);
                                        dispatch.OutFrame = LEB128.ReadUShort(chunkIO.Raw);
                                        dispatch.NextAnimation = LEB128.ReadUShort(chunkIO.Raw);
                                        dispatch.NextFrame = LEB128.ReadUShort(chunkIO.Raw);
                                        stateChange.Dispatches.Add(dispatch);
                                    }
                                    else
                                    {
                                        return false;
                                    }
                                    return true;
                                });
                                animation.StateChanges.Add(stateChange);
                            }
                            else if (id3 == Wad2Chunks.AnimCommand)
                            {
                                var command = new WadAnimCommand();
                                long offset = chunkIO.Raw.BaseStream.Position;
                                command.Type = (WadAnimCommandType)LEB128.ReadUShort(chunkIO.Raw);
                                command.Parameter1 = LEB128.ReadShort(chunkIO.Raw);
                                command.Parameter2 = LEB128.ReadShort(chunkIO.Raw);
                                command.Parameter3 = LEB128.ReadShort(chunkIO.Raw);

                                chunkIO.ReadChunks((id4, chunkSize4) =>
                                {
                                    if (id4 == Wad2Chunks.AnimCommandSoundInfo)
                                    {
                                        var info = chunkIO.ReadChunkInt(chunkSize4);
                                        if (info != -1)
                                            command.SoundInfoObsolete = soundInfos[info];
                                        return true;
                                    }
                                    else
                                        return false;
                                });

                                animation.AnimCommands.Add(command);
                            }
                            else
                            {
                                return false;
                            }
                            return true;
                        });

                        // Legacy code for calculating start and end velocities
                        if (!foundNewVelocitiesChunk)
                        {
                            float acceleration = oldAccel / 65536.0f;
                            animation.StartVelocity = oldSpeed / 65536.0f;
                            animation.EndVelocity = animation.StartVelocity + acceleration *
                                                        (animation.KeyFrames.Count - 1) * animation.FrameRate;

                            float lateralAcceleration = oldLatAccel / 65536.0f;
                            animation.StartLateralVelocity = oldLatSpeed / 65536.0f;
                            animation.EndLateralVelocity = animation.StartLateralVelocity + lateralAcceleration *
                                                                (animation.KeyFrames.Count - 1) * animation.FrameRate;
                        }

                        mov.Animations.Add(animation);
                    }
                    else
                    {
                        return false;
                    }
                    return true;
                });

                wad.Moveables.Add(mov.Id, mov);

                return true;
            });

            return true;
        }

        private static bool LoadStatics(ChunkReader chunkIO, ChunkId idOuter, Wad2 wad, /*Dictionary<long, WadMesh> meshes*/
                                        Dictionary<long, WadTexture> textures)
        {
            if (idOuter != Wad2Chunks.Statics)
                return false;

            chunkIO.ReadChunks((id, chunkSize) =>
            {
                if (id != Wad2Chunks.Static)
                    return false;

                var s = new WadStatic(new WadStaticId(LEB128.ReadUInt(chunkIO.Raw)));
                //s.Mesh = meshes[LEB128.ReadInt(chunkIO.Raw)];
                s.Flags = LEB128.ReadShort(chunkIO.Raw);
                s.LightingType = (WadMeshLightingType)LEB128.ReadShort(chunkIO.Raw);

                chunkIO.ReadChunks((id2, chunkSize2) =>
                {
                    if (id2 == Wad2Chunks.StaticVisibilityBox)
                    {
                        var min = Vector3.Zero;
                        var max = Vector3.Zero;
                        chunkIO.ReadChunks((id3, chunkSize3) =>
                        {
                            if (id3 == Wad2Chunks.MeshBoundingBoxMin)
                                min = chunkIO.ReadChunkVector3(chunkSize3);
                            else if (id3 == Wad2Chunks.MeshBoundingBoxMax)
                                max = chunkIO.ReadChunkVector3(chunkSize3);
                            else
                                return false;
                            return true;
                        });
                        s.VisibilityBox = new BoundingBox(min, max);
                    }
                    else if (id2 == Wad2Chunks.StaticCollisionBox)
                    {
                        var min = Vector3.Zero;
                        var max = Vector3.Zero;
                        chunkIO.ReadChunks((id3, chunkSize3) =>
                        {
                            if (id3 == Wad2Chunks.MeshBoundingBoxMin)
                                min = chunkIO.ReadChunkVector3(chunkSize3);
                            else if (id3 == Wad2Chunks.MeshBoundingBoxMax)
                                max = chunkIO.ReadChunkVector3(chunkSize3);
                            else
                                return false;
                            return true;
                        });
                        s.CollisionBox = new BoundingBox(min, max);
                    }
                    else if (id2 == Wad2Chunks.Mesh)
                        s.Mesh = LoadMesh(chunkIO, chunkSize2, textures);
                    else if (id2 == Wad2Chunks.StaticAmbientLight)
                        s.AmbientLight = chunkIO.ReadChunkShort(chunkSize2);
                    else if (id2 == Wad2Chunks.StaticLight)
                    {
                        var light = new WadLight();
                        chunkIO.ReadChunks((id3, chunkSize3) =>
                        {
                            if (id3 == Wad2Chunks.StaticLightPosition)
                                light.Position = chunkIO.ReadChunkVector3(chunkSize3);
                            else if (id3 == Wad2Chunks.StaticLightRadius)
                                light.Radius = chunkIO.ReadChunkFloat(chunkSize3);
                            else if (id3 == Wad2Chunks.StaticLightIntensity)
                                light.Intensity = chunkIO.ReadChunkFloat(chunkSize3);
                            else
                                return false;
                            return true;
                        });
                        s.Lights.Add(light);
                    }
                    else
                    {
                        return false;
                    }
                    return true;
                });

                wad.Statics.Add(s.Id, s);
                return true;
            });

            return true;
        }
    }
}
