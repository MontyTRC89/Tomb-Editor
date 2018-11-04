using NLog;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using TombLib.Utils;
using TombLib.Wad.Catalog;

namespace TombLib.Wad.Tr4Wad
{
    public class SamplePathInfo
    {
        public string Name { get; set; }
        public string FullPath { get; set; } = null;
        public bool Found { get { return (!string.IsNullOrEmpty(FullPath)) && File.Exists(FullPath); } }
    }

    internal static class Tr4WadOperations
    {
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();

        public static Wad2 ConvertTr4Wad(Tr4Wad oldWad, List<string> soundPaths, IDialogHandler progressReporter)
        {
            logger.Info("Converting TR4 WAD to Wad2");

            var wad = new Wad2();
            wad.SuggestedGameVersion = WadGameVersion.TR4_TRNG;

            // Try to find all samples
            var samples = new List<SamplePathInfo>();
            for (var i = 0; i < oldWad.Sounds.Count; i++)
                samples.Add(new SamplePathInfo { Name = oldWad.Sounds[i] });
            Func<bool> FindTr4Samples = () =>
            {
                bool everythingOk = true;
                for (var i = 0; i < oldWad.Sounds.Count; i++)
                    if (!samples[i].Found)
                    {
                        samples[i].FullPath = WadSample.LookupSound(samples[i].Name, true, oldWad.BasePath, soundPaths);
                        everythingOk = everythingOk && !string.IsNullOrEmpty(samples[i].FullPath);
                    }
                return everythingOk;
            };
            if (!FindTr4Samples())
            {
                var soundPathInformation = new DialogDescriptonMissingSounds { WadBasePath = oldWad.BasePath,
                    WadBaseFileName = oldWad.BaseName, Samples = samples, SoundPaths = soundPaths }; // Reuse "SoundPaths" list directly, to update sound list in this file too.
                soundPathInformation.FindTr4Samples = FindTr4Samples;
                progressReporter?.RaiseDialog(soundPathInformation);
                samples = soundPathInformation.Samples;
            }

            // Convert all textures
            Dictionary<int, WadTexture> textures = ConvertTr4TexturesToWadTexture(oldWad, wad);
            logger.Info("Textures read.");

            // Convert sounds
            WadSoundInfo[] soundInfos = ConvertTr4Sounds(wad, oldWad, samples);
            logger.Info("Sounds read.");

            // Convert moveables
            for (int i = 0; i < oldWad.Moveables.Count; i++)
                ConvertTr4MoveableToWadMoveable(wad, oldWad, i, textures, soundInfos);
            logger.Info("Moveables read.");

            // Convert statics
            for (int i = 0; i < oldWad.Statics.Count; i++)
                ConvertTr4StaticMeshToWadStatic(wad, oldWad, i, textures);
            logger.Info("Statics read.");

            // Convert sprites
            ConvertTr4Sprites(wad, oldWad);
            logger.Info("Sprites read.");

            // Insert also additional sounds
            AddAdditionalSoundInfos(wad, oldWad, soundInfos);

            return wad;
        }

        private static void AddAdditionalSoundInfos(Wad2 wad, Tr4Wad oldWad, WadSoundInfo[] infos)
        {
            var newSoundInfos = wad.SoundInfosUnique.ToList();
            for (uint i = 0; i < infos.Length; ++i)
                if (infos[i] != null && !newSoundInfos.Contains(infos[i]))
                {
                    var id = new WadAdditionalSoundInfoId(TrCatalog.GetOriginalSoundName(wad.SuggestedGameVersion, i));
                    wad.AdditionalSoundInfos.Add(id, new WadAdditionalSoundInfo(id) { SoundInfo = infos[i] });
                }
        }

