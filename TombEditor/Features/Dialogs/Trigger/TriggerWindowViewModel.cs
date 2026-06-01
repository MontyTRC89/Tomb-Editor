#nullable enable

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MvvmDialogs;
using System;

namespace TombEditor.Features.Dialogs.Trigger;

public partial class TriggerWindowViewModel : ObservableObject, IModalDialogViewModel
{
	[ObservableProperty] private bool? _dialogResult;
	[ObservableProperty] private string _title = "Trigger editor";

	[ObservableProperty] private bool _isNG;
	[ObservableProperty] private bool _isTombEngine;

	[ObservableProperty] private bool _bit1;
	[ObservableProperty] private bool _bit2;
	[ObservableProperty] private bool _bit3;
	[ObservableProperty] private bool _bit4;
	[ObservableProperty] private bool _bit5;

	[ObservableProperty] private bool _oneShot;
	[ObservableProperty] private bool _oneShotEnabled = true;

	[ObservableProperty] private bool _rawMode;

	[ObservableProperty] private string _scriptText = string.Empty;
	[ObservableProperty] private bool _scriptEnabled;
	[ObservableProperty] private bool _copyAsAnimcommandEnabled;

	// Owner-supplied actions (set by the Window code-behind on construction).
	public Action? OnConfirm { get; set; }
	public Action? OnCopyToClipboard { get; set; }
	public Action? OnCopyWithComments { get; set; }
	public Action? OnCopyAsAnimcommand { get; set; }
	public Action? OnSearchTrigger { get; set; }

	[RelayCommand]
	private void Confirm()
	{
		// Defer to code-behind for validation; it sets DialogResult on success.
		OnConfirm?.Invoke();
	}

	[RelayCommand]
	private void Cancel() => DialogResult = false;

	[RelayCommand]
	private void CopyToClipboard() => OnCopyToClipboard?.Invoke();

	[RelayCommand]
	private void CopyWithComments() => OnCopyWithComments?.Invoke();

	[RelayCommand]
	private void CopyAsAnimcommand() => OnCopyAsAnimcommand?.Invoke();

	[RelayCommand]
	private void SearchTrigger() => OnSearchTrigger?.Invoke();
}
