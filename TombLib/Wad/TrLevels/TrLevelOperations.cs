using NLog;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using TombLib.Utils;
using TombLib.Wad.Catalog;

namespace TombLib.Wad.TrLevels
{
    public static class TrLevelOperations
    {
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();

        public static Dictionary<int, WadTexture> ConvertTrLevelTexturesToWadTexture(TrLevel oldLevel)
        {
            var textures = new ConcurrentDictionary<int, WadTexture>();

            Parallel.For(0, oldLevel.ObjectTextures.Count, i =>
            {
                var oldTexture = oldLevel.ObjectTextures[i];
                var texture = new WadTexture();

                // Find the corners of the texture
                var minX = Math.Min(Math.Min(oldTexture.Vertices[0].Xp, oldTexture.Vertices[1].Xp), oldTexture.Vertices[2].Xp);
                if (oldTexture.Vertices[3].Xc != 0) minX = Math.Min(minX, oldTexture.Vertices[3].Xp);
                var minY = Math.Min(Math.Min(oldTexture.Vertices[0].Yp, oldTexture.Vertices[1].Yp), oldTexture.Vertices[2].Yp);
                if (oldTexture.Vertices[3].Yc != 0) minY = Math.Min(minY, oldTexture.Vertices[3].Yp);

                var maxX = Math.Max(Math.Max(oldTexture.Vertices[0].Xp, oldTexture.Vertices[1].Xp), oldTexture.Vertices[2].Xp);
                if (oldTexture.Vertices[3].Xc != 0) maxX = Math.Max(maxX, oldTexture.Vertices[3].Xp);
                var maxY = Math.Max(Math.Max(oldTexture.Vertices[0].Yp, oldTexture.Vertices[1].Yp), oldTexture.Vertices[2].Yp);
                if (oldTexture.Vertices[3].Yc != 0) maxY = Math.Max(maxY, oldTexture.Vertices[3].Yp);

                texture.PositionInOriginalTexturePage = new VectorInt2(minX, minY);

                var width = (int)(maxX - minX + 1);
                var height = (int)(maxY - minY + 1);
                var tile = oldTexture.TileAndFlags & 0xFF;

                // Create the texture ImageC
                var textureData = ImageC.CreateNew(width, height);

                for (int y = 0; y < textureData.Height; y++)
                {
                    for (int x = 0; x < textureData.Width; x++)
                    {
                        byte b = oldLevel.TextureMap32[tile * 65536 * 4 + (y + minY) * 1024 + (x + minX) * 4 + 0];
                        byte g = oldLevel.TextureMap32[tile * 65536 * 4 + (y + minY) * 1024 + (x + minX) * 4 + 1];
                        byte r = oldLevel.TextureMap32[tile * 65536 * 4 + (y + minY) * 1024 + (x + minX) * 4 + 2];
                        byte a = oldLevel.TextureMap32[tile * 65536 * 4 + (y + minY) * 1024 + (x + minX) * 4 + 3];

                        var color = new ColorC(r, g, b, a);
                        textureData.SetPixel(x, y, color);
                    }
                }

                texture.Image = textureData;
                
                // Update the hash of the texture
                texture.UpdateHash();

                textures.TryAdd(i, texture);

                i++;
            });

            return new Dictionary<int, WadTexture>(textures);
        }

