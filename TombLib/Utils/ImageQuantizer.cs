using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Runtime.InteropServices;

namespace TombLib.Utils.ImageQuantizer
{
    // Modified code from https://www.codeproject.com/Articles/66341/A-Simple-Yet-Quite-Powerful-Palette-Quantizer-in-C
    // (c) Smart-K8 https://www.codeproject.com/script/Membership/View.aspx?mid=1266003
    // Distributed under CPOL 1.02 license.


    /// <summary>
    /// The utility extender class.
    /// </summary>
    public static class ImageExtensions
    {
        /// <summary>
        /// Locks the image data in a given access mode.
        /// </summary>
        /// <param name="image">The source image containing the data.</param>
        /// <param name="lockMode">The lock mode (see <see cref="ImageLockMode"/> for more details).</param>
        /// <returns>The locked image data reference.</returns>
        public static BitmapData LockBits(this Image image, ImageLockMode lockMode)
        {
            // checks whether a source image is valid
            if (image == null)
            {
                const String message = "Cannot lock the bits for a null image.";
                throw new ArgumentNullException(message);
            }

            // determines the bounds of an image, and locks the data in a specified mode
            Bitmap bitmap = (Bitmap)image;
            Rectangle bounds = Rectangle.FromLTRB(0, 0, image.Width, image.Height);
            BitmapData result = bitmap.LockBits(bounds, lockMode, image.PixelFormat);
            return result;
        }

        /// <summary>
        /// Unlocks the data for a given image.
        /// </summary>
        /// <param name="image">The image containing the data.</param>
        /// <param name="data">The data belonging to the image.</param>
        public static void UnlockBits(this Image image, BitmapData data)
        {
            // checks whether a source image is valid
            if (image == null)
            {
                const String message = "Cannot unlock the bits for a null image.";
                throw new ArgumentNullException(message);
            }

            // checks if data to be unlocked are valid
            if (data == null)
            {
                const String message = "Cannot unlock null image data.";
                throw new ArgumentNullException(message);
            }

            // releases a lock
            Bitmap bitmap = (Bitmap)image;
            bitmap.UnlockBits(data);
        }

        /// <summary>
        /// Enumerates the image pixels colors.
        /// </summary>
        /// <param name="image">The source image to be enumerated.</param>
        /// <returns>The traversable enumeration of the image colors.</returns>
        public static IEnumerable<Color> EnumerateImageColors(this Image image)
        {
            // checks whether a source image is valid
            if (image == null)
            {
                const String message = "Cannot enumerate the pixels for a null image.";
                throw new ArgumentNullException(message);
            }

            // determines whether the format is indexed
            Boolean isFormatIndexed = image.PixelFormat.IsIndexed();

            // enumerates all the image's pixel colors
            foreach (Pixel pixel in image.EnumerateImagePixels(ImageLockMode.ReadOnly))
            {
                Color color = isFormatIndexed ? image.Palette.Entries[pixel.Index] : pixel.Color;
                yield return color;
            }
        }

        /// <summary>
        /// Enumerates the image pixels.
        /// </summary>
        /// <param name="image">The source image to be enumerated.</param>
        /// <param name="accessMode">The lock mode (see <see cref="ImageLockMode"/> for more details).</param>
        /// <returns>The traversable enumeration of the image pixels.</returns>
        public static IEnumerable<Pixel> EnumerateImagePixels(this Image image, ImageLockMode accessMode)
        {
            // checks whether a source image is valid
            if (image == null)
            {
                const String message = "Cannot enumerate the pixels for a null image.";
                throw new ArgumentNullException(message);
            }

            // locks the image data
            BitmapData data = image.LockBits(accessMode);
            PixelFormat pixelFormat = image.PixelFormat;

            try
            {
                // calculates all the values necessary for enumeration
                Byte bitDepth = pixelFormat.GetBitDepth();
                Int32 bitLength = image.Width * bitDepth;
                Int32 byteLength = data.Stride < 0 ? -data.Stride : data.Stride;
                Int32 byteCount = Math.Max(1, bitDepth >> 3);

                // initializes the transfer buffers, and current pixel offset
                Byte[] buffer = new Byte[byteLength];
                Byte[] value = new Byte[byteCount];
                Int64 offset = data.Scan0.ToInt64();

                // enumerates the pixels row by row
                for (Int32 row = 0; row < image.Height; row++)
                {
                    // aquires the pointer to the first row pixel
                    IntPtr offsetPointer = new IntPtr(offset);
                    Int32 column = 0;

                    // if a read operation is possible, reads the row buffer from the image at current offset
                    if (accessMode == ImageLockMode.ReadOnly || accessMode == ImageLockMode.ReadWrite)
                    {
                        // copies the row image data to the transfer buffer 
                        Marshal.Copy(offsetPointer, buffer, 0, byteLength);
                    }

                    // enumerates the buffer per pixel
                    for (Int32 index = 0; index < bitLength; index += bitDepth)
                    {
                        // when read is allowed, retrieves current value (in bytes)
                        if (accessMode == ImageLockMode.ReadOnly || accessMode == ImageLockMode.ReadWrite)
                        {
                            Array.Copy(buffer, index >> 3, value, 0, byteCount);
                        }

                        // enumerates the pixel, and returns the control to the outside
                        Pixel pixel = new Pixel(value, column++, row, index % 8, pixelFormat);
                        yield return pixel;

                        // when write is allowed, copies the value back to the row buffer
                        if (accessMode == ImageLockMode.WriteOnly || accessMode == ImageLockMode.ReadWrite)
                        {
                            Array.Copy(value, 0, buffer, index >> 3, byteCount);
                        }
                    }

                    // if a write operation is possible, writes the row buffer back to current offset
                    if (accessMode == ImageLockMode.WriteOnly || accessMode == ImageLockMode.ReadWrite)
                    {
                        // copies the row image data from the buffer back to the image
                        Marshal.Copy(buffer, 0, offsetPointer, byteLength);
                    }

                    // increases offset by a row
                    offset += data.Stride;
                }
            }
            finally
            {
                // releases the lock on the image data
                image.UnlockBits(data);
            }
        }

        /// <summary>
        /// Adds all the colors from a source image to a given color quantizer.
        /// </summary>
        /// <param name="image">The image to be processed.</param>
        /// <param name="quantizer">The target color quantizer.</param>
        public static void AddColorsToQuantizer(this Image image, IColorQuantizer quantizer)
        {
            // checks whether a source image is valid
            if (image == null)
            {
                const String message = "Cannot add colors from a null image.";
                throw new ArgumentNullException(message);
            }

            // checks whether the quantizer is valid
            if (quantizer == null)
            {
                const String message = "Cannot add colors to a null quantizer.";
                throw new ArgumentNullException(message);
            }

            // determines which method of color retrieval to use
            Boolean isImageIndexed = image.PixelFormat.IsIndexed();

            // retrieves all the colors from image into a given quantizer
            foreach (Pixel pixel in image.EnumerateImagePixels(ImageLockMode.ReadOnly))
            {
                // determines a pixel color
                Color color = isImageIndexed ? image.Palette.Entries[pixel.Index] : pixel.Color;

                // adds the color to the quantizer
                quantizer.AddColor(color);
            }
        }

