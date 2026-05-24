using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;
using ICSharpCode.AvalonEdit.Document;
using ICSharpCode.AvalonEdit.Rendering;
using TextMateSharp.Model;

namespace TombLib.Scripting.UI.Highlighting
{
	internal sealed class TextMateColorizingTransformer : DocumentColorizingTransformer, IDisposable, IModelTokensChangedListener
	{
		private readonly TextView _textView;
		private readonly TMModel _model;
		private readonly TextMateThemeStyleResolver _styleResolver;
		private bool _isDisposed;

		public TextMateColorizingTransformer(TextView textView, TMModel model, TextMateThemeStyleResolver styleResolver)
		{
			_textView = textView ?? throw new ArgumentNullException(nameof(textView));
			_model = model ?? throw new ArgumentNullException(nameof(model));
			_styleResolver = styleResolver ?? throw new ArgumentNullException(nameof(styleResolver));

			_model.AddModelTokensChangedListener(this);
		}

		protected override void ColorizeLine(DocumentLine line)
		{
			if (_isDisposed || line is null)
				return;

			int lineIndex = Math.Max(0, line.LineNumber - 1);
			List<TMToken> tokens = _model.GetLineTokens(lineIndex);

			if (tokens is null || _model.IsLineInvalid(lineIndex))
			{
				_model.ForceTokenization(lineIndex);
				tokens = _model.GetLineTokens(lineIndex);
			}

			if (tokens is null || tokens.Count == 0)
				return;

			int lineLength = line.Length;

			for (int i = 0; i < tokens.Count; i++)
			{
				TMToken token = tokens[i];
				int startIndex = ClampToLine(token.StartIndex, lineLength);
				int endIndex = i + 1 < tokens.Count
					? ClampToLine(tokens[i + 1].StartIndex, lineLength)
					: lineLength;

				if (endIndex <= startIndex)
					continue;

				TextMateHighlightingStyle style = _styleResolver.Resolve(token.Scopes);

				if (!style.HasFormatting)
					continue;

				int startOffset = line.Offset + startIndex;
				int endOffset = line.Offset + endIndex;

				ChangeLinePart(startOffset, endOffset, element => ApplyStyle(element, style));
			}
		}

		public void Dispose()
		{
			if (_isDisposed)
				return;

			_isDisposed = true;
			_model.RemoveModelTokensChangedListener(this);
		}

		void IModelTokensChangedListener.ModelTokensChanged(ModelTokensChangedEvent e)
		{
			if (_isDisposed)
				return;

			// Always defer to avoid reentrancy during visual line construction.
			_textView.Dispatcher.BeginInvoke(new Action(() =>
			{
				if (!_isDisposed)
					_textView.Redraw();
			}));
		}

		private static int ClampToLine(int index, int lineLength)
			=> Math.Max(0, Math.Min(index, lineLength));

		private static void ApplyStyle(VisualLineElement element, TextMateHighlightingStyle style)
		{
			VisualLineElementTextRunProperties properties = element.TextRunProperties;

			if (style.Foreground is not null)
				properties.SetForegroundBrush(style.Foreground);

			if (style.IsBold || style.IsItalic)
				properties.SetTypeface(style.CreateTypeface(properties.Typeface));

			if (style.TextDecorations is not null)
				properties.SetTextDecorations(style.TextDecorations);
		}
	}
}