using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Threading.Tasks;
using TombLib.IO;
using TombLib.Utils;
using TombLib.Wad;
using TombLib.Wad.Catalog;

namespace TombLib.LevelData.Compilers
{
    public partial class LevelCompilerClassicTR
    {
        private static readonly bool _writeDbgWadTxt = false;
        private readonly Dictionary<Hash, int> _meshPointerLookup = new Dictionary<Hash, int>();
        private int _totalMeshSize = 0;
        private List<int> _finalSelectedSoundsList;
        private List<int> _finalSoundIndicesList;
        private List<WadSoundInfo> _finalSoundInfosList;
        private List<WadSample> _finalSamplesList;
        private int _soundMapSize = 0;
        private short[] _finalSoundMap;

        private void FixWadTextureCoordinates(ref TextureArea texture)
        {
            texture.TexCoord0.X = Math.Max(0.0f, texture.TexCoord0.X);
            texture.TexCoord0.Y = Math.Max(0.0f, texture.TexCoord0.Y);
            texture.TexCoord1.X = Math.Max(0.0f, texture.TexCoord1.X);
            texture.TexCoord1.Y = Math.Max(0.0f, texture.TexCoord1.Y);
            texture.TexCoord2.X = Math.Max(0.0f, texture.TexCoord2.X);
            texture.TexCoord2.Y = Math.Max(0.0f, texture.TexCoord2.Y);
            texture.TexCoord3.X = Math.Max(0.0f, texture.TexCoord3.X);
            texture.TexCoord3.Y = Math.Max(0.0f, texture.TexCoord3.Y);

            if (!texture.TextureIsInvisible && !texture.TextureIsUnavailable)
            {
                texture.TexCoord0.X = Math.Min(texture.Texture.Image.Width, texture.TexCoord0.X);
                texture.TexCoord0.Y = Math.Min(texture.Texture.Image.Height, texture.TexCoord0.Y);
                texture.TexCoord1.X = Math.Min(texture.Texture.Image.Width , texture.TexCoord1.X);
                texture.TexCoord1.Y = Math.Min(texture.Texture.Image.Height, texture.TexCoord1.Y);
                texture.TexCoord2.X = Math.Min(texture.Texture.Image.Width , texture.TexCoord2.X);
                texture.TexCoord2.Y = Math.Min(texture.Texture.Image.Height, texture.TexCoord2.Y);
                texture.TexCoord3.X = Math.Min(texture.Texture.Image.Width, texture.TexCoord3.X);
                texture.TexCoord3.Y = Math.Min(texture.Texture.Image.Height , texture.TexCoord3.Y);
            }
        }

