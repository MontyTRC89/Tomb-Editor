using ScriptLib.Core.Interfaces;
using System.Text.Json;

namespace ScriptLib.Core.Services.Implementations;

public sealed class BookmarkService : IBookmarkService
{
	public void ToggleBookmark(ISupportsBookmarks component, int lineNumber)
	{
		if (component.Bookmarks.Contains(lineNumber))
			component.Bookmarks.Remove(lineNumber);
		else
			component.Bookmarks.Add(lineNumber);
	}

	public void GoToNextBookmark(ISupportsBookmarks component, int currentLineNumber)
	{
		if (component.Bookmarks.Count == 0)
			return;

		// Find the first bookmark greater than the current line
		int nextBookmark = component.Bookmarks.FirstOrDefault(b => b > currentLineNumber);

		// If no bookmark was found after the current line, wrap around to the first bookmark
		if (nextBookmark == 0)
			nextBookmark = component.Bookmarks.First();

		component.GoToBookmark(nextBookmark);
	}

	public void GoToPreviousBookmark(ISupportsBookmarks component, int currentLineNumber)
	{
		if (component.Bookmarks.Count == 0)
			return;

		// Find the last bookmark less than the current line
		int prevBookmark = component.Bookmarks.LastOrDefault(b => b < currentLineNumber);

		// If no bookmark was found before the current line, wrap around to the last bookmark
		if (prevBookmark == 0)
			prevBookmark = component.Bookmarks.Last();

		component.GoToBookmark(prevBookmark);
	}

	public void ClearBookmarks(ISupportsBookmarks component)
		=> component.Bookmarks.Clear();

	public void LoadBookmarks(ISupportsBookmarks component, string filePath)
	{
		if (!File.Exists(filePath))
			return;

		try
		{
			string json = File.ReadAllText(filePath);
			int[]? bookmarkLines = JsonSerializer.Deserialize<int[]>(json);

			component.Bookmarks.Clear();

			if (bookmarkLines != null)
			{
				foreach (int line in bookmarkLines)
					component.Bookmarks.Add(line);
			}
		}
		catch
		{
			// Silently fail if the file can't be loaded or is in an invalid format
		}
	}

	public void SaveBookmarks(ISupportsBookmarks component, string filePath)
	{
		try
		{
			int[] bookmarkLines = component.Bookmarks.ToArray();
			string json = JsonSerializer.Serialize(bookmarkLines);

			File.WriteAllText(filePath, json);
		}
		catch
		{
			// Silently fail if the file can't be saved
		}
	}
}