        /// <summary>
        /// Changes the pixel format.
        /// </summary>
        /// <param name="image">The image.</param>
        /// <param name="targetFormat">The target format.</param>
        /// <param name="quantizer">The color quantizer.</param>
        /// <returns>The converted image in a target format.</returns>
        public static Image ChangePixelFormat(this Image image, PixelFormat targetFormat, IColorQuantizer quantizer)
        {
            // checks for image validity
            if (image == null)
            {
                const String message = "Cannot change a pixel format for a null image.";
                throw new ArgumentNullException(message);
            }

            // checks whether a target format is supported
            if (!targetFormat.IsSupported())
            {
                String message = string.Format("A pixel format '{0}' is not supported.", targetFormat);
                throw new NotSupportedException(message);
            }

            // checks whether there is a quantizer for a indexed format
            if (targetFormat.IsIndexed() && quantizer == null)
            {
                String message = string.Format("A quantizer is cannot be null for indexed pixel format '{0}'.", targetFormat);
                throw new NotSupportedException(message);
            }

            // creates an image with the target format
            Bitmap result = new Bitmap(image.Width, image.Height, targetFormat);

            // gathers some information about the target format
            Boolean hasSourceAlpha = image.PixelFormat.HasAlpha();
            Boolean hasTargetAlpha = targetFormat.HasAlpha();
            Boolean isSourceIndexed = image.PixelFormat.IsIndexed();
            Boolean isTargetIndexed = targetFormat.IsIndexed();
            Boolean isSourceDeepColor = image.PixelFormat.IsDeepColor();
            Boolean isTargetDeepColor = targetFormat.IsDeepColor();

            // if palette is needed create one first
            if (isTargetIndexed)
            {
                Int32 targetColorCount = result.GetPaletteColorCount();
                List<Color> palette = quantizer.GetPalette(targetColorCount);
                result.SetPalette(palette);
            }

            // initializes both source and target image enumerators
            IEnumerable<Pixel> sourceEnum = image.EnumerateImagePixels(ImageLockMode.ReadOnly);
            IEnumerable<Pixel> targetEnum = result.EnumerateImagePixels(ImageLockMode.WriteOnly);

            // ensures that both enumerators are released from memory afterwards
            using (IEnumerator<Pixel> source = sourceEnum.GetEnumerator())
            using (IEnumerator<Pixel> target = targetEnum.GetEnumerator())
            {
                Boolean isSourceAvailable = source.MoveNext();
                Boolean isTargetAvailable = target.MoveNext();

                // moves to next pixel for both images
                // while (source.MoveNext() || target.MoveNext())
                while (isSourceAvailable || isTargetAvailable)
                {
                    Color color;


                    // if both source and target formats are deep color formats, copies a value directly
                    if (isSourceDeepColor && isTargetDeepColor)
                    {
                        UInt64 value = source.Current.Value;
                        target.Current.SetValue(value);
                    }
                    else
                    {
                        // retrieves a source image color
                        if (isSourceIndexed)
                        {
                            // for the indexed images, retrieves a color from their palette
                            color = image.Palette.Entries[source.Current.Index];
                        }
                        else
                        {
                            // for the non-indexed image, retrieves a color directly
                            color = source.Current.Color;
                        }

                        // if alpha is not present in the source image, but is present in the target, make one up
                        if (!hasSourceAlpha && hasTargetAlpha)
                        {
                            color = Color.FromArgb(255, color.R, color.G, color.B);
                        }

                        // sets the color to a target pixel
                        if (isTargetIndexed)
                        {
                            // for the indexed images, determines a color from the octree
                            Byte paletteIndex = (Byte)quantizer.GetPaletteIndex(color);
                            target.Current.SetIndex(paletteIndex);
                        }
                        else
                        {
                            // for the non-indexed images, sets the color directly
                            target.Current.SetColor(color);
                        }
                    }

                    isSourceAvailable = source.MoveNext();
                    isTargetAvailable = target.MoveNext();
                }
            }

            // returns the image in the target format
            return result;
        }

        public static Int32 GetPaletteColorCount(this Image image)
        {
            // checks whether a source image is valid
            if (image == null)
            {
                const String message = "Cannot assign a palette to a null image.";
                throw new ArgumentNullException(message);
            }

            // checks if the image has an indexed format
            if (!image.PixelFormat.IsIndexed())
            {
                String message = string.Format("Cannot retrieve a color count from a non-indexed image with pixel format '{0}'.", image.PixelFormat);
                throw new InvalidOperationException(message);
            }

            // returns the color count
            return image.Palette.Entries.Length;
        }

        /// <summary>
        /// Gets the palette of an indexed image.
        /// </summary>
        /// <param name="image">The source image.</param>
        public static List<Color> GetPalette(this Image image)
        {
            // checks whether a source image is valid
            if (image == null)
            {
                const String message = "Cannot assign a palette to a null image.";
                throw new ArgumentNullException(message);
            }

            // checks if the image has an indexed format
            if (!image.PixelFormat.IsIndexed())
            {
                String message = string.Format("Cannot retrieve a palette from a non-indexed image with pixel format '{0}'.", image.PixelFormat);
                throw new InvalidOperationException(message);
            }

            // retrieves and returns the palette
            return image.Palette.Entries.ToList();
        }

        /// <summary>
        /// Sets the palette of an indexed image.
        /// </summary>
        /// <param name="image">The target image.</param>
        /// <param name="palette">The palette.</param>
        public static void SetPalette(this Image image, List<Color> palette)
        {
            // checks whether a palette is valid
            if (palette == null)
            {
                const String message = "Cannot assign a null palette.";
                throw new ArgumentNullException(message);
            }

            // checks whether a target image is valid
            if (image == null)
            {
                const String message = "Cannot assign a palette to a null image.";
                throw new ArgumentNullException(message);
            }

            // checks if the image has indexed format
            if (!image.PixelFormat.IsIndexed())
            {
                String message = string.Format("Cannot store a palette to a non-indexed image with pixel format '{0}'.", image.PixelFormat);
                throw new InvalidOperationException(message);
            }

            // checks if the palette can fit into the image palette
            if (palette.Count > image.Palette.Entries.Length)
            {
                String message = string.Format("Cannot store a palette with '{0}' colors intto an image palette where only '{1}' colors are allowed.", palette.Count, image.Palette.Entries.Length);
                throw new ArgumentOutOfRangeException(message);
            }

            // retrieves a target image palette
            ColorPalette imagePalette = image.Palette;

            // copies all color entries
            for (Int32 index = 0; index < palette.Count; index++)
            {
                imagePalette.Entries[index] = palette[index];
            }

            // assigns the palette to the target image
            image.Palette = imagePalette;
        }

