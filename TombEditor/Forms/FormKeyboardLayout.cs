using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using DarkUI.Forms;

namespace TombEditor.Forms
{
    public partial class FormKeyboardLayout : DarkForm
    {
        private readonly Editor _editor;
        private List<HotkeySet> _currConfig;

        private int _keyPushCount = 0;
        private Keys _listenedKeys = Keys.None;
        private bool _clearAfterListening = false;

        private static readonly string _listenerMessage = "Push keys! Click here to cancel!";

        public FormKeyboardLayout(Editor editor)
        {
            InitializeComponent();

            _editor = editor;
            _currConfig = _editor.Configuration.Keyboard_Hotkeys.ConvertAll(hotkeys => hotkeys.Clone());
            commandList.RowCount = _editor.CommandHandler.Commands.Count();
            listenKeys.Text = _listenerMessage;

            CheckForConflicts();
        }

        private void RedrawList()
        {
            foreach (DataGridViewColumn column in commandList.Columns)
                commandList.InvalidateColumn(column.Index);
            CheckForConflicts();
        }

        private void CheckForConflicts()
        {
            lblConflicts.Visible = false;

            foreach (var left in _currConfig)
                foreach (var right in _currConfig)
                {
                    if ((left.Name != right.Name) && (left.Hotkeys.Intersect(right.Hotkeys).Count() > 0))
                    {
                        lblConflicts.Visible = true;
                        lblConflicts.Text = "Possible conflict found: " + left.Name + " and " + right.Name;
                        return;
                    }
                }
        }

        private void StartListening(bool clearAfterListening = false)
        {
            if (!listenKeys.Visible)
            {
                _clearAfterListening = clearAfterListening;
                listenKeys.Visible = true;
                _keyPushCount = 0;
            }
        }

        private void StopListening()
        {
            if (listenKeys.Visible)
            {
                _keyPushCount = 0;
                _listenedKeys = Keys.None;
                listenKeys.Visible = false;
                listenKeys.Text = _listenerMessage;
            }
        }

        private void ClearCurrentCommand()
        {
            _currConfig.FirstOrDefault(set => set.Name == _editor.CommandHandler.Commands[commandList.SelectedRows[0].Index].Name)?.Hotkeys?.Clear();
            RedrawList();
        }

        private void butOK_Click(object sender, EventArgs e)
        {
            _editor.Configuration.Keyboard_Hotkeys = _currConfig;
            _editor.ConfigurationChange();
            Close();
        }

        private void butCancel_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void butDefaults_Click(object sender, EventArgs e)
        {
            _currConfig.Clear();
            _currConfig = CommandHandler.GenerateDefaultHotkeys();
            RedrawList();
        }

        private void commandList_CellValueNeeded(object sender, DataGridViewCellValueEventArgs e)
        {
            if (_editor.CommandHandler.Commands == null || _editor.CommandHandler.Commands.Count == 0 || e.RowIndex < 0 || e.RowIndex >= _editor.CommandHandler.Commands.Count)
                return;

            EditorCommand entry = _editor.CommandHandler.Commands.ElementAt(e.RowIndex);

            if (commandList.Columns[e.ColumnIndex].Name == commandListColumnType.Name)
                e.Value = entry.Type;
            else if (commandList.Columns[e.ColumnIndex].Name == commandListColumnCommand.Name)
                e.Value = entry.FriendlyName;
            else if (commandList.Columns[e.ColumnIndex].Name == commandListColumnHotkeys.Name)
            {
                var hotkeyList = _currConfig.FirstOrDefault(set => set.Name.ToUpper().Equals(entry.Name.ToUpper()));
                string hotkeyString = "";

                if(hotkeyList != null)
                    for (int i = 0; i < hotkeyList.Hotkeys.Count; i++)
                    {
                        hotkeyString += CommandHandler.KeysToString((Keys)hotkeyList.Hotkeys[i]);
                        hotkeyString += hotkeyList.Hotkeys.Count > 1 && i < hotkeyList.Hotkeys.Count - 1 ? ", " : "";
                    }

                e.Value = hotkeyString;
            }
        }

        private void butClear_Click(object sender, EventArgs e)
        {
            ClearCurrentCommand();
        }

        private void butAdd_Click(object sender, EventArgs e)
        {
            StartListening();
        }

        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            if (listenKeys.Visible)
            {
                Keys realKey = keyData & ~(Keys.Alt | Keys.Shift | Keys.Control);
                switch (realKey)
                {
                    case Keys.ControlKey:
                        realKey = Keys.Control;
                        break;
                    case Keys.ShiftKey:
                        realKey = Keys.Shift;
                        break;
                    case Keys.Menu:
                        realKey = Keys.Alt;
                        break;
                }

                if((_listenedKeys & realKey) != realKey)
                {
                    _keyPushCount++;
                    _listenedKeys |= realKey;
                    listenKeys.Text = CommandHandler.KeysToString(_listenedKeys);
                }

                return true; // Always don't process while listening
            }
            else if (keyData == Keys.Delete || keyData == Keys.Back)
                ClearCurrentCommand();

            return base.ProcessCmdKey(ref msg, keyData);
        }

        private void FormKeyboardLayout_KeyUp(object sender, KeyEventArgs e)
        {
            if (listenKeys.Visible)
            {
                _keyPushCount--;
                if (_keyPushCount == 0)
                {
                    if (HotkeySet.ReservedKeys.Contains(_listenedKeys))
                        DarkMessageBox.Show(this, "This key is reserved for camera movement. Please define another key.", "Reserved key", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    else if((_listenedKeys & ~(Keys.Alt | Keys.Shift | Keys.Control)) != Keys.None)
                    {
                        if (_clearAfterListening)
                            ClearCurrentCommand();

                        var foundHotkeySet = _currConfig.FirstOrDefault(set => set.Name == _editor.CommandHandler.Commands[commandList.SelectedRows[0].Index].Name)?.Hotkeys;
                        if (foundHotkeySet == null)
                            _currConfig.Add(new HotkeySet { Name = _editor.CommandHandler.Commands[commandList.SelectedRows[0].Index].Name, Hotkeys = new List<uint> { (uint)_listenedKeys } });
                        else if (!foundHotkeySet.Any(hotkey => hotkey == (uint)_listenedKeys))
                            foundHotkeySet.Add((uint)_listenedKeys);
                    }
                    RedrawList();
                    StopListening();
                }
            }
        }

        private void listenKeys_Click(object sender, EventArgs e)
        {
            StopListening();
        }

        private void commandList_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            StartListening(true);
        }
    }
}
