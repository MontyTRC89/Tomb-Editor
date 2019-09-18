using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using TombLib.Wad;

namespace WadTool
{
    public partial class FormStateChangesEditor : DarkUI.Forms.DarkForm
    {
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

        public FormStateChangesEditor(List<WadStateChange> stateChanges)
        {
            InitializeComponent();

            var rows = new List<WadStateChangeRow>();
            foreach (var sc in stateChanges)
                foreach (var d in sc.Dispatches)
                    rows.Add(new WadStateChangeRow(sc.StateId, d.InFrame, d.OutFrame, d.NextAnimation, d.NextFrame));

            dgvStateChanges.DataSource = new BindingList<WadStateChangeRow>(new List<WadStateChangeRow>(rows));
            dgvControls.CreateNewRow = newObject;
            dgvControls.DataGridView = dgvStateChanges;
            dgvControls.Enabled = true;
        }

        private object newObject()
        {
            return new WadStateChangeRow();
        }

        private void btOk_Click(object sender, EventArgs e)
        {
            // Update data
            StateChanges= new List<WadStateChange>();
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

            // Close
            DialogResult = DialogResult.OK;
            Close();
        }

        private void btCancel_Click(object sender, EventArgs e)
        {
            // Close
            DialogResult = DialogResult.Cancel;
            Close();
        }

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
