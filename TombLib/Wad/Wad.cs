using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using TombLib.IO;
using SharpDX.Toolkit.Graphics;
using SharpDX;
using TombLib.Graphics;
using System.Drawing;

namespace TombLib.Wad
{
    public class Wad
    {
        public TR4Wad OriginalWad { get; set; }

        // dati provenienti dal WAD
        public Dictionary<uint, WadMoveable> WadMoveables { get; } = new Dictionary<uint, WadMoveable>();
        public Dictionary<uint, WadStaticMesh> WasStaticMeshes { get; } = new Dictionary<uint, WadStaticMesh>();
        public Dictionary<uint, WadTexturePage> TexturePages { get; } = new Dictionary<uint, WadTexturePage>();
        public Dictionary<uint, WadTextureSample> TextureSamples { get; } = new Dictionary<uint, WadTextureSample>();

        // il device DirectX 11
        private GraphicsDevice _device;

        // firma del WAD
        //private static string _magicWord = "WAD2";

        // i dati del WAD in formato DirectX 11
        public Dictionary<uint, Texture2D> Textures { get; } = new Dictionary<uint, Texture2D>();
        public Dictionary<uint, SkinnedModel> Moveables { get; } = new Dictionary<uint, SkinnedModel>();
        public Dictionary<uint, StaticModel> StaticMeshes { get; } = new Dictionary<uint, StaticModel>();

        // sound samples
        public List<string> Samples { get; set; }
        public GraphicsDevice GraphicsDevice
        {
            get
            {
                return _device;
            }
            set
            {
                _device = value;
            }
        }

        public static Wad LoadWad(string filename)
        {
            TR4Wad original = new TR4Wad();
            original.LoadWad(filename);
            return original.GetTheWad();
        }

        public void DisposeWad()
        {
            if (Textures.Count != 0)
            {
                Textures[0].Dispose();
                Textures.Remove(0);
            }
        }

