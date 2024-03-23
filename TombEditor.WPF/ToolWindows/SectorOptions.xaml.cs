using System.Windows.Controls;
using TombEditor.Controls;
using TombEditor.WPF.ViewModels;

namespace TombEditor.WPF.ToolWindows;

public partial class SectorOptions : UserControl
{
	private readonly Editor _editor;
	private readonly Panel2DGrid panel2DGrid;

	public SectorOptions()
	{
		InitializeComponent();
		DataContext = new SectorOptionsViewModel(Editor.Instance);

		_editor = Editor.Instance;
		_editor.EditorEventRaised += EditorEventRaised;

		panel2DGrid = new Panel2DGrid()
		{
			Room = _editor.SelectedRoom
		};

		Panel2DHost.Child = panel2DGrid;
	}

	private void EditorEventRaised(IEditorEvent obj)
	{
		if (obj is Editor.SelectedRoomChangedEvent selectedRoomChangedEvent)
			panel2DGrid.Room = selectedRoomChangedEvent.Current;
	}
}
