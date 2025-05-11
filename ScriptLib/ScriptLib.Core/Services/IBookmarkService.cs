using ScriptLib.Core.Interfaces;

namespace ScriptLib.Core.Services;

/// <summary>
/// Defines a service for managing bookmarks within components that support bookmark functionality.
/// </summary>
public interface IBookmarkService
{
	/// <summary>
	/// Toggles a bookmark at the specified line number. If a bookmark exists, it will be removed;
	/// otherwise, a new bookmark will be added.
	/// </summary>
	/// <param name="component">The component that supports bookmarks.</param>
	/// <param name="lineNumber">The line number where to toggle the bookmark.</param>
	void ToggleBookmark(ISupportsBookmarks component, int lineNumber);

	/// <summary>
	/// Navigates to the next bookmark after the current line number.
	/// If the current line is the last bookmark, wraps around to the first bookmark.
	/// </summary>
	/// <param name="component">The component that supports bookmarks.</param>
	/// <param name="currentLineNumber">The current line number to navigate from.</param>
	void GoToNextBookmark(ISupportsBookmarks component, int currentLineNumber);

	/// <summary>
	/// Navigates to the previous bookmark before the current line number.
	/// If the current line is the first bookmark, wraps around to the last bookmark.
	/// </summary>
	/// <param name="component">The component that supports bookmarks.</param>
	/// <param name="currentLineNumber">The current line number to navigate from.</param>
	void GoToPreviousBookmark(ISupportsBookmarks component, int currentLineNumber);

	/// <summary>
	/// Removes all bookmarks from the specified component.
	/// </summary>
	/// <param name="component">The component from which to clear all bookmarks.</param>
	void ClearBookmarks(ISupportsBookmarks component);

	/// <summary>
	/// Loads bookmarks from a file and applies them to the specified component.
	/// </summary>
	/// <param name="component">The component to which the bookmarks will be loaded.</param>
	/// <param name="filePath">The path to the file containing the bookmark data.</param>
	void LoadBookmarks(ISupportsBookmarks component, string filePath);

	/// <summary>
	/// Saves the bookmarks from the specified component to a file.
	/// </summary>
	/// <param name="component">The component whose bookmarks will be saved.</param>
	/// <param name="filePath">The path to the file where the bookmark data will be saved.</param>
	void SaveBookmarks(ISupportsBookmarks component, string filePath);
}
