#nullable enable

using TombIDE.ScriptingStudio.UI;
using TombLib.Scripting.ClassicScript.ContentNodes;
using TombLib.Scripting.GameFlowScript.ContentNodes;
using TombLib.Scripting.TRX.ContentNodes;
using TombLib.Scripting.UI.ContentNodes;

namespace TombIDE.ScriptingStudio.DocumentOutline;

internal sealed class ContentNodesProviderFactory
{
	public ContentNodesProviderBase? Create(DocumentMode documentMode) => documentMode switch
	{
		DocumentMode.ClassicScript => new ClassicScriptNodesProvider(),
		DocumentMode.GameFlowScript => new GameFlowNodesProvider(),
		DocumentMode.TRX => new TRXNodesProvider(),
		DocumentMode.Strings => new StringFileNodesProvider(),
		_ => null
	};
}