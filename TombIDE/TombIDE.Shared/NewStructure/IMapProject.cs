namespace TombIDE.Shared.NewStructure
{
	public interface IMapProject : IProject
	{
		/// <summary>
		/// The target .prj2 file which should be opened in TombEditor when double-clicking on the map entry on the list.
		/// </summary>
		string TargetPrj2FilePath { get; }

		/// <summary>
		/// Returns the path to the .trmap file of the map. File name should be "project.trmap".
		/// </summary>
		string GetTrmapFilePath();

		/// <summary>
		/// Returns paths of all .prj2 files in the project folder. Includes backup files if specified.
		/// </summary>
		string[] GetPrj2FilePaths(bool includeBackups = false);
	}
}