        /// <summary>
        /// Gets the bit count for a given pixel format.
        /// </summary>
        /// <param name="pixelFormat">The pixel format.</param>
        /// <returns>The bit count.</returns>
        public static Byte GetBitDepth(this PixelFormat pixelFormat)
        {
            switch (pixelFormat)
            {
                case PixelFormat.Format1bppIndexed:
                    return 1;

                case PixelFormat.Format4bppIndexed:
                    return 4;

                case PixelFormat.Format8bppIndexed:
                    return 8;

                case PixelFormat.Format16bppArgb1555:
                case PixelFormat.Format16bppGrayScale:
                case PixelFormat.Format16bppRgb555:
                case PixelFormat.Format16bppRgb565:
                    return 16;

                case PixelFormat.Format24bppRgb:
                    return 24;

                case PixelFormat.Format32bppArgb:
                case PixelFormat.Format32bppPArgb:
                case PixelFormat.Format32bppRgb:
                    return 32;

                case PixelFormat.Format48bppRgb:
                    return 48;

                case PixelFormat.Format64bppArgb:
                case PixelFormat.Format64bppPArgb:
                    return 64;

                default:
                    String message = string.Format("A pixel format '{0}' not supported!", pixelFormat);
                    throw new NotSupportedException(message);
            }
        }

        /// <summary>
        /// Gets the available color count for a given pixel format.
        /// </summary>
        /// <param name="pixelFormat">The pixel format.</param>
        /// <returns>The available color count.</returns>
        public static UInt16 GetColorCount(this PixelFormat pixelFormat)
        {
            // checks whether a pixel format is indexed, otherwise throw an exception
            if (!pixelFormat.IsIndexed())
            {
                String message = string.Format("Cannot retrieve color count for a non-indexed format '{0}'.", pixelFormat);
                throw new NotSupportedException(message);
            }

            switch (pixelFormat)
            {
                case PixelFormat.Format1bppIndexed:
                    return 2;

                case PixelFormat.Format4bppIndexed:
                    return 16;

                case PixelFormat.Format8bppIndexed:
                    return 256;

                default:
                    String message = string.Format("A pixel format '{0}' not supported!", pixelFormat);
                    throw new NotSupportedException(message);
            }
        }

        /// <summary>
        /// Gets the friendly name of the pixel format.
        /// </summary>
        /// <param name="pixelFormat">The pixel format.</param>
        /// <returns></returns>
        public static String GetFriendlyName(this PixelFormat pixelFormat)
        {
            switch (pixelFormat)
            {
                case PixelFormat.Format1bppIndexed:
                    return "Indexed (2 colors)";

                case PixelFormat.Format4bppIndexed:
                    return "Indexed (16 colors)";

                case PixelFormat.Format8bppIndexed:
                    return "Indexed (256 colors)";

                case PixelFormat.Format16bppGrayScale:
                    return "Grayscale (65536 shades)";

                case PixelFormat.Format16bppArgb1555:
                    return "Highcolor + Alpha mask (32768 colors)";

                case PixelFormat.Format16bppRgb555:
                case PixelFormat.Format16bppRgb565:
                    return "Highcolor (65536 colors)";

                case PixelFormat.Format24bppRgb:
                    return "Truecolor (24-bit)";

                case PixelFormat.Format32bppArgb:
                case PixelFormat.Format32bppPArgb:
                    return "Truecolor + Alpha (32-bit)";

                case PixelFormat.Format32bppRgb:
                    return "Truecolor (32-bit)";

                case PixelFormat.Format48bppRgb:
                    return "Truecolor (48-bit)";

                case PixelFormat.Format64bppArgb:
                case PixelFormat.Format64bppPArgb:
                    return "Truecolor + Alpha (64-bit)";

                default:
                    String message = string.Format("A pixel format '{0}' not supported!", pixelFormat);
                    throw new NotSupportedException(message);
            }
        }

        /// <summary>
        /// Determines whether the specified pixel format is indexed.
        /// </summary>
        /// <param name="pixelFormat">The pixel format.</param>
        /// <returns>
        /// 	<c>true</c> if the specified pixel format is indexed; otherwise, <c>false</c>.
        /// </returns>
        public static Boolean IsIndexed(this PixelFormat pixelFormat)
        {
            return (pixelFormat & PixelFormat.Indexed) == PixelFormat.Indexed;
        }

        /// <summary>
        /// Determines whether the specified pixel format is supported.
        /// </summary>
        /// <param name="pixelFormat">The pixel format.</param>
        /// <returns>
        /// 	<c>true</c> if the specified pixel format is supported; otherwise, <c>false</c>.
        /// </returns>
        public static Boolean IsSupported(this PixelFormat pixelFormat)
        {
            switch (pixelFormat)
            {
                case PixelFormat.Format1bppIndexed:
                case PixelFormat.Format4bppIndexed:
                case PixelFormat.Format8bppIndexed:
                case PixelFormat.Format16bppArgb1555:
                case PixelFormat.Format16bppRgb555:
                case PixelFormat.Format16bppRgb565:
                case PixelFormat.Format24bppRgb:
                case PixelFormat.Format32bppArgb:
                case PixelFormat.Format32bppPArgb:
                case PixelFormat.Format32bppRgb:
                case PixelFormat.Format48bppRgb:
                case PixelFormat.Format64bppArgb:
                case PixelFormat.Format64bppPArgb:
                    return true;

                default:
                    return false;
            }
        }

        /// <summary>
        /// Determines whether the specified pixel format has an alpha channel.
        /// </summary>
        /// <param name="pixelFormat">The pixel format.</param>
        /// <returns>
        /// 	<c>true</c> if the specified pixel format has an alpha channel; otherwise, <c>false</c>.
        /// </returns>
        public static Boolean HasAlpha(this PixelFormat pixelFormat)
        {
            return (pixelFormat & PixelFormat.Alpha) == PixelFormat.Alpha ||
                   (pixelFormat & PixelFormat.PAlpha) == PixelFormat.PAlpha;
        }

        /// <summary>
        /// Determines whether [is deep color] [the specified pixel format].
        /// </summary>
        /// <param name="pixelFormat">The pixel format.</param>
        /// <returns>
        /// 	<c>true</c> if [is deep color] [the specified pixel format]; otherwise, <c>false</c>.
        /// </returns>
        public static Boolean IsDeepColor(this PixelFormat pixelFormat)
        {
            switch (pixelFormat)
            {
                case PixelFormat.Format16bppGrayScale:
                case PixelFormat.Format48bppRgb:
                case PixelFormat.Format64bppArgb:
                case PixelFormat.Format64bppPArgb:
                    return true;

                default:
                    return false;
            }
        }
    }


    /// <summary>
    /// This is a pixel format independent pixel.
    /// </summary>
    public struct Pixel
    {
        private static readonly float[,] Rgb2Xyz =
        {
            { 0.41239083F, 0.35758433F, 0.18048081F },
            { 0.21263903F, 0.71516865F, 0.072192319F },
            { 0.019330820F, 0.11919473F, 0.95053220F }
        };

        private static readonly float[,] Xyz2Rgb =
        {
            { 3.2409699F, -1.5373832F, -0.49861079F },
            { -0.96924376F, 1.8759676F, 0.041555084F },
            { 0.055630036F, -0.20397687F, 1.0569715F }
        };


        #region | Properties |

        private Byte[] Data { get; set; }
        private Int32 BitOffset { get; set; }
        private Int32 BitDepth { get; set; }
        private BitArray Bits { get; set; }
        private PixelFormat Format { get; set; }

        public Int32 X { get; private set; }
        public Int32 Y { get; private set; }

        #endregion

