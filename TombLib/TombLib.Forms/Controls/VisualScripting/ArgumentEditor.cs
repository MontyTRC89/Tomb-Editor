using DarkUI.Config;
using System;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using TombLib.LevelData;
using TombLib.LevelData.VisualScripting;
using TombLib.Utils;

namespace TombLib.Controls.VisualScripting
{
    public partial class ArgumentEditor : UserControl
    {
        private class ComboBoxItem
        {
            public ComboBoxItem(string displayText, string value)
            {
                DisplayText = displayText;
                Value = value;
            }

            public string DisplayText;
            public string Value;
            public override string ToString() => DisplayText;
        }

        private const string _separatorChar = ",";

        private ArgumentType _argumentType = ArgumentType.Numerical;

        public event EventHandler ValueChanged;
        private void OnValueChanged(EventArgs e)
            => ValueChanged?.Invoke(this, e);

        public event EventHandler LocatedItemFound;
        private void OnLocatedItemFound(IHasLuaName item, EventArgs e)
            => LocatedItemFound?.Invoke(item, e);

        public new string Text 
        { 
            get { return _text; }
            set { UnboxValue(value); }
        }
        private string _text = string.Empty;

        public ArgumentEditor()
        {
            InitializeComponent();
            container.Visible = (LicenseManager.UsageMode == LicenseUsageMode.Runtime);

            // HACK: Fix textbox UI height
            tbString.AutoSize = false;
        }

        public void SetArgumentType(ArgumentType type, NodeEditor editor)
        {
            _argumentType = type;

            if (type >= ArgumentType.LuaScript)
                container.SelectedTab = tabList;
            else
            {
                container.SelectedIndex = (int)type;
                return;
            }

            switch (type)
            {
                case ArgumentType.Sinks:
                case ArgumentType.Statics:
                case ArgumentType.Moveables:
                case ArgumentType.Volumes:
                case ArgumentType.Cameras:
                case ArgumentType.FlybyCameras:
                    panelLocate.Size = new Size(cbList.Height, cbList.Height);
                    panelLocate.Visible = true;
                    break;

                default:
                    panelLocate.Visible = false;
                    break;
            }

            cbList.Items.Clear();

            switch (type)
            {
                case ArgumentType.LuaScript:
                    foreach (var item in editor.CachedLuaFunctions)
                        cbList.Items.Add(new ComboBoxItem(item, "LevelFuncs." + item));
                    break;
                case ArgumentType.Sinks:
                    foreach (var item in editor.CachedSinks)
                        cbList.Items.Add(new ComboBoxItem(item.ToShortString(), item.LuaName == null ? string.Empty : item.LuaName));
                    break;
                case ArgumentType.Statics:
                    foreach (var item in editor.CachedStatics)
                        cbList.Items.Add(new ComboBoxItem(item.ToShortString(), item.LuaName == null ? string.Empty : item.LuaName));
                    break;
                case ArgumentType.Moveables:
                    foreach (var item in editor.CachedMoveables)
                        cbList.Items.Add(new ComboBoxItem(item.ToShortString(), item.LuaName == null ? string.Empty : item.LuaName));
                    break;
                case ArgumentType.Volumes:
                    foreach (var item in editor.CachedVolumes)
                        cbList.Items.Add(new ComboBoxItem(item.ToShortString(), item.LuaName == null ? string.Empty : item.LuaName));
                    break;
                case ArgumentType.Cameras:
                    foreach (var item in editor.CachedCameras)
                        cbList.Items.Add(new ComboBoxItem(item.ToShortString(), item.LuaName == null ? string.Empty : item.LuaName));
                    break;
                case ArgumentType.FlybyCameras:
                    foreach (var item in editor.CachedFlybys)
                        cbList.Items.Add(new ComboBoxItem(item.ToShortString(), item.LuaName == null ? string.Empty : item.LuaName));
                    break;
                case ArgumentType.Rooms:
                    foreach (var item in editor.CachedRooms)
                        cbList.Items.Add(new ComboBoxItem(item.ToString(), item.Name));
                    break;
                case ArgumentType.SoundEffects:
                    foreach (var item in editor.CachedSoundInfos)
                        cbList.Items.Add(new ComboBoxItem(item.ToString(), item.Id.ToString()));
                    break;
                case ArgumentType.CompareOperand:
                    foreach (var item in Enum.GetValues(typeof(ConditionType)).OfType<ConditionType>())
                        cbList.Items.Add(new ComboBoxItem(item.ToString().SplitCamelcase(), cbList.Items.Count.ToString()));
                    break;
                default:
                    break;
            }
        }

