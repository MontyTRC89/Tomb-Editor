#nullable enable

using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using TombIDE.ScriptingStudio.Objects;
using TombIDE.Shared;

namespace TombIDE.ScriptingStudio.ViewModels;

internal sealed class LuaReferencesResultsViewModel : INotifyPropertyChanged
{
	private readonly ObservableCollection<LuaReferenceGroup> _groups = [];
	private string _statusText = string.Empty;

	public event PropertyChangedEventHandler? PropertyChanged;

	public ObservableCollection<LuaReferenceGroup> Groups => _groups;

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

	public bool HasResults => _groups.Count > 0;

	public void ShowNoActiveDocument()
		=> ReplaceGroups([], Strings.Default.LuaReferencesNoDocument);

	public void ShowUnsupported()
		=> ReplaceGroups([], Strings.Default.LuaReferencesUnsupported);

	public void ShowLoading()
		=> ReplaceGroups([], Strings.Default.LuaReferencesLoading);

	public void ShowReferences(IReadOnlyList<LuaReferenceGroup> groups)
		=> ReplaceGroups(groups, groups.Count == 0 ? Strings.Default.NoReferencesFound : string.Empty);

	private void ReplaceGroups(IReadOnlyList<LuaReferenceGroup> groups, string statusText)
	{
		_groups.Clear();

		foreach (LuaReferenceGroup group in groups)
			_groups.Add(group);

		StatusText = statusText;
		OnPropertyChanged(nameof(HasResults));
	}

	private bool SetField<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
	{
		if (EqualityComparer<T>.Default.Equals(field, value))
			return false;

		field = value;
		OnPropertyChanged(propertyName);
		return true;
	}

	private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
		=> PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
}