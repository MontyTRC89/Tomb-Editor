using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;
using ICSharpCode.SharpZipLib.Zip.Compression;
using ICSharpCode.SharpZipLib.Zip.Compression.Streams;
using System.IO;
using TombLib.IO;

namespace TombEngine
{

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct tr3_face4
    {
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
        public ushort[] Vertices;
        public ushort Texture;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct tr3_face3
    {
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 3)]
        public ushort[] Vertices;
        public ushort Texture;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct tr3_mesh
    {
        public tr_vertex Centre;
        public int Radius;
        public short NumVertices;
        public tr_vertex[] Vertices;
        public short NumNormals;
        public tr_vertex[] Normals;
        public short[] Lights;
        public short NumTexturedRectangles;
        public tr3_face4[] TexturedRectangles;
        public short NumTexturedTriangles;
        public tr3_face3[] TexturedTriangles;
        public short NumColoredRectangles;
        public tr3_face4[] ColoredRectangles;
        public short NumColoredTriangles;
        public tr3_face3[] ColoredTriangles;
        public int MeshSize;
        public int MeshPointer;
    }


    class TombRaider3Level
    {
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
        public tr_room[] Rooms;

        public uint NumFloorData;
        public short[] FloorData;

        public uint NumMeshData;
        public uint NumMeshes;
        public tr3_mesh[] Meshes;

        public uint NumMeshPointers;
        public uint[] MeshPointers;
        public uint NumAnimations;
        //public tr_animation[] Animations;
        public uint NumStateChanges;
        public tr_state_change[] StateChanges;
        public uint NumAnimDispatches;
        public tr_anim_dispatch[] AnimDispatches;
        public uint NumAnimCommands;
        public short[] AnimCommands;
        public uint NumMeshTrees;
        public int[] MeshTrees;
        public uint NumFrames;
        public ushort[] Frames;
        public uint NumMoveables;
        public tr_moveable[] Moveables;
        public uint NumStaticMeshes;
        public tr_staticmesh[] StaticMeshes;

        //public byte[] SPR;

        public uint NumSpriteTextures;
        public tr_sprite_texture[] SpriteTextures;
        public uint NumSpriteSequences;
        public tr_sprite_sequence[] SpriteSequences;
        public uint NumCameras;
        public tr_camera[] Cameras;
        //public uint NumFlyByCameras;
        //public byte[] FlyByCameras;
        public uint NumSoundSources;
        public tr_sound_source[] SoundSources;
        public uint NumBoxes;
        public tr_box[] Boxes;
        public uint NumOverlaps;
        public ushort[] Overlaps;
        public short[] Zones;
        public uint NumAnimatedTextures;
        //public ushort[] AnimatedTextures;

        //public byte[] TEX;

        public uint NumObjectTextures;
        //public tr_object_texture[] ObjectTextures;
        public uint NumItems;
        public tr_item[] Items;
        public uint NumAiItems;
        public tr_ai_item[] AiItems;

        string fileName;
        byte[,] _texture16;

        public TombRaider3Level(string fileName )
        {
            this.fileName = fileName;
        }

