using NLog;
using System.ComponentModel;

namespace TombEditor.WPF.Commands;

internal sealed class FileOpenCommand : UnconditionalEditorCommand
{
	public FileOpenCommand(INotifyPropertyChanged caller, Editor editor, Logger? logger = null) : base(caller, editor, logger)
	{ }

	public override void Execute(object? parameter)
		=> Editor.OpenLevel(Caller);
}
