using System.IO;
using TombLib.IO;

namespace TombEditor.Compilers
{
    public sealed partial class LevelCompilerTr4
    {
        private void WriteLevelTr4()
        {
            var wad = _editor.Level.Wad.OriginalWad;

            // Now begin to compile the geometry block in a MemoryStream
            using (var writer = new BinaryWriterEx(File.OpenWrite("temp.bin")))
            {

                ReportProgress(85, "Writing geometry data to memory buffer");

                const int filler = 0;
                writer.Write(filler);

                _numRooms = (ushort)_rooms.Length;
                writer.Write(_numRooms);

                long offset;
                long offset2;
                for (var i = 0; i < _numRooms; i++)
                {
                    writer.WriteBlock(_rooms[i].Info);

                    offset = writer.BaseStream.Position;

                    const int numdw = 0;
                    writer.Write(numdw);

                    var tmp = (ushort)_rooms[i].Vertices.Length;
                    writer.Write(tmp);
                    writer.WriteBlockArray(_rooms[i].Vertices);

                    tmp = (ushort)_rooms[i].Rectangles.Length;
                    writer.Write(tmp);
                    if (tmp != 0)
                    {
                        for (var k = 0; k < _rooms[i].Rectangles.Length; k++)
                        {
                            writer.Write(_rooms[i].Rectangles[k].Vertices[0]);
                            writer.Write(_rooms[i].Rectangles[k].Vertices[1]);
                            writer.Write(_rooms[i].Rectangles[k].Vertices[2]);
                            writer.Write(_rooms[i].Rectangles[k].Vertices[3]);
                            writer.Write(_rooms[i].Rectangles[k].Texture);
                        }
                    }

                    tmp = (ushort)_rooms[i].Triangles.Length;
                    writer.Write(tmp);
                    if (tmp != 0)
                    {
                        for (var k = 0; k < _rooms[i].Triangles.Length; k++)
                        {
                            writer.Write(_rooms[i].Triangles[k].Vertices[0]);
                            writer.Write(_rooms[i].Triangles[k].Vertices[1]);
                            writer.Write(_rooms[i].Triangles[k].Vertices[2]);
                            writer.Write(_rooms[i].Triangles[k].Texture);
                        }
                    }

                    // For sprites, not used
                    tmp = 0;
                    writer.Write(tmp);

                    // Now save current offset and calculate the size of the geometry
                    offset2 = writer.BaseStream.Position;
                    // ReSharper disable once SuggestVarOrType_BuiltInTypes
                    ushort roomGeometrySize = (ushort)((offset2 - offset - 4) / 2);

                    // Save the size of the geometry
                    writer.BaseStream.Seek(offset, SeekOrigin.Begin);
                    writer.Write(roomGeometrySize);
                    writer.BaseStream.Seek(offset2, SeekOrigin.Begin);

                    // Write portals
                    tmp = (ushort)_rooms[i].Portals.Length;
                    writer.WriteBlock(tmp);
                    if (tmp != 0)
                        writer.WriteBlockArray(_rooms[i].Portals);

                    // Write sectors
                    writer.Write(_rooms[i].NumZSectors);
                    writer.Write(_rooms[i].NumXSectors);
                    writer.WriteBlockArray(_rooms[i].Sectors);

                    // Write room color
                    writer.Write(_rooms[i].AmbientIntensity1);
                    writer.Write(_rooms[i].AmbientIntensity2);

                    // Write lights
                    tmp = (ushort)_rooms[i].Lights.Length;
                    writer.WriteBlock(tmp);
                    if (tmp != 0)
                        writer.WriteBlockArray(_rooms[i].Lights);

                    // Write static meshes
                    tmp = (ushort)_rooms[i].StaticMeshes.Length;
                    writer.WriteBlock(tmp);
                    if (tmp != 0)
                        writer.WriteBlockArray(_rooms[i].StaticMeshes);

                    // Write final data
                    writer.Write(_rooms[i].AlternateRoom);
                    writer.Write(_rooms[i].Flags);
                    writer.Write(_rooms[i].WaterScheme);
                    writer.Write(_rooms[i].ReverbInfo);
                    writer.Write(_rooms[i].AlternateGroup);
                }

                // Write floordata
                _numFloorData = (uint)_floorData.Length;
                writer.Write(_numFloorData);
                writer.WriteBlockArray(_floorData);

                // Write meshes
                offset = writer.BaseStream.Position;

                _numMeshData = 0;
                writer.Write(_numMeshData);
                var totalMeshSize = 0;

                for (var i = 0; i < _meshes.Length; i++)
                {
                    long meshOffset1 = writer.BaseStream.Position;

                    writer.WriteBlock(_meshes[i].Centre);
                    writer.Write(_meshes[i].Radius);

                    writer.Write(_meshes[i].NumVertices);
                    writer.WriteBlockArray(_meshes[i].Vertices);

                    writer.Write(_meshes[i].NumNormals);
                    if (_meshes[i].NumNormals > 0)
                    {
                        writer.WriteBlockArray(_meshes[i].Normals);
                    }
                    else
                    {
                        writer.WriteBlockArray(_meshes[i].Lights);
                    }

                    writer.Write(_meshes[i].NumTexturedRectangles);
                    writer.WriteBlockArray(_meshes[i].TexturedRectangles);

                    writer.Write(_meshes[i].NumTexturedTriangles);
                    writer.WriteBlockArray(_meshes[i].TexturedTriangles);

                    var meshOffset2 = writer.BaseStream.Position;
                    var meshSize = (meshOffset2 - meshOffset1);
                    if (meshSize % 4 != 0)
                    {
                        const ushort tempFiller = 0;
                        writer.Write(tempFiller);
                        meshSize += 2;
                    }

                    for (var n = 0; n < _numMeshPointers; n++)
                    {
                        if (wad.HelperPointers[n] == i)
                        {
                            _meshPointers[n] = (uint)totalMeshSize;
                        }
                    }

                    totalMeshSize += (int)meshSize;
                }

                offset2 = writer.BaseStream.Position;
                // ReSharper disable once SuggestVarOrType_BuiltInTypes
                uint meshDataSize = (uint)((offset2 - offset - 4) / 2);

                // Save the size of the meshes
                writer.BaseStream.Seek(offset, SeekOrigin.Begin);
                writer.Write(meshDataSize);
                writer.BaseStream.Seek(offset2, SeekOrigin.Begin);

                // Write mesh pointers
                writer.Write(_numMeshPointers);
                writer.WriteBlockArray(_meshPointers);

                // Write animations' data
                writer.Write(_numAnimations);
                writer.WriteBlockArray(_animations);

                writer.Write(_numStateChanges);
                writer.WriteBlockArray(_stateChanges);

                writer.Write(_numAnimDispatches);
                writer.WriteBlockArray(_animDispatches);

                writer.Write(_numAnimCommands);
                writer.WriteBlockArray(_animCommands);

                writer.Write(_numMeshTrees);
                writer.WriteBlockArray(_meshTrees);

                writer.Write(_numFrames);
                writer.WriteBlockArray(_frames);

                writer.Write(_numMoveables);
                writer.WriteBlockArray(_moveables);

                writer.Write(_numStaticMeshes);
                writer.WriteBlockArray(_staticMeshes);

                // SPR block
                _spr = new byte[] { 0x53, 0x50, 0x52 };
                writer.WriteBlockArray(_spr);

                writer.Write(_numSpriteTextures);
                writer.WriteBlockArray(_spriteTextures);

                writer.Write(_numSpriteSequences);
                writer.WriteBlockArray(_spriteSequences);

                // Write camera, flyby and sound sources
                writer.Write(_numCameras);
                writer.WriteBlockArray(_cameras);

                writer.Write(_numFlyByCameras);
                writer.WriteBlockArray(_flyByCameras);

                writer.Write(_numSoundSources);
                writer.WriteBlockArray(_soundSources);

                // Write pathfinding data
                writer.Write(_numBoxes);
                writer.WriteBlockArray(_boxes);

                writer.Write(_numOverlaps);
                writer.WriteBlockArray(_overlaps);

                for (var i = 0; i < _numBoxes; i++)
                    writer.Write(_zones[i].GroundZone1_Normal);
                for (var i = 0; i < _numBoxes; i++)
                    writer.Write(_zones[i].GroundZone2_Normal);
                for (var i = 0; i < _numBoxes; i++)
                    writer.Write(_zones[i].GroundZone3_Normal);
                for (var i = 0; i < _numBoxes; i++)
                    writer.Write(_zones[i].GroundZone4_Normal);
                for (var i = 0; i < _numBoxes; i++)
                    writer.Write(_zones[i].FlyZone_Normal);
                for (var i = 0; i < _numBoxes; i++)
                    writer.Write(_zones[i].GroundZone1_Alternate);
                for (var i = 0; i < _numBoxes; i++)
                    writer.Write(_zones[i].GroundZone2_Alternate);
                for (var i = 0; i < _numBoxes; i++)
                    writer.Write(_zones[i].GroundZone3_Alternate);
                for (var i = 0; i < _numBoxes; i++)
                    writer.Write(_zones[i].GroundZone4_Alternate);
                for (var i = 0; i < _numBoxes; i++)
                    writer.Write(_zones[i].FlyZone_Alternate);

                //   writer.WriteBlockArray(Zones);

                // Write animated textures
                writer.Write(_numAnimatedTextures);

                // ReSharper disable once SuggestVarOrType_BuiltInTypes
                short numSets = (short)_animatedTextures.Length;
                writer.Write(numSets);

                for (var i = 0; i < _animatedTextures.Length; i++)
                {
                    writer.Write(_animatedTextures[i].NumTextures);

                    foreach (var texture in _animatedTextures[i].Textures)
                    {
                        writer.Write(texture);
                    }
                }

                // Write object textures
                var tex = new byte[] { 0x00, 0x54, 0x45, 0x58 };
                writer.WriteBlockArray(tex);

                _objectTextures = _tempObjectTextures.ToArray();
                _numObjectTextures = (uint)_objectTextures.Length;

                writer.Write(_numObjectTextures);
                writer.WriteBlockArray(_objectTextures);

                // Write items and AI objects
                writer.Write(_numItems);
                writer.WriteBlockArray(_items);

                writer.Write(_numAiItems);
                writer.WriteBlockArray(_aiItems);

                const short numDemo = 0;
                writer.Write(numDemo);

                // Write sound data
                byte[] sampleIndices;
                byte[] soundDetails;
                byte[] soundMap;
                uint numSampleIndices;
                using (var readerSounds = new BinaryReaderEx(File.OpenRead(
                    _editor.Level.Wad.OriginalWad.BasePath + "\\" + _editor.Level.Wad.OriginalWad.BaseName + ".sfx")))
                {

                    /*byte[] sfxBuffer = readerSounds.ReadBytes((int)readerSounds.BaseStream.Length);
                    readerSounds.BaseStream.Seek(0, SeekOrigin.Begin);
                    readerSounds.ReadBytes(370 * 2);*/

                    soundMap = readerSounds.ReadBytes(370 * 2);
                    _numSoundDetails = (uint)readerSounds.ReadInt32();
                    soundDetails = readerSounds.ReadBytes((int)_numSoundDetails * 8);
                    // ReSharper disable once SuggestVarOrType_BuiltInTypes
                    numSampleIndices = (uint)readerSounds.ReadInt32();
                    sampleIndices = readerSounds.ReadBytes((int)numSampleIndices * 4);
                }

                writer.Write(soundMap);
                writer.Write(_numSoundDetails);
                writer.Write(soundDetails);
                writer.Write(numSampleIndices);
                writer.Write(sampleIndices);
                // writer.WriteBlockArray(sfxBuffer);

                writer.Write(numDemo);
                writer.Write(numDemo);
                writer.Write(numDemo);

                writer.Flush();
            }

            using (var writer = new BinaryWriterEx(File.OpenWrite(_dest)))
            {
                using (var reader = new BinaryReaderEx(File.OpenRead("temp.bin")))
                {
                    ReportProgress(90, "Writing final level");

                    var version = new byte[] { 0x54, 0x52, 0x34, 0x00 };
                    writer.WriteBlockArray(version);

                    ReportProgress(95, "Writing textures");

                    writer.Write(_numRoomTextureTiles);
                    writer.Write(_numObjectTextureTiles);
                    writer.Write(NumBumpTextureTiles);

                    writer.Write(_texture32UncompressedSize);
                    writer.Write(_texture32CompressedSize);
                    writer.WriteBlockArray(_texture32);

                    writer.Write(_texture16UncompressedSize);
                    writer.Write(_texture16CompressedSize);
                    writer.WriteBlockArray(_texture16);

                    writer.Write(_miscTextureUncompressedSize);
                    writer.Write(_miscTextureCompressedSize);
                    writer.WriteBlockArray(_miscTexture);

                    ReportProgress(95, "Compressing geometry data");

                    var geometrySize = (int)reader.BaseStream.Length;
                    var levelData = reader.ReadBytes(geometrySize);
                    var buffer = Utils.CompressDataZLIB(levelData);
                    _levelUncompressedSize = (uint)geometrySize;
                    _levelCompressedSize = (uint)buffer.Length;

                    ReportProgress(80, "Writing goemetry data");

                    writer.Write(_levelUncompressedSize);
                    writer.Write(_levelCompressedSize);
                    writer.WriteBlockArray(buffer);

                    // ReSharper disable once SuggestVarOrType_BuiltInTypes
                    int numSamples = _editor.Level.Wad.OriginalWad.Sounds.Count;
                    writer.WriteBlock(numSamples);

                    ReportProgress(80, "Writing WAVE sounds");

                    writer.Write(_bufferSamples);

                    ReportProgress(99, "Done");

                    writer.Flush();
                }
            }
        }

