using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TombLib.IO;
using TombLib.Utils;

namespace TombLib.Wad.TrLevels
{
    // This is a class for loading objects data from original TR levels.
    // We are interested only in meshes, animations, textures.
    // Everything else will be ignored.
    public class TrLevel
    {
        private TrVersion _version;

        private byte[] _textureMap;
        private List<tr_mesh> _meshes = new List<tr_mesh>();
        private List<uint> _meshPointers = new List<uint>();
        private List<tr_animation> _animations = new List<tr_animation>();
        private List<tr_state_change> _stateChanges = new List<tr_state_change>();
        private List<tr_anim_dispatch> _animDispatches = new List<tr_anim_dispatch>();
        private List<ushort> _animCommands = new List<ushort>();
        private List<int> _meshTrees = new List<int>();
        private List<short> _frames = new List<short>();
        private List<tr_moveable> _moveables = new List<tr_moveable>();
        private List<tr_staticmesh> _staticMeshes = new List<tr_staticmesh>();

        private List<tr_sprite_texture> _spriteTextures = new List<tr_sprite_texture>();
        private List<tr_sprite_sequence> _spriteSequences = new List<tr_sprite_sequence>();

        public bool LoadLevel(string fileName)
        {
            using (var reader = new BinaryReaderEx(File.OpenRead(fileName)))
            {
                _version = (TrVersion)reader.ReadUInt32();
                if (_version == TrVersion.TR4 && fileName.ToLower().Trim().EndsWith(".trc")) _version = TrVersion.TR5;

                var palette8 = new tr_color[256];
                var palette16 = new tr_color4[256];

                // Read the palette only for TR2 and TR3, TR1 has the palette near the end of the file
                if (_version == TrVersion.TR2 || _version == TrVersion.TR3)
                {
                    reader.ReadBlockArray(out palette8, 256);
                    reader.ReadBlockArray(out palette16, 256);
                }

                byte[] texture8;
                byte[] texture16;
                byte[] texture32;

                // Read 8 bit and 16 bit textures if version is <= TR3
                if (_version == TrVersion.TR1 || _version == TrVersion.TR2 || _version == TrVersion.TR3)
                {
                    uint numTextureTiles = reader.ReadUInt32();
                    texture8 = reader.ReadBytes((int)numTextureTiles * 65536);
                    if (_version != TrVersion.TR1) texture16 = reader.ReadBytes((int)numTextureTiles * 131072);

                    // Later I will convert textures to 32 bit format
                }

                byte[] levelData;

                // Read 16 and 32 bit textures and uncompress them if TR4 and TR5
                if (_version == TrVersion.TR4 || _version == TrVersion.TR5)
                {
                    var numRoomTiles = reader.ReadUInt16();
                    var numObjectTiles = reader.ReadUInt16();
                    var numBumpTiles = reader.ReadUInt16();

                    // 32 bit textures
                    var uncompressedSize = reader.ReadUInt32();
                    var compressedSize = reader.ReadUInt32();
                    _textureMap = reader.ReadBytes((int)compressedSize);
                    _textureMap = ZLib.DecompressData(_textureMap);

                    // 16 bit textures (not needed)
                    uncompressedSize = reader.ReadUInt32();
                    compressedSize = reader.ReadUInt32();
                    reader.ReadBytes((int)compressedSize);

                    // Misc textures (not needed?)
                    uncompressedSize = reader.ReadUInt32();
                    compressedSize = reader.ReadUInt32();
                    reader.ReadBytes((int)compressedSize);
                }

                // Put the level geometry into a byte array
                if (_version == TrVersion.TR1 || _version == TrVersion.TR2 || _version == TrVersion.TR3)
                {
                    levelData = reader.ReadBytes((int)(reader.BaseStream.Length - reader.BaseStream.Position));
                }
                else if (_version == TrVersion.TR4)
                {
                    var uncompressedSize = reader.ReadUInt32();
                    var compressedSize = reader.ReadUInt32();
                    levelData = reader.ReadBytes((int)compressedSize);
                    levelData = ZLib.DecompressData(levelData);
                }
                else
                {
                    reader.ReadBytes(32);

                    var uncompressedSize = reader.ReadUInt32();
                    var compressedSize = reader.ReadUInt32();
                    levelData = reader.ReadBytes((int)compressedSize);
                }

                // Now store the level data in a new stream
                using (var stream = new MemoryStream(levelData))
                {
                    using (var levelReader = new BinaryReaderEx(stream))
                    {
                        var unused = levelReader.ReadUInt32();
                        var numRooms = (_version != TrVersion.TR5 ? levelReader.ReadUInt16() : levelReader.ReadUInt32());

                        for (int i = 0; i < numRooms; i++)
                        {
                            // We'll skip quickly this section
                            if (_version != TrVersion.TR5)
                            {
                                // Room info
                                levelReader.ReadBytes(16);

                                var numDataWords = levelReader.ReadUInt32();
                                levelReader.ReadBytes((int)numDataWords * 2);

                                var numPortals = levelReader.ReadUInt16();
                                levelReader.ReadBytes(numPortals * 32);

                                var numXsectors = levelReader.ReadUInt16();
                                var numZsectors = levelReader.ReadUInt16();
                                levelReader.ReadBytes(numXsectors * numZsectors * 8);

                                // Ambient intensity 1 & 2
                                levelReader.ReadUInt16();
                                if (_version != TrVersion.TR1) levelReader.ReadUInt16();

                                // Lightmode
                                if (_version == TrVersion.TR2) levelReader.ReadUInt16();

                                var numLights = reader.ReadUInt16();
                                if (_version == TrVersion.TR1) levelReader.ReadBytes(numLights * 18);
                                if (_version == TrVersion.TR2) levelReader.ReadBytes(numLights * 24);
                                if (_version == TrVersion.TR3) levelReader.ReadBytes(numLights * 24);
                                if (_version == TrVersion.TR4) levelReader.ReadBytes(numLights * 46);

                                var numRoomStaticMeshes = levelReader.ReadUInt16();
                                if (_version == TrVersion.TR1)
                                    levelReader.ReadBytes(numRoomStaticMeshes * 18);
                                else
                                    levelReader.ReadBytes(numRoomStaticMeshes * 20);

                                // Various flags and alternate room
                                if (_version == TrVersion.TR1) levelReader.ReadBytes(4);
                                if (_version == TrVersion.TR2) levelReader.ReadBytes(4);
                                if (_version == TrVersion.TR3) levelReader.ReadBytes(7);
                                if (_version == TrVersion.TR4) levelReader.ReadBytes(7);
                            }
                            else
                            {
                                // TR5 is very different, but luckly we have a field with the total size

                                // XELA
                                var xela = System.Text.ASCIIEncoding.ASCII.GetString(levelReader.ReadBytes(4));

                                var roomDataSize = levelReader.ReadUInt32();
                                levelReader.BaseStream.Seek(roomDataSize, SeekOrigin.Current);
                            }
                        }

                        // Floordata
                        var numFloorData = levelReader.ReadUInt32();
                        levelReader.ReadBytes((int)numFloorData * 2);

                        var numMeshData = levelReader.ReadUInt32();
                        var numMeshes = 0;
                        var numBytes = 0;
                        var totalBytes = 0;
                        var l = 0;
                        var temp = 0;

                        _meshes = new List<tr_mesh>();

                        while (totalBytes < (numMeshData * 2))
                        {
                            long offset1 = levelReader.BaseStream.Position;

                            var mesh = new tr_mesh();

                            mesh.Center = new tr_vertex(levelReader.ReadInt16(), levelReader.ReadInt16(), levelReader.ReadInt16());
                            mesh.Radius = levelReader.ReadInt32();
                            numBytes += 10;

                            var numVertices = levelReader.ReadUInt16();
                            levelReader.ReadBlockArray(out mesh.Vertices, numVertices);
                            numBytes += 2 + 6 * numVertices;

                            var numNormals = levelReader.ReadInt16();
                            if (numNormals > 0)
                            {
                                levelReader.ReadBlockArray(out mesh.Normals, numNormals);
                                numBytes += 2 + 6 * numNormals;
                            }
                            else
                            {
                                levelReader.ReadBlockArray(out mesh.Lights, -numNormals);
                                numBytes += 2 - 2 * numNormals;
                            }

                            var numTexturedRectangles = 0;
                            var numColoredRectangles = 0;
                            var numTexturedTriangles = 0;
                            var numColoredTriangles = 0;

                            numTexturedRectangles = levelReader.ReadUInt16();
                            mesh.TexturedQuads = new tr_face4[numTexturedRectangles];
                            for (int i = 0; i < numTexturedRectangles; i++)
                            {
                                var poly = new tr_face4();
                                poly.Vertices = new ushort[4];
                                for (int j = 0; j < 4; j++)
                                    poly.Vertices[j] = levelReader.ReadUInt16();
                                poly.Texture = levelReader.ReadUInt16();
                                if (_version == TrVersion.TR4 || _version == TrVersion.TR5)
                                    poly.LightingEffect = levelReader.ReadUInt16();
                                mesh.TexturedQuads[i] = poly;
                            }

                            numTexturedTriangles = levelReader.ReadUInt16();
                            mesh.TexturedTriangles = new tr_face3[numTexturedTriangles];
                            for (int i = 0; i < numTexturedTriangles; i++)
                            {
                                var poly = new tr_face3();
                                poly.Vertices = new ushort[3];
                                for (int j = 0; j < 3; j++)
                                    poly.Vertices[j] = levelReader.ReadUInt16();
                                poly.Texture = levelReader.ReadUInt16();
                                if (_version == TrVersion.TR4 || _version == TrVersion.TR5)
                                    poly.LightingEffect = levelReader.ReadUInt16();
                                mesh.TexturedTriangles[i] = poly;
                            }

                            if (_version == TrVersion.TR1 || _version == TrVersion.TR2 || _version == TrVersion.TR3)
                            {
                                numColoredRectangles = levelReader.ReadUInt16();
                                mesh.ColoredRectangles = new tr_face4[numColoredRectangles];
                                for (int i = 0; i < numColoredRectangles; i++)
                                {
                                    var poly = new tr_face4();
                                    poly.Vertices = new ushort[4];
                                    for (int j = 0; j < 4; j++)
                                        poly.Vertices[j] = levelReader.ReadUInt16();
                                    poly.Texture = levelReader.ReadUInt16();
                                    mesh.ColoredRectangles[i] = poly;
                                }

                                numColoredTriangles = levelReader.ReadUInt16();
                                mesh.ColoredTriangles = new tr_face3[numColoredTriangles];
                                for (int i = 0; i < numColoredTriangles; i++)
                                {
                                    var poly = new tr_face3();
                                    poly.Vertices = new ushort[3];
                                    for (int j = 0; j < 3; j++)
                                        poly.Vertices[j] = levelReader.ReadUInt16();
                                    poly.Texture = levelReader.ReadUInt16();
                                    mesh.ColoredTriangles[i] = poly;
                                }
                            }

                            if (_version == TrVersion.TR1 || _version == TrVersion.TR2 || _version == TrVersion.TR3)
                            {
                                numBytes += 2 + numTexturedRectangles * 10;
                                numBytes += 2 + numTexturedTriangles * 8;
                                numBytes += 2 + numColoredRectangles * 10;
                                numBytes += 2 + numColoredTriangles * 8;
                            }
                            else
                            {
                                numBytes += 2 + numTexturedRectangles * 12;
                                numBytes += 2 + numTexturedTriangles * 10;
                            }

                            long offset2 = levelReader.BaseStream.Position;
                            int diff = (int)(offset2 - offset1);
                            if (diff % 4 != 0)
                            { levelReader.ReadUInt16(); diff += 2; }

                            mesh.MeshSize = numBytes;
                            mesh.MeshPointer = totalBytes;

                            totalBytes += diff;
                            numBytes = 0;
                            l++;

                            _meshes.Add(mesh);
                        }

                        var numMeshPointers = levelReader.ReadUInt32();
                        _meshPointers = new List<uint>();
                        for (int i = 0; i < numMeshPointers; i++)
                            _meshPointers.Add(levelReader.ReadUInt32());

                        var numAnimations = levelReader.ReadUInt32();
                        _animations = new List<tr_animation>();
                        for (int i = 0; i < numAnimations; i++)
                        {
                            var animation = new tr_animation();
                            animation.FrameOffset = levelReader.ReadUInt32();
                            animation.FrameRate = levelReader.ReadByte();
                            animation.FrameSize = levelReader.ReadByte();
                            animation.StateID = levelReader.ReadUInt16();
                            animation.Speed = levelReader.ReadInt32();
                            animation.Accel = levelReader.ReadInt32();
                            if (_version == TrVersion.TR4 || _version == TrVersion.TR5)
                            {
                                animation.SpeedLateral = levelReader.ReadInt32();
                                animation.AccelLateral = levelReader.ReadInt32();
                            }
                            animation.FrameStart = levelReader.ReadUInt16();
                            animation.FrameEnd = levelReader.ReadUInt16();
                            animation.NextAnimation = levelReader.ReadUInt16();
                            animation.NextFrame = levelReader.ReadUInt16();
                            animation.NumStateChanges = levelReader.ReadUInt16();
                            animation.StateChangeOffset = levelReader.ReadUInt16();
                            animation.NumAnimCommands = levelReader.ReadUInt16();
                            animation.AnimCommand = levelReader.ReadUInt16();
                            _animations.Add(animation);
                        }

                        var numStateChanges = levelReader.ReadUInt32();
                        _stateChanges = new List<tr_state_change>();
                        for (int i = 0; i < numStateChanges; i++)
                        {
                            var stateChange = new tr_state_change();
                            stateChange.StateID = levelReader.ReadUInt16();
                            stateChange.NumAnimDispatches = levelReader.ReadUInt16();
                            stateChange.AnimDispatch = levelReader.ReadUInt16();
                            _stateChanges.Add(stateChange);
                        }

                        var numDispatches = levelReader.ReadUInt32();
                        _animDispatches = new List<tr_anim_dispatch>();
                        for (int i = 0; i < numDispatches; i++)
                        {
                            var dispatch = new tr_anim_dispatch();
                            dispatch.Low = levelReader.ReadUInt16();
                            dispatch.High = levelReader.ReadUInt16();
                            dispatch.NextAnimation = levelReader.ReadUInt16();
                            dispatch.NextFrame = levelReader.ReadUInt16();
                            _animDispatches.Add(dispatch);
                        }

                        var numAnimCommands = levelReader.ReadUInt32();
                        _animCommands = new List<ushort>();
                        for (int i = 0; i < numAnimCommands; i++)
                            _animCommands.Add(levelReader.ReadUInt16());

                        var numMeshTrees = levelReader.ReadUInt32();
                        _meshTrees = new List<int>();
                        for (int i = 0; i < numMeshTrees; i++)
                            _meshTrees.Add(levelReader.ReadInt32());

                        var numFrames = levelReader.ReadUInt32();
                        _frames = new List<short>();
                        for (int i = 0; i < numFrames; i++)
                            _frames.Add(levelReader.ReadInt16());

                        var numMoveables = levelReader.ReadUInt32();
                        _moveables = new List<tr_moveable>();
                        for (int i = 0; i < numMoveables; i++)
                        {
                            var moveable = new tr_moveable();
                            moveable.ObjectID = levelReader.ReadUInt32();
                            moveable.NumMeshes = levelReader.ReadUInt16();
                            moveable.StartingMesh = levelReader.ReadUInt16();
                            moveable.MeshTree = levelReader.ReadUInt32();
                            moveable.FrameOffset = levelReader.ReadUInt32();
                            moveable.Animation = levelReader.ReadUInt16();

                            if (_version == TrVersion.TR5) levelReader.ReadUInt16();

                            _moveables.Add(moveable);
                        }

                        var numStaticMeshes = levelReader.ReadUInt32();
                        _staticMeshes = new List<tr_staticmesh>();
                        for (int i = 0; i < numStaticMeshes; i++)
                        {
                            var staticMesh = new tr_staticmesh();
                            staticMesh.ObjectID = levelReader.ReadUInt32();
                            staticMesh.Mesh = levelReader.ReadUInt16();

                            var visibilityBox = new tr_bounding_box();
                            visibilityBox.X1 = reader.ReadInt16();
                            visibilityBox.X2 = reader.ReadInt16();
                            visibilityBox.Y1 = reader.ReadInt16();
                            visibilityBox.Y2 = reader.ReadInt16();
                            visibilityBox.Z1 = reader.ReadInt16();
                            visibilityBox.Z2 = reader.ReadInt16();

                            var collisionBox = new tr_bounding_box();
                            collisionBox.X1 = reader.ReadInt16();
                            collisionBox.X2 = reader.ReadInt16();
                            collisionBox.Y1 = reader.ReadInt16();
                            collisionBox.Y2 = reader.ReadInt16();
                            collisionBox.Z1 = reader.ReadInt16();
                            collisionBox.Z2 = reader.ReadInt16();

                            staticMesh.VisibilityBox = visibilityBox;
                            staticMesh.CollisionBox = collisionBox;

                            staticMesh.Flags = levelReader.ReadUInt16();

                            _staticMeshes.Add(staticMesh);
                        }

                        // If version <= TR2 object textures are here
                        if (_version == TrVersion.TR1 || _version == TrVersion.TR2)
                        {
                            var numObjectTextures = reader.ReadUInt32();

                        }
                    }
                }
            }

            return true;
        }
    }
}
