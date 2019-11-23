using DarkUI.Controls;
using DarkUI.Forms;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace TombEditor.Forms
{
    public partial class FormToolBarLayout : DarkForm
    {
        private const string _separatorString = "[ Separator ]";

        private List<ToolStripItem> _toolstripButtons;
        private Editor _editor;

        public FormToolBarLayout(Editor editor, List<ToolStripItem> toolstripButtons)
        {
            InitializeComponent();
            _editor = editor;
            _toolstripButtons = toolstripButtons;

            BuildLists(_editor.Configuration);
        }

        public void MoveToDest()
        {
            if (dgvSource.SelectedRows.Count == 0) return;
            var item = dgvSource.SelectedRows[0].Tag as ToolStripItem;

            var removeFromSource = !(item is ToolStripSeparator);
            string label = removeFromSource ? ExtractButtonName(item.Name) : _separatorString;
            dgvDest.Rows.Insert(dgvDest.SelectedRows[0].Index, string.Empty, label);
            dgvDest.Rows[dgvDest.SelectedRows[0].Index - 1].Tag = item;

            if (removeFromSource) dgvSource.Rows.RemoveAt(dgvSource.SelectedRows[0].Index);
        }

        public void MoveToSource()
        {
            if (dgvDest.SelectedRows.Count == 0) return;
            var item = dgvDest.SelectedRows[0].Tag as ToolStripItem;

            if (!(item is ToolStripSeparator))
            {
                string label = ExtractButtonName(item.Name);
                dgvSource.Rows.Add(string.Empty, label);
                dgvSource.Rows[dgvSource.Rows.Count - 1].Tag = item;
            }

            dgvDest.Rows.RemoveAt(dgvDest.SelectedRows[0].Index);
        }

        public void BuildLists(Configuration configuration)
        {
            var existingList = new List<ToolStripItem>();

            dgvDest.Rows.Clear();
            dgvSource.Rows.Clear();

            foreach (var entry in configuration.UI_ToolbarButtons)
            {
                if (entry == "|")
                {
                    dgvDest.Rows.Add(string.Empty, _separatorString);
                    dgvDest.Rows[dgvDest.Rows.Count - 1].Tag = new ToolStripSeparator();
                }
                else
                {
                    var realName = "but" + entry;
                    var potentialButton = _toolstripButtons.FirstOrDefault(item => item.Name == realName);
                    if (potentialButton != null)
                    {
                        dgvDest.Rows.Add(string.Empty, entry);
                        dgvDest.Rows[dgvDest.Rows.Count - 1].Tag = potentialButton;
                        existingList.Add(potentialButton);
                    }
                }
            }

            dgvSource.Rows.Add(string.Empty, _separatorString);
            dgvSource.Rows[dgvSource.Rows.Count - 1].Tag = new ToolStripSeparator();

            foreach (var entry in _toolstripButtons)
            {
                if (existingList.Contains(entry))
                    continue;
                else
                {
                    dgvSource.Rows.Add(string.Empty, ExtractButtonName(entry.Name));
                    dgvSource.Rows[dgvSource.Rows.Count - 1].Tag = entry;
                }
            }
        }

        private string ExtractButtonName(string name)
        {
            const string prefix = "but";
            int index = name.IndexOf(prefix);
            return (index < 0) ? name : name.Remove(index, prefix.Length);
        }

        private void SaveConfiguration()
        {
            var buttonList = new List<string>();

            foreach (DataGridViewRow row in dgvDest.Rows)
            {
                if (row.Tag is ToolStripSeparator)
                    buttonList.Add("|");
                else
                    buttonList.Add(row.Cells[1].Value.ToString());
            }

            _editor.Configuration.UI_ToolbarButtons = buttonList.ToArray();
            _editor.ConfigurationChange(false, false, true);
        }

        private void dataGridView_CellPainting(DarkDataGridView dgv, DataGridViewCellPaintingEventArgs e)
        {
            if (dgv == null || e.RowIndex < 0 || e.RowIndex >= dgv.Rows.Count) return;

            if (dgv.Columns[e.ColumnIndex].Name == ColumnSourceButton.Name || dgv.Columns[e.ColumnIndex].Name == ColumnDestButton.Name)
            {
                var button = dgv.Rows[e.RowIndex].Tag as ToolStripItem;
                if (!(button is ToolStripSeparator))
                    dgv.PaintCell(e, button.Image);
            }
        }

        private void butOk_Click(object sender, EventArgs e)
        {
            SaveConfiguration();
            Close();
        }

        private void butDefaults_Click(object sender, EventArgs e) => BuildLists(new Configuration());
        private void butCancel_Click(object sender, EventArgs e) => Close();
        private void butApply_Click(object sender, EventArgs e) => SaveConfiguration();
        private void butToSource_Click(object sender, EventArgs e) => MoveToSource();
        private void butToDest_Click(object sender, EventArgs e) => MoveToDest();
        private void dgvSource_CellContentDoubleClick(object sender, DataGridViewCellEventArgs e) => MoveToDest();
        private void dgvDest_CellContentDoubleClick(object sender, DataGridViewCellEventArgs e) => MoveToSource();
        private void dgvSource_CellPainting(object sender, DataGridViewCellPaintingEventArgs e) => dataGridView_CellPainting(dgvSource, e);
        private void dgvDest_CellPainting(object sender, DataGridViewCellPaintingEventArgs e) => dataGridView_CellPainting(dgvDest, e);
    }
}