        #region | Calculated properties |

        /// <summary>
        /// Gets the value.
        /// </summary>
        /// <value>The value.</value>
        public UInt64 Value
        {
            get { return GetValue(); }
            set { SetValue(value); }
        }

        /// <summary>
        /// Gets or sets the index.
        /// </summary>
        /// <value>The index.</value>
        public Byte Index
        {
            get { return GetIndex(); }
            set { SetIndex(value); }
        }

        /// <summary>
        /// Gets or sets the color.
        /// </summary>
        /// <value>The color.</value>
        public Color Color
        {
            get
            { return GetColor(); }
            set { SetColor(value); }
        }

        #endregion

        #region | Bit methods |

        private TType GetBit<TType>(Byte offset, TType value)
        {
            return Bits[offset] ? value : default(TType);
        }

        private Int32 GetBitRange(Byte startOffset, Byte endOffset)
        {
            Int32 result = 0;
            Byte index = 0;

            for (Byte offset = startOffset; offset <= endOffset; offset++)
            {
                result += GetBit(offset, 1 << index);
                index++;
            }

            return result;
        }

        private void SetBitRange(Byte startOffset, Byte endOffset, Int32 value)
        {
            Byte index = 0;

            for (Byte offset = startOffset; offset <= endOffset; offset++)
            {
                Int32 bitValue = 1 << index;
                Bits[offset] = (value & bitValue) == bitValue;
                index++;
            }
        }

        #endregion

        #region | Value methods |

        /// <summary>
        /// Gets the value.
        /// </summary>
        /// <returns></returns>
        public UInt64 GetValue()
        {
            UInt64 result;

            switch (BitDepth)
            {
                case 1:
                    result = GetValueAsBit();
                    break;

                case 2:
                    result = GetValueAsTwoBits();
                    break;

                case 4:
                    result = GetValueAsFourBits();
                    break;

                case 8:
                    result = GetValueAsByte();
                    break;

                case 16:
                    result = GetValueAsTwoBytes();
                    break;

                case 24:
                    result = GetValueAsThreeBytes();
                    break;

                case 32:
                    result = GetValueAsFourBytes();
                    break;

                case 48:
                    result = GetValueAsSixBytes();
                    break;

                case 64:
                    result = GetValueAsEightBytes();
                    break;

                default:
                    String message = string.Format("A bit depth of '{0}' is not supported", BitDepth);
                    throw new NotSupportedException(message);
            }

            return result;
        }

        /// <summary>
        /// Gets the value.
        /// </summary>
        /// <returns></returns>
        public void SetValue(UInt64 value)
        {
            switch (BitDepth)
            {
                case 1:
                    SetValueAsBit((Byte)value);
                    break;

                case 2:
                    SetValueAsTwoBits((Byte)value);
                    break;

                case 4:
                    SetValueAsFourBits((Byte)value);
                    break;

                case 8:
                    SetValueAsByte((Byte)value);
                    break;

                case 16:
                    SetValueAsTwoBytes((UInt16)value);
                    break;

                case 24:
                    SetValueAsThreeBytes((UInt32)value);
                    break;

                case 32:
                    SetValueAsFourBytes((UInt32)value);
                    break;

                case 48:
                    SetValueAsSixBytes(value);
                    break;

                case 64:
                    SetValueAsEightBytes(value);
                    break;

                default:
                    String message = string.Format("A bit depth of '{0}' is not supported", BitDepth);
                    throw new NotSupportedException(message);
            }
        }

        #endregion

        #region | Index methods |

        /// <summary>
        /// Gets the index.
        /// </summary>
        /// <returns></returns>
        public Byte GetIndex()
        {
            if (!Format.IsIndexed())
            {
                String message = string.Format("Cannot retrieve index for a non-indexed format '{0}'. Please use Color (or Value) property instead.", Format);
                throw new NotSupportedException(message);
            }

            Byte result;

            switch (Format)
            {
                case PixelFormat.Format1bppIndexed:
                    result = GetValueAsBit();
                    break;

                case PixelFormat.Format4bppIndexed:
                    result = GetValueAsFourBits();
                    break;

                case PixelFormat.Format8bppIndexed:
                    result = GetValueAsByte();
                    break;

                default:
                    String message = string.Format("This pixel format '{0}' is not supported.", Format);
                    throw new NotSupportedException(message);
            }

            return result;
        }

        /// <summary>
        /// Sets the index.
        /// </summary>
        /// <param name="index">The index.</param>
        public void SetIndex(Byte index)
        {
            if (!Format.IsIndexed())
            {
                String message = string.Format("Cannot set index for a non-indexed format '{0}'. Please use Color (or Value) property instead.", Format);
                throw new NotSupportedException(message);
            }

            switch (Format)
            {
                case PixelFormat.Format1bppIndexed:
                    SetValueAsBit(index);
                    break;

                case PixelFormat.Format4bppIndexed:
                    SetValueAsFourBits(index);
                    break;

                case PixelFormat.Format8bppIndexed:
                    SetValueAsByte(index);
                    break;

                default:
                    String message = string.Format("This pixel format '{0}' is not supported.", Format);
                    throw new NotSupportedException(message);
            }
        }

        #endregion

        #region | Color methods |

        /// <summary>
        /// Gets the color.
        /// </summary>
        /// <returns></returns>
        public Color GetColor()
        {
            if (Format.IsIndexed())
            {
                String message = string.Format("Cannot retrieve color for an indexed format '{0}'. Please use Index (or Value) property instead.", Format);
                throw new NotSupportedException(message);
            }

            Int32 alpha, red, green, blue;

            switch (Format)
            {
                case PixelFormat.Format16bppArgb1555:
                    alpha = GetBit(15, 255);
                    red = GetBitRange(10, 14);
                    green = GetBitRange(5, 9);
                    blue = GetBitRange(0, 4);
                    break;

                case PixelFormat.Format16bppGrayScale:
                    alpha = 255;
                    red = green = blue = GetBitRange(0, 15);
                    break;

                case PixelFormat.Format16bppRgb555:
                    alpha = 255;
                    red = GetBitRange(10, 14);
                    green = GetBitRange(5, 9);
                    blue = GetBitRange(0, 4);
                    break;

                case PixelFormat.Format16bppRgb565:
                    alpha = 255;
                    red = GetBitRange(11, 15);
                    green = GetBitRange(5, 10);
                    blue = GetBitRange(0, 4);
                    break;

                case PixelFormat.Format24bppRgb:
                    alpha = 255;
                    red = Data[2];
                    green = Data[1];
                    blue = Data[0];
                    break;

                case PixelFormat.Format32bppArgb:
                case PixelFormat.Format32bppPArgb:
                    alpha = Data[3];
                    red = Data[2];
                    green = Data[1];
                    blue = Data[0];
                    break;

                case PixelFormat.Format32bppRgb:
                    alpha = 255;
                    red = Data[2];
                    green = Data[1];
                    blue = Data[0];
                    break;

                case PixelFormat.Format48bppRgb:
                    alpha = 255;

                    red = Data[4] + (Data[5] << 8);
                    green = Data[2] + (Data[3] << 8);
                    blue = Data[0] + (Data[1] << 8);

                    DoCalculate(ref red, ref green, ref blue);
                    break;

                case PixelFormat.Format64bppArgb:
                case PixelFormat.Format64bppPArgb:
                    alpha = (Data[6] + (Data[7] << 8)) >> 5;
                    red = (Data[4] + (Data[5] << 8)) >> 5;
                    green = (Data[2] + (Data[3] << 8)) >> 5;
                    blue = (Data[0] + (Data[1] << 8)) >> 5;
                    break;

                default:
                    String message = string.Format("This pixel format '{0}' is not supported.", Format);
                    throw new NotSupportedException(message);
            }

            Color result = Color.FromArgb(alpha, red, green, blue);
            return result;
        }

