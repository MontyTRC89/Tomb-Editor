using System;
using System.IO;
using System.Reflection;
using System.Threading;
using TombIDE.Shared;

namespace TombIDE.REGSVR
{
	internal class Program
	{
		private static void Main()
		{
			Console.WriteLine("Installing required COM libraries...");
			Thread.Sleep(500);

			RegisterNGCenterLibraries();
		}

		private static void RegisterNGCenterLibraries()
		{
			string MSCOMCTL = Path.Combine(SharedMethods.GetSystemDirectory(), "Mscomctl.ocx");
			string RICHTX32 = Path.Combine(SharedMethods.GetSystemDirectory(), "Richtx32.ocx");
			string PICFORMAT32 = Path.Combine(SharedMethods.GetSystemDirectory(), "PicFormat32.ocx");
			string COMDLG32 = Path.Combine(SharedMethods.GetSystemDirectory(), "Comdlg32.ocx");

			using (FileStream fileStream = File.Create(MSCOMCTL))
				Assembly.GetExecutingAssembly().GetManifestResourceStream("TombIDE.REGSVR.COM.Mscomctl.ocx").CopyTo(fileStream);

			using (FileStream fileStream = File.Create(RICHTX32))
				Assembly.GetExecutingAssembly().GetManifestResourceStream("TombIDE.REGSVR.COM.Richtx32.ocx").CopyTo(fileStream);

			using (FileStream fileStream = File.Create(PICFORMAT32))
				Assembly.GetExecutingAssembly().GetManifestResourceStream("TombIDE.REGSVR.COM.PicFormat32.ocx").CopyTo(fileStream);

			using (FileStream fileStream = File.Create(COMDLG32))
				Assembly.GetExecutingAssembly().GetManifestResourceStream("TombIDE.REGSVR.COM.Comdlg32.ocx").CopyTo(fileStream);

			Registration.RegisterCom(MSCOMCTL);
			Registration.RegisterCom(RICHTX32);
			Registration.RegisterCom(PICFORMAT32);
			Registration.RegisterCom(COMDLG32);
		}
	}
}
