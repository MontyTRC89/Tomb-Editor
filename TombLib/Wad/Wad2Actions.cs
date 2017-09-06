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
                                                             out List<WadTexture> textures,
                                                             out List<WadMesh> meshes)
        {
            textures = new List<WadTexture>();
            meshes = new List<WadMesh>();

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
                bool found = false;

                for (int i = 0; i < Moveables.Count; i++)
                {
                    var moveable = Moveables.ElementAt(i).Value;
                    if (obj.GetType() == typeof(WadMoveable) && moveable.ObjectID == obj.ObjectID) continue;

                    foreach (var moveableMesh in moveable.Meshes)
                    {
                        if (moveableMesh == mesh)
                        {
                            found = true;
                            break;
                        }
                    }

                    if (found) break;
                }

                if (!found) meshes.Add(mesh);

                found = false;

                for (int i = 0; i < Statics.Count; i++)
                {
                    var staticMesh = Statics.ElementAt(i).Value;
                    if (obj.GetType() == typeof(WadStatic) && staticMesh.ObjectID == obj.ObjectID) continue;

                    if (staticMesh.Mesh == mesh)
                    {
                        found = true;
                        break;
                    }
                }

                if (!found) meshes.Add(mesh);
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
                bool found = false;

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
                                found = true;
                                break;
                            }
                        }

                        if (found) break;
                    }

                    if (found) break;
                }

                if (!found) textures.Add(texture);

                found = false;

                for (int i = 0; i < Statics.Count; i++)
                {
                    var staticMesh = Statics.ElementAt(i).Value;
                    if (obj.GetType() == typeof(WadStatic) && staticMesh.ObjectID == obj.ObjectID) continue;

                    foreach (var poly in staticMesh.Mesh.Polys)
                    {
                        if (poly.Texture == texture)
                        {
                            found = true;
                            break;
                        }
                    }

                    if (found) break;
                }

                if (!found) textures.Add(texture);
            }
        }

        public void DeleteMoveable(WadMoveable moveable)
        {
            throw new NotImplementedException();
        }

        public void AddMoveable(Wad2 sourceWad, WadMoveable moveable)
        {
            throw new NotImplementedException();
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
