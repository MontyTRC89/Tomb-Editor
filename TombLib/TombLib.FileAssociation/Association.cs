using Microsoft.Win32;
using System;
using System.IO;

namespace TombLib.FileAssociation
{
	internal class Association
	{
		private const string FileExtsRegistryPath = @"Software\Microsoft\Windows\CurrentVersion\Explorer\FileExts\";
		private const string BadExtensionMessage = "The extension has to start with a '.' character.";

		#region Association checks

		public static bool IsPRJ2Associated()
			=> IsAssociated(".prj2", DefaultPaths.TombEditorExecutable);

		public static bool IsWAD2Associated()
			=> IsAssociated(".wad2", DefaultPaths.WadToolExecutable);

		public static bool IsTRPROJAssociated()
			=> IsAssociated(".trproj", DefaultPaths.TombIDEExecutable);

		/// <exception cref="ArgumentException" />
		public static bool IsAssociated(string extension, string openWith)
		{
			if (!extension.StartsWith("."))
				throw new ArgumentException(BadExtensionMessage);

			string openMethodKeyName = GetOpenMethodKeyName(extension);

			if (string.IsNullOrEmpty(openMethodKeyName))
				return false;

			string openCommandValue = GetOpenCommandValue(openMethodKeyName);

			if (string.IsNullOrEmpty(openCommandValue))
				return false;

			if (!openCommandValue.Equals($"\"{openWith}\" \"%1\"", StringComparison.OrdinalIgnoreCase))
				return false;

			string progId = GetProgId(extension);

			if (string.IsNullOrEmpty(progId))
				return true; // No UserChoice registry was found

			return progId == openMethodKeyName;
		}

		private static string GetOpenMethodKeyName(string extension)
		{
			using (RegistryKey extensionKey = Registry.ClassesRoot.OpenSubKey(extension))
				return extensionKey?.GetValue("")?.ToString();
		}

		private static string GetOpenCommandValue(string openMethodkeyName)
		{
			using (RegistryKey openMethodKey = Registry.ClassesRoot.OpenSubKey(openMethodkeyName))
			using (RegistryKey shellKey = openMethodKey?.OpenSubKey("Shell"))
			using (RegistryKey openKey = shellKey?.OpenSubKey("open"))
			using (RegistryKey commandKey = openKey?.OpenSubKey("command"))
				return commandKey?.GetValue("")?.ToString();
		}

		private static string GetProgId(string extension)
		{
			using (RegistryKey extensionKey = Registry.CurrentUser.OpenSubKey(FileExtsRegistryPath + extension))
			using (RegistryKey userChoiceKey = extensionKey?.OpenSubKey("UserChoice"))
				return userChoiceKey?.GetValue("ProgId")?.ToString();
		}

		#endregion Association checks

		#region Association

		public static void AssociatePRJ2()
		{
			string extension = ".prj2";
			string keyName = "TombEditor";
			string openWith = DefaultPaths.TombEditorExecutable;
			string description = "TombEditor Project File";
			string iconPath = Path.Combine(DefaultPaths.ResourcesDirectory, "te_file.ico");

			SetAssociation(extension, keyName, openWith, description, iconPath);
		}

		public static void AssociateWAD2()
		{
			string extension = ".wad2";
			string keyName = "WadTool";
			string openWith = DefaultPaths.WadToolExecutable;
			string description = "Wad2 Object File";
			string iconPath = Path.Combine(DefaultPaths.ResourcesDirectory, "wt_file.ico");

			SetAssociation(extension, keyName, openWith, description, iconPath);
		}

		public static void AssociateTRPROJ()
		{
			string extension = ".trproj";
			string keyName = "TombIDE";
			string openWith = DefaultPaths.TombIDEExecutable;
			string description = "TombIDE Project File";
			string iconPath = Path.Combine(DefaultPaths.ResourcesDirectory, "tide_file.ico");

			SetAssociation(extension, keyName, openWith, description, iconPath);
		}

		/// <summary>
		/// WARNING: Method requires admin privileges.
		/// </summary>
		/// <exception cref="ArgumentException" />
		public static void SetAssociation(string extension, string keyName, string openWith, string fileDescription, string iconPath = null)
		{
			if (!extension.StartsWith("."))
				throw new ArgumentException(BadExtensionMessage);

			if (string.IsNullOrEmpty(iconPath))
				iconPath = openWith;

			using (RegistryKey extensionKey = Registry.ClassesRoot.CreateSubKey(extension))
				extensionKey.SetValue("", keyName);

			using (RegistryKey openMethodKey = Registry.ClassesRoot.CreateSubKey(keyName))
			{
				openMethodKey.SetValue("", fileDescription);

				using (RegistryKey defaultIconKey = openMethodKey.CreateSubKey("DefaultIcon"))
					defaultIconKey.SetValue("", $"\"{iconPath}\", 0");

				using (RegistryKey shellKey = openMethodKey.CreateSubKey("Shell"))
				using (RegistryKey openKey = shellKey.CreateSubKey("open"))
				using (RegistryKey commandKey = openKey.CreateSubKey("command"))
					commandKey.SetValue("", $"\"{openWith}\" \"%1\"");
			}

			// Delete the UserChoice key
			using (RegistryKey extensionKey = Registry.CurrentUser.OpenSubKey(FileExtsRegistryPath + extension, true))
				extensionKey?.DeleteSubKey("UserChoice", false);

			// Tell the explorer that the file association has been changed
			NativeMethods.SHChangeNotify(0x08000000, 0x0000, IntPtr.Zero, IntPtr.Zero);
		}

		/// <summary>
		/// WARNING: Method requires admin privileges.
		/// </summary>
		/// <exception cref="ArgumentException" />
		public static void RemoveAssociation(string extension)
		{
			if (!extension.StartsWith("."))
				throw new ArgumentException(BadExtensionMessage);

			Registry.ClassesRoot.DeleteSubKey(extension, false);

			using (RegistryKey fileExtsKey = Registry.CurrentUser.OpenSubKey(FileExtsRegistryPath, true))
				fileExtsKey.DeleteSubKeyTree(extension, false);

			// Tell the explorer that the file association has been changed
			NativeMethods.SHChangeNotify(0x08000000, 0x0000, IntPtr.Zero, IntPtr.Zero);
		}

		#endregion Association
	}
}
