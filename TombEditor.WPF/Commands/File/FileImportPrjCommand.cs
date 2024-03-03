using NLog;
using System.ComponentModel;

namespace TombEditor.WPF.Commands;

internal sealed class FileImportPrjCommand : UnconditionalEditorCommand
{
	public FileImportPrjCommand(INotifyPropertyChanged caller, Editor editor, Logger? logger = null) : base(caller, editor, logger)
	{ }

	public override void Execute(object? parameter)
		=> Editor.OpenLevelPrj(Caller);
}
