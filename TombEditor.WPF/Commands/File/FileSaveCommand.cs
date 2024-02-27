using MvvmDialogs.FrameworkDialogs.SaveFile;
using NLog;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows;
using TombLib.Forms;
using TombLib.LevelData.IO;

namespace TombEditor.WPF.Commands;

internal sealed class FileSaveCommand : UnconditionalEditorCommand
{
	private readonly bool _askForPath;

	public FileSaveCommand(INotifyPropertyChanged caller, Editor editor, bool askForPath, Logger? logger = null) : base(caller, editor, logger)
		=> _askForPath = askForPath;

	public override void Execute(object? parameter)
		=> Editor.SaveLevel(Caller, _askForPath);
}