        internal static WadMesh ConvertTrLevelMeshToWadMesh(Wad2 wad, TrLevel oldLevel, tr_mesh oldMesh,
                                                      Dictionary<int, WadTexture> convertedTextures)
        {
            WadMesh mesh = new WadMesh();

            int xMin = Int32.MaxValue;
            int yMin = Int32.MaxValue;
            int zMin = Int32.MaxValue;
            int xMax = Int32.MinValue;
            int yMax = Int32.MinValue;
            int zMax = Int32.MinValue;

            // Create the bounding sphere
            mesh.BoundingSphere = new BoundingSphere(new Vector3(oldMesh.Center.X, oldMesh.Center.Y, oldMesh.Center.Z),
                                                     oldMesh.Radius);

            // Add positions
            foreach (var oldVertex in oldMesh.Vertices)
            {
                mesh.VerticesPositions.Add(new Vector3(oldVertex.X, -oldVertex.Y, oldVertex.Z));

                if (oldVertex.X < xMin)
                    xMin = oldVertex.X;
                if (-oldVertex.Y < yMin)
                    yMin = -oldVertex.Y;
                if (oldVertex.Z < zMin)
                    zMin = oldVertex.Z;

                if (oldVertex.X > xMax)
                    xMax = oldVertex.X;
                if (-oldVertex.Y > yMax)
                    yMax = -oldVertex.Y;
                if (oldVertex.Z > zMax)
                    zMax = oldVertex.Z;
            }

            Vector3 minVertex = new Vector3(xMin, yMin, zMin);
            Vector3 maxVertex = new Vector3(xMax, yMax, zMax);

            // Add normals
            foreach (var oldNormal in oldMesh.Normals)
            {
                mesh.VerticesNormals.Add(new Vector3(oldNormal.X, -oldNormal.Y, oldNormal.Z));
            }

            // Add shades
            foreach (var oldShade in oldMesh.Lights)
            {
                mesh.VerticesShades.Add(oldShade);
            }

            // Add polygons
            foreach (var oldPoly in oldMesh.TexturedQuads)
            {
                WadPolygon poly = new WadPolygon(WadPolygonShape.Quad);

                // Polygon indices
                poly.Indices.Add(oldPoly.Vertices[0]);
                poly.Indices.Add(oldPoly.Vertices[1]);
                poly.Indices.Add(oldPoly.Vertices[2]);
                poly.Indices.Add(oldPoly.Vertices[3]);

                // Polygon special effects
                poly.ShineStrength = (byte)((oldPoly.LightingEffect & 0x7c) >> 2);

                // Add the texture
                TextureArea textureArea = new TextureArea();
                var textureId = oldPoly.Texture & 0x7fff;
                var newTexture = convertedTextures[textureId];
                var oldTexture = oldLevel.ObjectTextures[textureId];
                textureArea.DoubleSided = false;
                textureArea.BlendMode = ((oldPoly.LightingEffect & 0x01) == 0x01 ? BlendMode.Additive : (BlendMode)oldTexture.Attributes);
                if (wad.Textures.ContainsKey(newTexture.Hash))
                {
                    textureArea.Texture = wad.Textures[newTexture.Hash];
                }
                else
                {
                    wad.Textures.Add(newTexture.Hash, newTexture);
                    textureArea.Texture = newTexture;
                }
                textureArea.TexCoord0 = new Vector2(oldTexture.Vertices[0].Xp - newTexture.PositionInOriginalTexturePage.X + 0.5f,
                                                    oldTexture.Vertices[0].Yp - newTexture.PositionInOriginalTexturePage.Y + 0.5f);
                textureArea.TexCoord1 = new Vector2(oldTexture.Vertices[1].Xp - newTexture.PositionInOriginalTexturePage.X + 0.5f,
                                                    oldTexture.Vertices[1].Yp - newTexture.PositionInOriginalTexturePage.Y + 0.5f);
                textureArea.TexCoord2 = new Vector2(oldTexture.Vertices[2].Xp - newTexture.PositionInOriginalTexturePage.X + 0.5f,
                                                    oldTexture.Vertices[2].Yp - newTexture.PositionInOriginalTexturePage.Y + 0.5f);
                textureArea.TexCoord3 = new Vector2(oldTexture.Vertices[3].Xp - newTexture.PositionInOriginalTexturePage.X + 0.5f,
                                                    oldTexture.Vertices[3].Yp - newTexture.PositionInOriginalTexturePage.Y + 0.5f);
                poly.Texture = textureArea;
                poly.BlendMode = (BlendMode)oldTexture.Attributes;

                mesh.Polys.Add(poly);
            }

            foreach (var oldPoly in oldMesh.TexturedTriangles)
            {
                WadPolygon poly = new WadPolygon(WadPolygonShape.Triangle);

                // Polygon indices
                poly.Indices.Add(oldPoly.Vertices[0]);
                poly.Indices.Add(oldPoly.Vertices[1]);
                poly.Indices.Add(oldPoly.Vertices[2]);

                // Polygon special effects
                poly.ShineStrength = (byte)((oldPoly.LightingEffect & 0x7c) >> 2);

                // Add the texture
                var textureId = oldPoly.Texture & 0x7fff;
                var newTexture = convertedTextures[textureId];
                var oldTexture = oldLevel.ObjectTextures[textureId];
                TextureArea textureArea = new TextureArea();
                textureArea.DoubleSided = false;
                textureArea.BlendMode = ((oldPoly.LightingEffect & 0x01) == 0x01 ? BlendMode.Additive : (BlendMode)oldTexture.Attributes);
                if (wad.Textures.ContainsKey(newTexture.Hash))
                {
                    textureArea.Texture = wad.Textures[newTexture.Hash];
                }
                else
                {
                    wad.Textures.Add(newTexture.Hash, newTexture);
                    textureArea.Texture = newTexture;
                }
                textureArea.TexCoord0 = new Vector2(oldTexture.Vertices[0].Xp - newTexture.PositionInOriginalTexturePage.X + 0.5f,
                                                    oldTexture.Vertices[0].Yp - newTexture.PositionInOriginalTexturePage.Y + 0.5f);
                textureArea.TexCoord1 = new Vector2(oldTexture.Vertices[1].Xp - newTexture.PositionInOriginalTexturePage.X + 0.5f,
                                                    oldTexture.Vertices[1].Yp - newTexture.PositionInOriginalTexturePage.Y + 0.5f);
                textureArea.TexCoord2 = new Vector2(oldTexture.Vertices[2].Xp - newTexture.PositionInOriginalTexturePage.X + 0.5f,
                                                    oldTexture.Vertices[2].Yp - newTexture.PositionInOriginalTexturePage.Y + 0.5f);
                poly.Texture = textureArea;
                
                mesh.Polys.Add(poly);
            }

            foreach (var oldPoly in oldMesh.ColoredRectangles)
            {
                WadPolygon poly = new WadPolygon(WadPolygonShape.Quad);

                // Polygon indices
                poly.Indices.Add(oldPoly.Vertices[0]);
                poly.Indices.Add(oldPoly.Vertices[1]);
                poly.Indices.Add(oldPoly.Vertices[2]);
                poly.Indices.Add(oldPoly.Vertices[3]);

                // Add the colored surface
                TextureArea textureArea = new TextureArea();
                var paletteIndex8 = oldPoly.Texture & 0xff;
                textureArea.Texture = ConvertColoredFaceToTexture(wad, oldLevel, paletteIndex8);
                textureArea.TexCoord0 = new Vector2(0.5f, 0.5f);
                textureArea.TexCoord1 = new Vector2(3.5f, 0.5f);
                textureArea.TexCoord2 = new Vector2(3.5f, 3.5f);
                textureArea.TexCoord3 = new Vector2(0.5f, 3.5f);
                poly.Texture = textureArea;

                mesh.Polys.Add(poly);
            }

            foreach (var oldPoly in oldMesh.ColoredTriangles)
            {
                WadPolygon poly = new WadPolygon(WadPolygonShape.Triangle);

                // Polygon indices
                poly.Indices.Add(oldPoly.Vertices[0]);
                poly.Indices.Add(oldPoly.Vertices[1]);
                poly.Indices.Add(oldPoly.Vertices[2]);

                // Add the colored surface
                TextureArea textureArea = new TextureArea();
                var paletteIndex8 = oldPoly.Texture & 0xff;
                textureArea.Texture = ConvertColoredFaceToTexture(wad, oldLevel, paletteIndex8);
                textureArea.TexCoord0 = new Vector2(0.5f, 0.5f);
                textureArea.TexCoord1 = new Vector2(3.5f, 0.5f);
                textureArea.TexCoord2 = new Vector2(3.5f, 3.5f);
                poly.Texture = textureArea;

                mesh.Polys.Add(poly);
            }

            mesh.BoundingBox = new BoundingBox(minVertex, maxVertex);

            // Calculate hash
            mesh.UpdateHash();

            // Now add to the dictionary only if it doesn't contain a mesh with this hash
            if (wad.Meshes.ContainsKey(mesh.Hash))
            {
                return wad.Meshes[mesh.Hash];
            }
            else
            {
                wad.Meshes.Add(mesh.Hash, mesh);
                return mesh;
            }
        }