        public void Load(string ind)
        {
            FileStream fileStream = File.OpenRead(fileName);
            BinaryReaderEx reader = new BinaryReaderEx(fileStream);
            MemoryStream stream = new MemoryStream();
            Inflater inflater = new Inflater(false);

            reader.ReadBlock(out Version);
            byte[] palette=reader.ReadBytes(768);
            BinaryWriterEx wrPalette = new BinaryWriterEx(File.OpenWrite("Palette.bin"));
            for (int jj = 0; jj< palette.Length; jj++)
            {
                byte col = (byte)(palette[jj] * 4);
                wrPalette.Write(col);
            }

            wrPalette.Flush();
            wrPalette.Close();
            

            reader.ReadBytes(1024);

            int numTextureTiles = 0;
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

            StreamWriter wp = new StreamWriter(File.OpenWrite("portals" + ind + ".txt"));

            Rooms = new tr_room[NumRooms];
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
                if (Rooms[i].NumVertices > max) max = Rooms[i].NumVertices;
                reader.ReadBlock(out Rooms[i].NumRectangles);
                Rooms[i].Rectangles = new tr_face4[Rooms[i].NumRectangles];
                for (int j = 0; j < Rooms[i].NumRectangles; j++)
                {
                    // Rooms[i].Rectangles[j].Vertices = new ushort[4];
                    reader.ReadBlockArray(out Rooms[i].Rectangles[j].Vertices, 4);
                    reader.ReadBlock(out Rooms[i].Rectangles[j].Texture);
                }

                reader.ReadBlock(out Rooms[i].NumTriangles);
                Rooms[i].Triangles = new tr_face3[Rooms[i].NumTriangles];
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

                for (int nn=0;nn<Rooms[i].Portals.Length;nn++)
                {
                    tr_room_portal pt = Rooms[i].Portals[nn];
                    wp.WriteLine(nn + ": ");
                    wp.WriteLine("Room: " + pt.AdjoiningRoom);
                    for (int vv = 0; vv < 4; vv++)
                    {
                        wp.Write("V" + vv + " = " + pt.Vertices[vv].X + ", " + pt.Vertices[vv].Y + ", " + pt.Vertices[vv].Z);
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
            short temp = 0;

            Meshes = new tr3_mesh[2048];
            while (totalBytes < (NumMeshData * 2))
            {
                long offset1 = reader.BaseStream.Position;

                reader.ReadBlock(out Meshes[l].Centre);
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
                if (diff % 4 != 0) { reader.ReadBlock(out temp); diff += 2; }
                Meshes[l].MeshSize = numBytes;
                Meshes[l].MeshPointer = totalBytes;


                totalBytes +=diff ;// numBytes;
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

            Console.WriteLine(reader.BaseStream.Position.ToString());
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
            for (int n=0;n<NumBoxes*10;n++)
            {
                Zones[n] = reader.ReadInt16();
            }

            reader.ReadBlock(out NumAnimatedTextures);
            short[] animTextures;
            reader.ReadBlockArray(out animTextures, NumAnimatedTextures);

            string fn = Path.GetFileNameWithoutExtension(fileName);
            if (File.Exists("pathfinding." + fn + "." + ind + ".txt")) File.Delete("pathfinding." + fn + "." + ind + ".txt");
            StreamWriter writer = new StreamWriter(File.OpenWrite("pathfinding." + fn + "." + ind + ".txt"));

            writer.WriteLine("BOXES");

            for (int n=0;n<Boxes.Length;n++)
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
                if ((Overlaps[n] & 0x8000) != 0) writer.WriteLine("--- END OF LIST ---");
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
                writer.WriteLine("[" + n + "] " + "Ground1: " + Zones[n] + ", " + "Ground2: " + Zones[1*NumBoxes+ n ] + ", " +
                                "Ground3: " + Zones[2 * NumBoxes + n] + ", " + "Ground4: " + Zones[3 * NumBoxes + n] + ", " +
                                "Fly: " + Zones[4 * NumBoxes + n] + ", A_Ground1: " + Zones[5 * NumBoxes + n] + ", " + "A_Ground2: " + Zones[6 * NumBoxes + n] + ", " +
                                "A_Ground3: " + Zones[7 * NumBoxes + n] + ", " + "A_Ground4: " + Zones[8 * NumBoxes + n] + ", " +
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
               /* BinaryWriterEx tmpwriter2 = new BinaryWriterEx(File.OpenWrite("test\\cleopal_" + ii + ".text"));
                tmpwriter2.WriteBlock(ObjectTextures[ii]);
                tmpwriter2.Flush();
                tmpwriter2.Close();*/
            }

            reader.ReadBytes((int)NumObjectTextures * 20);

            reader.ReadBlock(out NumItems);
            reader.ReadBlockArray(out Items, NumItems);

            reader.ReadBlock(out NumAiItems);
            reader.ReadBlockArray(out AiItems, NumItems);

            StreamWriter aiw = new StreamWriter(File.OpenWrite("AI" + ind + ".txt"));

            for (int n=0;n< NumAiItems;n++)
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

        struct tr_temp_sprite
        {
            public int x;
            public int y;
            public int width;
            public int height;
            public int page;
            public int originalID;
            public int NewX;
            public int NewY;
            public int NewPage;
        }

        private void ExportSprites()
        {
            List<tr_temp_sprite> _tempTexturesArray = new List<tr_temp_sprite>();

            for (int i = 0; i<NumSpriteTextures; i++)
            {
                tr_temp_sprite txt = new tr_temp_sprite();

                txt.width = (SpriteTextures[i].Width-255)/256;
                txt.height = (SpriteTextures[i].Width - 255) / 256;
                txt.x = SpriteTextures[i].X;
                txt.y = SpriteTextures[i].Y;
                txt.page = SpriteTextures[i].Tile;
                txt.originalID = i;

                _tempTexturesArray.Add(txt);
            }

            for (int i = 0; i < _tempTexturesArray.Count - 1; i++)
            {
                for (int k = 0; k < _tempTexturesArray.Count - 1 - i; k++)
                {
                    if (_tempTexturesArray[k].height < _tempTexturesArray[k + 1].height)
                    {
                       tr_temp_sprite temp = _tempTexturesArray[k];
                        _tempTexturesArray[k] = _tempTexturesArray[k + 1];
                        _tempTexturesArray[k + 1] = temp;
                    }
                }
            }

    
            // I've sorted the textures by height, now I build the texture map
            bool newLine = false;
            int numRoomTexturePages = 1;
            int maxHeight = 0;

            byte[,] raw = new byte[512, 512];

            for (int i = 0; i < _tempTexturesArray.Count; i++)
            {
                tr_temp_sprite tex = _tempTexturesArray[i];

				int currX = 0;
				int currY = 0;
				if (256 - (currX + tex.width) < 0)
                {
                    newLine = true;
                    if (256 * numRoomTexturePages - (currY + maxHeight + tex.height) < 0)
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

                if (tex.height > maxHeight) maxHeight = tex.height;
                if (newLine) maxHeight = tex.height;

                if (256 * numRoomTexturePages - (currY + maxHeight) < 0)
                {
                    numRoomTexturePages++;
                    currY = 256 * (numRoomTexturePages - 1);
                    currX = 0;
                }

                tex.NewX = currX;
                tex.NewY = currY;
                tex.NewPage = 0;

                for (int x = 0; x < tex.width; x++)
                {
                    for (int y = 0; y < tex.height; y++)
                    {
                        byte b1 = _texture16[tex.x * 2 + 2 * x+0, tex.page * 256 + tex.y + y];
                        byte b2 = _texture16[tex.x * 2 + 2 * x+1, tex.page * 256 + tex.y + y];

                        raw[currX * 2 + x * 2 + 0, currY + y] = b1;
                        raw[currX * 2 + x * 2 + 1, currY + y] = b2;

                    }
                }
                currX += tex.width;
            }
			
            BinaryWriterEx writer = new BinaryWriterEx(File.OpenWrite("sprites3.raw"));

            for (int z2 = 0; z2 < 256; z2++)
            {
                for (int x2 = 0; x2 < 512; x2++)
                {
                    writer.Write(raw[x2, z2]);
                }
            }

            writer.Flush();
            writer.Close();

            writer = new BinaryWriterEx(File.OpenWrite("sprites3.bin"));

            writer.WriteBlock(NumSpriteTextures);

            for (int i = 0; i < NumSpriteTextures; i++)
            {
                for (int j = 0; j < NumSpriteTextures; j++)
                {
                    if (_tempTexturesArray[j].originalID==i)
                    {
                        short page = 0;
                        byte w =(byte)( (_tempTexturesArray[j].NewX));
                        byte h = (byte)((_tempTexturesArray[j].NewY));

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
