using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using TombLib.LevelData.Compilers.Util;
using TombLib.Utils;
using TombLib.Wad;

namespace TombLib.LevelData.Compilers
{
    public partial class LevelCompilerClassicTR
    {
        private static readonly bool _writeDbgWadTxt = false;
        private readonly Dictionary<WadMesh, int> __meshPointers = new Dictionary<WadMesh, int>();

        private void ConvertWadMeshes(Wad2 wad)
        {
            // Build a list of meshes used by waterfalls
            var meshes = new List<WadMesh>(wad.MeshesUnique);
            var waterfallMeshes = new List<WadMesh>(wad.Moveables.Values
                .Where(moveable => moveable.Id.IsWaterfall(_level.Settings.WadGameVersion))
                .SelectMany(moveable => moveable.Meshes)
                .Distinct());

            ReportProgress(11, "Converting WAD meshes to TR4 format");
            ReportProgress(11, "    Number of meshes: " + meshes.Count);

            int currentMeshSize = 0;
            int totalMeshSize = 0;

            for (int i = 0; i < meshes.Count; i++)
            {
                var oldMesh = meshes[i];

                currentMeshSize = 0;

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

                newMesh.NumNormals = (short)(oldMesh.VerticesNormals.Count > 0 ? oldMesh.VerticesNormals.Count : -oldMesh.VerticesShades.Count);
                currentMeshSize += 2;

                if (oldMesh.VerticesNormals.Count > 0)
                {
                    newMesh.Normals = new tr_vertex[oldMesh.VerticesNormals.Count];

                    for (int j = 0; j < oldMesh.VerticesNormals.Count; j++)
                    {
                        var normal = oldMesh.VerticesNormals[j];
                        newMesh.Normals[j] = new tr_vertex((short)normal.X, (short)-normal.Y, (short)normal.Z);

                        currentMeshSize += 6;
                    }
                }
                else
                {
                    newMesh.Lights = new short[oldMesh.VerticesShades.Count];

                    for (int j = 0; j < oldMesh.VerticesShades.Count; j++)
                    {
                        newMesh.Lights[j] = oldMesh.VerticesShades[j];

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

                int packPriority = waterfallMeshes.Contains(oldMesh) ? 1 : 0;
                foreach (var poly in oldMesh.Polys)
                {
                    ushort lightingEffect = poly.Texture.BlendMode == BlendMode.Additive ? (ushort)1 : (ushort)0;
                    lightingEffect |= (ushort)(poly.ShineStrength << 1);

                    if (poly.Shape == WadPolygonShape.Quad)
                    {
                        ObjectTextureManager.Result result;
                        lock (_objectTextureManager)
                            result = _objectTextureManager.AddTexture(poly.Texture, false, false, packPriority);
                        newMesh.TexturedQuads[lastQuad++] = result.CreateFace4((ushort)poly.Index0, (ushort)poly.Index1, (ushort)poly.Index2, (ushort)poly.Index3, lightingEffect);
                        currentMeshSize += _level.Settings.GameVersion <= GameVersion.TR3 ? 10 : 12;
                    }
                    else
                    {
                        ObjectTextureManager.Result result;
                        lock (_objectTextureManager)
                            result = _objectTextureManager.AddTexture(poly.Texture, true, false, packPriority);

                        newMesh.TexturedTriangles[lastTriangle++] = result.CreateFace3((ushort)poly.Index0, (ushort)poly.Index1, (ushort)poly.Index2, lightingEffect);
                        currentMeshSize += _level.Settings.GameVersion <= GameVersion.TR3 ? 8 : 10;
                    }
                }

                if (_level.Settings.GameVersion <= GameVersion.TR3) currentMeshSize += 4; // Num colored quads and triangles

                if (currentMeshSize % 4 != 0)
                {
                    currentMeshSize += 2;
                }

                newMesh.MeshSize = currentMeshSize;
                newMesh.MeshPointer = totalMeshSize;
                __meshPointers.Add(oldMesh, totalMeshSize);

                totalMeshSize += currentMeshSize;

                _meshes.Add(newMesh);
            }
        }

        private class AnimationTr4HelperData
        {
            public int KeyFramesOffset { get; set; }
            public int KeyFramesSize { get; set; }
        }

        public void ConvertWad2DataToTr4(Wad2 wad)
        {
            int lastAnimation = 0;
            int lastAnimDispatch = 0;

            // First thing build frames
            var animationDictionary = new Dictionary<WadAnimation, AnimationTr4HelperData>();
            var keyFramesDictionary = new Dictionary<WadKeyFrame, uint>();

            int currentKeyFrameSize = 0;
            int totalKeyFrameSize = 0;

            foreach (WadMoveable moveable in wad.Moveables.Values)
            {
                foreach (var animation in moveable.Animations)
                {
                    AnimationTr4HelperData animationHelper = animationDictionary.TryAdd(animation, new AnimationTr4HelperData());
                    animationHelper.KeyFramesOffset = totalKeyFrameSize * 2;

                    // First I need to calculate the max frame size because I will need to pad later with 0x00
                    int maxKeyFrameSize = 0;
                    foreach (var keyframe in animation.KeyFrames)
                    {
                        currentKeyFrameSize = 9;

                        foreach (var angle in keyframe.Angles)
                        {
                            if (angle.Axis == WadKeyFrameRotationAxis.ThreeAxes)
                                currentKeyFrameSize += 2;
                            else
                                currentKeyFrameSize += 1;
                        }

                        if (currentKeyFrameSize > maxKeyFrameSize)
                            maxKeyFrameSize = currentKeyFrameSize;
                    }

                    foreach (var keyframe in animation.KeyFrames)
                    {
                        currentKeyFrameSize = 0;
                        int baseFrame = _frames.Count;

                        _frames.Add((short)Math.Max(short.MinValue, Math.Min(short.MaxValue, keyframe.BoundingBox.Minimum.X)));
                        _frames.Add((short)Math.Max(short.MinValue, Math.Min(short.MaxValue, keyframe.BoundingBox.Maximum.X)));
                        _frames.Add((short)Math.Max(short.MinValue, Math.Min(short.MaxValue, -keyframe.BoundingBox.Minimum.Y)));
                        _frames.Add((short)Math.Max(short.MinValue, Math.Min(short.MaxValue, -keyframe.BoundingBox.Maximum.Y)));
                        _frames.Add((short)Math.Max(short.MinValue, Math.Min(short.MaxValue, keyframe.BoundingBox.Minimum.Z)));
                        _frames.Add((short)Math.Max(short.MinValue, Math.Min(short.MaxValue, keyframe.BoundingBox.Maximum.Z)));

                        currentKeyFrameSize += 6;

                        _frames.Add((short)keyframe.Offset.X);
                        _frames.Add((short)-keyframe.Offset.Y);
                        _frames.Add((short)keyframe.Offset.Z);

                        currentKeyFrameSize += 3;

                        foreach (var angle in keyframe.Angles)
                        {
                            //long rotation32 = 0;
                            short rotation16 = 0;
                            short rotX = 0;
                            short rotY = 0;
                            short rotZ = 0;

                            switch (angle.Axis)
                            {
                                case WadKeyFrameRotationAxis.ThreeAxes:
                                    rotation16 = (short)((angle.X << 4) | ((angle.Y & 0xfc0) >> 6));
                                    _frames.Add(rotation16);

                                    rotation16 = (short)(((angle.Y & 0x3f) << 10) | (angle.Z & 0x3ff));
                                    _frames.Add(rotation16);

                                    currentKeyFrameSize += 2;

                                    break;

                                case WadKeyFrameRotationAxis.AxisX:
                                    rotation16 = 0x4000;
                                    rotX = (short)angle.X;
                                    if (_level.Settings.GameVersion <= GameVersion.TR3) rotX = (short)(rotX / 4);
                                    rotation16 |= rotX;
                                    _frames.Add(rotation16);

                                    currentKeyFrameSize += 1;

                                    break;

                                case WadKeyFrameRotationAxis.AxisY:
                                    rotation16 = unchecked((short)0x8000);
                                    rotY = (short)angle.Y;
                                    if (_level.Settings.GameVersion <= GameVersion.TR3) rotY = (short)(rotY / 4);
                                    rotation16 |= rotY;

                                    _frames.Add(rotation16);

                                    currentKeyFrameSize += 1;

                                    break;

                                case WadKeyFrameRotationAxis.AxisZ:
                                    rotation16 = unchecked((short)0xc000);
                                    rotZ = (short)angle.Z;
                                    if (_level.Settings.GameVersion <= GameVersion.TR3) rotZ = (short)(rotZ / 4);
                                    rotation16 += rotZ;

                                    _frames.Add(rotation16);
                                    //Console.WriteLine(rotation16.ToString("X"));

                                    currentKeyFrameSize += 1;

                                    break;
                            }
                        }

                        // Padding
                        if (currentKeyFrameSize < maxKeyFrameSize)
                        {
                            for (int p = 0; p < maxKeyFrameSize - currentKeyFrameSize; p++)
                            {
                                _frames.Add(0);
                            }
                            currentKeyFrameSize += maxKeyFrameSize - currentKeyFrameSize;
                        }

                        int endFrame = _frames.Count;

                        keyFramesDictionary.Add(keyframe, (uint)totalKeyFrameSize);
                        totalKeyFrameSize += currentKeyFrameSize;
                    }

                    animationHelper.KeyFramesSize = maxKeyFrameSize;
                }
            }

            foreach (WadMoveable oldMoveable in wad.Moveables.Values)
            {
                var newMoveable = new tr_moveable();
                newMoveable.Animation = checked((ushort)(oldMoveable.Animations.Count != 0 ? lastAnimation : 0xffff));
                newMoveable.NumMeshes = checked((ushort)oldMoveable.Meshes.Count);
                newMoveable.ObjectID = oldMoveable.Id.TypeId;
                newMoveable.FrameOffset = 0;

                // Add animations
                uint realFrameBase = 0;
                for (int j = 0; j < oldMoveable.Animations.Count; ++j)
                {
                    var oldAnimation = oldMoveable.Animations[j];
                    var newAnimation = new tr_animation();
                    var animationHelper = animationDictionary[oldAnimation];

                    // Setup the final animation
                    if (j == 0)
                        newMoveable.FrameOffset = checked((uint)animationHelper.KeyFramesOffset);
                    newAnimation.FrameOffset = checked((uint)animationHelper.KeyFramesOffset);
                    newAnimation.FrameRate = oldAnimation.FrameDuration;
                    newAnimation.FrameSize = checked((byte)animationHelper.KeyFramesSize);
                    newAnimation.Speed = oldAnimation.Speed;
                    newAnimation.Accel = oldAnimation.Acceleration;
                    newAnimation.SpeedLateral = oldAnimation.LateralSpeed;
                    newAnimation.AccelLateral = oldAnimation.LateralAcceleration;
                    newAnimation.FrameStart = unchecked((ushort)realFrameBase);
                    newAnimation.FrameEnd = unchecked((ushort)(realFrameBase + (oldAnimation.RealNumberOfFrames == 0 ? 0 : oldAnimation.RealNumberOfFrames - 1)));
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
                            case WadAnimCommandType.PositionReference:
                                _animCommands.Add(0x01);

                                _animCommands.Add(command.Parameter1);
                                _animCommands.Add(command.Parameter2);
                                _animCommands.Add(command.Parameter3);

                                break;

                            case WadAnimCommandType.JumpReference:
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

                                ushort soundIndex = _soundManager.AllocateSoundInfo(command.SoundInfo);
                                if (soundIndex > 0x3FFF)
                                    throw new IndexOutOfRangeException("Sound index '" + soundIndex + "' too big.");
                                _animCommands.Add(checked((ushort)(command.Parameter1 + newAnimation.FrameStart)));
                                _animCommands.Add(checked((ushort)(soundIndex | (command.Parameter2 & 0xC000))));

                                break;

                            case WadAnimCommandType.FlipEffect:
                                _animCommands.Add(0x06);

                                _animCommands.Add(checked((ushort)(command.Parameter1 + newAnimation.FrameStart)));
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

                    realFrameBase += oldAnimation.RealNumberOfFrames == 0xffff ? (ushort)0 : oldAnimation.RealNumberOfFrames;
                }
                lastAnimation += oldMoveable.Animations.Count;

                var meshTrees = new List<tr_meshtree>();
                var usedMeshes = new List<WadMesh>();
                BuildMeshTree(oldMoveable.Skeleton, meshTrees, usedMeshes);

                newMoveable.MeshTree = (uint)_meshTrees.Count;
                newMoveable.StartingMesh = (ushort)_meshPointers.Count;

                foreach (var meshTree in meshTrees)
                {
                    _meshTrees.Add(meshTree.Opcode);
                    _meshTrees.Add(meshTree.X);
                    _meshTrees.Add(meshTree.Y);
                    _meshTrees.Add(meshTree.Z);
                }

                foreach (var mesh in usedMeshes)
                {
                    _meshPointers.Add((uint)__meshPointers[mesh]);
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
            for (int i = 0; i < wad.Statics.Count; i++)
            {
                var oldStaticMesh = wad.Statics.ElementAt(i).Value;
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

                newStaticMesh.Flags = (ushort)oldStaticMesh.Flags;
                newStaticMesh.Mesh = (ushort)_meshPointers.Count;

                _staticMeshes.Add(newStaticMesh);

                _meshPointers.Add((uint)__meshPointers[oldStaticMesh.Mesh]);
            }

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
            tree.X = (int)bone.Transform.Translation.X;
            tree.Y = (int)-bone.Transform.Translation.Y;
            tree.Z = (int)bone.Transform.Translation.Z;

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
    }
}