        private void ConvertWadMesh(WadMesh oldMesh, bool isStatic, int objectId, int meshIndex,
                                       bool isWaterfall = false, bool isOptics = false,
                                       WadMeshLightingType lightType = WadMeshLightingType.PrecalculatedGrayShades)
        {
            // Don't add already existing meshes
            var hash = Hash.FromByteArray(oldMesh.ToByteArray());
            if (_meshPointerLookup.ContainsKey(hash))
            {
                _meshPointers.Add((uint)_meshPointerLookup[hash]);
                return;
            }

            int currentMeshSize = 0;

            var newMesh = new tr_mesh
            {
                Center = new tr_vertex
                {
                    X = (short)oldMesh.BoundingSphere.Center.X,
                    Y = (short)-oldMesh.BoundingSphere.Center.Y,
                    Z = (short)oldMesh.BoundingSphere.Center.Z
                },
                Radius = (short)oldMesh.BoundingSphere.Radius
            };

            currentMeshSize += 10;

            newMesh.NumVertices = (short)oldMesh.VerticesPositions.Count;
            currentMeshSize += 2;

            newMesh.Vertices = new tr_vertex[oldMesh.VerticesPositions.Count];

            for (int j = 0; j < oldMesh.VerticesPositions.Count; j++)
            {
                var vertex = oldMesh.VerticesPositions[j];
                newMesh.Vertices[j] = new tr_vertex((short)vertex.X, (short)-vertex.Y, (short)vertex.Z);

                currentMeshSize += 6;
            }

            // FIX: the following code will check for valid normals and shades combinations.
            // As last chance, I recalculate the normals on the fly.
            bool useShades = false;
            if (isStatic)
            {
                if (lightType == WadMeshLightingType.Normals)
                {
                    if (oldMesh.VerticesNormals.Count == 0)
                    {
                        _progressReporter.ReportWarn(string.Format("Static {0} is a mesh with invalid lighting data. Normals will be recalculated on the fly.", objectId));
                        oldMesh.CalculateNormals();
                    }
                    useShades = false;
                }
                else
                {
                    if (oldMesh.VerticesColors.Count == 0)
                    {
                        if (oldMesh.VerticesNormals.Count == 0)
                        {
                            
                            _progressReporter.ReportWarn(string.Format("Static {0} is a mesh with invalid lighting data. Normals will be recalculated on the fly.", objectId));
                            oldMesh.CalculateNormals();
                        }
                        useShades = false;
                    }
                    else
                    {
                        useShades = true;
                    }
                }
            }
            else
            {
                if (oldMesh.VerticesNormals.Count == 0)
                {
                    
                    _progressReporter.ReportWarn(string.Format("Mesh {0} of Moveable {1} contains invalid lighting data. Normals will be recalculated on the fly.", meshIndex, objectId));
                    oldMesh.CalculateNormals();
                }
                useShades = false;
            }

            newMesh.NumNormals = (short)(useShades ? -oldMesh.VerticesColors.Count : oldMesh.VerticesNormals.Count);
            currentMeshSize += 2;

            if (!useShades)
            {
                newMesh.Normals = new tr_vertex[oldMesh.VerticesNormals.Count];

                for (int j = 0; j < oldMesh.VerticesNormals.Count; j++)
                {
                    Vector3 normal = oldMesh.VerticesNormals[j];
                    normal = Vector3.Normalize(normal);
                    normal *= 16300.0f;
                    newMesh.Normals[j] = new tr_vertex((short)normal.X, (short)-normal.Y, (short)normal.Z);

                    currentMeshSize += 6;
                }
            }
            else
            {
                newMesh.Lights = new short[oldMesh.VerticesColors.Count];

                for (int j = 0; j < oldMesh.VerticesColors.Count; j++)
                {
                    // HACK: Because of inconsistent TE light model (0.0f-2.0f), clamp luma to 1.0f to avoid issues with
                    // incorrect shade translations in room meshes reimported as statics.
                    newMesh.Lights[j] = (short)(8191.0f - Math.Min(oldMesh.VerticesColors[j].GetLuma(), 1.0f) * 8191.0f);
                    currentMeshSize += 2;
                }
            }

            short numQuads = 0;
            short numTriangles = 0;

            foreach (var poly in oldMesh.Polys)
            {
                if (poly.Shape == WadPolygonShape.Quad)
                    numQuads++;
                else
                    numTriangles++;
            }

            newMesh.NumTexturedQuads = numQuads;
            currentMeshSize += 2;

            newMesh.NumTexturedTriangles = numTriangles;
            currentMeshSize += 2;

            int lastQuad = 0;
            int lastTriangle = 0;

            newMesh.TexturedQuads = new tr_face4[numQuads];
            newMesh.TexturedTriangles = new tr_face3[numTriangles];

            for (int j = 0; j < oldMesh.Polys.Count; j++)
            {
                var poly = oldMesh.Polys[j];

                ushort lightingEffect = poly.Texture.BlendMode == BlendMode.Additive ? (ushort)1 : (ushort)0;
                if(poly.ShineStrength > 0)
                {
                    if (useShades && isStatic)
                        _progressReporter.ReportWarn("Stray shiny effect found on static " + objectId + ", face " + oldMesh.Polys.IndexOf(poly) + ". Ignoring data.");
                    else
                    {
                        lightingEffect |= 0x02;
                        lightingEffect |= (ushort)(Math.Min((byte)63, poly.ShineStrength) << 2);
                    }
                }

                // Very quirky way to identify 1st face of a waterfall in TR4-TR5 wads.
                bool topmostAndUnpadded = (j == 0) ? isWaterfall : false;

                // Check if we should merge object and room textures in same texture tiles.
                bool agressivePacking = _level.Settings.AgressiveTexturePacking;

                if (poly.Shape == WadPolygonShape.Quad)
                {
                    FixWadTextureCoordinates(ref poly.Texture);

                    var result = _textureInfoManager.AddTexture(poly.Texture, agressivePacking, false, topmostAndUnpadded);
                    if (isOptics) result.Rotation = 0; // Very ugly hack for TR4-5 binocular/target optics!

                    newMesh.TexturedQuads[lastQuad++] = result.CreateFace4(new ushort[] { (ushort)poly.Index0, (ushort)poly.Index1, (ushort)poly.Index2, (ushort)poly.Index3 },
                        poly.Texture.DoubleSided, lightingEffect);
                    currentMeshSize += _level.Settings.GameVersion <= TRVersion.Game.TR3 ? 10 : 12;
                }
                else
                {
                    FixWadTextureCoordinates(ref poly.Texture);

                    var result = _textureInfoManager.AddTexture(poly.Texture, agressivePacking, true, topmostAndUnpadded);
                    if (isOptics) result.Rotation = 0; // Very ugly hack for TR4-5 binocular/target optics!

                    newMesh.TexturedTriangles[lastTriangle++] = result.CreateFace3(new ushort[] {(ushort)poly.Index0, (ushort)poly.Index1, (ushort)poly.Index2 }, 
                        poly.Texture.DoubleSided, lightingEffect);
                    currentMeshSize += _level.Settings.GameVersion <= TRVersion.Game.TR3 ? 8 : 10;
                }
            }

            if (_level.Settings.GameVersion <= TRVersion.Game.TR3)
                currentMeshSize += 4; // Num colored quads and triangles

            if (currentMeshSize % 4 != 0)
            {
                currentMeshSize += 2;
            }

            newMesh.MeshSize = currentMeshSize;
            newMesh.MeshPointer = _totalMeshSize;
            _meshPointers.Add((uint)_totalMeshSize);
            _meshPointerLookup.Add(hash, _totalMeshSize);

            _totalMeshSize += currentMeshSize;

            _meshes.Add(newMesh);
        }

        private class AnimationTr4HelperData
        {
            public int KeyFrameOffset { get; set; }
            public int KeyFrameSize { get; set; }
        }

