using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TombIDE.ScriptingStudio.CommandSurface;
using TombIDE.ScriptingStudio.UI;
using TombLib.Scripting.GameFlowScript;

namespace TombIDE.ScriptingStudio.GameFlowScript;

public sealed class GameFlowDocumentCommandHandler : IStudioDocumentCommandHandler
{
	private readonly GameFlowDocumentCommandCallbacks _callbacks;
	private readonly IReadOnlyDictionary<UICommand, Action<GameFlowEditor>> _editorHandlers;
	private readonly IReadOnlyDictionary<UICommand, Action> _globalHandlers;

	public GameFlowDocumentCommandHandler(GameFlowDocumentCommandCallbacks callbacks)
	{
		_callbacks = callbacks ?? throw new ArgumentNullException(nameof(callbacks));
		_globalHandlers = new Dictionary<UICommand, Action>
		{
			[UICommand.Tomb3ExtraCommands] = _callbacks.ShowExtraCommandsDocumentation
		};
		_editorHandlers = new Dictionary<UICommand, Action<GameFlowEditor>>
		{
			[UICommand.Reindent] = StudioDocumentCommandDispatcher.Run(_callbacks.FormatAsync),
			[UICommand.TrimWhiteSpace] = StudioDocumentCommandDispatcher.Run(_callbacks.FormatAsync)
		};
	}

	public bool TryHandle(UICommand command)
		=> StudioDocumentCommandDispatcher.TryHandle(command, _globalHandlers)
			|| StudioDocumentCommandDispatcher.TryHandle(command, _callbacks.GetCurrentEditor, _editorHandlers);
}

public sealed record GameFlowDocumentCommandCallbacks(
	Func<GameFlowEditor> GetCurrentEditor,
	Func<GameFlowEditor, Task> FormatAsync,
	Action ShowExtraCommandsDocumentation);
