using NLog;
using System;
using System.Collections.Generic;
using System.Numerics;
using System.Threading.Tasks;
using TombLib.LevelData;
using TombLib.Utils;
using TombLib.Wad.Catalog;

namespace TombLib.Wad.TrLevels
{
    internal static class TrLevelOperations
    {
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();

        public static Wad2 ConvertTrLevel(TrLevel oldLevel)
        {
            var wad = new Wad2() { GameVersion = oldLevel.Version, SoundSystem = SoundSystem.Xml } ;

            logger.Info("Converting TR level to WAD2");

            // Convert textures
            TextureArea[] objectTextures = ConvertTrLevelTexturesToWadTexture(oldLevel);
            logger.Info("Texture conversion complete.");

            // Then convert moveables and static meshes
            // Meshes will be converted inside each model
            for (int i = 0; i < oldLevel.Moveables.Count; i++)
            {
                WadMoveable moveable = ConvertTrLevelMoveableToWadMoveable(wad, oldLevel, i, objectTextures);
                wad.Moveables.Add(moveable.Id, moveable);
            }
            logger.Info("Moveable conversion complete.");

            for (int i = 0; i < oldLevel.StaticMeshes.Count; i++)
            {
                WadStatic @static = ConvertTrLevelStaticMeshToWadStatic(wad, oldLevel, i, objectTextures);
                wad.Statics.Add(@static.Id, @static);
            }
            logger.Info("Static mesh conversion complete.");

            // Convert sprites
            ConvertTrLevelSprites(wad, oldLevel);
            logger.Info("Sprite conversion complete.");

            return wad;
        }

        private static readonly Vector2[] _zero = new Vector2[4] { new Vector2(0.0f, 0.0f), new Vector2(0.0f, 0.0f), new Vector2(0.0f, 0.0f), new Vector2(0.0f, 0.0f) };

        private static TextureArea[] ConvertTrLevelTexturesToWadTexture(TrLevel oldLevel)
        {
            var objectTextures = new TextureArea[oldLevel.ObjectTextures.Count];
            ImageC tiles = ImageC.FromByteArray(oldLevel.TextureMap32, 256, oldLevel.TextureMap32.Length / 1024);
             tiles.ReplaceColor(new ColorC(255, 0, 255, 255), new ColorC(0, 0, 0, 0));

            // for (int i = 0; i < oldLevel.ObjectTextures.Count; ++i)
            Parallel.For(0, oldLevel.ObjectTextures.Count, i =>
            {
                var oldTexture = oldLevel.ObjectTextures[i];

                int textureTileIndex = oldTexture.TileAndFlags & 0x7fff;
                bool isTriangle = (oldTexture.TileAndFlags & 0x8000) != 0; // Exists only in TR4+

                if (oldLevel.Version == TRVersion.Game.TR1 || oldLevel.Version == TRVersion.Game.TR2 || oldLevel.Version == TRVersion.Game.TR3)
                    isTriangle = (oldTexture.Vertices[3].X == 0) && (oldTexture.Vertices[3].Y == 0);

                // Calculate UV coordinates...
                Vector2[] coords = new Vector2[isTriangle ? 3 : 4];
                for (int j = 0; j < coords.Length; ++j)
                    coords[j] = new Vector2(oldTexture.Vertices[j].X, oldTexture.Vertices[j].Y) * (1.0f / 256.0f);

                // Find the corners of the texture
                Vector2 min = coords[0], max = coords[0];
                for (int j = 1; j < coords.Length; ++j)
                {
                    min = Vector2.Min(min, coords[j]);
                    max = Vector2.Max(max, coords[j]);
                }

                VectorInt2 start = VectorInt2.FromFloor(min);
                VectorInt2 end = VectorInt2.FromCeiling(max);
                start = VectorInt2.Min(VectorInt2.Max(start, new VectorInt2()), new VectorInt2(256, 256));
                end = VectorInt2.Min(VectorInt2.Max(end, new VectorInt2()), new VectorInt2(256, 256));

                // Create image
                ImageC image = ImageC.CreateNew(end.X - start.X, end.Y - start.Y);
                image.CopyFrom(0, 0, tiles, start.X, start.Y + textureTileIndex * 256, end.X - start.X, end.Y - start.Y);

                // Replace black with transparent color
                //image.ReplaceColor(new ColorC(0, 0, 0, 255), new ColorC(0, 0, 0, 0));

                WadTexture texture = new WadTexture(image);

                // Create texture area
                TextureArea textureArea = new TextureArea();
                textureArea.DoubleSided = false;
                textureArea.BlendMode = (BlendMode)(oldTexture.Attributes);
                textureArea.TexCoord0 = coords[0] - start;
                textureArea.TexCoord1 = coords[1] - start;
                textureArea.TexCoord2 = coords[2] - start;
                textureArea.TexCoord3 = isTriangle ? textureArea.TexCoord2 : (coords[3] - start);
                textureArea.Texture = texture;

                objectTextures[i] = textureArea;
            });

            return objectTextures;
        }