        public void ConvertWad2DataToTrData()
        {
            ReportProgress(0, "Preparing WAD data");

            SortedList<WadMoveableId, WadMoveable> moveables = _level.Settings.WadGetAllMoveables();
            SortedList<WadStaticId, WadStatic> statics = _level.Settings.WadGetAllStatics();

            // First thing build frames
            ReportProgress(1, "Building meshes and animations");
            var animationDictionary = new Dictionary<WadAnimation, AnimationTr4HelperData>(new ReferenceEqualityComparer<WadAnimation>());
            foreach (WadMoveable moveable in moveables.Values)
                foreach (var animation in moveable.Animations)
                {
                    AnimationTr4HelperData animationHelper = animationDictionary.TryAdd(animation, new AnimationTr4HelperData());
                    animationHelper.KeyFrameOffset = _frames.Count * 2;

                    // Store the frames in an intermediate data structure to pad them to the same size in the next step.
                    var unpaddedFrames = new List<short>[animation.KeyFrames.Count];
                    for (int i = 0; i < animation.KeyFrames.Count; ++i)
                    {
                        var unpaddedFrame = new List<short>();
                        WadKeyFrame wadFrame = animation.KeyFrames[i];
                        unpaddedFrames[i] = unpaddedFrame;

                        unpaddedFrame.Add((short)Math.Max(short.MinValue, Math.Min(short.MaxValue, wadFrame.BoundingBox.Minimum.X)));
                        unpaddedFrame.Add((short)Math.Max(short.MinValue, Math.Min(short.MaxValue, wadFrame.BoundingBox.Maximum.X)));
                        unpaddedFrame.Add((short)Math.Max(short.MinValue, Math.Min(short.MaxValue, -wadFrame.BoundingBox.Minimum.Y)));
                        unpaddedFrame.Add((short)Math.Max(short.MinValue, Math.Min(short.MaxValue, -wadFrame.BoundingBox.Maximum.Y)));
                        unpaddedFrame.Add((short)Math.Max(short.MinValue, Math.Min(short.MaxValue, wadFrame.BoundingBox.Minimum.Z)));
                        unpaddedFrame.Add((short)Math.Max(short.MinValue, Math.Min(short.MaxValue, wadFrame.BoundingBox.Maximum.Z)));

                        unpaddedFrame.Add((short)Math.Round(Math.Max(short.MinValue, Math.Min(short.MaxValue, wadFrame.Offset.X))));
                        unpaddedFrame.Add((short)Math.Round(Math.Max(short.MinValue, Math.Min(short.MaxValue, -wadFrame.Offset.Y))));
                        unpaddedFrame.Add((short)Math.Round(Math.Max(short.MinValue, Math.Min(short.MaxValue, wadFrame.Offset.Z))));

                        // TR1 has also the number of angles to follow
                        if (_level.Settings.GameVersion == TRVersion.Game.TR1)
                            unpaddedFrame.Add((short)wadFrame.Angles.Count);

                        foreach (var angle in wadFrame.Angles)
                            WadKeyFrameRotation.ToTrAngle(angle, unpaddedFrame,
                                _level.Settings.GameVersion == TRVersion.Game.TR1,
                                _level.Settings.GameVersion == TRVersion.Game.TR4 ||
                                _level.Settings.GameVersion == TRVersion.Game.TRNG ||
                                _level.Settings.GameVersion == TRVersion.Game.TR5 ||
                                _level.Settings.GameVersion == TRVersion.Game.TR5Main);
                    }

                    // Figure out padding of the frames
                    int longestFrame = 0;
                    foreach (List<short> unpaddedFrame in unpaddedFrames)
                        longestFrame = Math.Max(longestFrame, unpaddedFrame.Count);

                    // Add frames
                    foreach (List<short> unpaddedFrame in unpaddedFrames)
                    {
                        _frames.AddRange(unpaddedFrame);
                        if (_level.Settings.GameVersion != TRVersion.Game.TR1)
                            _frames.AddRange(Enumerable.Repeat((short)0, longestFrame - unpaddedFrame.Count));
                    }
                    animationHelper.KeyFrameSize = longestFrame;
                }

            int lastAnimation = 0;
            int lastAnimDispatch = 0;
            foreach (WadMoveable oldMoveable in moveables.Values)
            {
                var newMoveable = new tr_moveable();
                newMoveable.Animation = checked((ushort)(oldMoveable.Animations.Count != 0 ? lastAnimation : 0xffff));
                newMoveable.NumMeshes = checked((ushort)oldMoveable.Meshes.Count());
                newMoveable.ObjectID = oldMoveable.Id.TypeId;
                newMoveable.FrameOffset = 0;

                // Add animations
                uint realFrameBase = 0;
                for (int j = 0; j < oldMoveable.Animations.Count; ++j)
                {
                    var oldAnimation = oldMoveable.Animations[j];
                    var newAnimation = new tr_animation();
                    var animationHelper = animationDictionary[oldAnimation];

                    // Calculate accelerations from velocities
                    int acceleration = 0;
                    int lateralAcceleration = 0;
                    int speed = 0;
                    int lateralSpeed = 0;

                    if (oldAnimation.KeyFrames.Count != 0 && oldAnimation.FrameRate != 0)
                    {
                        acceleration = (int)Math.Round((oldAnimation.EndVelocity - oldAnimation.StartVelocity) /
                                                       ((oldAnimation.KeyFrames.Count > 1 ? oldAnimation.KeyFrames.Count - 1 : 1) * oldAnimation.FrameRate) * 65536.0f);
                        lateralAcceleration = (int)Math.Round((oldAnimation.EndLateralVelocity - oldAnimation.StartLateralVelocity) /
                                                              ((oldAnimation.KeyFrames.Count > 1 ? oldAnimation.KeyFrames.Count - 1 : 1) * oldAnimation.FrameRate) * 65536.0f);
                    }
                    speed = (int)Math.Round(oldAnimation.StartVelocity * 65536.0f);
                    lateralSpeed = (int)Math.Round(oldAnimation.StartLateralVelocity * 65536.0f);

                    // Clamp EndFrame to max. frame count as a last resort to prevent glitching animations.

                    var frameCount = oldAnimation.EndFrame + 1;
                    var maxFrame = oldAnimation.GetRealNumberOfFrames();
                    if (frameCount > maxFrame)
                        frameCount = maxFrame;

                    // Setup the final animation
                    if (j == 0)
                        newMoveable.FrameOffset = checked((uint)animationHelper.KeyFrameOffset);
                    newAnimation.FrameOffset = checked((uint)animationHelper.KeyFrameOffset);
                    newAnimation.FrameRate = oldAnimation.FrameRate;
                    newAnimation.FrameSize = checked((byte)animationHelper.KeyFrameSize);
                    newAnimation.Speed = speed;
                    newAnimation.Accel = acceleration;
                    newAnimation.SpeedLateral = lateralSpeed;
                    newAnimation.AccelLateral = lateralAcceleration;
                    newAnimation.FrameStart = unchecked((ushort)realFrameBase);
                    newAnimation.FrameEnd = unchecked((ushort)(realFrameBase + (frameCount == 0 ? 0 : frameCount - 1)));
                    newAnimation.AnimCommand = checked((ushort)_animCommands.Count);
                    newAnimation.StateChangeOffset = checked((ushort)_stateChanges.Count);
                    newAnimation.NumAnimCommands = checked((ushort)oldAnimation.AnimCommands.Count);
                    newAnimation.NumStateChanges = checked((ushort)oldAnimation.StateChanges.Count);
                    newAnimation.NextAnimation = checked((ushort)(oldAnimation.NextAnimation + lastAnimation));
                    newAnimation.NextFrame = oldAnimation.NextFrame;
                    newAnimation.StateID = oldAnimation.StateId;

                    // Add anim commands
                    foreach (var command in oldAnimation.AnimCommands)
                    {
                        switch (command.Type)
                        {
                            case WadAnimCommandType.SetPosition:
                                _animCommands.Add(0x01);

                                _animCommands.Add(command.Parameter1);
                                _animCommands.Add(command.Parameter2);
                                _animCommands.Add(command.Parameter3);

                                break;

                            case WadAnimCommandType.SetJumpDistance:
                                _animCommands.Add(0x02);

                                _animCommands.Add(command.Parameter1);
                                _animCommands.Add(command.Parameter2);

                                break;

                            case WadAnimCommandType.EmptyHands:
                                _animCommands.Add(0x03);

                                break;

                            case WadAnimCommandType.KillEntity:
                                _animCommands.Add(0x04);

                                break;

                            case WadAnimCommandType.PlaySound:
                                _animCommands.Add(0x05);

                                _animCommands.Add(unchecked((short)(command.Parameter1 + newAnimation.FrameStart)));
                                _animCommands.Add(unchecked((short)(command.Parameter2)));

                                break;

                            case WadAnimCommandType.FlipEffect:
                                _animCommands.Add(0x06);

                                _animCommands.Add(checked((short)(command.Parameter1 + newAnimation.FrameStart)));
                                _animCommands.Add(command.Parameter2);

                                break;
                        }
                    }

                    // Add state changes
                    foreach (var stateChange in oldAnimation.StateChanges)
                    {
                        var newStateChange = new tr_state_change();

                        newStateChange.AnimDispatch = checked((ushort)lastAnimDispatch);
                        newStateChange.StateID = stateChange.StateId;
                        newStateChange.NumAnimDispatches = checked((ushort)stateChange.Dispatches.Count);

                        foreach (var dispatch in stateChange.Dispatches)
                        {
                            var newAnimDispatch = new tr_anim_dispatch();

                            newAnimDispatch.Low = unchecked((ushort)(dispatch.InFrame + newAnimation.FrameStart));
                            newAnimDispatch.High = unchecked((ushort)(dispatch.OutFrame + newAnimation.FrameStart));
                            newAnimDispatch.NextAnimation = checked((ushort)(dispatch.NextAnimation + lastAnimation));
                            newAnimDispatch.NextFrame = dispatch.NextFrame;

                            _animDispatches.Add(newAnimDispatch);
                        }

                        lastAnimDispatch += stateChange.Dispatches.Count;

                        _stateChanges.Add(newStateChange);
                    }

                    _animations.Add(newAnimation);

                    realFrameBase += frameCount < 0 ? (ushort)0 : (ushort)frameCount; // FIXME: Not really needed?
                }
                lastAnimation += oldMoveable.Animations.Count;

                newMoveable.MeshTree = (uint)_meshTrees.Count;
                newMoveable.StartingMesh = (ushort)_meshPointers.Count;

                for (int i = 0; i < oldMoveable.Meshes.Count; i++)
                {
                    var wadMesh = oldMoveable.Meshes[i];
                    ConvertWadMesh(wadMesh, false, (int)oldMoveable.Id.TypeId, i, oldMoveable.Id.IsWaterfall(_level.Settings.GameVersion), oldMoveable.Id.IsOptics(_level.Settings.GameVersion));
                }

                var meshTrees = new List<tr_meshtree>();
                var usedMeshes = new List<WadMesh>();
                usedMeshes.Add(oldMoveable.Bones[0].Mesh);

                for (int b = 1; b < oldMoveable.Bones.Count; b++)
                {
                    tr_meshtree tree = new tr_meshtree();

                    tree.Opcode = (int)oldMoveable.Bones[b].OpCode;
                    tree.X = (int)oldMoveable.Bones[b].Translation.X;
                    tree.Y = (int)-oldMoveable.Bones[b].Translation.Y;
                    tree.Z = (int)oldMoveable.Bones[b].Translation.Z;

                    usedMeshes.Add(oldMoveable.Bones[b].Mesh);
                    meshTrees.Add(tree);
                }

                foreach (var meshTree in meshTrees)
                {
                    _meshTrees.Add(meshTree.Opcode);
                    _meshTrees.Add(meshTree.X);
                    _meshTrees.Add(meshTree.Y);
                    _meshTrees.Add(meshTree.Z);
                }

                _moveables.Add(newMoveable);
            }

            // Adjust NextFrame of each Animation
            for (int i = 0; i < _animations.Count; i++)
            {
                var animation = _animations[i];
                animation.NextFrame += _animations[animation.NextAnimation].FrameStart;
                _animations[i] = animation;
            }

            // Adjust NextFrame of each AnimDispatch
            for (int i = 0; i < _animDispatches.Count; i++)
            {
                var dispatch = _animDispatches[i];
                dispatch.NextFrame += _animations[dispatch.NextAnimation].FrameStart;
                _animDispatches[i] = dispatch;
            }

            _progressReporter.ReportInfo("    Number of model mesh references: " + _meshPointers.Count);
            _progressReporter.ReportInfo("    Number of unique model meshes: " + _meshPointerLookup.Count);

            // Convert static meshes
            int convertedStaticsCount = 0;
            ReportProgress(10, "Converting static meshes");
            foreach (WadStatic oldStaticMesh in statics.Values)
            {
                var newStaticMesh = new tr_staticmesh();

                newStaticMesh.ObjectID = oldStaticMesh.Id.TypeId;

                newStaticMesh.CollisionBox = new tr_bounding_box
                {
                    X1 = (short)Math.Max(short.MinValue, Math.Min(short.MaxValue, oldStaticMesh.CollisionBox.Minimum.X)),
                    X2 = (short)Math.Max(short.MinValue, Math.Min(short.MaxValue, oldStaticMesh.CollisionBox.Maximum.X)),
                    Y1 = (short)Math.Max(short.MinValue, Math.Min(short.MaxValue, -oldStaticMesh.CollisionBox.Minimum.Y)),
                    Y2 = (short)Math.Max(short.MinValue, Math.Min(short.MaxValue, -oldStaticMesh.CollisionBox.Maximum.Y)),
                    Z1 = (short)Math.Max(short.MinValue, Math.Min(short.MaxValue, oldStaticMesh.CollisionBox.Minimum.Z)),
                    Z2 = (short)Math.Max(short.MinValue, Math.Min(short.MaxValue, oldStaticMesh.CollisionBox.Maximum.Z))
                };

                newStaticMesh.VisibilityBox = new tr_bounding_box
                {
                    X1 = (short)Math.Max(short.MinValue, Math.Min(short.MaxValue, oldStaticMesh.VisibilityBox.Minimum.X)),
                    X2 = (short)Math.Max(short.MinValue, Math.Min(short.MaxValue, oldStaticMesh.VisibilityBox.Maximum.X)),
                    Y1 = (short)Math.Max(short.MinValue, Math.Min(short.MaxValue, -oldStaticMesh.VisibilityBox.Minimum.Y)),
                    Y2 = (short)Math.Max(short.MinValue, Math.Min(short.MaxValue, -oldStaticMesh.VisibilityBox.Maximum.Y)),
                    Z1 = (short)Math.Max(short.MinValue, Math.Min(short.MaxValue, oldStaticMesh.VisibilityBox.Minimum.Z)),
                    Z2 = (short)Math.Max(short.MinValue, Math.Min(short.MaxValue, oldStaticMesh.VisibilityBox.Maximum.Z))
                };

                if (_level.Settings.GameVersion > TRVersion.Game.TR3)
                    newStaticMesh.Flags = (ushort)oldStaticMesh.Flags;
                else
                    newStaticMesh.Flags = 2; // bit 0: no collision, bit 1: visibility

                newStaticMesh.Mesh = (ushort)_meshPointers.Count;

                // Do not add faces and vertices to the wad, instead keep only the bounding boxes when we automatically merge the Mesh
                if (_level.Settings.FastMode || !_level.Settings.AutoStaticMeshMergeContainsStaticMesh(oldStaticMesh))
                {
                    ConvertWadMesh(oldStaticMesh.Mesh, true, (int)oldStaticMesh.Id.TypeId,0, false, false, oldStaticMesh.LightingType);
                }
                else
                {
                    convertedStaticsCount++;
                    logger.Info("Creating Dummy Mesh for automatically Merged Mesh: " + oldStaticMesh.ToString(_level.Settings.GameVersion));
                    CreateDummyWadMesh(oldStaticMesh.Mesh);
                }
                _staticMeshes.Add(newStaticMesh);
            }
            
            if (convertedStaticsCount > 0)
                _progressReporter.ReportInfo("    Number of statics merged with room geometry: " + convertedStaticsCount);
            else
                _progressReporter.ReportInfo("    No statics to merge into room geometry.");

            if (_writeDbgWadTxt)
                using (var fileStream = new FileStream("Wad.txt", FileMode.Create, FileAccess.Write, FileShare.None))
                using (var writer = new StreamWriter(fileStream))
                {
                    int n = 0;
                    foreach (var anim in _animations)
                    {
                        writer.WriteLine("Anim #" + n);
                        writer.WriteLine("    KeyframeOffset: " + anim.FrameOffset);
                        writer.WriteLine("    FrameRate: " + anim.FrameRate);
                        writer.WriteLine("    KeyFrameSize: " + anim.FrameSize);
                        writer.WriteLine("    FrameStart: " + anim.FrameStart);
                        writer.WriteLine("    FrameEnd: " + anim.FrameEnd);
                        writer.WriteLine("    StateChangeOffset: " + anim.StateChangeOffset);
                        writer.WriteLine("    NumStateChanges: " + anim.NumStateChanges);
                        writer.WriteLine("    AnimCommand: " + anim.AnimCommand);
                        writer.WriteLine("    NumAnimCommands: " + anim.NumAnimCommands);
                        writer.WriteLine("    NextAnimation: " + anim.NextAnimation);
                        writer.WriteLine("    NextFrame: " + anim.NextFrame);
                        writer.WriteLine("    StateID: " + anim.StateID);
                        writer.WriteLine("    Speed: " + anim.Speed.ToString("X"));
                        writer.WriteLine("    Accel: " + anim.Accel.ToString("X"));
                        writer.WriteLine("    SpeedLateral: " + anim.SpeedLateral.ToString("X"));
                        writer.WriteLine("    AccelLateral: " + anim.AccelLateral.ToString("X"));
                        writer.WriteLine();

                        n++;
                    }

                    n = 0;
                    foreach (var dispatch in _animDispatches)
                    {
                        writer.WriteLine("AnimDispatch #" + n);
                        writer.WriteLine("    In: " + dispatch.Low);
                        writer.WriteLine("    Out: " + dispatch.High);
                        writer.WriteLine("    NextAnimation: " + dispatch.NextAnimation);
                        writer.WriteLine("    NextFrame: " + dispatch.NextFrame);
                        writer.WriteLine();

                        n++;
                    }

                    n = 0;
                    for (int jj = 0; jj < _meshTrees.Count; jj += 4)
                    {
                        writer.WriteLine("MeshTree #" + jj);
                        writer.WriteLine("    Op: " + _meshTrees[jj + 0]);
                        writer.WriteLine("    X: " + _meshTrees[jj + 1]);
                        writer.WriteLine("    Y: " + _meshTrees[jj + 2]);
                        writer.WriteLine("    Z: " + _meshTrees[jj + 3]);
                        writer.WriteLine();

                        n++;
                    }

                    n = 0;
                    for (int jj = 0; jj < _meshPointers.Count; jj++)
                    {
                        writer.WriteLine("MeshPointer #" + jj + ": " + _meshPointers[jj]);

                        n++;
                    }

                    n = 0;
                    foreach (var mesh in _meshes)
                    {
                        writer.WriteLine("Mesh #" + n);
                        writer.WriteLine("    Vertices: " + mesh.NumVertices);
                        writer.WriteLine("    Normals: " + mesh.NumNormals);
                        writer.WriteLine("    Polygons: " + (mesh.NumTexturedQuads + mesh.NumTexturedTriangles));
                        writer.WriteLine("    MeshPointer: " + mesh.MeshPointer);
                        writer.WriteLine();

                        n++;
                    }

                    n = 0;
                    foreach (var mov in _moveables)
                    {
                        writer.WriteLine("Moveable #" + n);
                        writer.WriteLine("    MeshTree: " + mov.MeshTree);
                        writer.WriteLine("    MeshPointer: " + mov.StartingMesh);
                        writer.WriteLine("    AnimationIndex: " + mov.Animation);
                        writer.WriteLine("    NumMeshes: " + mov.NumMeshes);
                        writer.WriteLine();

                        n++;
                    }

                    n = 0;
                    foreach (var sc in _stateChanges)
                    {
                        writer.WriteLine("StateChange #" + n);
                        writer.WriteLine("    StateID: " + sc.StateID);
                        writer.WriteLine("    NumAnimDispatches: " + sc.NumAnimDispatches);
                        writer.WriteLine("    AnimDispatch: " + sc.AnimDispatch);
                        writer.WriteLine();

                        n++;
                    }
                }
        }

