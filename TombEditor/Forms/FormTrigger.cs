using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using DarkUI.Forms;
using NLog;
using TombLib;
using TombLib.Controls;
using TombLib.LevelData;
using TombLib.NG;

namespace TombEditor.Forms
{
    public partial class paramLuaScript : DarkForm
    {
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();

        private readonly Level _level;
        private readonly TriggerInstance _trigger;

        public paramLuaScript(Level level, TriggerInstance trigger, Action<ObjectInstance> selectObject,
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

            foreach (Control control in scriptExportPanel.Controls)
                control.Click += scriptExportPanel_Click;
            scriptExportPanel.Click += scriptExportPanel_Click;

            // Calculate the sizes at runtime since they actually depend on the choosen layout.
            // https://stackoverflow.com/questions/1808243/how-does-one-calculate-the-minimum-client-size-of-a-net-windows-form
            MaximumSize = new Size(32000, Size.Height);
            MinimumSize = new Size(611 + (Size.Height - ClientSize.Height), Size.Height);

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
            tbLuaScript.Text = _trigger.LuaScript;

            // Update the dialog
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
            paramExtra.ParameterRange = NgParameterInfo.GetExtraRange(_level.Settings, TriggerType, TargetType, paramTarget.Parameter, paramTimer.Parameter);

            if (TargetType == TriggerTargetType.LuaScript)
            {
                tbLuaScript.Visible = true;
                paramTarget.Visible = false;
            }
            else
            {
                tbLuaScript.Visible = false;
                paramTarget.Visible = true;
            }

            UpdateExportToTrigger();
        }

        private TriggerType TriggerType => paramTriggerType.Parameter is TriggerParameterUshort ?
                (TriggerType)((TriggerParameterUshort)paramTriggerType.Parameter).Key : TriggerType.Trigger;

        private TriggerTargetType TargetType => paramTargetType.Parameter is TriggerParameterUshort ?
                (TriggerTargetType)((TriggerParameterUshort)paramTargetType.Parameter).Key : TriggerTargetType.Object;

        private byte CodeBits
        {
            get
            {
                byte codeBits = 0;
                codeBits |= (byte)(cbBit1.Checked ? 1 << 0 : 0);
                codeBits |= (byte)(cbBit2.Checked ? 1 << 1 : 0);
                codeBits |= (byte)(cbBit3.Checked ? 1 << 2 : 0);
                codeBits |= (byte)(cbBit4.Checked ? 1 << 3 : 0);
                codeBits |= (byte)(cbBit5.Checked ? 1 << 4 : 0);
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
                    "Trigger invalid", MessageBoxButtons.OKCancel, MessageBoxIcon.Exclamation) != DialogResult.OK)
                    return;

            // Store some values like we have not NG triggers
            _trigger.TriggerType = TriggerType;
            _trigger.TargetType = TargetType;
            _trigger.Target = paramTarget.Parameter;
            _trigger.Timer = paramTimer.Parameter;
            _trigger.Extra = paramExtra.Parameter;
            _trigger.CodeBits = CodeBits;
            _trigger.OneShot = cbOneShot.Checked;
            _trigger.LuaScript = tbLuaScript.Text;

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
                tbScript.Tag  = NgParameterInfo.ExportToScriptTrigger(_level, TestTrigger, true);
                tbScript.Enabled = true;
                butCopyToClipboard.Enabled = true;
                butCopyWithComments.Enabled = true;
            }
            catch (NgParameterInfo.ExceptionScriptNotSupported)
            {
                tbScript.Text = "Not supported";
                tbScript.Tag = null;
                tbScript.Enabled = false;
                butCopyToClipboard.Enabled = false;
                butCopyWithComments.Enabled = false;
            }
            catch (NgParameterInfo.ExceptionScriptIdMissing)
            {
                tbScript.Text = "Click to generate";
                tbScript.Tag = null;
                tbScript.Enabled = false;
                butCopyToClipboard.Enabled = false;
                butCopyWithComments.Enabled = false;
            }
            catch (Exception exc)
            {
                tbScript.Text = "Check all fields";
                tbScript.Tag = null;
                tbScript.Enabled = false;
                butCopyToClipboard.Enabled = false;
                butCopyWithComments.Enabled = false;
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
            if(!string.IsNullOrEmpty(tbScript.Text))
                Clipboard.SetText(tbScript.Text);
        }

        private void butCopyWithComments_Click(object sender, EventArgs e)
        {
            var commentString = tbScript.Tag as string;
            if (!string.IsNullOrEmpty(commentString))
                Clipboard.SetText(commentString);
        }

        private void scriptExportPanel_Click(object sender, EventArgs e)
        {
            if (tbScript.Enabled == false)
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
