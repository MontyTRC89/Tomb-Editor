using SharpDX;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TombLib.Wad;

namespace TombEditor.Compilers
{
    public partial class LevelCompilerTr4
    {
        private static readonly bool _writeDbgWadTxt = false;
        private Dictionary<WadMesh, int> __meshPointers = new Dictionary<WadMesh, int>();

        private void ConvertWadMeshes(Wad2 wad)
        {
            // Build a list of meshes used by waterfalls
            var waterfallMeshes = new List<WadMesh>();
            foreach (var moveable in wad.Moveables)
            {
                var mov = moveable.Value;
                if ((wad.Version == TombRaiderVersion.TR4 && mov.ObjectID >= 423 && mov.ObjectID <= 425) ||
                    (wad.Version == TombRaiderVersion.TR5 && mov.ObjectID >= 410 && mov.ObjectID <= 415))
                {
                    foreach (var mesh in mov.Meshes)
                        if (!waterfallMeshes.Contains(mesh))
                            waterfallMeshes.Add(mesh);
                }
            }

            ReportProgress(11, "Converting WAD meshes to TR4 format");
            ReportProgress(11, "    Number of meshes: " + wad.Meshes.Count);

            int currentMeshSize = 0;
            int totalMeshSize = 0;

            for (int i = 0; i < wad.Meshes.Count; i++)
            {
                var oldMesh = wad.Meshes.ElementAt(i).Value;

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

                short numRectangles = 0;
                short numTriangles = 0;

                foreach (var poly in oldMesh.Polys)
                {
                    if (poly.Shape == WadPolygonShape.Quad)
                        numRectangles++;
                    else
                        numTriangles++;
                }

                newMesh.NumTexturedQuads = numRectangles;
                currentMeshSize += 2;

                newMesh.NumTexturedTriangles = numTriangles;
                currentMeshSize += 2;

                int lastRectangle = 0;
                int lastTriangle = 0;

                newMesh.TexturedQuads = new tr_face4[numRectangles];
                newMesh.TexturedTriangles = new tr_face3[numTriangles];

                foreach (var poly in oldMesh.Polys)
                {
                    if (poly.Shape == WadPolygonShape.Quad)
                    {
                        tr_face4 face = new tr_face4();

                        face.Vertices = new ushort[4];
                        face.Vertices[0] = (ushort)poly.Indices[0];
                        face.Vertices[1] = (ushort)poly.Indices[1];
                        face.Vertices[2] = (ushort)poly.Indices[2];
                        face.Vertices[3] = (ushort)poly.Indices[3];

                        var result = _objectTextureManager.AddTexture(poly.Texture, false, false, waterfallMeshes.Contains(oldMesh));
                        face.Texture = result.ObjectTextureIndex;

                        face.LightingEffect = (poly.Texture.BlendMode == TombLib.Utils.BlendMode.Additive) ? (ushort)1 : (ushort)0;
                        face.LightingEffect |= (ushort)(poly.ShineStrength << 1);

                        newMesh.TexturedQuads[lastRectangle] = face;

                        currentMeshSize += 12;
                        lastRectangle++;
                    }
                    else
                    {
                        tr_face3 face = new tr_face3();

                        face.Vertices = new ushort[3];
                        face.Vertices[0] = (ushort)poly.Indices[0];
                        face.Vertices[1] = (ushort)poly.Indices[1];
                        face.Vertices[2] = (ushort)poly.Indices[2];

                        var result = _objectTextureManager.AddTexture(poly.Texture, true, false, waterfallMeshes.Contains(oldMesh));
                        face.Texture = result.ObjectTextureIndex;

                        face.LightingEffect = (poly.Texture.BlendMode == TombLib.Utils.BlendMode.Additive) ? (ushort)1 : (ushort)0;
                        face.LightingEffect |= (ushort)(poly.ShineStrength << 1);

                        newMesh.TexturedTriangles[lastTriangle] = face;

                        currentMeshSize += 10;
                        lastTriangle++;
                    }
                }

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

        public void ConvertWad2DataToTr4(Wad2 wad)
        {
            int lastAnimation = 0;
            int lastAnimDispatch = 0;

            // First thing build frames
            var keyframesDictionary = new Dictionary<WadKeyFrame, uint>();

            int currentKeyFrameSize = 0;
            int totalKeyFrameSize = 0;
            int mmm = 0;

            for (int i = 0; i < wad.Moveables.Count; i++)
            {
                foreach (var animation in wad.Moveables.ElementAt(i).Value.Animations)
                {
                    animation.KeyFramesOffset = totalKeyFrameSize * 2;

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

                        _frames.Add((short)keyframe.BoundingBox.Minimum.X);
                        _frames.Add((short)keyframe.BoundingBox.Maximum.X);
                        _frames.Add((short)-keyframe.BoundingBox.Minimum.Y);
                        _frames.Add((short)-keyframe.BoundingBox.Maximum.Y);
                        _frames.Add((short)keyframe.BoundingBox.Minimum.Z);
                        _frames.Add((short)keyframe.BoundingBox.Maximum.Z);

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
                                    rotation16 = unchecked((short)0x4000);
                                    rotX = (short)angle.X;
                                    rotation16 |= rotX;

                                    _frames.Add(rotation16);
                                    //Console.WriteLine(rotation16.ToString("X"));

                                    currentKeyFrameSize += 1;

                                    break;

                                case WadKeyFrameRotationAxis.AxisY:
                                    rotation16 = unchecked((short)0x8000);
                                    rotY = (short)angle.Y;
                                    rotation16 |= rotY;

                                    _frames.Add(rotation16);
                                    //Console.WriteLine(rotation16.ToString("X"));

                                    currentKeyFrameSize += 1;

                                    break;

                                case WadKeyFrameRotationAxis.AxisZ:
                                    rotation16 = unchecked((short)0xc000);
                                    rotZ = (short)angle.Z;
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
                            for (int p = 0; p < (maxKeyFrameSize - currentKeyFrameSize); p++)
                            {
                                _frames.Add(0);
                            }

                            currentKeyFrameSize += maxKeyFrameSize - currentKeyFrameSize;
                        }

                        int endFrame = _frames.Count;

                        if (mmm == 0)
                        {
                            for (int jjj = baseFrame; jjj < endFrame; jjj++)
                                Console.WriteLine(_frames[jjj].ToString("X"));
                            Console.WriteLine("----------------------------");
                        }

                        keyframesDictionary.Add(keyframe, (uint)totalKeyFrameSize);
                        totalKeyFrameSize += currentKeyFrameSize;
                    }

                    animation.KeyFramesSize = maxKeyFrameSize;

                    mmm++;
                }
            }

            for (int i = 0; i < wad.Moveables.Count; i++)
            {
                var oldMoveable = wad.Moveables.ElementAt(i).Value;
                var newMoveable = new tr_moveable();

                newMoveable.Animation = (ushort)(oldMoveable.Animations.Count != 0 ? lastAnimation : -1);
                newMoveable.FrameOffset = 0;
                newMoveable.NumMeshes = (ushort)oldMoveable.Meshes.Count;
                newMoveable.ObjectID = oldMoveable.ObjectID;
                newMoveable.MeshTree = 0;
                newMoveable.StartingMesh = 0;

                // Add animations
                ushort frameBase = 0;
                for (int j = 0; j < oldMoveable.Animations.Count; ++j)
                {
                    var oldAnimation = oldMoveable.Animations[j];
                    var newAnimation = new tr_animation();

                    // Setup the final animation
                    newAnimation.FrameOffset = (uint)oldAnimation.KeyFramesOffset;
                    newAnimation.FrameRate = oldAnimation.FrameDuration;
                    newAnimation.FrameSize = (byte)oldAnimation.KeyFramesSize;
                    newAnimation.Speed = oldAnimation.Speed;
                    newAnimation.Accel = oldAnimation.Acceleration;
                    newAnimation.SpeedLateral = oldAnimation.LateralSpeed;
                    newAnimation.AccelLateral = oldAnimation.LateralAcceleration;
                    newAnimation.FrameStart = (ushort)(frameBase + oldAnimation.FrameStart);
                    newAnimation.FrameEnd = (ushort)(frameBase + oldAnimation.FrameEnd);
                    newAnimation.AnimCommand = (ushort)(_animCommands.Count);
                    newAnimation.StateChangeOffset = (ushort)(_stateChanges.Count);
                    newAnimation.NumAnimCommands = (ushort)oldAnimation.AnimCommands.Count;
                    newAnimation.NumStateChanges = (ushort)oldAnimation.StateChanges.Count;
                    newAnimation.NextAnimation = (ushort)(oldAnimation.NextAnimation + lastAnimation);
                    newAnimation.NextFrame = oldAnimation.NextFrame;
                    newAnimation.StateID = (oldAnimation.StateId);

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

                                _animCommands.Add(command.Parameter1);
                                _animCommands.Add(command.Parameter2);

                                break;

                            case WadAnimCommandType.FlipEffect:
                                _animCommands.Add(0x06);

                                _animCommands.Add(command.Parameter1);
                                _animCommands.Add(command.Parameter2);

                                break;
                        }
                    }

                    // Add state changes
                    foreach (var stateChange in oldAnimation.StateChanges)
                    {
                        var newStateChange = new tr_state_change();

                        newStateChange.AnimDispatch = (ushort)lastAnimDispatch;
                        newStateChange.StateID = stateChange.StateId;
                        newStateChange.NumAnimDispatches = (ushort)(stateChange.Dispatches.Count);

                        foreach (var dispatch in stateChange.Dispatches)
                        {
                            var newAnimDispatch = new tr_anim_dispatch();

                            newAnimDispatch.Low = (ushort)(dispatch.InFrame + newAnimation.FrameStart);
                            newAnimDispatch.High = (ushort)(dispatch.OutFrame + newAnimation.FrameStart);
                            newAnimDispatch.NextAnimation = (ushort)(dispatch.NextAnimation + lastAnimation);
                            newAnimDispatch.NextFrame = (ushort)(dispatch.NextFrame);

                            _animDispatches.Add(newAnimDispatch);
                        }

                        lastAnimDispatch += stateChange.Dispatches.Count;

                        _stateChanges.Add(newStateChange);
                    }

                    _animations.Add(newAnimation);

                    frameBase += oldAnimation.RealNumberOfFrames;
                }
                lastAnimation += oldMoveable.Animations.Count;

