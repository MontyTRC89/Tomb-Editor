using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using TombLib.GeometryIO;
using TombLib.GeometryIO.Importers;
using TombLib.Graphics;
using TombLib.Utils;
using TombLib.Wad.Catalog;

namespace TombLib.Wad
{
    public partial class Wad2
    {
        private void CollectResourcesForCancellation(WadObject obj,
                                                     List<WadTexture> textures,
                                                     List<WadMesh> meshes,
                                                     List<ushort> sounds,
                                                     List<WadSample> waves)
        {
            bool isMoveable = obj.GetType() == typeof(WadMoveable);

            var meshesToCheck = new List<WadMesh>();
            var texturesToCheck = new List<WadTexture>();

            // Collect all meshes
            if (isMoveable)
            {
                WadMoveable moveable = (WadMoveable)obj;

                foreach (var mesh in moveable.Meshes)
                {
                    if (!meshesToCheck.Contains(mesh)) meshesToCheck.Add(mesh);
                }
            }
            else
            {
                WadStatic staticMesh = (WadStatic)obj;
                meshesToCheck.Add(staticMesh.Mesh);
            }

            // Now check if some of selected meshes are used elsewhere
            foreach (var mesh in meshesToCheck)
            {
                bool foundInMoveables = false;

                for (int i = 0; i < Moveables.Count; i++)
                {
                    var moveable = Moveables.ElementAt(i).Value;
                    if (isMoveable && moveable.ObjectID == obj.ObjectID) continue;

                    foreach (var moveableMesh in moveable.Meshes)
                    {
                        if (moveableMesh == mesh)
                        {
                            foundInMoveables = true;
                            break;
                        }
                    }

                    if (foundInMoveables) break;
                }

                bool foundInStatics = false;

                if (!foundInMoveables)
                {
                    for (int i = 0; i < Statics.Count; i++)
                    {
                        var staticMesh = Statics.ElementAt(i).Value;
                        if (!isMoveable && staticMesh.ObjectID == obj.ObjectID) continue;

                        if (staticMesh.Mesh == mesh)
                        {
                            foundInStatics = true;
                            break;
                        }
                    }
                }

                if (!foundInMoveables && !foundInStatics) meshes.Add(mesh);
            }

            // At this point, I have only the meshes to remove and from them I collect all textures
            foreach (var mesh in meshes)
            {
                foreach (var poly in mesh.Polys)
                {
                    var wadTexture = poly.Texture.Texture as WadTexture;
                    if ((wadTexture != null) && !texturesToCheck.Contains(poly.Texture.Texture))
                        texturesToCheck.Add(wadTexture);
                }
            }

            // Like for meshes, search inside other objects
            foreach (var texture in texturesToCheck)
            {
                bool foundInMoveables = false;

                for (int i = 0; i < Moveables.Count; i++)
                {
                    var moveable = Moveables.ElementAt(i).Value;
                    if (isMoveable && moveable.ObjectID == obj.ObjectID) continue;

                    foreach (var moveableMesh in moveable.Meshes)
                    {
                        foreach (var poly in moveableMesh.Polys)
                        {
                            if (poly.Texture.Texture == texture)
                            {
                                foundInMoveables = true;
                                break;
                            }
                        }

                        if (foundInMoveables) break;
                    }

                    if (foundInMoveables) break;
                }

                bool foundInStatics = false;

                for (int i = 0; i < Statics.Count; i++)
                {
                    var staticMesh = Statics.ElementAt(i).Value;
                    if (!isMoveable && staticMesh.ObjectID == obj.ObjectID) continue;

                    foreach (var poly in staticMesh.Mesh.Polys)
                    {
                        if (poly.Texture.Texture == texture)
                        {
                            foundInStatics = true;
                            break;
                        }
                    }

                    if (foundInStatics) break;
                }

                if (!foundInMoveables && !foundInStatics) textures.Add(texture);
            }

            // Now check for sounds
            if (isMoveable)
            {
                var moveable = (WadMoveable)obj;
                var tempSounds = new List<WadSoundInfo>();
                var tempWaves = new List<WadSample>();

                // First, search for all referenced sounds
                foreach (var animation in moveable.Animations)
                {
                    foreach (var command in animation.AnimCommands)
                    {
                        if (command.Type == WadAnimCommandType.PlaySound)
                        {
                            ushort soundId = (ushort)(command.Parameter2 & 0x3fff);
                            if (Sounds.ContainsKey(soundId))
                            {
                                if (!sounds.Contains(soundId))
                                    sounds.Add(soundId);
                            }
                        }
                    }
                }

                // Second, for each sound found, collect samples
                foreach (var foundSound in tempSounds)
                    foreach (var wave in foundSound.Samples)
                        if (!tempWaves.Contains(wave))
                            tempWaves.Add(wave);

                // Third, from temp samples, identify which waves are not used
                foreach (var foundWave in tempWaves)
                {
                    bool isFound = false;

                    foreach (var mov in Moveables)
                    {
                        // Skip current moveable
                        if (moveable.ObjectID == mov.Value.ObjectID) continue;

                        foreach (var animation in moveable.Animations)
                        {
                            foreach (var command in animation.AnimCommands)
                            {
                                if (command.Type == WadAnimCommandType.PlaySound)
                                {
                                    ushort soundId = (ushort)(command.Parameter2 & 0x3fff);
                                    if (Sounds.ContainsKey(soundId))
                                    {
                                        var currentSoundInfo = Sounds[soundId];
                                        if (currentSoundInfo.Samples.Contains(foundWave))
                                        {
                                            isFound = true;
                                            break;
                                        }
                                    }
                                }
                            }

                            if (isFound) break;
                        }

                        if (isFound) break;
                    }

                    // If the wave file was not used by other moveables, then add it to the list
                    if (!isFound) waves.Add(foundWave);
                }

                // Fourth, identify which sound infos are not used
                foreach (var foundSoundInfo in tempSounds)
                {
                    bool isFound = false;
                    ushort foundId = 0;

                    foreach (var mov in Moveables)
                    {
                        // Skip current moveable
                        if (moveable.ObjectID == mov.Value.ObjectID) continue;

                        foreach (var animation in moveable.Animations)
                        {
                            foreach (var command in animation.AnimCommands)
                            {
                                if (command.Type == WadAnimCommandType.PlaySound)
                                {
                                    ushort soundId = (ushort)(command.Parameter2 & 0x3fff);
                                    if (Sounds.ContainsKey(soundId))
                                    {
                                        var currentInfo = Sounds[soundId];
                                        if (currentInfo.Hash == foundSoundInfo.Hash)
                                        {
                                            isFound = true;
                                            foundId = soundId;
                                            break;
                                        }
                                    }
                                }
                            }

                            if (isFound) break;
                        }

                        if (isFound) break;
                    }

                    // If this found info was not used by other moveables, then add it to the list
                    if (!isFound) sounds.Add(foundId);
                }
            }
        }