        /*
        public static Wad LoadWad(string fileName)
        {
            // Apro uno stream
            FileStream inputFile = File.OpenRead(fileName);
            BinaryReaderEx reader = new BinaryReaderEx(inputFile);

            // creo il nuovo wad
            Wad wad = new Wad();

            // inizio la lettura
            byte[] buffer = reader.ReadBytes(4);
            string magicWord = ASCIIEncoding.ASCII.GetString(buffer);
            if (magicWord != "WAD2") throw new InvalidDataException();

            // leggo le texture
            int numTextures = reader.ReadInt16();
            for (uint i = 0; i < numTextures; i++)
            {
                WadTexturePage page = new WadTexturePage();
                page.Type = (WadTexturePageType)reader.ReadByte();
                page.TexturePage = new byte[256, 1024];

                for (int y = 0; y < 256; y++)
                {
                    for (int x = 0; x < 1024; x++)
                    {
                        page.TexturePage[y, x] = reader.ReadByte();
                    }
                }
                    
                wad.TexturePages.Add(i, page);
            }

            // leggo i sample
            int numTextureSamples = reader.ReadInt16();
            for (uint i = 0; i < numTextureSamples; i++)
            {
                WadTextureSample sample;
                reader.ReadBlock<WadTextureSample>(out sample);
                wad.TextureSamples.Add(i, sample);
            }

            // leggo i modelli animati
            short numSkinnedModels = reader.ReadInt16();
            for (int i = 0; i < numSkinnedModels; i++)
            {
                // creo il nuovo modello
                WadMoveable model = new WadMoveable();

                // leggo il boundingbox
                model.ObjectID = reader.ReadUInt32();

                // leggo le mesh
                short numMeshes = reader.ReadInt16();

                for (int j = 0; j < numMeshes; j++)
                {
                    // creo la mesh
                    WadMesh mesh = new WadMesh();

                    // leggo la boundign sphere
                    mesh.SphereX = reader.ReadInt16();
                    mesh.SphereY = reader.ReadInt16();
                    mesh.SphereZ = reader.ReadInt16();
                    mesh.Radius = reader.ReadUInt16();
                    mesh.Unknown = reader.ReadUInt16();

                    // leggo i vertici
                    mesh.NumVertices = reader.ReadUInt16();
                    reader.ReadBlockArray<WadVertex>(out mesh.Vertices, mesh.NumVertices);

                    // leggo le normali o le luci
                    mesh.NumNormals = reader.ReadInt16();
                    if (mesh.NumNormals > 0)
                        reader.ReadBlockArray<WadVertex>(out mesh.Normals, mesh.NumNormals);
                    else
                        reader.ReadBlockArray<short>(out mesh.Shades, -mesh.NumNormals);

                    // leggo i poligoni
                    mesh.NumPolygons = reader.ReadUInt16();
                    reader.ReadBlockArray<WadPolygon>(out mesh.Polygons, mesh.NumPolygons);

                    model.Meshes.Add(mesh);
                }

                // leggo lo scheletro
                for (int j = 0; j < numMeshes - 1; j++)
                {
                    WadLink link;
                    reader.ReadBlock<WadLink>(out link);
                    model.Links.Add(link);
                }

                // leggo le animazioni
                short numAnimations = reader.ReadInt16();
                for (int j = 0; j < numAnimations; j++)
                {
                    WadAnimation anim = new WadAnimation();

                    anim.FrameDuration = reader.ReadByte();
                    anim.StateId = reader.ReadUInt16();
                    anim.Unknown1 = reader.ReadInt16();
                    anim.Speed = reader.ReadInt16();
                    anim.Acceleration = reader.ReadInt32();
                    anim.Unknown2 = reader.ReadInt32();
                    anim.Unknown3 = reader.ReadInt32();
                    anim.NextAnimation = reader.ReadUInt16();
                    anim.NextFrame = reader.ReadUInt16();

                    ushort numFrames = reader.ReadUInt16();
                    for (int k = 0; k < numFrames; k++)
                    {
                        WadKeyFrame keyframe = new WadKeyFrame();

                        reader.ReadBlock<WadVertex>(out keyframe.BoundingBox1);
                        reader.ReadBlock<WadVertex>(out keyframe.BoundingBox2);
                        reader.ReadBlock<WadVertex>(out keyframe.Offset);

                        reader.ReadBlockArray<WadKeyFrameRotation>(out keyframe.Angles, numMeshes);

                        anim.KeyFrames.Add(keyframe);
                    }

                    ushort numStateChanges = reader.ReadUInt16();
                    for (int k = 0; k < numStateChanges; k++)
                    {
                        WadStateChange sc = new WadStateChange();

                        sc.StateId = reader.ReadUInt16();
                        ushort numDispatches = reader.ReadUInt16();
                        reader.ReadBlockArray<WadAnimDispatch>(out sc.Dispatches, numDispatches);

                        anim.StateChanges.Add(sc);
                    }

                    ushort numAnimCommands = reader.ReadUInt16();
                    for (int k = 0; k < numAnimCommands; k++)
                    {
                        anim.AnimCommands.Add(reader.ReadInt16());
                    }

                    model.Animations.Add(anim);
                }

                wad.WadMoveables.Add(model.ObjectID, model);
            }

            // leggo i modelli statici
            short numStaticModels = reader.ReadInt16();
            for (int i = 0; i < numStaticModels; i++)
            {
                // leggo l'id
                uint objectId = reader.ReadUInt32();

                // creo il nuovo modello
                WadStaticMesh staticMesh = new WadStaticMesh();

                // leggo la mesh
                WadMesh mesh = new WadMesh();

                // leggo la boundign sphere
                mesh.SphereX = reader.ReadInt16();
                mesh.SphereY = reader.ReadInt16();
                mesh.SphereZ = reader.ReadInt16();
                mesh.Radius = reader.ReadUInt16();
                mesh.Unknown = reader.ReadUInt16();

                // leggo i vertici
                mesh.NumVertices = reader.ReadUInt16();
                reader.ReadBlockArray<WadVertex>(out mesh.Vertices, mesh.NumVertices);

                // leggo le normali o le luci
                mesh.NumNormals = reader.ReadInt16();
                if (mesh.NumNormals > 0)
                    reader.ReadBlockArray<WadVertex>(out mesh.Normals, mesh.NumNormals);
                else
                    reader.ReadBlockArray<short>(out mesh.Shades, -mesh.NumNormals);

                // leggo i poligoni
                mesh.NumPolygons = reader.ReadUInt16();
                reader.ReadBlockArray<WadPolygon>(out mesh.Polygons, mesh.NumPolygons);

                staticMesh.Mesh = mesh;

                // leggo i flag
                staticMesh.Flags = reader.ReadInt16();

                // leggo i box
                reader.ReadBlock<WadVertex>(out staticMesh.VisibilityBox1);
                reader.ReadBlock<WadVertex>(out staticMesh.VisibilityBox2);
                reader.ReadBlock<WadVertex>(out staticMesh.CollisionBox1);
                reader.ReadBlock<WadVertex>(out staticMesh.CollisionBox2);

                wad.WasStaticMeshes.Add(objectId, staticMesh);
            }

            return wad;
        }
        */

