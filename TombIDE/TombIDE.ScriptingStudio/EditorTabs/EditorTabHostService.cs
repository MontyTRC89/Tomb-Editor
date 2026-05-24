#nullable enable

using System;
using System.Windows.Forms;
using System.Windows.Forms.Integration;
using TombLib.Scripting.UI.Editors;

namespace TombIDE.ScriptingStudio.EditorTabs;

internal sealed class EditorTabHostService
{
	public Control CreateHostControl(IEditorControl editor)
	{
		ArgumentNullException.ThrowIfNull(editor);

		if (editor is Control control)
		{
			control.Dock = DockStyle.Fill;
			return control;
		}

		if (editor is System.Windows.UIElement element)
		{
			return new ElementHost
			{
				Dock = DockStyle.Fill,
				Child = element
			};
		}

		throw new InvalidOperationException($"Unsupported editor host type: {editor.GetType().FullName}");
	}

	public IEditorControl? GetEditor(TabPage? tabPage)
	{
		Control? hostedControl = GetHostedControl(tabPage);

		return hostedControl switch
		{
			ElementHost { Child: IEditorControl editor } => editor,
			IEditorControl editor => editor,
			_ => null
		};
	}

	public Control? GetHostedControl(TabPage? tabPage)
		=> tabPage?.Controls.Count > 0 ? tabPage.Controls[0] : null;
}