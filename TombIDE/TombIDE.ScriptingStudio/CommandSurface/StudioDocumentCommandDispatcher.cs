#nullable enable

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TombIDE.ScriptingStudio.UI;

namespace TombIDE.ScriptingStudio.CommandSurface;

internal static class StudioDocumentCommandDispatcher
{
	public static bool TryHandle(UICommand command, IReadOnlyDictionary<UICommand, Action> handlers)
	{
		if (!handlers.TryGetValue(command, out Action? handler))
			return false;

		handler();
		return true;
	}

	public static bool TryHandle<TEditor>(
		UICommand command,
		Func<TEditor?> getCurrentEditor,
		IReadOnlyDictionary<UICommand, Action<TEditor>> handlers)
		where TEditor : class
	{
		if (!handlers.TryGetValue(command, out Action<TEditor>? handler))
			return false;

		if (getCurrentEditor() is not TEditor editor)
			return false;

		handler(editor);
		return true;
	}

	public static Action Run(Func<Task> action)
		=> () => { _ = action(); };

	public static Action<TEditor> Run<TEditor>(Func<Task> action)
		=> ignored => { _ = action(); };

	public static Action<TEditor> Run<TEditor>(Func<TEditor, Task> action)
		=> editor => { _ = action(editor); };
}
