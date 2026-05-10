#nullable enable

using ICSharpCode.AvalonEdit.Document;
using System;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Data;
using TombIDE.ScriptingStudio.Objects;
using TombIDE.Shared;
using TombLib.Scripting.Objects;

namespace TombIDE.ScriptingStudio.ViewModels;

internal sealed class LuaDiagnosticsViewModel : INotifyPropertyChanged
{
	private readonly ObservableCollection<LuaDiagnosticListItem> _diagnostics = [];

	private LuaDiagnosticListItem? _selectedItem;
	private bool _showErrors = true;
	private bool _showWarnings = true;
	private bool _showMessages = true;
	private string _statusText = string.Empty;

	public LuaDiagnosticsViewModel()
	{
		Diagnostics = CollectionViewSource.GetDefaultView(_diagnostics);
		Diagnostics.Filter = FilterDiagnostic;

		ShowNoActiveDocument();
	}

	public event PropertyChangedEventHandler? PropertyChanged;

	public ICollectionView Diagnostics { get; }

	public LuaDiagnosticListItem? SelectedItem
	{
		get => _selectedItem;
		set => SetField(ref _selectedItem, value);
	}

	public bool ShowErrors
	{
		get => _showErrors;
		set
		{
			if (!SetField(ref _showErrors, value))
				return;

			Diagnostics.Refresh();
		}
	}

	public bool ShowWarnings
	{
		get => _showWarnings;
		set
		{
			if (!SetField(ref _showWarnings, value))
				return;

			Diagnostics.Refresh();
		}
	}

	public bool ShowMessages
	{
		get => _showMessages;
		set
		{
			if (!SetField(ref _showMessages, value))
				return;

			Diagnostics.Refresh();
		}
	}

	public string StatusText
	{
		get => _statusText;
		private set
		{
			if (!SetField(ref _statusText, value))
				return;

			OnPropertyChanged(nameof(HasStatusText));
		}
	}

	public bool HasStatusText => !string.IsNullOrWhiteSpace(StatusText);

	public string ErrorsLabel => Strings.Default.Errors;

	public string WarningsLabel => Strings.Default.Warnings;

	public string MessagesLabel => Strings.Default.Messages;

	public string SeverityHeader => Strings.Default.Severity;

	public string LineHeader => Strings.Default.LineHeader;

	public string ColumnHeader => Strings.Default.ColumnHeader;

	public string MessageHeader => Strings.Default.Message;

	public void ShowNoActiveDocument()
		=> ReplaceDiagnostics([], Strings.Default.LuaDiagnosticsNoDocument);

	public void ShowPending()
		=> ReplaceDiagnostics([], Strings.Default.LuaDiagnosticsUpdating);

	public void ShowDiagnostics(string filePath, TextDocument document, IReadOnlyList<TextEditorDiagnostic> diagnostics)
	{
		ArgumentNullException.ThrowIfNull(document);
		ArgumentNullException.ThrowIfNull(diagnostics);

		LuaDiagnosticListItem[] items = new LuaDiagnosticListItem[diagnostics.Count];

		for (int i = 0; i < diagnostics.Count; i++)
			items[i] = CreateItem(filePath, document, diagnostics[i]);

		ReplaceDiagnostics(items, items.Length == 0 ? Strings.Default.NoDiagnostics : string.Empty);
	}

	private void ReplaceDiagnostics(IReadOnlyList<LuaDiagnosticListItem> diagnostics, string statusText)
	{
		_diagnostics.Clear();

		foreach (LuaDiagnosticListItem diagnostic in diagnostics)
			_diagnostics.Add(diagnostic);

		SelectedItem = _diagnostics.Count > 0 ? _diagnostics[0] : null;
		StatusText = statusText;
		Diagnostics.Refresh();
	}

	private bool FilterDiagnostic(object item)
	{
		if (item is not LuaDiagnosticListItem diagnostic)
			return false;

		return diagnostic.Severity switch
		{
			TextEditorDiagnosticSeverity.Error => ShowErrors,
			TextEditorDiagnosticSeverity.Warning => ShowWarnings,
			_ => ShowMessages
		};
	}

	private static LuaDiagnosticListItem CreateItem(string filePath, TextDocument document, TextEditorDiagnostic diagnostic)
	{
		int documentLength = document.TextLength;
		int startOffset = Math.Max(0, Math.Min(diagnostic.StartOffset, documentLength));
		int endOffset = Math.Max(startOffset, Math.Min(diagnostic.EndOffset, documentLength));

		DocumentLine line = document.GetLineByOffset(startOffset);
		int columnNumber = startOffset - line.Offset + 1;

		return new LuaDiagnosticListItem(
			filePath,
			diagnostic.Severity,
			GetSeverityLabel(diagnostic.Severity),
			line.LineNumber,
			columnNumber,
			diagnostic.Message,
			startOffset,
			endOffset);
	}

	private static string GetSeverityLabel(TextEditorDiagnosticSeverity severity)
		=> severity switch
		{
			TextEditorDiagnosticSeverity.Error => Strings.Default.Error,
			TextEditorDiagnosticSeverity.Warning => Strings.Default.Warning,
			TextEditorDiagnosticSeverity.Information => Strings.Default.Information,
			TextEditorDiagnosticSeverity.Hint => Strings.Default.Hint,
			_ => Strings.Default.Message
		};

	private bool SetField<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
	{
		if (Equals(field, value))
			return false;

		field = value;
		OnPropertyChanged(propertyName);
		return true;
	}

	private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
		=> PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
}