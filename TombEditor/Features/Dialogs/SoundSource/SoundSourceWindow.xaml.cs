#nullable enable

using System.ComponentModel;
using System.Windows;

namespace TombEditor.Features.Dialogs.SoundSource;

public partial class SoundSourceWindow : Window
{
	public SoundSourceWindow()
	{
		InitializeComponent();
		Loaded += OnLoaded;
	}

	// Match legacy OnShown lstSounds.EnsureVisible(): scroll the current selection
	// into view on first show, plus on each subsequent selection change.
	private void OnLoaded(object sender, RoutedEventArgs e)
	{
		ScrollToSelection();
		if (DataContext is INotifyPropertyChanged vm)
			vm.PropertyChanged += OnVmPropertyChanged;
	}

	private void OnVmPropertyChanged(object? sender, PropertyChangedEventArgs e)
	{
		if (e.PropertyName == nameof(SoundSourceWindowViewModel.SelectedSound))
			ScrollToSelection();
	}

	private void ScrollToSelection()
	{
		if (SoundList.SelectedItem is not null)
			SoundList.ScrollIntoView(SoundList.SelectedItem);
	}
}
