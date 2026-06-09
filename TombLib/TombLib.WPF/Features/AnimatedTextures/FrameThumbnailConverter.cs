#nullable enable

using System;
using System.Globalization;
using System.Numerics;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using TombLib.LevelData;
using TombLib.Utils;

namespace TombLib.WPF.Features.AnimatedTextures
{
    /// <summary>Renders an <see cref="AnimatedTextureFrame"/> as a small perspective thumbnail for the frames grid.</summary>
    public sealed class FrameThumbnailConverter : IValueConverter
    {
        private const int ThumbnailSize = 32;

        private static readonly ImageC _checkerboard = BuildCheckerboard();

        public object? Convert(object value, Type targetType, object parameter, CultureInfo culture)
            => value is AnimatedTextureFrame frame ? RenderFrame(frame, ThumbnailSize) : null;

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotSupportedException();

        private static ImageSource? RenderFrame(AnimatedTextureFrame frame, int size)
        {
            if (frame.Texture?.Image is not { } image)
                return null;

            ImageC preview = GetPerspectivePreview(image, frame.TexCoord0, frame.TexCoord1, frame.TexCoord2, frame.TexCoord3, size, size);

            var bitmap = new WriteableBitmap(preview.Width, preview.Height, 96, 96, PixelFormats.Bgra32, null);
            byte[] bytes = preview.ToByteArray();
            bitmap.WritePixels(new Int32Rect(0, 0, preview.Width, preview.Height), bytes, preview.Width * 4, 0);
            bitmap.Freeze();
            return bitmap;
        }

        private static ImageC BuildCheckerboard()
        {
            const int size = 16;
            const int cell = 8;
            var image = ImageC.CreateNew(size, size);
            var light = new ColorC(0x60, 0x60, 0x60);
            var dark = new ColorC(0x40, 0x40, 0x40);
            for (int y = 0; y < size; y++)
                for (int x = 0; x < size; x++)
                    image.SetPixel(x, y, ((x / cell + y / cell) % 2 == 0) ? light : dark);
            return image;
        }

        private static ImageC GetPerspectivePreview(ImageC input, Vector2 texCoord01, Vector2 texCoord00, Vector2 texCoord10, Vector2 texCoord11, int width, int height)
        {
            ImageC output = ImageC.CreateNew(width, height);

            float xTexCoordFactor = 1.0f / width;
            float yTexCoordFactor = 1.0f / width;
            Vector2 max = input.Size - new Vector2(1.0f);

            for (int y = 0; y < height; ++y)
                for (int x = 0; x < width; ++x)
                {
                    float outputTexCoordX = (x + 0.5f) * xTexCoordFactor;
                    float outputTexCoordY = (y + 0.5f) * yTexCoordFactor;
                    Vector2 inputTexCoord = texCoord00 * ((1.0f - outputTexCoordX) * (1.0f - outputTexCoordY)) +
                                            texCoord01 * ((1.0f - outputTexCoordX) * outputTexCoordY) +
                                            texCoord10 * (outputTexCoordX * (1.0f - outputTexCoordY)) +
                                            texCoord11 * (outputTexCoordX * outputTexCoordY);

                    inputTexCoord -= new Vector2(0.5f);
                    inputTexCoord = Vector2.Min(Vector2.Max(inputTexCoord, new Vector2()), max);

                    int firstX = (int)inputTexCoord.X;
                    int firstY = (int)inputTexCoord.Y;
                    int secondX = Math.Min(firstX + 1, input.Width - 1);
                    int secondY = Math.Min(firstY + 1, input.Height - 1);
                    float secondFactorX = inputTexCoord.X - firstX;
                    float secondFactorY = inputTexCoord.Y - firstY;

                    Vector4 foregroundPixel =
                        (Vector4)input.GetPixel(secondX, secondY) * (secondFactorX * secondFactorY) +
                        (Vector4)input.GetPixel(secondX, firstY) * (secondFactorX * (1.0f - secondFactorY)) +
                        (Vector4)input.GetPixel(firstX, secondY) * ((1.0f - secondFactorX) * secondFactorY) +
                        (Vector4)input.GetPixel(firstX, firstY) * ((1.0f - secondFactorX) * (1.0f - secondFactorY));

                    ColorC backgroundPixel = _checkerboard.GetPixel(x % _checkerboard.Size.X, y % _checkerboard.Size.Y);
                    output.SetPixel(x, y, (ColorC)ColorC.Mix(backgroundPixel, foregroundPixel));
                }

            return output;
        }
    }
}