        private static Dictionary<int, WadTexture> ConvertTr4TexturesToWadTexture(Tr4Wad oldWad, Wad2 wad)
        {
            var textures = new ConcurrentDictionary<int, WadTexture>();

            Parallel.For(0, oldWad.Textures.Count, i =>
              {
                  var oldTexture = oldWad.Textures[i];
                  var startX = (short)(oldTexture.X);
                  var startY = (short)(oldTexture.Page * 256 + oldTexture.Y);

                  // Create the texture ImageC
                  var image = ImageC.CreateNew(oldTexture.Width + 1, oldTexture.Height + 1);

                  for (var y = 0; y < image.Height; y++)
                  {
                      for (var x = 0; x < image.Width; x++)
                      {
                          var baseIndex = (startY + y) * 768 + (startX + x) * 3;
                          var r = oldWad.TexturePages[baseIndex];
                          var g = oldWad.TexturePages[baseIndex + 1];
                          var b = oldWad.TexturePages[baseIndex + 2];
                          var a = (byte)255;

                          //var color = new ColorC(r, g, b, a);
                          image.SetPixel(x, y, r, g, b, a);
                      }
                  }

                  // Replace magenta color with alpha transparent black
                  image.ReplaceColor(new ColorC(255, 0, 255, 255), new ColorC(0, 0, 0, 0));

                  textures.TryAdd(i, new WadTexture(image));
              });

            return new Dictionary<int, WadTexture>(textures);
        }

        private static int GetTr4TextureIdFromPolygon(wad_polygon polygon)
        {
            short textureId = (short)(polygon.Texture);
            if (polygon.Shape == 8)
            {
                textureId = (short)(polygon.Texture & 0xFFF);
                if ((polygon.Texture & 0x8000) != 0)
                    textureId = (short)(-textureId);
            }
            else
            {
                // HACK: for taking in account sign
                if (textureId > 0x7FFF)
                {
                    textureId = (short)(textureId - 0xFFFF);
                }
            }

            textureId = Math.Abs(textureId);

            return textureId;
        }

        private static WadMesh ConvertTr4MeshToWadMesh(Wad2 wad, Tr4Wad oldWad, Dictionary<int, WadTexture> textures,
                                                       wad_mesh oldMesh, int objectID)
        {
            WadMesh mesh = new WadMesh();
            var meshIndex = oldWad.Meshes.IndexOf(oldMesh);
            mesh.Name = "Mesh-" + objectID + "-" + meshIndex;

            // Create the bounding sphere
            mesh.BoundingSphere = new BoundingSphere(new Vector3(oldMesh.SphereX, -oldMesh.SphereY, oldMesh.SphereZ), oldMesh.Radius);

            // Add positions
            foreach (var oldVertex in oldMesh.Vertices)
                mesh.VerticesPositions.Add(new Vector3(oldVertex.X, -oldVertex.Y, oldVertex.Z));

            // Add normals
            foreach (var oldNormal in oldMesh.Normals)
            {
                mesh.VerticesNormals.Add(new Vector3(oldNormal.X, -oldNormal.Y, oldNormal.Z));
            }

            // Add shades
            foreach (var oldShade in oldMesh.Shades)
            {
                mesh.VerticesShades.Add(oldShade);
            }

            // Add polygons
            foreach (var oldPoly in oldMesh.Polygons)
            {
                WadPolygon poly = new WadPolygon();
                poly.Shape = oldPoly.Shape == 8 ? WadPolygonShape.Triangle : WadPolygonShape.Quad;

                // Polygon indices
                poly.Index0 = oldPoly.V1;
                poly.Index1 = oldPoly.V2;
                poly.Index2 = oldPoly.V3;
                if (poly.Shape == WadPolygonShape.Quad)
                    poly.Index3 = oldPoly.V4;

                // Polygon special effects
                poly.ShineStrength = (byte)((oldPoly.Attributes & 0x7c) >> 2);

                // Add the texture
                poly.Texture = CalculateTr4UVCoordinates(wad, oldWad, oldPoly, textures);

                mesh.Polys.Add(poly);
            }

            mesh.BoundingBox = new BoundingBox(oldMesh.Minimum, oldMesh.Maximum);

            // Usually only for static meshes
            if (mesh.VerticesNormals.Count == 0)
                mesh.RecalculateNormals();

            return mesh;
        }

