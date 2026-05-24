using System;
using System.Threading.Tasks;
using TombIDE.ScriptingStudio.CommandSurface;
using TombIDE.ScriptingStudio.UI;
using TombLib.Scripting.ClassicScript;

namespace TombIDE.ScriptingStudio.ClassicScript;

public sealed class ClassicScriptDocumentCommandHandler : IStudioDocumentCommandHandler
{
	private readonly ClassicScriptDocumentCommandCallbacks _callbacks;

	public ClassicScriptDocumentCommandHandler(ClassicScriptDocumentCommandCallbacks callbacks)
	{
		_callbacks = callbacks ?? throw new ArgumentNullException(nameof(callbacks));
	}

	public bool TryHandle(UICommand command)
	{
		if (_callbacks.GetCurrentEditor() is not ClassicScriptEditor editor)
			return false;

		switch (command)
		{
			case UICommand.Reindent:
				_ = _callbacks.ReindentAsync(editor);
				return true;

			case UICommand.TrimWhiteSpace:
				_ = _callbacks.TrimWhitespaceAsync(editor);
				return true;

			case UICommand.TypeFirstAvailableId:
				_callbacks.TypeFirstAvailableId(editor);
				return true;

			case UICommand.NewFileAtCaret:
				_callbacks.CreateNewFileAtCaret(editor);
				return true;
		}

		return false;
	}
}

public sealed record ClassicScriptDocumentCommandCallbacks(
	Func<ClassicScriptEditor> GetCurrentEditor,
	Func<ClassicScriptEditor, Task> ReindentAsync,
	Func<ClassicScriptEditor, Task> TrimWhitespaceAsync,
	Action<ClassicScriptEditor> TypeFirstAvailableId,
	Action<ClassicScriptEditor> CreateNewFileAtCaret);