        private static WadMesh ConvertTrLevelMeshToWadMesh(Wad2 wad, TrLevel oldLevel, tr_mesh oldMesh, TextureArea[] objectTextures)
        {
            WadMesh mesh = new WadMesh();

            mesh.Name = "Mesh_" + oldLevel.Meshes.IndexOf(oldMesh);

            // Add positions
            foreach (var oldVertex in oldMesh.Vertices)
            {
                mesh.VerticesPositions.Add(new Vector3(oldVertex.X, -oldVertex.Y, oldVertex.Z));
            }

            // Create the bounding areas
            mesh.BoundingSphere = new BoundingSphere(new Vector3(oldMesh.Center.X, oldMesh.Center.Y, oldMesh.Center.Z), oldMesh.Radius);
            mesh.BoundingBox = mesh.CalculateBoundingBox();

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
                TextureArea textureArea = objectTextures[oldPoly.Texture & 0x7fff];
                textureArea.DoubleSided = (oldPoly.Texture & 0x8000) != 0;
                if ((oldPoly.LightingEffect & 1) != 0)
                    textureArea.BlendMode = BlendMode.Additive;

                WadPolygon poly;
                poly.Shape = WadPolygonShape.Quad;
                poly.Index0 = oldPoly.Index0;
                poly.Index1 = oldPoly.Index1;
                poly.Index2 = oldPoly.Index2;
                poly.Index3 = oldPoly.Index3;
                poly.ShineStrength = (byte)((oldPoly.LightingEffect & 0x7c) >> 2);
                poly.Texture = textureArea;
                mesh.Polys.Add(poly);
            }
            foreach (var oldPoly in oldMesh.TexturedTriangles)
            {
                TextureArea textureArea = objectTextures[oldPoly.Texture & 0x7fff];
                textureArea.DoubleSided = (oldPoly.Texture & 0x8000) != 0;
                if ((oldPoly.LightingEffect & 1) != 0)
                    textureArea.BlendMode = BlendMode.Additive;

                WadPolygon poly = new WadPolygon();
                poly.Shape = WadPolygonShape.Triangle;
                poly.Index0 = oldPoly.Index0;
                poly.Index1 = oldPoly.Index1;
                poly.Index2 = oldPoly.Index2;
                poly.Index3 = 0;
                poly.ShineStrength = (byte)((oldPoly.LightingEffect & 0x7c) >> 2);
                poly.Texture = textureArea;
                mesh.Polys.Add(poly);
            }

            foreach (var oldPoly in oldMesh.ColoredRectangles)
            {
                WadPolygon poly = new WadPolygon();
                poly.Shape = WadPolygonShape.Quad;
                poly.Index0 = oldPoly.Index0;
                poly.Index1 = oldPoly.Index1;
                poly.Index2 = oldPoly.Index2;
                poly.Index3 = oldPoly.Index3;
                poly.Texture = ConvertColoredFaceToTexture(wad, oldLevel, oldPoly.Texture);
                poly.ShineStrength = 0;
                mesh.Polys.Add(poly);
            }

            foreach (var oldPoly in oldMesh.ColoredTriangles)
            {
                WadPolygon poly;
                poly.Shape = WadPolygonShape.Triangle;
                poly.Index0 = oldPoly.Index0;
                poly.Index1 = oldPoly.Index1;
                poly.Index2 = oldPoly.Index2;
                poly.Index3 = 0;
                poly.Texture = ConvertColoredFaceToTexture(wad, oldLevel, oldPoly.Texture);
                poly.ShineStrength = 0;
                mesh.Polys.Add(poly);
            }