        internal static void ConvertTr4Sprites(Wad2 wad, Tr4Wad oldWad)
        {
            int spriteDataSize = oldWad.SpriteData.Length;

            // Load the real sprite texture data
            int numSpriteTexturePages = spriteDataSize / 196608;
            if ((spriteDataSize % 196608) != 0)
                numSpriteTexturePages++;

            foreach (var oldSequence in oldWad.SpriteSequences)
            {
                int lengthOfSequence = -oldSequence.NegativeLength;
                int startIndex = oldSequence.Offset;

                var newSequence = new WadSpriteSequence(new WadSpriteSequenceId((uint)oldSequence.ObjectID));

                for (int i = startIndex; i < startIndex + lengthOfSequence; i++)
                {
                    var oldSpriteTexture = oldWad.SpriteTextures[i];

                    int spriteWidth = oldSpriteTexture.Width + 1;
                    int spriteHeight = oldSpriteTexture.Height + 1;
                    int spriteX = oldSpriteTexture.X;
                    int spriteY = oldSpriteTexture.Y;
                    var spriteImage = ImageC.CreateNew(spriteWidth, spriteHeight);

                    for (int y = 0; y < spriteHeight; y++)
                        for (int x = 0; x < spriteWidth; x++)
                        {
                            int baseIndex = oldSpriteTexture.Tile * 196608 + 768 * (y + spriteY) + 3 * (x + spriteX);

                            byte b = oldWad.SpriteData[baseIndex + 0];
                            byte g = oldWad.SpriteData[baseIndex + 1];
                            byte r = oldWad.SpriteData[baseIndex + 2];

                            if (r == 255 & g == 0 && b == 255)
                                spriteImage.SetPixel(x, y, 0, 0, 0, 0);
                            else
                                spriteImage.SetPixel(x, y, b, g, r, 255);
                        }

                    // Add current sprite to the sequence
                    newSequence.Sprites.Add(new WadSprite { Texture = new WadTexture(spriteImage) });
                }

                wad.SpriteSequences.Add(newSequence.Id, newSequence);
            }
        }

        internal static WadSoundInfo[] ConvertTr4Sounds(Wad2 wad, Tr4Wad oldWad, List<SamplePathInfo> samplePathInfos)
        {
            // Load samples
            var loadedSamples = new Dictionary<int, WadSample>();
            Parallel.For(0, oldWad.Sounds.Count, i =>
            {
                WadSample currentSample = WadSample.NullSample;
                try
                {
                    if (samplePathInfos[i].Found)
                        using (var stream = new FileStream(samplePathInfos[i].FullPath, FileMode.Open, FileAccess.Read, FileShare.Read))
                        {
                            var buffer = new byte[stream.Length];
                            if (stream.Read(buffer, 0, buffer.Length) != buffer.Length)
                                throw new EndOfStreamException();
                            currentSample = new WadSample(WadSample.ConvertSampleFormat(buffer, false));
                        }
                }
                catch (Exception exc)
                {
                    logger.Warn(exc, "Unable to read file '" + samplePathInfos[i].FullPath + "'");
                }

                lock (loadedSamples)
                    loadedSamples.Add(i, currentSample);
            });

            // Load sound infos
            int soundMapSize = oldWad.Version == 130 ? 4096 : 370;
            WadSoundInfo[] soundInfos = new WadSoundInfo[soundMapSize];
            for (int i = 0; i < soundMapSize; i++)
            {
                // Check if sound is defined at all
                if (oldWad.SoundMap[i] == -1)
                    continue;

                // Fill the new sound info
                var oldInfo = oldWad.SoundInfo[oldWad.SoundMap[i]];
                var newInfo = new WadSoundInfoMetaData(TrCatalog.GetOriginalSoundName(WadGameVersion.TR4_TRNG, (uint)i));
                newInfo.VolumeByte = oldInfo.Volume;
                newInfo.RangeInSectorsByte = oldInfo.Range;
                newInfo.ChanceByte = oldInfo.Chance;
                newInfo.PitchFactorByte = oldInfo.Pitch;
                newInfo.RandomizePitch = ((oldInfo.Characteristics & 0x2000) != 0);
                newInfo.RandomizeVolume = ((oldInfo.Characteristics & 0x4000) != 0);
                newInfo.DisablePanning = ((oldInfo.Characteristics & 0x1000) != 0);
                newInfo.LoopBehaviour = (WadSoundLoopBehaviour)(oldInfo.Characteristics & 0x03);

                // Read all samples linked to this sound info (for example footstep has 4 samples)
                int numSamplesInGroup = (oldInfo.Characteristics & 0x00fc) >> 2;
                for (int j = oldInfo.Sample; j < oldInfo.Sample + numSamplesInGroup; j++)
                {
                    WadSample sample;
                    if (!loadedSamples.TryGetValue(j, out sample))
                    {
                        logger.Warn("Unable to find sample '" + oldWad.Sounds[j] + "'.");
                        sample = WadSample.NullSample;
                    }
                    newInfo.Samples.Add(sample);
                }
                soundInfos[i] = new WadSoundInfo(newInfo);
            }

            // Fix some sounds
            for (int i = 0; i < soundMapSize; i++)
                if (soundInfos[i] != null)
                    if (TrCatalog.IsSoundFixedByDefault(WadGameVersion.TR4_TRNG, (uint)i))
                    {
                        var id = new WadFixedSoundInfoId((uint)i);
                        wad.FixedSoundInfos.Add(id, new WadFixedSoundInfo(id) { SoundInfo = soundInfos[i] });
                    }

            return soundInfos;
        }

