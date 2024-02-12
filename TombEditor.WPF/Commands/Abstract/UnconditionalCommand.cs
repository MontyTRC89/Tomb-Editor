using System;
using System.ComponentModel;
using System.Windows.Input;

namespace TombEditor.WPF.Commands;

/// <summary>
/// A command where the <see cref="CanExecute(object)"/> method always returns <see langword="true" />.
/// </summary>
public abstract class UnconditionalCommand : ICommand
{
	public INotifyPropertyChanged Caller { get; init; }

	protected UnconditionalCommand(INotifyPropertyChanged caller)
		=> Caller = caller;

    public event EventHandler? CanExecuteChanged;
	public bool CanExecute(object? parameter) => true;
	public abstract void Execute(object? parameter);
}
