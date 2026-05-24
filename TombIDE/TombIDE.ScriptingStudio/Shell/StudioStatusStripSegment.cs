using System;
using System.Collections.Generic;
using TombIDE.ScriptingStudio.UI;
using TombLib.Scripting.UI.Editors;

namespace TombIDE.ScriptingStudio.Shell
{
	public enum StudioStatusStripSegment
	{
		CaretPosition,
		SelectionLength,
		Zoom,
		SyntaxPreview
	}

	public interface IStudioDocumentStatusStripProvider
	{
		IReadOnlyList<StudioStatusStripSegment> GetSegments(IEditorControl editor, DocumentMode documentMode);
	}
}