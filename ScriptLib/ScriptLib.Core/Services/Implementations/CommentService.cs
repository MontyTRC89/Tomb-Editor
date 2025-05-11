using ICSharpCode.AvalonEdit.Document;

namespace ScriptLib.Core.Services.Implementations;

public sealed class CommentService : ICommentService
{
	public void ToggleComments(TextDocument document, IReadOnlyList<DocumentLine> lines, string commentPrefix)
	{
		if (lines.Count == 0)
			return;

		// Determine if we need to add or remove comments
		bool addComments = !AreAllLinesCommented(document, lines, commentPrefix);

		document.BeginUpdate();

		try
		{
			if (addComments)
				AddComments(document, lines, commentPrefix);
			else
				RemoveComments(document, lines, commentPrefix);
		}
		finally
		{
			document.EndUpdate();
		}
	}

	public void CommentOutLines(TextDocument document, IReadOnlyList<DocumentLine> lines, string commentPrefix)
	{
		if (lines.Count == 0)
			return;

		document.BeginUpdate();

		try
		{
			AddComments(document, lines, commentPrefix);
		}
		finally
		{
			document.EndUpdate();
		}
	}

	public void UncommentLines(TextDocument document, IReadOnlyList<DocumentLine> lines, string commentPrefix)
	{
		if (lines.Count == 0)
			return;

		document.BeginUpdate();

		try
		{
			RemoveComments(document, lines, commentPrefix);
		}
		finally
		{
			document.EndUpdate();
		}
	}

	private static bool AreAllLinesCommented(TextDocument document, IReadOnlyList<DocumentLine> lines, string commentPrefix)
	{
		foreach (DocumentLine line in lines)
		{
			// Skip empty lines when determining if all lines are commented
			if (line.Length == 0)
				continue;

			string lineText = document.GetText(line.Offset, line.Length);
			string trimmedLine = lineText.TrimStart();

			if (!trimmedLine.StartsWith(commentPrefix))
				return false;
		}

		return true;
	}

	private static void AddComments(TextDocument document, IReadOnlyList<DocumentLine> lines, string commentPrefix)
	{
		// Process lines in reverse order to avoid offset issues
		for (int i = lines.Count - 1; i >= 0; i--)
		{
			DocumentLine line = lines[i];

			if (line.Length == 0)
				continue;

			string lineText = document.GetText(line.Offset, line.Length);
			int indentLength = lineText.Length - lineText.TrimStart().Length;

			// Insert comment at the start of the text (after indentation)
			document.Insert(line.Offset + indentLength, commentPrefix);
		}
	}

	private static void RemoveComments(TextDocument document, IReadOnlyList<DocumentLine> lines, string commentPrefix)
	{
		// Process lines in reverse order to avoid offset issues
		for (int i = lines.Count - 1; i >= 0; i--)
		{
			DocumentLine line = lines[i];

			if (line.Length == 0)
				continue;

			string lineText = document.GetText(line.Offset, line.Length);
			string trimmedLine = lineText.TrimStart();

			if (trimmedLine.StartsWith(commentPrefix))
			{
				int indentLength = lineText.Length - trimmedLine.Length;
				document.Remove(line.Offset + indentLength, commentPrefix.Length);
			}
		}
	}
}
