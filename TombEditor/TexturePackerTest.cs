using System.Collections.Generic;
using System.Drawing;
using System.IO;

namespace TombEditor
{
    struct PngInfo
    {
        public string Name;
        public int Width;
        public int Height;
        public int X;
        public int Y;
        public int NewWidth;
        public int NewHeight;
    }

    class TexturePackerTest
    {
        public List<string> Textures { get; set; } = new List<string>();
        public int Width { get; set; }

        public TexturePackerTest(int width)
        {
            Width = width;
        }

        public Bitmap PackTextures()
        {
            Bitmap dest = new Bitmap(256, 32768);
            PngInfo[] infos = new PngInfo[1024];
            int maxHeight = 0;
            int xcurr = 0;
            int ycurr = 0;

            for (int i = 0; i < Textures.Count; i++)
            {
                PngInfo info = new PngInfo();
                BinaryReader reader = new BinaryReader(new FileStream("Textures\\" + Textures[i], FileMode.Open, FileAccess.Read, FileShare.Read));
                reader.BaseStream.Seek(16, SeekOrigin.Begin);
                info.Name = Textures[i];
                info.Width = ReadInt32(reader);
                reader.BaseStream.Seek(20, SeekOrigin.Begin);
                info.Height = ReadInt32(reader);
                reader.Close();
                infos[i] = info;
            }
            PngInfo temp;

            for (int j = 0; j < (Textures.Count - 1); j++)
                for (int i = 0; i < (Textures.Count - 1); i++)
                    if (infos[i].Height < infos[i + 1].Height)
                    {
                        temp = infos[i];
                        infos[i] = infos[i + 1];
                        infos[i + 1] = temp;
                    }

            for (int i = 0; i < Textures.Count; i++)
            {
                PngInfo info = infos[i];
                if (info.Width > 512)
                    continue;

                if (xcurr + info.Width / 2 > 256)
                {
                    ycurr += maxHeight;
                    xcurr = 0;
                    maxHeight = 0;
                }

                info.X = xcurr;
                info.Y = ycurr;
                info.NewWidth = info.Width / 2;
                info.NewHeight = info.Height / 2;


                Bitmap bmp = (Bitmap)Image.FromFile("textures\\" + info.Name);
                for (int x = 0; x < bmp.Width; x += 2)
                {
                    for (int y = 0; y < bmp.Height; y += 2)
                    {
                        dest.SetPixel(xcurr + x / 2, ycurr + y / 2, bmp.GetPixel(x, y));
                    }
                }

                if (info.Height / 2 > maxHeight)
                    maxHeight = info.Height / 2;
                xcurr += info.Width / 2;

                infos[i] = info;

            }
            using (Graphics g = Graphics.FromImage(dest))
            {
                g.CompositingMode = System.Drawing.Drawing2D.CompositingMode.SourceCopy;

                for (int i = 0; i < Textures.Count; i++)
                {

                    g.DrawRectangle(new Pen(Color.White, 1), new Rectangle(infos[i].X, infos[i].Y, infos[i].NewWidth, infos[i].NewHeight));
                }
            }
            return dest;

        }

        private int ReadInt32(BinaryReader reader)
        {
            byte[] buffer = new byte[4];
            buffer = reader.ReadBytes(4);
            int result = buffer[3] + 256 * buffer[2];
            return result;
        }
    }
}