                newMoveable.MeshTree = (uint)_meshTrees.Count;
                newMoveable.StartingMesh = (ushort)_meshPointers.Count;

                // Now build mesh pointers and mesh trees
                foreach (var meshTree in oldMoveable.Links)
                {
                    _meshTrees.Add((int)meshTree.Opcode);
                    _meshTrees.Add((int)meshTree.Offset.X);
                    _meshTrees.Add((int)-meshTree.Offset.Y);
                    _meshTrees.Add((int)meshTree.Offset.Z);
                }

                foreach (var mesh in oldMoveable.Meshes)
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

                newStaticMesh.ObjectID = oldStaticMesh.ObjectID;

                newStaticMesh.CollisionBox = new tr_bounding_box
                {
                    X1 = (short)oldStaticMesh.CollisionBox.Minimum.X,
                    X2 = (short)oldStaticMesh.CollisionBox.Maximum.X,
                    Y1 = (short)-oldStaticMesh.CollisionBox.Minimum.Y,
                    Y2 = (short)-oldStaticMesh.CollisionBox.Maximum.Y,
                    Z1 = (short)oldStaticMesh.CollisionBox.Minimum.Z,
                    Z2 = (short)oldStaticMesh.CollisionBox.Maximum.Z
                };

                newStaticMesh.VisibilityBox = new tr_bounding_box
                {
                    X1 = (short)oldStaticMesh.VisibilityBox.Minimum.X,
                    X2 = (short)oldStaticMesh.VisibilityBox.Maximum.X,
                    Y1 = (short)-oldStaticMesh.VisibilityBox.Minimum.Y,
                    Y2 = (short)-oldStaticMesh.VisibilityBox.Maximum.Y,
                    Z1 = (short)oldStaticMesh.VisibilityBox.Minimum.Z,
                    Z2 = (short)oldStaticMesh.VisibilityBox.Maximum.Z
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
    }
}
