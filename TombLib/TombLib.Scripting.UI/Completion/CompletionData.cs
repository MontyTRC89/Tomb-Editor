using ICSharpCode.AvalonEdit.CodeCompletion;
using ICSharpCode.AvalonEdit.Document;
using ICSharpCode.AvalonEdit.Editing;
using System;
using System.ComponentModel;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Nickelony.LanguageServer.Abstractions.Completion;
using TombLib.Scripting.UI.Rendering;
using TombLib.Scripting.UI.Resources;

namespace TombLib.Scripting.UI.Completion
{
	public readonly record struct CompletionDataInsertionResult(string Text, int? CaretOffset);

	/// <summary>
	/// Implements AvalonEdit's completion-item UI contract for the shared scripting completion model.
	/// </summary>
	public sealed class CompletionData : ICompletionData, INotifyPropertyChanged
	{
		private const double DescriptionMaxWidth = 540.0;
		private const double DescriptionTextMaxWidth = 500.0;

		private static readonly SolidColorBrush DescriptionBorderBrush = TextEditorColorPalette.ToolTipBorder;
		private static readonly SolidColorBrush DescriptionBackgroundBrush = TextEditorColorPalette.ToolTipBackground;
		private static readonly SolidColorBrush DescriptionForegroundBrush = TextEditorColorPalette.ToolTipForeground;
		private static readonly object NoDescriptionSentinel = new();
		private static readonly PropertyInfo? OverstrikeModeProperty = typeof(TextArea).GetProperty("OverstrikeMode");

		private readonly object _resolveSync = new();
		private readonly Func<TextCompletionItem, ImageSource?>? _getImage;
		private readonly Func<TextCompletionItem, bool>? _canApplyItem;
		private readonly Func<TextArea, int, string, int?, CompletionDataInsertionResult>? _normalizeInsertion;

		private TextCompletionItem _item;
		private string? _displayDetail;
		private object? _cachedDescription;
		private Task<TextCompletionItem>? _resolveTask;

		public CompletionData(string text)
			: this(new TextCompletionItem(text))
		{
		}

		public CompletionData(string text, string insertText, string description = "")
			: this(new TextCompletionItem(text, insertText, description))
		{
		}

		public CompletionData(
			TextCompletionItem item,
			Func<TextCompletionItem, ImageSource?>? getImage = null,
			Func<TextCompletionItem, bool>? canApplyItem = null,
			Func<TextArea, int, string, int?, CompletionDataInsertionResult>? normalizeInsertion = null)
		{
			_item = item ?? throw new ArgumentNullException(nameof(item));
			_getImage = getImage;
			_canApplyItem = canApplyItem;
			_normalizeInsertion = normalizeInsertion;
			_displayDetail = FlattenSingleLineText(_item.Detail);
		}

		public event PropertyChangedEventHandler? PropertyChanged;

		public ImageSource Image => _getImage?.Invoke(_item)!;

		public string Text => _item.FilterText;

		public string DisplayText => _item.Label;

		public string? DisplayDetail => _displayDetail;

		public Visibility DetailVisibility => string.IsNullOrEmpty(_displayDetail) ? Visibility.Collapsed : Visibility.Visible;

		public object Content => DisplayText;

		public object? Description
		{
			get
			{
				_cachedDescription ??= BuildDescriptionContent() ?? NoDescriptionSentinel;
				return ReferenceEquals(_cachedDescription, NoDescriptionSentinel) ? null : _cachedDescription;
			}
		}

		public double Priority => _item.Priority;

		public bool CanResolve => _item.CanResolve;

		public void Complete(TextArea textArea, ISegment completionSegment, EventArgs insertionRequestEventArgs)
		{
			ArgumentNullException.ThrowIfNull(textArea);
			ArgumentNullException.ThrowIfNull(completionSegment);
			ArgumentNullException.ThrowIfNull(insertionRequestEventArgs);

			if (!CanApplyCompletionItem(_item, _canApplyItem))
				return;

			TextDocument document = textArea.Document;
			string insertText = _item.InsertText;
			int? insertCaretOffset = _item.InsertCaretOffset;
			(int replacementOffset, int replacementLength) = ResolveCompletionSegment(
				document,
				completionSegment.Offset,
				completionSegment.Length,
				_item.TextEdit,
				ShouldUseReplaceRange(textArea));

			if (ContainsLineBreak(insertText) && _normalizeInsertion is not null)
			{
				CompletionDataInsertionResult normalizedInsertion = _normalizeInsertion(textArea, replacementOffset, insertText, insertCaretOffset);
				insertText = normalizedInsertion.Text;
				insertCaretOffset = normalizedInsertion.CaretOffset;
			}

			document.Replace(replacementOffset, replacementLength, insertText);
			textArea.Caret.Offset = replacementOffset + (insertCaretOffset ?? insertText.Length);
		}

