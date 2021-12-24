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

namespace TombLib.LevelData.Compilers.TombEngine
{
    public sealed partial class LevelCompilerTombEngine
    {
        private static readonly bool _writeDbgWadTxt = false;
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

        private TombEngineMesh ConvertWadMesh(WadMesh oldMesh, bool isStatic, string objectName, int meshIndex,
                                            bool isWaterfall = false, bool isOptics = false)
        {
            var newMesh = new TombEngineMesh
            {
                Sphere = oldMesh.BoundingSphere
            };

            var objectString = isStatic ? "Static" : "Moveable";

            if (!oldMesh.HasNormals)
            {
                _progressReporter.ReportWarn(string.Format(objectString + " {0} has a mesh with invalid lighting data. Normals will be recalculated on the fly.", objectName));
                oldMesh.CalculateNormals();
            }

            for (int i = 0; i < oldMesh.VertexPositions.Count; i++)
            {
                var pos    = oldMesh.VertexPositions[i];
                var normal = oldMesh.VertexNormals[i];
                var color  = (oldMesh.HasColors) ? oldMesh.VertexColors[i] : new Vector3(1.0f);

                var v = new TombEngineVertex() 
                { 
                    Position = new Vector3(pos.X, -pos.Y, pos.Z),
                    Normal   = Vector3.Normalize(new Vector3(normal.X, -normal.Y, normal.Z)),
                    Color    = color,
                    Bone     = meshIndex,
                    Glow     = (oldMesh.HasAttributes) ? (float)oldMesh.VertexAttributes[i].Glow / 64.0f : 0.0f,
                    Move     = (oldMesh.HasAttributes) ? (float)oldMesh.VertexAttributes[i].Move / 64.0f : 0.0f
            };

                newMesh.Vertices.Add(v);
            }

            for (int j = 0; j < oldMesh.Polys.Count; j++)
            {
                var poly = oldMesh.Polys[j];

                ushort lightingEffect = poly.Texture.BlendMode == BlendMode.Additive ? (ushort)1 : (ushort)0;
                if (poly.ShineStrength > 0)
                {
                    if (oldMesh.LightingType == WadMeshLightingType.VertexColors)
                        _progressReporter.ReportWarn("Stray shiny effect found on static " + objectName + ", face " + oldMesh.Polys.IndexOf(poly) + ". Ignoring data.");
                    else
                    {
                        lightingEffect |= 0x02;
                        lightingEffect |= (ushort)(Math.Min((byte)63, poly.ShineStrength) << 2);
                    }
                }

                // Very quirky way to identify 1st face of a waterfall in TR4-TR5 wads.
                bool topmostAndUnpadded = (j == 0) ? isWaterfall : false;

                // Check if we should merge object and room textures in same texture tiles.
                TextureDestination destination = isStatic ? TextureDestination.Static : TextureDestination.Moveable;
                
                var texture = poly.Texture;
                FixWadTextureCoordinates(ref texture);

                foreach (bool doubleSided in new[] { false, true })
                {
                    if (doubleSided && !texture.DoubleSided)
                        break;

                    if (doubleSided)
                        texture.Mirror(poly.IsTriangle);
                    var result = _textureInfoManager.AddTexture(texture, destination, poly.IsTriangle, topmostAndUnpadded);
                    if (isOptics) result.Rotation = 0; // Very ugly hack for TR4-5 binocular/target optics!

                    int[] indices = poly.IsTriangle ? new int[] { poly.Index0, poly.Index1, poly.Index2 } : 
                                                      new int[] { poly.Index0, poly.Index1, poly.Index2, poly.Index3 };

                    if (doubleSided)
                        Array.Reverse(indices);

                    var realBlendMode = texture.BlendMode;
                    if (texture.BlendMode == BlendMode.Normal)
                        realBlendMode = texture.Texture.Image.HasAlpha(TRVersion.Game.TombEngine, texture.GetRect());

                    TombEnginePolygon newPoly;
                    if (poly.IsTriangle)
                        newPoly = result.CreateTombEnginePolygon3(indices, (byte)realBlendMode, null);
                    else
                        newPoly = result.CreateTombEnginePolygon4(indices, (byte)realBlendMode, null);

                    newMesh.Polygons.Add(newPoly);

                    newMesh.Vertices[indices[0]].Polygons.Add(new NormalHelper(newPoly));
                    newMesh.Vertices[indices[1]].Polygons.Add(new NormalHelper(newPoly));
                    newMesh.Vertices[indices[2]].Polygons.Add(new NormalHelper(newPoly));
                    if (!poly.IsTriangle)
                        newMesh.Vertices[indices[3]].Polygons.Add(new NormalHelper(newPoly));

                    newPoly.Normals.Add(newMesh.Vertices[newPoly.Indices[0]].Normal);
                    newPoly.Normals.Add(newMesh.Vertices[newPoly.Indices[1]].Normal);
                    newPoly.Normals.Add(newMesh.Vertices[newPoly.Indices[2]].Normal);
                    if (!poly.IsTriangle)
                        newPoly.Normals.Add(newMesh.Vertices[newPoly.Indices[3]].Normal);
                }
            }

            _meshes.Add(newMesh);

            return newMesh;
        }