        public static void SaveWad(string fileName, Wad wad)
        {
            // Apro uno stream in scrittura
            FileStream outputFile = File.OpenWrite(fileName);
            BinaryWriterEx writer = new BinaryWriterEx(outputFile);

            // Scrivo la parola magica
            writer.Write(ASCIIEncoding.ASCII.GetBytes("WAD2"), 0, 4);

            // scrivo le texture
            short numTextures = (short)wad.TexturePages.Count;
            writer.Write(numTextures);
            for (int i = 0; i < wad.TexturePages.Count; i++)
            {
                WadTexturePage page = wad.TexturePages.ElementAt(i).Value;
                writer.Write((byte)page.Type);
                for (int y = 0; y < 256; y++)
                    for (int x = 0; x < 1024; x++)
                        writer.Write(page.TexturePage[y, x]);
            }

            // scrivo i sample
            short numTextureSamples = (short)wad.TextureSamples.Count;
            writer.Write(numTextureSamples);
            for (int i = 0; i < numTextureSamples; i++)
            {
                writer.WriteBlock<WadTextureSample>(wad.TextureSamples.ElementAt(i).Value);
            }

            // scrivo i moveable
            short numMoveables = (short)wad.WadMoveables.Count;
            writer.WriteBlock(numMoveables);
            for (int i = 0; i < numMoveables; i++)
            {
                WadMoveable moveable = wad.WadMoveables.ElementAt(i).Value;

                writer.Write(moveable.ObjectID);

                writer.Write((short)moveable.Meshes.Count);
                for (int k = 0; k < moveable.Meshes.Count; k++)
                {
                    WadMesh mesh = moveable.Meshes[k];

                    writer.Write(mesh.SphereX);
                    writer.Write(mesh.SphereY);
                    writer.Write(mesh.SphereZ);
                    writer.Write(mesh.Radius);
                    writer.Write(mesh.Unknown);

                    writer.Write(mesh.NumVertices);
                    writer.WriteBlockArray<WadVertex>(mesh.Vertices);

                    writer.Write(mesh.NumNormals);
                    if (mesh.NumNormals > 0)
                        writer.WriteBlockArray<WadVertex>(mesh.Normals);
                    else
                        writer.WriteBlockArray<short>(mesh.Shades);

                    writer.Write(mesh.NumPolygons);
                    writer.WriteBlockArray(mesh.Polygons);
                }

                for (int k = 0; k < moveable.Meshes.Count - 1; k++)
                    writer.WriteBlock<WadLink>(moveable.Links[k]);

                writer.Write((short)moveable.Animations.Count);
                for (int k = 0; k < moveable.Animations.Count; k++)
                {
                    WadAnimation anim = moveable.Animations[k];

                    writer.Write(anim.FrameDuration);
                    writer.Write(anim.StateId);
                    writer.Write(anim.Unknown1);
                    writer.Write(anim.Speed);
                    writer.Write(anim.Acceleration);
                    writer.Write(anim.Unknown2);
                    writer.Write(anim.Unknown3);
                    writer.Write(anim.NextAnimation);
                    writer.Write(anim.NextFrame);

                    writer.Write((ushort)anim.KeyFrames.Count);
                    for (int f = 0; f < anim.KeyFrames.Count; f++)
                    {
                        WadKeyFrame keyframe = anim.KeyFrames[f];

                        writer.WriteBlock(keyframe.BoundingBox1);
                        writer.WriteBlock(keyframe.BoundingBox2);
                        writer.WriteBlock(keyframe.Offset);

                        writer.WriteBlockArray(keyframe.Angles);
                    }

                    writer.Write((ushort)anim.StateChanges.Count);
                    for (int f = 0; f < anim.StateChanges.Count; f++)
                    {
                        WadStateChange sc = anim.StateChanges[f];

                        writer.Write(sc.StateId);
                        writer.Write((ushort)sc.Dispatches.Length);
                        writer.WriteBlockArray(sc.Dispatches);
                    }

                    writer.Write((ushort)anim.AnimCommands.Count);
                    writer.WriteBlockArray(anim.AnimCommands.ToArray());
                }
            }

            // scrivo le static meshes
            writer.Write((short)wad.WasStaticMeshes.Count);
            for (int i = 0; i < wad.WasStaticMeshes.Count; i++)
            {
                WadStaticMesh staticMesh = wad.WasStaticMeshes.ElementAt(i).Value;

                writer.Write(staticMesh.ObjectID);

                WadMesh mesh = staticMesh.Mesh;

                writer.Write(mesh.SphereX);
                writer.Write(mesh.SphereY);
                writer.Write(mesh.SphereZ);
                writer.Write(mesh.Radius);
                writer.Write(mesh.Unknown);

                writer.Write(mesh.NumVertices);
                writer.WriteBlockArray<WadVertex>(mesh.Vertices);

                writer.Write(mesh.NumNormals);
                if (mesh.NumNormals > 0)
                    writer.WriteBlockArray<WadVertex>(mesh.Normals);
                else
                    writer.WriteBlockArray<short>(mesh.Shades);

                writer.Write(mesh.NumPolygons);
                writer.WriteBlockArray(mesh.Polygons);

                writer.Write(staticMesh.Flags);

                writer.WriteBlock(staticMesh.VisibilityBox1);
                writer.WriteBlock(staticMesh.VisibilityBox2);
                writer.WriteBlock(staticMesh.CollisionBox1);
                writer.WriteBlock(staticMesh.CollisionBox2);
            }

            // termino la scrittura
            writer.Flush();
            writer.Close();
        }