            // Usually only for static meshes
            if (mesh.VerticesNormals.Count == 0)
                mesh.CalculateNormals();

            return mesh;
        }

        private static TextureArea ConvertColoredFaceToTexture(Wad2 wad, TrLevel oldLevel, ushort paletteIndex, int textureSize = 2, int margin = 1)
        {
            if (textureSize < 1) textureSize = 1;
            if (margin < 1) margin = 1;

            ColorC color;

            var sixteenBitIndex = paletteIndex >> 8;
            if (oldLevel.Palette16.Count > 0 && oldLevel.Palette16.Count == oldLevel.Palette8.Count && sixteenBitIndex < oldLevel.Palette16.Count)
            {
                var trColor = oldLevel.Palette16[sixteenBitIndex];
                color = new ColorC(trColor.Red, trColor.Green, trColor.Blue, 255);
            }
            else
            {
                var trColor = oldLevel.Palette8[paletteIndex & 0xFF];
                color = new ColorC((byte)(trColor.Red * 4), (byte)(trColor.Green * 4), (byte)(trColor.Blue * 4), 255);
            }

            var image = ImageC.CreateNew(textureSize + margin * 2, textureSize + margin * 2);
            image.Fill(color);

            TextureArea textureArea = new TextureArea();
            textureArea.Texture = new WadTexture(image);
            textureArea.TexCoord0 = new Vector2(margin, margin);
            textureArea.TexCoord1 = new Vector2(image.Width - margin, margin);
            textureArea.TexCoord2 = new Vector2(image.Width - margin, image.Height - margin);
            textureArea.TexCoord3 = new Vector2(2, image.Height - margin);
            textureArea.BlendMode = BlendMode.Normal;
            textureArea.DoubleSided = false;
            return textureArea;
        }

        private static void ConvertTrLevelSprites(Wad2 wad, TrLevel oldLevel)
        {
            ImageC tiles = ImageC.FromByteArray(oldLevel.TextureMap32, 256, oldLevel.TextureMap32.Length / 1024);

            foreach (var oldSequence in oldLevel.SpriteSequences)
            {
                int lengthOfSequence = -oldSequence.NegativeLength;
                int startIndex = oldSequence.Offset;

                var newSequence = new WadSpriteSequence(new WadSpriteSequenceId((uint)oldSequence.ObjectID));
                for (int i = startIndex; i < startIndex + lengthOfSequence; i++)
                {
                    tr_sprite_texture oldSpriteTexture = oldLevel.SpriteTextures[i];

                    int spriteX, spriteY, spriteWidth, spriteHeight;

                    int x1 = oldSpriteTexture.LeftSide;
                    int x2 = oldSpriteTexture.RightSide;
                    int y1 = oldSpriteTexture.TopSide;
                    int y2 = oldSpriteTexture.BottomSide;

                    if (oldLevel.Version == TRVersion.Game.TR1 ||
                        oldLevel.Version == TRVersion.Game.TR2 ||
                        oldLevel.Version == TRVersion.Game.TR3)
                    {
                        spriteX = oldSpriteTexture.X;
                        spriteY = oldSpriteTexture.Y;
                        spriteWidth = ((oldSpriteTexture.Width - 255) / 256 + 1);
                        spriteHeight = ((oldSpriteTexture.Height - 255) / 256 + 1);
                        
                    }
                    else
                    {
                        spriteX = oldSpriteTexture.LeftSide;
                        spriteY = oldSpriteTexture.TopSide;
                        spriteWidth = ((oldSpriteTexture.Width / 256) + 1);
                        spriteHeight = ((oldSpriteTexture.Height / 256) + 1);
                    }

                    // Add current sprite to the sequence
                    var spriteImage = ImageC.CreateNew(spriteWidth, spriteHeight);
                    spriteImage.CopyFrom(0, 0, tiles, spriteX, spriteY + oldSpriteTexture.Tile * 256, spriteWidth, spriteHeight);
                    RectangleInt2 rect = new RectangleInt2(x1, y1, x2, y2);
                    newSequence.Sprites.Add(new WadSprite {
                        Texture = new WadTexture(spriteImage),
                        Alignment = rect
                    });
                }

                wad.SpriteSequences.Add(newSequence.Id, newSequence);
            }
        }

