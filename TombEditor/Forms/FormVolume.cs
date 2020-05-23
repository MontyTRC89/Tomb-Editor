﻿using DarkUI.Forms;
using System;
using System.Windows.Forms;
using System.Windows.Media;
using TombLib.LevelData;
using TombLib.Scripting.TextEditors;
using TombLib.Scripting.TextEditors.SyntaxHighlighting;

namespace TombEditor.Forms
{
    public partial class FormVolume : DarkForm
    {
        private VolumeInstance _volume;

        private VolumeScriptInstance _backupScripts;
        private VolumeActivators _backupState;
        private int prevIndex = -1;

        private bool _flagsLocked;

        private TextEditorConfigs _configs = TextEditorConfigs.Load();

        public FormVolume(VolumeInstance volume)
        {
            InitializeComponent();
            Editor.Instance.EditorEventRaised += EditorEventRaised;

            LoadVolume(volume);

            tbScript.TextEditor.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString(_configs.Lua.ColorScheme.Background));
            tbScript.TextEditor.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString(_configs.Lua.ColorScheme.Foreground));
            tbScript.TextEditor.SyntaxHighlighting = new LuaSyntaxHighlighting(_configs.Lua.ColorScheme);
            tbScript.TextEditor.UpdateSettings(_configs.Lua);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
                Editor.Instance.EditorEventRaised -= EditorEventRaised;
            base.Dispose(disposing);
        }

        private void EditorEventRaised(IEditorEvent obj)
        {
            if (obj is Editor.LevelChangedEvent)
                Close();

            if (obj is Editor.ObjectChangedEvent)
            {
                var o = (Editor.ObjectChangedEvent)obj;
                if (o.Object == _volume && o.ChangeType == ObjectChangeType.Remove)
                    Close();
            }
        }

        public void SaveAndReopenVolume(VolumeInstance volume)
        {
            SaveCurrentScript();
            LoadVolume(volume);
        }

        private void LoadVolume(VolumeInstance volume)
        {
            _volume = volume;
            _backupScripts = _volume.Scripts.Clone();
            _backupState = _volume.Activators;
            cmbEvent.SelectedIndex = 0; // Select first script
            tbScript.TextEditor.Text = _volume.Scripts.OnEnter; // Force 
            tbName.Text = _volume.Scripts.Name;
            UpdateFlags();
        }

        private void SaveCurrentScript(bool callEvent = true)
        {
            switch (prevIndex)
            {
                case 0:
                    _volume.Scripts.OnEnter = tbScript.TextEditor.Text;
                    break;
                case 1:
                    _volume.Scripts.OnLeave = tbScript.TextEditor.Text;
                    break;
                case 2:
                    _volume.Scripts.OnInside = tbScript.TextEditor.Text;
                    break;
                case 3:
                    _volume.Scripts.Environment = tbScript.TextEditor.Text;
                    break;
            }

            if (callEvent)
                Editor.Instance.ObjectChange(_volume, ObjectChangeType.Change);
        }

        private void UpdateFlags()
        {
            _flagsLocked = true;
            cbLara.Checked = _volume.Activators.HasFlag(VolumeActivators.Player);
            cbNPC.Checked = _volume.Activators.HasFlag(VolumeActivators.NPCs);
            cbOtherMoveables.Checked = _volume.Activators.HasFlag(VolumeActivators.OtherMoveables);
            cbStatics.Checked = _volume.Activators.HasFlag(VolumeActivators.Statics);
            cbFlybys.Checked = _volume.Activators.HasFlag(VolumeActivators.Flybys);
            _flagsLocked = false;
        }

        private void butOK_Click(object sender, EventArgs e)
        {
            SaveCurrentScript();
            Close();
        }

        private void butCancel_Click(object sender, EventArgs e)
        {
            _volume.Scripts = _backupScripts;
            _volume.Activators = _backupState;
            Close();
        }

        private void cmbEvent_SelectedIndexChanged(object sender, EventArgs e)
        {
            SaveCurrentScript(false);

            switch (cmbEvent.SelectedIndex)
            {
                case 0:
                    tbScript.TextEditor.Text = _volume.Scripts.OnEnter;
                    break;
                case 1:
                    tbScript.TextEditor.Text = _volume.Scripts.OnLeave;
                    break;
                case 2:
                    tbScript.TextEditor.Text = _volume.Scripts.OnInside;
                    break;
                case 3:
                    tbScript.TextEditor.Text = _volume.Scripts.Environment;
                    break;
            }

            prevIndex = cmbEvent.SelectedIndex;
        }

        private void tbName_TextChanged(object sender, EventArgs e)
        {
            if (tbName.Text != _volume.Scripts.Name)
                _volume.Scripts.Name = tbName.Text;
        }

        private void cbLara_CheckedChanged(object sender, EventArgs e)
        {
            if (_flagsLocked) return;
            _volume.Activators ^= VolumeActivators.Player;
            UpdateFlags();
        }

        private void cbNPC_CheckedChanged(object sender, EventArgs e)
        {
            if (_flagsLocked) return;
            _volume.Activators ^= VolumeActivators.NPCs;
            UpdateFlags();
        }

        private void cbOtherMoveables_CheckedChanged(object sender, EventArgs e)
        {
            if (_flagsLocked) return;
            _volume.Activators ^= VolumeActivators.OtherMoveables;
            UpdateFlags();
        }
        
        private void cbStatics_CheckedChanged(object sender, EventArgs e)
        {
            if (_flagsLocked) return;
            _volume.Activators ^= VolumeActivators.Statics;
            UpdateFlags();
        }

        private void cbFlybys_CheckedChanged(object sender, EventArgs e)
        {
            if (_flagsLocked) return;
            _volume.Activators ^= VolumeActivators.Flybys;
            UpdateFlags();
        }

        private void textEditor_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(typeof(MoveableInstance)) ||
                e.Data.GetDataPresent(typeof(StaticInstance)))
                e.Effect = DragDropEffects.Copy;
            else
                e.Effect = DragDropEffects.None;
        }

        private void textEditor_DragDrop(object sender, DragEventArgs e)
        {
            string prefix = string.Empty;
            string postfix = string.Empty;
            string data = string.Empty;

            if (e.Data.GetDataPresent(typeof(MoveableInstance)))
            {
                prefix = "GetItemByID(";
                postfix = ")";
                data = ((MoveableInstance)e.Data.GetData(typeof(MoveableInstance))).LuaId.ToString();
            }
            else if (e.Data.GetDataPresent(typeof(StaticInstance)))
            {
                prefix = "GetStaticByID(";
                postfix = ")";
                data = ((StaticInstance)e.Data.GetData(typeof(StaticInstance))).LuaId.ToString();
            }

            var finalResult = (prefix + data + postfix).Trim();
            if (!string.IsNullOrEmpty(finalResult))
                tbScript.Paste(finalResult);
        }
    }
}
