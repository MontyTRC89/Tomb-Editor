using System;
using System.Collections.Generic;
using TombLib.Scripting.Objects;

namespace TombLib.Scripting.Interfaces
{
	public interface IErrorDetector
	{
		IReadOnlyList<TextEditorDiagnostic> FindErrors(string editorContent, Version engineVersion);
	}
}
