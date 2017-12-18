using NLog;
using SharpDX;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using TombLib.Forms;
using TombLib.Utils;
using TombLib.Wad.Catalog;

namespace TombLib.Wad.Tr4Wad
{
    internal class SamplePathInfo
    {
        public string Sample { get; }
        public string Path { get; set; }
        public bool Found { get { return Path != "" && File.Exists(Path); } }
        public int Id { get; }

        public SamplePathInfo(int id, string sample)
        {
            Sample = sample;
            Id = id;
        }
    }

    public static class Tr4WadOperations
    {
        private static readonly Logger _logger = LogManager.GetCurrentClassLogger();
        private static Tr4Wad _oldWad;
        private static Wad2 _wad;
        private static List<string> _soundPaths;
        private static Dictionary<int, WadTexture> _convertedTextures;
        private static List<WadMesh> _meshes;
        private static List<SamplePathInfo> _samples;
        private static IProgressReporter _progressReporter;

        internal static List<SamplePathInfo> Samples { get { return _samples; } }
        internal static List<string> SoundPaths { get { return _soundPaths; } }

        internal static bool FindTr4Samples()
        {
            if (_samples.Count == 0)
            {
                for (var i = 0; i < _oldWad.Sounds.Count; i++)
                    _samples.Add(new SamplePathInfo(i, _oldWad.Sounds[i]));
            }

            var foundSamples = 0;
            for (var i = 0; i < _oldWad.Sounds.Count; i++)
            {
                if (_samples[i].Path != "")
                {
                    // If wave sound exists, then load it in memory
                    if (File.Exists(_samples[i].Path))
                    {
                        foundSamples++;
                        break;
                    }
                }
                else
                {
                    _samples[i].Path = "";
                }

                foreach (string soundPath in _soundPaths)
                {
                    string fileName = Path.Combine(_oldWad.BasePath, soundPath, _oldWad.Sounds[i]);

                    // If wave sound exists, then load it in memory
                    if (File.Exists(fileName))
                    {
                        _samples[i].Path = fileName;
                        foundSamples++;
                        break;
                    }
                }
            }

            return (foundSamples == _oldWad.Sounds.Count);
        }

        internal static Dictionary<int, WadTexture> ConvertTr4TexturesToWadTexture()
        {
            var textures = new ConcurrentDictionary<int, WadTexture>();

            Parallel.For(0, _oldWad.Textures.Count, i =>
              {
                  var oldTexture = _oldWad.Textures[i];
                  var texture = new WadTexture();

                  var startX = (short)(oldTexture.X);
                  var startY = (short)(oldTexture.Page * 256 + oldTexture.Y);

                  // Create the texture ImageC
                  var textureData = ImageC.CreateNew(oldTexture.Width + 1, oldTexture.Height + 1);

                  for (var y = 0; y < textureData.Height; y++)
                  {
                      for (var x = 0; x < textureData.Width; x++)
                      {
                          var baseIndex = (startY + y) * 768 + (startX + x) * 3;
                          var r = _oldWad.TexturePages[baseIndex];
                          var g = _oldWad.TexturePages[baseIndex + 1];
                          var b = _oldWad.TexturePages[baseIndex + 2];
                          var a = (byte)255;
                          
                          //var color = new ColorC(r, g, b, a);
                          textureData.SetPixel(x, y, r, g, b, a);
                      }
                  }

                  // Replace magenta color with alpha transparent black
                  textureData.ReplaceColor(new ColorC(255, 0, 255, 255), new ColorC(0, 0, 0, 0));

                  texture.Image = textureData;

                  // Update the hash of the texture
                  texture.UpdateHash();

                  textures.TryAdd(i, texture);

                  i++;
              });

            return new Dictionary<int, WadTexture>(textures);
        }

