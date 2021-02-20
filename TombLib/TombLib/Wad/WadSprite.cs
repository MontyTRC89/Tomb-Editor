namespace TombLib.Wad
{
    public struct WadSprite
    {
        public enum VerticalAlignment
        {
            Top,
            Center,
            Bottom
        }

        public enum HorizontalAlignment
        {
            Left,
            Center,
            Right
        }

        public WadTexture Texture { get; set; }
        public RectangleInt2 Alignment { get; set; }

        public void RecalculateAlignment(HorizontalAlignment horAdj = HorizontalAlignment.Center, VerticalAlignment verAdj = VerticalAlignment.Bottom, float scale = 5.0f)
        {
            var width = (int)(Texture.Image.Width * scale);
            var height = (int)(Texture.Image.Height * scale);

            int L, T, R, B;

            switch (horAdj)
            {
                case HorizontalAlignment.Left:
                    L = 0;
                    R = width;
                    break;
                case HorizontalAlignment.Right:
                    L = -width;
                    R = 0;
                    break;
                default:
                    L = -width / 2;
                    R = width / 2;
                    break;
            }

            switch (verAdj)
            {
                case VerticalAlignment.Top:
                    T = 0;
                    B = height;
                    break;
                case VerticalAlignment.Center:
                    T = -height / 2;
                    B = height / 2;
                    break;
                default:
                    T = -height;
                    B = 0;
                    break;
            }

            Alignment = new RectangleInt2(L, T, R, B);
        }
    }
}
