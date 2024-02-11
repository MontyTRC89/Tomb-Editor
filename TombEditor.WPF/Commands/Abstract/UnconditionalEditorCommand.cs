using NLog;
using System.ComponentModel;
using TombLib.Forms;

namespace TombEditor.WPF.Commands;

public abstract class UnconditionalEditorCommand : UnconditionalCommand
{
	public Editor Editor { get; }
	public Logger Logger { get; }

	protected UnconditionalEditorCommand(INotifyPropertyChanged caller, Editor editor, Logger? logger = null) : base(caller)
    {
        Editor = editor;
		Logger = logger ?? LogManager.GetCurrentClassLogger();
    }

	protected bool VersionCheck(bool supported, string objectType)
	{
		if (!supported)
			Editor.SendMessage($"{objectType} is not supported in current game version.", PopupType.Info);

		return supported;
	}

	protected bool CheckForRoomAndBlockSelection()
	{
		if (Editor.SelectedRoom == null || !Editor.SelectedSectors.Valid)
		{
			Editor.SendMessage("Please select a valid group of sectors.", PopupType.Error);
			return false;
		}

		return true;
	}
}
