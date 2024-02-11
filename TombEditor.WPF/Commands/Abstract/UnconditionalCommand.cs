using System;
using System.ComponentModel;
using System.Windows.Input;

namespace TombEditor.WPF.Commands;

public abstract class UnconditionalCommand : ICommand
{
	public INotifyPropertyChanged Caller { get; init; }

	protected UnconditionalCommand(INotifyPropertyChanged caller)
		=> Caller = caller;

    public event EventHandler? CanExecuteChanged;
	public bool CanExecute(object? parameter) => true;
	public abstract void Execute(object? parameter);
}