        private bool WriteLevelTr3()
        {
            var wad = _editor.Level.Wad.OriginalWad;

            // Now begin to compile the geometry block in a MemoryStream
            using (var writer = new BinaryWriterEx(File.OpenWrite(_dest)))
            {

                ReportProgress(85, "Writing geometry data to memory buffer");

                // Write version
                var version = new byte[] { 0x38, 0x00, 0x18, 0xFF };
                writer.WriteBlockArray(version);

                using (var readerPalette = new BinaryReader(File.OpenRead("Editor\\palette.bin")))
                {
                    var palette = readerPalette.ReadBytes(1792);
                    // Write palette
                    writer.Write(palette);
                }

                // Write textures
                // ReSharper disable once SuggestVarOrType_BuiltInTypes
                int numTextureTiles = _numRoomTextureTiles + _numObjectTextureTiles + 1;
                writer.Write(numTextureTiles);

                // Fake 8 bit textures
                var fakeTextures = new byte[256 * 256 * numTextureTiles];
                writer.Write(fakeTextures);

                // 16 bit textures
                writer.Write(_textures16);

                using (var readerRaw = new BinaryReader(File.OpenRead("sprites3.raw")))
                {
                    var raw = readerRaw.ReadBytes(131072);
                    writer.Write(raw);
                }

                const int filler = 0;
                writer.Write(filler);

                _numRooms = (ushort)_rooms.Length;
                writer.Write(_numRooms);

                long offset;
                long offset2;
                for (var i = 0; i < _numRooms; i++)
                {
                    writer.WriteBlock(_rooms[i].Info);

                    offset = writer.BaseStream.Position;

                    const int numdw = 0;
                    writer.Write(numdw);

                    var tmp = (ushort)_rooms[i].Vertices.Length;
                    writer.Write(tmp);
                    writer.WriteBlockArray(_rooms[i].Vertices);

                    tmp = (ushort)_rooms[i].Rectangles.Length;
                    writer.Write(tmp);
                    if (tmp != 0)
                    {
                        for (var k = 0; k < _rooms[i].Rectangles.Length; k++)
                        {
                            writer.Write(_rooms[i].Rectangles[k].Vertices[0]);
                            writer.Write(_rooms[i].Rectangles[k].Vertices[1]);
                            writer.Write(_rooms[i].Rectangles[k].Vertices[2]);
                            writer.Write(_rooms[i].Rectangles[k].Vertices[3]);
                            writer.Write(_rooms[i].Rectangles[k].Texture);
                        }
                    }

                    tmp = (ushort)_rooms[i].Triangles.Length;
                    writer.Write(tmp);
                    if (tmp != 0)
                    {
                        for (var k = 0; k < _rooms[i].Triangles.Length; k++)
                        {
                            writer.Write(_rooms[i].Triangles[k].Vertices[0]);
                            writer.Write(_rooms[i].Triangles[k].Vertices[1]);
                            writer.Write(_rooms[i].Triangles[k].Vertices[2]);
                            writer.Write(_rooms[i].Triangles[k].Texture);
                        }
                    }

                    // For sprites, not used
                    tmp = 0;
                    writer.Write(tmp);

                    // Now save current offset and calculate the size of the geometry
                    offset2 = writer.BaseStream.Position;
                    // ReSharper disable once SuggestVarOrType_BuiltInTypes
                    ushort roomGeometrySize = (ushort)((offset2 - offset - 4) / 2);

                    // Save the size of the geometry
                    writer.BaseStream.Seek(offset, SeekOrigin.Begin);
                    writer.Write(roomGeometrySize);
                    writer.BaseStream.Seek(offset2, SeekOrigin.Begin);

                    // Write portals
                    tmp = (ushort)_rooms[i].Portals.Length;
                    writer.WriteBlock(tmp);
                    if (tmp != 0)
                        writer.WriteBlockArray(_rooms[i].Portals);

                    // Write sectors
                    writer.Write(_rooms[i].NumZSectors);
                    writer.Write(_rooms[i].NumXSectors);
                    writer.WriteBlockArray(_rooms[i].Sectors);

                    // Write room color
                    writer.Write(_rooms[i].AmbientIntensity1);
                    writer.Write(_rooms[i].AmbientIntensity2);

                    // Write lights
                    tmp = (ushort)_rooms[i].Lights.Length;
                    writer.WriteBlock(tmp);

                    for (var j = 0; j < tmp; j++)
                    {
                        var light = _rooms[i].Lights[j];
                        writer.Write(light.X);
                        writer.Write(light.Y);
                        writer.Write(light.Z);

                        int intensity = light.Intensity;
                        // ReSharper disable once SuggestVarOrType_BuiltInTypes
                        int falloff = (int)light.Out;

                        writer.Write(intensity);
                        writer.Write(falloff);
                    }

                    // Write static meshes
                    tmp = (ushort)_rooms[i].StaticMeshes.Length;
                    writer.WriteBlock(tmp);
                    if (tmp != 0)
                        writer.WriteBlockArray(_rooms[i].StaticMeshes);

                    // Write final data
                    writer.Write(_rooms[i].AlternateRoom);
                    writer.Write(_rooms[i].Flags);
                    writer.Write(_rooms[i].WaterScheme);
                    writer.Write(_rooms[i].ReverbInfo);
                    writer.Write(_rooms[i].AlternateGroup);
                }

                // Write floordata
                _numFloorData = (uint)_floorData.Length;
                writer.Write(_numFloorData);
                writer.WriteBlockArray(_floorData);

                // Write meshes
                offset = writer.BaseStream.Position;

                _numMeshData = 0;
                writer.Write(_numMeshData);
                var totalMeshSize = 0;

                for (var i = 0; i < _meshes.Length; i++)
                {
                    var meshOffset1 = writer.BaseStream.Position;

                    writer.WriteBlock(_meshes[i].Centre);
                    writer.Write(_meshes[i].Radius);

                    writer.Write(_meshes[i].NumVertices);
                    writer.WriteBlockArray(_meshes[i].Vertices);

                    writer.Write(_meshes[i].NumNormals);
                    if (_meshes[i].NumNormals > 0)
                    {
                        writer.WriteBlockArray(_meshes[i].Normals);
                    }
                    else
                    {
                        writer.WriteBlockArray(_meshes[i].Lights);
                    }

                    writer.Write(_meshes[i].NumTexturedRectangles);
                    for (var k = 0; k < _meshes[i].NumTexturedRectangles; k++)
                    {
                        writer.Write(_meshes[i].TexturedRectangles[k].Vertices[0]);
                        writer.Write(_meshes[i].TexturedRectangles[k].Vertices[1]);
                        writer.Write(_meshes[i].TexturedRectangles[k].Vertices[2]);
                        writer.Write(_meshes[i].TexturedRectangles[k].Vertices[3]);
                        writer.Write(_meshes[i].TexturedRectangles[k].Texture);

                    }
                    // writer.WriteBlockArray(Meshes[i].TexturedRectangles);

                    writer.Write(_meshes[i].NumTexturedTriangles);
                    for (var k = 0; k < _meshes[i].NumTexturedTriangles; k++)
                    {
                        writer.Write(_meshes[i].TexturedTriangles[k].Vertices[0]);
                        writer.Write(_meshes[i].TexturedTriangles[k].Vertices[1]);
                        writer.Write(_meshes[i].TexturedTriangles[k].Vertices[2]);
                        writer.Write(_meshes[i].TexturedTriangles[k].Texture);

                    }

                    //  writer.WriteBlockArray(Meshes[i].TexturedTriangles);

                    writer.Write(_meshes[i].NumColoredRectangles);
                    //writer.WriteBlockArray(Meshes[i].ColoredRectangles);

                    writer.Write(_meshes[i].NumColoredTriangles);
                    //writer.WriteBlockArray(Meshes[i].ColoredTriangles);

                    var meshOffset2 = writer.BaseStream.Position;
                    var meshSize = meshOffset2 - meshOffset1;
                    if (meshSize % 4 != 0)
                    {
                        const ushort tempFiller = 0;
                        writer.Write(tempFiller);
                        meshSize += 2;
                    }

                    for (var n = 0; n < _numMeshPointers; n++)
                    {
                        if (wad.HelperPointers[n] == i)
                        {
                            _meshPointers[n] = (uint)totalMeshSize;
                        }
                    }

                    totalMeshSize += (int)meshSize;

                    //if (i < NumMeshes - 1) MeshPointers[i + 1] = MeshPointers[i] + (uint)meshSize;
                }

                offset2 = writer.BaseStream.Position;
                // ReSharper disable once SuggestVarOrType_BuiltInTypes
                uint meshDataSize = (uint)((offset2 - offset - 4) / 2);

                // Save the size of the meshes
                writer.BaseStream.Seek(offset, SeekOrigin.Begin);
                writer.Write(meshDataSize);
                writer.BaseStream.Seek(offset2, SeekOrigin.Begin);

                // Write mesh pointers
                writer.Write(_numMeshPointers);
                writer.WriteBlockArray(_meshPointers);

                // Write animations' data
                writer.Write(_numAnimations);
                foreach (var anim in _animations)
                {
                    writer.Write(anim.FrameOffset);
                    writer.Write(anim.FrameRate);
                    writer.Write(anim.FrameSize);
                    writer.Write(anim.StateID);
                    writer.Write(anim.Speed);
                    writer.Write(anim.Accel);
                    writer.Write(anim.FrameStart);
                    writer.Write(anim.FrameEnd);
                    writer.Write(anim.NextAnimation);
                    writer.Write(anim.NextFrame);
                    writer.Write(anim.NumStateChanges);
                    writer.Write(anim.StateChangeOffset);
                    writer.Write(anim.NumAnimCommands);
                    writer.Write(anim.AnimCommand);
                }

                writer.Write(_numStateChanges);
                writer.WriteBlockArray(_stateChanges);

                writer.Write(_numAnimDispatches);
                writer.WriteBlockArray(_animDispatches);

                writer.Write(_numAnimCommands);
                writer.WriteBlockArray(_animCommands);

                writer.Write(_numMeshTrees);
                writer.WriteBlockArray(_meshTrees);

                writer.Write(_numFrames);
                writer.WriteBlockArray(_frames);

                writer.Write(_numMoveables);
                writer.WriteBlockArray(_moveables);

                writer.Write(_numStaticMeshes);
                writer.WriteBlockArray(_staticMeshes);

                // SPR block
                using (var readerSprites = new BinaryReader(File.OpenRead("sprites3.bin")))
                {
                    var bufferSprites = readerSprites.ReadBytes((int)readerSprites.BaseStream.Length);
                    writer.Write(bufferSprites);
                }

                /*writer.Write(NumSpriteTextures);
                writer.WriteBlockArray(SpriteTextures);

                writer.Write(NumSpriteSequences);
                writer.WriteBlockArray(SpriteSequences);
                */
                // Write camera, flyby and sound sources
                writer.Write(_numCameras);
                writer.WriteBlockArray(_cameras);

                writer.Write(_numSoundSources);
                writer.WriteBlockArray(_soundSources);

                // Write pathfinding data
                writer.Write(_numBoxes);
                writer.WriteBlockArray(_boxes);

                writer.Write(_numOverlaps);
                writer.WriteBlockArray(_overlaps);

                for (var i = 0; i < _numBoxes; i++)
                    writer.Write(_zones[i].GroundZone1_Normal);
                for (var i = 0; i < _numBoxes; i++)
                    writer.Write(_zones[i].GroundZone2_Normal);
                for (var i = 0; i < _numBoxes; i++)
                    writer.Write(_zones[i].GroundZone3_Normal);
                for (var i = 0; i < _numBoxes; i++)
                    writer.Write(_zones[i].GroundZone4_Normal);
                for (var i = 0; i < _numBoxes; i++)
                    writer.Write(_zones[i].FlyZone_Normal);
                for (var i = 0; i < _numBoxes; i++)
                    writer.Write(_zones[i].GroundZone1_Alternate);
                for (var i = 0; i < _numBoxes; i++)
                    writer.Write(_zones[i].GroundZone2_Alternate);
                for (var i = 0; i < _numBoxes; i++)
                    writer.Write(_zones[i].GroundZone3_Alternate);
                for (var i = 0; i < _numBoxes; i++)
                    writer.Write(_zones[i].GroundZone4_Alternate);
                for (var i = 0; i < _numBoxes; i++)
                    writer.Write(_zones[i].FlyZone_Alternate);

                //   writer.WriteBlockArray(Zones);

                // Write animated textures
                writer.Write(_numAnimatedTextures);

                // ReSharper disable once SuggestVarOrType_BuiltInTypes
                short numSets = (short)_animatedTextures.Length;
                writer.Write(numSets);

                for (var i = 0; i < _animatedTextures.Length; i++)
                {
                    writer.Write(_animatedTextures[i].NumTextures);

                    // ReSharper disable once SuggestVarOrType_BuiltInTypes
                    foreach (short texture in _animatedTextures[i].Textures)
                    {
                        writer.Write(texture);
                    }
                }

                // Write object textures
                _objectTextures = _tempObjectTextures.ToArray();
                _numObjectTextures = (uint)_objectTextures.Length;

                writer.Write(_numObjectTextures);
                for (var j = 0; j < _numObjectTextures; j++)
                {
                    writer.Write(_objectTextures[j].Attributes);
                    writer.Write(_objectTextures[j].Tile);
                    writer.WriteBlockArray(_objectTextures[j].Vertices);
                }

                // Write items and AI objects
                writer.Write(_numItems);
                writer.WriteBlockArray(_items);

                var lightmap = new byte[8192];
                writer.Write(lightmap);

                const short numDemo = 0;
                writer.Write(numDemo);
                writer.Write(numDemo);

                // Write sound data
                byte[] sfxBuffer;
                using (var readerSounds =
                    new BinaryReaderEx(
                        File.OpenRead(@"Graphics\Wads\" + _editor.Level.Wad.OriginalWad.BaseName + ".sfx")))
                {
                    sfxBuffer = readerSounds.ReadBytes((int)readerSounds.BaseStream.Length);
                    readerSounds.BaseStream.Seek(0, SeekOrigin.Begin);
                    readerSounds.ReadBytes(370 * 2);
                    _numSoundDetails = (uint)readerSounds.ReadInt16();
                }

                writer.WriteBlockArray(sfxBuffer);

                writer.Flush();
            }

            return true;
        }
    }
}
