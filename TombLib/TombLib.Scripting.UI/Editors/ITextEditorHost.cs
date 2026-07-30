#nullable enable

using System;
using System.Collections.Generic;
using System.Runtime.Versioning;
using TombLib.Scripting.UI.Bases;

namespace TombLib.Scripting.UI.Editors;

/// <summary>
/// Represents the host-side editor coordination needed by shared workspace-edit services.
/// </summary>
[SupportedOSPlatform("windows")]
public interface ITextEditorHost
{
	/// <summary>
	/// Opens a text editor for the specified file path.
	/// </summary>
	/// <param name="filePath">The file path to open.</param>
	/// <param name="editorType">The preferred editor type.</param>
	/// <param name="openSourceView">Whether the source-view editor should be opened.</param>
	/// <returns>The opened text editor.</returns>
	TextEditorBase OpenTextEditor(string filePath, EditorType editorType = EditorType.Default, bool openSourceView = false);

	/// <summary>
	/// Gets all open editors for the specified file path.
	/// </summary>
	/// <param name="filePath">The file path whose editors should be returned.</param>
	/// <returns>The open editor controls for the file.</returns>
	IReadOnlyList<IEditorControl> GetOpenEditors(string filePath);

	/// <summary>
	/// Executes an action while preserving the current host selection where possible.
	/// </summary>
	/// <typeparam name="TResult">The result type produced by the action.</typeparam>
	/// <param name="action">The action to execute.</param>
	/// <returns>The action result.</returns>
	TResult ExecutePreservingSelection<TResult>(Func<TResult> action);
}