        public void SetToolTip(ToolTip toolTip, string caption)
        {
            foreach (TabPage tab in container.TabPages)
                foreach (Control control in tab.Controls)
                    toolTip.SetToolTip(control, caption);
        }

        public IHasLuaName LocateItem(NodeEditor editor)
        {
            if (cbList.Items.Count == 0 || cbList.SelectedIndex == -1)
                return null;

            switch (_argumentType)
            {
                case ArgumentType.Sinks:
                    return editor.CachedSinks.FirstOrDefault(i => i.LuaName == (cbList.SelectedItem as ComboBoxItem).Value);
                case ArgumentType.Statics:
                    return editor.CachedStatics.FirstOrDefault(i => i.LuaName == (cbList.SelectedItem as ComboBoxItem).Value);
                case ArgumentType.Moveables:
                    return editor.CachedMoveables.FirstOrDefault(i => i.LuaName == (cbList.SelectedItem as ComboBoxItem).Value);
                case ArgumentType.Volumes:
                    return editor.CachedVolumes.FirstOrDefault(i => i.LuaName == (cbList.SelectedItem as ComboBoxItem).Value);
                case ArgumentType.Cameras:
                    return editor.CachedCameras.FirstOrDefault(i => i.LuaName == (cbList.SelectedItem as ComboBoxItem).Value);
                case ArgumentType.FlybyCameras:
                    return editor.CachedFlybys.FirstOrDefault(i => i.LuaName == (cbList.SelectedItem as ComboBoxItem).Value);
                default:
                    return null;
            }
        }

        protected override void OnPaintBackground(PaintEventArgs e)
        {
            using (var b = new SolidBrush(Colors.LightBackground))
                e.Graphics.FillRectangle(b, ClientRectangle);

            using (var p = new Pen(Colors.GreySelection, 1))
                e.Graphics.DrawRectangle(p, 0, 0, ClientSize.Width - 1, ClientSize.Height - 1);

            if (LicenseManager.UsageMode == LicenseUsageMode.Runtime)
                e.Graphics.DrawString("Changes dependent on arg type in runtime.", Font, Brushes.DarkGray, ClientRectangle,
                    new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center });

        }

        protected override void OnLocationChanged(EventArgs e)
        {
            // Absorb event
        }

        protected override void OnResize(EventArgs e)
        {
            // Absorb event
        }

        private void UnboxValue(string source)
        {
            switch((ArgumentType)container.SelectedIndex)
            {
                case ArgumentType.Boolean:
                    {
                        bool result;
                        if (!(bool.TryParse(source, out result)))
                            result = false;
                        rbTrue.Checked = result;
                        break;
                    }
                case ArgumentType.Numerical:
                    {
                        float result;
                        if (!(float.TryParse(source, out result)))
                            result = 0.0f;
                        try { nudNumerical.Value = (decimal)result; }
                        catch { nudNumerical.Value = (decimal)0; }
                        
                        break;
                    }
                case ArgumentType.Vector3:
                    {
                        var floats = UnboxVector3Value(source);

                        try
                        {
                            for (int i = 0; i < 3; i++)
                            {
                                var currentFloat = 0.0f;
                                if (floats.Length > i)
                                    currentFloat = floats[i];

                                switch (i)
                                {
                                    case 0: nudVector3X.Value = (decimal)currentFloat; break;
                                    case 1: nudVector3Y.Value = (decimal)currentFloat; break;
                                    case 2: nudVector3Z.Value = (decimal)currentFloat; break;
                                }
                            }
                        }
                        catch
                        {
                            nudVector3X.Value = nudVector3Y.Value = nudVector3Z.Value = (decimal)0;
                        }
                        break;
                    }
                case ArgumentType.Color:
                    {
                        var floats = UnboxVector3Value(source);
                        var bytes = new byte[3] { 0, 0, 0 };

                        for (int i = 0; i < 3; i++)
                            if (floats.Length > i)
                                bytes[i] = (byte)MathC.Clamp(floats[i], 0, 255);

                        panelColor.BackColor = Color.FromArgb(255, bytes[0], bytes[1], bytes[2]);
                        break;
                    }
                case ArgumentType.String:
                    {
                        tbString.Text = source;
                        break;
                    }
                default: // Lists
                    {
                        var index = cbList.Items.Cast<ComboBoxItem>().FirstOrDefault(i => i.Value == source);
                        if (index != null)
                            cbList.SelectedItem = index;
                        break;
                    }
            }
        }

