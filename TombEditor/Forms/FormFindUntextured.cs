using DarkUI.Forms;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using TombLib;
using TombLib.LevelData;

namespace TombEditor.Forms
{
    public partial class FormFindUntextured : DarkForm
    {
        private readonly Editor _editor;
        private List<KeyValuePair<Room, VectorInt2>> _list;
        private bool _lockSelection = true;
        private bool _firstLaunch = true;

        public FormFindUntextured(Editor editor)
        {
            _editor = editor;
            _editor.EditorEventRaised += _editor_EditorEventRaised;
            InitializeComponent();

            // Set window property handlers
            Configuration.LoadWindowProperties(this, _editor.Configuration);
            FormClosing += new FormClosingEventHandler((s, e) => Configuration.SaveWindowProperties(this, _editor.Configuration));

            InitializeNewSearch();
        }
        private void _editor_EditorEventRaised(IEditorEvent obj)
        {
            // Level has changed, reset everything
            if (obj is Editor.LevelChangedEvent)
                InitializeNewSearch();

            // Re-toggle current object on changed object selection
            if (obj is Editor.RoomListChangedEvent ||
                obj is Editor.RoomGeometryChangedEvent ||
                obj is Editor.RoomPropertiesChangedEvent)
            {
                _list = _list.Where(item => _editor.Level.Rooms.Contains(item.Key)).ToList();
                RefreshList();
            }
        }

        private void InitializeNewSearch()
        {
            _list = EditorActions.FindUntextured();
            RefreshList();
        }

        private void RefreshList()
        {
            _lockSelection = true;
            dgvUntextured.Rows.Clear();
            foreach (var entry in _list)
                dgvUntextured.Rows.Add(entry.Key.Name, (entry.Value.X.ToString() + ", " + entry.Value.Y.ToString()));

            dgvUntextured.ClearSelection();
            _lockSelection = false;
        }

        private void dgvUntextured_SelectionChanged(object sender, EventArgs e)
        {
            if (_firstLaunch)
            {
                _firstLaunch = false;
                dgvUntextured.ClearSelection();
                return;
            }

            if (_lockSelection || dgvUntextured.SelectedCells.Count == 0) return;
            var entry = _list[dgvUntextured.SelectedCells[0].RowIndex];

            if (_editor.Level.Rooms.Contains(entry.Key))
                _editor.SelectRoom(entry.Key);

            if (!_editor.SelectedRoom.CoordinateInvalid(entry.Value))
                _editor.SelectedSectors = new SectorSelection() { Start = entry.Value, End = entry.Value };

            _editor.MoveCameraToSector(entry.Value);
        }

        private void butNewSearch_Click(object sender, EventArgs e) => InitializeNewSearch();

        private void butCancel_Click(object sender, EventArgs e) => Close();
    }
}
