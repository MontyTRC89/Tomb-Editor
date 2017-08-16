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
using System.Drawing.Imaging;

namespace TombLib.Wad
{
    public class Wad : IDisposable
    {
        public TR4Wad OriginalWad { get; set; }

        // Data of the wad
        public Dictionary<uint, WadMoveable> WadMoveables { get; } = new Dictionary<uint, WadMoveable>();
        public Dictionary<uint, WadStatic> WadStatics { get; } = new Dictionary<uint, WadStatic>();
        public Dictionary<uint, WadTexturePage> TexturePages { get; } = new Dictionary<uint, WadTexturePage>();
        public Dictionary<uint, WadTextureSample> TextureSamples { get; } = new Dictionary<uint, WadTextureSample>();
        
        // DirectX 11 data
        public GraphicsDevice GraphicsDevice { get; set; }
        public Texture2D DirectXTexture { get; private set; }
        public Dictionary<uint, SkinnedModel> DirectXMoveables { get; } = new Dictionary<uint, SkinnedModel>();
        public Dictionary<uint, StaticModel> DirectXStatics { get; } = new Dictionary<uint, StaticModel>();

        public void Dispose()
        {
            DirectXTexture?.Dispose();
            DirectXTexture = null;

            foreach (var obj in DirectXMoveables.Values)
                obj.Dispose();
            DirectXMoveables.Clear();

            foreach (var obj in DirectXStatics.Values)
                obj.Dispose();
            DirectXStatics.Clear();
        }

        public static Wad LoadWad(string filename)
        {
            TR4Wad original = new TR4Wad();
            original.LoadWad(filename);
            return original.GetTheWad();
        }

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
                    writer.WriteBlockArray<WadVector>(mesh.Vertices);

                    writer.Write(mesh.NumNormals);
                    if (mesh.NumNormals > 0)
                        writer.WriteBlockArray<WadVector>(mesh.Normals);
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
            writer.Write((short)wad.WadStatics.Count);
            for (int i = 0; i < wad.WadStatics.Count; i++)
            {
                WadStatic staticMesh = wad.WadStatics.ElementAt(i).Value;

                writer.Write(staticMesh.ObjectID);

                WadMesh mesh = staticMesh.Mesh;

                writer.Write(mesh.SphereX);
                writer.Write(mesh.SphereY);
                writer.Write(mesh.SphereZ);
                writer.Write(mesh.Radius);
                writer.Write(mesh.Unknown);

                writer.Write(mesh.NumVertices);
                writer.WriteBlockArray<WadVector>(mesh.Vertices);

                writer.Write(mesh.NumNormals);
                if (mesh.NumNormals > 0)
                    writer.WriteBlockArray<WadVector>(mesh.Normals);
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
            Dispose();

            // Copy the page in a temp bitmap. I generate a texture atlas, putting all texture pages inside 2048x2048 pixel textures.
            using (Bitmap tempBitmap = new Bitmap(2048, 2048, System.Drawing.Imaging.PixelFormat.Format32bppArgb))
            {
                tempBitmap.MakeTransparent(System.Drawing.Color.FromArgb(255, 255, 0, 255));
                ColorMap[] colorMap = new ColorMap[1];
                colorMap[0] = new ColorMap();
                colorMap[0].OldColor = System.Drawing.Color.FromArgb(255, 255, 0, 255);
                colorMap[0].NewColor = System.Drawing.Color.Transparent;
                ImageAttributes attr = new ImageAttributes();
                attr.SetRemapTable(colorMap);

                int currentXblock = 0;
                int currentYblock = 0;
                for (uint i = 0; i < TexturePages.Count; i++)
                {
                    WadTexturePage page = TexturePages[i];

                    for (int x = 0; x < 256; x++)
                        for (int y = 0; y < 256; y++)
                        {
                            int x1 = currentXblock * 256 + x;
                            int y1 = currentYblock * 256 + y;

                            /*System.Drawing.Color c = System.Drawing.Color.FromArgb(page.TexturePage[y, x * 4 + 3],
                                                                                   page.TexturePage[y, x * 4],
                                                                                   page.TexturePage[y, x * 4 + 1],
                                                                                   page.TexturePage[y, x * 4 + 2]);*/

                            tempBitmap.SetPixel(x1, y1, System.Drawing.Color.FromArgb(255, page.TexturePage[y, x * 4],
                                                                                           page.TexturePage[y, x * 4 + 1],
                                                                                           page.TexturePage[y, x * 4 + 2]));
                        }

                    currentXblock++;
                    if (currentXblock == 8)
                    {
                        currentXblock = 0;
                        currentYblock++;
                    }
                }

                DirectXTexture = TextureLoad.LoadToTexture(GraphicsDevice, tempBitmap);
            }

            // Create movable models
            for (int i = 0; i < WadMoveables.Count; i++)
            {
                WadMoveable mov = WadMoveables.ElementAt(i).Value;
                DirectXMoveables.Add(mov.ObjectID, SkinnedModel.FromWad(GraphicsDevice, mov, TexturePages, TextureSamples));
            }

            // Create static meshes
            for (int i = 0; i < WadStatics.Count; i++)
            {
                WadStatic static_ = WadStatics.ElementAt(i).Value;
                DirectXStatics.Add(static_.ObjectID, StaticModel.FromWad(GraphicsDevice, static_, TexturePages, TextureSamples));
            }
        }
    }
}
