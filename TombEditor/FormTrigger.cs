using DarkUI.Forms;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using TombEditor.Geometry;

namespace TombEditor
{
    public partial class FormTrigger : DarkForm
    {
        private Level _level;
        private TriggerInstance _trigger;
        private bool comboParameterBeingInitialized = false;

        public FormTrigger(Level level, TriggerInstance trigger, Action<ObjectInstance> selectObject)
        {
            _level = level;
            _trigger = trigger;
            InitializeComponent();

            // Calculate the sizes at runtime since they actually depend on the choosen layout.
            // https://stackoverflow.com/questions/1808243/how-does-one-calculate-the-minimum-client-size-of-a-net-windows-form
            MaximumSize = new Size(32000, Size.Height);
            MinimumSize = new Size(347 + (Size.Height - ClientSize.Height), Size.Height);

            // Populate lists
            foreach (TriggerType triggerType in Enum.GetValues(typeof(TriggerType)))
                comboType.Items.Add(triggerType);
            foreach (TriggerTargetType triggerTargetType in Enum.GetValues(typeof(TriggerTargetType)))
                comboTargetType.Items.Add(triggerTargetType);

            // Center items in the trigger window
            comboParameter.SelectedIndexChanged += delegate
            {
                if (!comboParameterBeingInitialized)
                {
                    ObjectInstance objectToSelect = comboParameter.SelectedItem as ObjectInstance;
                    if (objectToSelect != null)
                        selectObject(objectToSelect);
                }
            };
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
            try
            {
                comboParameterBeingInitialized = true;

                // Save selected item
                var selectedItem = comboParameter.SelectedItem;
                comboParameter.SelectedItem = null;

                // Populate list with new items
                comboParameter.Items.Clear();
                foreach (Room room in _level.Rooms.Where(room => room != null))
                    foreach (var instance in room.Objects.OfType<T>())
                    {
                        comboParameter.Items.Add(instance);
                        if (_trigger.TargetObj == instance)
                            comboParameter.SelectedItem = instance;
                    }

                // Select old item if possible
                if ((selectedItem != null) && comboParameter.Items.Contains(selectedItem))
                    comboParameter.SelectedItem = selectedItem;
            }
            finally
            {
                comboParameterBeingInitialized = false;
            }
        }

        private void butOK_Click(object sender, EventArgs e)
        {
            _trigger.TriggerType = (TriggerType)comboType.SelectedItem;
            _trigger.TargetType = (TriggerTargetType)comboTargetType.SelectedItem;
            _trigger.Timer = short.Parse(tbTimer.Text);
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
                var selectedObject = comboParameter.SelectedItem as ObjectInstance;
                if (selectedObject == null)
                {
                    DarkMessageBox.Show(this, "You don't have in your project any object of the type that you have required", "Save trigger", MessageBoxIcon.Warning);
                    return;
                }

                _trigger.TargetObj = selectedObject;
                _trigger.TargetData = 0;
            }
            else
            {
                if (!Utils.IsValidNumber(tbParameter.Text))
                {
                    DarkMessageBox.ShowWarning("You must insert a valid value for parameter",
                                                            "Save trigger", DarkUI.Forms.DarkDialogButton.Ok);
                    return;
                }

                _trigger.TargetObj = null;
                _trigger.TargetData = short.Parse(tbParameter.Text);
            }

            DialogResult = DialogResult.OK;
            Close();
        }

        private void butCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }
    }
}
