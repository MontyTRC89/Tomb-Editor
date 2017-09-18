using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TombLib.Utils;

namespace TombLib.Wad
{
    public partial class Wad2
    {
        private void CollectResourcesForCancellation(WadObject obj,
                                                     List<WadTexture> textures,
                                                     List<WadMesh> meshes,
                                                     List<ushort> sounds)
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
            }
        }

        public void DeleteObject(WadObject obj)
        {
            // Collect resources to remove
            var textures = new List<WadTexture>();
            var meshes = new List<WadMesh>();
            var sounds = new List<ushort>();

            CollectResourcesForCancellation(obj, textures, meshes, sounds);

            // Delete shared resource not needed anymore
            foreach (var texture in textures)
                Textures.Remove(texture.Hash);

            foreach (var mesh in meshes)
                Meshes.Remove(mesh.Hash);

            foreach (var sound in sounds)
                SoundInfo.Remove(sound);

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
            var soundsToAdd = new List<ushort>();

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

            // Add all sounds
            if (isMoveable)
            {
                var moveable = (WadMoveable)obj;

                foreach (var animation in moveable.Animations)
                {
                    foreach (var command in animation.AnimCommands)
                    {
                        if (command.Type == WadAnimCommandType.PlaySound)
                        {
                            ushort soundId = (ushort)(command.Parameter2 & 0x3fff);
                            if (srcWad.SoundInfo.ContainsKey(soundId))
                            {
                                if (!SoundInfo.ContainsKey(soundId))
                                    SoundInfo.Add(soundId, srcWad.SoundInfo[soundId]);
                                else
                                    SoundInfo[soundId] = srcWad.SoundInfo[soundId];
                            }
                        }
                    }
                }
            }

            // Add the object to Wad2
            if (isMoveable)
            {
                var moveable = (WadMoveable)obj;
                moveable.Meshes.Clear();
               
                foreach (var mesh in meshesToAdd)
                {
                    if (!Meshes.ContainsKey(mesh.Hash))
                        Meshes.Add(mesh.Hash, mesh);

                    moveable.Meshes.Add(Meshes[mesh.Hash]);
                }

                moveable.ObjectID = destination;
                Moveables.Add(destination, moveable);
            }
            else
            {
                var staticMesh = (WadStatic)obj;
                
                if (!Meshes.ContainsKey(meshesToAdd[0].Hash))
                    Meshes.Add(meshesToAdd[0].Hash, meshesToAdd[0]);

                staticMesh.Mesh = Meshes[meshesToAdd[0].Hash];

                staticMesh.ObjectID = destination;
                Statics.Add(destination, staticMesh);
            }
        }

        public bool AddSprite(WadSpriteSequence sequence, ImageC image)
        {
            // Create a new texture
            var sprite = new WadTexture();
            sprite.Image = image;
            sprite.UpdateHash();

            // Check if texture exists
            if (!SpriteTextures.ContainsKey(sprite.Hash)) return false;

            // Add the texture to the sequence
            sequence.Sprites.Add(sprite);

            return true;
        }

        public void DeleteSprite(WadSpriteSequence sequence, WadTexture sprite)
        {
            sequence.Sprites.Remove(sprite);
            SpriteTextures.Remove(sprite.Hash);
        }
    }
}
