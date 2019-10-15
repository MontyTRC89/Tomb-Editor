using System.Drawing;
using System.Drawing.Imaging;

namespace DarkUI.Extensions
{
    public static class BitmapExtensions
    {
        internal static Bitmap SetColor(this Bitmap bitmap, Color color)
        {
            var newBitmap = new Bitmap(bitmap.Width, bitmap.Height);
            for (int i = 0; i < bitmap.Width; i++)
            {
                for (int j = 0; j < bitmap.Height; j++)
                {
                    var pixel = bitmap.GetPixel(i, j);
                    if (pixel.A > 0)
                        newBitmap.SetPixel(i, j, color);
                }
            }
            return newBitmap;
        }

        public static Bitmap ChangeColor(this Bitmap bitmap, Color oldColor, Color newColor)
        {
            var newBitmap = new Bitmap(bitmap.Width, bitmap.Height);
            for (int i = 0; i < bitmap.Width; i++)
            {
                for (int j = 0; j < bitmap.Height; j++)
                {
                    var pixel = bitmap.GetPixel(i, j);
                    if (pixel == oldColor)
                        newBitmap.SetPixel(i, j, newColor);
                }
            }
            return newBitmap;
        }

        public static Bitmap SetOpacity(this Bitmap image, float opacity)
        {
            try
            {
                //create a Bitmap the size of the image provided  
                Bitmap bmp = new Bitmap(image.Width, image.Height);

                //create a graphics object from the image  
                using (Graphics gfx = Graphics.FromImage(bmp))
                {

                    //create a color matrix object  
                    ColorMatrix matrix = new ColorMatrix();

                    //set the opacity  
                    matrix.Matrix33 = opacity;

                    //create image attributes  
                    ImageAttributes attributes = new ImageAttributes();

                    //set the color(opacity) of the image  
                    attributes.SetColorMatrix(matrix, ColorMatrixFlag.Default, ColorAdjustType.Bitmap);

                    //now draw the image  
                    gfx.DrawImage(image, new Rectangle(0, 0, bmp.Width, bmp.Height), 0, 0, image.Width, image.Height, GraphicsUnit.Pixel, attributes);
                }
                return bmp;
            }
            catch
            {
                return null;
            }
        }
        internal static Image SetOpacity(this Image image, float opacity) => ((Bitmap)image).SetOpacity(opacity);
    }
}
