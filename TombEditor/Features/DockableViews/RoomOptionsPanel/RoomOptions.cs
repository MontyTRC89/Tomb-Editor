using DarkUI.Docking;

namespace TombEditor.Features.DockableViews.RoomOptionsPanel;

public partial class RoomOptions : DarkToolWindow
{
	private readonly Editor _editor;

	public RoomOptions()
	{
		InitializeComponent();
		_roomOptionsView.SetWinFormsHost(_elementHost);
		_editor = Editor.Instance;
		_editor.EditorEventRaised += EditorEventRaised;
	}

	protected override void Dispose(bool disposing)
	{
		if (disposing)
		{
			_editor.EditorEventRaised -= EditorEventRaised;
			_roomOptionsView?.Cleanup();
			components?.Dispose();
		}

		base.Dispose(disposing);
	}

	private void EditorEventRaised(IEditorEvent obj)
	{
		if (obj is Editor.DefaultControlActivationEvent activationEvent &&
			DockPanel is not null &&
			activationEvent.ContainerName == GetType().Name)
		{
			MakeActive();
			_elementHost.Focus();
			_roomOptionsView.FocusRoomSearch();
		}
	}
}
