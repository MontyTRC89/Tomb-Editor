using DarkUI.Controls;
using System.Collections.Generic;

namespace TombLib.Scripting.UI.ContentNodes;

/// <summary>
/// Provides the content nodes shown for a document in the content explorer.
/// </summary>
public abstract class ContentNodesProviderBase
{
	/// <summary>
	/// Gets the content nodes for the given content and filter.
	/// </summary>
	/// <param name="content">The document content.</param>
	/// <param name="filter">The filter text to apply.</param>
	/// <returns>The content nodes.</returns>
	public IReadOnlyList<DarkTreeNode> GetNodes(string content, string filter)
		=> GetNodesCore(content ?? string.Empty, filter ?? string.Empty);

	/// <summary>
	/// Builds the content nodes for the given content and filter.
	/// </summary>
	/// <param name="content">The document content.</param>
	/// <param name="filter">The filter text to apply.</param>
	/// <returns>The content nodes.</returns>
	protected abstract IReadOnlyList<DarkTreeNode> GetNodesCore(string content, string filter);
}
