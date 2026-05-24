using System;
using System.Threading.Tasks;
using TombIDE.ScriptingStudio.CommandSurface;
using TombIDE.ScriptingStudio.UI;
using TombLib.Scripting.GameFlowScript;

namespace TombIDE.ScriptingStudio.GameFlowScript;

public sealed class GameFlowDocumentCommandHandler : IStudioDocumentCommandHandler
{
	private readonly GameFlowDocumentCommandCallbacks _callbacks;

	public GameFlowDocumentCommandHandler(GameFlowDocumentCommandCallbacks callbacks)
	{
		_callbacks = callbacks ?? throw new ArgumentNullException(nameof(callbacks));
	}

	public bool TryHandle(UICommand command)
	{
		if (command == UICommand.Tomb3ExtraCommands)
		{
			_callbacks.ShowExtraCommandsDocumentation();
			return true;
		}

		if (_callbacks.GetCurrentEditor() is not GameFlowEditor editor)
			return false;

		if (command == UICommand.Reindent || command == UICommand.TrimWhiteSpace)
		{
			_ = _callbacks.FormatAsync(editor);
			return true;
		}

		return false;
	}
}

public sealed record GameFlowDocumentCommandCallbacks(
	Func<GameFlowEditor> GetCurrentEditor,
	Func<GameFlowEditor, Task> FormatAsync,
	Action ShowExtraCommandsDocumentation);