        private float[] UnboxVector3Value(string source)
        {
            return source.Split(new string[] { _separatorChar }, StringSplitOptions.None).Select(x =>
            {
                float result;
                if (float.TryParse(x.Trim(), out result))
                    return result;
                else
                    return 0.0f;
            }).ToArray();
        }

        private void BoxBoolValue()
        {
            _text = rbTrue.Checked.ToString().ToLower();
            OnValueChanged(EventArgs.Empty);
        }

            private void BoxVector3Value()
        {
            var x = ((float)nudVector3X.Value).ToString();
            var y = ((float)nudVector3Y.Value).ToString();
            var z = ((float)nudVector3Z.Value).ToString();
            _text = x + _separatorChar + y + _separatorChar + z;
            OnValueChanged(EventArgs.Empty);
        }

        private void BoxColorValue()
        {
            _text = panelColor.BackColor.R.ToString() + _separatorChar + 
                    panelColor.BackColor.G.ToString() + _separatorChar + 
                    panelColor.BackColor.B.ToString();
            OnValueChanged(EventArgs.Empty);
        }

        private void BoxNumericalValue()
        {
            _text = ((float)nudNumerical.Value).ToString();
            OnValueChanged(EventArgs.Empty);
        }

        private void BoxStringValue()
        {
            _text = tbString.Text;
            OnValueChanged(EventArgs.Empty);
        }

        private void BoxListValue()
        {
            _text = (cbList.SelectedItem as ComboBoxItem).Value;
            OnValueChanged(EventArgs.Empty);
        }

        private void rbTrue_CheckedChanged(object sender, EventArgs e) => BoxBoolValue();
        private void rbFalse_CheckedChanged(object sender, EventArgs e) => BoxBoolValue();
        private void nudNumerical_ValueChanged(object sender, EventArgs e) => BoxNumericalValue();
        private void nudVector3_ValueChanged(object sender, EventArgs e) => BoxVector3Value();
        private void tbString_TextChanged(object sender, EventArgs e) => BoxStringValue();
        private void panelColor_BackColorChanged(object sender, EventArgs e) => BoxColorValue();
        private void cbList_SelectedIndexChanged(object sender, EventArgs e) => BoxListValue();

        private void panelColor_MouseClick(object sender, MouseEventArgs e)
        {
            using (var colorDialog = new RealtimeColorDialog())
            {
                var oldColor = panelColor.BackColor;
                colorDialog.Color = oldColor;
                colorDialog.FullOpen = true;
                if (colorDialog.ShowDialog(this) != DialogResult.OK)
                    return;

                if (oldColor != colorDialog.Color)
                {
                    panelColor.BackColor = colorDialog.Color;
                    BoxColorValue();
                }
            }
        }

        private void butLocate_Click(object sender, EventArgs e)
        {
            var item = LocateItem((Parent as VisibleNodeBase)?.Editor);
            if (item == null)
                return;

            OnLocatedItemFound(item, EventArgs.Empty);
        }
    }
}
