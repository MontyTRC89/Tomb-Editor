using System;
using System.Buffers.Binary;
using System.IO;
using System.IO.Compression;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.Intrinsics.X86;
using System.Runtime.Intrinsics;
using System.Text;
using System.Runtime.InteropServices;


namespace TombLib.Utils
{
	public static class PngWriter
	{
		public static void SaveToFile(string filePath, ReadOnlySpan<byte> rgba, int width, int height)
		{
			using var fs = new FileStream(filePath, FileMode.Create, FileAccess.Write, FileShare.None);
			SaveToStream(fs, rgba, width, height);
		}

		[MethodImpl(MethodImplOptions.AggressiveOptimization)]
		public static void SaveToStream(Stream stream, ReadOnlySpan<byte> rgba, int width, int height)
		{
			int stride = width * 4;
			if (rgba.Length != stride * height)
				throw new ArgumentException("Invalid data size for given dimensions.");

			using var writer = new BinaryWriter(stream, Encoding.UTF8, leaveOpen: true);

			// PNG signature
			writer.Write(new byte[] { 137, 80, 78, 71, 13, 10, 26, 10 });

			// IHDR
			WriteChunk(writer, "IHDR", stream =>
			{
				Span<byte> hdr = stackalloc byte[13];
				BinaryPrimitives.WriteInt32BigEndian(hdr.Slice(0, 4), width);
				BinaryPrimitives.WriteInt32BigEndian(hdr.Slice(4, 4), height);
				hdr[8] = 8;  // Bit depth
				hdr[9] = 6;  // RGBA
				hdr[10] = 0; // Compression (DEFLATE)
				hdr[11] = 0; // Filter
				hdr[12] = 0; // No interlace
				stream.Write(hdr);
			});

			// IDAT (scrittura separata)
			WriteIdatChunkBgraToRgbaSsse3(writer, rgba, width, height);

			// IEND
			WriteChunk(writer, "IEND", stream => { });
		}

		[MethodImpl(MethodImplOptions.AggressiveOptimization)]
		private static void WriteIdatChunk(BinaryWriter writer, ReadOnlySpan<byte> rgba, int width, int height)
		{
			int stride = width * 4;
			int simdSize = Vector<byte>.Count;

			using var ms = new MemoryStream();
			using (var deflate = new ZLibStream(ms, CompressionLevel.Fastest, leaveOpen: true))
			{
				Span<byte> row = stackalloc byte[stride + 1]; // filter byte + data

				for (int y = 0; y < height; y++)
				{
					row[0] = 0; // PNG filter type 0 (none)
					ReadOnlySpan<byte> src = rgba.Slice(y * stride, stride);
					Span<byte> dst = row.Slice(1); // skip filter byte

					// Convert BGRA to RGBA
					for (int i = 0; i < stride; i += 4)
					{
						dst[i + 0] = src[i + 2]; // R
						dst[i + 1] = src[i + 1]; // G
						dst[i + 2] = src[i + 0]; // B
						dst[i + 3] = src[i + 3]; // A
					}

					deflate.Write(row);
				}
			}

			// Scrivi il contenuto zlib come chunk IDAT
			WriteChunk(writer, "IDAT", ms.WriteTo);
		}

		[MethodImpl(MethodImplOptions.AggressiveOptimization)]
		private static void WriteIdatChunkBgraToRgbaSsse3(BinaryWriter writer, ReadOnlySpan<byte> bgra, int width, int height)
		{
			int stride = width * 4;

			using var ms = new MemoryStream();
			using (var deflate = new ZLibStream(ms, CompressionLevel.Fastest, leaveOpen: true))
			{
				Span<byte> row = stackalloc byte[stride + 1];

				for (int y = 0; y < height; y++)
				{
					row[0] = 0;
					var src = bgra.Slice(y * stride, stride);
					var dst = row.Slice(1);

					if (Ssse3.IsSupported)
					{
						var mask = Vector128.Create(
							(byte)2, 1, 0, 3,
							6, 5, 4, 7,
							10, 9, 8, 11,
							14, 13, 12, 15
						);

						ref byte srcRef = ref MemoryMarshal.GetReference(src);
						ref byte dstRef = ref MemoryMarshal.GetReference(dst);

						int i = 0;
						for (; i <= stride - 16; i += 16)
						{
							var vec = Unsafe.ReadUnaligned<Vector128<byte>>(ref Unsafe.Add(ref srcRef, i));
							var shuffled = Ssse3.Shuffle(vec, mask);
							Unsafe.WriteUnaligned(ref Unsafe.Add(ref dstRef, i), shuffled);
						}

						for (; i < stride; i += 4)
						{
							dst[i + 0] = src[i + 2]; // R
							dst[i + 1] = src[i + 1]; // G
							dst[i + 2] = src[i + 0]; // B
							dst[i + 3] = src[i + 3]; // A
						}
					}
					else
					{
						int i = 0;
						for (; i <= stride - 16; i += 16)
						{
							dst[i + 0] = src[i + 2];
							dst[i + 1] = src[i + 1];
							dst[i + 2] = src[i + 0];
							dst[i + 3] = src[i + 3];

							dst[i + 4] = src[i + 6];
							dst[i + 5] = src[i + 5];
							dst[i + 6] = src[i + 4];
							dst[i + 7] = src[i + 7];

							dst[i + 8] = src[i + 10];
							dst[i + 9] = src[i + 9];
							dst[i + 10] = src[i + 8];
							dst[i + 11] = src[i + 11];

							dst[i + 12] = src[i + 14];
							dst[i + 13] = src[i + 13];
							dst[i + 14] = src[i + 12];
							dst[i + 15] = src[i + 15];
						}

						for (; i < stride; i += 4)
						{
							dst[i + 0] = src[i + 2];
							dst[i + 1] = src[i + 1];
							dst[i + 2] = src[i + 0];
							dst[i + 3] = src[i + 3];
						}
					}

					deflate.Write(row);
				}
			}

			WriteChunk(writer, "IDAT", ms.WriteTo);
		}

		[MethodImpl(MethodImplOptions.AggressiveOptimization)]
		private static void WriteChunk(BinaryWriter writer, string type, Action<Stream> writeData)
		{
			using var ms = new MemoryStream();
			writeData(ms);

			byte[] data = ms.ToArray();
			byte[] typeBytes = Encoding.ASCII.GetBytes(type);

			writer.Write(BinaryPrimitives.ReverseEndianness(data.Length));
			writer.Write(typeBytes);
			writer.Write(data);

			uint crc = Crc32(typeBytes, data);
			writer.Write(BinaryPrimitives.ReverseEndianness((int)crc));
		}

		[MethodImpl(MethodImplOptions.AggressiveOptimization)]
		private static uint Crc32(byte[] type, byte[] data)
		{
			const uint poly = 0xEDB88320;
			Span<uint> table = stackalloc uint[256];
			for (uint i = 0; i < 256; i++)
			{
				uint c = i;
				for (int k = 0; k < 8; k++)
					c = (c & 1) != 0 ? poly ^ (c >> 1) : c >> 1;
				table[(int)i] = c;
			}

			uint crc = 0xffffffff;
			foreach (byte b in type)
				crc = table[(int)((crc ^ b) & 0xff)] ^ (crc >> 8);
			foreach (byte b in data)
				crc = table[(int)((crc ^ b) & 0xff)] ^ (crc >> 8);
			return crc ^ 0xffffffff;
		}
	}
}