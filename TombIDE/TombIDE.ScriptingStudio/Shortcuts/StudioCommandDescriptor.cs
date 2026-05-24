using System;
using System.Collections.Generic;
using System.Windows.Forms;
using TombIDE.ScriptingStudio.UI;

namespace TombIDE.ScriptingStudio.Shortcuts
{
	internal sealed class StudioCommandDescriptor
	{
		public StudioCommandDescriptor(UICommand command, params StudioShortcutBinding[] defaultBindings)
		{
			Command = command;
			DefaultBindings = Array.AsReadOnly(defaultBindings ?? Array.Empty<StudioShortcutBinding>());
		}

		public UICommand Command { get; }

		public IReadOnlyList<StudioShortcutBinding> DefaultBindings { get; }
	}

	internal sealed class StudioShortcutBinding
	{
		public StudioShortcutBinding(Keys keys, string displayText = null)
		{
			Keys = keys;
			DisplayText = displayText;
		}

		public Keys Keys { get; }

		public string DisplayText { get; }
	}
}