        private void PrepareMeshBuckets()
        {
            for (int i = 0; i < _meshes.Count; i++)
                PrepareMeshBuckets(_meshes[i]);
        }

        private void PrepareMeshBuckets(TombEngineMesh mesh)
        {
            var textures = _textureInfoManager.GetObjectTextures();
            mesh.Buckets = new Dictionary<TombEngineMaterial, TombEngineBucket>(new TombEngineMaterial.TombEngineMaterialComparer());
            foreach (var poly in mesh.Polygons)
            {
                var bucket = GetOrAddBucket(textures[poly.TextureId].AtlasIndex, poly.BlendMode, poly.Animated, 0, mesh.Buckets);

                var texture = textures[poly.TextureId];

                poly.AnimatedSequence = -1;
                poly.AnimatedFrame = -1;

                // We output only triangles, no quads anymore
                if (poly.Shape == TombEnginePolygonShape.Quad)
                {
                    for (int n = 0; n < 4; n++)
                    {
                        poly.TextureCoordinates.Add(texture.TexCoordFloat[n]);
                        poly.Tangents.Add(Vector3.Zero);
                        poly.Bitangents.Add(Vector3.Zero);
                    }

                    bucket.Polygons.Add(poly);
                }
                else
                {
                    for (int n = 0; n < 3; n++)
                    {
                        poly.TextureCoordinates.Add(texture.TexCoordFloat[n]);
                        poly.Tangents.Add(poly.Tangent);
                        poly.Bitangents.Add(poly.Bitangent);
                    }

                    bucket.Polygons.Add(poly);
                }
            }

            // Calculate tangents and bitangents
            for (int i = 0; i < mesh.Vertices.Count; i++)
            {
                var vertex = mesh.Vertices[i];
                var polygons = vertex.Polygons;

                for (int j = 0; j < polygons.Count; j++)
                {
                    var poly = polygons[j];

                    var e1 = mesh.Vertices[poly.Polygon.Indices[1]].Position - mesh.Vertices[poly.Polygon.Indices[0]].Position;
                    var e2 = mesh.Vertices[poly.Polygon.Indices[2]].Position - mesh.Vertices[poly.Polygon.Indices[0]].Position;

                    var uv1 = poly.Polygon.TextureCoordinates[1] - poly.Polygon.TextureCoordinates[0];
                    var uv2 = poly.Polygon.TextureCoordinates[2] - poly.Polygon.TextureCoordinates[0];

                    float r = 1.0f / (uv1.X * uv2.Y - uv1.Y * uv2.X);
                    poly.Polygon.Tangent = Vector3.Normalize((e1 * uv2.Y - e2 * uv1.Y) * r);
                    poly.Polygon.Bitangent = Vector3.Normalize((e2 * uv1.X - e1 * uv2.X) * r);
                }
            }

            // Average everything
            for (int i = 0; i < mesh.Vertices.Count; i++)
            {
                var vertex = mesh.Vertices[i];
                var polygons = vertex.Polygons;

                var tangent = Vector3.Zero;
                var bitangent = Vector3.Zero;

                for (int j = 0; j < polygons.Count; j++)
                {
                    var poly = polygons[j];
                    tangent += poly.Polygon.Tangent;
                    bitangent += poly.Polygon.Bitangent;
                }

                if (polygons.Count > 0)
                {
                    tangent = Vector3.Normalize(tangent / (float)polygons.Count);
                    bitangent = Vector3.Normalize(bitangent / (float)polygons.Count);
                }

                for (int j = 0; j < polygons.Count; j++)
                {
                    var poly = polygons[j];

                    // TODO: for now we smooth all tangents and bitangents
                    for (int k = 0; k < poly.Polygon.Indices.Count; k++)
                    {
                        int index = poly.Polygon.Indices[k];
                        if (index == i)
                        {
                            poly.Polygon.Tangents[k] = tangent;
                            poly.Polygon.Bitangents[k] = bitangent;
                            break;
                        }
                    }
                }
            }
        }

        private class AnimationTr4HelperData
        {
            public int KeyFrameOffset { get; set; }
            public int KeyFrameSize { get; set; }
        }