        public void PrepareDataForDirectX()
        {
            int currentXblock = 0;
            int currentYblock = 0;

            // Copy the page in a temp bitmap. I generate a texture atlas, putting all texture pages inside 2048x2048 pixel 
            // textures.
            Bitmap tempBitmap = new Bitmap(2048, 2048, System.Drawing.Imaging.PixelFormat.Format32bppArgb);

            for (uint i = 0; i < TexturePages.Count; i++)
            {
                WadTexturePage page = TexturePages[i];

                for (int x = 0; x < 256; x++)
                {
                    for (int y = 0; y < 256; y++)
                    {
                        int x1 = currentXblock * 256 + x;
                        int y1 = currentYblock * 256 + y;

                        System.Drawing.Color c = System.Drawing.Color.FromArgb(page.TexturePage[y, x * 4 + 3],
                                                                               page.TexturePage[y, x * 4],
                                                                               page.TexturePage[y, x * 4 + 1],
                                                                               page.TexturePage[y, x * 4 + 2]);

                        tempBitmap.SetPixel(x1, y1, System.Drawing.Color.FromArgb(255, c.R, c.G, c.B));
                    }
                }

                currentXblock++;
                if (currentXblock == 8)
                {
                    currentXblock = 0;
                    currentYblock++;
                }
            }

            // Create DirectX texture
            MemoryStream outputTexture = new MemoryStream();
            tempBitmap.Save(outputTexture, System.Drawing.Imaging.ImageFormat.Png);
            outputTexture.Seek(0, SeekOrigin.Begin);
            Texture2D newTexture = Texture2D.Load(_device, outputTexture, TextureFlags.None, SharpDX.Direct3D11.ResourceUsage.Default);

            // Add texture to the dictionary
            Textures.Add(0, newTexture);

            // Clean used memory
            outputTexture.Close();
            tempBitmap.Dispose();

            // creo i moveable
            for (int i = 0; i < WadMoveables.Count; i++)
            {
                WadMoveable mov = WadMoveables.ElementAt(i).Value;
                SkinnedModel model = new SkinnedModel(_device, mov.ObjectID);
                model.Offset = mov.Offset;

                // inizializzo le mesh
                for (int m = 0; m < mov.Meshes.Count; m++)
                {
                    WadMesh msh = mov.Meshes[m];
                    SkinnedMesh mesh = new SkinnedMesh(_device, mov.ObjectID.ToString() + "_mesh_" + i.ToString());

                    mesh.BoundingBox = msh.BoundingBox;

                    for (int j = 0; j < TexturePages.Count; j++)
                    {
                        Submesh submesh = new Submesh();
                        submesh.Material = new TombLib.Graphics.Material();
                        submesh.Material.Type = MaterialType.Flat;
                        submesh.Material.Name = "material_" + j.ToString();
                        submesh.Material.DiffuseMap = (uint)j;
                        mesh.SubMeshes.Add(submesh);
                    }

                    for (int j = 0; j < msh.Polygons.Length; j++)
                    {
                        WadPolygon poly = msh.Polygons[j];
                        int textureId = poly.Texture & 0xfff;
                        if (textureId > 2047)
                            textureId = -(textureId - 4096);
                        short submeshIndex = TextureSamples[(uint)textureId].Page;

                        List<Vector2> uv = CalculateUVCoordinates(poly);

                        if (poly.Shape == Shape.Triangle)
                        {
                            AddSkinnedVertexAndIndex(msh.Vertices[poly.V1], mesh, uv[0], submeshIndex, i);
                            AddSkinnedVertexAndIndex(msh.Vertices[poly.V2], mesh, uv[1], submeshIndex, i);
                            AddSkinnedVertexAndIndex(msh.Vertices[poly.V3], mesh, uv[2], submeshIndex, i);
                        }
                        else
                        {
                            AddSkinnedVertexAndIndex(msh.Vertices[poly.V1], mesh, uv[0], submeshIndex, i);
                            AddSkinnedVertexAndIndex(msh.Vertices[poly.V2], mesh, uv[1], submeshIndex, i);
                            AddSkinnedVertexAndIndex(msh.Vertices[poly.V4], mesh, uv[3], submeshIndex, i);

                            AddSkinnedVertexAndIndex(msh.Vertices[poly.V4], mesh, uv[3], submeshIndex, i);
                            AddSkinnedVertexAndIndex(msh.Vertices[poly.V2], mesh, uv[1], submeshIndex, i);
                            AddSkinnedVertexAndIndex(msh.Vertices[poly.V3], mesh, uv[2], submeshIndex, i);

                        }
                    }

                    for (int j = 0; j < mesh.SubMeshes.Count; j++)
                    {
                        Submesh current = mesh.SubMeshes[j];
                        current.StartIndex = (ushort)mesh.Indices.Count;
                        for (int k = 0; k < current.Indices.Count; k++)
                            mesh.Indices.Add(current.Indices[k]);
                        current.NumIndices = (ushort)current.Indices.Count;
                    }

                    mesh.BoundingSphere = new BoundingSphere(new Vector3(msh.SphereX, msh.SphereY, msh.SphereZ), msh.Radius);

                    model.Meshes.Add(mesh);
                }

                // inizializzo lo scheletro
                Bone root = new Bone();
                root.Name = "root_bone";
                root.Parent = null;
                root.Transform = Matrix.Identity;
                root.Index = 0;
                model.Bones.Add(root);
                model.Root = root;
                model.Transforms.Add(Matrix.Translation(Vector3.Zero));
                model.InverseTransforms.Add(Matrix.Translation(Vector3.Zero));
                model.AnimationTransforms.Add(Matrix.Translation(Vector3.Zero));

                for (int j = 0; j < mov.Meshes.Count - 1; j++)
                {
                    Bone bone = new Bone();
                    bone.Name = "bone_" + (j + 1).ToString();
                    bone.Parent = null;
                    bone.Transform = Matrix.Translation(Vector3.Zero);
                    bone.Index = (short)(j + 1);
                    model.Transforms.Add(Matrix.Translation(Vector3.Zero));
                    model.InverseTransforms.Add(Matrix.Translation(Vector3.Zero));
                    model.AnimationTransforms.Add(Matrix.Translation(Vector3.Zero));
                    model.Bones.Add(bone);
                }

                Bone currentBone = root;
                Bone stackBone = root;
                Stack<Bone> stack = new Stack<Bone>();

                for (int m = 0; m < (mov.Meshes.Count - 1); m++)
                {
                    int j = m + 1;
                    WadLink link = mov.Links[m];

                    switch (link.Opcode)
                    {
                        case WadLinkOpcode.NotUseStack:
                            model.Bones[j].Transform = Matrix.Translation(new Vector3(link.X, -link.Y, link.Z));
                            model.Bones[j].Parent = currentBone;
                            currentBone.Children.Add(model.Bones[j]);
                            currentBone = model.Bones[j];

                            break;
                        case WadLinkOpcode.Push:
                            try
                            {
                                currentBone = stack.Pop();
                            }
                            catch (Exception)
                            {
                                //numErrors++;
                                //processedLinks += 4;
                                continue;
                            }

                            model.Bones[j].Transform = Matrix.Translation(new Vector3(link.X, -link.Y, link.Z));
                            model.Bones[j].Parent = currentBone;
                            currentBone.Children.Add(model.Bones[j]);
                            currentBone = model.Bones[j];

                            break;
                        case WadLinkOpcode.Pop:
                            model.Bones[j].Transform = Matrix.Translation(new Vector3(link.X, -link.Y, link.Z));
                            try
                            {
                                stack.Push(currentBone);
                            }
                            catch (Exception)
                            {
                                //  numErrors++;
                                //  processedLinks += 4;
                                continue;
                            }
                            model.Bones[j].Parent = currentBone;
                            currentBone.Children.Add(model.Bones[j]);
                            currentBone = model.Bones[j];

                            break;
                        case WadLinkOpcode.Read:
                            Bone bone;
                            try
                            {
                                bone = stack.Pop();
                            }
                            catch (Exception)
                            {
                                // numErrors++;
                                // processedLinks += 4;
                                continue;
                            }
                            model.Bones[j].Transform = Matrix.Translation(new Vector3(link.X, -link.Y, link.Z));
                            model.Bones[j].Parent = bone;
                            bone.Children.Add(model.Bones[j]);
                            currentBone = model.Bones[j];
                            stack.Push(bone);

                            break;
                    }
                }

                // preparo le animazioni
                for (int j = 0; j < mov.Animations.Count; j++)
                {
                    Animation animation = new Animation();
                    WadAnimation wadAnim = mov.Animations[j];

                    animation.KeyFrames = new List<KeyFrame>();

                    for (int f = 0; f < wadAnim.KeyFrames.Count; f++)
                    {
                        KeyFrame frame = new KeyFrame();
                        WadKeyFrame wadFrame = wadAnim.KeyFrames[f];

                        for (int k = 0; k < mov.Meshes.Count; k++)
                        {
                            frame.Rotations.Add(Matrix.Identity);
                            frame.Translations.Add(Matrix.Identity);
                        }

                        frame.Translations[0] = Matrix.Translation(new Vector3(wadFrame.Offset.X, -wadFrame.Offset.Y, wadFrame.Offset.Z));

                        for (int k = 1; k < frame.Translations.Count; k++)
                            frame.Translations[k] = Matrix.Translation(Vector3.Zero);

                        for (int n = 0; n < frame.Rotations.Count; n++)
                        {
                            WadKeyFrameRotation rot = wadFrame.Angles[n];
                            switch (rot.Axis)
                            {
                                case WadKeyFrameRotationAxis.ThreeAxes:
                                    frame.Rotations[n] = Matrix.RotationYawPitchRoll((float)rot.Y, (float)-rot.X, (float)-rot.Z);
                                    break;

                                case WadKeyFrameRotationAxis.AxisX:
                                    frame.Rotations[n] = Matrix.RotationX((float)-rot.X);
                                    break;

                                case WadKeyFrameRotationAxis.AxisY:
                                    frame.Rotations[n] = Matrix.RotationY((float)rot.Y);
                                    break;

                                case WadKeyFrameRotationAxis.AxisZ:
                                    frame.Rotations[n] = Matrix.RotationZ((float)-rot.Z);
                                    break;
                            }
                        }

                        animation.KeyFrames.Add(frame);
                    }

                    model.Animations.Add(animation);
                }

                Moveables.Add(model.ObjectID, model);
            }

            // creo le static mesh
            for (int i = 0; i < WasStaticMeshes.Count; i++)
            {
                WadStaticMesh mov = WasStaticMeshes.ElementAt(i).Value;
                StaticModel model = new StaticModel(_device, mov.ObjectID);

                // inizializzo le mesh
                WadMesh msh = mov.Mesh;
                StaticMesh mesh = new StaticMesh(_device, mov.ObjectID.ToString() + "_mesh_" + i.ToString());
                mesh.BoundingBox = msh.BoundingBox;

                for (int j = 0; j < TexturePages.Count; j++)
                {
                    Submesh submesh = new Submesh();
                    submesh.Material = new TombLib.Graphics.Material();
                    submesh.Material.Type = MaterialType.Flat;
                    submesh.Material.Name = "material_" + j.ToString();
                    submesh.Material.DiffuseMap = (uint)j;
                    mesh.SubMeshes.Add(submesh);
                }

                for (int j = 0; j < msh.Polygons.Length; j++)
                {
                    WadPolygon poly = msh.Polygons[j];
                    int textureId = poly.Texture & 0xfff;
                    if (textureId > 2047)
                        textureId = -(textureId - 4096);
                    short submeshIndex = TextureSamples[(uint)textureId].Page;

                    List<Vector2> uv = CalculateUVCoordinates(poly);

                    if (poly.Shape == Shape.Triangle)
                    {

                        AddStaticVertexAndIndex(msh.Vertices[poly.V1], mesh, uv[0], submeshIndex);
                        AddStaticVertexAndIndex(msh.Vertices[poly.V2], mesh, uv[1], submeshIndex);
                        AddStaticVertexAndIndex(msh.Vertices[poly.V3], mesh, uv[2], submeshIndex);
                    }
                    else
                    {
                        AddStaticVertexAndIndex(msh.Vertices[poly.V1], mesh, uv[0], submeshIndex);
                        AddStaticVertexAndIndex(msh.Vertices[poly.V2], mesh, uv[1], submeshIndex);
                        AddStaticVertexAndIndex(msh.Vertices[poly.V4], mesh, uv[3], submeshIndex);

                        AddStaticVertexAndIndex(msh.Vertices[poly.V4], mesh, uv[3], submeshIndex);
                        AddStaticVertexAndIndex(msh.Vertices[poly.V2], mesh, uv[1], submeshIndex);
                        AddStaticVertexAndIndex(msh.Vertices[poly.V3], mesh, uv[2], submeshIndex);

                    }
                }

                for (int j = 0; j < mesh.SubMeshes.Count; j++)
                {
                    Submesh current = mesh.SubMeshes[j];
                    current.StartIndex = (ushort)mesh.Indices.Count;
                    for (int k = 0; k < current.Indices.Count; k++)
                        mesh.Indices.Add(current.Indices[k]);
                    current.NumIndices = (ushort)current.Indices.Count;
                }

                mesh.BoundingSphere = new BoundingSphere(new Vector3(msh.SphereX, msh.SphereY, msh.SphereZ), msh.Radius);

                model.Meshes.Add(mesh);

                StaticMeshes.Add(model.ObjectID, model);
            }
        }

