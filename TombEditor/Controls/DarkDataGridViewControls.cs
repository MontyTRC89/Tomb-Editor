using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using DarkUI.Controls;
using System.Collections.ObjectModel;

namespace TombEditor.Controls
{
    public partial class DarkDataGridViewControls : UserControl
    {
        private DarkDataGridView _dataGridView;

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public DarkDataGridView DataGridView
        {
            get { return _dataGridView; }
            set
            {
                if (_dataGridView != null)
                    _dataGridView.SelectionChanged -= dataGridView_SelectionChanged;

                _dataGridView = value;
                Enabled = value != null;

                if (value != null)
                    value.SelectionChanged += dataGridView_SelectionChanged;
            }
        }

        // Must return a suitable object if data binding is used, other a DataGridViewRow object must be returned.
        public Func<object> CreateNewRow;
        public Func<bool> DeleteRowCheckIfCancel = () => false;

        public DarkDataGridViewControls()
        {
            InitializeComponent();
        }

        private void butNew_Click(object sender, EventArgs e)
        {
            // Create new row
            object newRow = CreateNewRow();
            if (newRow == null)
                return;
            int newIndex = DataGridView.EditableRowCollection.Add(newRow);

            // Select new row
            foreach (var row in DataGridView.SelectedRows.Cast<DataGridViewRow>().ToList())
                row.Selected = false;
            DataGridView.Rows[newIndex].Selected = true;
            DataGridView.FirstDisplayedScrollingRowIndex = newIndex;
        }

        private void butDelete_Click(object sender, EventArgs e)
        {
            if (DataGridView.SelectedRows.Count <= 0)
                return;

            if (DeleteRowCheckIfCancel())
                return;

            // Get sorted selection
            List<int> selectedRowIndices = DataGridView.SelectedRows.Cast<DataGridViewRow>().Select(row => row.Index).ToList();
            selectedRowIndices.Sort();
            selectedRowIndices.Reverse();

            //Remove rows
            var rows = DataGridView.EditableRowCollection;
            foreach (var selectedRowIndex in selectedRowIndices)
                rows.RemoveAt(selectedRowIndex);

            //Remove selection
            foreach (var selectedRow in DataGridView.SelectedRows.Cast<DataGridViewRow>().ToList())
                selectedRow.Selected = false;
        }

        private void butUp_Click(object sender, EventArgs e)
        {
            if (DataGridView.SelectedRows.Count <= 0)
                return;

            // Get sorted selection
            List<int> selectedRowIndices = DataGridView.SelectedRows.Cast<DataGridViewRow>().Select(row => row.Index).ToList();
            selectedRowIndices.Sort();

            // Move items
            var rows = DataGridView.EditableRowCollection;
            int lastItemIndex = 0;
            foreach (int selectedRowIndex in selectedRowIndices)
            {
                if (selectedRowIndex == lastItemIndex++)
                    continue;

                var row = rows[selectedRowIndex - 1];
                rows.RemoveAt(selectedRowIndex - 1);
                rows.Insert(selectedRowIndex, row);
            }
        }

        private void butDown_Click(object sender, EventArgs e)
        {
            if (DataGridView.SelectedRows.Count <= 0)
                return;

            // Get sorted selection
            List<int> selectedRowIndices = DataGridView.SelectedRows.Cast<DataGridViewRow>()
                .Select(row => row.Index).ToList();
            selectedRowIndices.Sort();
            selectedRowIndices.Reverse();

            // Move items
            var rows = DataGridView.EditableRowCollection;
            int lastItemIndex = rows.Count - 1;
            foreach (int selectedRowIndex in selectedRowIndices)
            {
                if (selectedRowIndex == lastItemIndex--)
                    continue;

                var row = rows[selectedRowIndex + 1];
                rows.RemoveAt(selectedRowIndex + 1);
                rows.Insert(selectedRowIndex, row);
            }
        }

        private void dataGridView_SelectionChanged(object sender, EventArgs e)
        {
            butUp.Enabled = DataGridView.SelectedRows.Count > 0;
            butDown.Enabled = DataGridView.SelectedRows.Count > 0;
            butDelete.Enabled = DataGridView.SelectedRows.Count > 0;
        }

        [DefaultValue("New")]
        public string NewName
        {
            get { return butNew.Text; }
            set { butNew.Text = value; }
        }

        [DefaultValue(true)]
        public bool AllowUserMove
        {
            get { return butDown.Visible; }
            set { butDown.Visible = butUp.Visible = value; }
        }

        [DefaultValue(true)]
        public bool AllowUserDelete
        {
            get { return butDelete.Visible; }
            set { butDelete.Visible = value; }
        }

        [DefaultValue(true)]
        public bool AllowUserNew
        {
            get { return butNew.Visible; }
            set { butNew.Visible = value; }
        }
    }
}