        private static WadTexture ConvertColoredFaceToTexture(Wad2 wad, TrLevel oldLevel, int palette8)
        {
            var texture = new WadTexture();
            var image = ImageC.CreateNew(4, 4);
            var color = oldLevel.Palette8[palette8];
            for (var x = 0; x < 4; x++)
                for (var y = 0; y < 4; y++)
                    image.SetPixel(x, y, new ColorC(color.Red, color.Green, color.Blue, 255));
            texture.Image = image;
            texture.UpdateHash();
            if (!wad.Textures.ContainsKey(texture.Hash))
                wad.Textures.Add(texture.Hash, texture);
            return wad.Textures[texture.Hash];
        }

        public static Wad2 ConvertTrLevel(TrLevel oldLevel)
        {
            Wad2 wad = new Wad2(GetTrVersion(oldLevel.Version), true);

            logger.Info("Converting TR level to WAD2");

            // First convert all textures
            Dictionary<int, WadTexture> textures = ConvertTrLevelTexturesToWadTexture(oldLevel);
            for (int i = 0; i < textures.Count; i++)
            {
                if (!wad.Textures.ContainsKey(textures.ElementAt(i).Value.Hash))
                    wad.Textures.Add(textures.ElementAt(i).Value.Hash, textures.ElementAt(i).Value);
            }
            logger.Info("Texture conversion complete.");

            // Then convert moveables and static meshes
            // Meshes will be converted inside each model
            for (int i = 0; i < oldLevel.Moveables.Count; i++)
            {
                ConvertTrLevelMoveableToWadMoveable(wad, oldLevel, i, textures);
            }
            logger.Info("Moveable conversion complete.");

            for (int i = 0; i < oldLevel.StaticMeshes.Count; i++)
            {
                ConvertTrLevelStaticMeshToWadStatic(wad, oldLevel, i, textures);
            }
            logger.Info("Static mesh conversion complete.");

            // Convert sounds
            ConvertTrLevelSounds(wad, oldLevel);
            logger.Info("Sound conversion complete.");

            // Convert sprites
            ConvertTrLevelSprites(wad, oldLevel);
            logger.Info("Sprite conversion complete.");

            return wad;
        }

