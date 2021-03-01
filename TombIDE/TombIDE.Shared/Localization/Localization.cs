using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Xml.Serialization;

namespace TombIDE.Shared.Local
{
	public class Localization
	{
		public string AskRestoreSession { get; set; }
		public string AskFileReload { get; set; }
		public string AskMoveFileToBin { get; set; }
		public string AskMoveFolderToBin { get; set; }
		public string AskUnsavedChanged { get; set; }

		public string AreYouSure { get; set; }
		public string RestoreSessionMBT { get; set; }
		public string FileReloadMBT { get; set; }
		public string UnsavedChangedMBT { get; set; }

		public string Error { get; set; }

		public string File { get; set; }
		public string Edit { get; set; }
		public string Document { get; set; }
		public string Options { get; set; }
		public string View { get; set; }
		public string Help { get; set; }

		public string NewFile { get; set; }
		public string Save { get; set; }
		public string SaveAs { get; set; }
		public string SaveAll { get; set; }
		public string Build { get; set; }

		public string Undo { get; set; }
		public string CantUndo { get; set; }
		public string Redo { get; set; }
		public string CantRedo { get; set; }
		public string Cut { get; set; }
		public string Copy { get; set; }
		public string Paste { get; set; }
		public string FindReplace { get; set; }
		public string SelectAll { get; set; }

		public string Reindent { get; set; }
		public string TrimWhitespace { get; set; }
		public string CommentOut { get; set; }
		public string Uncomment { get; set; }
		public string ToggleBookmark { get; set; }
		public string PrevBookmark { get; set; }
		public string NextBookmark { get; set; }
		public string ClearBookmarks { get; set; }

		public string UseNewInclude { get; set; }
		public string ShowLogsAfterBuild { get; set; }
		public string ReindentOnSave { get; set; }
		public string Settings { get; set; }

		public string ContentExplorer { get; set; }
		public string FileExplorer { get; set; }
		public string ReferenceBrowser { get; set; }
		public string CompilerLogs { get; set; }
		public string SearchResults { get; set; }
		public string ToolStrip { get; set; }
		public string StatusStrip { get; set; }

		public string About { get; set; }

		public string Close { get; set; }
		public string OpenContainingFolder { get; set; }

		public string Untitled { get; set; }

		public string SearchContent { get; set; }
		public string SearchFunctions { get; set; }
		public string SearchFiles { get; set; }
		public string SearchReferences { get; set; }

		public string Sections { get; set; }
		public string Levels { get; set; }
		public string Includes { get; set; }
		public string Defines { get; set; }

		public string Row { get; set; }
		public string Column { get; set; }
		public string Line { get; set; }
		public string Selected { get; set; }
		public string Zoom { get; set; }
		public string ResetZoom { get; set; }

		public string DecimalValue { get; set; }
		public string HexadecimalValue { get; set; }
		public string Macro { get; set; }
		public string ArgumentRange { get; set; }
		public string Variable { get; set; }
		public string Description { get; set; }
		public string Sounds { get; set; }

		public string MnemonicConstants { get; set; }
		public string EnemyDamageValues { get; set; }
		public string KeyboardScancodes { get; set; }
		public string OCBList { get; set; }
		public string OldCommandsList { get; set; }
		public string NewCommandsList { get; set; }
		public string SoundIndices { get; set; }
		public string MoveableSlotIndices { get; set; }
		public string StaticObjectIndices { get; set; }
		public string VariablePlaceholders { get; set; }

		public string NewFolder { get; set; }
		public string ViewInDefaultEditor { get; set; }
		public string ViewCode { get; set; }
		public string RenameItem { get; set; }
		public string DeleteItem { get; set; }
		public string OpenItemInExplorer { get; set; }

		public string MatchSourceNodeText { get; set; }
		public string SingleMatchNodeText { get; set; }

		public static Localization GetLocalization(Language language)
		{
			string file = Path.Combine(DefaultPaths.LocalizationDirectory, "EN", "TombIDE.xml");

			using (var reader = new StreamReader(file))
			{
				var serializer = new XmlSerializer(typeof(Localization));
				var localization = serializer.Deserialize(reader) as Localization;

				NormalizeFieldValues(localization);

				return localization;
			}
		}

		private static void NormalizeFieldValues(Localization localization)
		{
			var fields = localization.GetType().GetRuntimeFields();

			foreach (FieldInfo field in fields)
			{
				string fieldValue = field.GetValue(localization).ToString();
				string newValue = GetNormalizedValue(fieldValue);

				field.SetValue(localization, newValue);
			}
		}

		private static string GetNormalizedValue(string fieldValue)
		{
			List<string> lines = fieldValue.Split('\n').ToList();

			if (lines.First().Trim() == string.Empty)
				lines.RemoveAt(0);

			if (lines.Count > 0 && lines.Last().Trim() == string.Empty)
				lines.RemoveAt(lines.Count - 1);

			for (int i = 0; i < lines.Count; i++)
				lines[i] = lines[i].Trim();

			return string.Join(Environment.NewLine, lines);
		}
	}

	public enum Language
	{
		English
	}
}