        public static WadMoveable ConvertTrLevelMoveableToWadMoveable(Wad2 wad, TrLevel oldLevel, int moveableIndex,
                                                                      TextureArea[] objectTextures)
        {
            Console.WriteLine("Converting Moveable " + moveableIndex);

            var oldMoveable = oldLevel.Moveables[moveableIndex];
            WadMoveable newMoveable = new WadMoveable(new WadMoveableId(oldMoveable.ObjectID));

            // First a list of meshes for this moveable is built
            var oldMeshes = new List<tr_mesh>();
            for (int j = 0; j < oldMoveable.NumMeshes; j++)
                oldMeshes.Add(oldLevel.Meshes[(int)oldLevel.RealPointers[(int)(oldMoveable.StartingMesh + j)]]);

            // Convert the WadMesh
            var newMeshes = new List<WadMesh>();
            foreach (var oldMesh in oldMeshes)
                newMeshes.Add(ConvertTrLevelMeshToWadMesh(wad, oldLevel, oldMesh, objectTextures));

            // Build the skeleton
            var root = new WadBone();
            root.Name = "bone_0_root";
            root.Parent = null;
            root.Translation = Vector3.Zero;
            root.Mesh = newMeshes[0];

            newMoveable.Bones.Add(root);

            for (int j = 0; j < oldMoveable.NumMeshes - 1; j++)
            {
                var bone = new WadBone();
                bone.Name = "bone_" + (j + 1).ToString();
                bone.Parent = null;
                bone.Translation = Vector3.Zero;
                bone.Mesh = newMeshes[j + 1];
                newMoveable.Bones.Add(bone);
            }

            for (int mi = 0; mi < (oldMeshes.Count - 1); mi++)
            {
                int j = mi + 1;

                var opcode = (WadLinkOpcode)oldLevel.MeshTrees[(int)(oldMoveable.MeshTree + mi * 4)];
                int linkX = oldLevel.MeshTrees[(int)(oldMoveable.MeshTree + mi * 4) + 1];
                int linkY = -oldLevel.MeshTrees[(int)(oldMoveable.MeshTree + mi * 4) + 2];
                int linkZ = oldLevel.MeshTrees[(int)(oldMoveable.MeshTree + mi * 4) + 3];

                newMoveable.Bones[j].OpCode = opcode;
                newMoveable.Bones[j].Translation = new Vector3(linkX, linkY, linkZ);
            }

            // Convert animations
            int numAnimations = 0;
            int nextMoveable = oldLevel.GetNextMoveableWithAnimations(moveableIndex);

            if (nextMoveable == -1)
                numAnimations = oldLevel.Animations.Count - oldMoveable.Animation;
            else
                numAnimations = oldLevel.Moveables[nextMoveable].Animation - oldMoveable.Animation;

            var frameBases = new Dictionary<WadAnimation, ushort>();
            for (int j = 0; j < numAnimations; j++)
            {
                if (oldMoveable.Animation == -1)
                    break;

                WadAnimation newAnimation = new WadAnimation();
                var oldAnimation = oldLevel.Animations[j + oldMoveable.Animation];
                newAnimation.FrameRate = oldAnimation.FrameRate;
                newAnimation.NextAnimation = (ushort)(oldAnimation.NextAnimation - oldMoveable.Animation);
                newAnimation.NextFrame = oldAnimation.NextFrame;
                newAnimation.StateId = oldAnimation.StateID;
                newAnimation.EndFrame = (ushort)(oldAnimation.FrameEnd - oldAnimation.FrameStart);
                newAnimation.Name = TrCatalog.GetAnimationName(oldLevel.Version, oldMoveable.ObjectID, (uint)j);

                for (int k = 0; k < oldAnimation.NumStateChanges; k++)
                {
                    WadStateChange sc = new WadStateChange();
                    var wadSc = oldLevel.StateChanges[(int)oldAnimation.StateChangeOffset + k];
                    sc.StateId = wadSc.StateID;

                    for (int n = 0; n < wadSc.NumAnimDispatches; n++)
                    {
                        WadAnimDispatch ad = new WadAnimDispatch();
                        var wadAd = oldLevel.AnimDispatches[(int)wadSc.AnimDispatch + n];

                        ad.InFrame = (ushort)(wadAd.Low - oldAnimation.FrameStart);
                        ad.OutFrame = (ushort)(wadAd.High - oldAnimation.FrameStart);
                        ad.NextAnimation = (ushort)((wadAd.NextAnimation - oldMoveable.Animation) % numAnimations);
                        ad.NextFrame = (ushort)wadAd.NextFrame;

                        sc.Dispatches.Add(ad);
                    }

                    newAnimation.StateChanges.Add(sc);
                }

                if (oldAnimation.NumAnimCommands < oldLevel.AnimCommands.Count)
                {
                    int lastCommand = oldAnimation.AnimCommand;

                    for (int k = 0; k < oldAnimation.NumAnimCommands; k++)
                    {
                        // HACK: FexMerger corrupts some animcommand sequences, refering to the anim command uint16 outside
                        // of animcommand block. We still try to load animations, ignoring corrupted animcommands.
                        if (lastCommand >= oldLevel.AnimCommands.Count)
                            continue;

                        short commandType = oldLevel.AnimCommands[lastCommand + 0];

                        WadAnimCommand command = new WadAnimCommand { Type = (WadAnimCommandType)commandType };
                        switch (commandType)
                        {
                            case 1:
                                command.Parameter1 = (short)oldLevel.AnimCommands[lastCommand + 1];
                                command.Parameter2 = (short)oldLevel.AnimCommands[lastCommand + 2];
                                command.Parameter3 = (short)oldLevel.AnimCommands[lastCommand + 3];

                                lastCommand += 4;
                                break;

                            case 2:
                                command.Parameter1 = (short)oldLevel.AnimCommands[lastCommand + 1];
                                command.Parameter2 = (short)oldLevel.AnimCommands[lastCommand + 2];

                                lastCommand += 3;
                                break;

                            case 3:
                                lastCommand += 1;
                                break;

                            case 4:
                                lastCommand += 1;
                                break;

                            case 5:
                                command.Parameter1 = (short)(oldLevel.AnimCommands[lastCommand + 1] - oldAnimation.FrameStart);
                                command.Parameter2 = (short)oldLevel.AnimCommands[lastCommand + 2];
                                lastCommand += 3;
                                break;

                            case 6:
                                command.Parameter1 = (short)(oldLevel.AnimCommands[lastCommand + 1] - oldAnimation.FrameStart);
                                command.Parameter2 = (short)oldLevel.AnimCommands[lastCommand + 2];
                                lastCommand += 3;
                                break;

                            default: // Ignore invalid anim commands (see for example karnak.wad)
                                logger.Warn("Unknown anim command " + commandType);
                                goto ExitForLoop;
                        }

                        newAnimation.AnimCommands.Add(command);
                    }
                    ExitForLoop:
                    ;
                }

                int frames = (int)oldAnimation.FrameOffset / 2;
                uint numFrames;

                if (j + oldMoveable.Animation == oldLevel.Animations.Count - 1)
                {
                    if (oldAnimation.FrameSize == 0)
                        numFrames = oldLevel.Version == TRVersion.Game.TR1 ? (uint)(Math.Ceiling((float)newAnimation.EndFrame / (float)newAnimation.FrameRate) + 1) : 0;
                    else
                        numFrames = ((uint)(2 * oldLevel.Frames.Count) - oldAnimation.FrameOffset) / (uint)(2 * oldAnimation.FrameSize);
                }
                else
                {
                    if (oldAnimation.FrameSize == 0)
                        numFrames = oldLevel.Version == TRVersion.Game.TR1 ? (uint)(Math.Ceiling((float)newAnimation.EndFrame / (float)newAnimation.FrameRate) + 1) : 0;
                    else
                        numFrames = (oldLevel.Animations[oldMoveable.Animation + j + 1].FrameOffset - oldAnimation.FrameOffset) / (uint)(2 * oldAnimation.FrameSize);
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
                    if (oldLevel.Version == TRVersion.Game.TR1)
                        frames++;

                    for (int n = 0; n < oldMoveable.NumMeshes; n++)
                        frame.Angles.Add(
                            WadKeyFrameRotation.FromTrAngle(ref frames, oldLevel.Frames,
                                oldLevel.Version == TRVersion.Game.TR1,
                                oldLevel.Version == TRVersion.Game.TR4 || oldLevel.Version == TRVersion.Game.TR5));

                    if ((frames - startOfFrame) < oldAnimation.FrameSize)
                        frames += ((int)oldAnimation.FrameSize - (frames - startOfFrame));

                    newAnimation.KeyFrames.Add(frame);
                }

                frameBases.Add(newAnimation, oldAnimation.FrameStart);

                // New velocities
                float acceleration = oldAnimation.Accel / 65536.0f;
                newAnimation.StartVelocity = oldAnimation.Speed / 65536.0f;

                float lateralAcceleration = oldAnimation.AccelLateral / 65536.0f;
                newAnimation.StartLateralVelocity = oldAnimation.SpeedLateral / 65536.0f;

                if (newAnimation.KeyFrames.Count != 0 && newAnimation.FrameRate != 0)
                {
                    newAnimation.EndVelocity = newAnimation.StartVelocity + acceleration *
                                                 (newAnimation.KeyFrames.Count - 1) * newAnimation.FrameRate;
                    newAnimation.EndLateralVelocity = newAnimation.StartLateralVelocity + lateralAcceleration *
                                                        (newAnimation.KeyFrames.Count - 1) * newAnimation.FrameRate;
                }

                newMoveable.Animations.Add(newAnimation);
            }

            for (int i = 0; i < newMoveable.Animations.Count; i++)
            {
                var animation = newMoveable.Animations[i];

                if (animation.KeyFrames.Count == 0)
                    animation.EndFrame = 0;

                newMoveable.Animations[i] = animation;
            }

            for (int i = 0; i < newMoveable.Animations.Count; i++)
            {
                var animation = newMoveable.Animations[i];

                if (frameBases[newMoveable.Animations[animation.NextAnimation]] != 0)
                    animation.NextFrame -= frameBases[newMoveable.Animations[animation.NextAnimation]];

                foreach (var stateChange in animation.StateChanges)
                {
                    for (int J = 0; J < stateChange.Dispatches.Count; ++J)
                    {
                        WadAnimDispatch animDispatch = stateChange.Dispatches[J];
                        if (frameBases[newMoveable.Animations[animDispatch.NextAnimation]] != 0)
                            animDispatch.NextFrame = (ushort)(animDispatch.NextFrame - frameBases[newMoveable.Animations[animDispatch.NextAnimation]]);
                        stateChange.Dispatches[J] = animDispatch;
                    }
                }

                newMoveable.Animations[i] = animation;
            }

            return newMoveable;
        }

