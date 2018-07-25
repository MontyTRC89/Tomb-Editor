using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using DarkUI.Forms;
using DarkUI.Collections;
using System.Drawing;

namespace TombEditor.Forms
{
    public partial class FormKeyboardLayout : DarkForm
    {
        private readonly Editor _editor;
        private HotkeySets _currConfig;

        private Keys _listeningKeys = Keys.None;
        private bool _listeningClearAfterwards = false;
        private CommandObj _listeningDestination = null;

        private static readonly string _listenerMessage = "Push keys! Click here to cancel!";

        public FormKeyboardLayout(Editor editor)
        {
            InitializeComponent();

            _editor = editor;
            _currConfig = _editor.Configuration.Window_HotkeySets.Clone();
            commandList.DataSource = new SortableBindingList<CommandObj>(CommandHandler.Commands);
            listenKeys.Text = _listenerMessage;

            CheckForConflicts();
        }

        private void RedrawList()
        {
            commandList.InvalidateColumn(commandList.Columns[commandListColumnHotkeys.Name].Index);
            CheckForConflicts();
        }

        private void CheckForConflicts()
        {
            lblConflicts.Visible = false;

            foreach (var left in _currConfig)
                foreach (var right in _currConfig)
                {
                    if ((left.Key != right.Key) && (left.Value.Intersect(right.Value).Count() > 0))
                    {
                        lblConflicts.Visible = true;
                        lblConflicts.Text = "Possible conflict found: " + left.Key + " and " + right.Key;
                        return;
                    }
                }
        }

        private void StartListening(CommandObj destination, bool clearAfterListening)
        {
            if (!listenKeys.Visible)
            {
                _listeningClearAfterwards = clearAfterListening;
                listenKeys.Visible = true;
                _listeningDestination = destination;
            }
        }

        private void StopListening()
        {
            if (listenKeys.Visible)
            {
                _listeningKeys = Keys.None;
                listenKeys.Visible = false;
                listenKeys.Text = _listenerMessage;
            }
        }

        private void butOK_Click(object sender, EventArgs e)
        {
            _editor.Configuration.Window_HotkeySets = _currConfig;
            _editor.ConfigurationChange();
            Close();
        }

        private void butCancel_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void butDefaults_Click(object sender, EventArgs e)
        {
            if (DarkMessageBox.Show(this, "Do you really want to restore ALL key bindings to their default?", "Are you sure?", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
                return;

            _currConfig = new HotkeySets();
            RedrawList();
        }

        private void commandList_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            if (e.RowIndex < 0 || e.RowIndex >= commandList.Rows.Count)
                return;

            if (commandList.Columns[e.ColumnIndex].Name == commandListColumnHotkeys.Name)
            {
                CommandObj entry = (CommandObj)(commandList.Rows[e.RowIndex].DataBoundItem);
                e.Value = string.Join(", ", _currConfig[entry].Select(h => h.ToString()));
            }
        }

        private void commandList_CellParsing(object sender, DataGridViewCellParsingEventArgs e)
        {
            if (e.RowIndex < 0 || e.RowIndex >= commandList.Rows.Count)
                return;

            if (commandList.Columns[e.ColumnIndex].Name == commandListColumnHotkeys.Name)
            {
                // Parse
                string errorMessage;
                SortedSet<Hotkey> hotkeys = HotkeySets.ParseHotkeys(e.Value.ToString(), out errorMessage);
                if (hotkeys == null)
                    return;

                // Set
                CommandObj entry = (CommandObj)(commandList.Rows[e.RowIndex].DataBoundItem);
                _currConfig[entry] = hotkeys;

                CheckForConflicts();
            }
        }

        private void commandList_CellValidating(object sender, DataGridViewCellValidatingEventArgs e)
        {
            if (commandList.Columns[e.ColumnIndex].Name == commandListColumnHotkeys.Name)
            {
                // Parse
                string errorMessage;
                HotkeySets.ParseHotkeys(e.FormattedValue.ToString(), out errorMessage);
                if (errorMessage != null)
                    if (DarkMessageBox.Show(this, errorMessage, "Invalid input", MessageBoxButtons.RetryCancel, MessageBoxIcon.Warning) != DialogResult.Cancel)
                        e.Cancel = true; // We cancel the cell parsing, the user can retry inputting something.
            }
        }

        private void commandList_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0 || e.RowIndex >= commandList.Rows.Count)
                return;
            var command = (CommandObj)(commandList.Rows[e.RowIndex].DataBoundItem);
            if (commandList.Columns[e.ColumnIndex].Name == commandListColumnAdd.Name)
                StartListening(command, false);
            else if (commandList.Columns[e.ColumnIndex].Name == commandListColumnDelete.Name)
            {
                _currConfig[command].Clear();
                commandList.InvalidateCell(commandList.Columns[commandListColumnHotkeys.Name].Index, e.RowIndex);
            }
        }

        private void commandList_CellPainting(object sender, DataGridViewCellPaintingEventArgs e)
        {
            if (e.RowIndex < 0 || e.RowIndex >= commandList.Rows.Count)
                return;
            if (commandList.Columns[e.ColumnIndex].Name == commandListColumnDelete.Name)
                PaintCell(e, Properties.Resources.general_trash_16);
            else if (commandList.Columns[e.ColumnIndex].Name == commandListColumnAdd.Name)
                PaintCell(e, Properties.Resources.general_plus_math_16);
        }

        private static void PaintCell(DataGridViewCellPaintingEventArgs e, Image image)
        {
            e.Paint(e.CellBounds, DataGridViewPaintParts.All);
            e.Graphics.DrawImage(image,
                e.CellBounds.Left + (e.CellBounds.Width - image.Width) / 2 - 1,
                e.CellBounds.Top + (e.CellBounds.Height - image.Height) / 2,
                image.Width, image.Height);
            e.Handled = true;
        }

        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            if (listenKeys.Visible)
            {
                _listeningKeys |= keyData & ~Keys.KeyCode;
                switch (keyData & Keys.KeyCode)
                {
                    case Keys.ControlKey:
                        _listeningKeys |= Keys.Control;
                        break;
                    case Keys.ShiftKey:
                        _listeningKeys |= Keys.Shift;
                        break;
                    case Keys.Menu:
                        _listeningKeys |= Keys.Alt;
                        break;
                    default:
                        _listeningKeys = (_listeningKeys & ~Keys.KeyCode) | (keyData & Keys.KeyCode);
                        break;
                }

                listenKeys.Text = _listeningKeys.ToString();

                return true; // Always don't process while listening
            }
            else if (keyData == Keys.Delete)
            {
                _currConfig[(CommandObj)(commandList.SelectedRows[0].DataBoundItem)].Clear();
                RedrawList();
            }

            return base.ProcessCmdKey(ref msg, keyData);
        }

        private void FormKeyboardLayout_KeyUp(object sender, KeyEventArgs e)
        {
            if (!listenKeys.Visible)
                return;
            if ((_listeningKeys & Keys.KeyCode) == Keys.None)
                return;

            if (HotkeySets.ReservedCameraKeys.Contains(_listeningKeys))
            {
                DarkMessageBox.Show(this, "This key is reserved for camera movement. Please define another key.", "Reserved key", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (_listeningClearAfterwards)
                _currConfig[_listeningDestination.Name]?.Clear();
            _currConfig[_listeningDestination.Name].Add(_listeningKeys);

            RedrawList();
            StopListening();
        }

        private void listenKeys_Click(object sender, EventArgs e)
        {
            StopListening();
        }
    }
}
