using System;
using System.IO;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using SharpDX;

namespace TombLib.Utils
{
    public struct ColorC
    {
        public byte B;
        public byte G;
        public byte R;
        public byte A;
        public ColorC(byte r, byte g, byte b, byte a = 255)
        {
            B = b;
            G = g;
            R = r;
            A = a;
        }
    }

    // A very simple but very efficient image that is independent of GDI+.
    // This structure is under the control of the garbage collector and therefore does not need a Dispose() call.
    // Pixel format
    //   [0]: Blue
    //   [1]: Green
    //   [2]: Red
    //   [3]: Alpha
    public struct ImageC : IEquatable<ImageC>
    {
        public static ImageC Black { get; } = new ImageC(1, 1, new byte[] { 0, 0, 0, 0xFF });
        public static ImageC Transparent { get; } = new ImageC(1, 1, new byte[] { 0, 0, 0, 0 });
        public const int PixelSize = 4;

        public int Width { get; set; }
        public int Height { get; set; }
        private byte[] _data { get; set; }

        private ImageC(int width, int height, byte[] data)
        {
            Width = width;
            Height = height;
            _data = data;
        }

        public static bool operator ==(ImageC first, ImageC second)
        {
            return (first.Width == second.Width) && (first.Height == second.Height) && (first._data == second._data);
        }

        public static bool operator !=(ImageC first, ImageC second)
        {
            return !(first == second);
        }

        public bool Equals(ImageC other)
        {
            return this == other;
        }

        public override bool Equals(object obj)
        {
            return this == (ImageC)obj;
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        public override string ToString()
        {
            return "Image (Width=" + Width + ", Height=" + Height + ")";
        }

        public static ImageC CreateNew(int width, int height)
        {
            return new ImageC(width, height, new byte[width * height * PixelSize]);
        }

        public ColorC Get(int i)
        {
            int index = i * PixelSize;
            return new ColorC { B = _data[index], G = _data[index + 1], R = _data[index + 2], A = _data[index + 3] };
        }

        public void Set(int i, byte r, byte g, byte b, byte a = 255)
        {
            int index = i * PixelSize;
            _data[index] = b;
            _data[index + 1] = g;
            _data[index + 2] = r;
            _data[index + 3] = a;
        }

        public void Set(int i, ColorC color)
        {
            Set(i, color.R, color.G, color.B, color.A);
        }

        public ColorC GetPixel(int x, int y)
        {
            int index = (y * Width + x) * PixelSize;
            return new ColorC { B = _data[index], G = _data[index + 1], R = _data[index + 2], A = _data[index + 3] };
        }

        public void SetPixel(int x, int y, byte r, byte g, byte b, byte a = 255)
        {
            int index = (y * Width + x) * PixelSize;
            _data[index] = b;
            _data[index + 1] = g;
            _data[index + 2] = r;
            _data[index + 3] = a;
        }
        
        public void SetPixel(int x, int y, ColorC color)
        {
            SetPixel(x, y, color.R, color.G, color.B, color.A);
        }
        
        public Vector2 Size => new Vector2(Width, Height);

        public int DataSize => Width * Height * PixelSize;

        public static ImageC FromStream(Stream stream)
        {
            long PreviousPosition = stream.Position;

            Image image = null;
            try
            {
                // First try to open it with .Net methods
                try
                {
                    image = Image.FromStream(stream);
                }
                catch (ArgumentException) //Fires if default .NET methods fail 
                { // Try to open it as tga file
                    stream.Position = PreviousPosition;
                    image = Paloma.TargaImage.LoadTargaImage(stream);
                }

                return FromSystemDrawingImage(image);
            }
            finally
            {
                image?.Dispose();
            }
        }

        public static ImageC FromFile(string path)
        {
            using (var stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read))
                return FromStream(stream);
        }
        
