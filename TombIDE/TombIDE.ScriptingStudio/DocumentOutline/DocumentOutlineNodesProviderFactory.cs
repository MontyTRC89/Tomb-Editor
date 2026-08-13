#nullable enable

using System;
using TombIDE.ScriptingStudio.UI;
using TombLib.Scripting.ClassicScript;
using TombLib.Scripting.ClassicScript.ContentNodes;
using TombLib.Scripting.GameFlowScript;
using TombLib.Scripting.GameFlowScript.ContentNodes;
using TombLib.Scripting.TRX;
using TombLib.Scripting.TRX.ContentNodes;
using TombLib.Scripting.UI.ContentNodes;

namespace TombIDE.ScriptingStudio.DocumentOutline;

internal sealed class DocumentOutlineNodesProviderFactory
{
	private readonly ClassicScriptLanguageServices _languageServices;
	private readonly GameFlowLanguageServices _gameFlowLanguageServices;
	private readonly TRXLanguageServices _trxLanguageServices;

	public DocumentOutlineNodesProviderFactory(
		ClassicScriptLanguageServices languageServices,
		GameFlowLanguageServices gameFlowLanguageServices,
		TRXLanguageServices trxLanguageServices)
	{
		ArgumentNullException.ThrowIfNull(languageServices);
		ArgumentNullException.ThrowIfNull(gameFlowLanguageServices);
		ArgumentNullException.ThrowIfNull(trxLanguageServices);

		_languageServices = languageServices;
		_gameFlowLanguageServices = gameFlowLanguageServices;
		_trxLanguageServices = trxLanguageServices;
	}

	public ContentNodesProviderBase? Create(DocumentMode documentMode) => documentMode switch
	{
		DocumentMode.ClassicScript => new ClassicScriptNodesProvider(_languageServices.LineService),
		DocumentMode.GameFlowScript => new GameFlowNodesProvider(_gameFlowLanguageServices.LineService),
		DocumentMode.TRX => new TRXNodesProvider(_trxLanguageServices.LineService),
		DocumentMode.Strings => new StringFileNodesProvider(_languageServices.LineService),
		_ => null
	};
}
