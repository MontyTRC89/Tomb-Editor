using DarkUI.Forms;
using System;
using System.Collections.Generic;
using System.ComponentModel;
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
        private BindingList<WadAnimCommand> _animCommands = new BindingList<WadAnimCommand>();
        private List<WadAnimCommand> _backupCommands;

        public FormAnimCommandsEditor(AnimationEditor editor, AnimationNode animation)
        {
            InitializeComponent();
            animCommandEditor.Initialize(editor);
            _editor = editor;
            Initialize(animation);
            
            _editor.Tool.EditorEventRaised += Tool_EditorEventRaised;

            // Set window property handlers
            Configuration.ConfigureWindow(this, _editor.Tool.Configuration);
        }

        private void Initialize(AnimationNode animation)
        {
            _animation = animation;
            
            // Copy current set of animation commands to backup list and directly reference
            // current set of animcommands to data grid view, so live preview is possible.
            // Backup list is restored if user pushes cancel or changes animation in parent window.

            _backupCommands = new List<WadAnimCommand>();
            foreach (var ac in animation.WadAnimation.AnimCommands)
                _backupCommands.Add(ac.Clone()); // Do deep copy
            _animCommands = new BindingList<WadAnimCommand>(animation.WadAnimation.AnimCommands);
            gridViewCommands.DataSource = _animCommands;
        }

        private void AnimCommandEditor_AnimCommandChanged(object sender, AnimCommandEditor.AnimCommandEventArgs e)
        {
            if (gridViewCommands.SelectedRows.Count == 1)
            {
                int index = gridViewCommands.SelectedRows[0].Index;
                _animCommands[index] = e.Command.Clone();
                Invalidate();
                _editor.Tool.AnimationEditorAnimcommandChanged();
            }
        }

        private void GridViewCommands_SelectionChanged(object sender, EventArgs e)
        {
            if (gridViewCommands.SelectedRows.Count == 1)
            {
                int index = gridViewCommands.SelectedRows[0].Index;
                WadAnimCommand cmd = _animCommands[index];
                animCommandEditor.Command = cmd;
            }
            else return;
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
                if (e != null && e.Current != _animation)
                {
                    ApplyAnimCommands(_backupCommands);
                    Initialize(e.Current);
                }
            }

            if (obj is WadToolClass.AnimationEditorAnimationChangedEvent)
            {
                var e = obj as WadToolClass.AnimationEditorAnimationChangedEvent;
                if (e != null && e.Animation == _animation && e.Focus)
                    Initialize(e.Animation);
            }
        }

        protected override void OnShown(EventArgs e)
        {
            base.OnShown(e);
            if (gridViewCommands.SelectedRows.Count > 0)
                gridViewCommands.FirstDisplayedScrollingRowIndex = gridViewCommands.SelectedRows[0].Index;
            else if (gridViewCommands.Rows.Count > 0)
                gridViewCommands.Rows[0].Selected = true; // Select 1st row by default
        }

        private void DeleteCommand()
        {
            if (gridViewCommands.SelectedRows.Count == 0)
                return;

            foreach (DataGridViewRow row in gridViewCommands.SelectedRows)
            {
                int index = row.Index;
                _animCommands.RemoveAt(index);
            }

            if (gridViewCommands.SelectedRows.Count == 0)
                animCommandEditor.Command = null;

            _editor.Tool.AnimationEditorAnimcommandChanged();
        }

        private void MoveCommand(bool down)
        {
            if(down)
            {
                foreach (DataGridViewRow row in gridViewCommands.SelectedRows)
                {
                    int index = row.Index;
                    if (index + 1 > _animCommands.Count-1)
                        continue;
                    WadAnimCommand cmd = _animCommands[index];
                    _animCommands.RemoveAt(index);
                    _animCommands.Insert(index +1, cmd);
                    gridViewCommands.Rows[index].Selected = false;
                    gridViewCommands.Rows[index + 1].Selected = true;
                }
            }
            else
            {
                foreach (DataGridViewRow row in gridViewCommands.SelectedRows)
                {
                    int index = row.Index;
                    if (index - 1 < 0)
                        continue;
                    WadAnimCommand cmd = _animCommands[index];
                    _animCommands.RemoveAt(index);
                    _animCommands.Insert(index - 1, cmd);
                    gridViewCommands.Rows[index].Selected = false;
                    gridViewCommands.Rows[index - 1].Selected = true;
                }
            }

            _editor.Tool.AnimationEditorAnimcommandChanged();
        }

        private void ApplyChanges()
        {
            // Bounce to old animcommands for undo
            var newCommands = new List<WadAnimCommand>(_animation.WadAnimation.AnimCommands);
            ApplyAnimCommands(_backupCommands);
            _editor.Tool.UndoManager.PushAnimationChanged(_editor, _animation);

            // Apply new commands
            ApplyAnimCommands(newCommands);
            _editor.Tool.AnimationEditorAnimationChanged(_animation, false);
        }

        private void DiscardChanges()
        {
            ApplyAnimCommands(_backupCommands);
            _editor.Tool.AnimationEditorAnimationChanged(_animation, false);
        }

        private void ApplyAnimCommands(List<WadAnimCommand> newCommands)
        {
            _animation.WadAnimation.AnimCommands.Clear();
            _animation.WadAnimation.AnimCommands.AddRange(newCommands);
        }

        private void butCommandUp_Click(object sender, EventArgs e) => MoveCommand(false);
        private void butCommandDown_Click(object sender, EventArgs e) => MoveCommand(true);
        private void butDeleteEffect_Click(object sender, EventArgs e) => DeleteCommand();

        private void butAddEffect_Click(object sender, EventArgs e)
        {
            WadAnimCommand newCmd = new WadAnimCommand() { Type = WadAnimCommandType.SetPosition };
            _animCommands.Add(newCmd);
            for (int i = 0; i < _animCommands.Count-1;i++)
            {
                gridViewCommands.Rows[i].Selected = false;
            }
            gridViewCommands.Rows[_animCommands.Count - 1].Selected = true;

            _editor.Tool.AnimationEditorAnimcommandChanged();
        }

        private void butCopy_Click(object sender, EventArgs e)
        {
            if (gridViewCommands.SelectedRows.Count == 0)
                return;

            var index = gridViewCommands.SelectedRows[0].Index;
            _animCommands.Insert(index + 1, _animCommands[index].Clone());
            _editor.Tool.AnimationEditorAnimcommandChanged();
        }

        private void butCopyToAll_Click(object sender, EventArgs e)
        {
            if (_editor.SelectionIsEmpty || gridViewCommands.SelectedRows.Count == 0)
                return;

            for (int i = _editor.Selection.Y; i >= _editor.Selection.X; i--)
            {
                foreach (DataGridViewRow row in gridViewCommands.SelectedRows)
                {
                    int index = row.Index;

                    // Don't create copy if animcommand isn't frame-based or there's the same animcommand within selection area
                    if (_animCommands[index].FrameBased && _animCommands[index].Parameter1 == i)
                        continue;

                    var cmdCopy = _animCommands[index].Clone();

                    // Change frame number
                    if (_animCommands[index].FrameBased)
                        cmdCopy.Parameter1 = (short)i;

                    _animCommands.Insert(index + 1, cmdCopy);
                }
            }

            _editor.Tool.AnimationEditorAnimcommandChanged();
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
            if (DialogResult == DialogResult.Cancel) DiscardChanges();
            WadSoundPlayer.StopSample();
        }

        private void FormAnimCommandsEditor_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Delete)
                DeleteCommand();
        }

        internal void SelectCommand(WadAnimCommand cmd)
        {
            bool commandFound = false;

            foreach (DataGridViewRow row in gridViewCommands.Rows)
            {
                if (_animCommands[row.Index].Equals(cmd))
                {
                    row.Selected = true;
                    commandFound = true;
                }
                else
                    row.Selected = false;
            }

            if (!commandFound)
            {
                _animCommands.Add(cmd);
                gridViewCommands.Rows[_animCommands.Count - 1].Selected = true;
            }
        }
    }
}
