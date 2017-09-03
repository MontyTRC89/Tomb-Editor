using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using NLog;
using TombLib.IO;

namespace TombEditor
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct Tr3Face4
    {
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)] public ushort[] Vertices;
        public ushort Texture;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct Tr3Face3
    {
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 3)] public ushort[] Vertices;
        public ushort Texture;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct Tr3Mesh
    {
        public TrVertex Center;
        public int Radius;
        public short NumVertices;
        public TrVertex[] Vertices;
        public short NumNormals;
        public TrVertex[] Normals;
        public short[] Lights;
        public short NumTexturedRectangles;
        public Tr3Face4[] TexturedRectangles;
        public short NumTexturedTriangles;
        public Tr3Face3[] TexturedTriangles;
        public short NumColoredRectangles;
        public Tr3Face4[] ColoredRectangles;
        public short NumColoredTriangles;
        public Tr3Face3[] ColoredTriangles;
        public int MeshSize;
        public int MeshPointer;
    }


    class TombRaider3Level
    {
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();

        public int Version;

        //public ushort NumRoomTextureTiles;
        //public ushort NumObjectTextureTiles;
        //public ushort NumBumpTextureTiles;

        //public uint Texture32UncompressedSize;
        //public uint Texture32CompressedSize;
        //public byte[] Texture32;

        //public uint Texture16UncompressedSize;
        //public uint Texture16CompressedSize;
        //public byte[] Texture16;

        //public uint MiscTextureUncompressedSize;
        //public uint MiscTextureCompressedSize;
        //public byte[] MiscTexture;

        //public uint LevelUncompressedSize;
        //public uint LevelCompressedSize;

        public int Unused;

        public ushort NumRooms;
        public TrRoom[] Rooms;

        public uint NumFloorData;
        public short[] FloorData;

        public uint NumMeshData;
        public uint NumMeshes;
        public Tr3Mesh[] Meshes;

        public uint NumMeshPointers;
        public uint[] MeshPointers;

        public uint NumAnimations;

        //public tr_animation[] Animations;
        public uint NumStateChanges;

        public TrStateChange[] StateChanges;
        public uint NumAnimDispatches;
        public TrAnimDispatch[] AnimDispatches;
        public uint NumAnimCommands;
        public short[] AnimCommands;
        public uint NumMeshTrees;
        public int[] MeshTrees;
        public uint NumFrames;
        public ushort[] Frames;
        public uint NumMoveables;
        public TrMoveable[] Moveables;
        public uint NumStaticMeshes;
        public TrStaticmesh[] StaticMeshes;

        //public byte[] SPR;

        public uint NumSpriteTextures;
        public TrSpriteTexture[] SpriteTextures;
        public uint NumSpriteSequences;
        public TrSpriteSequence[] SpriteSequences;
        public uint NumCameras;

        public TrCamera[] Cameras;

        //public uint NumFlyByCameras;
        //public byte[] FlyByCameras;
        public uint NumSoundSources;

        public TrSoundSource[] SoundSources;
        public uint NumBoxes;
        public TrBox[] Boxes;
        public uint NumOverlaps;
        public ushort[] Overlaps;
        public short[] Zones;

        public uint NumAnimatedTextures;
        //public ushort[] AnimatedTextures;

        //public byte[] TEX;

        public uint NumObjectTextures;

        //public tr_object_texture[] ObjectTextures;
        public uint NumItems;

        public TrItem[] Items;
        public uint NumAiItems;
        public TrAiItem[] AiItems;

        string _fileName;
        byte[,] _texture16;

        public TombRaider3Level(string fileName)
        {
            _fileName = fileName;
        }

        public void Load(string ind)
        {
            FileStream fileStream = new FileStream(_fileName, FileMode.Open, FileAccess.Read, FileShare.Read);
            BinaryReaderEx reader = new BinaryReaderEx(fileStream);

            reader.ReadBlock(out Version);
            byte[] palette = reader.ReadBytes(768);
            BinaryWriterEx wrPalette =
                new BinaryWriterEx(new FileStream("Palette.bin", FileMode.Create, FileAccess.Write, FileShare.None));
            for (int jj = 0; jj < palette.Length; jj++)
            {
                byte col = (byte)(palette[jj] * 4);
                wrPalette.Write(col);
            }

            wrPalette.Flush();
            wrPalette.Close();


            reader.ReadBytes(1024);

            int numTextureTiles;
            reader.ReadBlock(out numTextureTiles);

            reader.ReadBytes(256 * 256 * numTextureTiles);

            int height = numTextureTiles * 256;

            _texture16 = new byte[512, height];

            for (int z = 0; z < height; z++)
            {
                for (int x = 0; x < 512; x++)
                {
                    _texture16[x, z] = reader.ReadByte();
                }
            }

            //byte[] texture16 = reader.ReadBytes(256 * 256 * 2 * numTextureTiles);


            reader.ReadBlock(out Unused);
            reader.ReadBlock(out NumRooms);

            int max = 0;

            StreamWriter wp = new StreamWriter(new FileStream("portals" + ind + ".txt", FileMode.Create,
                FileAccess.Write, FileShare.None));

            Rooms = new TrRoom[NumRooms];
            for (int i = 0; i < NumRooms; i++)
            {
                wp.WriteLine("=====================================================================");
                wp.WriteLine("ROOM #" + i);
                wp.WriteLine("=====================================================================");

                reader.ReadBlock(out Rooms[i].Info);
                reader.ReadBlock(out Rooms[i].NumDataWords);
                reader.ReadBlock(out Rooms[i].NumVertices);
                //  Rooms[i].Vertices = new tr_room_vertex[Rooms[i].NumVertices];
                reader.ReadBlockArray(out Rooms[i].Vertices, Rooms[i].NumVertices);
                if (Rooms[i].NumVertices > max)
                    max = Rooms[i].NumVertices;
                reader.ReadBlock(out Rooms[i].NumRectangles);
                Rooms[i].Rectangles = new TrFace4[Rooms[i].NumRectangles];
                for (int j = 0; j < Rooms[i].NumRectangles; j++)
                {
                    // Rooms[i].Rectangles[j].Vertices = new ushort[4];
                    reader.ReadBlockArray(out Rooms[i].Rectangles[j].Vertices, 4);
                    reader.ReadBlock(out Rooms[i].Rectangles[j].Texture);
                }

                reader.ReadBlock(out Rooms[i].NumTriangles);
                Rooms[i].Triangles = new TrFace3[Rooms[i].NumTriangles];
                for (int j = 0; j < Rooms[i].NumTriangles; j++)
                {
                    // Rooms[i].Triangles[j].Vertices = new ushort[3];
                    reader.ReadBlockArray(out Rooms[i].Triangles[j].Vertices, 3);
                    reader.ReadBlock(out Rooms[i].Triangles[j].Texture);
                }

                reader.ReadBlock(out Rooms[i].NumSprites);

                reader.ReadBlock(out Rooms[i].NumPortals);
                //Rooms[i].Portals = new tr_room_portal[Rooms[i].NumPortals];
                reader.ReadBlockArray(out Rooms[i].Portals, Rooms[i].NumPortals);

                for (int nn = 0; nn < Rooms[i].Portals.Length; nn++)
                {
                    TrRoomPortal pt = Rooms[i].Portals[nn];
                    wp.WriteLine(nn + ": ");
                    wp.WriteLine("Room: " + pt.AdjoiningRoom);
                    for (int vv = 0; vv < 4; vv++)
                    {
                        wp.Write("V" + vv + " = " + pt.Vertices[vv].X + ", " + pt.Vertices[vv].Y + ", " +
                                 pt.Vertices[vv].Z);
                        wp.WriteLine("");
                    }
                    wp.WriteLine("");
                }

                reader.ReadBlock(out Rooms[i].NumZSectors);
                reader.ReadBlock(out Rooms[i].NumXSectors);
                //Rooms[i].Sectors = new tr_room_sector[Rooms[i].NumZSectors * Rooms[i].NumXSectors];
                reader.ReadBlockArray(out Rooms[i].Sectors, (uint)Rooms[i].NumZSectors * Rooms[i].NumXSectors);

                reader.ReadBlock(out Rooms[i].AmbientIntensity1);
                reader.ReadBlock(out Rooms[i].AmbientIntensity2);

                reader.ReadBlock(out Rooms[i].NumLights);
                //reader.ReadBlockArray(out Rooms[i].Lights, Rooms[i].NumLights);
                reader.ReadBytes(24 * Rooms[i].NumLights);

                reader.ReadBlock(out Rooms[i].NumStaticMeshes);
                reader.ReadBlockArray(out Rooms[i].StaticMeshes, Rooms[i].NumStaticMeshes);

                reader.ReadBlock(out Rooms[i].AlternateRoom);
                reader.ReadBlock(out Rooms[i].Flags);
                reader.ReadBlock(out Rooms[i].Param1);
                reader.ReadBlock(out Rooms[i].Unknown1);
                reader.ReadBlock(out Rooms[i].Unknown2);
            }

            wp.Flush();
            wp.Close();
            //return;

            reader.ReadBlock(out NumFloorData);
            reader.ReadBlockArray(out FloorData, NumFloorData);

            reader.ReadBlock(out NumMeshData);

            int numBytes = 0;
            int totalBytes = 0;
            int l = 0;

            Meshes = new Tr3Mesh[2048];
            while (totalBytes < (NumMeshData * 2))
            {
                long offset1 = reader.BaseStream.Position;

                reader.ReadBlock(out Meshes[l].Center);
                reader.ReadBlock(out Meshes[l].Radius);
                numBytes += 10;

                reader.ReadBlock(out Meshes[l].NumVertices);
                reader.ReadBlockArray(out Meshes[l].Vertices, Meshes[l].NumVertices);
                numBytes += 2 + 6 * Meshes[l].NumVertices;

                reader.ReadBlock(out Meshes[l].NumNormals);
                if (Meshes[l].NumNormals > 0)
                {
                    reader.ReadBlockArray(out Meshes[l].Normals, Meshes[l].NumNormals);
                    numBytes += 2 + 6 * Meshes[l].NumNormals;
                }
                else
                {
                    reader.ReadBlockArray(out Meshes[l].Lights, -Meshes[l].NumNormals);
                    numBytes += 2 - 2 * Meshes[l].NumNormals;
                }

                reader.ReadBlock(out Meshes[l].NumTexturedRectangles);
                reader.ReadBlockArray(out Meshes[l].TexturedRectangles, Meshes[l].NumTexturedRectangles);
                numBytes += 2 + 10 * Meshes[l].NumTexturedRectangles;

                reader.ReadBlock(out Meshes[l].NumTexturedTriangles);
                reader.ReadBlockArray(out Meshes[l].TexturedTriangles, Meshes[l].NumTexturedTriangles);
                numBytes += 2 + 8 * Meshes[l].NumTexturedTriangles;

                reader.ReadBlock(out Meshes[l].NumColoredRectangles);
                reader.ReadBlockArray(out Meshes[l].ColoredRectangles, Meshes[l].NumColoredRectangles);
                numBytes += 2 + 10 * Meshes[l].NumColoredRectangles;

                reader.ReadBlock(out Meshes[l].NumColoredTriangles);
                reader.ReadBlockArray(out Meshes[l].ColoredTriangles, Meshes[l].NumColoredTriangles);
                numBytes += 2 + 8 * Meshes[l].NumColoredTriangles;

                long offset2 = reader.BaseStream.Position;
                int diff = (int)(offset2 - offset1);
                if (diff % 4 != 0)
                {
                    short temp;
                    reader.ReadBlock(out temp);
                    diff += 2;
                }
                Meshes[l].MeshSize = numBytes;
                Meshes[l].MeshPointer = totalBytes;


                totalBytes += diff; // numBytes;
                numBytes = 0;
                l++;
            }

            Array.Resize(ref Meshes, l);

            NumMeshes = (uint)Meshes.Length;

            reader.ReadBlock(out NumMeshPointers);
            reader.ReadBlockArray(out MeshPointers, NumMeshPointers);

            reader.ReadBlock(out NumAnimations);
            //reader.ReadBlockArray(out Animations, NumAnimations);
            reader.ReadBytes((int)NumAnimations * 32);

            reader.ReadBlock(out NumStateChanges);
            reader.ReadBlockArray(out StateChanges, NumStateChanges);

            reader.ReadBlock(out NumAnimDispatches);
            reader.ReadBlockArray(out AnimDispatches, NumAnimDispatches);

            reader.ReadBlock(out NumAnimCommands);
            reader.ReadBlockArray(out AnimCommands, NumAnimCommands);

            reader.ReadBlock(out NumMeshTrees);
            reader.ReadBlockArray(out MeshTrees, NumMeshTrees);

            logger.Debug(reader.BaseStream.Position.ToString());
            reader.ReadBlock(out NumFrames);
            reader.ReadBlockArray(out Frames, NumFrames);

            reader.ReadBlock(out NumMoveables);
            reader.ReadBlockArray(out Moveables, NumMoveables);

            reader.ReadBlock(out NumStaticMeshes);
            reader.ReadBlockArray(out StaticMeshes, NumStaticMeshes);


            reader.ReadBlock(out NumSpriteTextures);
            reader.ReadBlockArray(out SpriteTextures, NumSpriteTextures);

            reader.ReadBlock(out NumSpriteSequences);
            reader.ReadBlockArray(out SpriteSequences, NumSpriteSequences);

            //return;
            //       ExportSprites();


            reader.ReadBlock(out NumCameras);
            reader.ReadBlockArray(out Cameras, NumCameras);

            reader.ReadBlock(out NumSoundSources);
            reader.ReadBlockArray(out SoundSources, NumSoundSources);

            reader.ReadBlock(out NumBoxes);
            reader.ReadBlockArray(out Boxes, NumBoxes);

            reader.ReadBlock(out NumOverlaps);
            reader.ReadBlockArray(out Overlaps, NumOverlaps);

            // reader.ReadBlockArray(out Zones, NumBoxes * 10);
            Zones = new short[NumBoxes * 10];
            for (int n = 0; n < NumBoxes * 10; n++)
            {
                Zones[n] = reader.ReadInt16();
            }

            reader.ReadBlock(out NumAnimatedTextures);
            short[] animTextures;
            reader.ReadBlockArray(out animTextures, NumAnimatedTextures);

            string fn = Path.GetFileNameWithoutExtension(_fileName);
            if (File.Exists("pathfinding." + fn + "." + ind + ".txt"))
                File.Delete("pathfinding." + fn + "." + ind + ".txt");
            StreamWriter writer = new StreamWriter(new FileStream("pathfinding." + fn + "." + ind + ".txt",
                FileMode.Create, FileAccess.Write, FileShare.None));

            writer.WriteLine("BOXES");

            for (int n = 0; n < Boxes.Length; n++)
            {
                writer.WriteLine("[" + n + "] " + "Xmin: " + Boxes[n].Xmin + ", " + "Xmax: " + Boxes[n].Xmax + ", " +
                                 "Zmin: " + Boxes[n].Zmin + ", " + "Zmax: " + Boxes[n].Zmax + ", " +
                                 "Floor: " + Boxes[n].TrueFloor + ", Overlap Index: " + Boxes[n].OverlapIndex);
            }

            writer.WriteLine(" ");
            writer.WriteLine("OVERLAPS");

            for (int n = 0; n < Overlaps.Length; n++)
            {
                writer.WriteLine("[" + n + "] " + (Overlaps[n] & 0x7fff).ToString());
                if ((Overlaps[n] & 0x8000) != 0)
                    writer.WriteLine("--- END OF LIST ---");
            }

            writer.WriteLine(" ");
            writer.WriteLine("ZONES");

            for (int n = 0; n < Boxes.Length; n++)
            {
                /*writer.WriteLine("[" + n + "] " + "Ground1: " + Zones[n * 10 + 0] + ", " + "Ground2: " + Zones[n * 10 + 1] + ", " +
                                 "Ground3: " + Zones[n * 10 + 2] + ", " + "Ground4: " + Zones[n * 10 + 3] + ", " +
                                 "Fly: " + Zones[n * 10 + 4] + ", A_Ground1: " + Zones[n * 10 + 5] + ", " + "A_Ground2: " + Zones[n * 10 + 6] + ", " +
                                 "A_Ground3: " + Zones[n * 10 + 7] + ", " + "A_Ground4: " + Zones[n * 10 + 8] + ", " +
                                 "A_Fly: " + Zones[n * 10 + 9]);*/
                writer.WriteLine("[" + n + "] " + "Ground1: " + Zones[n] + ", " + "Ground2: " +
                                 Zones[1 * NumBoxes + n] + ", " +
                                 "Ground3: " + Zones[2 * NumBoxes + n] + ", " + "Ground4: " + Zones[3 * NumBoxes + n] +
                                 ", " +
                                 "Fly: " + Zones[4 * NumBoxes + n] + ", A_Ground1: " + Zones[5 * NumBoxes + n] + ", " +
                                 "A_Ground2: " + Zones[6 * NumBoxes + n] + ", " +
                                 "A_Ground3: " + Zones[7 * NumBoxes + n] + ", " + "A_Ground4: " +
                                 Zones[8 * NumBoxes + n] + ", " +
                                 "A_Fly: " + Zones[9 * NumBoxes + n]);
            }

            writer.Flush();
            writer.Close();

            //  return;


            //reader.ReadBlockArray(out TEX, 4);

            reader.ReadBlock(out NumObjectTextures);
            //reader.ReadBlockArray(out ObjectTextures, NumObjectTextures);

            for (int ii = 0; ii < NumObjectTextures; ii++)
            {
                /* BinaryWriterEx tmpwriter2 = new BinaryWriterEx(new FileStream("test\\cleopal_" + ii + ".text", FileMode.Create, FileAccess.Write, FileShare.None));
                 tmpwriter2.WriteBlock(ObjectTextures[ii]);
                 tmpwriter2.Flush();
                 tmpwriter2.Close();*/
            }

            reader.ReadBytes((int)NumObjectTextures * 20);

            reader.ReadBlock(out NumItems);
            reader.ReadBlockArray(out Items, NumItems);

            reader.ReadBlock(out NumAiItems);
            reader.ReadBlockArray(out AiItems, NumItems);

            StreamWriter aiw = new StreamWriter(new FileStream("AI" + ind + ".txt", FileMode.Create, FileAccess.Write,
                FileShare.None));

            for (int n = 0; n < NumAiItems; n++)
            {
                aiw.WriteLine("[" + n + "]");
                aiw.WriteLine("    ObjectID: " + AiItems[n].ObjectID);
                aiw.WriteLine("    X: " + AiItems[n].X);
                aiw.WriteLine("    Y: " + AiItems[n].Y);
                aiw.WriteLine("    Z: " + AiItems[n].Z);
                aiw.WriteLine("    Room: " + AiItems[n].Room);
                aiw.WriteLine("    Angle: " + AiItems[n].Angle);
            }

            aiw.Flush();
            aiw.Close();
        }

        struct TrTempSprite
        {
            public int X;
            public int Y;
            public int Width;
            public int Height;
            public int Page;
            public int OriginalId;
            public int NewX;
            public int NewY;
            public int NewPage;
        }

        private void ExportSprites()
        {
            List<TrTempSprite> tempTexturesArray = new List<TrTempSprite>();

            for (int i = 0; i < NumSpriteTextures; i++)
            {
                TrTempSprite txt = new TrTempSprite();

                txt.Width = (SpriteTextures[i].Width - 255) / 256;
                txt.Height = (SpriteTextures[i].Width - 255) / 256;
                txt.X = SpriteTextures[i].X;
                txt.Y = SpriteTextures[i].Y;
                txt.Page = SpriteTextures[i].Tile;
                txt.OriginalId = i;

                tempTexturesArray.Add(txt);
            }

            for (int i = 0; i < tempTexturesArray.Count - 1; i++)
            {
                for (int k = 0; k < tempTexturesArray.Count - 1 - i; k++)
                {
                    if (tempTexturesArray[k].Height < tempTexturesArray[k + 1].Height)
                    {
                        TrTempSprite temp = tempTexturesArray[k];
                        tempTexturesArray[k] = tempTexturesArray[k + 1];
                        tempTexturesArray[k + 1] = temp;
                    }
                }
            }


            // I've sorted the textures by height, now I build the texture map
            int numRoomTexturePages = 1;
            int maxHeight = 0;

            byte[,] raw = new byte[512, 512];

            for (int i = 0; i < tempTexturesArray.Count; i++)
            {
                TrTempSprite tex = tempTexturesArray[i];

                int currX = 0;
                int currY = 0;
                bool newLine;
                if (256 - (currX + tex.Width) < 0)
                {
                    newLine = true;
                    if (256 * numRoomTexturePages - (currY + maxHeight + tex.Height) < 0)
                    {
                        numRoomTexturePages++;
                        currY = 256 * (numRoomTexturePages - 1);
                        currX = 0;
                    }
                    else
                    {
                        currX = 0;
                        currY += maxHeight;
                    }
                }
                else
                {
                    newLine = false;
                }

                if (tex.Height > maxHeight)
                    maxHeight = tex.Height;
                if (newLine)
                    maxHeight = tex.Height;

                if (256 * numRoomTexturePages - (currY + maxHeight) < 0)
                {
                    numRoomTexturePages++;
                    currY = 256 * (numRoomTexturePages - 1);
                    currX = 0;
                }

                tex.NewX = currX;
                tex.NewY = currY;
                tex.NewPage = 0;

                for (int x = 0; x < tex.Width; x++)
                {
                    for (int y = 0; y < tex.Height; y++)
                    {
                        byte b1 = _texture16[tex.X * 2 + 2 * x + 0, tex.Page * 256 + tex.Y + y];
                        byte b2 = _texture16[tex.X * 2 + 2 * x + 1, tex.Page * 256 + tex.Y + y];

                        raw[currX * 2 + x * 2 + 0, currY + y] = b1;
                        raw[currX * 2 + x * 2 + 1, currY + y] = b2;
                    }
                }
                currX += tex.Width;
            }

            BinaryWriterEx writer =
                new BinaryWriterEx(new FileStream("sprites3.raw", FileMode.Create, FileAccess.Write, FileShare.None));

            for (int z2 = 0; z2 < 256; z2++)
            {
                for (int x2 = 0; x2 < 512; x2++)
                {
                    writer.Write(raw[x2, z2]);
                }
            }

            writer.Flush();
            writer.Close();

            writer = new BinaryWriterEx(new FileStream("sprites3.bin", FileMode.Create, FileAccess.Write,
                FileShare.None));

            writer.WriteBlock(NumSpriteTextures);

            for (int i = 0; i < NumSpriteTextures; i++)
            {
                for (int j = 0; j < NumSpriteTextures; j++)
                {
                    if (tempTexturesArray[j].OriginalId == i)
                    {
                        short page = 0;
                        byte w = (byte)((tempTexturesArray[j].NewX));
                        byte h = (byte)((tempTexturesArray[j].NewY));

                        writer.Write(page);
                        writer.Write(w);
                        writer.Write(h);
                        writer.Write(SpriteTextures[i].Width);
                        writer.Write(SpriteTextures[i].Height);
                        writer.Write(SpriteTextures[i].LeftSide);
                        writer.Write(SpriteTextures[i].TopSide);
                        writer.Write(SpriteTextures[i].RightSide);
                        writer.Write(SpriteTextures[i].BottomSide);

                        break;
                    }
                }
            }

            writer.WriteBlock(NumSpriteSequences);
            writer.WriteBlockArray(SpriteSequences);

            writer.Flush();
            writer.Close();
        }
    }
}
