namespace TombLib.Scripting.Lua.Objects;

/// <summary>
/// Describes the content shown in a Lua hover tooltip.
/// </summary>
public sealed class LuaHoverInfo
{
	/// <summary>
	/// Initializes a new instance of the <see cref="LuaHoverInfo"/> class.
	/// </summary>
	/// <param name="content">The hover content.</param>
	/// <param name="isMarkdown">Whether the content should be rendered as Markdown.</param>
	public LuaHoverInfo(string content, bool isMarkdown)
	{
		Content = content;
		IsMarkdown = isMarkdown;
	}

	/// <summary>
	/// Gets the hover content.
	/// </summary>
	public string Content { get; }

	/// <summary>
	/// Gets a value indicating whether <see cref="Content"/> is Markdown.
	/// </summary>
	public bool IsMarkdown { get; }
}