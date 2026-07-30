using System.Data;
using TombLib.Scripting.ClassicScript.Mnemonics;
using TombLib.Scripting.ClassicScript.ReferenceTables;

namespace TombIDE.ScriptingStudio.ClassicScript;

internal sealed class ClassicScriptReferenceBrowserDataProvider
{
	private readonly ClassicScriptMnemonicCatalogService _mnemonicCatalogService = new();
	private readonly ClassicScriptReferenceTableService _referenceTableService = new();

	public DataTable GetTable(ReferenceItemType itemType)
	{
		if (itemType == ReferenceItemType.MnemonicConstants)
			return _mnemonicCatalogService.CreateMnemonicTable();

		return _referenceTableService.GetTable(itemType.ToString());
	}
}
