using System.IO;

namespace TombIDE.Shared.NewStructure
{
	public interface ILevelProject : IProject
	{
		/// <summary>
		/// The target .prj2 file name (not path) which should be opened in TombEditor when double-clicking on the level entry on the list.
		/// <para>Set to <see langword="null" /> to get the file name of the most recently modified .prj2 file from the level's directory.</para>
		/// </summary>
		string TargetPrj2FileName { get; set; }

		int Order { get; set; }

		/// <summary>
		/// Returns the path to the .trlvl file of the level. File name should be "project.trlvl".
		/// </summary>
		string GetTrlvlFilePath();

		/// <summary>
		/// Returns all .prj2 files in the level's directory. Includes backup files if specified.
		/// </summary>
		FileInfo[] GetPrj2Files(bool includeBackups = false);

		/// <summary>
		/// Returns the name (not path) of the most recently modified .prj2 file in the level project folder. Excludes backup files.
		/// </summary>
		string GetMostRecentlyModifiedPrj2FileName();

		/// <summary>
		/// Determines whether the level project is stored outside of the game's "Levels" directory.
		/// </summary>
		bool IsExternal(string relativeToLevelsDirectoryPath);
	}
}
