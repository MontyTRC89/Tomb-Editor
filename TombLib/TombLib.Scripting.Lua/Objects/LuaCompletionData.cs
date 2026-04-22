using ICSharpCode.AvalonEdit.CodeCompletion;
using ICSharpCode.AvalonEdit.Document;
using ICSharpCode.AvalonEdit.Editing;
using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using TombLib.Scripting.Lua.Resources;
using TombLib.Scripting.Rendering;
using TombLib.Scripting.Resources;

namespace TombLib.Scripting.Lua.Objects;

/// <summary>
/// Adapts a <see cref="LuaCompletionItem"/> to AvalonEdit's completion-item UI contract.
/// </summary>
internal sealed class LuaCompletionData : ICompletionData, INotifyPropertyChanged
{
	private const double DescriptionMaxWidth = 540.0;
	private const double DescriptionTextMaxWidth = 500.0;

	private static readonly SolidColorBrush DescriptionBorderBrush = TextEditorColorPalette.ToolTipBorder;
	private static readonly SolidColorBrush DescriptionBackgroundBrush = TextEditorColorPalette.ToolTipBackground;
	private static readonly SolidColorBrush DescriptionForegroundBrush = TextEditorColorPalette.ToolTipForeground;

	private static readonly object NoDescriptionSentinel = new();

	private readonly object _resolveSync = new();

	private readonly LuaThemeBrushSet _brushSet;
	private LuaCompletionItem _item;
	private string? _displayDetail;
	private object? _cachedDescription;
	private Task<LuaCompletionItem>? _resolveTask;

	/// <summary>
	/// Initializes a new instance of the <see cref="LuaCompletionData"/> class.
	/// </summary>
	/// <param name="item">The completion item being adapted.</param>
	/// <param name="brushSet">The theme brushes used to render the item.</param>
	public LuaCompletionData(LuaCompletionItem item, LuaThemeBrushSet brushSet)
	{
		_item = item ?? throw new ArgumentNullException(nameof(item));
		_brushSet = brushSet ?? throw new ArgumentNullException(nameof(brushSet));
		_displayDetail = FlattenSingleLineText(_item.Detail);
	}

	/// <summary>
	/// Occurs when a bindable completion-property value changes.
	/// </summary>
	public event PropertyChangedEventHandler? PropertyChanged;

	/// <summary>
	/// Gets the icon image shown for the completion item.
	/// </summary>
	public ImageSource Image => LuaCompletionIconFactory.GetIcon(_item.IconKind, _brushSet);

	/// <summary>
	/// Gets the text used for filtering and matching.
	/// </summary>
	public string Text => _item.FilterText;

	/// <summary>
	/// Gets the primary label shown in the completion list.
	/// </summary>
	public string DisplayText => _item.Label;

	/// <summary>
	/// Gets the optional secondary detail shown inline in the completion list.
	/// </summary>
	public string? DisplayDetail => _displayDetail;

	/// <summary>
	/// Gets the visibility for the inline detail label.
	/// </summary>
	public Visibility DetailVisibility => string.IsNullOrEmpty(_displayDetail) ? Visibility.Collapsed : Visibility.Visible;

	/// <summary>
	/// Gets the content object used by AvalonEdit for the completion row.
	/// </summary>
	public object Content => DisplayText;

	/// <summary>
	/// Gets the tooltip content for the completion item.
	/// </summary>
	public object? Description
	{
		get
		{
			// Cache a sentinel for a missing description so subsequent accesses do not keep
			// rebuilding the same null result on every selection change.
			_cachedDescription ??= BuildDescriptionContent() ?? NoDescriptionSentinel;
			return ReferenceEquals(_cachedDescription, NoDescriptionSentinel) ? null : _cachedDescription;
		}
	}

	/// <summary>
	/// Gets the sort priority used by the completion list.
	/// </summary>
	public double Priority => _item.Priority;

	/// <summary>
	/// Gets a value indicating whether this item supports lazy resolve for richer detail.
	/// </summary>
	public bool CanResolve => _item.CanResolve;

	/// <summary>
	/// Inserts the completion text into the editor.
	/// </summary>
	/// <param name="textArea">The target text area.</param>
	/// <param name="completionSegment">The segment to replace.</param>
	/// <param name="insertionRequestEventArgs">The insertion request context.</param>
	public void Complete(TextArea textArea, ISegment completionSegment, EventArgs insertionRequestEventArgs)
		=> textArea.Document.Replace(completionSegment, _item.InsertText);

	/// <summary>
	/// Resolves and returns the tooltip content for the completion item.
	/// </summary>
	/// <param name="cancellationToken">A token that can cancel the lazy resolve request.</param>
	/// <returns>The tooltip content, or <see langword="null"/> when no detail is available.</returns>
	public async Task<object?> GetDescriptionAsync(CancellationToken cancellationToken = default)
	{
		if (!CanResolve)
			return Description;

		Task<LuaCompletionItem> resolveTask;

		lock (_resolveSync)
		{
			if (_resolveTask is null || _resolveTask.IsFaulted || _resolveTask.IsCanceled)
			{
				// Use the caller's token so a stale, abandoned resolve also cancels the underlying LSP request
				// instead of staying in flight against the language server.
				_resolveTask = _item.ResolveAsync(cancellationToken);
			}

			resolveTask = _resolveTask;
		}

		try
		{
			LuaCompletionItem resolvedItem = cancellationToken.CanBeCanceled
				? await resolveTask.WaitAsync(cancellationToken).ConfigureAwait(true)
				: await resolveTask.ConfigureAwait(true);

			ApplyResolvedItem(resolvedItem);
			return Description;
		}
		catch (OperationCanceledException)
		{
			// Allow a future request to retry the resolve when the caller cancels mid-flight.
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

	private static string? FlattenSingleLineText(string? text)
	{
		if (string.IsNullOrWhiteSpace(text))
			return null;

		string[] lines = text.Split(['\r', '\n'], StringSplitOptions.RemoveEmptyEntries);
		return lines.Length == 0 ? null : string.Join(" ", lines).Trim();
	}

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

	private void ApplyResolvedItem(LuaCompletionItem resolvedItem)
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