        private static void DoCalculate(ref Int32 red, ref Int32 green, ref Int32 blue)
        {
            Single redfloatValue = red / 8192.0f;
            Single greenfloatValue = green / 8192.0f;
            Single bluefloatValue = blue / 8192.0f;

            Single[] result = new Single[3];

            for (Int32 index = 0; index < 3; index++)
            {
                result[index] += Rgb2Xyz[index, 0] * redfloatValue;
                result[index] += Rgb2Xyz[index, 1] * greenfloatValue;
                result[index] += Rgb2Xyz[index, 2] * bluefloatValue;
            }

            Single x = result[0] + result[1] + result[2];
            Single y = result[1];

            if (x > 0)
            {
                redfloatValue = y;
                greenfloatValue = result[0] / x;
                bluefloatValue = result[1] / x;
            }
            else
            {
                redfloatValue = 0.0f;
                greenfloatValue = 0.0f;
                bluefloatValue = 0.0f;
            }

            Single bias = (Single)(Math.Log(0.85f) / -0.693147f);
            Single exposure = 1.0f;
            Single lumAvg = redfloatValue;
            Single lumMax = redfloatValue;
            Single lumNormal = lumMax / lumAvg;
            Single divider = (Single)Math.Log10(lumNormal + 1.0);

            Double yw = (redfloatValue / lumAvg) * exposure;
            Double interpol = Math.Log(2 + Math.Pow(yw / lumNormal, bias) * 8);
            Double l = PadeLog(yw);
            redfloatValue = (Single)((l / interpol) / divider);

            Single z;
            y = redfloatValue;
            Array.Clear(result, 0, 3);

            result[1] = greenfloatValue;
            result[2] = bluefloatValue;

            if ((y > 1e-06f) && (result[1] > 1e-06F) && (result[2] > 1e-06F))
            {
                x = (result[1] * y) / result[2];
                z = (x / result[1]) - x - y;
            }
            else
            {
                x = z = 1e-06F;
            }

            redfloatValue = x;
            greenfloatValue = y;
            bluefloatValue = z;

            Array.Clear(result, 0, 3);

            for (int i = 0; i < 3; i++)
            {
                result[i] += Xyz2Rgb[i, 0] * redfloatValue;
                result[i] += Xyz2Rgb[i, 1] * greenfloatValue;
                result[i] += Xyz2Rgb[i, 2] * bluefloatValue;
            }

            redfloatValue = result[0] > 1 ? 1 : result[0];
            greenfloatValue = result[1] > 1 ? 1 : result[1];
            bluefloatValue = result[2] > 1 ? 1 : result[2];

            red = (Byte)(255 * redfloatValue + 0.5);
            green = (Byte)(255 * greenfloatValue + 0.5);
            blue = (Byte)(255 * bluefloatValue + 0.5);
        }

        private static Double PadeLog(Double value)
        {
            if (value < 1)
            {
                return (value * (6 + value) / (6 + 4 * value));
            }

            if (value < 2)
            {
                return (value * (6 + 0.7662 * value) / (5.9897 + 3.7658 * value));
            }

            return Math.Log(value + 1);
        }

        /// <summary>
        /// Sets the color.
        /// </summary>
        /// <param name="value">The value.</param>
        public void SetColor(Color value)
        {
            if (Format.IsIndexed())
            {
                String message = string.Format("Cannot set color for an indexed format '{0}'. Please use Index (or Value) property instead.", Format);
                throw new NotSupportedException(message);
            }

            Int32 alpha = value.A;
            Int32 red = value.R;
            Int32 green = value.G;
            Int32 blue = value.B;

            switch (Format)
            {
                case PixelFormat.Format16bppArgb1555:
                    Bits[15] = alpha > 0;
                    SetBitRange(10, 14, red >> 3);
                    SetBitRange(5, 9, green >> 3);
                    SetBitRange(0, 4, blue >> 3);
                    Bits.CopyTo(Data, 0);
                    break;

                case PixelFormat.Format16bppGrayScale:
                    SetBitRange(0, 15, red << 8 + red);
                    Bits.CopyTo(Data, 0);
                    break;

                case PixelFormat.Format16bppRgb555:
                    SetBitRange(10, 14, red >> 3);
                    SetBitRange(5, 9, green >> 3);
                    SetBitRange(0, 4, blue >> 3);
                    Bits.CopyTo(Data, 0);
                    break;

                case PixelFormat.Format16bppRgb565:
                    SetBitRange(11, 15, red >> 3);
                    SetBitRange(5, 10, green >> 2);
                    SetBitRange(0, 4, blue >> 3);
                    Bits.CopyTo(Data, 0);
                    break;

                case PixelFormat.Format24bppRgb:
                    Data[2] = (Byte)red;
                    Data[1] = (Byte)green;
                    Data[0] = (Byte)blue;
                    break;

                case PixelFormat.Format32bppArgb:
                case PixelFormat.Format32bppPArgb:
                    Data[3] = (Byte)alpha;
                    Data[2] = (Byte)red;
                    Data[1] = (Byte)green;
                    Data[0] = (Byte)blue;
                    break;

                case PixelFormat.Format32bppRgb:
                    Data[3] = 0;
                    Data[2] = (Byte)red;
                    Data[1] = (Byte)green;
                    Data[0] = (Byte)blue;
                    break;

                case PixelFormat.Format48bppRgb:
                    Data[5] = (Byte)(red >> 3);
                    Data[4] = (Byte)((red << 5) % 256);
                    Data[3] = (Byte)(green >> 3);
                    Data[2] = (Byte)((green << 5) % 256);
                    Data[1] = (Byte)(blue >> 3);
                    Data[0] = (Byte)((blue << 5) % 256);
                    break;

                case PixelFormat.Format64bppArgb:
                case PixelFormat.Format64bppPArgb:
                    Data[7] = (Byte)(alpha >> 3);
                    Data[6] = (Byte)((alpha << 5) % 256);
                    Data[5] = (Byte)(red >> 3);
                    Data[4] = (Byte)((red << 5) % 256);
                    Data[3] = (Byte)(green >> 3);
                    Data[2] = (Byte)((green << 5) % 256);
                    Data[1] = (Byte)(blue >> 3);
                    Data[0] = (Byte)((blue << 5) % 256);
                    break;

                default:
                    String message = string.Format("This pixel format '{0}' is not supported.", Format);
                    throw new NotSupportedException(message);
            }
        }

        #endregion

        #region | Helper get methods |

        private Byte GetValueAsBit()
        {
            return (Byte)(Bits[BitOffset] ? 1 : 0);
        }

