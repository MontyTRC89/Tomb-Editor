using DarkUI.Controls;
using DarkUI.Forms;
using NLog;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using TombLib;
using TombLib.Controls;
using TombLib.LevelData;
using TombLib.NG;
using TombLib.Wad.Catalog;

namespace TombEditor
{
    public partial class FormTrigger : DarkForm
    {
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();

        private Level _level;
        private TriggerInstance _trigger;
        private bool _loading = true;

        public FormTrigger(Level level, TriggerInstance trigger, Action<ObjectInstance> selectObject,
                           Action<Room> selectRoom)
        {
            InitializeComponent();

            _level = level;
            _trigger = trigger;

            // Setup events
            foreach (var control in Controls.OfType<TriggerParameterControl>())
            {
                control.ViewObject += selectObject;
                control.ViewRoom += selectRoom;
                control.Level = level;
            }

            // Calculate the sizes at runtime since they actually depend on the choosen layout.
            // https://stackoverflow.com/questions/1808243/how-does-one-calculate-the-minimum-client-size-of-a-net-windows-form
            MaximumSize = new Size(32000, Size.Height);
            MinimumSize = new Size(347 + (Size.Height - ClientSize.Height), Size.Height);

            UpdateDialog();

            // Set values
            cbBit1.Checked = (_trigger.CodeBits & (1 << 0)) != 0;
            cbBit2.Checked = (_trigger.CodeBits & (1 << 1)) != 0;
            cbBit3.Checked = (_trigger.CodeBits & (1 << 2)) != 0;
            cbBit4.Checked = (_trigger.CodeBits & (1 << 3)) != 0;
            cbBit5.Checked = (_trigger.CodeBits & (1 << 4)) != 0;
            cbOneShot.Checked = _trigger.OneShot;
            paramTriggerType.Parameter = new TriggerParameterUshort((ushort)_trigger.TriggerType);
            paramTargetType.Parameter = new TriggerParameterUshort((ushort)_trigger.TargetType);
            paramTarget.Parameter = _trigger.Target;
            paramTimer.Parameter = _trigger.Timer;
            paramExtra.Parameter = _trigger.Extra;

            // Update the dialog
            _loading = false;
            UpdateDialog();
        }

        public void UpdateDialog()
        {
            /*if (_loading)
                return;*/
            paramTriggerType.ParameterRange = NgParameterInfo.GetTriggerTypeRange(_level.Settings).ToParameterRange();
            paramTargetType.ParameterRange = NgParameterInfo.GetTargetTypeRange(_level.Settings, TriggerType).ToParameterRange();
            paramTarget.ParameterRange = NgParameterInfo.GetTargetRange(_level.Settings, TriggerType, TargetType, paramTimer.Parameter);
            paramTimer.ParameterRange = NgParameterInfo.GetTimerRange(_level.Settings, TriggerType, TargetType, paramTarget.Parameter);
            paramExtra.ParameterRange = NgParameterInfo.GetExtraRange(_level.Settings, TriggerType, TargetType, paramTarget.Parameter, paramTimer.Parameter);

            UpdateExportToTrigger();
        }

        private TriggerType TriggerType => paramTriggerType.Parameter is TriggerParameterUshort ?
                (TriggerType)((TriggerParameterUshort)(paramTriggerType.Parameter)).Key : TriggerType.Trigger;

        private TriggerTargetType TargetType => paramTargetType.Parameter is TriggerParameterUshort ?
                (TriggerTargetType)((TriggerParameterUshort)(paramTargetType.Parameter)).Key : TriggerTargetType.Object;

        private byte CodeBits
        {
            get
            {
                byte codeBits = 0;
                codeBits |= (byte)(cbBit1.Checked ? (1 << 0) : 0);
                codeBits |= (byte)(cbBit2.Checked ? (1 << 1) : 0);
                codeBits |= (byte)(cbBit3.Checked ? (1 << 2) : 0);
                codeBits |= (byte)(cbBit4.Checked ? (1 << 3) : 0);
                codeBits |= (byte)(cbBit5.Checked ? (1 << 4) : 0);
                return codeBits;
            }
        }