        internal static int GetTr4TextureIdFromPolygon(wad_polygon polygon)
        {
            short textureId = (short)(polygon.Texture);
            if (polygon.Shape == 8)
            {
                textureId = (short)(polygon.Texture & 0xFFF);
                if ((polygon.Texture & 0x8000) != 0) textureId = (short)(-textureId);
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

        internal static WadMesh ConvertTr4MeshToWadMesh(wad_mesh oldMesh)
        {
            WadMesh mesh = new WadMesh();

            int xMin = Int32.MaxValue;
            int yMin = Int32.MaxValue;
            int zMin = Int32.MaxValue;
            int xMax = Int32.MinValue;
            int yMax = Int32.MinValue;
            int zMax = Int32.MinValue;

            // Create the bounding sphere
            mesh.BoundingSphere = new BoundingSphere(new Vector3(oldMesh.SphereX, -oldMesh.SphereY, oldMesh.SphereZ),
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
            foreach (var oldShade in oldMesh.Shades)
            {
                mesh.VerticesShades.Add(oldShade);
            }

            // Add polygons
            foreach (var oldPoly in oldMesh.Polygons)
            {
                WadPolygon poly = new WadPolygon(oldPoly.Shape == 8 ? WadPolygonShape.Triangle : WadPolygonShape.Quad);

                // Polygon indices
                poly.Indices.Add(oldPoly.V1);
                poly.Indices.Add(oldPoly.V2);
                poly.Indices.Add(oldPoly.V3);
                if (poly.Shape == WadPolygonShape.Quad) poly.Indices.Add(oldPoly.V4);

                // Polygon special effects
                poly.ShineStrength = (byte)((oldPoly.Attributes & 0x7c) >> 2);

                // Add the texture
                poly.Texture = CalculateTr4UVCoordinates(oldPoly);

                mesh.Polys.Add(poly);
            }

            mesh.BoundingBox = new BoundingBox(minVertex, maxVertex);

            // Calculate hash
            mesh.UpdateHash();

            // Now add to the dictionary only if it doesn't contain a mesh with this hash
            if (_wad.Meshes.ContainsKey(mesh.Hash))
            {
                return _wad.Meshes[mesh.Hash];
            }
            else
            {
                _wad.Meshes.Add(mesh.Hash, mesh);
                return mesh;
            }
        }

        private static void TaskMoveablesAndStatics()
        {
            // Then convert moveables and static meshes
            ConvertTr4Meshes();

            for (int i = 0; i < _oldWad.Moveables.Count; i++)
            {
                ConvertTr4MoveableToWadMoveable(i);
                _wad.LegacyNames.Add(_oldWad.LegacyNames[i], _wad.Moveables.ElementAt(i).Value);
            }
            _logger.Info("Moveable conversion complete.");

            for (int i = 0; i < _oldWad.StaticMeshes.Count; i++)
            {
                ConvertTr4StaticMeshToWadStatic(i);
                _wad.LegacyNames.Add(_oldWad.LegacyNames[i + _oldWad.Moveables.Count + _oldWad.SpriteSequences.Count], 
                                     _wad.Statics.ElementAt(i).Value);
            }
            _logger.Info("Static mesh conversion complete.");
        }

        private static void TaskSpritesAndSounds()
        {
            // Convert sounds
            ConvertTr4Sounds();
            _logger.Info("Sound conversion complete.");

            // Convert sprites
            ConvertTr4Sprites();
            _logger.Info("Sprite conversion complete.");
        }

        private static bool ShowImportTr4WadForm()
        {
            /// Ask the help of the user
            using (var form = new ImportTr4WadDialog())
            {
                if (form.ShowDialog() == DialogResult.Cancel) return false;

                // At this point, the user is warned if there are missing files,
                // and if he accepts then we continue adding NullSample.wav for each missing sample
                FindTr4Samples();

                return true;
            }
        }

        public static Wad2 ConvertTr4Wad(Tr4Wad old, List<string> soundPaths, IProgressReporter progressReporter)
        {
            _oldWad = old;
            _wad = new Wad2(TombRaiderVersion.TR4, true);
            _soundPaths = soundPaths;
            _progressReporter = progressReporter;

            _logger.Info("Converting TR4 WAD to WAD2");

            // Try to find all samples
            _samples = new List<SamplePathInfo>();
            var result = FindTr4Samples();
            if (!result)
            {
                // In multithreading context, we must use this for not raising exceptions
                if (_progressReporter != null)
                {
                    _progressReporter.InvokeGui(delegate (IWin32Window owner)
                    {
                        result = ShowImportTr4WadForm();
                    });
                }
                else
                {
                    result = ShowImportTr4WadForm();
                }

                if (!result) return null;
            }

            // First convert all textures
            _convertedTextures = ConvertTr4TexturesToWadTexture();
            for (int i = 0; i < _convertedTextures.Count; i++)
            {
                if (!_wad.Textures.ContainsKey(_convertedTextures.ElementAt(i).Value.Hash))
                    _wad.Textures.Add(_convertedTextures.ElementAt(i).Value.Hash, _convertedTextures.ElementAt(i).Value);
            }
            _logger.Info("Texture conversion complete.");

            using (Task task1 = Task.Factory.StartNew(TaskMoveablesAndStatics))
                using (Task task2 = Task.Factory.StartNew(TaskSpritesAndSounds))
                    Task.WaitAll(task1, task2);

            return _wad;
        }

        internal static void ConvertTr4Sprites()
        {
            int spriteDataSize = _oldWad.SpriteData.Length;

            // Load the real sprite texture data
            int numSpriteTexturePages = spriteDataSize / 196608;
            if ((spriteDataSize % 196608) != 0)
                numSpriteTexturePages++;

            foreach (var oldSequence in _oldWad.SpriteSequences)
            {
                int lengthOfSequence = -oldSequence.NegativeLength;
                int startIndex = oldSequence.Offset;

                var newSequence = new WadSpriteSequence();
                newSequence.ObjectID = (uint)oldSequence.ObjectID;
                newSequence.Name = TrCatalog.GetSpriteName(TombRaiderVersion.TR4, (uint)oldSequence.ObjectID);

                for (int i = startIndex; i < startIndex + lengthOfSequence; i++)
                {
                    var oldSpriteTexture = _oldWad.SpriteTextures[i];

                    var spriteWidth = oldSpriteTexture.Width + 1;
                    var spriteHeight = oldSpriteTexture.Height + 1;
                    var spriteX = oldSpriteTexture.X;
                    var spriteY = oldSpriteTexture.Y;
                    var spritePage = ImageC.CreateNew(spriteWidth, spriteHeight);

                    for (int y = 0; y < spriteHeight; y++)
                        for (int x = 0; x < spriteWidth; x++)
                        {
                            int baseIndex = oldSpriteTexture.Tile * 196608 + 768 * (y + spriteY) + 3 * (x + spriteX);

                            byte b = _oldWad.SpriteData[baseIndex + 0];
                            byte g = _oldWad.SpriteData[baseIndex + 1];
                            byte r = _oldWad.SpriteData[baseIndex + 2];

                            if (r == 255 & g == 0 && b == 255)
                                spritePage.SetPixel(x, y, 0, 0, 0, 0);
                            else
                                spritePage.SetPixel(x, y, b, g, r, 255);
                        }

                    // Create the texture
                    var texture = new WadSprite();
                    texture.Image = spritePage;
                    texture.UpdateHash();

                    // Check if texture already exists in Wad2 and eventually add it
                    if (_wad.SpriteTextures.ContainsKey(texture.Hash))
                        texture = _wad.SpriteTextures[texture.Hash];
                    else
                        _wad.SpriteTextures.Add(texture.Hash, texture);

                    // Add current sprite to the sequence
                    newSequence.Sprites.Add(texture);
                }

                _wad.SpriteSequences.Add(newSequence);
            }
        }

        internal static void ConvertTr4Sounds()
        {
            _wad.SoundMapSize = TrCatalog.GetSoundMapSize(TombRaiderVersion.TR4, _oldWad.Version == 130);

            // Read all samples with multithreading
            var loadedSamples = new ConcurrentDictionary<int, WadSample>();
            Parallel.For(0, _oldWad.Sounds.Count, i =>
              {
                  var info = _samples[i];
                  var sampleName = info.Sample;
                  var sampleFileName = info.Path;
                  if (!info.Found)
                  {
                      sampleName = "NullSample.wav";
                      sampleFileName = "Editor\\Misc\\NullSample.wav";
                  }

                  using (var stream = File.OpenRead(sampleFileName))
                  {
                      var buffer = new byte[stream.Length];
                      stream.Read(buffer, 0, buffer.Length);
                      var sound = new WadSample(sampleName, buffer);
                      loadedSamples.TryAdd(i, sound);
                  }
              });

            for (int i = 0; i < 370; i++)
            {
                // Check if sound was used
                if (_oldWad.SoundMap[i] == -1) continue;

                var oldInfo = _oldWad.SoundInfo[_oldWad.SoundMap[i]];
                var newInfo = new WadSoundInfo();

                // Fill the new sound info
                newInfo.Name = TrCatalog.GetSoundName(TombRaiderVersion.TR4, (uint)i);
                newInfo.Volume = oldInfo.Volume;
                newInfo.Range = oldInfo.Range;
                newInfo.Chance = oldInfo.Chance;
                newInfo.Pitch = oldInfo.Pitch;
                newInfo.RandomizePitch = ((oldInfo.Characteristics & 0x2000) != 0);
                newInfo.RandomizeGain = ((oldInfo.Characteristics & 0x4000) != 0);
                newInfo.FlagN = ((oldInfo.Characteristics & 0x1000) != 0);
                newInfo.Loop = (WadSoundLoopType)(oldInfo.Characteristics & 0x03);

                int numSamplesInGroup = (oldInfo.Characteristics & 0x00fc) >> 2;

                // Read all samples linked to this sound info (for example footstep has 4 samples)
                for (int j = oldInfo.Sample; j < oldInfo.Sample + numSamplesInGroup; j++)
                {
                    if (loadedSamples.ContainsKey(j))
                    {
                        var sound = loadedSamples[j];
                        if (_wad.Samples.ContainsKey(sound.Hash))
                        {
                            newInfo.Samples.Add(_wad.Samples[sound.Hash]);
                        }
                        else
                        {
                            _wad.Samples.Add(sound.Hash, sound);
                            newInfo.Samples.Add(sound);
                        }
                    }
                    else
                    {
                        _logger.Warn("Unable to find sample '" + _oldWad.Sounds[j] + "' at any of the defined sound paths");
                    }
                }

                newInfo.UpdateHash();

                _wad.SoundInfo.Add((ushort)i, newInfo);
            }
        }

        internal static void ConvertTr4Meshes()
        {
            _meshes = new List<WadMesh>();
            foreach (var mesh in _oldWad.Meshes)
                _meshes.Add(ConvertTr4MeshToWadMesh(mesh));
        }

        internal static WadMoveable ConvertTr4MoveableToWadMoveable(int moveableIndex)
        {
            WadMoveable moveable = new WadMoveable(_wad);
            wad_moveable m = _oldWad.Moveables[moveableIndex];

            moveable.ObjectID = m.ObjectID;
            //moveable.Name = TrCatalog.GetMoveableName(TombRaiderVersion.TR4, m.ObjectID);

            for (int j = 0; j < m.NumPointers; j++)
            {
                var realPointer = (int)_oldWad.RealPointers[(int)(m.PointerIndex + j)];
                moveable.Meshes.Add(_wad.Meshes[_meshes[realPointer].Hash]);
            }

            int currentLink = (int)m.LinksIndex;

            moveable.Offset = Vector3.Zero;

            // Build the skeleton
            for (int j = 0; j < m.NumPointers - 1; j++)
            {
                WadLink link = new WadLink((WadLinkOpcode)_oldWad.Links[currentLink],
                                           new Vector3(_oldWad.Links[currentLink + 1],
                                                       -_oldWad.Links[currentLink + 2],
                                                       _oldWad.Links[currentLink + 3]));

                currentLink += 4;

                moveable.Links.Add(link);
            }

            // Convert animations
            int numAnimations = 0;
            int nextMoveable = _oldWad.GetNextMoveableWithAnimations(moveableIndex);

            if (nextMoveable == -1)
                numAnimations = _oldWad.Animations.Count - m.AnimationIndex;
            else
                numAnimations = _oldWad.Moveables[nextMoveable].AnimationIndex - m.AnimationIndex;

            for (int j = 0; j < numAnimations; j++)
            {
                if (m.AnimationIndex == -1)
                    break;

                WadAnimation animation = new WadAnimation();
                wad_animation anim = _oldWad.Animations[j + m.AnimationIndex];
                animation.Acceleration = anim.Accel;
                animation.Speed = anim.Speed;
                animation.LateralSpeed = anim.SpeedLateral;
                animation.LateralAcceleration = anim.AccelLateral;
                animation.FrameDuration = anim.FrameDuration;
                animation.FrameStart = anim.FrameStart;
                animation.FrameEnd = anim.FrameEnd;
                animation.NextAnimation = (ushort)(anim.NextAnimation - m.AnimationIndex);
                animation.NextFrame = anim.NextFrame;
                animation.StateId = anim.StateId;
                animation.RealNumberOfFrames = (ushort)(anim.FrameEnd - anim.FrameStart + 1);
                animation.Name = "Animation " + j;

                for (int k = 0; k < anim.NumStateChanges; k++)
                {
                    WadStateChange sc = new WadStateChange();
                    wad_state_change wadSc = _oldWad.Changes[(int)anim.ChangesIndex + k];
                    sc.StateId = (ushort)wadSc.StateId;

                    for (int n = 0; n < wadSc.NumDispatches; n++)
                    {
                        WadAnimDispatch ad = new WadAnimDispatch();
                        wad_anim_dispatch wadAd = _oldWad.Dispatches[(int)wadSc.DispatchesIndex + n];

                        ad.InFrame = (ushort)(wadAd.Low - anim.FrameStart);
                        ad.OutFrame = (ushort)(wadAd.High - anim.FrameStart);
                        ad.NextAnimation = (ushort)((wadAd.NextAnimation - m.AnimationIndex) % numAnimations);
                        ad.NextFrame = (ushort)wadAd.NextFrame;

                        sc.Dispatches.Add(ad);
                    }

                    animation.StateChanges.Add(sc);
                }

                if (anim.NumCommands < _oldWad.Commands.Count)
                {
                    int lastCommand = anim.CommandOffset;

                    for (int k = 0; k < anim.NumCommands; k++)
                    {
                        short commandType = _oldWad.Commands[lastCommand + 0];

                        // Ignore invalid anim commands (see for example karnak.wad)
                        if (commandType < 1 || commandType > 6) continue;

                        WadAnimCommand command = new WadAnimCommand((WadAnimCommandType)commandType);

                        switch (commandType)
                        {
                            case 1:
                                command.Parameter1 = (ushort)_oldWad.Commands[lastCommand + 1];
                                command.Parameter2 = (ushort)_oldWad.Commands[lastCommand + 2];
                                command.Parameter3 = (ushort)_oldWad.Commands[lastCommand + 3];

                                lastCommand += 4;
                                break;

                            case 2:
                                command.Parameter1 = (ushort)_oldWad.Commands[lastCommand + 1];
                                command.Parameter2 = (ushort)_oldWad.Commands[lastCommand + 2];

                                lastCommand += 3;
                                break;

                            case 3:
                                lastCommand += 1;
                                break;

                            case 4:
                                lastCommand += 1;
                                break;

                            case 5:
                                command.Parameter1 = (ushort)(_oldWad.Commands[lastCommand + 1] - anim.FrameStart);
                                command.Parameter2 = (ushort)_oldWad.Commands[lastCommand + 2];
                                lastCommand += 3;
                                break;

                            case 6:
                                command.Parameter1 = (ushort)(_oldWad.Commands[lastCommand + 1] - anim.FrameStart);
                                command.Parameter2 = (ushort)_oldWad.Commands[lastCommand + 2];
                                lastCommand += 3;
                                break;
                        }

                        animation.AnimCommands.Add(command);
                    }
                }

                int frames = (int)anim.KeyFrameOffset / 2;
                uint numFrames;

                if (j + m.AnimationIndex == _oldWad.Animations.Count - 1)
                {
                    if (anim.KeyFrameSize == 0)
                        numFrames = 0;
                    else
                        numFrames = ((uint)(2 * _oldWad.KeyFrames.Count) - anim.KeyFrameOffset) / (uint)(2 * anim.KeyFrameSize);
                }
                else
                {
                    if (anim.KeyFrameSize == 0)
                    {
                        numFrames = 0;
                    }
                    else
                    {
                        numFrames = (_oldWad.Animations[m.AnimationIndex + j + 1].KeyFrameOffset - anim.KeyFrameOffset) / (uint)(2 * anim.KeyFrameSize);
                    }
                }

                for (int f = 0; f < numFrames; f++)
                {
                    WadKeyFrame frame = new WadKeyFrame();
                    int startOfFrame = frames;

                    frame.BoundingBox = new BoundingBox(new Vector3(_oldWad.KeyFrames[frames],
                                                                    -_oldWad.KeyFrames[frames + 2],
                                                                    _oldWad.KeyFrames[frames + 4]),
                                                        new Vector3(_oldWad.KeyFrames[frames + 1],
                                                                    -_oldWad.KeyFrames[frames + 3],
                                                                    _oldWad.KeyFrames[frames + 5]));

                    frames += 6;

                    frame.Offset = new Vector3(_oldWad.KeyFrames[frames],
                                               (short)(-_oldWad.KeyFrames[frames + 1]),
                                               _oldWad.KeyFrames[frames + 2]);

                    frames += 3;

                    for (int n = 0; n < m.NumPointers; n++)
                    {
                        short rot = _oldWad.KeyFrames[frames];
                        WadKeyFrameRotation kfAngle = new WadKeyFrameRotation();

                        switch (rot & 0xc000)
                        {
                            case 0:
                                int rotation = rot;
                                int rotation2 = _oldWad.KeyFrames[frames + 1];

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
                                int rotationX = rot & 0x3fff;

                                kfAngle.Axis = WadKeyFrameRotationAxis.AxisX;
                                kfAngle.X = rotationX;

                                break;

                            case 0x8000:
                                frames += 1;
                                int rotationY = rot & 0x3fff;

                                kfAngle.Axis = WadKeyFrameRotationAxis.AxisY;
                                kfAngle.Y = rotationY;

                                break;

                            case 0xc000:
                                int rotationZ = rot & 0x3fff;
                                frames += 1;

                                kfAngle.Axis = WadKeyFrameRotationAxis.AxisZ;
                                kfAngle.Z = rotationZ;

                                break;
                        }

                        frame.Angles.Add(kfAngle);
                    }

                    if ((frames - startOfFrame) < anim.KeyFrameSize)
                        frames += ((int)anim.KeyFrameSize - (frames - startOfFrame));

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
                        // HACK: Probably WadMerger's bug
                        if (animDispatch.NextAnimation > 32767)
                        {
                            animDispatch.NextAnimation = 0;
                            animDispatch.NextFrame = 0;
                            continue;
                        }

                        if (moveable.Animations[animDispatch.NextAnimation].FrameBase != 0)
                        {
                            ushort newFrame = (ushort)(animDispatch.NextFrame % moveable.Animations[animDispatch.NextAnimation].FrameBase);

                            // HACK: In some cases dispatches have invalid NextFrame.
                            // From tests it seems that's ok to delete the dispatch or put the NextFrame equal to zero.
                            if (newFrame > moveable.Animations[animDispatch.NextAnimation].RealNumberOfFrames) newFrame = 0;

                            animDispatch.NextFrame = newFrame;
                        }
                    }
                }

                moveable.Animations[i] = animation;
            }

            _wad.Moveables.Add(m.ObjectID, moveable);

            return moveable;
        }

        internal static WadStatic ConvertTr4StaticMeshToWadStatic(int staticIndex)
        {
            var staticMesh = new WadStatic(_wad);
            var oldStaticMesh = _oldWad.StaticMeshes[staticIndex];

            //staticMesh.Name = TrCatalog.GetStaticName(TombRaiderVersion.TR4, oldStaticMesh.ObjectId);

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

            // Then import the mesh. If it was already added, the mesh will not be added to the dictionary.
            staticMesh.Mesh = _wad.Meshes[_meshes[(int)_oldWad.RealPointers[oldStaticMesh.PointersIndex]].Hash];

            staticMesh.ObjectID = oldStaticMesh.ObjectId;

            _wad.Statics.Add(staticMesh.ObjectID, staticMesh);

            return staticMesh;
        }

        private static TextureArea CalculateTr4UVCoordinates(wad_polygon poly)
        {
            TextureArea textureArea;
            textureArea.BlendMode = (poly.Attributes & 0x01) != 0 ? BlendMode.Additive : BlendMode.Normal;
            textureArea.DoubleSided = false;

            int textureId = GetTr4TextureIdFromPolygon(poly);
            WadTexture newTexture = _convertedTextures[textureId];

            if (_wad.Textures.ContainsKey(newTexture.Hash))
            {
                textureArea.Texture = _wad.Textures[newTexture.Hash];
            }
            else
            {
                _wad.Textures.Add(newTexture.Hash, newTexture);
                textureArea.Texture = newTexture;
            }

            // Add the UV coordinates
            int shape = (poly.Texture & 0x7000) >> 12;
            int flipped = (poly.Texture & 0x8000) >> 15;

            wad_object_texture texture = _oldWad.Textures[textureId];

            Vector2 nw = new Vector2(0.5f, 0.5f);
            Vector2 ne = new Vector2(texture.Width - 0.5f, 0.5f);
            Vector2 se = new Vector2(texture.Width - 0.5f, texture.Height - 0.5f);
            Vector2 sw = new Vector2(0.5f, texture.Height - 0.5f);

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
                textureArea.TexCoord3 = new Vector2();
            }

            return textureArea;
        }
    }
}
