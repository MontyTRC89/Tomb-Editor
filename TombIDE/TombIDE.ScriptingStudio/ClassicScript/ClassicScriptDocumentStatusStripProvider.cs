using System;
using System.Collections.Generic;
using TombIDE.ScriptingStudio.Shell;
using TombIDE.ScriptingStudio.UI;
using TombLib.Scripting.UI.Editors;

namespace TombIDE.ScriptingStudio.ClassicScript;

internal sealed class ClassicScriptDocumentStatusStripProvider : IStudioDocumentStatusStripProvider
{
	public IReadOnlyList<StudioStatusStripSegment> GetSegments(IEditorControl editor, DocumentMode documentMode)
		=> documentMode == DocumentMode.ClassicScript
			? new[] { StudioStatusStripSegment.SyntaxPreview }
			: Array.Empty<StudioStatusStripSegment>();
}
