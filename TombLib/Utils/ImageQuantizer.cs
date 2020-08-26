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
        public static void AddColorsToQuantizer(this Image image, OctreeQuantizer quantizer)
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
    }

    internal class OctreeNode
    {
        private static readonly Byte[] Mask = new Byte[] { 0x80, 0x40, 0x20, 0x10, 0x08, 0x04, 0x02, 0x01 };

        private Int32 red;
        private Int32 green;
        private Int32 blue;

        private Int32 pixelCount;
        private Int32 paletteIndex;

        private readonly OctreeNode[] nodes;

        /// <summary>
        /// Initializes a new instance of the <see cref="OctreeNode"/> class.
        /// </summary>
        public OctreeNode(Int32 level, OctreeQuantizer parent)
        {
            nodes = new OctreeNode[8];

            if (level < 7)
            {
                parent.AddLevelNode(level, this);
            }
        }

        #region | Calculated properties |

        /// <summary>
        /// Gets a value indicating whether this node is a leaf.
        /// </summary>
        /// <value><c>true</c> if this node is a leaf; otherwise, <c>false</c>.</value>
        public Boolean IsLeaf
        {
            get { return pixelCount > 0; }
        }

        /// <summary>
        /// Gets the averaged leaf color.
        /// </summary>
        /// <value>The leaf color.</value>
        public Color Color
        {
            get
            {
                Color result;

                // determines a color of the leaf
                if (IsLeaf)
                {
                    if (pixelCount == 1)
                    {
                        // if a pixel count for this color is 1 than this node contains our color already
                        result = Color.FromArgb(255, red, green, blue);
                    }
                    else
                    {
                        // otherwise calculates the average color (without rounding)
                        result = Color.FromArgb(255, red / pixelCount, green / pixelCount, blue / pixelCount);
                    }
                }
                else
                {
                    throw new InvalidOperationException("Cannot retrieve a color for other node than leaf.");
                }

                return result;
            }
        }

        /// <summary>
        /// Gets the active nodes pixel count.
        /// </summary>
        /// <value>The active nodes pixel count.</value>
        public Int32 ActiveNodesPixelCount
        {
            get
            {
                Int32 result = pixelCount;

                // sums up all the pixel presence for all the active nodes
                for (Int32 index = 0; index < 8; index++)
                {
                    OctreeNode node = nodes[index];

                    if (node != null)
                    {
                        result += node.pixelCount;
                    }
                }

                return result;
            }
        }

        /// <summary>
        /// Enumerates only the leaf nodes.
        /// </summary>
        /// <value>The enumerated leaf nodes.</value>
        public IEnumerable<OctreeNode> ActiveNodes
        {
            get
            {
                List<OctreeNode> result = new List<OctreeNode>();

                // adds all the active sub-nodes to a list
                for (Int32 index = 0; index < 8; index++)
                {
                    OctreeNode node = nodes[index];

                    if (node != null)
                    {
                        if (node.IsLeaf)
                        {
                            result.Add(node);
                        }
                        else
                        {
                            result.AddRange(node.ActiveNodes);
                        }
                    }
                }

                return result;
            }
        }

        #endregion

        #region | Methods |

        /// <summary>
        /// Adds the color.
        /// </summary>
        /// <param name="color">The color.</param>
        /// <param name="level">The level.</param>
        /// <param name="parent">The parent.</param>
        public void AddColor(Color color, Int32 level, OctreeQuantizer parent)
        {
            // if this node is a leaf, then increase a color amount, and pixel presence
            if (level == 8)
            {
                red += color.R;
                green += color.G;
                blue += color.B;
                pixelCount++;
            }
            else if (level < 8) // otherwise goes one level deeper
            {
                // calculates an index for the next sub-branch
                Int32 index = GetColorIndexAtLevel(color, level);

                // if that branch doesn't exist, grows it
                if (nodes[index] == null)
                {
                    nodes[index] = new OctreeNode(level, parent);
                }

                // adds a color to that branch
                nodes[index].AddColor(color, level + 1, parent);
            }
        }

        /// <summary>
        /// Gets the index of the palette.
        /// </summary>
        /// <param name="color">The color.</param>
        /// <param name="level">The level.</param>
        /// <returns></returns>
        public Int32 GetPaletteIndex(Color color, Int32 level)
        {
            Int32 result;

            // if a node is leaf, then we've found are best match already
            if (IsLeaf)
            {
                result = paletteIndex;
            }
            else // otherwise continue in to the lower depths
            {
                Int32 index = GetColorIndexAtLevel(color, level);
                result = nodes[index].GetPaletteIndex(color, level + 1);
            }

            return result;
        }

        /// <summary>
        /// Removes the leaves by summing all it's color components and pixel presence.
        /// </summary>
        /// <returns></returns>
        public Int32 RemoveLeaves(Int32 level, Int32 activeColorCount, Int32 targetColorCount, OctreeQuantizer parent)
        {
            Int32 result = 0;

            // scans thru all the active nodes
            for (Int32 index = 0; index < 8; index++)
            {
                OctreeNode node = nodes[index];

                if (node != null)
                {
                    // sums up their color components
                    red += node.red;
                    green += node.green;
                    blue += node.blue;

                    // and pixel presence
                    pixelCount += node.pixelCount;

                    // then deactivates the node 
                    // nodes[index] = null;

                    // increases the count of reduced nodes
                    result++;
                }
            }

            // returns a number of reduced sub-nodes, minus one because this node becomes a leaf
            return result - 1;
        }

        #endregion

        #region | Helper methods |

        /// <summary>
        /// Calculates the color component bit (level) index.
        /// </summary>
        /// <param name="color">The color for which the index will be calculated.</param>
        /// <param name="level">The bit index to be used for index calculation.</param>
        /// <returns>The color index at a certain depth level.</returns>
        private static Int32 GetColorIndexAtLevel(Color color, Int32 level)
        {
            return ((color.R & Mask[level]) == Mask[level] ? 4 : 0) |
                   ((color.G & Mask[level]) == Mask[level] ? 2 : 0) |
                   ((color.B & Mask[level]) == Mask[level] ? 1 : 0);
        }

        /// <summary>
        /// Sets a palette index to this node.
        /// </summary>
        /// <param name="index">The palette index.</param>
        internal void SetPaletteIndex(Int32 index)
        {
            paletteIndex = index;
        }

        #endregion
    }

    /// <summary>
    /// The idea here is to build a tree structure containing always a maximum of K different 
    /// colors. If a further color is to be added to the tree structure, its color value has 
    /// to be merged with the most likely one that is already in the tree. The both values are 
    /// substituted by their mean. 
    ///
    /// The most important data structure are the nodes of the octree. Each inner node of the 
    /// octree contain a maximum of eight successors, the leave nodes keep information for the 
    /// color value (colorvalue), the color index (colorindex), and a counter (colorcount) for 
    /// the pixel that are already mapped to a particular leave. Because each of the red, green 
    /// and blue value is between 0 and 255 the maximum depth of the tree is eight. In level i 
    /// Bit i of RGB is used as selector for the successors. 
    ///
    /// The octree is constructed during reading the image that is to be quantized. Only that 
    /// parts of the octree are created that are really needed. Initially the first K values 
    /// are represented exactly (in level eight). When the number of leaves nodes (currentK) 
    /// exceeds K, the tree has to reduced. That would mean that leaves at the largest depth 
    /// are substituted by their predecessor.
    /// </summary>
    /// 
    public class OctreeQuantizer
    {
        private OctreeNode root;
        private readonly List<OctreeNode>[] levels;

        /// <summary>
        /// Initializes a new instance of the <see cref="Octree"/> class.
        /// </summary>
        public OctreeQuantizer()
        {
            // initializes the octree level lists
            levels = new List<OctreeNode>[7];

            // creates the octree level lists
            for (Int32 level = 0; level < 7; level++)
            {
                levels[level] = new List<OctreeNode>();
            }

            // creates a root node
            root = new OctreeNode(0, this);
        }

        #region | Calculated properties |

        /// <summary>
        /// Gets the leaf nodes only (recursively).
        /// </summary>
        /// <value>All the tree leaves.</value>
        internal IEnumerable<OctreeNode> Leaves
        {
            get { return root.ActiveNodes.Where(node => node.IsLeaf); }
        }

        #endregion

        #region | Methods |

        /// <summary>
        /// Adds the node to a level node list.
        /// </summary>
        /// <param name="level">The depth level.</param>
        /// <param name="octreeNode">The octree node to be added.</param>
        internal void AddLevelNode(Int32 level, OctreeNode octreeNode)
        {
            levels[level].Add(octreeNode);
        }

        #endregion

        #region << IColorQuantizer >>

        /// <summary>
        /// Adds the color to quantizer.
        /// </summary>
        /// <param name="color">The color to be added.</param>
        public void AddColor(Color color)
        {
            color = QuantizationHelper.ConvertAlpha(color);
            root.AddColor(color, 0, this);
        }

        /// <summary>
        /// Gets the palette with specified count of the colors.
        /// </summary>
        /// <param name="colorCount">The color count.</param>
        /// <returns></returns>
        public List<Color> GetPalette(Int32 colorCount)
        {
            List<Color> result = new List<Color>();
            Int32 leafCount = Leaves.Count();
            Int32 paletteIndex = 0;

            // goes thru all the levels starting at the deepest, and goes upto a root level
            for (Int32 level = 6; level >= 0; level--)
            {
                // if level contains any node
                if (levels[level].Count > 0)
                {
                    // orders the level node list by pixel presence (those with least pixels are at the top)
                    IEnumerable<OctreeNode> sortedNodeList = levels[level].
                        OrderBy(node => node.ActiveNodesPixelCount);

                    // removes the nodes unless the count of the leaves is lower or equal than our requested color count
                    foreach (OctreeNode node in sortedNodeList)
                    {
                        // removes a node
                        leafCount -= node.RemoveLeaves(level, leafCount, colorCount, this);

                        // if the count of leaves is lower then our requested count terminate the loop
                        if (leafCount <= colorCount) break;
                    }

                    // if the count of leaves is lower then our requested count terminate the level loop as well
                    if (leafCount <= colorCount) break;

                    // otherwise clear whole level, as it is not needed anymore
                    levels[level].Clear();
                }
            }

            // goes through all the leaves that are left in the tree (there should now be less or equal than requested)
            foreach (OctreeNode node in Leaves.OrderByDescending(node => node.ActiveNodesPixelCount))
            {
                if (paletteIndex >= colorCount) break;

                // adds the leaf color to a palette
                if (node.IsLeaf)
                {
                    result.Add(node.Color);
                }

                // and marks the node with a palette index
                node.SetPaletteIndex(paletteIndex++);
            }

            // we're unable to reduce the Octree with enough precision, and the leaf count is zero
            if (result.Count == 0)
            {
                throw new NotSupportedException("The Octree contains after the reduction 0 colors, it may happen for 1-16 colors because it reduces by 1-8 nodes at time. Should be used on 8 or above to ensure the correct functioning.");
            }

            // returns the palette
            return result;
        }

        /// <summary>
        /// Gets the index of the palette for specific color.
        /// </summary>
        /// <param name="color">The color.</param>
        /// <returns></returns>
        public Int32 GetPaletteIndex(Color color)
        {
            color = QuantizationHelper.ConvertAlpha(color);

            // retrieves a palette index
            return root.GetPaletteIndex(color, 0);
        }

        /// <summary>
        /// Gets the color count.
        /// </summary>
        /// <returns></returns>
        public Int32 GetColorCount()
        {
            // calculates the number of leaves, by parsing the whole tree
            return Leaves.Count();
        }

        /// <summary>
        /// Clears this instance.
        /// </summary>
        public void Clear()
        {
            // clears all the node list levels
            foreach (List<OctreeNode> level in levels)
            {
                level.Clear();
            }

            // creates a new root node (thus throwing away the old tree)
            root = new OctreeNode(0, this);
        }

        #endregion
    }
}
