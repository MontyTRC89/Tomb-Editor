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
        public Room SelectedRoom => roomListBox.SelectedItem as Room;

        public FormChooseRoom(string why, IEnumerable<Room> rooms, Action<Room> roomSelectionChanged)
        {
            InitializeComponent();
            this.SetActualSize();

            // Populate lists
            titelLabel.Text = why;
            foreach (Room room in rooms)
                roomListBox.Items.Add(room);

            // View room when the selection is changed
            roomListBox.SelectedIndexChanged += delegate
                {
                    butOk.Enabled = SelectedRoom != null;
                    if (SelectedRoom != null)
                        roomSelectionChanged(SelectedRoom);
                };
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