        private static ImageC FromSystemDrawingBitmapMatchingPixelFormat(Bitmap bitmap)
        {
            BitmapData bitmapData = bitmap.LockBits(new System.Drawing.Rectangle(0, 0, bitmap.Width, bitmap.Height), ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);
            try
            {
                ImageC result = CreateNew(bitmap.Width, bitmap.Height);
                Marshal.Copy(bitmapData.Scan0, result._data, 0, bitmap.Width * bitmap.Height * PixelSize);
                return result;
            }
            finally
            {
                bitmap.UnlockBits(bitmapData);
            }
        }

        public static ImageC FromSystemDrawingImage(Image image)
        {
            Bitmap imageAsBitmap = image as Bitmap;
            if ((imageAsBitmap != null) && (imageAsBitmap.PixelFormat == PixelFormat.Format32bppArgb))
                return FromSystemDrawingBitmapMatchingPixelFormat(imageAsBitmap);

            using (var convertedBitmap = new Bitmap(image.Width, image.Height))
            {
                using (var g = System.Drawing.Graphics.FromImage(convertedBitmap))
                    g.DrawImageUnscaled(image, 0, 0);
                return FromSystemDrawingBitmapMatchingPixelFormat(convertedBitmap);
            }
        }

        public static ImageC FromStreamRaw(Stream stream, int width, int height)
        {
            ImageC result = ImageC.CreateNew(width, height);
            stream.Read(result._data, 0, width * height * PixelSize);
            return result;
        }

        public void WriteToStreamRaw(Stream stream)
        {
            stream.Write(_data, 0, Width * Height * PixelSize);
        }

        public void Save(string fileName)
        {
            GetTempSystemDrawingBitmap((bitmap) => bitmap.Save(fileName));
        }

        // Try to use 'GetTempSystemDrawingBitmap' instead if possible to avoid unnecessary data allocation
        // The returned Bitmap must be Dispose()ed.
        public Bitmap ToBitmap()
        {
            // Create bitmap
            Bitmap result = new Bitmap(Width, Height, PixelFormat.Format32bppArgb);
            try
            {
                BitmapData resultData = result.LockBits(new System.Drawing.Rectangle(0, 0, Width, Height), ImageLockMode.WriteOnly, PixelFormat.Format32bppArgb);
                try
                {
                    Marshal.Copy(_data, 0, resultData.Scan0, resultData.Height * resultData.Stride);
                }
                finally
                {
                    result.UnlockBits(resultData);
                }
                return result;
            }
            catch
            {
                result?.Dispose();
                throw;
            }
        }
        
        // Bitmap has the pixel format 'Format32bppArgb'
        public unsafe void GetTempSystemDrawingBitmap(Action<Bitmap> bitmapAction)
        {
            fixed (void* dataPtr = _data)
                using (var bitmap = new Bitmap(Width, Height, Width * PixelSize, PixelFormat.Format32bppArgb, new IntPtr(dataPtr))) // Temporaty bitmap
                    bitmapAction(bitmap);
        }

        public unsafe void GetIntPtr(Action<IntPtr> intPtrAction)
        {
            fixed (void* dataPtr = _data)
                intPtrAction(new IntPtr(dataPtr));
        }

        public unsafe void CopyFrom(int toX, int toY, ImageC fromImage, int fromX, int fromY, int width, int height)
        {
            // Check coordinates
            if ((toX < 0) || (toY < 0) || (fromX < 0) || (fromY < 0) || (width < 0) || (height < 0) ||
                (toX + width > Width) || (toY + height > Height) ||
                (fromX + width > fromImage.Width) || (fromY + height > fromImage.Height))
                throw new ArgumentOutOfRangeException();

            // Copy data quickly
            fixed (void* toPtr = _data)
            {
                fixed (void* fromPtr = fromImage._data)
                {
                    // Adjust starting position to account for image positions
                    uint* toPtrOffseted = (uint*)toPtr + toY * Width + toX;
                    uint* fromPtrOffseted = (uint*)fromPtr + fromY * fromImage.Width + fromX;

                    // Copy image data line by line
                    for (int y = 0; y < height; ++y)
                    {
                        uint* toLinePtr = toPtrOffseted + y * Width;
                        uint* fromLinePtr = fromPtrOffseted + y * fromImage.Width;
                        for (int x = 0; x < width; ++x)
                            toLinePtr[x] = fromLinePtr[x];
                    }
                }
            }
        }


