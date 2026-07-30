using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TombIDE.ScriptingStudio.CommandSurface;
using TombIDE.ScriptingStudio.UI;
using TombLib.Scripting.ClassicScript;

namespace TombIDE.ScriptingStudio.ClassicScript;

public sealed class ClassicScriptDocumentCommandHandler : IStudioDocumentCommandHandler
{
	private readonly ClassicScriptDocumentCommandCallbacks _callbacks;
	private readonly IReadOnlyDictionary<UICommand, Action<ClassicScriptEditor>> _editorHandlers;

	public ClassicScriptDocumentCommandHandler(ClassicScriptDocumentCommandCallbacks callbacks)
	{
		_callbacks = callbacks ?? throw new ArgumentNullException(nameof(callbacks));
		_editorHandlers = new Dictionary<UICommand, Action<ClassicScriptEditor>>
		{
			[UICommand.Reindent] = StudioDocumentCommandDispatcher.Run(_callbacks.ReindentAsync),
			[UICommand.TrimWhiteSpace] = StudioDocumentCommandDispatcher.Run(_callbacks.TrimWhitespaceAsync),
			[UICommand.TypeFirstAvailableId] = _callbacks.TypeFirstAvailableId,
			[UICommand.NewFileAtCaret] = _callbacks.CreateNewFileAtCaret
		};
	}

	public bool TryHandle(UICommand command)
		=> StudioDocumentCommandDispatcher.TryHandle(command, _callbacks.GetCurrentEditor, _editorHandlers);
}

public sealed record ClassicScriptDocumentCommandCallbacks(
	Func<ClassicScriptEditor> GetCurrentEditor,
	Func<ClassicScriptEditor, Task> ReindentAsync,
	Func<ClassicScriptEditor, Task> TrimWhitespaceAsync,
	Action<ClassicScriptEditor> TypeFirstAvailableId,
	Action<ClassicScriptEditor> CreateNewFileAtCaret);
