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
        public int TriggerID { get; set; }
        public TriggerInstance Trigger { get; set; }
        
        private TriggerInstance _trigger;
        private Editor _editor = Editor.Instance;
        private List<int> _items;

        public FormTrigger()
        {
            InitializeComponent();
        }

        private void comboType_SelectedIndexChanged(object sender, EventArgs e)
        {
            
        }

        private void FormTrigger_Load(object sender, EventArgs e)
        {
            /*if (Trigger==0)
            {
                _trigger = new Geometry.TriggerInstance(0, 0);
                _trigger.TriggerType = TriggerType.Trigger;
                _trigger.TargetType = TriggerTargetType.Object;
            }
            else
            {
                _trigger
            }*/

            if (TriggerID == -1)
            {
                comboType.SelectedIndex = 0;
                comboTargetType.SelectedIndex = 0;
                cbBit1.Checked = true;
                cbBit2.Checked = true;
                cbBit3.Checked = true;
                cbBit4.Checked = true;
                cbBit5.Checked = true;
                cbOneShot.Checked = false;
                tbTimer.Text = "0";
            }
            else
            {
                comboType.SelectedIndex = (int)Trigger.TriggerType;
                comboTargetType.SelectedIndex = (int)Trigger.TargetType;
                cbBit1.Checked = Trigger.Bits[0];
                cbBit2.Checked = Trigger.Bits[1];
                cbBit3.Checked = Trigger.Bits[2];
                cbBit4.Checked = Trigger.Bits[3];
                cbBit5.Checked = Trigger.Bits[4];
                cbOneShot.Checked = Trigger.OneShot;
                tbTimer.Text = Trigger.Timer.ToString();
            }
        }

        private void comboTargetType_SelectedIndexChanged(object sender, EventArgs e)
        {
            TriggerTargetType triggerTargetType = (TriggerTargetType)comboTargetType.SelectedIndex;

            switch (triggerTargetType)
            {
                case TriggerTargetType.Object:
                    tbParameter.Visible = false;
                    comboParameter.Visible = true;

                    comboParameter.Items.Clear();
                    _items = new List<int>();

                    for (int i=0;i<_editor.Level.Objects.Count;i++)
                    {
                        IObjectInstance instance = _editor.Level.Objects.ElementAt(i).Value;

                        if (instance.Type==ObjectInstanceType.Moveable)
                        {
                            MoveableInstance mov = (MoveableInstance)instance;

                            _items.Add(instance.ID);
                            comboParameter.Items.Add(_editor.MoveablesObjectIds[(int)mov.Model.ObjectID] + " ID = " + instance.ID + ", Room = " + instance.Room +
                                                     ", X = " + mov.Position.X + ", Z = " + mov.Position.Z);
                            if (TriggerID != -1 && Trigger.Target == instance.ID) comboParameter.SelectedIndex = comboParameter.Items.Count - 1;
                        }
                    }

                    if (comboParameter.Items.Count != 0 && comboParameter.SelectedIndex==-1) comboParameter.SelectedIndex = 0;

                    break;

                case TriggerTargetType.Camera:
                    tbParameter.Visible = false;
                    comboParameter.Visible = true;

                    comboParameter.Items.Clear();
                    _items = new List<int>();

                    for (int i = 0; i < _editor.Level.Objects.Count; i++)
                    {
                        IObjectInstance instance = _editor.Level.Objects[i];

                        if (instance.Type == ObjectInstanceType.Camera)
                        {
                            CameraInstance mov = (CameraInstance)instance;

                            _items.Add(instance.ID);
                            comboParameter.Items.Add("Camera ID = " + instance.ID + ", Room = " + instance.Room +
                                                     ", X = " + mov.Position.X + ", Z = " + mov.Position.Z);
                            if (TriggerID != -1 && Trigger.Target == instance.ID) comboParameter.SelectedIndex = comboParameter.Items.Count - 1;
                        }
                    }

                    if (comboParameter.Items.Count != 0 && comboParameter.SelectedIndex == -1) comboParameter.SelectedIndex = 0;

                    break;

                case TriggerTargetType.Sink:
                    tbParameter.Visible = false;
                    comboParameter.Visible = true;

                    comboParameter.Items.Clear();
                    _items = new List<int>();

                    for (int i = 0; i < _editor.Level.Objects.Count; i++)
                    {
                        IObjectInstance instance = _editor.Level.Objects[i];

                        if (instance.Type == ObjectInstanceType.Sink)
                        {
                            SinkInstance mov = (SinkInstance)instance;

                            _items.Add(instance.ID);
                            comboParameter.Items.Add("Sink ID = " + instance.ID + ", Room = " + instance.Room +
                                                     ", X = " + mov.Position.X + ", Z = " + mov.Position.Z);
                            if (TriggerID != -1 && Trigger.Target == instance.ID) comboParameter.SelectedIndex = comboParameter.Items.Count - 1;
                        }
                    }

                    if (comboParameter.Items.Count != 0 && comboParameter.SelectedIndex == -1) comboParameter.SelectedIndex = 0;

                    break;

                case TriggerTargetType.FlipEffect:
                    tbParameter.Visible = true;
                    comboParameter.Visible = false;
                    if (TriggerID == -1)
                        tbParameter.Text = "0";
                    else
                        tbParameter.Text = Trigger.Target.ToString();

                    break;

                case TriggerTargetType.FlipOn:
                    tbParameter.Visible = true;
                    comboParameter.Visible = false;
                    if (TriggerID == -1)
                        tbParameter.Text = "0";
                    else
                        tbParameter.Text = Trigger.Target.ToString();

                    break;

                case TriggerTargetType.FlipOff:
                    tbParameter.Visible = true;
                    comboParameter.Visible = false;
                    if (TriggerID == -1)
                        tbParameter.Text = "0";
                    else
                        tbParameter.Text = Trigger.Target.ToString();

                    break;

                case TriggerTargetType.Target:
                    tbParameter.Visible = false;
                    comboParameter.Visible = true;

                    comboParameter.Items.Clear();
                    _items = new List<int>();

                    for (int i = 0; i < _editor.Level.Objects.Count; i++)
                    {
                        IObjectInstance instance = _editor.Level.Objects[i];

                        if (instance.Type == ObjectInstanceType.Moveable)
                        {
                            MoveableInstance mov = (MoveableInstance)instance;
                            if (mov.ObjectID == 422 || 1==1)
                            {
                                _items.Add(instance.ID);
                                comboParameter.Items.Add("Target ID = " + instance.ID + ", Room = " + instance.Room +
                                                         ", X = " + mov.Position.X + ", Z = " + mov.Position.Z);
                                if (TriggerID != -1 && Trigger.Target == instance.ID) comboParameter.SelectedIndex = comboParameter.Items.Count - 1;
                            }
                        }
                    }

                    if (comboParameter.Items.Count != 0 && comboParameter.SelectedIndex == -1) comboParameter.SelectedIndex = 0;

                    break;

                case TriggerTargetType.FlipMap:
                    tbParameter.Visible = true;
                    comboParameter.Visible = false;
                    if (TriggerID == -1)
                        tbParameter.Text = "0";
                    else
                        tbParameter.Text = Trigger.Target.ToString();

                    break;

                case TriggerTargetType.FinishLevel:
                    tbParameter.Visible = true;
                    comboParameter.Visible = false;
                    if (TriggerID == -1)
                        tbParameter.Text = "0";
                    else
                        tbParameter.Text = Trigger.Target.ToString();

                    break;

                case TriggerTargetType.Secret:
                    tbParameter.Visible = true;
                    comboParameter.Visible = false;
                    if (TriggerID == -1)
                        tbParameter.Text = "0";
                    else
                        tbParameter.Text = Trigger.Target.ToString();

                    break;

                case TriggerTargetType.PlayAudio:
                    tbParameter.Visible = true;
                    comboParameter.Visible = false;
                    if (TriggerID == -1)
                        tbParameter.Text = "0";
                    else
                        tbParameter.Text = Trigger.Target.ToString();

                    break;

                case TriggerTargetType.FlyByCamera:
                    tbParameter.Visible = false;
                    comboParameter.Visible = true;

                    comboParameter.Items.Clear();
                    _items = new List<int>();

                    for (int i = 0; i < _editor.Level.Objects.Count; i++)
                    {
                        IObjectInstance instance = _editor.Level.Objects[i];

                        if (instance.Type == ObjectInstanceType.FlyByCamera)
                        {
                            FlybyCameraInstance mov = (FlybyCameraInstance)instance;
                            _items.Add(instance.ID);
                            comboParameter.Items.Add("Flyby ID = " + instance.ID + ", Room = " + instance.Room +
                                                         ", X = " + mov.Position.X + ", Z = " + mov.Position.Z);
                            if (TriggerID != -1 && Trigger.Target == instance.ID) comboParameter.SelectedIndex = comboParameter.Items.Count - 1;
                        }
                    }

                    if (comboParameter.Items.Count != 0 && comboParameter.SelectedIndex == -1) comboParameter.SelectedIndex = 0;

                    break;

                case TriggerTargetType.FMV:
                    tbParameter.Visible = true;
                    comboParameter.Visible = false;

                    if (TriggerID == -1)
                        tbParameter.Text = "0";
                    else
                        tbParameter.Text = Trigger.Target.ToString();

                    break;
            }
        }

        private void butOK_Click(object sender, EventArgs e)
        {
            if (TriggerID == -1) Trigger = new Geometry.TriggerInstance(0, 0);

            Trigger.TriggerType = (TriggerType)comboType.SelectedIndex;
            Trigger.TargetType = (TriggerTargetType)comboTargetType.SelectedIndex;
            Trigger.Timer = Int16.Parse(tbTimer.Text);
            Trigger.Bits[0] = cbBit1.Checked;
            Trigger.Bits[1] = cbBit2.Checked;
            Trigger.Bits[2] = cbBit3.Checked;
            Trigger.Bits[3] = cbBit4.Checked;
            Trigger.Bits[4] = cbBit5.Checked;
            Trigger.OneShot= cbOneShot.Checked;

            if (Trigger.TargetType == TriggerTargetType.Object || Trigger.TargetType == TriggerTargetType.Camera ||
                Trigger.TargetType == TriggerTargetType.Target || Trigger.TargetType == TriggerTargetType.FlyByCamera ||
                Trigger.TargetType == TriggerTargetType.Sink)
            {
                if (comboParameter.SelectedIndex == -1)
                {
                    DarkUI.Forms.DarkMessageBox.ShowWarning("You don't have in your project any object of the type that you have required",
                                                            "Save trigger", DarkUI.Forms.DarkDialogButton.Ok);
                    return;
                }

                Trigger.Target = _items[comboParameter.SelectedIndex];
            }
            else
            {
                Trigger.Target = Int16.Parse(tbParameter.Text);
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