        private static void ConvertTrLevelSprites(Wad2 wad, TrLevel oldLevel)
        {
            foreach (var oldSequence in oldLevel.SpriteSequences)
            {
                int lengthOfSequence = -oldSequence.NegativeLength;
                int startIndex = oldSequence.Offset;

                var newSequence = new WadSpriteSequence();
                newSequence.ObjectID = (uint)oldSequence.ObjectID;
                newSequence.Name = TrCatalog.GetSpriteName(GetTrVersion(oldLevel.Version), (uint)oldSequence.ObjectID);

                for (int i = startIndex; i < startIndex + lengthOfSequence; i++)
                {
                    var oldSpriteTexture = oldLevel.SpriteTextures[i];

                    int spriteWidth = 0;
                    int spriteHeight = 0;
                    int spriteX = 0;
                    int spriteY = 0;

                    if (oldLevel.Version == TrVersion.TR1 || oldLevel.Version == TrVersion.TR2 || oldLevel.Version == TrVersion.TR3)
                    {
                        spriteX = oldSpriteTexture.X;
                        spriteY = oldSpriteTexture.Y;
                        spriteWidth = (oldSpriteTexture.Width - 255) / 256;
                        spriteHeight = (oldSpriteTexture.Height - 255) / 256;
                    }
                    else
                    {
                        spriteX = oldSpriteTexture.LeftSide;
                        spriteY = oldSpriteTexture.TopSide;
                        spriteWidth = (oldSpriteTexture.Width / 256) + 1;
                        spriteHeight = (oldSpriteTexture.Height / 256) + 1;
                    }

                    var spriteImage = ImageC.CreateNew(spriteWidth, spriteHeight);

                    for (int y = 0; y < spriteHeight; y++)
                        for (int x = 0; x < spriteWidth; x++)
                        {
                            byte b = oldLevel.TextureMap32[oldSpriteTexture.Tile * 65536 * 4 + (spriteY + y) * 1024 + (spriteX + x) * 4 + 0];
                            byte g = oldLevel.TextureMap32[oldSpriteTexture.Tile * 65536 * 4 + (spriteY + y) * 1024 + (spriteX + x) * 4 + 1];
                            byte r = oldLevel.TextureMap32[oldSpriteTexture.Tile * 65536 * 4 + (spriteY + y) * 1024 + (spriteX + x) * 4 + 2];
                            byte a = oldLevel.TextureMap32[oldSpriteTexture.Tile * 65536 * 4 + (spriteY + y) * 1024 + (spriteX + x) * 4 + 3];

                            spriteImage.SetPixel(x, y, new ColorC(r, g, b, a));
                        }

                    // Create the texture
                    var texture = new WadSprite();
                    texture.Image = spriteImage;
                    texture.UpdateHash();

                    // Check if texture already exists in Wad2 and eventually add it
                    if (wad.SpriteTextures.ContainsKey(texture.Hash))
                        texture = wad.SpriteTextures[texture.Hash];
                    else
                        wad.SpriteTextures.Add(texture.Hash, texture);

                    // Add current sprite to the sequence
                    newSequence.Sprites.Add(texture);
                }

                wad.SpriteSequences.Add(newSequence);
            }
        }

