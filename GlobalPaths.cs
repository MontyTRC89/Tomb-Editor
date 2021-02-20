using System;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;

internal static class DefaultPaths
{
	public static string ProgramDirectory => Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

	#region Configs

	public static string ConfigsDirectory => Path.Combine(ProgramDirectory, "Configs");
	public static string TextEditorConfigsDirectory => Path.Combine(ConfigsDirectory, "TextEditors");
	public static string ColorSchemesDirectory => Path.Combine(TextEditorConfigsDirectory, "ColorSchemes");

	public static string ClassicScriptColorConfigsDirectory => Path.Combine(ColorSchemesDirectory, "ClassicScript");
	public static string LuaColorConfigsDirectory => Path.Combine(ColorSchemesDirectory, "Lua");

	#endregion Configs

	#region Localization

	public static string LocalizationDirectory => Path.Combine(ProgramDirectory, "Localization");

	#endregion Localization

	#region Resources

	public static string ResourcesDirectory => Path.Combine(ProgramDirectory, "Resources");
	public static string ReferencesDirectory => Path.Combine(ResourcesDirectory, "ClassicScript");
	public static string ReferenceDescriptionsDirectory => Path.Combine(ReferencesDirectory, "Descriptions");

	#endregion Resources

	#region TIDE

	public static string TIDEDirectory => Path.Combine(ProgramDirectory, "TIDE");

	public static string InternalNGCDirectory => Path.Combine(TIDEDirectory, "NGC");
	public static string VGEDirectory => Path.Combine(InternalNGCDirectory, "VGE");
	public static string VGEScriptDirectory => Path.Combine(VGEDirectory, "Script");

	public static string TemplatesDirectory => Path.Combine(TIDEDirectory, "Templates");
	public static string EngineTemplatesDirectory => Path.Combine(TemplatesDirectory, "Engines");

	public static string TRNGPluginsDirectory => Path.Combine(TIDEDirectory, "TRNG Plugins");

	#endregion TIDE

	#region .EXEs

	public static string TombEditorExecutable => Path.Combine(ProgramDirectory, "TombEditor.exe");
	public static string WadToolExecutable => Path.Combine(ProgramDirectory, "WadTool.exe");
	public static string SoundToolExecutable => Path.Combine(ProgramDirectory, "SoundTool.exe");
	public static string TombIDEExecutable => Path.Combine(ProgramDirectory, "TombIDE.exe");

	public static string LibraryRegistrationExecutable => Path.Combine(ProgramDirectory, "TombIDE Library Registration.exe");
	public static string FileAssociationExecutable => Path.Combine(ProgramDirectory, "TombLib File Association.exe");

	public static string NGCExecutable => Path.Combine(InternalNGCDirectory, "NG_Center.exe");

	#endregion .EXEs

	#region System

	public static string MscomctlSystemFile => Path.Combine(SystemDirectory, "Mscomctl.ocx");
	public static string Richtx32SystemFile => Path.Combine(SystemDirectory, "Richtx32.ocx");
	public static string PicFormat32SystemFile => Path.Combine(SystemDirectory, "PicFormat32.ocx");
	public static string Comdlg32SystemFile => Path.Combine(SystemDirectory, "Comdlg32.ocx");

	/// <summary>
	/// Returns either the "System32" path or the "SysWOW64" path.
	/// </summary>
	public static string SystemDirectory
	{
		get
		{
			StringBuilder path = new StringBuilder(260);
			SHGetSpecialFolderPath(IntPtr.Zero, path, 0x0029, false);

			return path.ToString();
		}
	}

	#endregion System

	#region Native methods

	[DllImport("shell32.dll")]
	private static extern bool SHGetSpecialFolderPath(IntPtr hwndOwner, [Out] StringBuilder lpszPath, int nFolder, bool fCreate);

	#endregion Native methods
}
