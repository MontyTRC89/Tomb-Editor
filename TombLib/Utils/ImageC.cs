using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Drawing.PSD;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Runtime.InteropServices;

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

        public static implicit operator Vector4(ColorC this_)
        {
            const float _floatFactor = 1.0f / 255.0f;
            return new Vector4(this_.R * _floatFactor, this_.G * _floatFactor, this_.B * _floatFactor, this_.A * _floatFactor);
        }

        public static explicit operator ColorC(Vector4 this_)
        {
            this_ = Vector4.Min(Vector4.Max(this_ * 255.99998f, new Vector4()), new Vector4(255.0f));
            return new ColorC((byte)this_.X, (byte)this_.Y, (byte)this_.Z, (byte)this_.W);
        }

        public static Vector4 Mix(Vector4 background, Vector4 foreground)
        {
            // https://en.wikipedia.org/wiki/Alpha_compositing#Alpha_blending
            float backgroundTotal = background.W * (1.0f - foreground.W);
            float alpha = foreground.W + backgroundTotal;
            Vector4 output = foreground * foreground.W + background * backgroundTotal;
            if (alpha > float.Epsilon)
                output /= alpha;
            output.W = alpha;
            return output;
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

        public int Width { get; private set; }
        public int Height { get; private set; }
        private byte[] _data { get; set; }

        private ImageC(int width, int height, byte[] data)
        {
            Width = width;
            Height = height;
            _data = data;
        }

        public static bool operator ==(ImageC first, ImageC second) =>
            first.Width == second.Width && first.Height == second.Height && first._data == second._data;
        public static bool operator !=(ImageC first, ImageC second) => !(first == second);
        public bool Equals(ImageC other) => this == other;
        public override bool Equals(object other) => other is ImageC && this == (ImageC)other;
        public override int GetHashCode() => base.GetHashCode();
        public override string ToString() => "Image (Width=" + Width + ", Height=" + Height + ")";

        public static ImageC CreateNew(int width, int height)
        {
            return new ImageC(width, height, new byte[width * height * PixelSize]);
        }

        public ColorC Get(int i)
        {
            int index = i * PixelSize;
            if (index + 3 >= _data.Length || index < 0)
                return new ColorC(255, 0, 0);
            else
                return new ColorC { B = _data[index], G = _data[index + 1], R = _data[index + 2], A = _data[index + 3] };
        }

        public void Set(int i, byte r, byte g, byte b, byte a = 255)
        {
            int index = i * PixelSize;
            if (index + 3 >= _data.Length || index < 0)
                return;

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
            if (index + 3 >= _data.Length || index < 0)
                return new ColorC(255, 0, 0);
            else
                return new ColorC { B = _data[index], G = _data[index + 1], R = _data[index + 2], A = _data[index + 3] };
        }

        public void SetPixel(int x, int y, byte r, byte g, byte b, byte a = 255)
        {
            int index = (y * Width + x) * PixelSize;
            if (index + 3 >= _data.Length || index < 0)
                return;

            _data[index] = b;
            _data[index + 1] = g;
            _data[index + 2] = r;
            _data[index + 3] = a;
        }

        public void SetPixel(int x, int y, ColorC color)
        {
            SetPixel(x, y, color.R, color.G, color.B, color.A);
        }

        public void ApplyKernel(int xStart, int yStart, int width, int height, int weight, int [,] kernel)
        {
            ImageC oldImage = new ImageC(width, height, new byte[width * height * 4]);
            oldImage.CopyFrom(0, 0, this, xStart, yStart, width, height);

            int kernel_width = kernel.GetUpperBound(0) + 1;
            int kernel_height = kernel.GetUpperBound(1) + 1;

            for (int x = 0, xReal = xStart; x < width; x++, xReal++)
                for (int y = 0, yReal = yStart; y < height; y++, yReal++)
                {
                    int r = 0, g = 0, b = 0;
                    for (int dx = 0; dx < kernel_width; dx++)
                        for (int dy = 0; dy < kernel_height; dy++)
                        {
                            int sourceX = MathC.Clamp(x + dx, 0, width - 1);
                            int sourceY = MathC.Clamp(y + dy, 0, height - 1);
                            ColorC clr = oldImage.GetPixel(sourceX, sourceY);
                            r += (int)clr.R * kernel[dx, dy];
                            g += (int)clr.G * kernel[dx, dy];
                            b += (int)clr.B * kernel[dx, dy];
                        }
                    r = MathC.Clamp((int)(127 + r / weight), 0, 255);
                    g = MathC.Clamp((int)(127 + g / weight), 0, 255);
                    b = MathC.Clamp((int)(127 + b / weight), 0, 255);
                    SetPixel(xReal, yReal, new ColorC((byte)r, (byte)g, (byte)b));
                }

            // Restore alpha
            for (int x = 0, xReal = xStart; x < width; x++, xReal++)
                for (int y = 0, yReal = yStart; y < height; y++, yReal++)
                {
                    var alpha = oldImage.GetPixel(xReal, yReal).A;
                    var color = GetPixel(xReal, yReal);
                    color.A = alpha;
                    SetPixel(xReal, yReal, color);
                }
        }

        public void Emboss(int xStart, int yStart, int width, int height, int weight, int size)
        {
            size = MathC.Clamp(size, 2, 8);
            int[,] kernel = new int[size, size];
            kernel[0, 0] = -1;
            kernel[size - 1, size - 1] = 1;

            ApplyKernel(xStart, yStart, width, height, weight, kernel);
        }

        public VectorInt2 Size => new VectorInt2(Width, Height);

        public int DataSize => Width * Height * PixelSize;

        private static readonly byte[] Tga2_Signature = new byte[18] { 84, 82, 85, 69, 86, 73, 83, 73, 79, 78, 45, 88, 70, 73, 76, 69, 46, 0 };

        private static bool IsTga(byte[] startBytes)
        {
            // Inspired by the FreeType tga image validation routine
            // "Validate" in PluginTARGE.cpp

            if (startBytes.SequenceEqual(Tga2_Signature))
                return true;

            byte colorMapType = startBytes[1];
            byte imageType = startBytes[2];
            ushort colorMapFirstEntry = BitConverter.ToUInt16(startBytes, 3);
            ushort colorMapLength = BitConverter.ToUInt16(startBytes, 5);
            byte colorMapSize = startBytes[7];
            ushort width = BitConverter.ToUInt16(startBytes, 12);
            ushort height = BitConverter.ToUInt16(startBytes, 14);
            byte pixelDepth = startBytes[16];

            if (colorMapType != 0 && colorMapType != 1)
                return false;
            if (colorMapType == 1)
            {
                if (colorMapFirstEntry >= colorMapLength)
                    return false;
                if (colorMapSize == 0 || colorMapSize > 32)
                    return false;
            }
            if (width == 0 || height == 0)
                return false;

            switch (imageType)
            {
                case 1: // Cmap
                case 2: // RGB
                case 3: // Mono
                case 9: // RLE Cmap
                case 10: // RLE RGB
                case 11: // RLE Mono
                    break;
                default:
                    return false;
            }

            switch (pixelDepth)
            {
                case 8:
                case 16:
                case 24:
                case 32:
                    break;
                default:
                    return false;
            }

            return true;
        }

        private static ImageC FromPfimImage(Pfim.IImage image)
        {
            switch (image.Format)
            {
                case Pfim.ImageFormat.Rgba32:
                    return new ImageC(image.Width, image.Height, image.Data);
                case Pfim.ImageFormat.Rgb24:
                    byte[] data = image.Data;
                    int stride = image.Stride;
                    ImageC result = CreateNew(image.Width, image.Height);
                    for (int y = 0; y < result.Height; ++y)
                    {
                        int inputIndex = y * stride;
                        int outputIndex = y * result.Width * PixelSize;

                        for (int x = 0; x < result.Width; ++x)
                        {
                            result._data[outputIndex + 0] = data[inputIndex + 0];
                            result._data[outputIndex + 1] = data[inputIndex + 1];
                            result._data[outputIndex + 2] = data[inputIndex + 2];
                            result._data[outputIndex + 3] = 0xff;

                            inputIndex += 3;
                            outputIndex += 4;
                        }
                    }
                    return result;
                default:
                    throw new NotImplementedException("Pfim image library type " + image.Format + " not handled!");
            }
        }

        public static ImageC FromStream(Stream stream)
        {
            long PreviousPosition = stream.Position;

            // Read some start bytes
            long startPos = stream.Position;
            byte[] startBytes = new byte[18];
            stream.Read(startBytes, 0, 18);
            stream.Position = startPos;

            // Detect special image types
            if (startBytes[0] == 0x44 && startBytes[1] == 0x44 && startBytes[2] == 0x53 && startBytes[3] == 0x20)
            { // dds image
                return FromPfimImage(Pfim.Dds.Create(stream, new Pfim.PfimConfig()));
            }
            else if (startBytes[0] == 0x38 && startBytes[1] == 0x42 && startBytes[2] == 0x50 && startBytes[3] == 0x53)
            { // psd image
                PsdFile image = new PsdFile();
                image.Load(stream);
                using (Image image2 = ImageDecoder.DecodeImage(image))
                    return FromSystemDrawingImage(image2);
            }
            else if (IsTga(startBytes))
            { // tga image
                return FromPfimImage(Pfim.Targa.Create(stream, new Pfim.PfimConfig()));
            }
            else
            { // other image
                using (Image image = Image.FromStream(stream))
                    return FromSystemDrawingImage(image);
            }
        }

        public static ImageC FromFile(string path)
        {
            using (var stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read))
                return FromStream(stream);
        }

        public static IReadOnlyList<FileFormat> FromFileFileExtensions { get; } = new List<FileFormat>()
        {
            new FileFormat("Portable Network Graphics", "png"),
            new FileFormat("Truevision Targa", "tga"),
            new FileFormat("Windows Bitmap", "bmp", "dib"),
            new FileFormat("Jpeg Image (Not recommended)", "jpg", "jpeg", "jpe", "jif", "jfif", "jfi"),
            new FileFormat("Graphics Interchange Format (Not recommended)", "gif"),
            new FileFormat("Photoshop File", "psd"),
            new FileFormat("Windows Meta File (Not recommended)", "wmf", "emf")
        };

        public static IReadOnlyList<FileFormat> SaveFileFileExtensions { get; } = new List<FileFormat>()
        {
            new FileFormat("Portable Network Graphics", "png"),
            new FileFormat("Windows Bitmap", "bmp", "dib"),
            new FileFormat("Jpeg Image", "jpg", "jpeg", "jpe", "jif", "jfif", "jfi"),
            new FileFormat("Graphics Interchange Format (Not recommended)", "gif")
        };

        private static ImageC FromSystemDrawingBitmapMatchingPixelFormat(Bitmap bitmap)
        {
            BitmapData bitmapData = bitmap.LockBits(new Rectangle(0, 0, bitmap.Width, bitmap.Height), ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);
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
            if (imageAsBitmap != null && imageAsBitmap.PixelFormat == PixelFormat.Format32bppArgb)
                return FromSystemDrawingBitmapMatchingPixelFormat(imageAsBitmap);

            using (var convertedBitmap = new Bitmap(image.Width, image.Height))
            {
                using (var g = System.Drawing.Graphics.FromImage(convertedBitmap))
                    g.DrawImage(image, 0, 0, image.Width, image.Height);
                return FromSystemDrawingBitmapMatchingPixelFormat(convertedBitmap);
            }
        }

        public static ImageC FromStreamRaw(Stream stream, int width, int height)
        {
            ImageC result = CreateNew(width, height);
            stream.Read(result._data, 0, width * height * PixelSize);
            return result;
        }

        public static ImageC FromByteArray(byte[] data, int width, int height)
        {
            ImageC result = CreateNew(width, height);
            Array.Copy(data, result._data, data.Length);
            return result;
        }

        public void WriteToStreamRaw(Stream stream)
        {
            stream.Write(_data, 0, Width * Height * PixelSize);
        }

        public void Save(string fileName)
        {
            // Figure out image format
            string extension = Path.GetExtension(fileName).Remove(0, 1).ToLowerInvariant();
            switch (extension)
            {
                case "png":
                    GetTempSystemDrawingBitmap(bitmap => bitmap.Save(fileName, ImageFormat.Png));
                    break;
                case "bmp":
                case "dib":
                    GetTempSystemDrawingBitmap(bitmap => bitmap.Save(fileName, ImageFormat.Bmp));
                    break;
                case "jpg":
                case "jpeg":
                case "jpe":
                case "jif":
                case "jfif":
                case "jfi":
                    using (EncoderParameters encoderParameters = new EncoderParameters(1))
                    using (EncoderParameter encoderParameter = new EncoderParameter(Encoder.Quality, 95L))
                    {
                        ImageCodecInfo codecInfo = ImageCodecInfo.GetImageDecoders().First(codec => codec.FormatID == ImageFormat.Jpeg.Guid);
                        encoderParameters.Param[0] = encoderParameter;
                        GetTempSystemDrawingBitmap(bitmap => bitmap.Save(fileName, codecInfo, encoderParameters));
                    }
                    break;
                case "gif":
                    GetTempSystemDrawingBitmap(bitmap => bitmap.Save(fileName, ImageFormat.Gif));
                    break;
                default:
                    GetTempSystemDrawingBitmap(bitmap => bitmap.Save(fileName));
                    break;
            }
        }

        // Try to use 'GetTempSystemDrawingBitmap' instead if possible to avoid unnecessary data allocation
        // The returned Bitmap must be Dispose()ed.
        public Bitmap ToBitmap()
        {
            // Create bitmap
            Bitmap result = new Bitmap(Width, Height, PixelFormat.Format32bppArgb);
            try
            {
                BitmapData resultData = result.LockBits(new Rectangle(0, 0, Width, Height), ImageLockMode.WriteOnly, PixelFormat.Format32bppArgb);
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
                using (var bitmap = new Bitmap(Width, Height, Width * PixelSize, PixelFormat.Format32bppArgb, new IntPtr(dataPtr))) // Temporary bitmap
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
            if (toX < 0 || toY < 0 || fromX < 0 || fromY < 0 || width < 0 || height < 0 ||
                toX + width > Width || toY + height > Height ||
                fromX + width > fromImage.Width || fromY + height > fromImage.Height)
                return;
            //throw new ArgumentOutOfRangeException();

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
            // TODO: check Woops bug
            if (X < 0 || Y < 0 || width < 0 || height < 0 ||
                X + width > Width || Y + height > Height)
                return false; // throw new ArgumentOutOfRangeException();

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
            return _data;
        }

        public Stream ToRawStream(int yStart, int Height)
        {
            return new MemoryStream(_data, yStart * Width * PixelSize, Height * Width * PixelSize);
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
