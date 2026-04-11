using System;

namespace TombLib.Scripting.Lua.Objects
{
	public sealed class LuaDefinitionLocation
	{
		public LuaDefinitionLocation(string filePath, int lineNumber, int columnNumber)
		{
			FilePath = filePath ?? throw new ArgumentNullException(nameof(filePath));
			LineNumber = lineNumber;
			ColumnNumber = columnNumber;
		}

		public string FilePath { get; }
		public int LineNumber { get; }
		public int ColumnNumber { get; }
	}
}