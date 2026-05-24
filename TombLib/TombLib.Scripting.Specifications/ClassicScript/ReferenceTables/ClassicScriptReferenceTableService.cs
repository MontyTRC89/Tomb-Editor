#nullable enable

using System;
using System.Data;

namespace TombLib.Scripting.Specifications.ClassicScript.ReferenceTables;

public sealed class ClassicScriptReferenceTableService
{
	private readonly ClassicScriptReferenceTableLoader _loader = new();

	public DataTable GetTable(string tableName)
	{
		ArgumentNullException.ThrowIfNull(tableName);

		return _loader.Load(ClassicScriptResourcePaths.GetReferenceTablePath(tableName + ".xml"));
	}
}