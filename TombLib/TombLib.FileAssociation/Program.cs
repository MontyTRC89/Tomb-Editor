using System;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace TombLib.FileAssociation
{
	internal static class Program
	{
		/// <summary>
		/// The main entry point for the application.
		/// </summary>
		[STAThread]
		private static void Main(string[] args)
		{
			if (args.Length > 0)
			{
				if (args[0].Equals("-a", StringComparison.OrdinalIgnoreCase))
					RunAssociation();
				else if (args[0].Equals("-d", StringComparison.OrdinalIgnoreCase))
					RunDeassociation();
				else if (Regex.IsMatch(args[0], @"-\d\d\d"))
					RunAssociationBinaryArgs(args[0]);
			}
			else
				OpenGUI();
		}

		private static void OpenGUI()
		{
			Application.EnableVisualStyles();
			Application.SetCompatibleTextRenderingDefault(false);
			Application.Run(new FormMain());
		}

		private static void RunAssociationBinaryArgs(string args)
		{
			bool associatePRJ2 = int.Parse(args[1].ToString()) != 0;
			bool associateWAD2 = int.Parse(args[2].ToString()) != 0;
			bool associateTRPROJ = int.Parse(args[3].ToString()) != 0;

			if (associatePRJ2)
				Association.AssociatePRJ2();

			if (associateWAD2)
				Association.AssociateWAD2();

			if (associateTRPROJ)
				Association.AssociateTRPROJ();
		}

		private static void RunAssociation()
		{
			if (!Association.IsPRJ2Associated())
				AskAssociatePRJ2();

			if (!Association.IsWAD2Associated())
				AskAssociateWAD2();

			if (!Association.IsTRPROJAssociated())
				AskAssociateTRPROJ();
		}

		private static void RunDeassociation() // Is this even a real word?
		{
			Association.RemoveAssociation(".prj2");
			Association.RemoveAssociation(".wad2");
			Association.RemoveAssociation(".trproj");
		}

		private static void AskAssociatePRJ2()
		{
			DialogResult result = MessageBox.Show(
				"Would you like to associate .prj2 files with TombEditor?", "Associate?",
				MessageBoxButtons.YesNo, MessageBoxIcon.Question);

			if (result == DialogResult.Yes)
				Association.AssociatePRJ2();
		}

		private static void AskAssociateWAD2()
		{
			DialogResult result = MessageBox.Show(
				"Would you like to associate .wad2 files with WadTool?", "Associate?",
				MessageBoxButtons.YesNo, MessageBoxIcon.Question);

			if (result == DialogResult.Yes)
				Association.AssociateWAD2();
		}

		private static void AskAssociateTRPROJ()
		{
			DialogResult result = MessageBox.Show(
				"Would you like to associate .trproj files with TombIDE?", "Associate?",
				MessageBoxButtons.YesNo, MessageBoxIcon.Question);

			if (result == DialogResult.Yes)
				Association.AssociateTRPROJ();
		}
	}
}