        private void BuildMeshTree(WadBone bone, List<tr_meshtree> meshTrees, List<WadMesh> usedMeshes)
        {
            tr_meshtree tree = new tr_meshtree();
            tree.X = (int)bone.Translation.X;
            tree.Y = (int)-bone.Translation.Y;
            tree.Z = (int)bone.Translation.Z;
            

            if (bone.Parent == null)
                tree.Opcode = 2;
            else
            {
                if (bone.Parent.Children.Count == 1)
                    tree.Opcode = 0;
                else
                {
                    int childrenCount = bone.Parent.Children.Count;
                    if (bone.Parent.Children.IndexOf(bone) == 0)
                        tree.Opcode = 2;
                    else if (bone.Parent.Children.IndexOf(bone) == childrenCount - 1)
                        tree.Opcode = 1;
                    else
                        tree.Opcode = 3;
                }
            }

            if (bone.Parent != null)
                meshTrees.Add(tree);

            usedMeshes.Add(bone.Mesh);

            for (int i = 0; i < bone.Children.Count; i++)
                BuildMeshTree(bone.Children[i], meshTrees, usedMeshes);
        }

        private void PrepareSoundsData()
        {
            // Step 1: create the real list of sounds and indices to compile

            _finalSoundInfosList = new List<WadSoundInfo>();

            if (_level.Settings.GameVersion.UsesMainSfx())
                _finalSelectedSoundsList = new List<int>(_level.Settings.SelectedSounds);
            else
                _finalSelectedSoundsList = new List<int>(_level.Settings.SelectedAndAvailableSounds);

            foreach (var soundInfo in _level.Settings.GlobalSoundMap)
                if (_finalSelectedSoundsList.Contains(soundInfo.Id))
                {
                    // Check if sound is marked as indexed. If not, exclude it from processing.
                    if (_level.Settings.GameVersion.UsesMainSfx() && !soundInfo.Indexed)
                    {
                        _progressReporter.ReportWarn("Sound info #" + soundInfo.Id +
                            " was selected but was not set as included for TR2-3 MAIN.SFX in SoundTool. It was removed.");
                        _finalSelectedSoundsList.Remove(soundInfo.Id);
                    }
                    else
                        _finalSoundInfosList.Add(soundInfo);
                }

            // Step 2: Prepare indices list to be written (needed only for TR2-3).
            // For that, we iterate through ALL sound infos available and count how many samples
            // each of them have. Then we build a list of needed sound IDs with appropriate 
            // sample count, which is used later to store indices.
            // Indices are not necessary for TR4-5, but are for TR2-3 cause main.sfx is used.

            _finalSoundIndicesList = new List<int>();
            int currentIndex = 0;
            foreach (var sound in _level.Settings.GlobalSoundMap)
            {
                // Don't include indices for non-indexed sounds
                if (_level.Settings.GameVersion.UsesMainSfx() && !sound.Indexed) continue;

                if (_finalSoundInfosList.Contains(sound))
                    foreach (var sample in sound.Samples)
                    {
                        _finalSoundIndicesList.Add(currentIndex);
                        currentIndex++;
                    }
                else
                    currentIndex += sound.Samples.Count;
            }

            // HACK: TRNG for some reason remaps certain legacy TR object sounds into extended soundmap array.
            // There is no other way of guessing it except looking if there is a specific object in any of wads.

            if (_level.Settings.GameVersion == TRVersion.Game.TRNG)
            {
                Action<int, int, int> AddRemappedNGSound = delegate (int moveableTypeToCheck, int originalId, int remappedId)
                {
                    if (_level.Settings.Wads.Any(w => w.Wad.Moveables.Any(m => (m.Value.Id.TypeId == moveableTypeToCheck))))
                    {
                        if (!_finalSelectedSoundsList.Contains(remappedId) && _finalSelectedSoundsList.Contains(originalId))
                        {
                            _progressReporter.ReportWarn("TRNG object with ID " + moveableTypeToCheck + " was found which uses missing hardcoded sound ID " + remappedId + " in embedded soundmap. Trying to remap sound ID from legacy ID (" + originalId + ").");
                            _finalSelectedSoundsList.Add(remappedId);

                            var oldSound = _finalSoundInfosList.FirstOrDefault(snd => snd.Id == originalId);
                            if (oldSound != null)
                            {
                                var newSound = new WadSoundInfo(oldSound);
                                newSound.Id = remappedId;
                                _finalSoundInfosList.Add(newSound);
                            }
                        }
                    }
                };

                // Motorboat
                AddRemappedNGSound(465, 308, 1053);
                AddRemappedNGSound(465, 307, 1055);

                // Rubber boat
                AddRemappedNGSound(467, 308, 1423);
                AddRemappedNGSound(467, 307, 1425);
            }

            // Step 3: create the sound map
            if (_level.Settings.GameVersion == TRVersion.Game.TRNG)
                _soundMapSize = _limits[Limit.NG_SoundMapSize];
            else
                _soundMapSize = _limits[Limit.SoundMapSize];

            _finalSoundMap = Enumerable.Repeat((short)-1, _soundMapSize).ToArray<short>();
            foreach (var sound in _finalSoundInfosList)
            {
                if (sound.Id >= _soundMapSize)
                {
                    _progressReporter.ReportWarn("Sound info #" + sound.Id + " wasn't included because max. ID for this game version is " + _soundMapSize + ".");
                    continue;
                }
                _finalSoundMap[sound.Id] = (short)_finalSoundInfosList.IndexOf(sound);
            }

            // Samples aren't needed for TR2-3, skip next step
            if (_level.Settings.GameVersion.UsesMainSfx())
            {
                // Additionally warn user if he uses several sound catalogs which is incompatible with MAIN.SFX workflow.
                if (_level.Settings.SoundsCatalogs.Count > 1)
                    _progressReporter.ReportWarn("Multiple sound catalogs can't be used with TR2 and TR3. Results are unpredictable. Remove all sound catalogs but one.");
                return;
            }

            // Step 4: load samples
            var loadedSamples = WadSample.CompileSamples(_finalSoundInfosList, _level.Settings, false, _progressReporter);
            _finalSamplesList = loadedSamples.Values.ToList();
        }

