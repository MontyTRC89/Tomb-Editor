using Nickelony.LanguageServer.Abstractions.Diagnostics;
using System;
using System.Collections.Generic;

namespace TombLib.Scripting.UI.Diagnostics;

public interface IErrorDetector
{
	IReadOnlyList<TextEditorDiagnostic> FindErrors(string editorContent, Version engineVersion);
}
