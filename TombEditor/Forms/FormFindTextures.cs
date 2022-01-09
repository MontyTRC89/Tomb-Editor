using DarkUI.Forms;
using System;
using System.Collections.Generic;
using System.Linq;
using TombLib;
using TombLib.LevelData;
using TombLib.Utils;

namespace TombEditor.Forms
{
    public partial class FormFindTextures : DarkForm
    {
        private const uint _maxEntries = 1000;

        private readonly Editor _editor;
        private List<KeyValuePair<Room, VectorInt2>> _list;
        private bool _lockSelection = true;
        private bool _firstLaunch = true;

        public FormFindTextures(Editor editor)
        {
            _editor = editor;
            _editor.EditorEventRaised += _editor_EditorEventRaised;
            InitializeComponent();

            cbSearchType.SelectedIndex = 0;

            // Set window property handlers
            Configuration.ConfigureWindow(this, _editor.Configuration);
        }
        
        protected override void Dispose(bool disposing)
        {
            if (disposing)
                _editor.EditorEventRaised -= _editor_EditorEventRaised;
            base.Dispose(disposing);
        }

        private void _editor_EditorEventRaised(IEditorEvent obj)
        {
            // Level has changed, reset everything
            if (obj is Editor.LevelChangedEvent)
                InitializeNewSearch();

            // Re-toggle current object on changed object selection
            if (obj is Editor.RoomListChangedEvent)
            {
                _list = _list.Where(item => _editor.Level.Rooms.Contains(item.Key)).ToList();
                RefreshList();
            }
        }

        private void InitializeNewSearch()
        {
            bool tooManyEntries = false;
            _list = EditorActions.FindTextures((TextureSearchType)cbSearchType.SelectedIndex, 
                _editor.SelectedTexture.GetCanonicalTexture(_editor.SelectedTexture.TextureIsTriangle), 
                cbSelectedRooms.Checked, _maxEntries, out tooManyEntries);

            lblStatus.Text = "Search finished. ";
            if (tooManyEntries)
                lblStatus.Text += "Too many entries, showing first " + _list.Count + ".";
            else if (_list.Count == 0)
                lblStatus.Text += "No entries found.";
            else
                lblStatus.Text += "Found " + _list.Count + " entries.";

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
            // A hack to remove selection from empty table on non-modal form
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
