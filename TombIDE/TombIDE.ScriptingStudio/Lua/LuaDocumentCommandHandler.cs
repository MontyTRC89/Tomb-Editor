using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TombIDE.ScriptingStudio.CommandSurface;
using TombIDE.ScriptingStudio.UI;

namespace TombIDE.ScriptingStudio.Lua;

public sealed class LuaDocumentCommandHandler : IStudioDocumentCommandHandler
{
	private readonly LuaDocumentCommandCallbacks _callbacks;
	private readonly IReadOnlyDictionary<UICommand, Action<LuaEditor>> _editorHandlers;
	private readonly IReadOnlyDictionary<UICommand, Action> _globalHandlers;

	public LuaDocumentCommandHandler(LuaDocumentCommandCallbacks callbacks)
	{
		_callbacks = callbacks ?? throw new ArgumentNullException(nameof(callbacks));
		_globalHandlers = new Dictionary<UICommand, Action>
		{
			[UICommand.NavigateBack] = _callbacks.NavigateBack,
			[UICommand.NavigateForward] = _callbacks.NavigateForward,
			[UICommand.FindReferences] = StudioDocumentCommandDispatcher.Run(_callbacks.FindReferencesAsync),
			[UICommand.RenameSymbol] = StudioDocumentCommandDispatcher.Run(_callbacks.RenameSymbolAsync),
			[UICommand.LuaBasics] = _callbacks.ShowLuaBasics
		};
		_editorHandlers = new Dictionary<UICommand, Action<LuaEditor>>
		{
			[UICommand.Reindent] = StudioDocumentCommandDispatcher.Run<LuaEditor>(_callbacks.ReindentAsync),
			[UICommand.TrimWhiteSpace] = StudioDocumentCommandDispatcher.Run<LuaEditor>(_callbacks.TrimWhitespaceAsync),
			[UICommand.GoToDefinition] = _callbacks.GoToDefinition
		};
	}

	public bool TryHandle(UICommand command)
		=> StudioDocumentCommandDispatcher.TryHandle(command, _globalHandlers)
			|| StudioDocumentCommandDispatcher.TryHandle(command, _callbacks.GetCurrentEditor, _editorHandlers);
}

public sealed record LuaDocumentCommandCallbacks(
	Func<LuaEditor> GetCurrentEditor,
	Func<Task> ReindentAsync,
	Func<Task> TrimWhitespaceAsync,
	Action NavigateBack,
	Action NavigateForward,
	Action<LuaEditor> GoToDefinition,
	Func<Task> FindReferencesAsync,
	Func<Task> RenameSymbolAsync,
	Action ShowLuaBasics);
