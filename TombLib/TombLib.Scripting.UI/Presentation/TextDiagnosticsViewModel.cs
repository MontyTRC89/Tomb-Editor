#nullable enable

using CommunityToolkit.Mvvm.ComponentModel;
using ICSharpCode.AvalonEdit.Document;
using Nickelony.LanguageServer.Abstractions.Diagnostics;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Data;

namespace TombLib.Scripting.UI.Presentation;

public sealed partial class TextDiagnosticsViewModel : ObservableObject
{
	private readonly TextDiagnosticsPresentation _presentation;
	private readonly ObservableCollection<TextDiagnosticListItem> _diagnostics = [];

	[ObservableProperty]
	private TextDiagnosticListItem? _selectedItem;

	[ObservableProperty]
	private bool _showErrors = true;

	[ObservableProperty]
	private bool _showWarnings = true;

	[ObservableProperty]
	private bool _showMessages = true;

	[ObservableProperty]
	[NotifyPropertyChangedFor(nameof(HasStatusText))]
	private string _statusText = string.Empty;

	public TextDiagnosticsViewModel(TextDiagnosticsPresentation presentation)
	{
		_presentation = presentation ?? throw new ArgumentNullException(nameof(presentation));
		Diagnostics = CollectionViewSource.GetDefaultView(_diagnostics);
		Diagnostics.Filter = FilterDiagnostic;

		ShowNoActiveDocument();
	}

	public ICollectionView Diagnostics { get; }

	public bool HasStatusText => !string.IsNullOrWhiteSpace(StatusText);

	public string ErrorsLabel => _presentation.ErrorsLabel;

	public string WarningsLabel => _presentation.WarningsLabel;

	public string MessagesLabel => _presentation.MessagesLabel;

	public string SeverityHeader => _presentation.SeverityHeader;

	public string LineHeader => _presentation.LineHeader;

	public string ColumnHeader => _presentation.ColumnHeader;

	public string MessageHeader => _presentation.MessageHeader;

	public void ShowNoActiveDocument()
		=> ReplaceDiagnostics([], _presentation.NoActiveDocumentText);

	public void ShowPending()
		=> ReplaceDiagnostics([], _presentation.PendingText);

	public void ShowDiagnostics(string filePath, TextDocument document, IReadOnlyList<TextEditorDiagnostic> diagnostics)
	{
		ArgumentNullException.ThrowIfNull(document);
		ArgumentNullException.ThrowIfNull(diagnostics);

		TextDiagnosticListItem[] items = new TextDiagnosticListItem[diagnostics.Count];

		for (int i = 0; i < diagnostics.Count; i++)
			items[i] = CreateItem(filePath, document, diagnostics[i]);

		ReplaceDiagnostics(items, items.Length == 0 ? _presentation.EmptyText : string.Empty);
	}

	partial void OnShowErrorsChanged(bool value)
		=> Diagnostics.Refresh();

	partial void OnShowWarningsChanged(bool value)
		=> Diagnostics.Refresh();

	partial void OnShowMessagesChanged(bool value)
		=> Diagnostics.Refresh();

	private void ReplaceDiagnostics(IReadOnlyList<TextDiagnosticListItem> diagnostics, string statusText)
	{
		_diagnostics.Clear();

		foreach (TextDiagnosticListItem diagnostic in diagnostics)
			_diagnostics.Add(diagnostic);

		SelectedItem = _diagnostics.Count > 0 ? _diagnostics[0] : null;
		StatusText = statusText;
		Diagnostics.Refresh();
	}

	private bool FilterDiagnostic(object item)
	{
		if (item is not TextDiagnosticListItem diagnostic)
			return false;

		return diagnostic.Severity switch
		{
			TextEditorDiagnosticSeverity.Error => ShowErrors,
			TextEditorDiagnosticSeverity.Warning => ShowWarnings,
			_ => ShowMessages
		};
	}

	private static TextDiagnosticListItem CreateItem(string filePath, TextDocument document, TextEditorDiagnostic diagnostic)
	{
		int documentLength = document.TextLength;
		int startOffset = Math.Max(0, Math.Min(diagnostic.StartOffset, documentLength));
		int endOffset = Math.Max(startOffset, Math.Min(diagnostic.EndOffset, documentLength));

		DocumentLine line = document.GetLineByOffset(startOffset);
		int lineNumber = line.LineNumber;
		int columnNumber = Math.Max(1, startOffset - line.Offset + 1);

		return new TextDiagnosticListItem(
			filePath,
			diagnostic.Severity,
			diagnostic.Severity.ToString(),
			lineNumber,
			columnNumber,
			diagnostic.Message,
			startOffset,
			endOffset);
	}
}
