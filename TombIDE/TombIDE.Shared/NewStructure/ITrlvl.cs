namespace TombIDE.Shared.NewStructure
{
	public interface ITrlvl
	{
		string FileFormatVersion { get; }
		int Order { get; set; }

		void WriteToFile(string filePath);
	}
}