        private void WriteSoundMetadata(BinaryWriter writer)
        {
            if (_level.Settings.GameVersion > TRVersion.Game.TR3)
            {
                // In TRNG and TR5Main NumDemoData is used as sound map size
                writer.Write((ushort)(_level.Settings.GameVersion == TRVersion.Game.TRNG ||
                                      _level.Settings.GameVersion == TRVersion.Game.TR5Main ? _soundMapSize : 0));
            }

            using (var ms = new MemoryStream())
            {
                using (var bw = new BinaryWriterEx(ms))
                {
                    // Write soundmap to level file
                    for (int i = 0; i < _finalSoundMap.Length; i++)
                        bw.Write(_finalSoundMap[i]);

                    // Write sound details
                    int lastSampleIndex = 0;
                    bw.Write((uint)_finalSoundInfosList.Count);
                    for (int i = 0; i < _finalSoundInfosList.Count; i++)
                    {
                        var soundDetail = _finalSoundInfosList[i];
                        
                        if (soundDetail.Samples.Count > 0x0F)
                            throw new Exception("Too many sound effects for sound info '" + soundDetail.Name + "'.");

                        ushort characteristics;

                        if (_level.Settings.GameVersion == TRVersion.Game.TR1)
                        {
                            switch (soundDetail.LoopBehaviour)
                            {
                                default:
                                case WadSoundLoopBehaviour.None:
                                case WadSoundLoopBehaviour.OneShotRewound:
                                    characteristics = 1;
                                    break;
                                case WadSoundLoopBehaviour.OneShotWait:
                                    characteristics = 0;
                                    break;
                                case WadSoundLoopBehaviour.Looped:
                                    characteristics = 2;
                                    break;
                            }
                        }
                        else
                            characteristics = (ushort)(3 & (int)soundDetail.LoopBehaviour);

                        characteristics |= (ushort)(soundDetail.Samples.Count << 2);
                        if (soundDetail.DisablePanning)
                            characteristics |= 0x1000;
                        if (soundDetail.RandomizePitch)
                            characteristics |= 0x2000;
                        if (soundDetail.RandomizeVolume)
                            characteristics |= 0x4000;

                        if (_level.Settings.GameVersion <= TRVersion.Game.TR2)
                        {
                            var newSoundDetail = new tr_sound_details();
                            newSoundDetail.Sample = (ushort)lastSampleIndex;
                            newSoundDetail.Volume = (ushort)Math.Round(soundDetail.Volume / 100.0f * 32767.0f);
                            newSoundDetail.Chance = (byte)Math.Round((soundDetail.Chance == 100 ? 0 : soundDetail.Chance) / 100.0f * 32767.0f);
                            newSoundDetail.Characteristics = characteristics;
                            bw.WriteBlock(newSoundDetail);
                        }
                        else
                        {
                            var newSoundDetail = new tr3_sound_details();
                            newSoundDetail.Sample = (ushort)lastSampleIndex;
                            newSoundDetail.Volume = (byte)Math.Round(soundDetail.Volume / 100.0f * 255.0f);
                            newSoundDetail.Range = (byte)soundDetail.RangeInSectors;
                            newSoundDetail.Chance = (byte)Math.Round((soundDetail.Chance == 100 ? 0 : soundDetail.Chance) / 100.0f * 255.0f);
                            newSoundDetail.Pitch = (byte)Math.Round(soundDetail.PitchFactor / 100.0f * 127.0f + (soundDetail.PitchFactor < 0 ? 256 : 0));
                            newSoundDetail.Characteristics = characteristics;
                            bw.WriteBlock(newSoundDetail);
                        }
                        lastSampleIndex += soundDetail.Samples.Count;
                    }

                    int maxSampleCount = _limits[Limit.SoundSampleCount];
                    if (lastSampleIndex > maxSampleCount)
                        _progressReporter.ReportWarn("Level contains " + lastSampleIndex + 
                            " samples, while maximum is " + maxSampleCount + ". Level may crash. Turn off some sounds to prevent that.");

                    // Write sample indices (not used but parsed in TR4-5)
                    if (_level.Settings.GameVersion > TRVersion.Game.TR1)
                    {
                        bw.Write((uint)_finalSoundIndicesList.Count);
                        for (int i = 0; i < _finalSoundIndicesList.Count; i++)
                            bw.Write((uint)_finalSoundIndicesList[i]);
                    }
                }

                writer.Write(ms.ToArray());
            }
        }

