namespace ScriptLib.Core.Interfaces;

/// <summary>
/// Defines a contract for components that support bookmark functionality.
/// </summary>
/// <remarks>
/// Implementations of this interface can track bookmarked positions,
/// typically line numbers in text-based editors or other positional markers.
/// </remarks>
public interface ISupportsBookmarks
{
	/// <summary>
	/// Gets or sets the sorted collection of bookmarks.
	/// </summary>
	/// <value>
	/// A set of integer values representing bookmarked positions.
	/// </value>
	SortedSet<int> Bookmarks { get; set; }

	/// <summary>
	/// Navigates to the bookmark at the specified index.
	/// </summary>
	/// <param name="index">The index or position of the bookmark to navigate to (usually a line number).</param>
	void GoToBookmark(int index);
}
