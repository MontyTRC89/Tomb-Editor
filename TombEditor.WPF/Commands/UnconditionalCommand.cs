using System;
using System.Windows.Input;

namespace TombEditor.WPF.Commands;

public abstract class UnconditionalCommand : ICommand
{
	public event EventHandler? CanExecuteChanged;
	public bool CanExecute(object? parameter) => true;
	public abstract void Execute(object? parameter);
}
