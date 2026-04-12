using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using ICSharpCode.AvalonEdit.CodeCompletion;
using ICSharpCode.AvalonEdit.Document;
using ICSharpCode.AvalonEdit.Editing;
using TombLib.Scripting.Rendering;
using TombLib.Scripting.Resources;

namespace TombLib.Scripting.Lua.Objects
{
	public sealed class LuaCompletionData : ICompletionData, INotifyPropertyChanged
	{
		private const double DescriptionMaxWidth = 540.0;
		private const double DescriptionTextMaxWidth = 500.0;
		private static readonly SolidColorBrush DescriptionBorderBrush = TextEditorColorPalette.ToolTipBorder;
		private static readonly SolidColorBrush DescriptionBackgroundBrush = TextEditorColorPalette.ToolTipBackground;
		private static readonly SolidColorBrush DescriptionForegroundBrush = TextEditorColorPalette.ToolTipForeground;
		private readonly object _resolveSync = new object();
		private LuaCompletionItem _item;
		private string? _displayDetail;
		private object? _cachedDescription;
		private Task<LuaCompletionItem>? _resolveTask;

		public LuaCompletionData(LuaCompletionItem item)
		{
			_item = item ?? throw new ArgumentNullException(nameof(item));
			_displayDetail = FlattenSingleLineText(_item.Detail);
		}

		public event PropertyChangedEventHandler? PropertyChanged;

		public ImageSource Image => LuaCompletionIconFactory.GetIcon(_item.IconKind);
		public string Text => _item.FilterText;
		public string DisplayText => _item.Label;
		public string? DisplayDetail => _displayDetail;
		public Visibility DetailVisibility => string.IsNullOrEmpty(_displayDetail) ? Visibility.Collapsed : Visibility.Visible;
		public object Content => DisplayText;
		public object? Description => _cachedDescription ??= BuildDescriptionContent();
		public double Priority => _item.Priority;
		public bool CanResolve => _item.CanResolve;

		public void Complete(TextArea textArea, ISegment completionSegment, EventArgs insertionRequestEventArgs)
			=> textArea.Document.Replace(completionSegment, _item.InsertText);

		public async Task<object?> GetDescriptionAsync(CancellationToken cancellationToken = default)
		{
			if (!CanResolve)
				return Description;

			Task<LuaCompletionItem> resolveTask;

			lock (_resolveSync)
			{
				if (!CanResolve)
					return Description;

				if (_resolveTask is null || _resolveTask.IsCanceled || _resolveTask.IsFaulted)
					_resolveTask = _item.ResolveAsync(cancellationToken);

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

			string[] lines = text.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
			return lines.Length == 0 ? null : string.Join(" ", lines).Trim();
		}

		private object? BuildDescriptionContent()
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
}