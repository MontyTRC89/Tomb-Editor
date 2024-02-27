using NLog;
using System.ComponentModel;
using TombLib.Forms;

namespace TombEditor.WPF.Commands;

/// <summary>
/// A command where the <see cref="CanExecute(object)"/> method always returns <see langword="true" />.
/// <para>It additionally bundles a reference to the <see cref="TombEditor.Editor"/> and <see cref="NLog.Logger"/> class.</para>
/// </summary>
public abstract class UnconditionalEditorCommand : UnconditionalCommand
{
	public Editor Editor { get; }
	public Logger Logger { get; }

	protected UnconditionalEditorCommand(INotifyPropertyChanged caller, Editor editor, Logger? logger = null) : base(caller)
    {
        Editor = editor;
		Logger = logger ?? LogManager.GetCurrentClassLogger();
    }

	protected bool CheckVersion(bool condition, string objectType)
	{
		if (!condition)
			Editor.SendMessage($"{objectType} is not supported in current game version.", PopupType.Info);

		return condition;
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