        /// <summary>
        /// uint's are platform dependet representation of the color. 
        /// They should stay private inside ImageC to prevent abuse.
        /// </summary>
        private static unsafe uint ColorToUint(ColorC color)
        {
            byte* byteArray = stackalloc byte[4];
            byteArray[0] = color.B;
            byteArray[1] = color.G;
            byteArray[2] = color.R;
            byteArray[3] = color.A;
            return *(uint*)byteArray;
        }

        public unsafe void ReplaceColor(ColorC from, ColorC to)
        {
            uint fromUint = ColorToUint(from);
            uint toUint = ColorToUint(to);
            
            fixed (void* ptr = _data)
            {
                uint* ptrUint = (uint*)ptr;
                uint* ptrUintEnd = ptrUint + Width * Height;
                while (ptrUint < ptrUintEnd)
                {
                    if (*ptrUint == fromUint)
                        *ptrUint = toUint;
                    ++ptrUint;
                }
            }
        }

        public unsafe bool HasAlpha()
        {
            uint alphaBits = ColorToUint(new ColorC(0, 0, 0, 255));

            fixed (void* ptr = _data)
            {
                uint* ptrUint = (uint*)ptr;
                uint* ptrUintEnd = ptrUint + Width * Height;
                while (ptrUint < ptrUintEnd)
                {
                    if ((*ptrUint & alphaBits) != alphaBits)
                        return true;
                    ++ptrUint;
                }
            }
            return false;
        }

        public unsafe bool HasAlpha(int X, int Y, int width, int height)
        {
            // Check coordinates
            if ((X < 0) || (Y < 0) || (width < 0) || (height < 0) ||
                (X + width > Width) || (Y + height > Height))
                throw new ArgumentOutOfRangeException();

            // Check for alpha
            uint alphaBits = ColorToUint(new ColorC(0, 0, 0, 255));

            fixed (void* ptr = _data)
            {
                uint* toPtrOffseted = (uint*)ptr + Y * Width + X;
                for (int y = 0; y < height; ++y)
                {
                    uint* linePtr = toPtrOffseted + y * Width;
                    for (int x = 0; x < width; ++x)
                        if ((linePtr[x] & alphaBits) != alphaBits)
                            return true;
                }
            }

            return false;
        }
        public void CopyFrom(int toX, int toY, ImageC fromImage)
        {
            CopyFrom(toX, toY, fromImage, 0, 0, fromImage.Width, fromImage.Height);
        }

        public Stream ToRawStream()
        {
            return new MemoryStream(_data);
        }

        public byte[] ToByteArray()
        {
            byte[] buffer = new byte[_data.Length];
            Array.Copy(_data, buffer, _data.Length);
            return buffer;
        }

        public Stream ToRawStream(int yStart, int Height)
        {
            return new MemoryStream(_data, yStart * (Width * PixelSize), Height * (Width * PixelSize));
        }
        
        public ulong HashImageData(System.Security.Cryptography.HashAlgorithm hashAlgorithm)
        {
            ulong metaHash = unchecked((ulong)Width * 4551534108298448059ul + (ulong)Height * 7310107420406914801ul); // two random primes
            byte[] dataHashArr = hashAlgorithm.ComputeHash(_data);
            ulong dataHash = BitConverter.ToUInt64(dataHashArr, 0);
            return dataHash ^ metaHash;
        }

        public void RawCopyTo(byte[] destination, int offset)
        {
            Array.Copy(_data, 0, destination, offset, _data.GetLength(0));
        }
    }
}
