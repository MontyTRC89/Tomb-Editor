using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TombLib.IO;
using System.IO;
using System.Runtime.InteropServices;
using System.Drawing;
using TombLib.Wad;
using SharpDX;
using TombLib.Graphics;

namespace TombLib.Wad
{
    public class TR4Wad
    {
        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct wad_object_texture
        {
            public byte X;
            public byte Y;
            public ushort Page;
            public sbyte FlipX;
            public byte Width;
            public sbyte FlipY;
            public byte Height;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct wad_vertex
        {
            public short X;
            public short Y;
            public short Z;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct wad_polygon
        {
            public ushort Shape;
            public ushort V1;
            public ushort V2;
            public ushort V3;
            public ushort V4;
            public ushort Texture;
            public byte Attributes;
            public byte Unknown;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct wad_mesh
        {
            public short SphereX;
            public short SphereY;
            public short SphereZ;
            public ushort Radius;
            public ushort Unknown;
            public ushort NumVertices;
            public List<wad_vertex> Vertices;
            public short NumNormals;
            public List<wad_vertex> Normals;
            public List<short> Shades;
            public ushort NumPolygons;
            public List<wad_polygon> Polygons;

            public Vector3 Minimum;
            public Vector3 Maximum;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct wad_animation
        {
            public uint KeyFrameOffset;
            public byte FrameDuration;
            public byte KeyFrameSize;
            public ushort StateId;
            public int Speed;
            public int Accel;
            public int SpeedLateral;
            public int AccelLateral;
            public ushort FrameStart;
            public ushort FrameEnd;
            public ushort NextAnimation;
            public ushort NextFrame;
            public ushort NumStateChanges;
            public ushort ChangesIndex;
            public ushort NumCommands;
            public ushort CommandOffset;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct wad_state_change
        {
            public ushort StateId;
            public ushort NumDispatches;
            public ushort DispatchesIndex;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct wad_anim_dispatch
        {
            public short Low;
            public short High;
            public short NextAnimation;
            public short NextFrame;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct wad_moveable
        {
            public uint ObjectID;
            public ushort NumPointers;
            public ushort PointerIndex;
            public uint LinksIndex;
            public uint KeyFrameOffset;
            public short AnimationIndex;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct wad_static_mesh
        {
            public uint ObjectId;
            public ushort PointersIndex;
            public short VisibilityX1;
            public short VisibilityX2;
            public short VisibilityY1;
            public short VisibilityY2;
            public short VisibilityZ1;
            public short VisibilityZ2;
            public short CollisionX1;
            public short CollisionX2;
            public short CollisionY1;
            public short CollisionY2;
            public short CollisionZ1;
            public short CollisionZ2;
            public ushort Flags;
        }

        public struct texture_piece
        {
            public byte Width;
            public byte Height;
            public byte[] Data;
        }

        public List<wad_object_texture> Textures;
        public byte[,] TexturePages;
        public int NumTexturePages;
        public List<uint> Pointers;
        public List<uint> RealPointers;
        public List<uint> HelperPointers;
        public List<wad_mesh> Meshes;
        public List<wad_animation> Animations;
        public List<wad_state_change> Changes;
        public List<wad_anim_dispatch> Dispatches;
        public List<short> Commands;
        public List<int> Links;
        public List<short> KeyFrames;
        public List<wad_moveable> Moveables;
        public List<wad_static_mesh> StaticMeshes;

        public List<string> Sounds { get; set; }
        public string BaseName { get; set; }
        public string BasePath { get; set; }

        public TR4Wad()
        {
            Textures = new List<wad_object_texture>();
            Pointers = new List<uint>();
            Meshes = new List<wad_mesh>();
            Animations = new List<wad_animation>();
            Changes = new List<wad_state_change>();
            Dispatches = new List<wad_anim_dispatch>();
            Commands = new List<short>();
            KeyFrames = new List<short>();
            Moveables = new List<wad_moveable>();
            StaticMeshes = new List<wad_static_mesh>();
            Links = new List<int>();
            RealPointers = new List<uint>();
            HelperPointers = new List<uint>();
            Sounds = new List<string>();
        }

        public void LoadWad(string fileName)
        {
            BaseName = Path.GetFileNameWithoutExtension(fileName);
            BasePath = Path.GetDirectoryName(fileName);

            // inizializzo lo stream
            FileStream inputStream = File.OpenRead(fileName);
            BinaryReaderEx reader = new BinaryReaderEx(inputStream);

            // leggo la versione
            int version = reader.ReadInt32();
            if (version != 129) throw new InvalidDataException();

            // leggo le texture
            uint numTextures = reader.ReadUInt32();
            for (int i = 0; i < numTextures; i++)
            {
                wad_object_texture text;
                reader.ReadBlock<wad_object_texture>(out text);
                Textures.Add(text);
            }

            uint numTextureBytes = reader.ReadUInt32();
            TexturePages = new byte[numTextureBytes / 768, 768];
            for (int y = 0; y < numTextureBytes / 768; y++)
                for (int x = 0; x < 768; x++)
                    TexturePages[y, x] = reader.ReadByte();

            NumTexturePages = (int)(numTextureBytes / 196608);

            // leggo le mesh
            uint numMeshPointers = reader.ReadUInt32();
            for (int i = 0; i < numMeshPointers; i++)
            {
                Pointers.Add(reader.ReadUInt32());
                RealPointers.Add(0);
                HelperPointers.Add(0);
            }

            uint numMeshWords = reader.ReadUInt32();
            uint bytesRead = 0;
            uint numMeshes = 0;

            while (bytesRead < (numMeshWords * 2))
            {
                uint startOfMesh = (uint)reader.BaseStream.Position;

                wad_mesh mesh = new wad_mesh();
                mesh.Polygons = new List<wad_polygon>();
                mesh.Vertices = new List<wad_vertex>();
                mesh.Normals = new List<wad_vertex>();
                mesh.Shades = new List<short>();

                mesh.SphereX = reader.ReadInt16();
                mesh.SphereY = reader.ReadInt16();
                mesh.SphereZ = reader.ReadInt16();
                mesh.Radius = reader.ReadUInt16();
                mesh.Unknown = reader.ReadUInt16();

                ushort numVertices = reader.ReadUInt16();
                mesh.NumVertices = numVertices;

                int xMin = Int32.MaxValue;
                int yMin = Int32.MaxValue;
                int zMin = Int32.MaxValue;
                int xMax = Int32.MinValue;
                int yMax = Int32.MinValue;
                int zMax = Int32.MinValue;
                
                for (int i = 0; i < numVertices; i++)
                {
                    wad_vertex v;
                    reader.ReadBlock<wad_vertex>(out v);

                    if (v.X < xMin) xMin = v.X;
                    if (-v.Y < yMin) yMin = -v.Y;
                    if (v.Z < zMin) zMin = v.Z;

                    if (v.X > xMax) xMax = v.X;
                    if (-v.Y > yMax) yMax = -v.Y;
                    if (v.Z > zMax) zMax = v.Z;

                    mesh.Vertices.Add(v);
                }

                mesh.Minimum = new Vector3(xMin, yMin, zMin);
                mesh.Maximum = new Vector3(xMax, yMax, zMax);

                short numNormals = reader.ReadInt16();
                mesh.NumNormals = numNormals;
                if (numNormals > 0)
                {
                    for (int i = 0; i < numNormals; i++)
                    {
                        wad_vertex v;
                        reader.ReadBlock<wad_vertex>(out v);
                        mesh.Normals.Add(v);
                    }
                }
                else
                {
                    for (int i = 0; i < -numNormals; i++)
                    {
                        mesh.Shades.Add(reader.ReadInt16());
                    }
                }

                ushort numPolygons = reader.ReadUInt16();
                mesh.NumPolygons = numPolygons;
                ushort numQuads = 0;
                for (int i = 0; i < numPolygons; i++)
                {
                    wad_polygon poly = new wad_polygon();
                    poly.Shape = reader.ReadUInt16();
                    poly.V1 = reader.ReadUInt16();
                    poly.V2 = reader.ReadUInt16();
                    poly.V3 = reader.ReadUInt16();
                    if (poly.Shape == 9) poly.V4 = reader.ReadUInt16();
                    poly.Texture = reader.ReadUInt16();
                    poly.Attributes = reader.ReadByte();
                    poly.Unknown = reader.ReadByte();

                    if (poly.Shape == 9) numQuads++;
                    mesh.Polygons.Add(poly);
                }

                if (numQuads % 2 != 0) reader.ReadInt16();

                uint endPosition = (uint)reader.BaseStream.Position;
                bytesRead += endPosition - startOfMesh;
                Meshes.Add(mesh);

                // aggiorno i real pointers
                for (int k = 0; k < Pointers.Count; k++)
                {
                    if (Pointers[k] == bytesRead)
                    {
                        RealPointers[k] = (uint)Meshes.Count;
                        HelperPointers[k] = (uint)Meshes.Count;
                    }

                }


            }

            // leggo le animazioni
            uint numAnimations = reader.ReadUInt32();
            for (int i = 0; i < numAnimations; i++)
            {
                wad_animation anim;
                reader.ReadBlock<wad_animation>(out anim);
                Animations.Add(anim);
            }

            uint numChanges = reader.ReadUInt32();
            for (int i = 0; i < numChanges; i++)
            {
                wad_state_change change;
                reader.ReadBlock<wad_state_change>(out change);
                Changes.Add(change);
            }

            uint numDispatches = reader.ReadUInt32();
            for (int i = 0; i < numDispatches; i++)
            {
                wad_anim_dispatch anim;
                reader.ReadBlock<wad_anim_dispatch>(out anim);
                Dispatches.Add(anim);
            }

            uint numCommands = reader.ReadUInt32();
            for (int i = 0; i < numCommands; i++)
            {
                short anim;
                reader.ReadBlock<short>(out anim);
                Commands.Add(anim);
            }

            uint numLinks = reader.ReadUInt32();
            for (int i = 0; i < numLinks; i++)
            {
                int link;
                reader.ReadBlock<int>(out link);
                Links.Add(link);
            }

            uint numFrames = reader.ReadUInt32();
            for (int i = 0; i < numFrames; i++)
            {
                short frame;
                reader.ReadBlock<short>(out frame);
                KeyFrames.Add(frame);
            }

            // leggo gli oggetti
            uint numMoveables = reader.ReadUInt32();
            for (int i = 0; i < numMoveables; i++)
            {
                long pos = reader.BaseStream.Position;
                wad_moveable moveable;
                reader.ReadBlock<wad_moveable>(out moveable);
                Moveables.Add(moveable);
            }

            uint numStaticMeshes = reader.ReadUInt32();
            for (int i = 0; i < numStaticMeshes; i++)
            {
                wad_static_mesh staticMesh;
                reader.ReadBlock<wad_static_mesh>(out staticMesh);
                StaticMeshes.Add(staticMesh);
            }

            reader.Close();

            StreamReader readerSounds = new StreamReader(File.OpenRead(BasePath + "\\" + BaseName + ".sam"));
            while (!readerSounds.EndOfStream)
            {
                Sounds.Add(readerSounds.ReadLine());
            }
        }

        public Wad GetTheWad()
        {
            TR4Wad old = this;
            Wad wad = new Wad();
            wad.OriginalWad = this;

            // converto le texture
            int numPages = old.TexturePages.Length / 196608;
            for (uint i = 0; i < numPages; i++)
            {
                WadTexturePage page = new WadTexturePage();
                page.Type = WadTexturePageType.Shared;
                page.TexturePage = new byte[256, 1024];

                for (int y = 0; y < 256; y++)
                {
                    for (int x = 0; x < 256; x++)
                    {
                        page.TexturePage[y, x * 4 + 0] = old.TexturePages[i * 256 + y, x * 3 + 0];
                        page.TexturePage[y, x * 4 + 1] = old.TexturePages[i * 256 + y, x * 3 + 1];
                        page.TexturePage[y, x * 4 + 2] = old.TexturePages[i * 256 + y, x * 3 + 2];
                        page.TexturePage[y, x * 4 + 3] = (byte)(old.TexturePages[i * 256 + y, x * 3 + 0] == 255 && old.TexturePages[i * 256 + y, x * 3 + 1] == 0 &&
                            old.TexturePages[i * 256 + y, x * 3 + 2] == 0 ? 0 : 255); // alpha channel
                    }
                }

                wad.TexturePages.Add(i, page);
            }

            for (int i = 0; i < old.Textures.Count; i++)
            {
                WadTextureSample sample = new WadTextureSample();

                sample.Type = WadTextureSampleType.CoreWad;
                sample.X = old.Textures[i].X;
                sample.Y = old.Textures[i].Y;
                sample.FlipX = old.Textures[i].FlipX;
                sample.FlipY = old.Textures[i].FlipY;
                sample.Width = old.Textures[i].Width;
                sample.Height = old.Textures[i].Height;
                sample.Page = (short)old.Textures[i].Page;

                wad.TextureSamples.Add((uint)i, sample);
            }

            // ora converto i moveables
            for (int i = 0; i < Moveables.Count; i++)
            {
                WadMoveable moveable = new WadMoveable();
                wad_moveable m = Moveables[i];

                moveable.ObjectID = m.ObjectID;

                // costruisco la lista delle mesh di questo moveable
                List<wad_mesh> meshes = new List<wad_mesh>();
                for (int j = 0; j < m.NumPointers; j++)
                    meshes.Add(Meshes[(int)RealPointers[(int)(m.PointerIndex + j)]]);

                // converto le mesh
                for (int j = 0; j < meshes.Count; j++)
                    moveable.Meshes.Add(GetNewMesh(meshes[j]));

                int currentLink = (int)m.LinksIndex;

                if (currentLink >= Links.Count)
                    moveable.Offset = Vector3.Zero;
                else
                    moveable.Offset = Vector3.Zero;  //new Vector3(_links[currentLink + 1], -_links[currentLink + 2], _links[currentLink + 3]);

                // converto lo scheletro
                for (int j = 0; j < meshes.Count - 1; j++)
                {
                    WadLink link = new WadLink();

                    link.Opcode = (WadLinkOpcode)Links[currentLink];
                    link.X = Links[currentLink + 1];
                    link.Y = Links[currentLink + 2];
                    link.Z = Links[currentLink + 3];

                    currentLink += 4;

                    moveable.Links.Add(link);
                }

                // converto le animazioni
                int numAnimations = 0;
                int nextMoveable = GetNextMoveableWithAnimations(i);

                if (nextMoveable == -1)
                    numAnimations = Animations.Count - m.AnimationIndex;
                else
                    numAnimations = Moveables[nextMoveable].AnimationIndex - m.AnimationIndex;

                for (int j = 0; j < numAnimations; j++)
                {
                    if (m.AnimationIndex == -1) break;

                    WadAnimation animation = new WadAnimation();
                    wad_animation anim = Animations[j + m.AnimationIndex];
                    //animation.Acceleration = anim.Accel;
                    //animation.Speed = anim.Speed;
                    animation.FrameDuration = anim.FrameDuration;
                    animation.NextAnimation = (ushort)(anim.NextAnimation - m.AnimationIndex);
                    animation.NextFrame = 0;// GetNextFrame(anim.NextAnimation, anim.NextFrame);
                    animation.StateId = (ushort)anim.StateId;

                    for (int k = 0; k < anim.NumStateChanges; k++)
                    {
                        WadStateChange sc = new WadStateChange();
                        wad_state_change wadSc = Changes[(int)anim.ChangesIndex + k];
                        sc.StateId = (ushort)wadSc.StateId;
                        sc.Dispatches = new WadAnimDispatch[wadSc.NumDispatches];

                        for (int n = 0; n < wadSc.NumDispatches; n++)
                        {
                            WadAnimDispatch ad = new WadAnimDispatch();
                            wad_anim_dispatch wadAd = Dispatches[(int)wadSc.DispatchesIndex + n];
                            ad.InFrame = (ushort)wadAd.Low;
                            ad.OutFrame = (ushort)wadAd.High;
                            ad.NextAnimation = (ushort)(wadAd.NextAnimation - m.AnimationIndex);
                            ad.NextFrame = 0;// GetNextFrame(wadAd.NextAnimation, wadAd.NextFrame);
                            sc.Dispatches[n] = ad;
                        }

                        animation.StateChanges.Add(sc);
                    }

                    //  animation.AnimCommands = new List<short>();
                    if (anim.NumCommands < Commands.Count)
                    {
                        for (int k = 0; k < anim.NumCommands; k++)
                        {
                            animation.AnimCommands.Add((short)Commands[(int)anim.CommandOffset + k]);
                        }
                    }

                    int frames = (int)anim.KeyFrameOffset / 2;
                    uint numFrames;

                    if (j + m.AnimationIndex == Animations.Count - 1)
                    {
                        numFrames = ((uint)(2 * KeyFrames.Count) - anim.KeyFrameOffset) / (uint)(2 * anim.KeyFrameSize);
                    }
                    else
                    {
                        if (anim.KeyFrameSize == 0)
                        {
                            numFrames = 0;
                        }
                        else
                        {
                            numFrames = (Animations[m.AnimationIndex + j + 1].KeyFrameOffset - anim.KeyFrameOffset) / (uint)(2 * anim.KeyFrameSize);
                        }
                    }

                    for (int f = 0; f < numFrames; f++)
                    {
                        WadKeyFrame frame = new WadKeyFrame();
                        int startOfFrame = frames;

                        frame.BoundingBox1 = new WadVertex();
                        frame.BoundingBox2 = new WadVertex();

                        frame.BoundingBox1.X = KeyFrames[frames];
                        frame.BoundingBox2.X = KeyFrames[frames + 1];

                        frame.BoundingBox1.Y = KeyFrames[frames + 2];
                        frame.BoundingBox2.Y = KeyFrames[frames + 3];

                        frame.BoundingBox1.Z = KeyFrames[frames + 4];
                        frame.BoundingBox2.Z = KeyFrames[frames + 5];

                        frames += 6;

                        frame.Offset = new WadVertex();
                        frame.Offset.X = KeyFrames[frames];
                        frame.Offset.Y = KeyFrames[frames + 1];
                        frame.Offset.Z = KeyFrames[frames + 2];

                        frames += 3;

                        frame.Angles = new WadKeyFrameRotation[m.NumPointers];

                        for (int n = 0; n < frame.Angles.Length; n++)
                        {

                            short rot = KeyFrames[frames];
                            WadKeyFrameRotation kfAngle = new WadKeyFrameRotation();

                            switch (rot & 0xc000)
                            {
                                case 0:
                                    int rotation = rot;//( rot <<16) + _keyFrames[frames + 1];
                                    int rotation2 = KeyFrames[frames + 1];
                                    frames += 2;
                                    float rotZ = rotation2 & 0x3ff;
                                    rotZ *= 360.0f / 1024.0f;
                                    rotZ *= 2 * (float)Math.PI / 360.0f;
                                    int angle = ((rotation2 & 0xfc00) >> 10) + ((rotation & 0xf) << 6);
                                    float rotY = angle & 0x3ff;
                                    rotY *= 360.0f / 1024.0f;
                                    rotY *= 2 * (float)Math.PI / 360.0f;
                                    //  rotation = rotation >> 10;
                                    float rotX = ((rotation & 0x3ff0) >> 4);
                                    rotX *= 360.0f / 1024.0f;
                                    rotX *= 2 * (float)Math.PI / 360.0f;

                                    kfAngle.Axis = WadKeyFrameRotationAxis.ThreeAxes;
                                    kfAngle.X = rotX;
                                    kfAngle.Y = rotY;
                                    kfAngle.Z = rotZ;

                                    break;

                                case 0x4000:
                                    float rotationX = rot & 0x3fff;
                                    frames += 1;
                                    rotationX *= 360.0f / 4096.0f;
                                    rotationX *= 2 * (float)Math.PI / 360.0f;

                                    kfAngle.Axis = WadKeyFrameRotationAxis.AxisX;
                                    kfAngle.X = rotationX;

                                    break;

                                case 0x8000:
                                    float rotationY = rot & 0x3fff;
                                    frames += 1;
                                    rotationY *= 360.0f / 4096.0f * 2 * (float)Math.PI / 360.0f;
                                    kfAngle.Axis = WadKeyFrameRotationAxis.AxisY;
                                    kfAngle.Y = rotationY;
                                    break;

                                case 0xc000:
                                    float rotationZ = rot & 0x3fff;
                                    frames += 1;
                                    rotationZ *= 360.0f / 4096.0f * 2 * (float)Math.PI / 360.0f;
                                    kfAngle.Axis = WadKeyFrameRotationAxis.AxisZ;
                                    kfAngle.Z = rotationZ;
                                    break;
                            }

                            frame.Angles[n] = kfAngle;
                        }

                        if ((frames - startOfFrame) < anim.KeyFrameSize) frames += ((int)anim.KeyFrameSize - (frames - startOfFrame));

                        animation.KeyFrames.Add(frame);
                    }

                    moveable.Animations.Add(animation);
                }

                if (moveable.Animations.Count != 0 && moveable.Animations[0].KeyFrames.Count > 0)
                {
                    WadKeyFrame kf = moveable.Animations[0].KeyFrames[0];
                    Vector3 offset = new Vector3(kf.Offset.X, -kf.Offset.Y, kf.Offset.Z);
                    //offset = Vector3.Zero;

                     moveable.BoundingBox = new BoundingBox(new Vector3(kf.BoundingBox1.X, -kf.BoundingBox2.Y, kf.BoundingBox1.Z) + offset,
                                                           new Vector3(kf.BoundingBox2.X, -kf.BoundingBox1.Y, kf.BoundingBox2.Z) + offset);
                }
                else
                {
                    moveable.BoundingBox = new BoundingBox(new Vector3(-128, -128, -128), new Vector3(128, 128, 128));
                }

                wad.WadMoveables.Add(m.ObjectID, moveable);
            }

            for (int i = 0; i < StaticMeshes.Count; i++)
            {
                WadStaticMesh staticMesh = new WadStaticMesh();
                wad_static_mesh oldStaticMesh = StaticMeshes[i];

                staticMesh.ObjectID = oldStaticMesh.ObjectId;
                staticMesh.Mesh = GetNewMesh(Meshes[(int)RealPointers[(int)oldStaticMesh.PointersIndex]]);
                staticMesh.Flags = (short)oldStaticMesh.Flags;
                staticMesh.VisibilityBox1 = new WadVertex();
                staticMesh.VisibilityBox1.X = (short)oldStaticMesh.VisibilityX1;
                staticMesh.VisibilityBox1.Y = (short)oldStaticMesh.VisibilityY1;
                staticMesh.VisibilityBox1.Z = (short)oldStaticMesh.VisibilityZ1;
                staticMesh.VisibilityBox2.X = (short)oldStaticMesh.VisibilityX2;
                staticMesh.VisibilityBox2.Y = (short)oldStaticMesh.VisibilityY2;
                staticMesh.VisibilityBox2.Z = (short)oldStaticMesh.VisibilityZ2;
                staticMesh.CollisionBox1.X = (short)oldStaticMesh.CollisionX1;
                staticMesh.CollisionBox1.Y = (short)oldStaticMesh.CollisionY1;
                staticMesh.CollisionBox1.Z = (short)oldStaticMesh.CollisionZ1;
                staticMesh.CollisionBox2.X = (short)oldStaticMesh.CollisionX2;
                staticMesh.CollisionBox2.Y = (short)oldStaticMesh.CollisionY2;
                staticMesh.CollisionBox2.Z = (short)oldStaticMesh.CollisionZ2;

                staticMesh.BoundingBox = new BoundingBox(new Vector3(oldStaticMesh.VisibilityX1, oldStaticMesh.VisibilityY1, oldStaticMesh.VisibilityZ1),
                                                         new Vector3(oldStaticMesh.VisibilityX2, oldStaticMesh.VisibilityY2, oldStaticMesh.VisibilityZ2));

                wad.WasStaticMeshes.Add(oldStaticMesh.ObjectId, staticMesh);
            }

            return wad;
        }

        private WadMesh GetNewMesh(wad_mesh oldMesh)
        {
            //wad_mesh oldMesh = _meshes[index];
            WadMesh mesh = new WadMesh();

            mesh.SphereX = oldMesh.SphereX;
            mesh.SphereY = oldMesh.SphereY;
            mesh.SphereZ = oldMesh.SphereZ;
            mesh.Radius = oldMesh.Radius;
            mesh.Unknown = oldMesh.Unknown;

            mesh.NumVertices = (ushort)oldMesh.Vertices.Count;
            mesh.Vertices = new WadVertex[oldMesh.Vertices.Count];
            for (int i = 0; i < oldMesh.Vertices.Count; i++)
            {
                WadVertex vertex = new WadVertex();

                vertex.X = oldMesh.Vertices[i].X;
                vertex.Y = oldMesh.Vertices[i].Y;
                vertex.Z = oldMesh.Vertices[i].Z;

                mesh.Vertices[i] = vertex;
            }

            if (oldMesh.Normals != null && oldMesh.Normals.Count > 0)
                mesh.NumNormals = (short)oldMesh.Normals.Count;

            if (oldMesh.Shades != null && oldMesh.Shades.Count > 0)
                mesh.NumNormals = (short)-oldMesh.Shades.Count;

            if (mesh.NumNormals > 0)
            {
                mesh.Normals = new WadVertex[oldMesh.Vertices.Count];
                for (int i = 0; i < oldMesh.Normals.Count; i++)
                {
                    WadVertex vertex = new WadVertex();

                    vertex.X = oldMesh.Vertices[i].X;
                    vertex.Y = oldMesh.Vertices[i].Y;
                    vertex.Z = oldMesh.Vertices[i].Z;

                    mesh.Normals[i] = vertex;
                }
            }
            else
            {
                mesh.Shades = new short[-mesh.NumNormals];
                for (int i = 0; i < oldMesh.Shades.Count; i++)
                    mesh.Shades[i] = oldMesh.Shades[i];
            }

            mesh.NumPolygons = (ushort)oldMesh.Polygons.Count;
            mesh.Polygons = new WadPolygon[mesh.NumPolygons];
            for (int i = 0; i < mesh.NumPolygons; i++)
            {
                WadPolygon poly = new WadPolygon();

                poly.V1 = oldMesh.Polygons[i].V1;
                poly.V2 = oldMesh.Polygons[i].V2;
                poly.V3 = oldMesh.Polygons[i].V3;

                if (oldMesh.Polygons[i].Shape == 9)
                {
                    poly.V4 = oldMesh.Polygons[i].V4;
                    poly.Shape = Shape.Rectangle;
                }
                else
                {
                    poly.Shape = Shape.Triangle;
                }

                poly.Attributes = oldMesh.Polygons[i].Attributes;
                poly.Texture = oldMesh.Polygons[i].Texture;

                mesh.Polygons[i] = poly;
            }

            mesh.BoundingBox = new BoundingBox(oldMesh.Minimum, oldMesh.Maximum);

            return mesh;
        }

        private int GetNextMoveableWithAnimations(int current)
        {
            for (int i = current + 1; i < Moveables.Count; i++)
                if (Moveables[i].AnimationIndex != -1) return i;
            return -1;
        }

        private short GetNextFrame(int animation, int frame)
        {
            short output = 0;

            if (animation > Animations.Count - 1) return 0;

            for (int i = 0; i < animation; i++)
            {
                output += (short)(Animations[i].FrameEnd - Animations[i].FrameStart + 1);
            }

            output = (short)(frame - output);
            return output;
        }
    }
}
