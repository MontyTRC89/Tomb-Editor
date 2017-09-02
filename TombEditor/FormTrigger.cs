using DarkUI.Forms;
using System;
using System.Linq;
using System.Windows.Forms;
using TombEditor.Geometry;

namespace TombEditor
{
    public partial class FormTrigger : DarkForm
    {
        private Level _level;
        private TriggerInstance _trigger;

        public FormTrigger(Level level, TriggerInstance trigger)
        {
            _level = level;
            _trigger = trigger;
            InitializeComponent();

            foreach (TriggerType triggerType in Enum.GetValues(typeof(TriggerType)))
                comboType.Items.Add(triggerType);
            foreach (TriggerTargetType triggerTargetType in Enum.GetValues(typeof(TriggerTargetType)))
                comboTargetType.Items.Add(triggerTargetType);
        }

        private void FormTrigger_Load(object sender, EventArgs e)
        {
            comboType.SelectedItem = _trigger.TriggerType;
            comboTargetType.SelectedItem = _trigger.TargetType;
            cbBit1.Checked = (_trigger.CodeBits & (1 << 0)) != 0;
            cbBit2.Checked = (_trigger.CodeBits & (1 << 1)) != 0;
            cbBit3.Checked = (_trigger.CodeBits & (1 << 2)) != 0;
            cbBit4.Checked = (_trigger.CodeBits & (1 << 3)) != 0;
            cbBit5.Checked = (_trigger.CodeBits & (1 << 4)) != 0;
            cbOneShot.Checked = _trigger.OneShot;
            tbTimer.Text = _trigger.Timer.ToString();
        }

        private void comboTargetType_SelectedIndexChanged(object sender, EventArgs e)
        {
            TriggerTargetType triggerTargetType = (TriggerTargetType)comboTargetType.SelectedItem;

            switch (triggerTargetType)
            {
                case TriggerTargetType.Object:
                    tbParameter.Visible = false;
                    comboParameter.Visible = true;
                    FindAndAddObjects<MoveableInstance>();
                    break;

                case TriggerTargetType.Camera:
                    tbParameter.Visible = false;
                    comboParameter.Visible = true;
                    FindAndAddObjects<CameraInstance>();
                    break;

                case TriggerTargetType.Sink:
                    tbParameter.Visible = false;
                    comboParameter.Visible = true;
                    FindAndAddObjects<SinkInstance>();
                    break;

                case TriggerTargetType.FlipEffect:
                    tbParameter.Visible = true;
                    comboParameter.Visible = false;
                    tbParameter.Text = _trigger.TargetData.ToString();
                    break;

                case TriggerTargetType.FlipOn:
                    tbParameter.Visible = true;
                    comboParameter.Visible = false;
                    tbParameter.Text = _trigger.TargetData.ToString();
                    break;

                case TriggerTargetType.FlipOff:
                    tbParameter.Visible = true;
                    comboParameter.Visible = false;
                    tbParameter.Text = _trigger.TargetData.ToString();
                    break;

                case TriggerTargetType.Target:
                    tbParameter.Visible = false;
                    comboParameter.Visible = true;

                    // Actually it is possible to not only target Target objects, but all movables.
                    // This is also useful: It makes sense to target egg a trap or an enemy.
                    FindAndAddObjects<MoveableInstance>();
                    break;

                case TriggerTargetType.FlipMap:
                    tbParameter.Visible = true;
                    comboParameter.Visible = false;
                    tbParameter.Text = _trigger.TargetData.ToString();
                    break;

                case TriggerTargetType.FinishLevel:
                    tbParameter.Visible = true;
                    comboParameter.Visible = false;
                    tbParameter.Text = _trigger.TargetData.ToString();
                    break;

                case TriggerTargetType.Secret:
                    tbParameter.Visible = true;
                    comboParameter.Visible = false;
                    tbParameter.Text = _trigger.TargetData.ToString();
                    break;

                case TriggerTargetType.PlayAudio:
                    tbParameter.Visible = true;
                    comboParameter.Visible = false;
                    tbParameter.Text = _trigger.TargetData.ToString();
                    break;

                case TriggerTargetType.FlyByCamera:
                    tbParameter.Visible = false;
                    comboParameter.Visible = true;
                    FindAndAddObjects<FlybyCameraInstance>();
                    break;

                case TriggerTargetType.FmvNg:
                    tbParameter.Visible = true;
                    comboParameter.Visible = false;
                    tbParameter.Text = _trigger.TargetData.ToString();
                    break;
            }
        }

        private void FindAndAddObjects<T>() where T : ObjectInstance
        {
            comboParameter.Items.Clear();
            comboParameter.SelectedItem = null;

            foreach (Room room in _level.Rooms.Where(room => room != null))
                foreach (var instance in room.Objects.OfType<T>())
                {
                    comboParameter.Items.Add(instance);
                    if (_trigger.TargetObj == instance)
                        comboParameter.SelectedItem = instance;
                }

            if ((comboParameter.Items.Count > 0) && (comboParameter.SelectedItem == null))
                comboParameter.SelectedIndex = 0; // Select top item, no matter what it is.
        }

        private void butOK_Click(object sender, EventArgs e)
        {
            _trigger.TriggerType = (TriggerType)comboType.SelectedItem;
            _trigger.TargetType = (TriggerTargetType)comboTargetType.SelectedItem;
            _trigger.Timer = Int16.Parse(tbTimer.Text);
            byte codeBits = 0;
            codeBits |= (byte)(cbBit1.Checked ? (1 << 0) : 0);
            codeBits |= (byte)(cbBit2.Checked ? (1 << 1) : 0);
            codeBits |= (byte)(cbBit3.Checked ? (1 << 2) : 0);
            codeBits |= (byte)(cbBit4.Checked ? (1 << 3) : 0);
            codeBits |= (byte)(cbBit5.Checked ? (1 << 4) : 0);
            _trigger.CodeBits = codeBits;
            _trigger.OneShot = cbOneShot.Checked;

            if (_trigger.TargetType == TriggerTargetType.Object || _trigger.TargetType == TriggerTargetType.Camera ||
                _trigger.TargetType == TriggerTargetType.Target || _trigger.TargetType == TriggerTargetType.FlyByCamera ||
                _trigger.TargetType == TriggerTargetType.Sink)
            {
                if (comboParameter.SelectedItem == null)
                {
                    DarkUI.Forms.DarkMessageBox.ShowWarning("You don't have in your project any object of the type that you have required",
                                                            "Save trigger", DarkUI.Forms.DarkDialogButton.Ok);
                    return;
                }

                _trigger.TargetObj = (ObjectInstance)comboParameter.SelectedItem;
                _trigger.TargetData = 0;
            }
            else
            {
                _trigger.TargetObj = null;
                _trigger.TargetData = Int16.Parse(tbParameter.Text);
            }

            DialogResult = DialogResult.OK;
            this.Close();
        }

        private void butCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            this.Close();
        }
    }
}
