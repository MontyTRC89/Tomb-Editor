using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using TombLib.IO;
using SharpDX.Toolkit.Graphics;
using SharpDX;
using TombLib.Graphics;
using TombLib.Utils;

namespace TombLib.Wad
{
    public class Wad : IDisposable
    {
        public TR4Wad OriginalWad { get; set; }

        // Data of the wad
        public Dictionary<uint, WadMoveable> WadMoveables { get; } = new Dictionary<uint, WadMoveable>();
        public Dictionary<uint, WadStatic> WadStatics { get; } = new Dictionary<uint, WadStatic>();
        public Dictionary<uint, WadTexture> TexturePages { get; } = new Dictionary<uint, WadTexture>();
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
            using (var writer = new BinaryWriterEx(new FileStream(fileName, FileMode.Create, FileAccess.Write, FileShare.None)))
            {
                // Scrivo la parola magica
                writer.Write(ASCIIEncoding.ASCII.GetBytes("WAD2"), 0, 4);

                // scrivo le texture
                short numTextures = (short)wad.TexturePages.Count;
                writer.Write(numTextures);
                for (int i = 0; i < wad.TexturePages.Count; i++)
                {
                    WadTexture page = wad.TexturePages.ElementAt(i).Value;
                    writer.Write((byte)page.Type);
                    page.Image.WriteToStreamRaw(writer.BaseStream);
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
            }
        }

        public void PrepareDataForDirectX()
        {
            Dispose();

            {
                const int atlasSize = 8;
                // Copy the page in a temp bitmap. I generate a texture atlas, putting all texture pages inside 2048x2048 pixel textures.
                var textureAtlas = Utils.ImageC.CreateNew(256 * atlasSize, 256 * atlasSize);
            
                for (int i = 0; i < TexturePages.Count; i++)
                {
                    WadTexture page = TexturePages[(uint)i];

                    int currentXblock = i % atlasSize;
                    int currentYblock = i / atlasSize;
                    textureAtlas.CopyFrom(currentXblock * 256, currentYblock * 256, page.Image);
                }
                textureAtlas.ReplaceColor(new Utils.ColorC(255, 0, 255, 255), new Utils.ColorC(0, 0, 0, 0));

                DirectXTexture = TextureLoad.Load(GraphicsDevice, textureAtlas);
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
        
        // Lets remove this methode once we use UVs internally in the Wad representation.
        // Until then this converts the deprecated format to UVs that the new texture manager in the *.tr4 export understands.
        public TextureArea GetTextureArea(ushort textureIndex, bool triangle, short attributes)
        {
            int original = textureIndex;
            bool isFlipped = (original & 0x8000) != 0;
            if (triangle)
                textureIndex = (ushort)(original & 0xfff);

            if (textureIndex > short.MaxValue)
                textureIndex = unchecked((ushort)-textureIndex);
            
            var tex = OriginalWad.Textures[textureIndex];

            // Texture page
            TextureArea result = new TextureArea();
            result.Texture = TexturePages[tex.Page];
            result.BlendMode = (attributes & 1) != 0 ? BlendMode.Additive : BlendMode.Normal;
            result.DoubleSided = false; // TODO isn't this flag also available in wads?

            // Texture UV
            Vector2 texCoord00 = new Vector2(tex.X + 0.5f, tex.Y + 0.5f);
            Vector2 texCoord10 = new Vector2(tex.X + tex.Width + 0.5f, tex.Y + 0.5f);
            Vector2 texCoord01 = new Vector2(tex.X + 0.5f, tex.Y + tex.Height + 0.5f);
            Vector2 texCoord11 = new Vector2(tex.X + tex.Width + 0.5f, tex.Y + tex.Height + 0.5f);

            if (triangle)
            {
                if (isFlipped)
                {
                    switch (original & 0x7000)
                    {
                        case 0x0000:
                            result.TexCoord0 = texCoord01;
                            result.TexCoord1 = texCoord00;
                            result.TexCoord2 = texCoord11;
                            break;

                        case 0x2000:
                            result.TexCoord0 = texCoord00;
                            result.TexCoord1 = texCoord01;
                            result.TexCoord2 = texCoord10;
                            break;

                        case 0x4000:
                            result.TexCoord0 = texCoord01;
                            result.TexCoord1 = texCoord11;
                            result.TexCoord2 = texCoord00;
                            break;
                        case 0x6000:
                            result.TexCoord0 = texCoord11;
                            result.TexCoord1 = texCoord10;
                            result.TexCoord2 = texCoord01;
                            break;
                    }
                }
                else
                {
                    switch (original & 0x7000)
                    {
                        case 0x0000:
                            result.TexCoord0 = texCoord00;
                            result.TexCoord1 = texCoord10;
                            result.TexCoord2 = texCoord01;
                            break;
                        case 0x2000:
                            result.TexCoord0 = texCoord10;
                            result.TexCoord1 = texCoord11;
                            result.TexCoord2 = texCoord00;
                            break;
                        case 0x4000:
                            result.TexCoord0 = texCoord11;
                            result.TexCoord1 = texCoord01;
                            result.TexCoord2 = texCoord10;
                            break;
                        case 0x6000:
                            result.TexCoord0 = texCoord01;
                            result.TexCoord1 = texCoord00;
                            result.TexCoord2 = texCoord11;
                            break;
                    }
                }
                result.TexCoord3 = result.TexCoord2;
            }
            else
            {
                if (isFlipped)
                {
                    result.TexCoord0 = texCoord10;
                    result.TexCoord1 = texCoord00;
                    result.TexCoord2 = texCoord01;
                    result.TexCoord3 = texCoord11;
                }
                else
                {
                    result.TexCoord0 = texCoord00;
                    result.TexCoord1 = texCoord10;
                    result.TexCoord2 = texCoord11;
                    result.TexCoord3 = texCoord01;
                }
            }

            return result;
        }
    }
}
