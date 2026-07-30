#nullable enable

using System.Data;
using System.Xml;

namespace TombLib.Scripting.ClassicScript.ReferenceTables;

public sealed class ClassicScriptReferenceTableLoader
{
	public DataTable Load(string xmlPath)
	{
		using var reader = XmlReader.Create(xmlPath);
		using var dataSet = new DataSet();

		dataSet.ReadXml(reader);

		return dataSet.Tables[0] ?? new DataTable();
	}
}
