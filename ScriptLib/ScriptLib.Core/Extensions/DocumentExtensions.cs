using ICSharpCode.AvalonEdit.Document;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScriptLib.Core.Extensions;

public static class DocumentExtensions
{
	public static DocumentLine? TryGetLineByNumber(this TextDocument document, int lineNumber)
		=> lineNumber < 1 || lineNumber > document.LineCount
			? null
			: document.GetLineByNumber(lineNumber);

	public static DocumentLine? TryGetLineByOffset(this TextDocument document, int offset)
		=> offset < 0 || offset > document.TextLength
			? null
			: document.GetLineByOffset(offset);
}