        public void DeleteObject(WadObject obj)
        {
            // Collect resources to remove
            var textures = new List<WadTexture>();
            var meshes = new List<WadMesh>();
            var sounds = new List<ushort>();
            var waves = new List<WadSample>();

            CollectResourcesForCancellation(obj, textures, meshes, sounds, waves);

            // Delete shared resource not needed anymore
            foreach (var texture in textures)
                Textures.Remove(texture.Hash);

            foreach (var mesh in meshes)
                Meshes.Remove(mesh.Hash);

            foreach (var sound in sounds)
                Sounds.Remove(sound);

            foreach (var wave in waves)
                Samples.Remove(wave.Hash);

            // Delete object
            if (obj.GetType() == typeof(WadMoveable))
                Moveables.Remove(obj.ObjectID);
            else
                Statics.Remove(obj.ObjectID);
        }

        public void AddObject(WadObject obj, Wad2 srcWad, uint destination)
        {
            bool isMoveable = (obj.GetType() == typeof(WadMoveable));

            // Check if the destination slot is alredy used
            // If the destination slot is used, then delete object
            if (isMoveable)
            {
                if (Moveables.ContainsKey(destination))
                    DeleteObject(Moveables[destination]);
            }
            else
            {
                if (Statics.ContainsKey(destination))
                    DeleteObject(Statics[destination]);
            }

            // Collect all meshes and textures
            var texturesToAdd = new List<WadTexture>();
            var meshesToAdd = new List<WadMesh>();
            var soundsRemapTable = new Dictionary<ushort, ushort>();

            // Add meshes to the shared meshes array
            if (isMoveable)
            {
                var moveable = (WadMoveable)obj;

                // Add a clone of meshes
                foreach (var mesh in moveable.Meshes)
                    meshesToAdd.Add(mesh.Clone());
            }
            else
            {
                var staticMesh = (WadStatic)obj;
                meshesToAdd.Add(staticMesh.Mesh.Clone());
            }

            // Now in the clone list check for textures and update them if present
            foreach (var mesh in meshesToAdd)
            {
                foreach (var poly in mesh.Polys)
                {
                    var wadTexture = poly.Texture.Texture as WadTexture;
                    if (wadTexture != null)
                        if (Textures.ContainsKey(wadTexture.Hash))
                        {
                            var textureArea = poly.Texture;
                            textureArea.Texture = Textures[wadTexture.Hash];
                            poly.Texture = textureArea;
                        }
                        else
                            Textures.Add(wadTexture.Hash, wadTexture);
                }

                mesh.UpdateHash();
            }

            // Add the object
            WadObject newObject;

            if (isMoveable)
            {
                var moveable = (WadMoveable)obj;
                newObject = new WadMoveable(this);
                var newMoveable = (WadMoveable)newObject;

                // Add meshes
                foreach (var mesh in meshesToAdd)
                {
                    if (!Meshes.ContainsKey(mesh.Hash))
                        Meshes.Add(mesh.Hash, mesh);

                    newMoveable.Meshes.Add(Meshes[mesh.Hash]);
                }

                newMoveable.ObjectID = destination;
                newMoveable.Offset = new Vector3(moveable.Offset.X, moveable.Offset.Y, moveable.Offset.Z);
                //newMoveable.Name = moveable.Name;

                // Add mesh trees
                foreach (var link in moveable.Links)
                    newMoveable.Links.Add(link.Clone());

                // Add animations
                foreach (var animation in moveable.Animations)
                    newMoveable.Animations.Add(animation.Clone());

                Moveables.Add(destination, newMoveable);
            }
            else
            {
                var staticMesh = (WadStatic)obj;
                newObject = new WadStatic(this);
                var newStaticMesh = (WadStatic)newObject;

                // Add mesh
                if (!Meshes.ContainsKey(meshesToAdd[0].Hash))
                    Meshes.Add(meshesToAdd[0].Hash, meshesToAdd[0]);

                newStaticMesh.Mesh = Meshes[meshesToAdd[0].Hash];
                newStaticMesh.ObjectID = destination;
                //newStaticMesh.Name = staticMesh.Name;

                newStaticMesh.CollisionBox = new BoundingBox(new Vector3(staticMesh.CollisionBox.Minimum.X,
                                                                         staticMesh.CollisionBox.Minimum.Y,
                                                                         staticMesh.CollisionBox.Minimum.Z),
                                                             new Vector3(staticMesh.CollisionBox.Maximum.X,
                                                                         staticMesh.CollisionBox.Maximum.Y,
                                                                         staticMesh.CollisionBox.Maximum.Z)
                                                                        );
                newStaticMesh.VisibilityBox = new BoundingBox(new Vector3(staticMesh.VisibilityBox.Minimum.X,
                                                                          staticMesh.VisibilityBox.Minimum.Y,
                                                                          staticMesh.VisibilityBox.Minimum.Z),
                                                              new Vector3(staticMesh.VisibilityBox.Maximum.X,
                                                                          staticMesh.VisibilityBox.Maximum.Y,
                                                                          staticMesh.VisibilityBox.Maximum.Z)
                                                                          );
                newStaticMesh.Flags = staticMesh.Flags;

                Statics.Add(destination, newStaticMesh);
            }

            if (SoundManagementSystem == WadSoundManagementSystem.DynamicSoundMap)
            {
                // Collect sounds using the new remap system
                if (isMoveable)
                {
                    var newMoveable = (WadMoveable)newObject;

                    foreach (var animation in newMoveable.Animations)
                    {
                        foreach (var command in animation.AnimCommands)
                        {
                            if (command.Type == WadAnimCommandType.PlaySound)
                            {
                                ushort soundId = (ushort)(command.Parameter2 & 0x3fff);
                                if (srcWad.Sounds.ContainsKey(soundId))
                                {
                                    // First I check if sound was already remapped
                                    if (soundsRemapTable.ContainsKey(soundId))
                                    {
                                        // Remap current sound
                                        command.Parameter2 = (ushort)((command.Parameter2 & 0xc000) + soundsRemapTable[soundId]);
                                    }
                                    else
                                    {
                                        if (TrCatalog.IsSoundMandatory(Version, soundId))
                                        {
                                            // If this is a mandatory sound, I can add it only if doesn't exist in dest Wad2
                                            if (!Sounds.ContainsKey(soundId))
                                            {
                                                // Add this sound in the same slot
                                                var newSoundInfo = srcWad.Sounds[soundId].Clone();

                                                Sounds.Add(soundId, newSoundInfo);
                                                soundsRemapTable.Add(soundId, soundId);

                                                // Add wave files or get them if they exist
                                                for (int k = 0; k < newSoundInfo.Samples.Count; k++)
                                                {
                                                    var wave = newSoundInfo.Samples[k];

                                                    if (!Samples.ContainsKey(wave.Hash))
                                                        Samples.Add(wave.Hash, wave);
                                                    else
                                                        newSoundInfo.Samples[k] = Samples[wave.Hash];
                                                }
                                            }
                                        }
                                        else
                                        {
                                            bool foundSoundInfo = false;

                                            // Search for an identical WadSoundInfo
                                            for (int i = 0; i < Sounds.Count; i++)
                                            {
                                                var currentInfo = Sounds.ElementAt(i).Value;
                                                var currentSoundId = Sounds.ElementAt(i).Key;

                                                if (currentInfo.Hash == srcWad.Sounds[soundId].Hash)
                                                {
                                                    soundsRemapTable.Add(soundId, currentSoundId);
                                                    foundSoundInfo = true;

                                                    // Remap current sound
                                                    command.Parameter2 = (ushort)((command.Parameter2 & 0xc000) + currentSoundId);

                                                    break;
                                                }
                                            }

                                            if (!foundSoundInfo)
                                            {
                                                // Get a free sound index
                                                ushort freeId = 0;
                                                for (int j = 0; j < 370; j++)
                                                {
                                                    if (!Sounds.ContainsKey((ushort)j))
                                                    {
                                                        freeId = (ushort)j;
                                                        break;
                                                    }
                                                }

                                                var newSoundInfo = srcWad.Sounds[soundId].Clone();
                                                Sounds.Add(freeId, newSoundInfo);
                                                soundsRemapTable.Add(soundId, freeId);

                                                // Add waves
                                                for (int k = 0; k < newSoundInfo.Samples.Count; k++)
                                                {
                                                    var wave = newSoundInfo.Samples[k];

                                                    if (!Samples.ContainsKey(wave.Hash))
                                                        Samples.Add(wave.Hash, wave);
                                                    else
                                                        newSoundInfo.Samples[k] = Samples[wave.Hash];
                                                }

                                                // Remap current sound
                                                command.Parameter2 = (ushort)((command.Parameter2 & 0xc000) + freeId);
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
            else
            {
                // Add sounds using the old system
                var soundsToAdd = new List<ushort>();
                if (isMoveable)
                {
                    var newMoveable = (WadMoveable)newObject;

                    foreach (var animation in newMoveable.Animations)
                    {
                        foreach (var command in animation.AnimCommands)
                        {
                            if (command.Type == WadAnimCommandType.PlaySound)
                            {
                                ushort soundId = (ushort)(command.Parameter2 & 0x3fff);
                                if (srcWad.Sounds.ContainsKey(soundId))
                                {
                                    if (Sounds.ContainsKey(soundId))
                                    {
                                        // Don't do anything for now, just use the copy of sound 
                                        // The user should manually copy the sound
                                    }
                                    else
                                    {
                                        // Add this sound in the same slot
                                        var newSoundInfo = srcWad.Sounds[soundId].Clone();

                                        Sounds.Add(soundId, newSoundInfo);
                                        
                                        // Add wave files or get them if they exist
                                        for (int k = 0; k < newSoundInfo.Samples.Count; k++)
                                        {
                                            var wave = newSoundInfo.Samples[k];

                                            if (!Samples.ContainsKey(wave.Hash))
                                                Samples.Add(wave.Hash, wave);
                                            else
                                                newSoundInfo.Samples[k] = Samples[wave.Hash];
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        public bool AddSpriteSequence(uint objectID)
        {
            // Check if the sequence already exists
            foreach (var seq in SpriteSequences)
                if (seq.ObjectID == objectID)
                    return false;

            var sequence = new WadSpriteSequence();
            sequence.ObjectID = objectID;

            SpriteSequences.Add(sequence);

            return true;
        }

        public bool AddSprite(WadSpriteSequence sequence, ImageC image)
        {
            // Create a new texture
            var sprite = new WadSprite();
            sprite.Image = image;
            sprite.UpdateHash();

            // Check if texture exists
            if (SpriteTextures.ContainsKey(sprite.Hash))
                sprite = SpriteTextures[sprite.Hash];
            else
                if (GraphicsDevice != null) sprite.DirectXTexture = TextureLoad.Load(GraphicsDevice, sprite.Image);

            // Add the texture to the sequence
            sequence.Sprites.Add(sprite);

            return true;
        }

        public bool DeleteSprite(WadSpriteSequence sequence, WadSprite sprite)
        {
            if (!SpriteSequences.Contains(sequence) || !sequence.Sprites.Contains(sprite)) return false;

            // Check if sprite exists
            bool found = false;
            foreach (var seq in SpriteSequences)
                if (seq != sequence)
                    if (seq.Sprites.Contains(sprite))
                        found = true;

            // Remove the sprite from the sequence
            sequence.Sprites.Remove(sprite);

            // Eventually remove sprite from the shared list
            if (!found)
            {
                sprite.Dispose();
                SpriteTextures.Remove(sprite.Hash);
            }

            return true;
        }

        public bool DeleteSpriteSequence(WadSpriteSequence sequence)
        {
            if (!SpriteSequences.Contains(sequence)) return false;

            var spritesToRemove = new List<WadSprite>();

            foreach (var sprite in sequence.Sprites)
            {
                // Check if sprite exists
                bool found = false;
                foreach (var seq in SpriteSequences)
                    if (seq != sequence)
                        if (seq.Sprites.Contains(sprite))
                            found = true;

                if (!found)
                    spritesToRemove.Add(sprite);
            }

            // Remove unused sprites
            for (int i = 0; i < spritesToRemove.Count; i++)
            {
                spritesToRemove[i].Dispose();
                SpriteTextures.Remove(spritesToRemove[i].Hash);
            }

            // Remove the sequence
            SpriteSequences.Remove(sequence);

            return true;
        }

        public bool DeleteSound(ushort soundId)
        {
            // Get the first available sound
            int found = -1;
            for (int i = 0; i < 370; i++)
                if (i != soundId && Sounds.ContainsKey((ushort)i))
                {
                    found = i;
                    break;
                }

            foreach (var moveable in Moveables)
            {
                foreach (var animation in moveable.Value.Animations)
                {
                    foreach (var command in animation.AnimCommands)
                    {
                        if (command.Type == WadAnimCommandType.PlaySound)
                        {
                            ushort currentSoundId = (ushort)(command.Parameter2 & 0x3fff);
                            if (soundId == currentSoundId)
                            {
                                // Remap current sound
                                command.Parameter2 = (ushort)((command.Parameter2 & 0xc000) + (ushort)found);
                            }
                        }
                    }
                }
            }

            Sounds.Remove(soundId);

            return true;
        }

        public List<WadMoveable> GetAllMoveablesReferencingSound(ushort soundId)
        {
            var moveables = new List<WadMoveable>();

            foreach (var moveable in Moveables)
            {
                foreach (var animation in moveable.Value.Animations)
                {
                    foreach (var command in animation.AnimCommands)
                    {
                        if (command.Type == TombLib.Wad.WadAnimCommandType.PlaySound)
                        {
                            ushort currentSoundId = (ushort)(command.Parameter2 & 0x3fff);
                            if (soundId == currentSoundId)
                            {
                                if (!moveables.Contains(moveable.Value))
                                    moveables.Add(moveable.Value);
                            }
                        }
                    }
                }
            }

            return moveables;
        }

        public void CleanUnusedSamples()
        {
            var samplesToRemove = new List<WadSample>();

            foreach (var wave in Samples)
            {
                bool found = false;

                foreach (var soundInfo in Sounds)
                {
                    if (soundInfo.Value.Samples.Contains(wave.Value))
                    {
                        found = true;
                        break;
                    }
                }

                if (!found)
                    samplesToRemove.Add(wave.Value);
            }

            foreach (var sample in samplesToRemove)
                Samples.Remove(sample.Hash);
        }

        public void CleanUnusedSprites()
        {
            var spritesToRemove = new List<WadSprite>();

            foreach (var sprite in SpriteTextures)
            {
                bool found = false;

                foreach (var sequence in SpriteSequences)
                {
                    foreach (var spr in sequence.Sprites)
                    {
                        if (spr.Hash == sprite.Key)
                        {
                            found = true;
                            break;
                        }
                    }
                }

                if (!found)
                    spritesToRemove.Add(sprite.Value);
            }

            foreach (var sprite in spritesToRemove)
            {
                sprite.Dispose();
                SpriteTextures.Remove(sprite.Hash);
            }
        }

        public void CleanUnusedTextures()
        {
            var texturesToRemove = new List<WadTexture>();

            foreach (var texture in Textures)
            {
                bool found = false;

                foreach (var mesh in Meshes)
                {
                    foreach (var poly in mesh.Value.Polys)
                    {
                        var wadTexture = poly.Texture.Texture as WadTexture;
                        if (wadTexture != null && wadTexture.Hash == texture.Key)
                        {
                            found = true;
                            break;
                        }
                    }
                }

                if (!found)
                    texturesToRemove.Add(texture.Value);
            }

            foreach (var texture in texturesToRemove)
                Textures.Remove(texture.Hash);

            // Rebuild atlas
            RebuildTextureAtlas();
        }

        public ushort GetFirstFreeSoundSlot()
        {
            // Get the first available sound
            ushort found = UInt16.MaxValue;
            for (int i = 0; i < 370; i++)
                if (!Sounds.ContainsKey((ushort)i))
                {
                    found = (ushort)i;
                    break;
                }

            return found;
        }

        public uint GetFirstFreeStaticMesh()
        {
            if (Statics.Count == 0) return 0;

            // Get the first available sound
            uint found = UInt32.MaxValue;
            for (int i = 0; i < Statics.Count; i++)
                if (!Statics.ContainsKey((uint)i))
                {
                    found = (uint)i;
                    break;
                }

            return found;
        }

        public void CreateNewStaticMeshFromExternalModel(string fileName, IOGeometrySettings settings)
        {
            uint objectId = GetFirstFreeStaticMesh();

            var mesh = ImportWadMeshFromExternalModel(fileName, settings);

            var staticMesh = new WadStatic(this);

            staticMesh.ObjectID = objectId;
            staticMesh.Mesh = mesh;
            staticMesh.VisibilityBox = mesh.BoundingBox;
            staticMesh.CollisionBox = mesh.BoundingBox;

            Statics.Add(objectId, staticMesh);

            // Reload DirectX data
            PrepareDataForDirectX();
        }

        public WadMesh ImportWadMeshFromExternalModel(string fileName, IOGeometrySettings settings)
        {
            // Import the model
            var importer = new AssimpImporter(settings, (absoluteTexturePath) =>
            {
                var texture = new WadTexture();
                texture.Image = ImageC.FromFile(absoluteTexturePath);
                texture.UpdateHash();
                if (!Textures.ContainsKey(texture.Hash))
                    Textures.Add(texture.Hash, texture);
                return texture;
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
                            var poly = new WadPolygon(WadPolygonShape.Quad);

                            foreach (var index in tmpPoly.Indices)
                                poly.Indices.Add(index + lastBaseVertex);

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
                            var poly = new WadPolygon(WadPolygonShape.Triangle);

                            foreach (var index in tmpPoly.Indices)
                                poly.Indices.Add(index + lastBaseVertex);

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
            if (!Meshes.ContainsKey(mesh.Hash))
                Meshes.Add(mesh.Hash, mesh);
            mesh = Meshes[mesh.Hash];

            return Meshes[mesh.Hash];
        }

        /*public WadMesh ImportWadMeshFromExternalModel(string fileName, float scale = 1000.0f)
        {
            // Use Assimp.NET for importing model
            AssimpContext context = new AssimpContext();
            Scene scene = context.ImportFile(fileName, PostProcessPreset.TargetRealTimeMaximumQuality);

            var newMesh = new WadMesh();
            var textureFiles = new Dictionary<int, string>();
            var meshTextures = new Dictionary<int, WadTexture>();

            // Create the list of textures to load
            for (int i = 0; i < scene.Materials.Count; i++)
            {
                var mat = scene.Materials[i];

                var diffusePath = (mat.HasTextureDiffuse ? mat.TextureDiffuse.FilePath : null);
                if (diffusePath == null || diffusePath == "") continue;

                if (!textureFiles.ContainsValue(diffusePath))
                    textureFiles.Add(i, diffusePath);
            }

            // Now load them into the Wad2
            foreach (var textureFile in textureFiles)
            {
                var texture = new WadTexture();
                var image = ImageC.FromFile(textureFile.Value);
                texture.Image = image;
                texture.UpdateHash();

                if (!Textures.ContainsKey(texture.Hash))
                    Textures.Add(texture.Hash, texture);

                meshTextures.Add(textureFile.Key, Textures[texture.Hash]);
            }

            var minVertex = new Vector3(float.MaxValue, float.MaxValue, float.MaxValue);
            var maxVertex = new Vector3(float.MinValue, float.MinValue, float.MinValue);

            var lastBaseVertex = 0;

            // Loop for each mesh loaded in scene
            foreach (var mesh in scene.Meshes)
            {
                if (!meshTextures.ContainsKey(mesh.MaterialIndex)) continue;

                var faceTexture = meshTextures[mesh.MaterialIndex];

                bool hasTexCoords = mesh.HasTextureCoords(0);

                // Source data
                List<Vector3D> positions = mesh.Vertices;
                List<Vector3D> texCoords = mesh.TextureCoordinateChannels[0];

                var vertices = new List<Vector3>();
                var uv = new List<Vector2>();

                for (int i = 0; i < mesh.VertexCount; i++)
                {
                    var position = new Vector3(positions[i].X, positions[i].Y, positions[i].Z) * scale;
                    vertices.Add(position);

                    // Add now positions and shades to the new mesh
                    newMesh.VerticesPositions.Add(position);
                    newMesh.VerticesShades.Add(0);

                    // Track min & max vertex for bounding box
                    if (position.X <= minVertex.X && position.Y <= minVertex.Y && position.Z <= minVertex.Z)
                        minVertex = position;

                    if (position.X >= maxVertex.X && position.Y >= maxVertex.Y && position.Z >= maxVertex.Z)
                        maxVertex = position;

                    // Add texture coordinates
                    var currentUV = new Vector2(texCoords[i].X, 1.0f - texCoords[i].Y);

                    // HACK: maybe something better can be done, but for now it works
                    if (currentUV.X > 1.0f) currentUV.X -= 1.0f;
                    if (currentUV.Y > 1.0f) currentUV.Y -= 1.0f;
                    if (currentUV.X < 0.0f) currentUV.X += 1.0f;
                    if (currentUV.Y < 0.0f) currentUV.Y += 1.0f;

                    currentUV.X *= faceTexture.Width;
                    currentUV.Y *= faceTexture.Height;

                    uv.Add(currentUV);
                }

                // Add polygons
                foreach (var face in mesh.Faces)
                {
                    var poly = new WadPolygon(WadPolygonShape.Triangle);

                    poly.Indices.Add(lastBaseVertex + face.Indices[0]);
                    poly.Indices.Add(lastBaseVertex + face.Indices[1]);
                    poly.Indices.Add(lastBaseVertex + face.Indices[2]);

                    TextureArea textureArea = new TextureArea();
                    textureArea.TexCoord0 = uv[face.Indices[0]];
                    textureArea.TexCoord1 = uv[face.Indices[1]];
                    textureArea.TexCoord2 = uv[face.Indices[2]];
                    textureArea.Texture = faceTexture;
                    poly.Texture = textureArea;

                    newMesh.Polys.Add(poly);
                }

                lastBaseVertex = (int)newMesh.VerticesPositions.Count;
            }

            // Set the bounding box
            newMesh.BoundingBox = new BoundingBox(minVertex, maxVertex);

            // Calculate bounding sphere
            var centre = (minVertex + maxVertex) / 2.0f;
            var radius = (maxVertex - centre).Length();
            newMesh.BoundingSphere = new BoundingSphere(centre, radius);

            newMesh.UpdateHash();

            if (!Meshes.ContainsKey(newMesh.Hash))
                Meshes.Add(newMesh.Hash, newMesh);

            return Meshes[newMesh.Hash];
        }*/

        public static BoundingBox CalculateBoundingBox(WadMesh mesh, Matrix4x4 transform)
        {
            float xMin = float.MaxValue;
            float yMin = float.MaxValue;
            float zMin = float.MaxValue;
            float xMax = float.MinValue;
            float yMax = float.MinValue;
            float zMax = float.MinValue;

            // Add positions
            foreach (var oldVertex in mesh.VerticesPositions)
            {
                var transformedVertex = MathC.HomogenousTransform(oldVertex, transform);

                if (transformedVertex.X < xMin)
                    xMin = transformedVertex.X;
                if (transformedVertex.Y < yMin)
                    yMin = transformedVertex.Y;
                if (transformedVertex.Z < zMin)
                    zMin = transformedVertex.Z;

                if (transformedVertex.X > xMax)
                    xMax = transformedVertex.X;
                if (transformedVertex.Y > yMax)
                    yMax = transformedVertex.Y;
                if (transformedVertex.Z > zMax)
                    zMax = transformedVertex.Z;
            }

            Vector3 minVertex = new Vector3(xMin, yMin, zMin);
            Vector3 maxVertex = new Vector3(xMax, yMax, zMax);

            return new BoundingBox(minVertex, maxVertex);
        }
    }
}
