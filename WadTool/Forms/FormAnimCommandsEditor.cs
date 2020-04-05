using DarkUI.Controls;
using DarkUI.Forms;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows.Forms;
using TombLib;
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
        public FormAnimCommandsEditor(AnimationEditor editor, AnimationNode animation)
        {
            InitializeComponent();
            animCommandEditor.Initialize(editor);
            _editor = editor;
            Initialize(animation);
            
            _editor.Tool.EditorEventRaised += Tool_EditorEventRaised;

            // Set window property handlers
            Configuration.LoadWindowProperties(this, _editor.Tool.Configuration);
            FormClosing += new FormClosingEventHandler((s, e) => Configuration.SaveWindowProperties(this, _editor.Tool.Configuration));
        }

        private void Initialize(AnimationNode animation)
        {
            _animation = animation;
            //Make a copy of the Animation's commands. we dont want to edit the Commands directly
            _animCommands = new BindingList<WadAnimCommand>(animation.WadAnimation.AnimCommands.ToList());
            this.gridViewCommands.DataSource = _animCommands;
        }

        private void AnimCommandEditor_AnimCommandChanged(object sender, AnimCommandEditor.AnimCommandEventArgs e)
        {
            int index = gridViewCommands.SelectedRows[0].Index;
            _animCommands[index] = e.Command;
            Invalidate();
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
                if (e != null && e.Animation != _animation)
                    Initialize(e.Animation);
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
        }

        private void DeleteCommand()
        {
            if (gridViewCommands.SelectedRows.Count == 0)
                return;

            foreach(DataGridViewRow row in gridViewCommands.SelectedRows)
            {
                int index = row.Index;
                _animCommands.RemoveAt(index);
            }
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
            
        }

        private void ApplyChanges(bool undo = true)
        {
            if (undo)
                _editor.Tool.UndoManager.PushAnimationChanged(_editor, _animation);

            // Add new commands
            _animation.WadAnimation.AnimCommands.Clear();
            _animation.WadAnimation.AnimCommands.AddRange(_animCommands);

            // Update state in parent window
            _editor.Tool.AnimationEditorAnimationChanged(_animation, false);
        }

        private void butCommandUp_Click(object sender, EventArgs e) => MoveCommand(false);
        private void butCommandDown_Click(object sender, EventArgs e) => MoveCommand(true);
        private void butDeleteEffect_Click(object sender, EventArgs e) => DeleteCommand();

        private void butAddEffect_Click(object sender, EventArgs e)
        {
            WadAnimCommand newCmd = new WadAnimCommand() { Type = WadAnimCommandType.SetPosition };
            _animCommands.Add(newCmd);
            for(int i = 0; i < _animCommands.Count-1;i++)
            {
                gridViewCommands.Rows[i].Selected = false;
            }
            gridViewCommands.Rows[_animCommands.Count - 1].Selected = true;
            
        }

        private void butCopy_Click(object sender, EventArgs e)
        {
            foreach(DataGridViewRow row in gridViewCommands.SelectedRows)
            {
                int index = row.Index;
                WadAnimCommand cmdCopy = _animCommands[index].Clone();
                _animCommands.Insert(index+1, cmdCopy);
            }
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
            WadSoundPlayer.StopSample();
        }

        private void FormAnimCommandsEditor_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Delete)
                DeleteCommand();
        }

        internal void SelectCommand(WadAnimCommand cmd)
        {
            foreach(DataGridViewRow row in gridViewCommands.Rows)
            {
                if(_animCommands[row.Index].Equals(cmd))
                {
                    row.Selected = true;
                }else
                {
                    row.Selected = false;
                }
               
            }
        }
    }
}
