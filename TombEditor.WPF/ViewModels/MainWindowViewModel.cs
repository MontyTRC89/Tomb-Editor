using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace TombEditor.WPF.ViewModels;

public partial class MainWindowViewModel : ObservableObject
{
    public ICommand NewLevelCommand { get; }
	public ICommand OpenLevelCommand { get; }
    public ICommand SaveLevelCommand { get; }
    public ICommand SaveLevelAsCommand { get; }
	public ICommand ImportPrjCommand { get; }
	public ICommand ConvertLevelToTombEngineCommand { get; }
	public ICommand BuildAndPlayCommand { get; }
    public ICommand BuildLevelCommand { get; }
    public ICommand QuitEditorCommand { get; }

	public ICommand UndoCommand { get; }
    public ICommand RedoCommand { get; }
    public ICommand CutCommand { get; }
    public ICommand CopyCommand { get; }
    public ICommand PasteCommand { get; }
    public ICommand StampObjectCommand { get; }
    public ICommand DeleteCommand { get; }

    private readonly Editor _editor;

	public MainWindowViewModel(Editor editor)
    {
        _editor = editor;

		NewLevelCommand = CommandHandler.GetCommand("NewLevel", new CommandArgs(this, _editor));
        OpenLevelCommand = CommandHandler.GetCommand("OpenLevel", new CommandArgs(this, _editor));
        SaveLevelCommand = CommandHandler.GetCommand("SaveLevel", new CommandArgs(this, _editor));
        SaveLevelAsCommand = CommandHandler.GetCommand("SaveLevelAs", new CommandArgs(this, _editor));
        ImportPrjCommand = CommandHandler.GetCommand("ImportPrj", new CommandArgs(this, _editor));
		ConvertLevelToTombEngineCommand = CommandHandler.GetCommand("ConvertLevelToTombEngine", new CommandArgs(this, _editor));
        BuildAndPlayCommand = CommandHandler.GetCommand("BuildAndPlay", new CommandArgs(this, _editor));
        BuildLevelCommand = CommandHandler.GetCommand("BuildLevel", new CommandArgs(this, _editor));
        QuitEditorCommand = CommandHandler.GetCommand("QuitEditor", new CommandArgs(this, _editor));

        UndoCommand = CommandHandler.GetCommand("Undo", new CommandArgs(this, _editor));
        RedoCommand = CommandHandler.GetCommand("Redo", new CommandArgs(this, _editor));
        CutCommand = CommandHandler.GetCommand("Cut", new CommandArgs(this, _editor));
        CopyCommand = CommandHandler.GetCommand("Copy", new CommandArgs(this, _editor));
        PasteCommand = CommandHandler.GetCommand("Paste", new CommandArgs(this, _editor));
        StampObjectCommand = CommandHandler.GetCommand("StampObject", new CommandArgs(this, _editor));
        DeleteCommand = CommandHandler.GetCommand("Delete", new CommandArgs(this, _editor));
	}
}
