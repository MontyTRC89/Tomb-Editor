using DarkUI.Controls;
using DarkUI.Forms;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using TombLib.Graphics;
using TombLib.Wad;

namespace WadTool
{
    public partial class FormAnimCommandsEditor : DarkForm
    {
        private readonly AnimationEditor _editor;
        private readonly AnimationNode _animation;
        private readonly List<WadAnimCommand> _oldAnimCommands = new List<WadAnimCommand>();
        public IEnumerable<WadAnimCommand> AnimCommands => lstCommands.Items.Select(item => item.Tag).OfType<WadAnimCommand>();
        
        public FormAnimCommandsEditor(AnimationEditor editor, AnimationNode animation, WadAnimCommand startupCommand = null)
        {
            InitializeComponent();
            animCommandEditor.Initialize(editor);
            _editor = editor;
            _animation = animation;

            // Backup existing animcommands and populate list
            foreach (var cmd in _animation.WadAnimation.AnimCommands)
            {
                _oldAnimCommands.Add(cmd.Clone());
                lstCommands.Items.Add(new DarkListItem(cmd.ToString()) { Tag = cmd });
            }

            // Try to add and/or select start-up command
            if (startupCommand != null)
            {
                if (!_animation.WadAnimation.AnimCommands.Contains(startupCommand))
                    lstCommands.Items.Add(new DarkListItem(startupCommand.ToString()) { Tag = startupCommand.Clone() });
                SelectCommand(startupCommand);
            }
            else if (lstCommands.Items.Count > 0)
                SelectCommand((WadAnimCommand)lstCommands.Items.First().Tag);

        }

        protected override void OnShown(EventArgs e)
        {
            base.OnShown(e);
            if (lstCommands.SelectedIndices.Count > 0)
                lstCommands.EnsureVisible();
        }

        public void SelectCommand(WadAnimCommand cmd, bool selectInTree = true)
        {
            animCommandEditor.Command = cmd;

            if (selectInTree)
            {
                for (int i = 0; i < lstCommands.Items.Count; i++)
                    if (lstCommands.Items[i].Text == cmd.ToString()) lstCommands.SelectItem(i);
                lstCommands.EnsureVisible();
            }
        }

        private void DeleteCommand()
        {
            if (lstCommands.SelectedIndices.Count == 0)
                return;

            for (int i = lstCommands.Items.Count - 1; i >= 0; i--)
                if (lstCommands.SelectedIndices.Contains(i))
                    lstCommands.Items.RemoveAt(i);

            SelectCommand(lstCommands.Items.FirstOrDefault()?.Tag as WadAnimCommand);
            ApplyChanges(false);
        }

        private void MoveCommand(bool down)
        {
            if (lstCommands.Items.Count <= 1)
                return;

            int index = lstCommands.SelectedIndices[0];
            int newIndex = down ? index + 1 : index - 1;
            var item = lstCommands.Items[index];

            if (down && index >= lstCommands.Items.Count - 1) return;
            if (!down && index <= 0) return;

            lstCommands.Items.RemoveAt(index);
            lstCommands.Items.Insert((newIndex), item);
            lstCommands.SelectItem(newIndex);
            lstCommands.EnsureVisible();

            ApplyChanges(false);
        }

        private void ApplyChanges(bool undo = true)
        {
            if (undo)
                _editor.Tool.UndoManager.PushAnimationChanged(_editor, _editor.CurrentAnim);

            // Add the new state changes
            _editor.CurrentAnim.WadAnimation.AnimCommands.Clear();
            _editor.CurrentAnim.WadAnimation.AnimCommands.AddRange(AnimCommands);

            // Update state in parent window
            _editor.Tool.AnimationEditorAnimationChanged(_editor.CurrentAnim);
        }

        // Only actual data is updating, not UI, so it shouldn't be used for in-window undo.
        private void DiscardChanges(bool undo = true)
        {
            // Add the new state changes
            _editor.CurrentAnim.WadAnimation.AnimCommands.Clear();
            _editor.CurrentAnim.WadAnimation.AnimCommands.AddRange(_oldAnimCommands);

            // Update state in parent window
            _editor.Tool.AnimationEditorAnimationChanged(_editor.CurrentAnim);
        }

        public void UpdateSelectedItemText() => lstCommands.Items[lstCommands.SelectedIndices[0]].Text = lstCommands.Items[lstCommands.SelectedIndices[0]].Tag.ToString();

        private void butCommandUp_Click(object sender, EventArgs e) => MoveCommand(false);
        private void butCommandDown_Click(object sender, EventArgs e) => MoveCommand(true);
        private void butDeleteEffect_Click(object sender, EventArgs e) => DeleteCommand();

        private void butAddEffect_Click(object sender, EventArgs e)
        {
            var newCmd = new WadAnimCommand() { Type = WadAnimCommandType.SetPosition };
            lstCommands.Items.Add(new DarkListItem(newCmd.ToString()) { Tag = newCmd });
            SelectCommand(newCmd);
            ApplyChanges(false);
        }

        private void btOk_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.OK;
            Close();
        }

        private void btCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            base.OnFormClosing(e);

            if (DialogResult == DialogResult.OK) ApplyChanges();
            else DiscardChanges();
        }

        private void lstCommands_SelectedIndicesChanged(object sender, EventArgs e)
        {
            if (lstCommands.Items.Count > 0 && lstCommands.SelectedIndices.Count > 0)
                SelectCommand((WadAnimCommand)(lstCommands.SelectedItem.Tag), false);
        }

        private void FormAnimCommandsEditor_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Delete)
                DeleteCommand();
        }

        private void animCommandEditor_AnimCommandChanged(object sender, AnimCommandEditor.AnimCommandEventArgs e)
        {
            if (e.Command != null &&
                lstCommands.Items.Count > 0 && lstCommands.SelectedIndices.Count > 0)
            {
                lstCommands.SelectedItem.Tag = e.Command;
                UpdateSelectedItemText();
                ApplyChanges(false);
            }
        }
    }
}
