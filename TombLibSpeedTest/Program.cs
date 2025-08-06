using K4os.Compression.LZ4;
using System.Diagnostics;
using TombLib.Utils;

namespace TombLibSpeedTest
{
	internal class Program
	{
		static void Main(string[] args)
		{
			string path = "D:\\Walrus\\Spring\\Spring\\Textures\\Temple\\DefaultMaterial_2D_View.png";

			ImageC image = ImageC.FromFile("D:\\Walrus\\Spring\\Spring\\Textures\\Temple\\DefaultMaterial_2D_View.png");
			
			var sw = new Stopwatch();
			int count = 20;

			/*float average = 0;
			for (int i=0;i<count;i++)
			{
				sw.Restart();
				image.Save("D:\\test.png");
				sw.Stop();
				average += sw.ElapsedMilliseconds;
			}
			average /= count;


			Console.WriteLine("System.Drawing: " + average + " ms");

			average = 0;
			for (int i = 0; i < count; i++)
			{
				sw.Restart();
				var s = File.OpenWrite("D:\\test.png");
				image.SaveToPngFast(s);
				s.Close();
				sw.Stop();
				average += sw.ElapsedMilliseconds;
			}
			average /= count;

			Console.WriteLine("Fast PNG writer: " + average + " ms");
			*/
			byte[] data = File.ReadAllBytes(path);

			float average = 0;
			int size = 0;
			for (int i = 0; i < count; i++)
			{
				sw.Restart();
				var output=ZLib.CompressData(data, System.IO.Compression.CompressionLevel.Fastest);
				size = output.Length;
				sw.Stop();
				average += sw.ElapsedMilliseconds;
			}
			average /= count;


			Console.WriteLine("ZLIB: " + average + " ms, size = " + size / 1024 + " KB");

			average = 0;
			for (int i = 0; i < count; i++)
			{
				sw.Restart();
				var output = LZ4.CompressData(data, System.IO.Compression.CompressionLevel.Fastest);
				size = output.Length;
				sw.Stop();
				average += sw.ElapsedMilliseconds;
			}
			average /= count;

			Console.WriteLine("LZ4: " + average + " ms, size = " + size / 1024 + " KB");

			Console.ReadKey();
		}
	}
}
