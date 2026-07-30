#nullable enable

using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace TombLib.Scripting.UI.Presentation;

public sealed partial class TextReferencesResultsViewModel : ObservableObject
{
	private readonly TextReferencesPresentation _presentation;
	private readonly ObservableCollection<TextReferenceGroup> _groups = [];

	[ObservableProperty]
	[NotifyPropertyChangedFor(nameof(HasStatusText))]
	private string _statusText = string.Empty;

	public TextReferencesResultsViewModel(TextReferencesPresentation presentation)
		=> _presentation = presentation ?? throw new ArgumentNullException(nameof(presentation));

	public ObservableCollection<TextReferenceGroup> Groups => _groups;

	public bool HasStatusText => !string.IsNullOrWhiteSpace(StatusText);

	public bool HasResults => _groups.Count > 0;

	public void ShowNoActiveDocument()
		=> ReplaceGroups([], _presentation.NoActiveDocumentText);

	public void ShowUnsupported()
		=> ReplaceGroups([], _presentation.UnsupportedText);

	public void ShowLoading()
		=> ReplaceGroups([], _presentation.LoadingText);

	public void ShowReferences(IReadOnlyList<TextReferenceGroup> groups)
		=> ReplaceGroups(groups, groups.Count == 0 ? _presentation.EmptyText : string.Empty);

	private void ReplaceGroups(IReadOnlyList<TextReferenceGroup> groups, string statusText)
	{
		_groups.Clear();

		foreach (TextReferenceGroup group in groups)
			_groups.Add(group);

		StatusText = statusText;
		OnPropertyChanged(nameof(HasResults));
	}
}