        public static WadStatic ConvertTrLevelStaticMeshToWadStatic(Wad2 wad, TrLevel oldLevel, int staticIndex, TextureArea[] objectTextures)
        {
            tr_staticmesh oldStatic = oldLevel.StaticMeshes[staticIndex];
            var newStatic = new WadStatic(new WadStaticId(oldStatic.ObjectID));

            // First setup collisional and visibility bounding boxes
            newStatic.CollisionBox = new BoundingBox(new Vector3(oldStatic.CollisionBox.X1,
                                                                  -oldStatic.CollisionBox.Y1,
                                                                  oldStatic.CollisionBox.Z1),
                                                      new Vector3(oldStatic.CollisionBox.X2,
                                                                  -oldStatic.CollisionBox.Y2,
                                                                  oldStatic.CollisionBox.Z2));

            newStatic.VisibilityBox = new BoundingBox(new Vector3(oldStatic.VisibilityBox.X1,
                                                                   -oldStatic.VisibilityBox.Y1,
                                                                   oldStatic.VisibilityBox.Z1),
                                                       new Vector3(oldStatic.VisibilityBox.X2,
                                                                   -oldStatic.VisibilityBox.Y2,
                                                                   oldStatic.VisibilityBox.Z2));

            // Then import the mesh. If it was already added, the mesh will not be added to the dictionary.
            newStatic.Mesh = ConvertTrLevelMeshToWadMesh(wad,
                                                      oldLevel,
                                                      oldLevel.GetMeshFromPointer(oldStatic.Mesh),
                                                      objectTextures);
            return newStatic;
        }
    }
}
