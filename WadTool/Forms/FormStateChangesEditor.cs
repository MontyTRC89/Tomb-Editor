using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows.Forms;
using TombLib.Graphics;
using TombLib.Wad;

namespace WadTool
{
    public partial class FormStateChangesEditor : DarkUI.Forms.DarkForm
    {
        private readonly AnimationEditor _editor;
        private AnimationNode _animation;
        private bool _createdNew = false;

        private class WadStateChangeRow
        {
            public ushort StateId { get; set; }
            public ushort LowFrame { get; set; }
            public ushort HighFrame { get; set; }
            public ushort NextAnimation { get; set; }
            public ushort NextFrame { get; set; }

            public WadStateChangeRow(ushort stateId, ushort lowFrame, ushort highFrame, ushort nextAnimation, ushort nextFrame)
            {
                StateId = stateId;
                LowFrame = lowFrame;
                HighFrame = highFrame;
                NextAnimation = nextAnimation;
                NextFrame = nextFrame;
            }

            public WadStateChangeRow() { }
        }

        public List<WadStateChange> StateChanges { get; private set; }

        public FormStateChangesEditor(AnimationEditor editor, AnimationNode animation, WadStateChange newStateChange = null)
        {
            InitializeComponent();

            _editor = editor;

            dgvControls.CreateNewRow = newObject;
            dgvControls.DataGridView = dgvStateChanges;
            dgvControls.Enabled = true;

            Initialize(animation, newStateChange);
            _editor.Tool.EditorEventRaised += Tool_EditorEventRaised;
        }

        private void Initialize(AnimationNode animation, WadStateChange newStateChange)
        {
            _animation = animation;

            dgvStateChanges.Rows.Clear();

            var rows = new List<WadStateChangeRow>();
            foreach (var sc in _animation.WadAnimation.StateChanges)
                foreach (var d in sc.Dispatches)
                    rows.Add(new WadStateChangeRow(sc.StateId, d.InFrame, d.OutFrame, d.NextAnimation, d.NextFrame));

            if (newStateChange != null && newStateChange.Dispatches.Count == 1)
            {
                rows.Add(new WadStateChangeRow(newStateChange.StateId,
                                               newStateChange.Dispatches[0].InFrame,
                                               newStateChange.Dispatches[0].OutFrame,
                                               newStateChange.Dispatches[0].NextAnimation,
                                               newStateChange.Dispatches[0].NextFrame));
                _createdNew = true;
            }

            dgvStateChanges.DataSource = new BindingList<WadStateChangeRow>(new List<WadStateChangeRow>(rows));
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
                _editor.Tool.EditorEventRaised -= Tool_EditorEventRaised;
            base.Dispose(disposing);
        }

        private void Tool_EditorEventRaised(IEditorEvent obj)
        {
            if (obj is WadToolClass.AnimationEditorCurrentAnimationChangedEvent)
            {
                var e = obj as WadToolClass.AnimationEditorCurrentAnimationChangedEvent;
                if (e != null && e.Animation != _animation)
                    Initialize(e.Animation, null);
            }

            if (obj is WadToolClass.AnimationEditorAnimationChangedEvent)
            {
                var e = obj as WadToolClass.AnimationEditorAnimationChangedEvent;
                if (e != null && e.Animation == _animation)
                    Initialize(e.Animation, null);
            }
        }

        protected override void OnShown(EventArgs e)
        {
            base.OnShown(e);

            if(_createdNew)
            {
                dgvStateChanges.ClearSelection();
                dgvStateChanges.Rows[dgvStateChanges.Rows.Count - 2].Selected = true;
                dgvStateChanges.FirstDisplayedScrollingRowIndex = dgvStateChanges.SelectedRows[0].Index;
            }
        }

        private object newObject()
        {
            return new WadStateChangeRow();
        }

        private void btOk_Click(object sender, EventArgs e)
        {
            // Update data
            StateChanges = new List<WadStateChange>();
            var tempDictionary = new Dictionary<int, WadStateChange>();
            foreach (var row in (IEnumerable<WadStateChangeRow>)dgvStateChanges.DataSource)
            {
                if (!tempDictionary.ContainsKey(row.StateId))
                    tempDictionary.Add(row.StateId, new WadStateChange());
                var sc = tempDictionary[row.StateId];
                sc.StateId = row.StateId;
                sc.Dispatches.Add(new WadAnimDispatch(row.LowFrame, row.HighFrame, row.NextAnimation, row.NextFrame));
                tempDictionary[row.StateId] = sc;
            }
            StateChanges.AddRange(tempDictionary.Values.ToList());

            // Undo
            _editor.Tool.UndoManager.PushAnimationChanged(_editor, _animation);

            // Add the new state changes
            _animation.WadAnimation.StateChanges.Clear();
            _animation.WadAnimation.StateChanges.AddRange(StateChanges);

            // Update state in parent window
            _editor.Tool.AnimationEditorAnimationChanged(_animation, false);

            // Close
            Close();
        }

        private void btCancel_Click(object sender, EventArgs e) => Close();

        private void dgvStateChanges_CellFormattingSafe(object sender, DarkUI.Controls.DarkDataGridViewSafeCellFormattingEventArgs e)
        {
            if (!(e.Row.DataBoundItem is WadStateChangeRow))
                return;
            WadStateChangeRow item = (WadStateChangeRow)e.Row.DataBoundItem;

            if (e.ColumnIndex == 0)
                e.Value = item.StateId;
            else if (e.ColumnIndex == 1)
                e.Value = item.LowFrame;
            else if (e.ColumnIndex == 2)
                e.Value = item.HighFrame;
            else if (e.ColumnIndex == 3)
                e.Value = item.NextAnimation;
            else if (e.ColumnIndex == 4)
                e.Value = item.NextFrame;

            e.FormattingApplied = true;
        }

        private void dgvStateChanges_CellValidated(object sender, DataGridViewCellEventArgs e)
        {

        }
    }
}
