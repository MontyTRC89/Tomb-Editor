using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using TombLib.Graphics;
using TombLib.Utils;
using TombLib.Wad;

namespace WadTool
{
    public partial class FormSpriteSequencesEditor : DarkUI.Forms.DarkForm
    {
        private WadToolClass _tool;

        public FormSpriteSequencesEditor()
        {
            InitializeComponent();

            _tool = WadToolClass.Instance;
            ReloadSpriteSequences();
        }

        private void ReloadSpriteSequences()
        {
            lstSequences.Items.Clear();

            foreach (var sequence in _tool.DestinationWad.SpriteSequences)
            {
                var item = new DarkUI.Controls.DarkListItem(sequence.ToString());
                item.Tag = sequence;
                lstSequences.Items.Add(item);
            }
        }

        private void FormSpriteEditor_Load(object sender, EventArgs e)
        {

        }

        private void butDeleteSequence_Click(object sender, EventArgs e)
        {
            if (lstSequences.SelectedIndices.Count == 0) return;

            // Get the current sequence
            var item = lstSequences.Items[lstSequences.SelectedIndices[0]];
            var sequence = (WadSpriteSequence)item.Tag;

            // Ask to the user the permission to delete sprite
            if (DarkUI.Forms.DarkMessageBox.ShowWarning(
                   "Are you really sure to delete '" + sequence.ToString() + "'?",
                   "Delete sequence", DarkUI.Forms.DarkDialogButton.YesNo) != DialogResult.Yes)
                return;

            _tool.DestinationWad.DeleteSpriteSequence(sequence);

            ReloadSpriteSequences();
        }

        private void butAddNewSequence_Click(object sender, EventArgs e)
        {
            var sequence = new WadSpriteSequence();

            var form = new FormSpriteEditor();
            form.SpriteSequence = sequence;
            if (form.ShowDialog() == DialogResult.Cancel) return;

            _tool.DestinationWad.SpriteSequences.Add(form.SpriteSequence);

            ReloadSpriteSequences();
        }

        private void lstSequences_DoubleClick(object sender, EventArgs e)
        {
            butEditSequence_Click(null, null);
        }

        private void butEditSequence_Click(object sender, EventArgs e)
        {
            if (lstSequences.SelectedIndices.Count == 0) return;

            // Get the current sequence
            var item = lstSequences.Items[lstSequences.SelectedIndices[0]];
            var sequence = (WadSpriteSequence)item.Tag;

            var form = new FormSpriteEditor();
            form.SpriteSequence = sequence;
            if (form.ShowDialog() == DialogResult.Cancel) return;

            ReloadSpriteSequences();
        }
    }
}
