using System;
using System.Threading.Tasks;
using TombIDE.ScriptingStudio.CommandSurface;
using TombIDE.ScriptingStudio.UI;
using TombLib.Scripting.TRX;

namespace TombIDE.ScriptingStudio.TRX;

public sealed class TrxDocumentCommandHandler : IStudioDocumentCommandHandler
{
	private readonly TrxDocumentCommandCallbacks _callbacks;

	public TrxDocumentCommandHandler(TrxDocumentCommandCallbacks callbacks)
	{
		_callbacks = callbacks ?? throw new ArgumentNullException(nameof(callbacks));
	}

	public bool TryHandle(UICommand command)
	{
		if (_callbacks.GetCurrentEditor() is not TRXEditor editor)
			return false;

		if (command == UICommand.Reindent || command == UICommand.TrimWhiteSpace)
		{
			_ = _callbacks.FormatAsync(editor);
			return true;
		}

		return false;
	}
}

public sealed record TrxDocumentCommandCallbacks(
	Func<TRXEditor> GetCurrentEditor,
	Func<TRXEditor, Task> FormatAsync);