        internal static WadMoveable ConvertTr4MoveableToWadMoveable(Wad2 wad, Tr4Wad oldWad, int moveableIndex,
                                                                    /*List<WadMesh> meshes, */
                                                                    Dictionary<int, WadTexture> textures,
                                                                    WadSoundInfo[] soundInfos)
        {
            wad_moveable oldMoveable = oldWad.Moveables[moveableIndex];
            WadMoveable newMoveable = new WadMoveable(new WadMoveableId(oldMoveable.ObjectID));
            var frameBases = new Dictionary<WadAnimation, ushort[]>();

            // Load meshes
            var meshes = new List<WadMesh>();
            for (int j = 0; j < oldMoveable.NumPointers; j++)
            {
                meshes.Add(ConvertTr4MeshToWadMesh(wad, oldWad, textures,
                                                               oldWad.Meshes[(int)oldWad.RealPointers[oldMoveable.PointerIndex + j]],
                                                               (int)oldMoveable.ObjectID));
            }

            // Build the skeleton
            WadBone root = new WadBone();
            root.Name = "bone_root";
            root.Parent = null;
            root.Translation = Vector3.Zero;
            root.Mesh = meshes[0];

            var bones = new List<WadBone>();
            bones.Add(root);
            newMoveable.Skeleton = root;

            for (int j = 0; j < oldMoveable.NumPointers - 1; j++)
            {
                WadBone bone = new WadBone();
                bone.Name = "bone_" + (j + 1).ToString();
                bone.Parent = null;
                bone.Translation = Vector3.Zero;
                bone.Mesh = meshes[j + 1];
                bones.Add(bone);
            }

            WadBone currentBone = root;
            WadBone stackBone = root;
            Stack<WadBone> stack = new Stack<WadBone>();

            for (int mi = 0; mi < (oldMoveable.NumPointers - 1); mi++)
            {
                int j = mi + 1;

                var opcode = (WadLinkOpcode)oldWad.Links[(int)(oldMoveable.LinksIndex + mi * 4)];
                int linkX = oldWad.Links[(int)(oldMoveable.LinksIndex + mi * 4) + 1];
                int linkY = -oldWad.Links[(int)(oldMoveable.LinksIndex + mi * 4) + 2];
                int linkZ = oldWad.Links[(int)(oldMoveable.LinksIndex + mi * 4) + 3];

                switch (opcode)
                {
                    case WadLinkOpcode.NotUseStack:
                        bones[j].Translation = new Vector3(linkX, linkY, linkZ);
                        bones[j].Parent = currentBone;
                        currentBone.Children.Add(bones[j]);
                        currentBone = bones[j];

                        break;
                    case WadLinkOpcode.Push:
                        if (stack.Count <= 0)
                            continue;
                        currentBone = stack.Pop();

                        bones[j].Translation = new Vector3(linkX, linkY, linkZ);
                        bones[j].Parent = currentBone;
                        currentBone.Children.Add(bones[j]);
                        currentBone = bones[j];

                        break;
                    case WadLinkOpcode.Pop:
                        stack.Push(currentBone);

                        bones[j].Translation = new Vector3(linkX, linkY, linkZ);
                        bones[j].Parent = currentBone;
                        currentBone.Children.Add(bones[j]);
                        currentBone = bones[j];

                        break;
                    case WadLinkOpcode.Read:
                        if (stack.Count <= 0)
                            continue;
                        WadBone bone = stack.Pop();
                        bones[j].Translation = new Vector3(linkX, linkY, linkZ);
                        bones[j].Parent = bone;
                        bone.Children.Add(bones[j]);
                        currentBone = bones[j];
                        stack.Push(bone);

                        break;
                }
            }

            // Convert animations
            int numAnimations = 0;
            int nextMoveable = oldWad.GetNextMoveableWithAnimations(moveableIndex);

            if (nextMoveable == -1)
                numAnimations = oldWad.Animations.Count - oldMoveable.AnimationIndex;
            else
                numAnimations = oldWad.Moveables[nextMoveable].AnimationIndex - oldMoveable.AnimationIndex;

            for (int j = 0; j < numAnimations; j++)
            {
                if (oldMoveable.AnimationIndex == -1)
                    break;

                wad_animation oldAnimation = oldWad.Animations[j + oldMoveable.AnimationIndex];
                WadAnimation newAnimation = new WadAnimation();
                newAnimation.StateId = oldAnimation.StateId;
                newAnimation.Acceleration = oldAnimation.Accel;
                newAnimation.Speed = oldAnimation.Speed;
                newAnimation.LateralSpeed = oldAnimation.SpeedLateral;
                newAnimation.LateralAcceleration = oldAnimation.AccelLateral;
                newAnimation.FrameRate = oldAnimation.FrameDuration;
                newAnimation.NextAnimation = (ushort)(oldAnimation.NextAnimation - oldMoveable.AnimationIndex);
                newAnimation.NextFrame = oldAnimation.NextFrame;
                newAnimation.Name = "Animation " + j;

                // Fix wadmerger bug with inverted frame start/end on 0-frame anims
                ushort newFrameStart = oldAnimation.FrameStart < oldAnimation.FrameEnd ? oldAnimation.FrameStart : oldAnimation.FrameEnd;
                ushort newFrameEnd   = oldAnimation.FrameStart < oldAnimation.FrameEnd ? oldAnimation.FrameEnd : oldAnimation.FrameStart;
                newAnimation.RealNumberOfFrames = (ushort)(newFrameEnd - newFrameStart + 1);

                for (int k = 0; k < oldAnimation.NumStateChanges; k++)
                {
                    WadStateChange sc = new WadStateChange();
                    wad_state_change wadSc = oldWad.Changes[(int)oldAnimation.ChangesIndex + k];
                    sc.StateId = (ushort)wadSc.StateId;

                    for (int n = 0; n < wadSc.NumDispatches; n++)
                    {
                        WadAnimDispatch ad = new WadAnimDispatch();
                        wad_anim_dispatch wadAd = oldWad.Dispatches[(int)wadSc.DispatchesIndex + n];

                        ad.InFrame = (ushort)(wadAd.Low - newFrameStart);
                        ad.OutFrame = (ushort)(wadAd.High - newFrameStart);
                        ad.NextAnimation = (ushort)((wadAd.NextAnimation - oldMoveable.AnimationIndex) % numAnimations);
                        ad.NextFrame = (ushort)wadAd.NextFrame;

                        sc.Dispatches.Add(ad);
                    }

                    newAnimation.StateChanges.Add(sc);
                }

                if (oldAnimation.NumCommands < oldWad.Commands.Count)
                {
                    int lastCommand = oldAnimation.CommandOffset;

                    for (int k = 0; k < oldAnimation.NumCommands; k++)
                    {
                        short commandType = oldWad.Commands[lastCommand];

                        WadAnimCommand command = new WadAnimCommand();
                        command.Type = (WadAnimCommandType)commandType;
                        switch (commandType)
                        {
                            case 1:
                                command.Parameter1 = (short)oldWad.Commands[lastCommand + 1];
                                command.Parameter2 = (short)oldWad.Commands[lastCommand + 2];
                                command.Parameter3 = (short)oldWad.Commands[lastCommand + 3];

                                lastCommand += 4;
                                break;

                            case 2:
                                command.Parameter1 = (short)oldWad.Commands[lastCommand + 1];
                                command.Parameter2 = (short)oldWad.Commands[lastCommand + 2];

                                lastCommand += 3;
                                break;

                            case 3:
                                lastCommand += 1;
                                break;

                            case 4:
                                lastCommand += 1;
                                break;

                            case 5:
                                command.Parameter1 = (short)(oldWad.Commands[lastCommand + 1] - newFrameStart);
                                command.Parameter2 = (short)oldWad.Commands[lastCommand + 2];
                                lastCommand += 3;

                                // Setup sound info reference
                                int soundInfoIndex = command.Parameter2 & 0x3FFF;
                                if (soundInfoIndex >= soundInfos.Length)
                                {
                                    logger.Warn("Invalid sound with index " + soundInfoIndex + " in anim command " +
                                        commandType + ". Sound map has only " + soundInfos.Length + " entries.");
                                    continue;
                                }
                                command.SoundInfo = soundInfos[soundInfoIndex];
                                if (command.SoundInfo == null)
                                {
                                    logger.Warn("Sound with index " + (soundInfoIndex) + " missing but used by animation.");
                                    continue;
                                }
                                command.Parameter2 &= unchecked((short)0xC000); // Clear sound ID
                                break;

                            case 6:
                                command.Parameter1 = (short)(oldWad.Commands[lastCommand + 1] - newFrameStart);
                                command.Parameter2 = (short)oldWad.Commands[lastCommand + 2];
                                lastCommand += 3;
                                break;
                            default: // Ignore invalid anim commands (see for example karnak.wad)
                                logger.Warn("Invalid anim command " + commandType);
                                goto ExitForLoop;
                        }

                        newAnimation.AnimCommands.Add(command);
                    }
                    ExitForLoop:
                    ;
                }

                int frames = (int)oldAnimation.KeyFrameOffset / 2;
                uint numFrames = 0;
                if (oldAnimation.KeyFrameSize != 0)
                    if ((j + oldMoveable.AnimationIndex) == (oldWad.Animations.Count - 1))
                        numFrames = ((uint)(2 * oldWad.KeyFrames.Count) - oldAnimation.KeyFrameOffset) / (uint)(2 * oldAnimation.KeyFrameSize);
                    else
                        numFrames = (oldWad.Animations[oldMoveable.AnimationIndex + j + 1].KeyFrameOffset - oldAnimation.KeyFrameOffset) / (uint)(2 * oldAnimation.KeyFrameSize);

                for (int f = 0; f < numFrames; f++)
                {
                    WadKeyFrame frame = new WadKeyFrame();
                    int startOfFrame = frames;

                    frame.BoundingBox = new BoundingBox(new Vector3(oldWad.KeyFrames[frames],
                                                                    -oldWad.KeyFrames[frames + 2],
                                                                    oldWad.KeyFrames[frames + 4]),
                                                        new Vector3(oldWad.KeyFrames[frames + 1],
                                                                    -oldWad.KeyFrames[frames + 3],
                                                                    oldWad.KeyFrames[frames + 5]));
                    frames += 6;

                    frame.Offset = new Vector3(oldWad.KeyFrames[frames],
                                               (short)(-oldWad.KeyFrames[frames + 1]),
                                               oldWad.KeyFrames[frames + 2]);
                    frames += 3;

                    for (int n = 0; n < oldMoveable.NumPointers; n++)
                        frame.Angles.Add(WadKeyFrameRotation.FromTrAngle(ref frames, oldWad.KeyFrames, false, true));
                    if ((frames - startOfFrame) < oldAnimation.KeyFrameSize)
                        frames += ((int)oldAnimation.KeyFrameSize - (frames - startOfFrame));

                    newAnimation.KeyFrames.Add(frame);
                }

                // New velocities
                float acceleration = newAnimation.Acceleration / 65536.0f;
                newAnimation.EndVelocity = newAnimation.Speed / 65536.0f;

                float lateralAcceleration = newAnimation.LateralAcceleration / 65536.0f;
                newAnimation.EndLateralVelocity = newAnimation.LateralSpeed / 65536.0f;

                if (newAnimation.KeyFrames.Count != 0 && newAnimation.FrameRate != 0)
                {
                    newAnimation.StartVelocity = newAnimation.EndVelocity - acceleration *
                                                 (newAnimation.KeyFrames.Count + 1) * newAnimation.FrameRate;
                    newAnimation.StartLateralVelocity = newAnimation.EndLateralVelocity - lateralAcceleration *
                                                        (newAnimation.KeyFrames.Count + 1) * newAnimation.FrameRate;
                }

                // Deduce real maximum frame number, based on interpolation and keyframes.
                // We need to refer this value in NextFrame-related fixes (below) because of epic WadMerger bug,
                // which incorrectly calculates NextFrame and "steals" last frame from every custom animation.
                ushort maxFrameCount = (ushort)((newAnimation.FrameRate == 1 || numFrames < 2) ? numFrames : ((numFrames - 1) * newAnimation.FrameRate + 1));

                // Also correct animation out-point
                if (newAnimation.RealNumberOfFrames > maxFrameCount)
                    newAnimation.RealNumberOfFrames = maxFrameCount;

                frameBases.Add(newAnimation, new ushort[] { newFrameStart, (ushort)(maxFrameCount - 1) });
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
                if (frameBases[newMoveable.Animations[animation.NextAnimation]][0] != 0)
                {
                    animation.NextFrame -= frameBases[newMoveable.Animations[animation.NextAnimation]][0];
                    if (animation.NextFrame > frameBases[newMoveable.Animations[animation.NextAnimation]][1])
                        animation.NextFrame = frameBases[newMoveable.Animations[animation.NextAnimation]][1];
                }

                foreach (var stateChange in animation.StateChanges)
                    for (int j = 0; j < stateChange.Dispatches.Count; ++j)
                    {
                        WadAnimDispatch animDispatch = stateChange.Dispatches[j];

                        // HACK: Probably WadMerger's bug
                        if (animDispatch.NextAnimation > 32767)
                        {
                            animDispatch.NextAnimation = 0;
                            animDispatch.NextFrame = 0;
                            continue;
                        }

                        if (frameBases[newMoveable.Animations[animDispatch.NextAnimation]][0] != 0)
                        {
                            // HACK: In some cases dispatches have invalid NextFrame.
                            // From tests it seems that's ok to make NextFrame equal to max frame number.
                            animDispatch.NextFrame -= frameBases[newMoveable.Animations[animDispatch.NextAnimation]][0];
                            if (animDispatch.NextFrame > frameBases[newMoveable.Animations[animDispatch.NextAnimation]][1])
                                animDispatch.NextFrame = frameBases[newMoveable.Animations[animDispatch.NextAnimation]][1];
                        }
                        stateChange.Dispatches[j] = animDispatch;
                    }
            }
            //newMoveable.LinearizeSkeleton();
            wad.Moveables.Add(newMoveable.Id, newMoveable);
            return newMoveable;
        }