        private void WriteSoundData(BinaryWriter writer)
        {
            var sampleRate = _limits[Limit.SoundSampleRate];

            if (_level.Settings.GameVersion == TRVersion.Game.TR1)
            {
                // Calculate sum of all sample sizes
                int sumSize = 0;
                _finalSamplesList.ForEach(s => sumSize += s.Data.Length);
                writer.Write(sumSize);

                // Write uncompressed samples
                foreach (WadSample sample in _finalSamplesList)
                {
                    writer.Write(sample.Data, 0, 24);
                    writer.Write((uint)sampleRate);
                    writer.Write((uint)sampleRate * 2);
                    writer.Write(sample.Data, 32, sample.Data.Length - 32);
                }

                // Write sample count
                writer.Write(_finalSamplesList.Count);

                // Write offset for every sample
                sumSize = 0;
                foreach (var sample in _finalSamplesList)
                {
                    writer.Write(sumSize);
                    sumSize += sample.Data.Length;
                }
            }
            else if (_level.Settings.GameVersion.Native() == TRVersion.Game.TR5)
            {
                // Write sample count
                writer.Write((uint)_finalSamplesList.Count);

                // TR5 uses compressed MS-ADPCM samples
                byte[][] compressedSamples = new byte[_finalSamplesList.Count][];
                int[] uncompressedSizes = new int[_finalSamplesList.Count];
                Parallel.For(0, _finalSamplesList.Count, delegate (int i)
                {
                    compressedSamples[i] = _finalSamplesList[i].CompressToMsAdpcm((uint)sampleRate, out uncompressedSizes[i]);
                });
                for (int i = 0; i < _finalSamplesList.Count; ++i)
                {
                    writer.Write((uint)uncompressedSizes[i]);
                    writer.Write((uint)compressedSamples[i].Length);
                    writer.Write(compressedSamples[i]);
                }
            }
            else
            {
                // Write sample count
                writer.Write((uint)_finalSamplesList.Count);

                // Write uncompressed samples
                foreach (WadSample sample in _finalSamplesList)
                {
                    writer.Write((uint)sample.Data.Length);
                    writer.Write((uint)sample.Data.Length);
                    writer.Write(sample.Data, 0, 24);
                    writer.Write((uint)sampleRate);
                    writer.Write((uint)sampleRate);
                    writer.Write(sample.Data, 32, sample.Data.Length - 32);
                }
            }
        }

