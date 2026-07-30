using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TombIDE.ScriptingStudio.CommandSurface;
using TombIDE.ScriptingStudio.UI;
using TombLib.Scripting.TRX;

namespace TombIDE.ScriptingStudio.TRX;

public sealed class TrxDocumentCommandHandler : IStudioDocumentCommandHandler
{
	private readonly TrxDocumentCommandCallbacks _callbacks;
	private readonly IReadOnlyDictionary<UICommand, Action<TRXEditor>> _editorHandlers;

	public TrxDocumentCommandHandler(TrxDocumentCommandCallbacks callbacks)
	{
		_callbacks = callbacks ?? throw new ArgumentNullException(nameof(callbacks));
		_editorHandlers = new Dictionary<UICommand, Action<TRXEditor>>
		{
			[UICommand.Reindent] = StudioDocumentCommandDispatcher.Run(_callbacks.FormatAsync),
			[UICommand.TrimWhiteSpace] = StudioDocumentCommandDispatcher.Run(_callbacks.FormatAsync)
		};
	}

	public bool TryHandle(UICommand command)
		=> StudioDocumentCommandDispatcher.TryHandle(command, _callbacks.GetCurrentEditor, _editorHandlers);
}

public sealed record TrxDocumentCommandCallbacks(
	Func<TRXEditor> GetCurrentEditor,
	Func<TRXEditor, Task> FormatAsync);
