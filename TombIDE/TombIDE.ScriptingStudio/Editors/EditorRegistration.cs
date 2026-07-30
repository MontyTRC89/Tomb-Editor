#nullable enable

using System;
using TombIDE.ScriptingStudio.UI;
using TombLib.Scripting.UI.Editors;

namespace TombIDE.ScriptingStudio.Editors;

internal sealed class EditorRegistration
{
	public EditorRegistration(
		EditorType editorType,
		DocumentMode documentMode,
		Func<string, bool> supportsFile,
		Func<string, bool> isDefaultForFile,
		Func<Version, IEditorControl> factory)
	{
		EditorType = editorType;
		DocumentMode = documentMode;
		SupportsFile = supportsFile ?? throw new ArgumentNullException(nameof(supportsFile));
		IsDefaultForFile = isDefaultForFile ?? throw new ArgumentNullException(nameof(isDefaultForFile));
		Factory = factory ?? throw new ArgumentNullException(nameof(factory));
	}

	public EditorType EditorType { get; }

	public DocumentMode DocumentMode { get; }

	public Func<Version, IEditorControl> Factory { get; }

	public Func<string, bool> IsDefaultForFile { get; }

	public Func<string, bool> SupportsFile { get; }
}