        private static void ConvertTrLevelSounds(Wad2 wad, TrLevel oldLevel)
        {
            for (int i = 0; i < oldLevel.SoundMap.Count; i++)
            {
                // Check if sound was used
                if (oldLevel.SoundMap[i] == -1) continue;

                var oldInfo = oldLevel.SoundDetails[oldLevel.SoundMap[i]];
                var newInfo = new WadSoundInfo();

                // Fill the new sound info
                newInfo.Name = TrCatalog.GetSoundName(GetTrVersion(oldLevel.Version), (uint)i);
                newInfo.Volume = (byte)oldInfo.Volume;
                newInfo.Range = oldInfo.Range;
                newInfo.Chance = (byte)oldInfo.Chance;
                newInfo.Pitch = oldInfo.Pitch;
                newInfo.RandomizePitch = ((oldInfo.Characteristics & 0x2000) != 0); // TODO: loop meaning changed between TR versions
                newInfo.RandomizeGain = ((oldInfo.Characteristics & 0x4000) != 0);
                newInfo.FlagN = ((oldInfo.Characteristics & 0x1000) != 0);
                newInfo.Loop = (WadSoundLoopType)(oldInfo.Characteristics & 0x03);

                int numSamplesInGroup = (oldInfo.Characteristics & 0x00fc) >> 2;

                // Read all samples linked to this sound info (for example footstep has 4 samples)
                for (int j = oldInfo.Sample; j < oldInfo.Sample + numSamplesInGroup; j++)
                {
                    var soundName = j + ".wav";

                    if (j < oldLevel.Samples.Count)
                    {
                        var theSoundIndex = 0;
                        if (oldLevel.Version == TrVersion.TR2 || oldLevel.Version == TrVersion.TR3)
                            theSoundIndex = (int)oldLevel.SamplesIndices[j];
                        else
                            theSoundIndex = j;

                        var sound = new WadSample(soundName, oldLevel.Samples[theSoundIndex].Data);
                        if (wad.Samples.ContainsKey(sound.Hash))
                        {
                            newInfo.Samples.Add(wad.Samples[sound.Hash]);
                        }
                        else
                        {
                            wad.Samples.Add(sound.Hash, sound);
                            newInfo.Samples.Add(sound);
                        }
                    }
                    else
                    {
                        logger.Warn("Unable to find sample " + j);
                    }
                }

                newInfo.UpdateHash();

                wad.SoundInfo.Add((ushort)i, newInfo);
            }
        }

