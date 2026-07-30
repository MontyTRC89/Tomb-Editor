using System.Collections.Generic;
using TombIDE.ScriptingStudio.UI;
using TombLib.Scripting.UI.Editors;

namespace TombIDE.ScriptingStudio.Shell;

public enum StudioStatusStripSegment
{
	CaretPosition,
	SelectionLength,
	Zoom,
	SyntaxPreview
}

/// <summary>
/// Provides status strip segments for a given editor and document mode.
/// </summary>
public interface IStudioDocumentStatusStripProvider
{
	/// <summary>
	/// Gets the status strip segments to display for the specified editor and document mode.
	/// </summary>
	IReadOnlyList<StudioStatusStripSegment> GetSegments(IEditorControl editor, DocumentMode documentMode);
}
