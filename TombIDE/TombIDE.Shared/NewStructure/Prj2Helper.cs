using System;
using System.IO;

namespace TombIDE.Shared.NewStructure
{
	public static class Prj2Helper
	{
		public static bool IsBackupFile(string filePath)
		{
			try
			{
				string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(filePath);

				if (fileNameWithoutExtension.EndsWith(".backup", StringComparison.OrdinalIgnoreCase))
					return true;

				// 05-06-23
				if (DateTime.TryParse(fileNameWithoutExtension[..7], out _) || DateTime.TryParse(fileNameWithoutExtension[^7..], out _))
					return true;

				// 05-06-2023 || 2023-06-05
				if (DateTime.TryParse(fileNameWithoutExtension[..9], out _) || DateTime.TryParse(fileNameWithoutExtension[^9..], out _))
					return true;
			}
			catch { }

			return false;
		}
	}
}