        private TriggerInstance TestTrigger => new TriggerInstance(new RectangleInt2())
            {
                TriggerType = TriggerType,
                TargetType = TargetType,
                Target = paramTarget.Parameter,
                Timer = paramTimer.Parameter,
                Extra = paramExtra.Parameter,
                CodeBits = CodeBits,
                OneShot = cbOneShot.Checked
            };

        private void butOK_Click(object sender, EventArgs e)
        {
            // Test if everything is 'ok'
            if (!NgParameterInfo.TriggerIsValid(_level.Settings, TestTrigger))
                if (DarkMessageBox.Show(this, "The currently selected trigger data is not valid for the engine.",
                    "Trigger invalid", MessageBoxButtons.OKCancel, MessageBoxIcon.Asterisk) != DialogResult.OK)
                    return;

            // Store some values like we have not NG triggers
            _trigger.TriggerType = TriggerType;
            _trigger.TargetType = TargetType;
            _trigger.Target = paramTarget.Parameter;
            _trigger.Timer = paramTimer.Parameter;
            _trigger.Extra = paramExtra.Parameter;
            _trigger.CodeBits = CodeBits;
            _trigger.OneShot = cbOneShot.Checked;

            // Close
            DialogResult = DialogResult.OK;
            Close();
        }

        private void butCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }

        private const string _noScriptIdStr = "<NoScriptID>";

        private void paramTriggerType_ParameterChanged(object sender, EventArgs e)
        {
            UpdateDialog();
        }

        private void paramTargetType_ParameterChanged(object sender, EventArgs e)
        {
            UpdateDialog();
        }

        private void paramTarget_ParameterChanged(object sender, EventArgs e)
        {
            UpdateDialog();
        }

        private void paramTimer_ParameterChanged(object sender, EventArgs e)
        {
            UpdateDialog();
        }

        private void paramExtra_ParameterChanged(object sender, EventArgs e)
        {
            UpdateDialog();
        }

        private void UpdateExportToTrigger()
        {
            scriptExportPanel.Visible = _level.Settings.GameVersion == GameVersion.TRNG;
            if (_level.Settings.GameVersion != GameVersion.TRNG)
                return;

            try
            {
                tbScript.Text = NgParameterInfo.ExportToScriptTrigger(_level, TestTrigger);
                scriptExportPanel.Enabled = true;
            }
            catch (NgParameterInfo.ExceptionScriptNotSupported)
            {
                tbScript.Text = "Not supported";
                scriptExportPanel.Enabled = false;
            }
            catch (NgParameterInfo.ExceptionScriptIdMissing)
            {
                tbScript.Text = "Click to generate";
                scriptExportPanel.Enabled = false;
            }
            catch (Exception exc)
            {
                tbScript.Text = "Check all fields";
                scriptExportPanel.Enabled = true;
                logger.Debug(exc, "\"ExportToScriptTrigger\" failed.");
            }
        }

        private void AllocateNewScriptIds()
        {
            (paramTarget.Parameter as IHasScriptID)?.AllocateNewScriptId();
            (paramTimer.Parameter as IHasScriptID)?.AllocateNewScriptId();
            (paramExtra.Parameter as IHasScriptID)?.AllocateNewScriptId();
        }

        private void butCopyToClipboard_Click(object sender, EventArgs e)
        {
            AllocateNewScriptIds();
            UpdateExportToTrigger();
            Clipboard.SetText(tbScript.Text);
        }

        private void FormTrigger_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            if (tbScript.Bounds.Contains(e.Location))
            {
                AllocateNewScriptIds();
                UpdateExportToTrigger();
            }
        }

        private void cbRawMode_CheckedChanged(object sender, EventArgs e)
        {
            foreach (var control in Controls.OfType<TriggerParameterControl>())
                control.RawMode = cbRawMode.Checked;
        }
    }
}
