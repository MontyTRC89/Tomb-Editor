#nullable enable

using System.Data;

namespace TombLib.Scripting.ClassicScript.ReferenceTables;

public sealed class ClassicScriptReferenceTableService
{
	private readonly ClassicScriptReferenceTableLoader _loader = new();

	public DataTable GetTable(string tableName)
	{
		ArgumentNullException.ThrowIfNull(tableName);

		return _loader.Load(ClassicScriptResourcePaths.GetReferenceTablePath(tableName + ".json"));
	}
}
