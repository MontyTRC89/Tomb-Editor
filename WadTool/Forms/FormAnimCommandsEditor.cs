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
        private bool _closing;

        private readonly AnimationEditor _editor;
        private AnimationNode _animation;
        private readonly List<WadAnimCommand> _oldAnimCommands = new List<WadAnimCommand>();
        public IEnumerable<WadAnimCommand> AnimCommands => lstCommands.Items.Select(item => item.Tag).OfType<WadAnimCommand>();
        
        public FormAnimCommandsEditor(AnimationEditor editor, AnimationNode animation, WadAnimCommand startupCommand = null)
        {
            InitializeComponent();
            animCommandEditor.Initialize(editor);
            _editor = editor;
            Initialize(animation, startupCommand, true);
            _editor.Tool.EditorEventRaised += Tool_EditorEventRaised;
        }

        private void Initialize(AnimationNode animation, WadAnimCommand startupCommand, bool genBackup = false)
        {
            _animation = animation;
            if (genBackup) _oldAnimCommands.Clear();
            lstCommands.Items.Clear();

            // Backup existing animcommands and populate list
            foreach (var cmd in _animation.WadAnimation.AnimCommands)
            {
                if (genBackup) _oldAnimCommands.Add(cmd.Clone());
                lstCommands.Items.Add(new DarkListItem(cmd.ToString()) { Tag = cmd });
            }

            // Try to add and/or select start-up command
            if (startupCommand != null)
            {
                if (!_animation.WadAnimation.AnimCommands.Contains(startupCommand))
                {
                    _animation.WadAnimation.AnimCommands.Add(startupCommand);
                    lstCommands.Items.Add(new DarkListItem(startupCommand.ToString()) { Tag = startupCommand.Clone() });
                }
                SelectCommand(startupCommand);
            }
            else if (lstCommands.Items.Count > 0)
                SelectCommand((WadAnimCommand)lstCommands.Items.First().Tag);
            else
                SelectCommand(null);

            if (Visible && lstCommands.SelectedIndices.Count > 0)
                lstCommands.EnsureVisible();  // This code oly works when control is already visible
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
                _editor.Tool.EditorEventRaised -= Tool_EditorEventRaised;
            base.Dispose(disposing);
        }

        private void Tool_EditorEventRaised(IEditorEvent obj)
        {
            if (_closing) return;

            if (obj is WadToolClass.AnimationEditorCurrentAnimationChangedEvent)
            {
                var e = obj as WadToolClass.AnimationEditorCurrentAnimationChangedEvent;
                if (e != null && e.Animation != _animation)
                    Initialize(e.Animation, null);
            }

            if (obj is WadToolClass.AnimationEditorAnimationChangedEvent)
            {
                var e = obj as WadToolClass.AnimationEditorAnimationChangedEvent;
                if (e != null && e.Animation == _animation && e.Focus)
                    Initialize(e.Animation, null);
            }
        }

        protected override void OnShown(EventArgs e)
        {
            base.OnShown(e);
            if (lstCommands.SelectedIndices.Count > 0)
                lstCommands.EnsureVisible();
        }

        public void SelectCommand(WadAnimCommand cmd, bool selectInTree = true)
        {
            if (cmd != null && !_animation.WadAnimation.AnimCommands.Contains(cmd))
            {
                _animation.WadAnimation.AnimCommands.Add(cmd);
                lstCommands.Items.Add(new DarkListItem(cmd.ToString()) { Tag = cmd });
            }

            animCommandEditor.Command = cmd;

            if (cmd != null && selectInTree)
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

            if (lstCommands.Items.Count > 0)
                SelectCommand(lstCommands.Items.FirstOrDefault()?.Tag as WadAnimCommand);
            else
                SelectCommand(null);
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
                _editor.Tool.UndoManager.PushAnimationChanged(_editor, _animation);

            // Add new commands
            _animation.WadAnimation.AnimCommands.Clear();
            _animation.WadAnimation.AnimCommands.AddRange(AnimCommands);

            // Update state in parent window
            _editor.Tool.AnimationEditorAnimationChanged(_animation, false);
        }

        // Only actual data is updating, not UI, so it shouldn't be used for in-window undo.
        private void DiscardChanges(bool undo = true)
        {
            // Add the new state changes
            _animation.WadAnimation.AnimCommands.Clear();
            _animation.WadAnimation.AnimCommands.AddRange(_oldAnimCommands);

            // Update state in parent window
            _editor.Tool.AnimationEditorAnimationChanged(_animation, false);
        }

        public void UpdateSelectedItemText() => lstCommands.Items[lstCommands.SelectedIndices[0]].Text = lstCommands.Items[lstCommands.SelectedIndices[0]].Tag.ToString();

        private void butCommandUp_Click(object sender, EventArgs e) => MoveCommand(false);
        private void butCommandDown_Click(object sender, EventArgs e) => MoveCommand(true);
        private void butDeleteEffect_Click(object sender, EventArgs e) => DeleteCommand();

        private void butAddEffect_Click(object sender, EventArgs e)
        {
            var newCmd = new WadAnimCommand() { Type = WadAnimCommandType.SetPosition };
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
            Close();
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            base.OnFormClosing(e);
            _closing = true;

            if (DialogResult == DialogResult.OK) ApplyChanges();
            else DiscardChanges();

            WadSoundPlayer.StopSample();
        }

        private void lstCommands_SelectedIndicesChanged(object sender, EventArgs e)
        {
            if (lstCommands.Items.Count > 0 && lstCommands.SelectedIndices.Count > 0)
                SelectCommand((WadAnimCommand)(lstCommands.SelectedItem.Tag), false);
            else
                SelectCommand(null);
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
