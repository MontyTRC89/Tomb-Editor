using SharpDX;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TombLib.Graphics;
using TombLib.Utils;

namespace TombLib.Wad
{
    public partial class Wad2
    {
        private void CollectResourcesForCancellation(WadObject obj,
                                                     List<WadTexture> textures,
                                                     List<WadMesh> meshes,
                                                     List<ushort> sounds,
                                                     List<WadSound> waves)
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
                    if (!texturesToCheck.Contains(poly.Texture) && poly.Texture != null) texturesToCheck.Add(poly.Texture);
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
                            if (poly.Texture == texture)
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
                        if (poly.Texture == texture)
                        {
                            foundInStatics = true;
                            break;
                        }
                    }

                    if (foundInStatics) break;
                }

                if (!foundInMoveables && !foundInStatics) textures.Add(texture);
            }

            // Now check for waves
            if (isMoveable)
            {
                var moveable = (WadMoveable)obj;
                var tempSounds = new List<WadSoundInfo>();
                var tempWaves = new List<WadSound>();

                // First, search for all referenced sounds
                foreach (var animation in moveable.Animations)
                {
                    foreach (var command in animation.AnimCommands)
                    {
                        if (command.Type == WadAnimCommandType.PlaySound)
                        {
                            ushort soundId = (ushort)(command.Parameter2 & 0x3fff);
                            if (SoundInfo.ContainsKey(soundId))
                            {
                                if (!sounds.Contains(soundId))
                                    sounds.Add(soundId);
                            }
                        }
                    }
                }

                // Second, for each sound found, collect samples
                foreach (var foundSound in tempSounds)
                    foreach (var wave in foundSound.WaveSounds)
                        if (!tempWaves.Contains(wave))
                            tempWaves.Add(wave);

                // Third, from temp waves, identify which waves are not used
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
                                    if (SoundInfo.ContainsKey(soundId))
                                    {
                                        var currentSoundInfo = SoundInfo[soundId];
                                        if (currentSoundInfo.WaveSounds.Contains(foundWave))
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
                                    if (SoundInfo.ContainsKey(soundId))
                                    {
                                        var currentInfo = SoundInfo[soundId];
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
            var waves = new List<WadSound>();

            CollectResourcesForCancellation(obj, textures, meshes, sounds, waves);

            // Delete shared resource not needed anymore
            foreach (var texture in textures)
                Textures.Remove(texture.Hash);

            foreach (var mesh in meshes)
                Meshes.Remove(mesh.Hash);

            foreach (var sound in sounds)
                SoundInfo.Remove(sound);

            foreach (var wave in waves)
                WaveSounds.Remove(wave.Hash);

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
                    if (Textures.ContainsKey(poly.Texture.Hash))
                        poly.Texture = Textures[poly.Texture.Hash];
                    else
                        Textures.Add(poly.Texture.Hash, poly.Texture);
                }

                mesh.UpdateHash();
            }

            // Add the object
            WadObject newObject;

            if (isMoveable)
            {
                var moveable = (WadMoveable)obj;
                newObject = new WadMoveable();
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
                newObject = new WadStatic();
                var newStaticMesh = (WadStatic)newObject;

                // Add mesh
                if (!Meshes.ContainsKey(meshesToAdd[0].Hash))
                    Meshes.Add(meshesToAdd[0].Hash, meshesToAdd[0]);

                newStaticMesh.Mesh = Meshes[meshesToAdd[0].Hash];
                newStaticMesh.ObjectID = destination;
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

            // Collect sounds
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
                            if (srcWad.SoundInfo.ContainsKey(soundId))
                            {
                                // First I check if sound was already remapped
                                if (soundsRemapTable.ContainsKey(soundId))
                                {
                                    // Remap current sound
                                    command.Parameter2 = (ushort)((command.Parameter2 & 0xc000) + soundsRemapTable[soundId]);
                                }
                                else
                                {
                                    if (Wad2.MandatorySounds.Contains(soundId))
                                    {
                                        // If this is a mandatory sound, I can add it only if doesn't exist in dest Wad2
                                        if (!SoundInfo.ContainsKey(soundId))
                                        {
                                            // Add this sound in the same slot
                                            var newSoundInfo = srcWad.SoundInfo[soundId].Clone();

                                            SoundInfo.Add(soundId, newSoundInfo);
                                            soundsRemapTable.Add(soundId, soundId);
                                            
                                            // Add wave files or get them if they exist
                                            for (int k = 0; k < newSoundInfo.WaveSounds.Count; k++)
                                            {
                                                var wave = newSoundInfo.WaveSounds[k];

                                                if (!WaveSounds.ContainsKey(wave.Hash))
                                                    WaveSounds.Add(wave.Hash, wave);
                                                else
                                                    newSoundInfo.WaveSounds[k] = WaveSounds[wave.Hash];
                                            }
                                        }
                                    }
                                    else
                                    {
                                        bool foundSoundInfo = false;

                                        // Search for an identical WadSoundInfo
                                        for (int i = 0; i < SoundInfo.Count; i++)
                                        {
                                            var currentInfo = SoundInfo.ElementAt(i).Value;
                                            var currentSoundId = SoundInfo.ElementAt(i).Key;

                                            if (currentInfo.Hash == srcWad.SoundInfo[soundId].Hash)
                                            {
                                                soundsRemapTable.Add(soundId, currentSoundId);

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
                                                if (!SoundInfo.ContainsKey((ushort)j))
                                                {
                                                    freeId = (ushort)j;
                                                    break;
                                                }
                                            }

                                            var newSoundInfo = srcWad.SoundInfo[soundId].Clone();
                                            SoundInfo.Add(freeId, newSoundInfo);
                                            soundsRemapTable.Add(soundId, freeId);
                                            
                                            // Add waves
                                            for (int k = 0; k < newSoundInfo.WaveSounds.Count; k++)
                                            {
                                                var wave = newSoundInfo.WaveSounds[k];

                                                if (!WaveSounds.ContainsKey(wave.Hash))
                                                    WaveSounds.Add(wave.Hash, wave);
                                                else
                                                    newSoundInfo.WaveSounds[k] = WaveSounds[wave.Hash];
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
    }
}
