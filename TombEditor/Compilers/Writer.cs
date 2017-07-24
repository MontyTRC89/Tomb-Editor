using System;
using System.IO;
using TombLib.IO;
using TombLib.Wad;

namespace TombEditor.Compilers
{
    public partial class LevelCompilerTR4
    {
        private void WriteLevelTR4()
        {
            var wad = _editor.Level.Wad.OriginalWad;

            // Now begin to compile the geometry block in a MemoryStream
            using (var writer = new BinaryWriterEx(File.OpenWrite("temp.bin")))
            {

                ReportProgress(85, "Writing geometry data to memory buffer");

                const int filler = 0;
                writer.Write(filler);

                NumRooms = (ushort)Rooms.Length;
                writer.Write(NumRooms);

                long offset;
                long offset2;
                for (var i = 0; i < NumRooms; i++)
                {
                    writer.WriteBlock(Rooms[i].Info);

                    offset = writer.BaseStream.Position;

                    const int numdw = 0;
                    writer.Write(numdw);

                    var tmp = (ushort)Rooms[i].Vertices.Length;
                    writer.Write(tmp);
                    writer.WriteBlockArray(Rooms[i].Vertices);

                    tmp = (ushort)Rooms[i].Rectangles.Length;
                    writer.Write(tmp);
                    if (tmp != 0)
                    {
                        for (var k = 0; k < Rooms[i].Rectangles.Length; k++)
                        {
                            writer.Write(Rooms[i].Rectangles[k].Vertices[0]);
                            writer.Write(Rooms[i].Rectangles[k].Vertices[1]);
                            writer.Write(Rooms[i].Rectangles[k].Vertices[2]);
                            writer.Write(Rooms[i].Rectangles[k].Vertices[3]);
                            writer.Write(Rooms[i].Rectangles[k].Texture);
                        }
                    }

                    tmp = (ushort)Rooms[i].Triangles.Length;
                    writer.Write(tmp);
                    if (tmp != 0)
                    {
                        for (var k = 0; k < Rooms[i].Triangles.Length; k++)
                        {
                            writer.Write(Rooms[i].Triangles[k].Vertices[0]);
                            writer.Write(Rooms[i].Triangles[k].Vertices[1]);
                            writer.Write(Rooms[i].Triangles[k].Vertices[2]);
                            writer.Write(Rooms[i].Triangles[k].Texture);
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
                    tmp = (ushort)Rooms[i].Portals.Length;
                    writer.WriteBlock(tmp);
                    if (tmp != 0)
                        writer.WriteBlockArray(Rooms[i].Portals);

                    // Write sectors
                    writer.Write(Rooms[i].NumZSectors);
                    writer.Write(Rooms[i].NumXSectors);
                    writer.WriteBlockArray(Rooms[i].Sectors);

                    // Write room color
                    writer.Write(Rooms[i].AmbientIntensity1);
                    writer.Write(Rooms[i].AmbientIntensity2);

                    // Write lights
                    tmp = (ushort)Rooms[i].Lights.Length;
                    writer.WriteBlock(tmp);
                    if (tmp != 0)
                        writer.WriteBlockArray(Rooms[i].Lights);

                    // Write static meshes
                    tmp = (ushort)Rooms[i].StaticMeshes.Length;
                    writer.WriteBlock(tmp);
                    if (tmp != 0)
                        writer.WriteBlockArray(Rooms[i].StaticMeshes);

                    // Write final data
                    writer.Write(Rooms[i].AlternateRoom);
                    writer.Write(Rooms[i].Flags);
                    writer.Write(Rooms[i].WaterScheme);
                    writer.Write(Rooms[i].ReverbInfo);
                    writer.Write(Rooms[i].AlternateGroup);
                }

                // Write floordata
                NumFloorData = (uint)FloorData.Length;
                writer.Write(NumFloorData);
                writer.WriteBlockArray(FloorData);

                // Write meshes
                offset = writer.BaseStream.Position;

                NumMeshData = 0;
                writer.Write(NumMeshData);
                var totalMeshSize = 0;

                for (var i = 0; i < Meshes.Length; i++)
                {
                    long meshOffset1 = writer.BaseStream.Position;

                    writer.WriteBlock(Meshes[i].Centre);
                    writer.Write(Meshes[i].Radius);

                    writer.Write(Meshes[i].NumVertices);
                    writer.WriteBlockArray(Meshes[i].Vertices);

                    writer.Write(Meshes[i].NumNormals);
                    if (Meshes[i].NumNormals > 0)
                    {
                        writer.WriteBlockArray(Meshes[i].Normals);
                    }
                    else
                    {
                        writer.WriteBlockArray(Meshes[i].Lights);
                    }

                    writer.Write(Meshes[i].NumTexturedRectangles);
                    writer.WriteBlockArray(Meshes[i].TexturedRectangles);

                    writer.Write(Meshes[i].NumTexturedTriangles);
                    writer.WriteBlockArray(Meshes[i].TexturedTriangles);

                    var meshOffset2 = writer.BaseStream.Position;
                    var meshSize = (meshOffset2 - meshOffset1);
                    if (meshSize % 4 != 0)
                    {
                        const ushort tempFiller = 0;
                        writer.Write(tempFiller);
                        meshSize += 2;
                    }

                    for (var n = 0; n < NumMeshPointers; n++)
                    {
                        if (wad.HelperPointers[n] == i)
                        {
                            MeshPointers[n] = (uint)totalMeshSize;
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
                writer.Write(NumMeshPointers);
                writer.WriteBlockArray(MeshPointers);

                // Write animations' data
                writer.Write(NumAnimations);
                writer.WriteBlockArray(Animations);

                writer.Write(NumStateChanges);
                writer.WriteBlockArray(StateChanges);

                writer.Write(NumAnimDispatches);
                writer.WriteBlockArray(AnimDispatches);

                writer.Write(NumAnimCommands);
                writer.WriteBlockArray(AnimCommands);

                writer.Write(NumMeshTrees);
                writer.WriteBlockArray(MeshTrees);

                writer.Write(NumFrames);
                writer.WriteBlockArray(Frames);

                writer.Write(NumMoveables);
                writer.WriteBlockArray(Moveables);

                writer.Write(NumStaticMeshes);
                writer.WriteBlockArray(StaticMeshes);

                // SPR block
                SPR = new byte[] { 0x53, 0x50, 0x52 };
                writer.WriteBlockArray(SPR);

                writer.Write(NumSpriteTextures);
                writer.WriteBlockArray(SpriteTextures);

                writer.Write(NumSpriteSequences);
                writer.WriteBlockArray(SpriteSequences);

                // Write camera, flyby and sound sources
                writer.Write(NumCameras);
                writer.WriteBlockArray(Cameras);

                writer.Write(NumFlyByCameras);
                writer.WriteBlockArray(FlyByCameras);

                writer.Write(NumSoundSources);
                writer.WriteBlockArray(SoundSources);

                // Write pathfinding data
                writer.Write(NumBoxes);
                writer.WriteBlockArray(Boxes);

                writer.Write(NumOverlaps);
                writer.WriteBlockArray(Overlaps);

                for (var i = 0; i < NumBoxes; i++)
                    writer.Write(Zones[i].GroundZone1_Normal);
                for (var i = 0; i < NumBoxes; i++)
                    writer.Write(Zones[i].GroundZone2_Normal);
                for (var i = 0; i < NumBoxes; i++)
                    writer.Write(Zones[i].GroundZone3_Normal);
                for (var i = 0; i < NumBoxes; i++)
                    writer.Write(Zones[i].GroundZone4_Normal);
                for (var i = 0; i < NumBoxes; i++)
                    writer.Write(Zones[i].FlyZone_Normal);
                for (var i = 0; i < NumBoxes; i++)
                    writer.Write(Zones[i].GroundZone1_Alternate);
                for (var i = 0; i < NumBoxes; i++)
                    writer.Write(Zones[i].GroundZone2_Alternate);
                for (var i = 0; i < NumBoxes; i++)
                    writer.Write(Zones[i].GroundZone3_Alternate);
                for (var i = 0; i < NumBoxes; i++)
                    writer.Write(Zones[i].GroundZone4_Alternate);
                for (var i = 0; i < NumBoxes; i++)
                    writer.Write(Zones[i].FlyZone_Alternate);

                //   writer.WriteBlockArray(Zones);

                // Write animated textures
                writer.Write(NumAnimatedTextures);

                // ReSharper disable once SuggestVarOrType_BuiltInTypes
                short numSets = (short)AnimatedTextures.Length;
                writer.Write(numSets);

                for (var i = 0; i < AnimatedTextures.Length; i++)
                {
                    writer.Write(AnimatedTextures[i].NumTextures);

                    foreach (var texture in AnimatedTextures[i].Textures)
                    {
                        writer.Write(texture);
                    }
                }

                // Write object textures
                var tex = new byte[] { 0x00, 0x54, 0x45, 0x58 };
                writer.WriteBlockArray(tex);

                ObjectTextures = _tempObjectTextures.ToArray();
                NumObjectTextures = (uint)ObjectTextures.Length;

                writer.Write(NumObjectTextures);
                writer.WriteBlockArray(ObjectTextures);

                // Write items and AI objects
                writer.Write(NumItems);
                writer.WriteBlockArray(Items);

                writer.Write(NumAiItems);
                writer.WriteBlockArray(AiItems);

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
                    NumSoundDetails = (uint)readerSounds.ReadInt32();
                    soundDetails = readerSounds.ReadBytes((int)NumSoundDetails * 8);
                    // ReSharper disable once SuggestVarOrType_BuiltInTypes
                    numSampleIndices = (uint)readerSounds.ReadInt32();
                    sampleIndices = readerSounds.ReadBytes((int)numSampleIndices * 4);
                }

                writer.Write(soundMap);
                writer.Write(NumSoundDetails);
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

                    writer.Write(NumRoomTextureTiles);
                    writer.Write(NumObjectTextureTiles);
                    writer.Write(NumBumpTextureTiles);

                    writer.Write(Texture32UncompressedSize);
                    writer.Write(Texture32CompressedSize);
                    writer.WriteBlockArray(Texture32);

                    writer.Write(Texture16UncompressedSize);
                    writer.Write(Texture16CompressedSize);
                    writer.WriteBlockArray(Texture16);

                    writer.Write(MiscTextureUncompressedSize);
                    writer.Write(MiscTextureCompressedSize);
                    writer.WriteBlockArray(MiscTexture);

                    ReportProgress(95, "Compressing geometry data");

                    var geometrySize = (int)reader.BaseStream.Length;
                    var levelData = reader.ReadBytes(geometrySize);
                    var buffer = Utils.CompressDataZLIB(levelData);
                    LevelUncompressedSize = (uint)geometrySize;
                    LevelCompressedSize = (uint)buffer.Length;

                    ReportProgress(80, "Writing goemetry data");

                    writer.Write(LevelUncompressedSize);
                    writer.Write(LevelCompressedSize);
                    writer.WriteBlockArray(buffer);

                    // ReSharper disable once SuggestVarOrType_BuiltInTypes
                    int numSamples = _editor.Level.Wad.OriginalWad.Sounds.Count;
                    writer.WriteBlock(numSamples);

                    ReportProgress(80, "Writing WAVE sounds");

                    writer.Write(bufferSamples);

                    ReportProgress(99, "Done");

                    writer.Flush();
                }
            }
        }

        private bool WriteLevelTR3()
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
                int numTextureTiles = NumRoomTextureTiles + NumObjectTextureTiles + 1;
                writer.Write(numTextureTiles);

                // Fake 8 bit textures
                var fakeTextures = new byte[256 * 256 * numTextureTiles];
                writer.Write(fakeTextures);

                // 16 bit textures
                writer.Write(Textures16);

                using (var readerRaw = new BinaryReader(File.OpenRead("sprites3.raw")))
                {
                    var raw = readerRaw.ReadBytes(131072);
                    writer.Write(raw);
                }

                const int filler = 0;
                writer.Write(filler);

                NumRooms = (ushort)Rooms.Length;
                writer.Write(NumRooms);

                long offset;
                long offset2;
                for (var i = 0; i < NumRooms; i++)
                {
                    writer.WriteBlock(Rooms[i].Info);

                    offset = writer.BaseStream.Position;

                    const int numdw = 0;
                    writer.Write(numdw);

                    ushort tmp = 0;
                    tmp = (ushort)Rooms[i].Vertices.Length;
                    writer.Write(tmp);
                    writer.WriteBlockArray(Rooms[i].Vertices);

                    tmp = (ushort)Rooms[i].Rectangles.Length;
                    writer.Write(tmp);
                    if (tmp != 0)
                    {
                        for (var k = 0; k < Rooms[i].Rectangles.Length; k++)
                        {
                            writer.Write(Rooms[i].Rectangles[k].Vertices[0]);
                            writer.Write(Rooms[i].Rectangles[k].Vertices[1]);
                            writer.Write(Rooms[i].Rectangles[k].Vertices[2]);
                            writer.Write(Rooms[i].Rectangles[k].Vertices[3]);
                            writer.Write(Rooms[i].Rectangles[k].Texture);
                        }
                    }

                    tmp = (ushort)Rooms[i].Triangles.Length;
                    writer.Write(tmp);
                    if (tmp != 0)
                    {
                        for (var k = 0; k < Rooms[i].Triangles.Length; k++)
                        {
                            writer.Write(Rooms[i].Triangles[k].Vertices[0]);
                            writer.Write(Rooms[i].Triangles[k].Vertices[1]);
                            writer.Write(Rooms[i].Triangles[k].Vertices[2]);
                            writer.Write(Rooms[i].Triangles[k].Texture);
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
                    tmp = (ushort)Rooms[i].Portals.Length;
                    writer.WriteBlock(tmp);
                    if (tmp != 0)
                        writer.WriteBlockArray(Rooms[i].Portals);

                    // Write sectors
                    writer.Write(Rooms[i].NumZSectors);
                    writer.Write(Rooms[i].NumXSectors);
                    writer.WriteBlockArray(Rooms[i].Sectors);

                    // Write room color
                    writer.Write(Rooms[i].AmbientIntensity1);
                    writer.Write(Rooms[i].AmbientIntensity2);

                    // Write lights
                    tmp = (ushort)Rooms[i].Lights.Length;
                    writer.WriteBlock(tmp);

                    for (var j = 0; j < tmp; j++)
                    {
                        var light = Rooms[i].Lights[j];
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
                    tmp = (ushort)Rooms[i].StaticMeshes.Length;
                    writer.WriteBlock(tmp);
                    if (tmp != 0)
                        writer.WriteBlockArray(Rooms[i].StaticMeshes);

                    // Write final data
                    writer.Write(Rooms[i].AlternateRoom);
                    writer.Write(Rooms[i].Flags);
                    writer.Write(Rooms[i].WaterScheme);
                    writer.Write(Rooms[i].ReverbInfo);
                    writer.Write(Rooms[i].AlternateGroup);
                }

                // Write floordata
                NumFloorData = (uint)FloorData.Length;
                writer.Write(NumFloorData);
                writer.WriteBlockArray(FloorData);

                // Write meshes
                offset = writer.BaseStream.Position;

                NumMeshData = 0;
                writer.Write(NumMeshData);
                var totalMeshSize = 0;

                for (var i = 0; i < Meshes.Length; i++)
                {
                    var meshOffset1 = writer.BaseStream.Position;

                    writer.WriteBlock(Meshes[i].Centre);
                    writer.Write(Meshes[i].Radius);

                    writer.Write(Meshes[i].NumVertices);
                    writer.WriteBlockArray(Meshes[i].Vertices);

                    writer.Write(Meshes[i].NumNormals);
                    if (Meshes[i].NumNormals > 0)
                    {
                        writer.WriteBlockArray(Meshes[i].Normals);
                    }
                    else
                    {
                        writer.WriteBlockArray(Meshes[i].Lights);
                    }

                    writer.Write(Meshes[i].NumTexturedRectangles);
                    for (var k = 0; k < Meshes[i].NumTexturedRectangles; k++)
                    {
                        writer.Write(Meshes[i].TexturedRectangles[k].Vertices[0]);
                        writer.Write(Meshes[i].TexturedRectangles[k].Vertices[1]);
                        writer.Write(Meshes[i].TexturedRectangles[k].Vertices[2]);
                        writer.Write(Meshes[i].TexturedRectangles[k].Vertices[3]);
                        writer.Write(Meshes[i].TexturedRectangles[k].Texture);

                    }
                    // writer.WriteBlockArray(Meshes[i].TexturedRectangles);

                    writer.Write(Meshes[i].NumTexturedTriangles);
                    for (var k = 0; k < Meshes[i].NumTexturedTriangles; k++)
                    {
                        writer.Write(Meshes[i].TexturedTriangles[k].Vertices[0]);
                        writer.Write(Meshes[i].TexturedTriangles[k].Vertices[1]);
                        writer.Write(Meshes[i].TexturedTriangles[k].Vertices[2]);
                        writer.Write(Meshes[i].TexturedTriangles[k].Texture);

                    }

                    //  writer.WriteBlockArray(Meshes[i].TexturedTriangles);

                    writer.Write(Meshes[i].NumColoredRectangles);
                    //writer.WriteBlockArray(Meshes[i].ColoredRectangles);

                    writer.Write(Meshes[i].NumColoredTriangles);
                    //writer.WriteBlockArray(Meshes[i].ColoredTriangles);

                    var meshOffset2 = writer.BaseStream.Position;
                    var meshSize = meshOffset2 - meshOffset1;
                    if (meshSize % 4 != 0)
                    {
                        const ushort tempFiller = 0;
                        writer.Write(tempFiller);
                        meshSize += 2;
                    }

                    for (var n = 0; n < NumMeshPointers; n++)
                    {
                        if (wad.HelperPointers[n] == i)
                        {
                            MeshPointers[n] = (uint)totalMeshSize;
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
                writer.Write(NumMeshPointers);
                writer.WriteBlockArray(MeshPointers);

                // Write animations' data
                writer.Write(NumAnimations);
                foreach (var anim in Animations)
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

                writer.Write(NumStateChanges);
                writer.WriteBlockArray(StateChanges);

                writer.Write(NumAnimDispatches);
                writer.WriteBlockArray(AnimDispatches);

                writer.Write(NumAnimCommands);
                writer.WriteBlockArray(AnimCommands);

                writer.Write(NumMeshTrees);
                writer.WriteBlockArray(MeshTrees);

                writer.Write(NumFrames);
                writer.WriteBlockArray(Frames);

                writer.Write(NumMoveables);
                writer.WriteBlockArray(Moveables);

                writer.Write(NumStaticMeshes);
                writer.WriteBlockArray(StaticMeshes);

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
                writer.Write(NumCameras);
                writer.WriteBlockArray(Cameras);

                writer.Write(NumSoundSources);
                writer.WriteBlockArray(SoundSources);

                // Write pathfinding data
                writer.Write(NumBoxes);
                writer.WriteBlockArray(Boxes);

                writer.Write(NumOverlaps);
                writer.WriteBlockArray(Overlaps);

                for (var i = 0; i < NumBoxes; i++)
                    writer.Write(Zones[i].GroundZone1_Normal);
                for (var i = 0; i < NumBoxes; i++)
                    writer.Write(Zones[i].GroundZone2_Normal);
                for (var i = 0; i < NumBoxes; i++)
                    writer.Write(Zones[i].GroundZone3_Normal);
                for (var i = 0; i < NumBoxes; i++)
                    writer.Write(Zones[i].GroundZone4_Normal);
                for (var i = 0; i < NumBoxes; i++)
                    writer.Write(Zones[i].FlyZone_Normal);
                for (var i = 0; i < NumBoxes; i++)
                    writer.Write(Zones[i].GroundZone1_Alternate);
                for (var i = 0; i < NumBoxes; i++)
                    writer.Write(Zones[i].GroundZone2_Alternate);
                for (var i = 0; i < NumBoxes; i++)
                    writer.Write(Zones[i].GroundZone3_Alternate);
                for (var i = 0; i < NumBoxes; i++)
                    writer.Write(Zones[i].GroundZone4_Alternate);
                for (var i = 0; i < NumBoxes; i++)
                    writer.Write(Zones[i].FlyZone_Alternate);

                //   writer.WriteBlockArray(Zones);

                // Write animated textures
                writer.Write(NumAnimatedTextures);

                // ReSharper disable once SuggestVarOrType_BuiltInTypes
                short numSets = (short)AnimatedTextures.Length;
                writer.Write(numSets);

                for (var i = 0; i < AnimatedTextures.Length; i++)
                {
                    writer.Write(AnimatedTextures[i].NumTextures);

                    // ReSharper disable once SuggestVarOrType_BuiltInTypes
                    foreach (short texture in AnimatedTextures[i].Textures)
                    {
                        writer.Write(texture);
                    }
                }

                // Write object textures
                ObjectTextures = _tempObjectTextures.ToArray();
                NumObjectTextures = (uint)ObjectTextures.Length;

                writer.Write(NumObjectTextures);
                for (var j = 0; j < NumObjectTextures; j++)
                {
                    writer.Write(ObjectTextures[j].Attributes);
                    writer.Write(ObjectTextures[j].Tile);
                    writer.WriteBlockArray(ObjectTextures[j].Vertices);
                }

                // Write items and AI objects
                writer.Write(NumItems);
                writer.WriteBlockArray(Items);

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
                    NumSoundDetails = (uint)readerSounds.ReadInt16();
                }

                writer.WriteBlockArray(sfxBuffer);

                writer.Flush();
            }

            return true;
        }
    }
}
