using System.Drawing;

namespace DarkUI.Extensions
{
    public static class ColorExtensions
    {
        public static Color Multiply(this Color color, float mulR, float mulG, float mulB, float mulA)
        {
            var newA = (int)(color.A * mulA); if (newA > 255) newA = 255; if (newA < 0) newA = 0;
            var newR = (int)(color.R * mulR); if (newR > 255) newR = 255; if (newR < 0) newR = 0;
            var newG = (int)(color.G * mulG); if (newG > 255) newG = 255; if (newG < 0) newG = 0;
            var newB = (int)(color.B * mulB); if (newB > 255) newB = 255; if (newB < 0) newB = 0;

            return Color.FromArgb(newA, newR, newG, newB);
        }

        public static Color Multiply(this Color color, float mul) => Multiply(color, mul, mul, mul, 1.0f);
        public static Color Multiply(this Color color, float mulR, float mulG, float mulB) => Multiply(color, mulR, mulG, mulB, 1.0f);
        public static Color MultiplyAlpha(this Color color, float mul) => Multiply(color, 1.0f, 1.0f, 1.0f, mul);
    }
}