        public static WadMoveable ConvertTrLevelMoveableToWadMoveable(Wad2 wad, TrLevel oldLevel, int moveableIndex,
                                                                      Dictionary<int, WadTexture> textures)
        {
            WadMoveable moveable = new WadMoveable(wad);
            var m = oldLevel.Moveables[moveableIndex];

            moveable.ObjectID = m.ObjectID;
            //moveable.Name = TrCatalog.GetMoveableName(GetTrVersion(oldLevel.Version), m.ObjectID);

            // First I build a list of meshes for this moveable
            var meshes = new List<tr_mesh>();
            for (int j = 0; j < m.NumMeshes; j++)
                meshes.Add(oldLevel.Meshes[(int)oldLevel.RealPointers[(int)(m.StartingMesh + j)]]);

            // Then I convert them to WadMesh
            foreach (var oldMesh in meshes)
            {
                WadMesh newMesh = ConvertTrLevelMeshToWadMesh(wad, oldLevel, oldMesh, textures);
                moveable.Meshes.Add(newMesh);
            }

            int currentLink = (int)m.MeshTree;

            moveable.Offset = Vector3.Zero;

            // Build the skeleton
            for (int j = 0; j < meshes.Count - 1; j++)
            {
                WadLink link = new WadLink((WadLinkOpcode)oldLevel.MeshTrees[currentLink],
                                           new Vector3(oldLevel.MeshTrees[currentLink + 1],
                                                       -oldLevel.MeshTrees[currentLink + 2],
                                                       oldLevel.MeshTrees[currentLink + 3]));

                currentLink += 4;

                moveable.Links.Add(link);
            }

            // Convert animations
            int numAnimations = 0;
            int nextMoveable = oldLevel.GetNextMoveableWithAnimations(moveableIndex);

            if (nextMoveable == -1)
                numAnimations = oldLevel.Animations.Count - m.Animation;
            else
                numAnimations = oldLevel.Moveables[nextMoveable].Animation - m.Animation;

            for (int j = 0; j < numAnimations; j++)
            {
                if (m.Animation == -1)
                    break;

                WadAnimation animation = new WadAnimation();
                var anim = oldLevel.Animations[j + m.Animation];
                animation.Acceleration = anim.Accel;
                animation.Speed = anim.Speed;
                animation.LateralSpeed = anim.SpeedLateral;
                animation.LateralAcceleration = anim.AccelLateral;
                animation.FrameDuration = anim.FrameRate;
                animation.FrameStart = anim.FrameStart;
                animation.FrameEnd = anim.FrameEnd;
                animation.NextAnimation = (ushort)(anim.NextAnimation - m.Animation);
                animation.NextFrame = anim.NextFrame;
                animation.StateId = anim.StateID;
                animation.RealNumberOfFrames = (ushort)(anim.FrameEnd - anim.FrameStart + 1);
                animation.Name = "Animation " + j;

                for (int k = 0; k < anim.NumStateChanges; k++)
                {
                    WadStateChange sc = new WadStateChange();
                    var wadSc = oldLevel.StateChanges[(int)anim.StateChangeOffset + k];
                    sc.StateId = wadSc.StateID;

                    for (int n = 0; n < wadSc.NumAnimDispatches; n++)
                    {
                        WadAnimDispatch ad = new WadAnimDispatch();
                        var wadAd = oldLevel.AnimDispatches[(int)wadSc.AnimDispatch + n];

                        ad.InFrame = (ushort)(wadAd.Low - anim.FrameStart);
                        ad.OutFrame = (ushort)(wadAd.High - anim.FrameStart);
                        ad.NextAnimation = (ushort)((wadAd.NextAnimation - m.Animation) % numAnimations);
                        ad.NextFrame = (ushort)wadAd.NextFrame;

                        sc.Dispatches.Add(ad);
                    }

                    animation.StateChanges.Add(sc);
                }

                if (anim.NumAnimCommands < oldLevel.AnimCommands.Count)
                {
                    int lastCommand = anim.AnimCommand;

                    for (int k = 0; k < anim.NumAnimCommands; k++)
                    {
                        short commandType = oldLevel.AnimCommands[lastCommand + 0];

                        // Ignore invalid anim commands (see for example karnak.wad)
                        if (commandType < 1 || commandType > 6)
                            continue;

                        WadAnimCommand command = new WadAnimCommand((WadAnimCommandType)commandType);

                        switch (commandType)
                        {
                            case 1:
                                command.Parameter1 = (ushort)oldLevel.AnimCommands[lastCommand + 1];
                                command.Parameter2 = (ushort)oldLevel.AnimCommands[lastCommand + 2];
                                command.Parameter3 = (ushort)oldLevel.AnimCommands[lastCommand + 3];

                                lastCommand += 4;
                                break;

                            case 2:
                                command.Parameter1 = (ushort)oldLevel.AnimCommands[lastCommand + 1];
                                command.Parameter2 = (ushort)oldLevel.AnimCommands[lastCommand + 2];

                                lastCommand += 3;
                                break;

                            case 3:
                                lastCommand += 1;
                                break;

                            case 4:
                                lastCommand += 1;
                                break;

                            case 5:
                                command.Parameter1 = (ushort)(oldLevel.AnimCommands[lastCommand + 1] - anim.FrameStart);
                                command.Parameter2 = (ushort)oldLevel.AnimCommands[lastCommand + 2];
                                lastCommand += 3;
                                break;

                            case 6:
                                command.Parameter1 = (ushort)(oldLevel.AnimCommands[lastCommand + 1] - anim.FrameStart);
                                command.Parameter2 = (ushort)oldLevel.AnimCommands[lastCommand + 2];
                                lastCommand += 3;
                                break;
                        }

                        animation.AnimCommands.Add(command);
                    }
                }

                int frames = (int)anim.FrameOffset / 2;
                uint numFrames;

                if (j + m.Animation == oldLevel.Animations.Count - 1)
                {
                    if (anim.FrameSize == 0)
                        numFrames = 0;
                    else
                        numFrames = ((uint)(2 * oldLevel.Frames.Count) - anim.FrameOffset) / (uint)(2 * anim.FrameSize);
                }
                else
                {
                    if (anim.FrameSize == 0)
                    {
                        numFrames = 0;
                    }
                    else
                    {
                        numFrames = (oldLevel.Animations[m.Animation + j + 1].FrameOffset - anim.FrameOffset) / (uint)(2 * anim.FrameSize);
                    }
                }

                for (int f = 0; f < numFrames; f++)
                {
                    WadKeyFrame frame = new WadKeyFrame();
                    int startOfFrame = frames;

                    frame.BoundingBox = new BoundingBox(new Vector3(oldLevel.Frames[frames],
                                                                    -oldLevel.Frames[frames + 2],
                                                                    oldLevel.Frames[frames + 4]),
                                                        new Vector3(oldLevel.Frames[frames + 1],
                                                                    -oldLevel.Frames[frames + 3],
                                                                    oldLevel.Frames[frames + 5]));

                    frames += 6;

                    frame.Offset = new Vector3(oldLevel.Frames[frames],
                                               (short)(-oldLevel.Frames[frames + 1]),
                                               oldLevel.Frames[frames + 2]);

                    frames += 3;

                    // TR1 has also the number of angles to follow
                    if (oldLevel.Version == TrVersion.TR1) frames++;

                    for (int n = 0; n < m.NumMeshes; n++)
                    {
                        short rot = oldLevel.Frames[frames];
                        WadKeyFrameRotation kfAngle = new WadKeyFrameRotation();

                        if (oldLevel.Version == TrVersion.TR1)
                        {
                            int rotation = rot;
                            int rotation2 = oldLevel.Frames[frames + 1];

                            frames += 2;

                            int rotX = (int)((rotation & 0x3ff0) >> 4);
                            int rotZ = (int)(((rotation2 & 0xfc00) >> 10) + ((rotation & 0xf) << 6) & 0x3ff);
                            int rotY = (int)((rotation2) & 0x3ff);

                            kfAngle.Axis = WadKeyFrameRotationAxis.ThreeAxes;
                            kfAngle.X = rotX;
                            kfAngle.Y = rotY;
                            kfAngle.Z = rotZ;

                            break;

                        }
                        else
                        {
                            switch (rot & 0xc000)
                            {
                                case 0:
                                    int rotation = rot;
                                    int rotation2 = oldLevel.Frames[frames + 1];

                                    frames += 2;

                                    int rotX = (int)((rotation & 0x3ff0) >> 4);
                                    int rotY = (int)(((rotation2 & 0xfc00) >> 10) + ((rotation & 0xf) << 6) & 0x3ff);
                                    int rotZ = (int)((rotation2) & 0x3ff);

                                    kfAngle.Axis = WadKeyFrameRotationAxis.ThreeAxes;
                                    kfAngle.X = rotX;
                                    kfAngle.Y = rotY;
                                    kfAngle.Z = rotZ;

                                    break;

                                case 0x4000:
                                    frames += 1;
                                    int rotationX;
                                    if (oldLevel.Version == TrVersion.TR4 || oldLevel.Version == TrVersion.TR5)
                                        rotationX = rot & 0xfff;
                                    else
                                        rotationX = (rot & 0x3ff) * 4;

                                    kfAngle.Axis = WadKeyFrameRotationAxis.AxisX;
                                    kfAngle.X = rotationX;

                                    break;

                                case 0x8000:
                                    frames += 1;
                                    int rotationY;
                                    if (oldLevel.Version == TrVersion.TR4 || oldLevel.Version == TrVersion.TR5)
                                        rotationY = rot & 0xfff;
                                    else
                                        rotationY = (rot & 0x3ff) * 4;

                                    kfAngle.Axis = WadKeyFrameRotationAxis.AxisY;
                                    kfAngle.Y = rotationY;

                                    break;

                                case 0xc000:
                                    frames += 1;
                                    int rotationZ;
                                    if (oldLevel.Version == TrVersion.TR4 || oldLevel.Version == TrVersion.TR5)
                                        rotationZ = rot & 0xfff;
                                    else
                                        rotationZ = (rot & 0x3ff) * 4;

                                    kfAngle.Axis = WadKeyFrameRotationAxis.AxisZ;
                                    kfAngle.Z = rotationZ;

                                    break;
                            }
                        }

                        frame.Angles.Add(kfAngle);
                    }

                    if ((frames - startOfFrame) < anim.FrameSize)
                        frames += ((int)anim.FrameSize - (frames - startOfFrame));

                    animation.KeyFrames.Add(frame);
                }

                // TODO: check if this hack work well
                // In original WADs animations with no keyframes had some random FrameEnd values
                if (animation.KeyFrames.Count == 0)
                {
                    animation.FrameEnd = anim.FrameStart;
                }

                animation.FrameBase = animation.FrameStart;

                moveable.Animations.Add(animation);
            }

            for (int i = 0; i < moveable.Animations.Count; i++)
            {
                var animation = moveable.Animations[i];

                if (animation.KeyFrames.Count == 0) animation.RealNumberOfFrames = 0;

                // HACK: this fixes some invalid NextAnimations values
                animation.NextAnimation %= (ushort)moveable.Animations.Count;

                ushort baseFrame = animation.FrameStart;

                // Frames become relative to current animation
                animation.FrameEnd -= baseFrame;
                animation.FrameStart -= baseFrame;

                moveable.Animations[i] = animation;
            }

            for (int i = 0; i < moveable.Animations.Count; i++)
            {
                var animation = moveable.Animations[i];

                // HACK: this fixes some invalid NextFrame values
                if (moveable.Animations[animation.NextAnimation].FrameBase != 0)
                    animation.NextFrame %= moveable.Animations[animation.NextAnimation].FrameBase;

                foreach (var stateChange in animation.StateChanges)
                {
                    foreach (var animDispatch in stateChange.Dispatches)
                    {
                        if (moveable.Animations[animDispatch.NextAnimation].FrameBase != 0)
                        {
                            ushort newFrame = (ushort)(animDispatch.NextFrame % moveable.Animations[animDispatch.NextAnimation].FrameBase);

                            // In some cases dispatches have invalid NextFrame.
                            // From tests it seems that's ok to delete the dispatch or put the NextFrame equal to zero.
                            if (newFrame > moveable.Animations[animDispatch.NextAnimation].RealNumberOfFrames) newFrame = 0;

                            animDispatch.NextFrame = newFrame;
                        }
                    }
                }

                moveable.Animations[i] = animation;
            }

            wad.Moveables.Add(m.ObjectID, moveable);

            return moveable;
        }