        internal static WadStatic ConvertTr4StaticMeshToWadStatic(Wad2 wad, Tr4Wad oldWad, int staticIndex, /*List<WadMesh> meshes*/
                                                                  Dictionary<int, WadTexture> textures)
        {
            var oldStaticMesh = oldWad.Statics[staticIndex];
            var staticMesh = new WadStatic(new WadStaticId(oldStaticMesh.ObjectId));

            //staticMesh.Name = TrCatalog.GetStaticName(WadTombRaiderVersion.TR4, oldStaticMesh.ObjectId);

            // First setup collisional and visibility bounding boxes
            staticMesh.CollisionBox = new BoundingBox(new Vector3(oldStaticMesh.CollisionX1,
                                                                  -oldStaticMesh.CollisionY1,
                                                                  oldStaticMesh.CollisionZ1),
                                                      new Vector3(oldStaticMesh.CollisionX2,
                                                                  -oldStaticMesh.CollisionY2,
                                                                  oldStaticMesh.CollisionZ2));

            staticMesh.VisibilityBox = new BoundingBox(new Vector3(oldStaticMesh.VisibilityX1,
                                                                   -oldStaticMesh.VisibilityY1,
                                                                   oldStaticMesh.VisibilityZ1),
                                                       new Vector3(oldStaticMesh.VisibilityX2,
                                                                   -oldStaticMesh.VisibilityY2,
                                                                   oldStaticMesh.VisibilityZ2));

            staticMesh.Mesh = ConvertTr4MeshToWadMesh(wad, oldWad, textures,
                                                      oldWad.Meshes[(int)oldWad.RealPointers[oldStaticMesh.PointersIndex]],
                                                      (int)oldStaticMesh.ObjectId);

            wad.Statics.Add(staticMesh.Id, staticMesh);

            return staticMesh;
        }

