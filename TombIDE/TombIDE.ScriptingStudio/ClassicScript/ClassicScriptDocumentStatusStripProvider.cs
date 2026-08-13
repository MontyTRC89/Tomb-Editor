using System.Collections.Generic;
using TombIDE.ScriptingStudio.Shell;
using TombIDE.ScriptingStudio.UI;
using TombLib.Scripting.UI.Editors;

namespace TombIDE.ScriptingStudio.ClassicScript;

internal sealed class ClassicScriptDocumentStatusStripProvider : IStudioDocumentStatusStripProvider
{
	public IReadOnlyList<StudioStatusStripSegment> GetSegments(IEditorControl editor, DocumentMode documentMode)
	{
		return documentMode == DocumentMode.ClassicScript
			? [StudioStatusStripSegment.SyntaxPreview]
			: [];
	}
}
