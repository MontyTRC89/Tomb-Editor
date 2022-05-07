using System;
using System.Collections.Generic;
using System.Windows.Forms;
using DarkUI.Forms;
using TombLib.LevelData;
using TombLib.Utils;

namespace TombEditor.Forms
{
    public partial class FormChooseRoom : DarkForm
    {
        public Room SelectedRoom => lstRooms.SelectedItem?.Tag as Room;

        public FormChooseRoom(string why, IEnumerable<Room> rooms, Action<Room> roomSelectionChanged)
        {
            InitializeComponent();
            this.SetActualSize();

            // Populate lists
            titelLabel.Text = why;
            foreach (Room room in rooms)
                lstRooms.Items.Add(new DarkUI.Controls.DarkListItem(room.Name) { Tag = room });

            // View room when the selection is changed
            lstRooms.SelectedIndicesChanged += delegate
            {
                butOk.Enabled = SelectedRoom != null;
                if (SelectedRoom != null)
                    roomSelectionChanged(SelectedRoom);
            };
        }
        private void lstRooms_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            if (SelectedRoom == null)
                return;

            DialogResult = DialogResult.OK;
            Close();
        }

        private void butOk_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.OK;
            Close();
        }

        private void butCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }
    }
}
