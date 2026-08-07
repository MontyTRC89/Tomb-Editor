using ICSharpCode.AvalonEdit.Document;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using TombLib.Scripting.UI.Editing;
using TombLib.Scripting.UI.Rendering;

namespace TombLib.Scripting.UI.Bases;

public abstract partial class TextEditorBase
{
	#region Auto bracket closing

	private void HandleAutoClosing(TextCompositionEventArgs e)
		=> _autoClosingService.HandleTextEntering(this, e, CreateAutoClosingOptions(), OnAutoClosingElementSkipped);

	private TextAutoClosingOptions CreateAutoClosingOptions()
		=> new(
			AutoCloseParentheses,
			AutoCloseBraces,
			AutoCloseBrackets,
			AutoCloseDoubleQuotes,
			AutoCloseSingleQuotes,
			ParenthesesClosingString,
			BracesClosingString,
			BracketsClosingString,
			QuotesClosingString,
			"'");

	protected virtual void OnAutoClosingElementSkipped(string element)
	{ }

	#endregion Auto bracket closing

	#region Multiline commenting

	public void CommentOutLines()
	{
		ApplyLineCommentTransformation(TextLineCommentAction.Comment);
	}

	public void UncommentLines()
	{
		ApplyLineCommentTransformation(TextLineCommentAction.Uncomment);
	}

	public void ToggleCommentLines()
	{
		ApplyLineCommentTransformation(TextLineCommentAction.Toggle);
	}

	private void ApplyLineCommentTransformation(TextLineCommentAction action)
		=> _commentService.ApplyEdit(this, CommentPrefix, action);

	#endregion Multiline commenting

	#region Bookmarks

	public void ToggleBookmark()
		=> _bookmarkCoordinator.ToggleBookmark(CaretOffset);

	public void GoToNextBookmark()
	{
		DocumentLine? nextBookmark = _bookmarkCoordinator.GetNextBookmarkLine(CaretOffset);

		if (nextBookmark is null)
			return;

		CaretOffset = nextBookmark.EndOffset;
		ScrollToLine(nextBookmark.LineNumber);
	}

	public void GoToPrevBookmark()
	{
		DocumentLine? previousBookmark = _bookmarkCoordinator.GetPreviousBookmarkLine(CaretOffset);

		if (previousBookmark is null)
			return;

		CaretOffset = previousBookmark.EndOffset;
		ScrollToLine(previousBookmark.LineNumber);
	}

	public void ClearAllBookmarks(Func<bool> confirmClearBookmarks)
	{
		ArgumentNullException.ThrowIfNull(confirmClearBookmarks);

		if (!confirmClearBookmarks())
			return;

		_bookmarkCoordinator.Clear();
	}

	internal IReadOnlyList<DocumentLine> GetBookmarkedLines()
		=> _bookmarkCoordinator.GetBookmarkedLines();

	#endregion Bookmarks

	#region Zoom

	public int Zoom
	{
		get => _statusCoordinator.Zoom;
		set
		{
			FontSize = DefaultFontSize * value / 100;
			_statusCoordinator.Zoom = value;
		}
	}

	#endregion Zoom

	#region View operations

	public void SelectLine(int lineNumber) => SelectLine(Document.GetLineByNumber(lineNumber));

	public void SelectLine(DocumentLine line) => _viewService.SelectLine(line);

	public void ReplaceLine(int lineNumber, string replacement, bool deselectAfterwards = false)
		=> ReplaceLine(Document.GetLineByNumber(lineNumber), replacement, deselectAfterwards);

	public void ReplaceLine(DocumentLine line, string replacement, bool deselectAfterwards = false)
		=> _viewService.ReplaceLine(line, replacement, deselectAfterwards);

	public void ReplaceContent(string newContent)
		=> _viewService.ReplaceContent(newContent);

	public void ResetSelection() => _viewService.ResetSelection();

	public void ResetSelectionAt(int lineNumber) => ResetSelectionAt(Document.GetLineByNumber(lineNumber));

	public void ResetSelectionAt(DocumentLine line) => _viewService.ResetSelectionAt(line);

	public int GetOffsetFromPoint(Point point)
		=> _viewService.GetOffsetFromPoint(point);

	public string? GetWordFromOffset(int offset)
		=> _viewService.GetWordFromOffset(offset);

	#endregion View operations

	#region ToolTips

	public void ShowToolTip(string content)
		=> ShowToolTip(content,
			DefaultToolTipBorder,
			DefaultToolTipBackground,
			ToolTipForeground);

	public void ShowMarkdownToolTip(string content)
		=> ShowMarkdownToolTip(content,
			DefaultToolTipBorder,
			DefaultToolTipBackground,
			ToolTipForeground);

	public void ShowToolTip(string content, SolidColorBrush border, SolidColorBrush background, SolidColorBrush foreground)
		=> ShowToolTip(TextEditorToolTipHelper.CreatePlainToolTipContent(content, foreground), border, background);

	public void ShowMarkdownToolTip(string content, SolidColorBrush border, SolidColorBrush background, SolidColorBrush foreground)
		=> ShowToolTip(TextEditorToolTipHelper.CreateMarkdownToolTipContent(content, foreground, background), border, background);

	public void ShowToolTip(object content, SolidColorBrush border, SolidColorBrush background)
		=> _toolTipPresenter.Show(content, border, background);

	#endregion ToolTips

	#region Formatting

	public void ConvertSpacesToTabs()
		=> Content = WhiteSpaceConverter.ConvertSpacesToTabs(Content, 4);

	public void ConvertTabsToSpaces()
		=> Content = WhiteSpaceConverter.ConvertTabsToSpaces(Content, 4);

	public virtual void TidyCode(bool trimOnly = false)
		=> FormattingService.FormatDocument(this, DocumentFormatter, trimOnly);

	#endregion Formatting
}
