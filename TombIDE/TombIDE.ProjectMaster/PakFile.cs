using System.IO;
using System.Linq;
using TombLib.Utils;

namespace TombIDE.ProjectMaster
{
	public class PakFile
	{
		public static byte[] GetDecompressedData(string pakFilePath)
		{
			using (FileStream stream = new FileStream(pakFilePath, FileMode.Open, FileAccess.Read))
			{
				byte[] bytes = new byte[stream.Length];
				stream.Read(bytes, 0, (int)stream.Length);
				bytes = bytes.Skip(4).ToArray();

				return ZLib.DecompressData(bytes);
			}
		}

		public static void SavePakFile(string pakFilePath, byte[] rawImageData)
		{
			byte[] pakFileData = CompressData(rawImageData);
			File.WriteAllBytes(pakFilePath, pakFileData);
		}

		private static byte[] CompressData(byte[] data)
		{
			byte[] prefix = { 0x00, 0x00, 0x06, 0x00 }; // These bytes are important, otherwise the game won't launch
			byte[] compressedData = ZLib.CompressData(data);

			return prefix.Concat(compressedData).ToArray();
		}
	}
}
