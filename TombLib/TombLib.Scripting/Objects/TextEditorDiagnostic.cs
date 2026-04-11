using System;

namespace TombLib.Scripting.Objects
{
	public sealed class TextEditorDiagnostic
	{
		public TextEditorDiagnostic(TextEditorDiagnosticSeverity severity, string message, int startOffset, int endOffset)
		{
			Severity = severity;
			Message = message ?? throw new ArgumentNullException(nameof(message));
			StartOffset = Math.Max(0, startOffset);
			EndOffset = Math.Max(StartOffset + 1, endOffset);
		}

		public TextEditorDiagnosticSeverity Severity { get; }
		public string Message { get; }
		public int StartOffset { get; }
		public int EndOffset { get; }

		public bool ContainsOffset(int offset)
			=> offset >= StartOffset && offset < EndOffset;

		public bool Intersects(int startOffset, int endOffset)
			=> endOffset > startOffset && EndOffset > startOffset && StartOffset < endOffset;
	}
}