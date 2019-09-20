using DarkUI.Forms;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using TombLib.Graphics;
using TombLib.Wad;

namespace WadTool
{
    public partial class FormReplaceAnimCommands : DarkForm
    {
        private readonly AnimationEditor _editor;
        private WadAnimCommand _backupCommand;

        public bool EditingWasDone { get; private set; }

        public FormReplaceAnimCommands(AnimationEditor editor, WadAnimCommand refCommand = null)
        {
            InitializeComponent();

            _editor = editor;
            aceFind.Initialize(_editor, true);
            aceReplace.Initialize(_editor, true);
            aceFind.Command = refCommand == null ? new WadAnimCommand() { Type = WadAnimCommandType.SetPosition } : refCommand;
            aceReplace.Command = new WadAnimCommand() { Type = WadAnimCommandType.SetPosition };

            UpdateUI();
        }

        private void UpdateUI()
        {
            bool searchDone = (dgvResults.Rows.Count != 0);
            bool allowActions = false;

            butDeselectAll.Enabled = searchDone;
            butSelectAll.Enabled = searchDone;

            if (searchDone)
                for (int i = 0; i < dgvResults.Rows.Count; i++)
                    if ((bool)dgvResults.Rows[i].Cells[0].Value == true)
                    { allowActions = true; break; }

            butReplace.Enabled = allowActions;
            butDelete.Enabled = allowActions;
        }

        private void SelectOrDeselectAll(bool select)
        {
            for (int i = 0; i < dgvResults.Rows.Count; i++)
                dgvResults.Rows[i].Cells[0].Value = select;
        }

        private void Search(bool newSearch = true)
        {
            // Reset previous search's state...
            dgvResults.Rows.Clear();

            // Store flipeffect settings in case user later changes them
            if (newSearch)
                _backupCommand = aceFind.Command.Clone();

            var collectedAnims = new List<int>();

            for (int i = 0; i < _editor.Animations.Count; i++)
            {
                var anim = _editor.Animations[i];

                foreach (var ac in anim.WadAnimation.AnimCommands)
                    if (WadAnimCommand.DistinctiveEquals(ac, _backupCommand, false))
                    {
                        if (!collectedAnims.Contains(i))
                            collectedAnims.Add(i);

                        string result = "Animation: " + anim.WadAnimation.Name + ", command: " + ac;
                        dgvResults.Rows.Add(false, anim.Index, result);
                    }
            }

            UpdateUI();
            if (newSearch)
                statusLabel.Text = "Search finished. Found " + dgvResults.Rows.Count + " matches in " + collectedAnims.Count + " animations.";
        }

        private void ReplaceOrDelete(bool delete = false)
        {
            // Enlist all animations which are pending for replacement
            var animsToUndo = new List<AnimationNode>();
            for (int i = 0; i < dgvResults.Rows.Count; i++)
            {
                var process = (bool)dgvResults.Rows[i].Cells[0].Value;
                var number  = (int) dgvResults.Rows[i].Cells[1].Value;

                if (process && !animsToUndo.Any(anim => anim.Index == number))
                    animsToUndo.Add(_editor.Animations[number]);
            }

            // Undo
            _editor.Tool.UndoManager.PushAnimationChanged(_editor, animsToUndo);

            int count = 0;
            int animCount = 0;
            int actionCount = 0;
            bool alreadyFound = false;

            for (int i = 0; i < _editor.Animations.Count; i++)
            {
                // Collect animcommand indices to remove later in reverse order
                // with RemoveAt function.
                List<int> indicesToDelete = new List<int>();

                for (int j = 0; j < _editor.Animations[i].WadAnimation.AnimCommands.Count; j++)
                {
                    alreadyFound = false;

                    var ac = _editor.Animations[i].WadAnimation.AnimCommands[j];
                    if (WadAnimCommand.DistinctiveEquals(ac, _backupCommand, false))
                    {
                        if ((bool)dgvResults.Rows[count].Cells[0].Value == true)
                        {
                            if (!alreadyFound) // Increase counter for statistics
                            {
                                animCount++;
                                alreadyFound = true;
                            }

                            if (delete)
                                indicesToDelete.Add(j);
                            else
                            {
                                var preparedCommand = aceReplace.Command.Clone();

                                // Preserve frame number in frame-based animcommands
                                if (preparedCommand.FrameBased && ac.FrameBased)
                                    preparedCommand.Parameter1 = ac.Parameter1;

                                _editor.Animations[i].WadAnimation.AnimCommands[j] = preparedCommand;
                            }
                            actionCount++; // Increase counter for statistics
                        }
                        count++;
                    }
                }
                // Remove previously collected indices in reverse order.
                if (indicesToDelete.Count > 0)
                    indicesToDelete
                        .OrderByDescending(a => a)
                        .ToList()
                        .ForEach(item => _editor.Animations[i].WadAnimation.AnimCommands.RemoveAt(item));
            }

            UpdateUI();
            EditingWasDone = true;
            animsToUndo.ForEach(anim => _editor.Tool.AnimationEditorAnimationChanged(anim, false));

            if (delete)
                statusLabel.Text = "Deleted " + actionCount + " animcommands in " + animCount + " animations.";
            else
                statusLabel.Text = "Replacement finished. Made " + actionCount + " replacements in " + animCount + " animations.";

            // Run one more extra pass to show deselected results
            Search(false);
        }


        private void dgvResults_CellContentClick(object sender, DataGridViewCellEventArgs e) => dgvResults.CommitEdit(DataGridViewDataErrorContexts.Commit);
        private void dgvResults_CellValueChanged(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0) return;

            if (dgvResults.Columns[e.ColumnIndex].Name == colReplaceFlag.Name)
                UpdateUI();
        }

        private void butSelectAll_Click(object sender, EventArgs e) => SelectOrDeselectAll(true);
        private void butDeselectAll_Click(object sender, EventArgs e) => SelectOrDeselectAll(false);
        private void butOK_Click(object sender, EventArgs e) => Close();
        private void butFind_Click(object sender, EventArgs e) => Search();
        private void butReplace_Click(object sender, EventArgs e) => ReplaceOrDelete();
        private void butDelete_Click(object sender, EventArgs e) => ReplaceOrDelete(true);
    }
}
