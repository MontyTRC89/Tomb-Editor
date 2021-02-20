using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using TombLib.Scripting.ClassicScript.Controls;

namespace TombLib.Scripting.ClassicScript.Utils
{
	public static class ContentBuilder
	{
		public static string BuildContent(StringDataGridView[] dataGrids)
		{
			var builder = new StringBuilder();

			builder.AppendLine($"; Automatically generated document using TombIDE {Application.ProductVersion}");
			builder.AppendLine($"; Do not add any comments into this document as they are");
			builder.AppendLine($"; going to be removed next time the file is regenerated.");

			foreach (StringDataGridView dataGrid in dataGrids)
			{
				bool isExtraNG = Regex.IsMatch(dataGrid.Name, @"^\[ExtraNG\]", RegexOptions.IgnoreCase);

				builder.AppendLine($"\n{dataGrid.Name}");

				foreach (DataGridViewRow row in dataGrid.Rows)
				{
					string line = GetParsedLine(row, isExtraNG);
					builder.AppendLine(line);
				}
			}

			return builder.ToString();
		}

		private static string GetParsedLine(DataGridViewRow row, bool isExtraNG = false)
		{
			string @string = GetParsedString(row.Cells[2].Value?.ToString());

			if (isExtraNG)
			{
				string id = row.Cells[0].Value?.ToString();
				return string.IsNullOrEmpty(id) ? string.Empty : $"{id}: {@string}";
			}
			else
				return @string;
		}

		private static string GetParsedString(string @string)
			=> HandleNewLine(EscapeComments(@string));

		private static string EscapeComments(string @string)
			=> @string?.Replace(";", "\\x3B");

		private static string HandleNewLine(string @string)
			=> @string?.Replace("\r", string.Empty).Replace("\n", "\\n");
	}
}
