using System;
using TombLib.Scripting.Enums;

namespace TombIDE.ScriptingStudio.Objects
{
	public class FileOpenedEventArgs : EventArgs
	{
		public string FilePath { get; }
		public EditorType EditorType { get; }

		public FileOpenedEventArgs(string filePath, EditorType editorType = EditorType.Default)
		{
			FilePath = filePath;
			EditorType = editorType;
		}
	}
}