        private Texture2D GetDirectXTexture(WadTexturePage page)
        {
            //  FileStream outputTexture = new FileStream("D:\\texture.png", FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.ReadWrite);
            MemoryStream outputTexture = new MemoryStream();
            using (var bmp = new Bitmap(256, 256, System.Drawing.Imaging.PixelFormat.Format32bppRgb))
            {

                for (int y = 0; y < 256; y++)
                    for (int x = 0; x < 1024; x = x + 4)
                        bmp.SetPixel(x / 4, y, System.Drawing.Color.FromArgb(page.TexturePage[y, x + 3], page.TexturePage[y, x], page.TexturePage[y, x + 1], page.TexturePage[y, x + 2]));

                bmp.Save(outputTexture, System.Drawing.Imaging.ImageFormat.Png);
                //     bmp.Dispose();
            }

            outputTexture.Seek(0, SeekOrigin.Begin);
            Texture2D newTexture = Texture2D.Load(_device, outputTexture, TextureFlags.None, SharpDX.Direct3D11.ResourceUsage.Default);

            return newTexture;
        }

        private List<Vector2> CalculateUVCoordinates(WadPolygon poly)
        {
            List<Vector2> uv = new List<Vector2>();

            // recupero le informazioni necessarie
            int shape = (poly.Texture & 0x7000) >> 12;
            int flipped = (poly.Texture & 0x8000) >> 15;
            int textureId = poly.Texture & 0xfff;
            if (textureId > 2047)
                textureId = -(textureId - 4096);


            // calcolo i quattro angoli della texture
            WadTextureSample texture = TextureSamples[(uint)textureId];

            int yBlock = (int)(texture.Page / 8);
            int xBlock = (int)(texture.Page % 8);

            Vector2 nw = new Vector2((xBlock * 256 + texture.X) / 2048.0f, (yBlock * 256 + texture.Y) / 2048.0f);
            Vector2 ne = new Vector2((xBlock * 256 + texture.X + texture.Width) / 2048.0f, (yBlock * 256 + texture.Y) / 2048.0f);
            Vector2 se = new Vector2((xBlock * 256 + texture.X + texture.Width) / 2048.0f, (yBlock * 256 + texture.Y + texture.Height) / 2048.0f);
            Vector2 sw = new Vector2((xBlock * 256 + texture.X) / 2048.0f, (yBlock * 256 + texture.Y + texture.Height) / 2048.0f);

            // in base alla forma assegno nel giusto ordine le coordinate
            if (poly.Shape == Shape.Rectangle)
            {
                if (flipped == 1)
                {
                    uv.Add(ne);
                    uv.Add(nw);
                    uv.Add(sw);
                    uv.Add(se);
                }
                else
                {
                    uv.Add(nw);
                    uv.Add(ne);
                    uv.Add(se);
                    uv.Add(sw);
                }
            }
            else
            {
                switch (shape)
                {
                    case 0:
                        if (flipped == 1)
                        {
                            uv.Add(ne);
                            uv.Add(nw);
                            uv.Add(se);
                        }
                        else
                        {
                            uv.Add(nw);
                            uv.Add(ne);
                            uv.Add(sw);
                        }
                        break;

                    case 2:
                        if (flipped == 1)
                        {
                            uv.Add(nw);
                            uv.Add(sw);
                            uv.Add(ne);
                        }
                        else
                        {
                            uv.Add(ne);
                            uv.Add(se);
                            uv.Add(nw);
                        }
                        break;

                    case 4:
                        if (flipped == 1)
                        {
                            uv.Add(sw);
                            uv.Add(se);
                            uv.Add(nw);
                        }
                        else
                        {
                            uv.Add(se);
                            uv.Add(sw);
                            uv.Add(ne);
                        }
                        break;

                    case 6:
                        if (flipped == 1)
                        {
                            uv.Add(se);
                            uv.Add(ne);
                            uv.Add(sw);
                        }
                        else
                        {
                            uv.Add(sw);
                            uv.Add(nw);
                            uv.Add(se);
                        }
                        break;
                }
            }

            return uv;
        }

