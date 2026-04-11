using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using ICSharpCode.AvalonEdit.CodeCompletion;
using ICSharpCode.AvalonEdit.Document;
using ICSharpCode.AvalonEdit.Editing;
using TombLib.Scripting.Rendering;

namespace TombLib.Scripting.Lua.Objects
{
	public sealed class LuaCompletionData : ICompletionData
	{
		private const double DescriptionMaxWidth = 540.0;
		private const double DescriptionTextMaxWidth = 500.0;
		private static readonly SolidColorBrush DescriptionBorderBrush = CreateFrozenBrush(Color.FromRgb(96, 96, 96));
		private static readonly SolidColorBrush DescriptionBackgroundBrush = CreateFrozenBrush(Color.FromRgb(64, 64, 64));
		private static readonly SolidColorBrush DescriptionForegroundBrush = CreateFrozenBrush(Colors.Gainsboro);
		private readonly LuaCompletionItem _item;
		private readonly string _displayDetail;
		private object _cachedDescription;

		public LuaCompletionData(LuaCompletionItem item)
		{
			_item = item ?? throw new ArgumentNullException(nameof(item));
			_displayDetail = FlattenSingleLineText(_item.Detail);
		}

		public ImageSource Image => LuaCompletionIconFactory.GetIcon(_item.IconKind);
		public string Text => _item.FilterText;
		public string DisplayText => _item.Label;
		public string DisplayDetail => _displayDetail;
		public Visibility DetailVisibility => string.IsNullOrEmpty(_displayDetail) ? Visibility.Collapsed : Visibility.Visible;
		public object Content => DisplayText;
		public object Description => _cachedDescription ??= BuildDescriptionContent();
		public double Priority => _item.Priority;

		public void Complete(TextArea textArea, ISegment completionSegment, EventArgs insertionRequestEventArgs)
			=> textArea.Document.Replace(completionSegment, _item.InsertText);

		private static string FlattenSingleLineText(string text)
		{
			if (string.IsNullOrWhiteSpace(text))
				return null;

			string[] lines = text.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
			return lines.Length == 0 ? null : string.Join(" ", lines).Trim();
		}

		private object BuildDescriptionContent()
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
				panel.Children.Add(_item.IsDescriptionMarkdown
					? MarkdownToolTipRenderer.CreateContent(_item.Description, DescriptionForegroundBrush, DescriptionBackgroundBrush)
					: CreatePlainDescriptionContent(_item.Description));
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

		private static FrameworkElement CreatePlainDescriptionContent(string text)
		{
			var textBlock = new TextBlock
			{
				Text = text,
				Foreground = DescriptionForegroundBrush,
				TextWrapping = TextWrapping.Wrap,
				MaxWidth = DescriptionTextMaxWidth
			};

			return new ScrollViewer
			{
				Content = textBlock,
				MaxHeight = 420.0,
				MaxWidth = DescriptionMaxWidth,
				VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
				HorizontalScrollBarVisibility = ScrollBarVisibility.Disabled,
				CanContentScroll = true
			};
		}

		private static SolidColorBrush CreateFrozenBrush(Color color)
		{
			var brush = new SolidColorBrush(color);
			brush.Freeze();
			return brush;
		}
	}
}