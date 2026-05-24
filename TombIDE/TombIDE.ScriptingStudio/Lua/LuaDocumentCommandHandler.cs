using System;
using System.Threading.Tasks;
using TombIDE.ScriptingStudio.CommandSurface;
using TombIDE.ScriptingStudio.UI;
using TombLib.Scripting.Lua;

namespace TombIDE.ScriptingStudio.Lua;

public sealed class LuaDocumentCommandHandler : IStudioDocumentCommandHandler
{
	private readonly LuaDocumentCommandCallbacks _callbacks;

	public LuaDocumentCommandHandler(LuaDocumentCommandCallbacks callbacks)
	{
		_callbacks = callbacks ?? throw new ArgumentNullException(nameof(callbacks));
	}

	public bool TryHandle(UICommand command)
	{
		if (command == UICommand.NavigateBack)
		{
			_callbacks.NavigateBack();
			return true;
		}

		if (command == UICommand.NavigateForward)
		{
			_callbacks.NavigateForward();
			return true;
		}

		if (command == UICommand.FindReferences)
		{
			_ = _callbacks.FindReferencesAsync();
			return true;
		}

		if (command == UICommand.RenameSymbol)
		{
			_ = _callbacks.RenameSymbolAsync();
			return true;
		}

		if (command == UICommand.LuaBasics)
		{
			_callbacks.ShowLuaBasics();
			return true;
		}

		if (_callbacks.GetCurrentEditor() is not LuaEditor editor)
			return false;

		switch (command)
		{
			case UICommand.Reindent:
				_ = _callbacks.ReindentAsync();
				return true;

			case UICommand.TrimWhiteSpace:
				_ = _callbacks.TrimWhitespaceAsync();
				return true;

			case UICommand.GoToDefinition:
				_callbacks.GoToDefinition(editor);
				return true;
		}

		return false;
	}
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