        private Byte GetValueAsTwoBits()
        {
            Byte lowBit = (Byte)(Bits[BitOffset] ? 1 : 0);
            Byte highBit = (Byte)(Bits[BitOffset + 1] ? 2 : 0);
            return (Byte)(lowBit + highBit);
        }

        private Byte GetValueAsFourBits()
        {
            Byte firstBit = (Byte)(Bits[BitOffset] ? 1 : 0);
            Byte secondBit = (Byte)(Bits[BitOffset + 1] ? 2 : 0);
            Byte thirdBit = (Byte)(Bits[BitOffset + 2] ? 4 : 0);
            Byte fourthBit = (Byte)(Bits[BitOffset + 3] ? 8 : 0);
            return (Byte)(firstBit + secondBit + thirdBit + fourthBit);
        }

        private Byte GetValueAsByte()
        {
            return Data[0];
        }

        private UInt16 GetValueAsTwoBytes()
        {
            UInt16 result = Data[0];
            result += Convert.ToUInt16(Data[1] << 8);
            return result;
        }

        private UInt32 GetValueAsThreeBytes()
        {
            UInt32 result = Data[0];
            result += (UInt32)Data[1] << 8;
            result += (UInt32)Data[2] << 16;
            return result;
        }

        private UInt32 GetValueAsFourBytes()
        {
            UInt32 result = Data[0];
            result += (UInt32)Data[1] << 8;
            result += (UInt32)Data[2] << 16;
            result += (UInt32)Data[3] << 24;
            return result;
        }

        private UInt64 GetValueAsSixBytes()
        {
            UInt64 result = Data[0];
            result += (UInt64)Data[1] << 8;
            result += (UInt64)Data[2] << 16;
            result += (UInt64)Data[3] << 24;
            result += (UInt64)Data[4] << 32;
            result += (UInt64)Data[5] << 40;
            result += (UInt64)255 << 48;
            result += (UInt64)31 << 56;
            return result;
        }

        private UInt64 GetValueAsEightBytes()
        {
            UInt64 result = Data[0];
            result += (UInt64)Data[1] << 8;
            result += (UInt64)Data[2] << 16;
            result += (UInt64)Data[3] << 24;
            result += (UInt64)Data[4] << 32;
            result += (UInt64)Data[5] << 40;
            result += (UInt64)Data[6] << 48;
            result += (UInt64)Data[7] << 56;
            return result;
        }

        #endregion

        #region | Helper set methods |

        private void SetValueAsBit(Byte value)
        {
            Bits[7 - BitOffset] = value > 0;
            Bits.CopyTo(Data, 0);
        }

        private void SetValueAsTwoBits(Byte value)
        {
            SetBitRange((Byte)BitOffset, (Byte)(BitOffset + 1), value);
            Bits.CopyTo(Data, 0);
        }

        private void SetValueAsFourBits(Byte value)
        {
            SetBitRange((Byte)(8 - BitOffset - BitDepth), (Byte)(7 - BitOffset), value);
            Bits.CopyTo(Data, 0);
        }

        private void SetValueAsByte(Byte value)
        {
            Data[0] = value;
        }

        private void SetValueAsTwoBytes(UInt16 value)
        {
            Data[0] = (Byte)(value % 256);
            Data[1] = (Byte)(value >> 8);
        }

        private void SetValueAsThreeBytes(UInt32 value)
        {
            UInt32 skipped = value >> 24;
            if (skipped > 0) value -= skipped;
            Data[2] = (Byte)(value >> 16);
            value -= (UInt32)Data[2] << 16;
            Data[1] = (Byte)(value >> 8);
            value -= (UInt32)Data[1] << 8;
            Data[0] = (Byte)value;
        }

        private void SetValueAsFourBytes(UInt32 value)
        {
            Data[3] = (Byte)(value >> 24);
            value -= (UInt32)Data[3] >> 24;
            Data[2] = (Byte)(value >> 16);
            value -= (UInt32)Data[2] << 16;
            Data[1] = (Byte)(value >> 8);
            value -= (UInt32)Data[1] << 8;
            Data[0] = (Byte)value;
        }

        private void SetValueAsSixBytes(UInt64 value)
        {
            Data[5] = (Byte)(value >> 40);
            value -= (UInt64)Data[5] << 40;
            Data[4] = (Byte)(value >> 32);
            value -= (UInt64)Data[4] << 32;
            Data[3] = (Byte)(value >> 24);
            value -= (UInt64)Data[3] << 24;
            Data[2] = (Byte)(value >> 16);
            value -= (UInt64)Data[2] << 16;
            Data[1] = (Byte)(value >> 8);
            value -= (UInt64)Data[1] << 8;
            Data[0] = (Byte)value;
        }

        private void SetValueAsEightBytes(UInt64 value)
        {
            Data[7] = (Byte)(value >> 56);
            value -= (UInt64)Data[7] << 56;
            Data[6] = (Byte)(value >> 48);
            value -= (UInt64)Data[6] << 48;
            Data[5] = (Byte)(value >> 40);
            value -= (UInt64)Data[5] << 40;
            Data[4] = (Byte)(value >> 32);
            value -= (UInt64)Data[4] << 32;
            Data[3] = (Byte)(value >> 24);
            value -= (UInt64)Data[3] << 24;
            Data[2] = (Byte)(value >> 16);
            value -= (UInt64)Data[2] << 16;
            Data[1] = (Byte)(value >> 8);
            value -= (UInt64)Data[1] << 8;
            Data[0] = (Byte)value;
        }

        #endregion

        #region | Constructors |

        /// <summary>
        /// Initializes a new instance of the <see cref="Pixel"/> struct.
        /// </summary>
        /// <param name="data">The data.</param>
        /// <param name="x">The X coordinate.</param>
        /// <param name="y">The Y coordinate.</param>
        /// <param name="bitOffset">The bit offset.</param>
        /// <param name="pixelFormat">The pixel format.</param>
        public Pixel(Byte[] data, Int32 x, Int32 y, Int32 bitOffset, PixelFormat pixelFormat) : this()
        {
            X = x;
            Y = y;
            Data = data;
            BitOffset = bitOffset;
            BitDepth = pixelFormat.GetBitDepth();
            Format = pixelFormat;
            Bits = new BitArray(data);
        }

        #endregion
    }

    /// <summary>
    /// Stores all the informations about single color only once, to be used later.
    /// </summary>
    internal struct ColorInfo
    {
        /// <summary>
        /// The original color.
        /// </summary>
        public Color Color { get; private set; }

        /// <summary>
        /// The pixel presence count in the image.
        /// </summary>
        public Int32 Count { get; private set; }

        /// <summary>
        /// A hue component of the color.
        /// </summary>
        public Single Hue { get; private set; }

        /// <summary>
        /// A saturation component of the color.
        /// </summary>
        public Single Saturation { get; private set; }

        /// <summary>
        /// A brightness component of the color.
        /// </summary>
        public Single Brightness { get; private set; }

        /// <summary>
        /// A cached hue hashcode.
        /// </summary>
        public Int32 HueHashCode { get; private set; }

