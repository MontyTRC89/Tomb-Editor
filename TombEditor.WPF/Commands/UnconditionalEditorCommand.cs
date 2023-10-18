using NLog;
using TombLib.Forms;

namespace TombEditor.WPF.Commands;

public abstract class UnconditionalEditorCommand : UnconditionalCommand
{
	public Editor Editor { get; }
	public Logger Logger { get; }

    public UnconditionalEditorCommand(Editor editor, Logger? logger = null)
    {
        Editor = editor;
		Logger = logger ?? LogManager.GetCurrentClassLogger();
    }

	public bool VersionCheck(bool supported, string objectType)
	{
		if (!supported)
			Editor.SendMessage($"{objectType} is not supported in current game version.", PopupType.Info);

		return supported;
	}
}