        private tr_mesh CreateDummyWadMesh(WadMesh oldMesh)
        {
            int currentMeshSize = 0;
            var newMesh = new tr_mesh
            {
                Center = new tr_vertex
                {
                    X = (short)oldMesh.BoundingSphere.Center.X,
                    Y = (short)-oldMesh.BoundingSphere.Center.Y,
                    Z = (short)oldMesh.BoundingSphere.Center.Z
                },
                Radius = (short)oldMesh.BoundingSphere.Radius
            };
            currentMeshSize += 10;
            newMesh.NumVertices = 0;
            currentMeshSize += 2;
            newMesh.Vertices = new tr_vertex[0];

            newMesh.NumNormals = 0;
            currentMeshSize += 2;
            short numQuads = 0;
            short numTriangles = 0;

            newMesh.NumTexturedQuads = numQuads;
            currentMeshSize += 2;

            newMesh.NumTexturedTriangles = numTriangles;
            currentMeshSize += 2;

            newMesh.TexturedQuads = new tr_face4[numQuads];
            newMesh.TexturedTriangles = new tr_face3[numTriangles];
            if (_level.Settings.GameVersion <= TRVersion.Game.TR3)
                currentMeshSize += 4; // Num colored quads and triangles

            if (currentMeshSize % 4 != 0)
            {
                currentMeshSize += 2;
            }

            newMesh.MeshSize = currentMeshSize;
            newMesh.MeshPointer = _totalMeshSize;
            _meshPointers.Add((uint)_totalMeshSize);

            _totalMeshSize += currentMeshSize;

            _meshes.Add(newMesh);

            return newMesh;
        }
    }
}