        public void ConvertWad2DataToTombEngine()
        {
            ReportProgress(0, "Preparing WAD data");

            SortedList<WadMoveableId, WadMoveable> moveables = _level.Settings.WadGetAllMoveables();
            SortedList<WadStaticId, WadStatic> statics = _level.Settings.WadGetAllStatics();

            // First thing build frames
            ReportProgress(1, "Building animations");
            var animationDictionary = new Dictionary<WadAnimation, int>(new ReferenceEqualityComparer<WadAnimation>());
            foreach (WadMoveable moveable in moveables.Values)
                foreach (var animation in moveable.Animations)
                { 
                    animationDictionary.Add(animation, _frames.Count);

                    foreach (var wadFrame in animation.KeyFrames)
                    {
                        var newFrame = new TombEngineKeyFrame
                        {
                            Angles = new List<Quaternion>()
                        };

                        newFrame.BoundingBox.X1 = (short)Math.Max(short.MinValue, Math.Min(short.MaxValue, wadFrame.BoundingBox.Minimum.X));
                        newFrame.BoundingBox.Y1 = (short)-Math.Max(short.MinValue, Math.Min(short.MaxValue, wadFrame.BoundingBox.Minimum.Y));
                        newFrame.BoundingBox.Z1 = (short)Math.Max(short.MinValue, Math.Min(short.MaxValue, wadFrame.BoundingBox.Minimum.Z));
                        newFrame.BoundingBox.X2 = (short)Math.Max(short.MinValue, Math.Min(short.MaxValue, wadFrame.BoundingBox.Maximum.X));
                        newFrame.BoundingBox.Y2 = (short)-Math.Max(short.MinValue, Math.Min(short.MaxValue, wadFrame.BoundingBox.Maximum.Y));
                        newFrame.BoundingBox.Z2 = (short)Math.Max(short.MinValue, Math.Min(short.MaxValue, wadFrame.BoundingBox.Maximum.Z));
                        newFrame.Offset = new Vector3(wadFrame.Offset.X, -wadFrame.Offset.Y, wadFrame.Offset.Z);

                        foreach (var oldRot in wadFrame.Angles)
                        {
                            newFrame.Angles.Add(oldRot.Quaternion);
                        }

                        _frames.Add(newFrame);
                    }
                }

            int lastAnimation = 0;
            int lastAnimDispatch = 0;
            foreach (WadMoveable oldMoveable in moveables.Values)
            {
                var newMoveable = new TombEngineMoveable();
                newMoveable.Animation = (short)(oldMoveable.Animations.Count != 0 ? lastAnimation : -1);
                newMoveable.NumMeshes = (short)(oldMoveable.Meshes.Count());
                newMoveable.ObjectID = checked((int)oldMoveable.Id.TypeId);
                newMoveable.FrameOffset = 0;

                // Add animations
                uint realFrameBase = 0;
                for (int j = 0; j < oldMoveable.Animations.Count; ++j)
                {
                    var oldAnimation = oldMoveable.Animations[j];
                    var newAnimation = new TombEngineAnimation();
                    var offset = animationDictionary[oldAnimation];

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
                    var maxFrame   = oldAnimation.GetRealNumberOfFrames(oldAnimation.KeyFrames.Count);
                    if (frameCount > maxFrame)
                        frameCount = maxFrame;

                    // Setup the final animation
                    newAnimation.FrameOffset = checked(offset);
                    newAnimation.FrameRate = oldAnimation.FrameRate;
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

                newMoveable.MeshTree = _meshTrees.Count;
                newMoveable.StartingMesh = (short)_meshes.Count;

                for (int i = 0; i < oldMoveable.Meshes.Count; i++) {
                    var wadMesh = oldMoveable.Meshes[i];
                    ConvertWadMesh(
                        wadMesh, 
                        false, 
                        oldMoveable.Id.ShortName(_level.Settings.GameVersion), 
                        i, 
                        oldMoveable.Id.IsWaterfall(_level.Settings.GameVersion), 
                        oldMoveable.Id.IsOptics(_level.Settings.GameVersion));
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

            // Convert static meshes
            int convertedStaticsCount = 0;
            ReportProgress(10, "Converting static meshes");
            foreach (WadStatic oldStaticMesh in statics.Values)
            {
                var newStaticMesh = new TombEngineStaticMesh();

                newStaticMesh.ObjectID = checked((int)oldStaticMesh.Id.TypeId);

                newStaticMesh.CollisionBox = new TombEngineBoundingBox
                {
                    X1 = (short)Math.Max(short.MinValue, Math.Min(short.MaxValue, oldStaticMesh.CollisionBox.Minimum.X)),
                    X2 = (short)Math.Max(short.MinValue, Math.Min(short.MaxValue, oldStaticMesh.CollisionBox.Maximum.X)),
                    Y1 = (short)Math.Max(short.MinValue, Math.Min(short.MaxValue, -oldStaticMesh.CollisionBox.Minimum.Y)),
                    Y2 = (short)Math.Max(short.MinValue, Math.Min(short.MaxValue, -oldStaticMesh.CollisionBox.Maximum.Y)),
                    Z1 = (short)Math.Max(short.MinValue, Math.Min(short.MaxValue, oldStaticMesh.CollisionBox.Minimum.Z)),
                    Z2 = (short)Math.Max(short.MinValue, Math.Min(short.MaxValue, oldStaticMesh.CollisionBox.Maximum.Z))
                };

                newStaticMesh.VisibilityBox = new TombEngineBoundingBox
                {
                    X1 = (short)Math.Max(short.MinValue, Math.Min(short.MaxValue, oldStaticMesh.VisibilityBox.Minimum.X)),
                    X2 = (short)Math.Max(short.MinValue, Math.Min(short.MaxValue, oldStaticMesh.VisibilityBox.Maximum.X)),
                    Y1 = (short)Math.Max(short.MinValue, Math.Min(short.MaxValue, -oldStaticMesh.VisibilityBox.Minimum.Y)),
                    Y2 = (short)Math.Max(short.MinValue, Math.Min(short.MaxValue, -oldStaticMesh.VisibilityBox.Maximum.Y)),
                    Z1 = (short)Math.Max(short.MinValue, Math.Min(short.MaxValue, oldStaticMesh.VisibilityBox.Minimum.Z)),
                    Z2 = (short)Math.Max(short.MinValue, Math.Min(short.MaxValue, oldStaticMesh.VisibilityBox.Maximum.Z))
                };

                newStaticMesh.Flags = (ushort)oldStaticMesh.Flags;
                newStaticMesh.Mesh = (short)_meshes.Count;

                // TODO! replace with customizable data from trcatalog, properties, etc?
                if (oldStaticMesh.ToString().ToLower().Contains("shatter"))
                    newStaticMesh.ShatterType = (short)ShatterType.Fragment;
                else
                    newStaticMesh.ShatterType = (short)ShatterType.None;

                newStaticMesh.ShatterSound = -1; // Default sound

                // Do not add faces and vertices to the wad, instead keep only the bounding boxes when we automatically merge the Mesh
                if (_level.Settings.FastMode || !_level.Settings.AutoStaticMeshMergeContainsStaticMesh(oldStaticMesh))
                {
                    ConvertWadMesh(
                        oldStaticMesh.Mesh, 
                        true,
                        oldStaticMesh.Id.ShortName(_level.Settings.GameVersion), 
                        0,
                        false, 
                        false);
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

            if (_level.IsNG)
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
             _soundMapSize = 4096;
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

            // Step 4: load samples
            var loadedSamples = WadSample.CompileSamples(_finalSoundInfosList, _level.Settings, false, _progressReporter);
            _finalSamplesList = loadedSamples.Values.ToList();
        }

        private void WriteSoundMetadata(BinaryWriter writer)
        {
            writer.Write((ushort)_soundMapSize);

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

                        ushort characteristics = (ushort)(3 & (int)soundDetail.LoopBehaviour);
                        characteristics |= (ushort)(soundDetail.Samples.Count << 2);
                        if (soundDetail.DisablePanning)
                            characteristics |= 0x1000;
                        if (soundDetail.RandomizePitch)
                            characteristics |= 0x2000;
                        if (soundDetail.RandomizeVolume)
                            characteristics |= 0x4000;

                        var newSoundDetail = new tr3_sound_details();
                        newSoundDetail.Sample = (ushort)lastSampleIndex;
                        newSoundDetail.Volume = (byte)Math.Round(soundDetail.Volume / 100.0f * 255.0f);
                        newSoundDetail.Range = (byte)soundDetail.RangeInSectors;
                        newSoundDetail.Chance = (byte)Math.Round((soundDetail.Chance == 100 ? 0 : soundDetail.Chance) / 100.0f * 255.0f);
                        newSoundDetail.Pitch = (byte)Math.Round(soundDetail.PitchFactor / 100.0f * 127.0f + (soundDetail.PitchFactor < 0 ? 256 : 0));
                        newSoundDetail.Characteristics = characteristics;
                        bw.WriteBlock(newSoundDetail);
                        lastSampleIndex += soundDetail.Samples.Count;
                    }
                }

                writer.Write(ms.ToArray());
            }
        }

        private void WriteSoundData(BinaryWriter writer)
        {
            var sampleRate = _limits[Limit.SoundSampleRate];

            writer.Write((uint)_finalSamplesList.Count); // Write sample count

            // We have to compress the samples first
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

        private TombEngineMesh CreateDummyWadMesh(WadMesh oldMesh)
        {
            var newMesh = new TombEngineMesh
            {
                Sphere = oldMesh.BoundingSphere
            };

            _meshes.Add(newMesh);

            return newMesh;
        }
    }
}
