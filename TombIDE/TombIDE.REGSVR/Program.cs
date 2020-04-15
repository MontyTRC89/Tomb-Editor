using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Threading;
using TombIDE.Shared.SharedClasses;

namespace TombIDE.REGSVR
{
	internal static class Program
	{
		private static void Main()
		{
			Console.WriteLine("Installing required COM libraries...\n");
			Thread.Sleep(200);

			RegisterNGCenterLibraries();
		}

		private static void RegisterNGCenterLibraries()
		{
			string systemDirectory = PathHelper.GetSystemDirectory();

			string MSCOMCTL = Path.Combine(systemDirectory, "Mscomctl.ocx");
			string RICHTX32 = Path.Combine(systemDirectory, "Richtx32.ocx");
			string PICFORMAT32 = Path.Combine(systemDirectory, "PicFormat32.ocx");
			string COMDLG32 = Path.Combine(systemDirectory, "Comdlg32.ocx");

			using (FileStream fileStream = File.Create(MSCOMCTL))
				Assembly.GetExecutingAssembly().GetManifestResourceStream("TombIDE.REGSVR.COM.Mscomctl.ocx").CopyTo(fileStream);

			using (FileStream fileStream = File.Create(RICHTX32))
				Assembly.GetExecutingAssembly().GetManifestResourceStream("TombIDE.REGSVR.COM.Richtx32.ocx").CopyTo(fileStream);

			using (FileStream fileStream = File.Create(PICFORMAT32))
				Assembly.GetExecutingAssembly().GetManifestResourceStream("TombIDE.REGSVR.COM.PicFormat32.ocx").CopyTo(fileStream);

			using (FileStream fileStream = File.Create(COMDLG32))
				Assembly.GetExecutingAssembly().GetManifestResourceStream("TombIDE.REGSVR.COM.Comdlg32.ocx").CopyTo(fileStream);

			RegisterCom(MSCOMCTL);

			// The code below is soo fake but people wanted it like that

			Console.WriteLine("Finished installing the MSCOMCTL.OCX library.");
			Thread.Sleep(10);

			RegisterCom(RICHTX32);

			Console.WriteLine("Finished installing the RICHTX32.OCX library.");
			Thread.Sleep(10);

			RegisterCom(PICFORMAT32);

			Console.WriteLine("Finished installing the PICFORMAT32.OCX library.");
			Thread.Sleep(10);

			RegisterCom(COMDLG32);

			Console.WriteLine("Finished installing the COMDLG32.OCX library.");
			Thread.Sleep(10);

			Console.WriteLine("\nAll libraries were installed and registered successfully!");

			Thread.Sleep(1000);
		}

		private static void RegisterCom(string path)
		{
			ProcessStartInfo startInfo = new ProcessStartInfo
			{
				FileName = "regsvr32.exe",
				Arguments = "/s " + "\"" + path + "\""
			};

			Process.Start(startInfo);
		}
	}
}
