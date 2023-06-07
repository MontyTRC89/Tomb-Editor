using System.IO;

namespace TombIDE.Shared.NewStructure
{
	public interface ILevelProject : IProject
	{
		/// <summary>
		/// The target .prj2 file name (not path) which should be opened in TombEditor when double-clicking on the map entry on the list.
		/// <para>Set to <see langword="null" /> to get the file name of the most recently modified .prj2 file from the map's directory.</para>
		/// </summary>
		string TargetPrj2FileName { get; set; }

		/// <summary>
		/// Returns the path to the .trmap file of the map. File name should be "project.trmap".
		/// </summary>
		string GetTrmapFilePath();

		/// <summary>
		/// Returns all .prj2 files in the map's directory. Includes backup files if specified.
		/// </summary>
		FileInfo[] GetPrj2Files(bool includeBackups = false);

		/// <summary>
		/// Returns the name (not path) of the most recently modified .prj2 file in the map project folder. Excludes backup files.
		/// </summary>
		string GetMostRecentlyModifiedPrj2FileName();

		/// <summary>
		/// Determines whether the level project is stored outside of the game's "Levels" directory.
		/// </summary>
		bool IsExternal(string relativeToLevelsDirectoryPath);
	}
}
