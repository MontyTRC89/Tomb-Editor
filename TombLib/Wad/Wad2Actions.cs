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
        private void CollectTexturesAndMeshesForCancellation(WadObject obj,
                                                             List<WadTexture> textures,
                                                             List<WadMesh> meshes)
        {
            var meshesToCheck = new List<WadMesh>();
            var texturesToCheck = new List<WadTexture>();

            // Collect all meshes
            if (obj.GetType() == typeof(WadMoveable))
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
                    if (obj.GetType() == typeof(WadMoveable) && moveable.ObjectID == obj.ObjectID) continue;

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
                        if (obj.GetType() == typeof(WadStatic) && staticMesh.ObjectID == obj.ObjectID) continue;

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
                    if (obj.GetType() == typeof(WadMoveable) && moveable.ObjectID == obj.ObjectID) continue;

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
                    if (obj.GetType() == typeof(WadStatic) && staticMesh.ObjectID == obj.ObjectID) continue;

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
        }

        public void DeleteObject(WadObject obj)
        {
            // Collect resources to remove
            var textures = new List<WadTexture>();
            var meshes = new List<WadMesh>();

            CollectTexturesAndMeshesForCancellation(obj, textures, meshes);

            // Delete shared resource not needed anymore
            foreach (var texture in textures)
                Textures.Remove(texture.Hash);

            foreach (var mesh in meshes)
                Meshes.Remove(mesh.Hash);

            // Delete object
            if (obj.GetType() == typeof(WadMoveable))
                Moveables.Remove(obj.ObjectID);
            else
                Statics.Remove(obj.ObjectID);
        }

        public void AddObject(WadObject obj, uint destination)
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
            var textures = new List<WadTexture>();
            var meshes = new List<WadMesh>();

            // Add meshes to the shared meshes array
            if (isMoveable)
            {
                var moveable = (WadMoveable)obj;

                // Add a clone of meshes
                foreach (var mesh in moveable.Meshes)
                    meshes.Add(mesh.Clone());
            }
            else
            {
                var staticMesh = (WadStatic)obj;
                meshes.Add(staticMesh.Mesh.Clone());
            }

            // Now in the clone list check for textures and update them if present
            foreach (var mesh in meshes)
            {
                foreach (var poly in mesh.Polys)
                {
                    if (Textures.ContainsKey(poly.Texture.Hash))
                        poly.Texture = Textures[poly.Texture.Hash];
                    else
                        Textures.Add(poly.Texture.Hash, poly.Texture);
                }
            }

            // Add the object to Wad2
            if (isMoveable)
            {
                var moveable = (WadMoveable)obj;
                moveable.Meshes.Clear();
                moveable.Meshes.AddRange(meshes);

                moveable.ObjectID = destination;
                Moveables.Add(destination, moveable);
            }
            else
            {
                var staticMesh = (WadStatic)obj;
                staticMesh.Mesh = meshes[0];

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
