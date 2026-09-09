#nullable enable

using System;
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
	private readonly ClassicScriptLanguageServices? _languageServices;
	private readonly GameFlowLanguageServices? _gameFlowLanguageServices;
	private readonly TRXLanguageServices? _trxLanguageServices;

	public DocumentOutlineNodesProviderFactory(
		ClassicScriptLanguageServices? languageServices,
		GameFlowLanguageServices? gameFlowLanguageServices,
		TRXLanguageServices? trxLanguageServices)
	{
		_languageServices = languageServices;
		_gameFlowLanguageServices = gameFlowLanguageServices;
		_trxLanguageServices = trxLanguageServices;
	}

	public ContentNodesProviderBase? CreateClassicScript()
		=> _languageServices is null ? null : new ClassicScriptNodesProvider(_languageServices.LineService);

	public ContentNodesProviderBase? CreateStrings()
		=> _languageServices is null ? null : new StringFileNodesProvider(_languageServices.LineService);

	public ContentNodesProviderBase? CreateGameFlowScript()
		=> _gameFlowLanguageServices is null ? null : new GameFlowNodesProvider(_gameFlowLanguageServices.LineService);

	public ContentNodesProviderBase? CreateTrx()
		=> _trxLanguageServices is null ? null : new TRXNodesProvider(_trxLanguageServices.LineService);
}
