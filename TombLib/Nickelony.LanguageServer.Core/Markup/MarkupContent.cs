namespace Nickelony.LanguageServer.Core;

/// <summary>
/// Represents extracted markup content together with whether it should be treated as Markdown.
/// </summary>
public readonly record struct MarkupContent
{
	/// <summary>
	/// Initializes a new instance of the <see cref="MarkupContent"/> struct.
	/// </summary>
	/// <param name="text">The extracted text value.</param>
	/// <param name="isMarkdown">Whether the extracted text should be treated as Markdown.</param>
	public MarkupContent(string? text, bool isMarkdown)
	{
		Text = text ?? string.Empty;
		IsMarkdown = isMarkdown;
	}

	/// <summary>
	/// Gets the extracted text.
	/// </summary>
	public string Text { get; }

	/// <summary>
	/// Gets a value indicating whether the content should be treated as Markdown.
	/// </summary>
	public bool IsMarkdown { get; }
}