        /// <summary>
        /// A cached saturation hashcode.
        /// </summary>
        public Int32 SaturationHashCode { get; private set; }

        /// <summary>
        /// A cached brightness hashcode.
        /// </summary>
        public Int32 BrightnessHashCode { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="ColorInfo"/> struct.
        /// </summary>
        /// <param name="color">The color.</param>
        public ColorInfo(Color color)
            : this()
        {
            Color = color;
            Count = 1;

            Hue = color.GetHue();
            Saturation = color.GetSaturation();
            Brightness = color.GetBrightness();

            HueHashCode = Hue.GetHashCode();
            SaturationHashCode = Saturation.GetHashCode();
            BrightnessHashCode = Brightness.GetHashCode();
        }

        /// <summary>
        /// Increases the count of pixels of this color.
        /// </summary>
        public void IncreaseCount()
        {
            Count++;
        }
    }

    public class QuantizationHelper
    {
        private static readonly Double[] Factors;

        static QuantizationHelper()
        {
            Factors = PrecalculateFactors();
        }

        /// <summary>
        /// Precalculates the alpha-fix values for all the possible alpha values (0-255).
        /// </summary>
        private static Double[] PrecalculateFactors()
        {
            Double[] result = new Double[256];

            for (Int32 value = 0; value < 256; value++)
            {
                result[value] = value / 255.0;
            }

            return result;
        }

        /// <summary>
        /// Converts the alpha blended color to a non-alpha blended color.
        /// </summary>
        /// <param name="color">The alpha blended color (ARGB).</param>
        /// <returns>The non-alpha blended color (RGB).</returns>
        internal static Color ConvertAlpha(Color color)
        {
            Color result = color;

            if (color.A < 255)
            {
                // performs a alpha blending (second color is BackgroundColor, by default a Control color)
                Double colorFactor = (Double)color.A / 255.0d;
                Int32 red = (Int32)(color.R * colorFactor);
                Int32 green = (Int32)(color.G * colorFactor);
                Int32 blue = (Int32)(color.B * colorFactor);
                result = Color.FromArgb(255, red, green, blue);
            }

            return result;
        }

        /// <summary>
        /// Finds the closest color match in a given palette using Euclidean distance.
        /// </summary>
        /// <param name="color">The color to be matched.</param>
        /// <param name="palette">The palette to search in.</param>
        /// <returns>The palette index of the closest match.</returns>
        internal static Int32 GetNearestColor(Color color, IList<Color> palette)
        {
            // initializes the best difference, set it for worst possible, it can only get better
            Int32 bestIndex = 0;
            Int32 leastDistance = Int32.MaxValue;

            // goes thru all the colors in the palette, looking for the best match
            for (Int32 index = 0; index < palette.Count; index++)
            {
                Color targetColor = palette[index];
                Int32 distance = GetColorEuclideanDistanceInRGB(palette.Count, color, targetColor);

                // if a difference is zero, we're good because it won't get better
                if (distance == 0)
                {
                    bestIndex = index;
                    break;
                }

                // if a difference is the best so far, stores it as our best candidate
                if (distance < leastDistance)
                {
                    leastDistance = distance;
                    bestIndex = index;
                }
            }

            // returns the palette index of the most similar color
            return bestIndex;
        }

        ///// <summary>
        ///// Gets the color euclidean distance.
        ///// </summary>
        ///// <param name="requestedColor">Color of the requested.</param>
        ///// <param name="realColor">Color of the real.</param>
        ///// <returns></returns>
        public static Int32 GetColorEuclideanDistanceInRGB(Int32 count, Color requestedColor, Color realColor)
        {
            // calculates a difference for all the color components
            Int32 redDelta = Math.Abs(requestedColor.R - realColor.R);
            Int32 greenDelta = Math.Abs(requestedColor.G - realColor.G);
            Int32 blueDelta = Math.Abs(requestedColor.B - realColor.B);

            Int32 redFactor = redDelta * redDelta;
            Int32 greenFactor = greenDelta * greenDelta;
            Int32 blueFactor = blueDelta * blueDelta;

            // calculates the Euclidean distance, a square-root is not need 
            // as we're only comparing distance, not measuring it
            return redFactor + greenFactor + blueFactor;
        }
    }

    /// <summary>
    /// This is my baby. Read more in the article on the Code Project:
    /// http://www.codeproject.com/KB/recipes/SimplePaletteQuantizer.aspx
    /// </summary>
    public class PaletteQuantizer : IColorQuantizer
    {
        private readonly List<Color> palette;
        private readonly Dictionary<Color, Int32> cache;
        private readonly Dictionary<Color, ColorInfo> colorMap;

        /// <summary>
        /// Initializes a new instance of the <see cref="PaletteQuantizer"/> class.
        /// </summary>
        public PaletteQuantizer()
        {
            palette = new List<Color>();
            cache = new Dictionary<Color, Int32>();
            colorMap = new Dictionary<Color, ColorInfo>();
        }

        #region << IColorQuantizer >>

        /// <summary>
        /// Adds the color to quantizer, only unique colors are added.
        /// </summary>
        /// <param name="color">The color to be added.</param>
        public void AddColor(Color color)
        {
            // if alpha is higher then fully transparent, convert it to a RGB value for more precise processing
            color = QuantizationHelper.ConvertAlpha(color);
            ColorInfo value;

            if (colorMap.TryGetValue(color, out value))
            {
                value.IncreaseCount();
            }
            else
            {
                ColorInfo colorInfo = new ColorInfo(color);
                colorMap.Add(color, colorInfo);
            }
        }

        /// <summary>
        /// Gets the palette with a specified count of the colors.
        /// </summary>
        /// <param name="colorCount">The color count.</param>
        /// <returns></returns>
        public List<Color> GetPalette(Int32 colorCount)
        {
            palette.Clear();

            // lucky seed :)
            Random random = new Random(13);

            // shuffles the colormap
            IEnumerable<ColorInfo> colorInfoList = colorMap.
                OrderBy(entry => random.NextDouble()).
                Select(entry => entry.Value);

            // if there're less colors in the image then allowed, simply pass them all
            if (colorMap.Count > colorCount)
            {
                // solves the color quantization
                colorInfoList = SolveRootLevel(colorInfoList, colorCount);

                // if there're still too much colors, just snap them from the top))
                if (colorInfoList.Count() > colorCount)
                {
                    colorInfoList.OrderByDescending(colorInfo => colorInfo.Count);
                    colorInfoList = colorInfoList.Take(colorCount);
                }
            }

            // clears the hit cache
            cache.Clear();

            // adds the selected colors to a final palette
            palette.AddRange(colorInfoList.Select(colorInfo => colorInfo.Color));

            // returns our new palette
            return palette;
        }

        /// <summary>
        /// Gets the index of the palette for specific color.
        /// </summary>
        /// <param name="color">The color.</param>
        /// <returns></returns>
        public Int32 GetPaletteIndex(Color color)
        {
            Int32 result;
            color = QuantizationHelper.ConvertAlpha(color);

            // checks whether color was already requested, in that case returns an index from a cache
            if (!cache.TryGetValue(color, out result))
            {
                // otherwise finds the nearest color
                result = QuantizationHelper.GetNearestColor(color, palette);
                cache[color] = result;
            }

            // returns a palette index
            return result;
        }