		public async Task<object?> GetDescriptionAsync(CancellationToken cancellationToken = default)
		{
			if (!CanResolve)
				return Description;

			Task<TextCompletionItem> resolveTask;

			lock (_resolveSync)
			{
				if (_resolveTask is null || _resolveTask.IsFaulted || _resolveTask.IsCanceled)
					_resolveTask = _item.ResolveAsync(cancellationToken);

				resolveTask = _resolveTask;
			}

			try
			{
				TextCompletionItem resolvedItem = cancellationToken.CanBeCanceled
					? await resolveTask.WaitAsync(cancellationToken).ConfigureAwait(true)
					: await resolveTask.ConfigureAwait(true);

				ApplyResolvedItem(resolvedItem);
				return Description;
			}
			catch (OperationCanceledException)
			{
				lock (_resolveSync)
				{
					if (ReferenceEquals(_resolveTask, resolveTask))
						_resolveTask = null;
				}

				throw;
			}
			catch
			{
				lock (_resolveSync)
				{
					if (ReferenceEquals(_resolveTask, resolveTask))
						_resolveTask = null;
				}

				throw;
			}
		}

		public void RebaseForCurrentDocument(int requestDocumentVersion, int requestGeneration)
		{
			_item = _item.WithFilteredCommitContext(requestDocumentVersion, requestGeneration);

			lock (_resolveSync)
				_resolveTask = null;
		}

		private static string? FlattenSingleLineText(string? text)
		{
			if (string.IsNullOrWhiteSpace(text))
				return null;

			string[] lines = text.Split(['\r', '\n'], StringSplitOptions.RemoveEmptyEntries);
			return lines.Length == 0 ? null : string.Join(" ", lines).Trim();
		}

		private static bool CanApplyCompletionItem(TextCompletionItem item, Func<TextCompletionItem, bool>? canApplyItem)
			=> canApplyItem?.Invoke(item) ?? true;

		private static bool ContainsLineBreak(string text)
			=> text.IndexOfAny(['\r', '\n']) >= 0;

		private static (int Offset, int Length) ResolveCompletionSegment(
			TextDocument document,
			int fallbackOffset,
			int fallbackLength,
			TextCompletionTextEdit? textEdit,
			bool useReplaceRange)
		{
			ArgumentNullException.ThrowIfNull(document);

			var fallbackSegment = (fallbackOffset, fallbackLength);

			if (textEdit is not TextCompletionTextEdit completionTextEdit)
				return fallbackSegment;

			TextCompletionRange range = useReplaceRange
				? completionTextEdit.ReplacementRange
				: completionTextEdit.InsertRange;

			return TryCreateCompletionSegment(document, range, out (int Offset, int Length) replacementSegment)
				? replacementSegment
				: fallbackSegment;
		}

		private static bool TryCreateCompletionSegment(TextDocument document,
			TextCompletionRange range,
			out (int Offset, int Length) segment)
		{
			segment = default;

			if (!TryGetCompletionOffset(document, range.Start, out int startOffset)
				|| !TryGetCompletionOffset(document, range.End, out int endOffset)
				|| endOffset < startOffset)
			{
				return false;
			}

			segment = (startOffset, endOffset - startOffset);
			return true;
		}

		private static bool TryGetCompletionOffset(TextDocument document, TextCompletionPosition position, out int offset)
		{
			offset = 0;
			int lineNumber = position.Line + 1;

			if (lineNumber < 1 || lineNumber > document.LineCount)
				return false;

			DocumentLine line = document.GetLineByNumber(lineNumber);

			if (position.Character < 0 || position.Character > line.Length)
				return false;

			offset = line.Offset + position.Character;
			return true;
		}

		private static bool ShouldUseReplaceRange(TextArea textArea)
			=> OverstrikeModeProperty?.GetValue(textArea) is true;

		private Border? BuildDescriptionContent()
		{
			bool hasDetail = !string.IsNullOrWhiteSpace(_item.Detail);
			bool hasDescription = !string.IsNullOrWhiteSpace(_item.Description);

			if (!hasDetail && !hasDescription)
				return null;

			var panel = new StackPanel
			{
				MaxWidth = DescriptionMaxWidth
			};

			if (hasDetail)
			{
				panel.Children.Add(new TextBlock
				{
					Text = _item.Detail,
					Foreground = DescriptionForegroundBrush,
					TextWrapping = TextWrapping.Wrap,
					MaxWidth = DescriptionTextMaxWidth,
					FontWeight = FontWeights.SemiBold,
					Margin = hasDescription
						? new Thickness(0.0, 0.0, 0.0, 8.0)
						: new Thickness(0.0)
				});
			}

			if (hasDescription)
			{
				string descriptionText = _item.Description!;

				panel.Children.Add(_item.IsDescriptionMarkdown
					? MarkdownToolTipRenderer.CreateContent(descriptionText, DescriptionForegroundBrush, DescriptionBackgroundBrush, false)
					: MarkdownToolTipRenderer.CreatePlainTextContent(descriptionText, DescriptionForegroundBrush, false));
			}

			return new Border
			{
				Background = DescriptionBackgroundBrush,
				BorderBrush = DescriptionBorderBrush,
				BorderThickness = new Thickness(1.0),
				Padding = new Thickness(8.0, 6.0, 8.0, 6.0),
				Child = panel,
				MaxWidth = DescriptionMaxWidth
			};
		}

		private void ApplyResolvedItem(TextCompletionItem resolvedItem)
		{
			if (resolvedItem is null || ReferenceEquals(resolvedItem, _item))
				return;

			_item = resolvedItem;
			_displayDetail = FlattenSingleLineText(_item.Detail);
			_cachedDescription = null;

			OnPropertyChanged(nameof(Image));
			OnPropertyChanged(nameof(DisplayDetail));
			OnPropertyChanged(nameof(DetailVisibility));
			OnPropertyChanged(nameof(Description));
		}

		private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
			=> PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
	}
}
