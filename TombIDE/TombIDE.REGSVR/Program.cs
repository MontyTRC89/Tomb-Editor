using System.IO;
using TombIDE.Shared;

namespace TombIDE.REGSVR
{
	internal class Program
	{
		private static void Main(string[] args)
		{
			RegisterNGCenterLibraries();
		}

		private static void RegisterNGCenterLibraries()
		{
			string MSCOMCTL = Path.Combine(SharedMethods.GetSystemDirectory(), "Mscomctl.ocx");
			string RICHTX32 = Path.Combine(SharedMethods.GetSystemDirectory(), "Richtx32.ocx");
			string PICFORMAT32 = Path.Combine(SharedMethods.GetSystemDirectory(), "PicFormat32.ocx");
			string COMDLG32 = Path.Combine(SharedMethods.GetSystemDirectory(), "Comdlg32.ocx");

			File.Copy(@"COM\Mscomctl.ocx", MSCOMCTL, true);
			File.Copy(@"COM\Richtx32.ocx", RICHTX32, true);
			File.Copy(@"COM\PicFormat32.ocx", PICFORMAT32, true);
			File.Copy(@"COM\Comdlg32.ocx", COMDLG32, true);

			Registration.RegisterCom(MSCOMCTL);
			Registration.RegisterCom(RICHTX32);
			Registration.RegisterCom(PICFORMAT32);
			Registration.RegisterCom(COMDLG32);
		}
	}
}
