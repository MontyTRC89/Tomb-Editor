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

        public static Wad2 ConvertTrLevel(TrLevel oldLevel)
        {
            var wad = new Wad2();
            wad.SuggestedGameVersion = TrLevel.GetWadGameVersion(oldLevel.Version);

            logger.Info("Converting TR level to WAD2");

            // Convert textures
            TextureArea[] objectTextures = ConvertTrLevelTexturesToWadTexture(oldLevel);
            logger.Info("Texture conversion complete.");

            // Convert sounds
            WadSoundInfo[] soundInfos = ConvertTrLevelSounds(wad, oldLevel);
            logger.Info("Sound conversion complete.");

            // Then convert moveables and static meshes
            // Meshes will be converted inside each model
            for (int i = 0; i < oldLevel.Moveables.Count; i++)
            {
                WadMoveable moveable = ConvertTrLevelMoveableToWadMoveable(wad, oldLevel, i, objectTextures, soundInfos);
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

            // for (int i = 0; i < oldLevel.ObjectTextures.Count; ++i)
            Parallel.For(0, oldLevel.ObjectTextures.Count, i =>
            {
                var oldTexture = oldLevel.ObjectTextures[i];

                int textureTileIndex = oldTexture.TileAndFlags & 0x7fff;
                // We can't use this bit in TR2...
                bool isTriangle = (oldTexture.TileAndFlags & 0x8000) != 0;
                if (oldLevel.Version == TrVersion.TR1 || oldLevel.Version == TrVersion.TR2)
                    isTriangle = (oldTexture.Vertices[3].X == 0) && (oldTexture.Vertices[3].Y == 0);

                // Calculate UV coordinates...
                Vector2[] coordAddArray;
                if (oldLevel.Version == TrVersion.TR1 || oldLevel.Version == TrVersion.TR2 || oldLevel.Version == TrVersion.TR3)
                    coordAddArray = _zero;
                else
                    coordAddArray = LevelData.Compilers.Util.ObjectTextureManager.GetTexCoordModificationFromNewFlags(oldTexture.NewFlags, isTriangle);
                Vector2[] coords = new Vector2[isTriangle ? 3 : 4];
                for (int j = 0; j < coords.Length; ++j)
                    coords[j] = new Vector2(oldTexture.Vertices[j].X, oldTexture.Vertices[j].Y) * (1.0f / 256f) + coordAddArray[j];

                // Find the corners of the texture
                Vector2 min = coords[0], max = coords[0];
                for (int j = 1; j < coords.Length; ++j)
                {
                    min = Vector2.Min(min, coords[j]);
                    max = Vector2.Max(max, coords[j]);
                }
                const float margin = 0.49f;
                VectorInt2 start = VectorInt2.FromFloor(min - new Vector2(margin));
                VectorInt2 end = VectorInt2.FromCeiling(max + new Vector2(margin));
                start = VectorInt2.Min(VectorInt2.Max(start, new VectorInt2()), new VectorInt2(256, 256));
                end = VectorInt2.Min(VectorInt2.Max(end, new VectorInt2()), new VectorInt2(256, 256));

                // Create image
                ImageC image = ImageC.CreateNew(end.X - start.X, end.Y - start.Y);
                image.CopyFrom(0, 0, tiles, start.X, start.Y + textureTileIndex * 256, end.X - start.X, end.Y - start.Y);
                WadTexture texture = new WadTexture(image);

                // Create texture area
                TextureArea textureArea;
                textureArea.DoubleSided = false;
                textureArea.BlendMode = (BlendMode)(oldTexture.Attributes);
                textureArea.TexCoord0 = coords[0] - start;
                textureArea.TexCoord1 = coords[1] - start;
                textureArea.TexCoord2 = coords[2] - start;
                textureArea.TexCoord3 = isTriangle ? new Vector2() : (coords[3] - start);
                textureArea.Texture = texture;

                objectTextures[i] = textureArea;
            });

            return objectTextures;
        }

        private static WadMesh ConvertTrLevelMeshToWadMesh(Wad2 wad, TrLevel oldLevel, tr_mesh oldMesh, TextureArea[] objectTextures)
        {
            WadMesh mesh = new WadMesh();

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

                WadPolygon poly;
                poly.Shape = WadPolygonShape.Quad;
                poly.Index0 = oldPoly.Index0;
                poly.Index1 = oldPoly.Index1;
                poly.Index2 = oldPoly.Index2;
                poly.Index3 = oldPoly.Index3;
                poly.ShineStrength = (byte)((oldPoly.LightingEffect & 0xfe) >> 1);
                poly.Texture = textureArea;
                mesh.Polys.Add(poly);
            }
            foreach (var oldPoly in oldMesh.TexturedTriangles)
            {
                TextureArea textureArea = objectTextures[oldPoly.Texture & 0x7fff];
                textureArea.DoubleSided = (oldPoly.Texture & 0x8000) != 0;

                WadPolygon poly;
                poly.Shape = WadPolygonShape.Triangle;
                poly.Index0 = oldPoly.Index0;
                poly.Index1 = oldPoly.Index1;
                poly.Index2 = oldPoly.Index2;
                poly.Index3 = 0;
                poly.ShineStrength = (byte)((oldPoly.LightingEffect & 0xfe) >> 1);
                poly.Texture = textureArea;
                mesh.Polys.Add(poly);
            }

            foreach (var oldPoly in oldMesh.ColoredRectangles)
            {
                WadPolygon poly;
                poly.Shape = WadPolygonShape.Quad;
                poly.Index0 = oldPoly.Index0;
                poly.Index1 = oldPoly.Index1;
                poly.Index2 = oldPoly.Index2;
                poly.Index3 = oldPoly.Index3;
                poly.Texture = ConvertColoredFaceToTexture(wad, oldLevel, oldPoly.Texture & 0xff);
                poly.ShineStrength = 0;
                mesh.Polys.Add(poly);
            }

            foreach (var oldPoly in oldMesh.ColoredTriangles)
            {
                WadPolygon poly;
                poly.Shape = WadPolygonShape.Quad;
                poly.Index0 = oldPoly.Index0;
                poly.Index1 = oldPoly.Index1;
                poly.Index2 = oldPoly.Index2;
                poly.Index3 = 0;
                poly.Texture = ConvertColoredFaceToTexture(wad, oldLevel, oldPoly.Texture & 0xff);
                poly.ShineStrength = 0;
                mesh.Polys.Add(poly);
            }

            // Calculate hash
            mesh.UpdateHash();
            return mesh;
        }

        private static TextureArea ConvertColoredFaceToTexture(Wad2 wad, TrLevel oldLevel, int palette8)
        {
            tr_color color = oldLevel.Palette8[palette8];
            ColorC color2 = new ColorC(color.Red, color.Green, color.Blue, 255);
            var image = ImageC.CreateNew(4, 4);
            image.SetPixel(0, 0, color2);
            image.SetPixel(1, 0, color2);
            image.SetPixel(0, 1, color2);
            image.SetPixel(1, 1, color2);

            TextureArea textureArea;
            textureArea.Texture = new WadTexture(image);
            textureArea.TexCoord0 = new Vector2(0.5f, 0.5f);
            textureArea.TexCoord1 = new Vector2(1.5f, 0.5f);
            textureArea.TexCoord2 = new Vector2(1.5f, 1.5f);
            textureArea.TexCoord3 = new Vector2(0.5f, 1.5f);
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

                    if (oldLevel.Version == TrVersion.TR1 ||
                        oldLevel.Version == TrVersion.TR2 ||
                        oldLevel.Version == TrVersion.TR3)
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
                    newSequence.Sprites.Add(new WadSprite { Texture = new WadTexture(spriteImage) });
                }

                wad.SpriteSequences.Add(newSequence.Id, newSequence);
            }
        }

        private static WadSoundInfo[] ConvertTrLevelSounds(Wad2 wad, TrLevel oldLevel)
        {
            // Convert samples...
            var samples = new WadSample[oldLevel.Samples.Count];
            Parallel.For(0, oldLevel.Samples.Count, delegate (int i)
            {
                samples[i] = new WadSample(WadSample.ConvertSampleFormat(oldLevel.Samples[i].Data, oldLevel.Version != TrVersion.TR4));
            });

            // For old TRs, we need to load the samples from MAIN.SFX
            int TODO_LOAD_MAIN_SFX_SAMPLES;

            // Convert sound details
            var soundInfos = new WadSoundInfo[oldLevel.SoundMap.Count];
            for (int i = 0; i < oldLevel.SoundMap.Count; i++)
            {
                // Check if sound was used
                if (oldLevel.SoundMap[i] == -1)
                    continue;

                tr_sound_details oldInfo = oldLevel.SoundDetails[oldLevel.SoundMap[i]];

                // Fill the new sound info
                var newInfo = new WadSoundInfoMetaData(TrCatalog.GetOriginalSoundName(TrLevel.GetWadGameVersion(oldLevel.Version), (uint)i));
                newInfo.VolumeDiv255 = (byte)oldInfo.Volume;
                newInfo.RangeInSectors = oldInfo.Range;
                newInfo.ChanceDiv255 = (byte)oldInfo.Chance;
                newInfo.PitchFactorDiv128 = oldInfo.Pitch;
                newInfo.RandomizePitch = ((oldInfo.Characteristics & 0x2000) != 0); // TODO: loop meaning changed between TR versions
                newInfo.RandomizeGain = ((oldInfo.Characteristics & 0x4000) != 0);
                newInfo.FlagN = ((oldInfo.Characteristics & 0x1000) != 0);
                newInfo.LoopBehaviour = (WadSoundLoopBehaviour)(oldInfo.Characteristics & 0x03);

                // Read all samples linked to this sound info (for example footstep has 4 samples)
                int numSamplesInGroup = (oldInfo.Characteristics & 0x00fc) >> 2;
                for (int j = 0; j < numSamplesInGroup; j++)
                {
                    int soundIndexIndex = j + oldInfo.Sample;
                    int sampleIndex;
                        if (oldLevel.Version == TrVersion.TR2 || oldLevel.Version == TrVersion.TR3)
                            sampleIndex = (int)oldLevel.SamplesIndices[soundIndexIndex];
                        else
                            sampleIndex = soundIndexIndex;
                    if (sampleIndex >= oldLevel.Samples.Count)
                    {
                        logger.Warn("Sample index out of range.");
                        continue;
                    }
                    newInfo.Samples.Add(samples[sampleIndex]);
                }

                soundInfos[i] = new WadSoundInfo(newInfo);
            }

            // Fix some sounds
            for (int i = 0; i < soundInfos.Length; i++)
                if (soundInfos[i] != null)
                    if (TrCatalog.IsSoundFixedByDefault(TrLevel.GetWadGameVersion(oldLevel.Version), (uint)i))
                    {
                        var id = new WadFixedSoundInfoId((uint)i);
                        wad.FixedSoundInfos.Add(id, new WadFixedSoundInfo(id) { SoundInfo = soundInfos[i] });
                    }

            return soundInfos;
        }

        public static WadMoveable ConvertTrLevelMoveableToWadMoveable(Wad2 wad, TrLevel oldLevel, int moveableIndex,
                                                                 TextureArea[] objectTextures, WadSoundInfo[] soundInfos)
        {
            Console.WriteLine("Converting Moveable " + moveableIndex);

            var oldMoveable = oldLevel.Moveables[moveableIndex];
            WadMoveable newMoveable = new WadMoveable(new WadMoveableId(oldMoveable.ObjectID));

            // First a list of meshes for this moveable is built
            var meshes = new List<tr_mesh>();
            var convertedMeshes = new List<WadMesh>();
            for (int j = 0; j < oldMoveable.NumMeshes; j++)
                meshes.Add(oldLevel.Meshes[(int)oldLevel.RealPointers[(int)(oldMoveable.StartingMesh + j)]]);

            // Convert the WadMesh
            foreach (var oldMesh in meshes)
            {
                WadMesh newMesh = ConvertTrLevelMeshToWadMesh(wad, oldLevel, oldMesh, objectTextures);
                convertedMeshes.Add(newMesh);
            }

            // Build the skeleton
            WadBone root = new WadBone();
            root.Name = "bone_root";
            root.Parent = null;
            root.Transform = Matrix4x4.Identity;
            root.Mesh = convertedMeshes[0];
            root.Index = 0;
            newMoveable.Meshes.Add(root.Mesh);

            var bones = new List<WadBone>();
            bones.Add(root);
            newMoveable.Skeleton = root;

            for (int j = 0; j < oldMoveable.NumMeshes - 1; j++)
            {
                WadBone bone = new WadBone();
                bone.Name = "bone_" + (j + 1).ToString();
                bone.Parent = null;
                bone.Transform = Matrix4x4.Identity;
                bone.Mesh = convertedMeshes[j + 1];
                bone.Index = j + 1;
                newMoveable.Meshes.Add(bone.Mesh);
                bones.Add(bone);
            }

            WadBone currentBone = root;
            WadBone stackBone = root;
            Stack<WadBone> stack = new Stack<WadBone>();

            for (int mi = 0; mi < (newMoveable.Meshes.Count - 1); mi++)
            {
                int j = mi + 1;

                var opcode = (WadLinkOpcode)oldLevel.MeshTrees[(int)(oldMoveable.MeshTree + mi * 4)];
                int linkX = oldLevel.MeshTrees[(int)(oldMoveable.MeshTree + mi * 4) + 1];
                int linkY = -oldLevel.MeshTrees[(int)(oldMoveable.MeshTree + mi * 4) + 2];
                int linkZ = oldLevel.MeshTrees[(int)(oldMoveable.MeshTree + mi * 4) + 3];

                switch (opcode)
                {
                    case WadLinkOpcode.NotUseStack:
                        bones[j].Transform = Matrix4x4.CreateTranslation(linkX, linkY, linkZ);
                        bones[j].Parent = currentBone;
                        currentBone.Children.Add(bones[j]);
                        currentBone = bones[j];

                        break;
                    case WadLinkOpcode.Push:
                        if (stack.Count <= 0)
                            continue;
                        currentBone = stack.Pop();

                        bones[j].Transform = Matrix4x4.CreateTranslation(linkX, linkY, linkZ);
                        bones[j].Parent = currentBone;
                        currentBone.Children.Add(bones[j]);
                        currentBone = bones[j];

                        break;
                    case WadLinkOpcode.Pop:
                        stack.Push(currentBone);

                        bones[j].Transform = Matrix4x4.CreateTranslation(linkX, linkY, linkZ);
                        bones[j].Parent = currentBone;
                        currentBone.Children.Add(bones[j]);
                        currentBone = bones[j];

                        break;
                    case WadLinkOpcode.Read:
                        if (stack.Count <= 0)
                            continue;
                        WadBone bone = stack.Pop();
                        bones[j].Transform = Matrix4x4.CreateTranslation(linkX, linkY, linkZ);
                        bones[j].Parent = bone;
                        bone.Children.Add(bones[j]);
                        currentBone = bones[j];
                        stack.Push(bone);

                        break;
                }
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
                newAnimation.Acceleration = oldAnimation.Accel;
                newAnimation.Speed = oldAnimation.Speed;
                newAnimation.LateralSpeed = oldAnimation.SpeedLateral;
                newAnimation.LateralAcceleration = oldAnimation.AccelLateral;
                newAnimation.FrameDuration = oldAnimation.FrameRate;
                newAnimation.NextAnimation = (ushort)(oldAnimation.NextAnimation - oldMoveable.Animation);
                newAnimation.NextFrame = oldAnimation.NextFrame;
                newAnimation.StateId = oldAnimation.StateID;
                newAnimation.RealNumberOfFrames = (ushort)(oldAnimation.FrameEnd - oldAnimation.FrameStart + 1);
                newAnimation.Name = "Animation " + j;

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
                        short commandType = oldLevel.AnimCommands[lastCommand + 0];

                        WadAnimCommand command = new WadAnimCommand { Type = (WadAnimCommandType)commandType };
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
                                command.Parameter1 = (ushort)(oldLevel.AnimCommands[lastCommand + 1] - oldAnimation.FrameStart);
                                command.Parameter2 = (ushort)oldLevel.AnimCommands[lastCommand + 2];
                                lastCommand += 3;

                                int soundInfoIndex = command.Parameter2 & 0x3fff;
                                command.Parameter2 &= 0xC000;
                                if (soundInfoIndex >= soundInfos.Length || soundInfos[soundInfoIndex] == null)
                                {
                                    logger.Warn("Anim command uses " + soundInfoIndex + " which is unavailable.");
                                    continue;
                                }
                                command.SoundInfo = soundInfos[soundInfoIndex];
                                break;

                            case 6:
                                command.Parameter1 = (ushort)(oldLevel.AnimCommands[lastCommand + 1] - oldAnimation.FrameStart);
                                command.Parameter2 = (ushort)oldLevel.AnimCommands[lastCommand + 2];
                                lastCommand += 3;
                                break;

                            default: // Ignore invalid anim commands (see for example karnak.wad)
                                logger.Warn("Unknown anim command " + commandType);
                                lastCommand += 1;
                                continue;
                        }

                        newAnimation.AnimCommands.Add(command);
                    }
                }

                int frames = (int)oldAnimation.FrameOffset / 2;
                uint numFrames;

                if (j + oldMoveable.Animation == oldLevel.Animations.Count - 1)
                {
                    if (oldAnimation.FrameSize == 0)
                        numFrames = 0;
                    else
                        numFrames = ((uint)(2 * oldLevel.Frames.Count) - oldAnimation.FrameOffset) / (uint)(2 * oldAnimation.FrameSize);
                }
                else
                {
                    if (oldAnimation.FrameSize == 0)
                    {
                        numFrames = 0;
                    }
                    else
                    {
                        numFrames = (oldLevel.Animations[oldMoveable.Animation + j + 1].FrameOffset - oldAnimation.FrameOffset) / (uint)(2 * oldAnimation.FrameSize);
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
                    if (oldLevel.Version == TrVersion.TR1)
                        frames++;

                    for (int n = 0; n < oldMoveable.NumMeshes; n++)
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

                    if ((frames - startOfFrame) < oldAnimation.FrameSize)
                        frames += ((int)oldAnimation.FrameSize - (frames - startOfFrame));

                    newAnimation.KeyFrames.Add(frame);
                }

                // TODO: check if this hack work well
                // In original WADs animations with no keyframes had some random FrameEnd values
                if (newAnimation.KeyFrames.Count == 0)
                {
                    newAnimation.FrameEnd = oldAnimation.FrameStart;
                }

                frameBases.Add(newAnimation, oldAnimation.FrameStart);

                newMoveable.Animations.Add(newAnimation);
            }

            for (int i = 0; i < newMoveable.Animations.Count; i++)
            {
                var animation = newMoveable.Animations[i];

                if (animation.KeyFrames.Count == 0)
                    animation.RealNumberOfFrames = 0;

                // HACK: this fixes some invalid NextAnimations values
                animation.NextAnimation %= (ushort)newMoveable.Animations.Count;

                newMoveable.Animations[i] = animation;
            }

            for (int i = 0; i < newMoveable.Animations.Count; i++)
            {
                var animation = newMoveable.Animations[i];

                // HACK: this fixes some invalid NextFrame values
                if (frameBases[newMoveable.Animations[animation.NextAnimation]] != 0)
                    animation.NextFrame %= frameBases[newMoveable.Animations[animation.NextAnimation]];

                foreach (var stateChange in animation.StateChanges)
                {
                    for (int J = 0; J < stateChange.Dispatches.Count; ++J)
                    {
                        WadAnimDispatch animDispatch = stateChange.Dispatches[J];
                        if (frameBases[newMoveable.Animations[animDispatch.NextAnimation]] != 0)
                        {
                            ushort newFrame = (ushort)(animDispatch.NextFrame % frameBases[newMoveable.Animations[animDispatch.NextAnimation]]);

                            // In some cases dispatches have invalid NextFrame.
                            // From tests it seems that's ok to delete the dispatch or put the NextFrame equal to zero.
                            if (newFrame > newMoveable.Animations[animDispatch.NextAnimation].RealNumberOfFrames)
                                newFrame = 0;

                            animDispatch.NextFrame = newFrame;
                        }
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
