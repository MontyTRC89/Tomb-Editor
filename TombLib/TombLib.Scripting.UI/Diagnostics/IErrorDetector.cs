using System;
using System.Collections.Generic;
using Nickelony.LanguageServer.Abstractions.Diagnostics;

namespace TombLib.Scripting.UI.Diagnostics
{
	public interface IErrorDetector
	{
		IReadOnlyList<TextEditorDiagnostic> FindErrors(string editorContent, Version engineVersion);
	}
}
