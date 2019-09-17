using DarkUI.Forms;
using System;
using System.Collections.Generic;
using System.Windows.Forms;
using TombLib.Wad;

namespace WadTool
{
    public partial class FormReplaceAnimCommands : DarkForm
    {
        private AnimationEditor _editor;

        private WadAnimCommand _backupCommand;
        private List<bool> results = new List<bool>();

        public FormReplaceAnimCommands(AnimationEditor editor, WadAnimCommand refCommand = null)
        {
            InitializeComponent();

            _editor = editor;
            aceFind.Initialize(_editor, true);
            aceReplace.Initialize(_editor, true);
            aceFind.Command = refCommand == null ? new WadAnimCommand() { Type = WadAnimCommandType.SetPosition } : refCommand;
            aceReplace.Command = new WadAnimCommand() { Type = WadAnimCommandType.SetPosition };
        }

        private void Search()
        {
            dgvResults.Rows.Clear();

            // Store flipeffect settings in case user later changes them
            _backupCommand = aceFind.Command.Clone(); 

            foreach (var anim in _editor.Animations)
                foreach (var ac in anim.WadAnimation.AnimCommands)
                    if (WadAnimCommand.DistinctiveEquals(ac, aceFind.Command, false))
                    {
                        string result = "Animation: (" + anim.Index + ") " + anim.WadAnimation.Name + ", command: " + ac;
                        dgvResults.Rows.Add(false, result);
                    }
        }

        private void UpdateUI()
        {
            bool searchDone = (results != null && results.Count > 0);
            butDeselectAll.Enabled = searchDone;
            butSelectAll.Enabled = searchDone;
            butReplace.Enabled = searchDone;
        }

        private void SelectOrDeselectAll(bool select)
        {
            for (int i = 0; i < dgvResults.Rows.Count; i++)
                dgvResults.Rows[i].Cells[0].Value = select;
        }

        private void butSelectAll_Click(object sender, EventArgs e) => SelectOrDeselectAll(true);
        private void butDeselectAll_Click(object sender, EventArgs e) => SelectOrDeselectAll(false);
        private void butOK_Click(object sender, EventArgs e) => Close();
        private void butFind_Click(object sender, EventArgs e) => Search();

        private void butReplace_Click(object sender, EventArgs e)
        {
            if (DarkMessageBox.Show(this, "Batch replace can't be undone. To rollback, close animation window without changes. Continue?", 
                "Confirm batch replace", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Cancel)
                return;

            int count = 0;

            for (int i = 0; i < _editor.Animations.Count; i++)
                for (int j = 0; j < _editor.Animations[i].WadAnimation.AnimCommands.Count; j++)
                {
                    var ac = _editor.Animations[i].WadAnimation.AnimCommands[j];
                    if (WadAnimCommand.DistinctiveEquals(ac, _backupCommand, false))
                    {
                        if ((bool)dgvResults.Rows[count].Cells[0].Value)
                        {
                            var preparedCommand = aceReplace.Command.Clone();

                            // Preserve frame number in frame-based animcommands
                            if (ac.FrameBased)
                                preparedCommand.Parameter1 = ac.Parameter1;

                            ac = preparedCommand;
                        }
                        count++;
                    }
                }

            // Run one more extra pass to show deselected results
            Search();
        }
    }
}
