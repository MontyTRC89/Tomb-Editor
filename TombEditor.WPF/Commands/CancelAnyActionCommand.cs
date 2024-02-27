using NLog;
using System.ComponentModel;

namespace TombEditor.WPF.Commands;

internal sealed class CancelAnyActionCommand : UnconditionalEditorCommand
{
	public CancelAnyActionCommand(INotifyPropertyChanged caller, Editor editor, Logger? logger = null) : base(caller, editor, logger)
	{ }

	public override void Execute(object? parameter)
		=> Editor.CancelAnyAction();
}