        private void AddSkinnedVertexAndIndex(WadVertex v, SkinnedMesh mesh, Vector2 uv, int submeshIndex, int boneIndex)
        {
            SkinnedVertex newVertex = new SkinnedVertex();

            newVertex.Position = new Vector4(v.X, -v.Y, v.Z, 1);
            newVertex.Normal = Vector3.Zero;
            newVertex.Tangent = Vector3.Zero;
            newVertex.Binormal = Vector3.Zero;
            newVertex.BoneWeigths = new Vector4(1, 0, 0, 0);
            newVertex.BoneIndices = new Vector4(boneIndex, 0, 0, 0);
            newVertex.UV = uv;

            mesh.Vertices.Add(newVertex);
            mesh.SubMeshes[submeshIndex].Indices.Add((ushort)(mesh.Vertices.Count - 1));
        }

        private void AddStaticVertexAndIndex(WadVertex v, StaticMesh mesh, Vector2 uv, int submeshIndex)
        {
            StaticVertex newVertex = new StaticVertex();

            newVertex.Position = new Vector4(v.X, -v.Y, v.Z, 1);
            newVertex.Normal = Vector3.Zero;
            newVertex.Tangent = Vector3.Zero;
            newVertex.Binormal = Vector3.Zero;
            newVertex.UV = uv;

            mesh.Vertices.Add(newVertex);
            mesh.SubMeshes[submeshIndex].Indices.Add((ushort)(mesh.Vertices.Count - 1));
        }
    }
}
