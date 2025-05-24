using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using CustomMessageBox.WPF;
using MvvmDialogs;
using TombLib.Messages;

namespace TombLib.Forms.ViewModels;

public partial class InputBoxViewModel : ObservableObject, IModalDialogViewModel
{
	[ObservableProperty] private string _title = string.Empty;
	[ObservableProperty] private string _label = "Please enter a value:";
	[ObservableProperty] private string _value = string.Empty;

	[ObservableProperty] private bool? _dialogResult;

	private readonly IMessenger _messenger;
	private readonly string[] _invalidNames = [];

	public InputBoxViewModel(IMessenger messenger)
	{
		_messenger = messenger;
	}

	public InputBoxViewModel(IMessenger messenger, string title, string label, string placeholder = null, params string[] invalidNames)
		: this(messenger)
	{
		Title = title;
		Label = label;
		Value = placeholder ?? string.Empty;

		_invalidNames = invalidNames;
	}

	[RelayCommand]
	private void Confirm()
	{
		foreach (string invalidName in _invalidNames)
		{
			if (Value.Equals(invalidName))
			{
				CMessageBox.Show("Invalid name, please choose another one.", "Invalid name", CMessageBoxButtons.OK, CMessageBoxIcon.Error, CMessageBoxDefaultButton.Button1);
				return;
			}
		}

		_messenger.Send(new InputBoxWindowCloseMessage(IsOK: true, Value));
	}

	[RelayCommand]
	private void Cancel()
		=> _messenger.Send(new InputBoxWindowCloseMessage(IsOK: false, Value));
}
