using DarkUI.Forms;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using TombEditor.Geometry;

namespace TombEditor
{
    public partial class FormChooseRoom : DarkForm
    {
        public Room SelectedRoom => roomListBox.SelectedItem as Room;

        public FormChooseRoom(string why, IEnumerable<Room> rooms, Action<Room> roomSelectionChanged)
        {
            InitializeComponent();

            titelLabel.Text = why;
            foreach (Room room in rooms)
                roomListBox.Items.Add(room);

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
