using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CustomMessageBox.WPF;
using MvvmDialogs;

namespace TombLib.Forms.ViewModels;

public partial class InputBoxWindowViewModel : ObservableObject, IModalDialogViewModel
{
	[ObservableProperty] private string _title = string.Empty;
	[ObservableProperty] private string _label = "Please enter a value:";
	[ObservableProperty] private string _value = string.Empty;

	[ObservableProperty] private bool? _dialogResult;

	private readonly string[] _invalidNames = [];

	public InputBoxWindowViewModel(string title, string label, string placeholder = null, params string[] invalidNames)
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
				CMessageBox.Show(
					"Invalid name, please choose another one.",
					"Invalid name",
					CMessageBoxButtons.OK,
					CMessageBoxIcon.Error,
					CMessageBoxDefaultButton.Button1
				);

				return;
			}
		}

		DialogResult = true;
	}

	[RelayCommand]
	private void Cancel()
		=> DialogResult = false;
}