        private static TextureArea CalculateTr4UVCoordinates(Wad2 wad, Tr4Wad oldWad, wad_polygon poly, Dictionary<int, WadTexture> textures, bool adjustUV = false)
        {
            TextureArea textureArea = new TextureArea();
            textureArea.BlendMode = (poly.Attributes & 0x01) != 0 ? BlendMode.Additive : BlendMode.Normal;
            textureArea.DoubleSided = false;

            int textureId = GetTr4TextureIdFromPolygon(poly);
            textureArea.Texture = textures[textureId];

            // Add the UV coordinates
            int shape = (poly.Texture & 0x7000) >> 12;
            int flipped = (poly.Texture & 0x8000) >> 15;

            wad_object_texture texture = oldWad.Textures[textureId];

            // For now, 0.5px alignment is disabled, because we now use padding
            // which prevents border bleeding (yet to solve in renderer).
            float alignUV = adjustUV ? 0.5f : 0.0f;

            Vector2 nw = new Vector2(alignUV, alignUV);
            Vector2 ne = new Vector2(texture.Width + (1.0f - alignUV), alignUV);
            Vector2 se = new Vector2(texture.Width + (1.0f - alignUV), texture.Height + (1.0f - alignUV));
            Vector2 sw = new Vector2(alignUV, texture.Height + (1.0f - alignUV));

            if (poly.Shape == 9)
            {
                if (flipped == 1)
                {
                    textureArea.TexCoord0 = ne;
                    textureArea.TexCoord1 = nw;
                    textureArea.TexCoord2 = sw;
                    textureArea.TexCoord3 = se;
                }
                else
                {
                    textureArea.TexCoord0 = nw;
                    textureArea.TexCoord1 = ne;
                    textureArea.TexCoord2 = se;
                    textureArea.TexCoord3 = sw;
                }
            }
            else
            {
                switch (shape)
                {
                    case 0:
                        if (flipped == 1)
                        {
                            textureArea.TexCoord0 = ne;
                            textureArea.TexCoord1 = nw;
                            textureArea.TexCoord2 = se;
                        }
                        else
                        {
                            textureArea.TexCoord0 = nw;
                            textureArea.TexCoord1 = ne;
                            textureArea.TexCoord2 = sw;
                        }
                        break;

                    case 2:
                        if (flipped == 1)
                        {
                            textureArea.TexCoord0 = nw;
                            textureArea.TexCoord1 = sw;
                            textureArea.TexCoord2 = ne;
                        }
                        else
                        {
                            textureArea.TexCoord0 = ne;
                            textureArea.TexCoord1 = se;
                            textureArea.TexCoord2 = nw;
                        }
                        break;

                    case 4:
                        if (flipped == 1)
                        {
                            textureArea.TexCoord0 = sw;
                            textureArea.TexCoord1 = se;
                            textureArea.TexCoord2 = nw;
                        }
                        else
                        {
                            textureArea.TexCoord0 = se;
                            textureArea.TexCoord1 = sw;
                            textureArea.TexCoord2 = ne;
                        }
                        break;

                    case 6:
                        if (flipped == 1)
                        {
                            textureArea.TexCoord0 = se;
                            textureArea.TexCoord1 = ne;
                            textureArea.TexCoord2 = sw;
                        }
                        else
                        {
                            textureArea.TexCoord0 = sw;
                            textureArea.TexCoord1 = nw;
                            textureArea.TexCoord2 = se;
                        }
                        break;
                    default:
                        throw new NotImplementedException("Unknown texture shape " + shape + " found in the wad.");
                }
                textureArea.TexCoord3 = textureArea.TexCoord2;
            }

            return textureArea;
        }
    }
}
