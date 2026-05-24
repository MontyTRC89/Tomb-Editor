using System;
using System.Collections.Generic;
using TombLib.Scripting.Diagnostics;

namespace TombLib.Scripting.UI.Diagnostics
{
	public interface IErrorDetector
	{
		IReadOnlyList<TextEditorDiagnostic> FindErrors(string editorContent, Version engineVersion);
	}
}
