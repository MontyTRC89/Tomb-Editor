using System;
using System.Linq;
using System.Windows.Forms;

namespace TombEditor.Forms
{
    public partial class FormSelectRoomByTags : DarkUI.Forms.DarkForm
    {
        public bool findAllTags;
        public FormSelectRoomByTags(Editor editor)
        {
            InitializeComponent();

            tbTagSearch.AutocompleteWords.Clear();
            foreach (var room in (editor.Level.Rooms))
                if (room != null && room.ExistsInLevel)
                    tbTagSearch.AutocompleteWords.AddRange(room.Properties.Tags.Except(tbTagSearch.AutocompleteWords));
        }

        private void ButOk_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.OK;
        }

        private void ButCancel_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
        }

        private void CbAllTags_CheckedChanged(object sender, EventArgs e)
        {
            findAllTags = cbAllTags.Checked;
        }
    }
}