        /// <summary>
        /// Gets the color count.
        /// </summary>
        /// <returns></returns>
        public Int32 GetColorCount()
        {
            return colorMap.Count;
        }

        /// <summary>
        /// Clears this instance.
        /// </summary>
        public void Clear()
        {
            // clears all the information
            cache.Clear();
            colorMap.Clear();
        }

        #endregion

        #region | Helper methods |

        /// <summary>
        /// Selects three lists, based on distinct values of each hue, saturation and brightness color
        /// components, in a single pass.
        /// </summary>
        private static void SelectDistinct(IEnumerable<ColorInfo> colors, out Dictionary<Single, ColorInfo> hueColors, out Dictionary<Single, ColorInfo> saturationColors, out Dictionary<Single, ColorInfo> brightnessColors)
        {
            hueColors = new Dictionary<Single, ColorInfo>();
            saturationColors = new Dictionary<Single, ColorInfo>();
            brightnessColors = new Dictionary<Single, ColorInfo>();

            foreach (ColorInfo colorInfo in colors)
            {
                if (!hueColors.ContainsKey(colorInfo.Hue))
                {
                    hueColors.Add(colorInfo.Hue, colorInfo);
                }

                if (!saturationColors.ContainsKey(colorInfo.Saturation))
                {
                    saturationColors.Add(colorInfo.Saturation, colorInfo);
                }

                if (!brightnessColors.ContainsKey(colorInfo.Brightness))
                {
                    brightnessColors.Add(colorInfo.Brightness, colorInfo);
                }
            }
        }

        private static IEnumerable<ColorInfo> SolveRootLevel(IEnumerable<ColorInfo> colors, Int32 colorCount)
        {
            // initializes the comparers based on hue, saturation and brightness (HSB color model)
            ColorHueComparer hueComparer = new ColorHueComparer();
            ColorSaturationComparer saturationComparer = new ColorSaturationComparer();
            ColorBrightnessComparer brightnessComparer = new ColorBrightnessComparer();

            // selects three palettes: 1) hue is unique, 2) saturation is unique, 3) brightness is unique
            Dictionary<Single, ColorInfo> hueColors, saturationColors, brightnessColors;
            SelectDistinct(colors, out hueColors, out saturationColors, out brightnessColors);

            // selects the palette (from those 3) which has the most colors, because an image has some details in that category)
            if (hueColors.Count > saturationColors.Count && hueColors.Count > brightnessColors.Count)
            {
                colors = Solve2ndLevel(colors, hueColors, saturationComparer, brightnessComparer, colorCount);
            }
            else if (saturationColors.Count > hueColors.Count && saturationColors.Count > brightnessColors.Count)
            {
                colors = Solve2ndLevel(colors, saturationColors, hueComparer, brightnessComparer, colorCount);
            }
            else
            {
                colors = Solve2ndLevel(colors, brightnessColors, hueComparer, saturationComparer, colorCount);
            }

            return colors;
        }

        /// <summary>
        /// If the color count is still high, determine which of the remaining color components 
        /// are prevalent, and filter all the non-distinct values of that color component.
        /// </summary>
        private static IEnumerable<ColorInfo> Solve2ndLevel(IEnumerable<ColorInfo> colors, Dictionary<Single, ColorInfo> defaultColors, IEqualityComparer<ColorInfo> firstComparer, IEqualityComparer<ColorInfo> secondComparer, Int32 colorCount)
        {
            IEnumerable<ColorInfo> result = colors;

            if (defaultColors.Count() > colorCount)
            {
                result = defaultColors.Select(entry => entry.Value);

                IEnumerable<ColorInfo> firstColors = result.Distinct(firstComparer);
                IEnumerable<ColorInfo> secondColors = result.Distinct(secondComparer);

                Int32 firstColorsCount = firstColors.Count();
                Int32 secondColorsCount = secondColors.Count();

                if (firstColorsCount > secondColorsCount)
                {
                    if (firstColorsCount > colorCount)
                    {
                        result = Solve3rdLevel(result, firstColors, secondComparer, colorCount);
                    }
                }
                else
                {
                    if (secondColorsCount > colorCount)
                    {
                        result = Solve3rdLevel(result, secondColors, firstComparer, colorCount);
                    }
                }
            }

            return result;
        }

        /// <summary>
        /// If the color count is still high even so, filter all the non-distinct values of the last color component.
        /// </summary>
        private static IEnumerable<ColorInfo> Solve3rdLevel(IEnumerable<ColorInfo> colors, IEnumerable<ColorInfo> defaultColors, IEqualityComparer<ColorInfo> comparer, Int32 colorCount)
        {
            IEnumerable<ColorInfo> result = colors;

            if (result.Count() > colorCount)
            {
                result = defaultColors;

                IEnumerable<ColorInfo> filteredColors = result.Distinct(comparer);

                if (filteredColors.Count() >= colorCount)
                {
                    result = filteredColors;
                }
            }

            return result;
        }

        #endregion

        #region | Helper classes (comparers) |

        /// <summary>
        /// Compares a hue components of a color info.
        /// </summary>
        private class ColorHueComparer : IEqualityComparer<ColorInfo>
        {
            public Boolean Equals(ColorInfo x, ColorInfo y)
            {
                return x.Hue == y.Hue;
            }

            public Int32 GetHashCode(ColorInfo color)
            {
                return color.HueHashCode;
            }
        }

        /// <summary>
        /// Compares a saturation components of a color info.
        /// </summary>
        private class ColorSaturationComparer : IEqualityComparer<ColorInfo>
        {
            public Boolean Equals(ColorInfo x, ColorInfo y)
            {
                return x.Saturation == y.Saturation;
            }

            public Int32 GetHashCode(ColorInfo color)
            {
                return color.SaturationHashCode;
            }
        }

        /// <summary>
        /// Compares a brightness components of a color info.
        /// </summary>
        private class ColorBrightnessComparer : IEqualityComparer<ColorInfo>
        {
            public Boolean Equals(ColorInfo x, ColorInfo y)
            {
                return x.Brightness == y.Brightness;
            }

            public Int32 GetHashCode(ColorInfo color)
            {
                return color.BrightnessHashCode;
            }
        }

        #endregion
    }

    /// <summary>
    /// This interface provides a color quantization capabilities.
    /// </summary>
    public interface IColorQuantizer
    {
        /// <summary>
        /// Adds the color to quantizer.
        /// </summary>
        /// <param name="color">The color to be added.</param>
        void AddColor(Color color);

        /// <summary>
        /// Gets the palette with specified count of the colors.
        /// </summary>
        /// <param name="colorCount">The color count.</param>
        /// <returns></returns>
        List<Color> GetPalette(Int32 colorCount);

        /// <summary>
        /// Gets the index of the palette for specific color.
        /// </summary>
        /// <param name="color">The color.</param>
        /// <returns></returns>
        Int32 GetPaletteIndex(Color color);

        /// <summary>
        /// Gets the color count.
        /// </summary>
        /// <returns></returns>
        Int32 GetColorCount();

        /// <summary>
        /// Clears this instance.
        /// </summary>
        void Clear();
    }
}