        internal static WadTombRaiderVersion GetTrVersion(TrVersion version)
        {
            if (version == TrVersion.TR1)
                return WadTombRaiderVersion.TR1;
            else if (version == TrVersion.TR2)
                return WadTombRaiderVersion.TR2;
            else if (version == TrVersion.TR3)
                return WadTombRaiderVersion.TR3;
            else if (version == TrVersion.TR4)
                return WadTombRaiderVersion.TR4;
            else
                return WadTombRaiderVersion.TR5;
        }

        public static WadStatic ConvertTrLevelStaticMeshToWadStatic(Wad2 wad, TrLevel oldLevel, int staticIndex,
                                                                Dictionary<int, WadTexture> textures)
        {
            var staticMesh = new WadStatic(wad);
            var oldStaticMesh = oldLevel.StaticMeshes[staticIndex];

            //staticMesh.Name = TrCatalog.GetStaticName(GetTrVersion(oldLevel.Version), oldStaticMesh.ObjectID);

            // First setup collisional and visibility bounding boxes
            staticMesh.CollisionBox = new BoundingBox(new Vector3(oldStaticMesh.CollisionBox.X1,
                                                                  -oldStaticMesh.CollisionBox.Y1,
                                                                  oldStaticMesh.CollisionBox.Z1),
                                                      new Vector3(oldStaticMesh.CollisionBox.X2,
                                                                  -oldStaticMesh.CollisionBox.Y2,
                                                                  oldStaticMesh.CollisionBox.Z2));

            staticMesh.VisibilityBox = new BoundingBox(new Vector3(oldStaticMesh.VisibilityBox.X1,
                                                                   -oldStaticMesh.VisibilityBox.Y1,
                                                                   oldStaticMesh.VisibilityBox.Z1),
                                                       new Vector3(oldStaticMesh.VisibilityBox.X2,
                                                                   -oldStaticMesh.VisibilityBox.Y2,
                                                                   oldStaticMesh.VisibilityBox.Z2));

            // Then import the mesh. If it was already added, the mesh will not be added to the dictionary.
            staticMesh.Mesh = ConvertTrLevelMeshToWadMesh(wad,
                                                      oldLevel,
                                                      oldLevel.GetMeshFromPointer(oldStaticMesh.Mesh),
                                                      textures);

            staticMesh.ObjectID = oldStaticMesh.ObjectID;

            wad.Statics.Add(staticMesh.ObjectID, staticMesh);

            return staticMesh;
        }
    }
}
