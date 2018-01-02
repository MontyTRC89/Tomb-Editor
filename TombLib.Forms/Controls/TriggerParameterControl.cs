using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using TombLib.LevelData;
using TombLib.NG;
using DarkUI.Config;
using NLog;
using TombLib.Utils;

namespace TombLib.Controls
{
    public partial class TriggerParameterControl : UserControl
    {
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();

        public event Action<ObjectInstance> ViewObject;
        public event Action<Room> ViewRoom;
        public event EventHandler ParameterChanged;

        private ITriggerParameter _parameter;
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [ReadOnly(true)]
        public ITriggerParameter Parameter
        {
            get { return _parameter; }
            set
            {
                if (_parameter == value)
                    return;
                bool matched = ParameterRange.ParameterMatches(_parameter, true);
                bool matches = ParameterRange.ParameterMatches(value, true);
                _parameter = value;
                UpdateVisibleControls(matched != matches);
                ParameterChanged?.Invoke(this, EventArgs.Empty);

                butView.Visible =
                    (GetObjectPointer(_parameter) is ObjectInstance) ||
                    (GetObjectPointer(_parameter) is Room);
            }
        }

        private NgParameterRange _parameterRange = new NgParameterRange();
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [ReadOnly(true)]
        public NgParameterRange ParameterRange
        {
            get { return _parameterRange; }
            set
            {
                if (_parameterRange == value)
                    return;
                _parameterRange = value;
                UpdateVisibleControls(true);
            }
        }

        public Level Level { get; set; }
        private bool _currentlyChanging = false;

        public TriggerParameterControl()
        {
            InitializeComponent();
            UpdateVisibleControls(true);
        }

        private void View(ITriggerParameter value)
        {
            value = GetObjectPointer(value);
            if (value is ObjectInstance)
                ViewObject?.Invoke((ObjectInstance)value);
            if (value is Room)
                ViewRoom?.Invoke((Room)value);
        }

        private void butView_Click(object sender, EventArgs e)
        {
            View((ITriggerParameter)combo.SelectedItem);
        }

        private void butReset_Click(object sender, EventArgs e)
        {
            Parameter = null;
        }

        private void combo_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (_currentlyChanging || !combo.Visible)
                return;
            Parameter = (ITriggerParameter)combo.SelectedItem;
            View(Parameter);
        }

        private void numericUpDown_ValueChanged(object sender, EventArgs e)
        {
            if (_currentlyChanging || !numericUpDown.Visible)
                return;
            Parameter = new TriggerParameterUshort(unchecked((ushort)(numericUpDown.Value)));
        }

        private void label_MouseDown(object sender, MouseEventArgs e)
        {
            // Label is used to show an empty numeric up/down.
            if (numericUpDown.Visible && Parameter == null)
                Parameter = new TriggerParameterUshort(0);
        }

        private ITriggerParameter GetObjectPointer(ITriggerParameter parameter)
        {
            if (_parameter is TriggerParameterUshort)
                return ((TriggerParameterUshort)_parameter).NameObject as ITriggerParameter;
            else
                return _parameter;
        }

        private void UpdateVisibleControls(bool repopulate)
        {
            // Prevent combo content manipulation
            try
            {
                _currentlyChanging = true;

                // Populate controls
                if (repopulate)
                    combo.DataSource = null;
                bool typeMatches = ParameterRange.ParameterMatches(Parameter, true);
                butReset.Visible = !typeMatches;
                if (typeMatches)
                {
                    ITriggerParameter[] listOfThings;
                    try
                    {
                        listOfThings = ParameterRange.BuildList(Level)?.ToArray();
                    }
                    catch (Exception exc)
                    {
                        logger.Warn(exc, "Unable to create trigger parameter list.");
                        listOfThings = null;
                    }

                    if (ParameterRange.IsEmpty)
                    {
                        label.Text = "-";
                        label.BackColor = Colors.GreyBackground;
                        combo.Visible = false;
                        numericUpDown.Visible = false;
                        label.Visible = true;
                        colorPreview.Visible = false;
                    }
                    else if (listOfThings == null)
                    {
                        label.BackColor = numericUpDown.BackColor;
                        label.Text = "";
                        combo.Visible = false;
                        numericUpDown.Visible = true;
                        label.Visible = Parameter == null;
                        colorPreview.Visible = false;
                    }
                    else
                    {
                        if (repopulate)
                            combo.DataSource = listOfThings;
                        combo.SelectedItem = listOfThings.FirstOrDefault(item => item.Equals(Parameter)) ?? Parameter;
                        if (combo.SelectedItem == null)
                            combo.Text = "";
                        combo.Visible = true;
                        numericUpDown.Visible = false;
                        label.Visible = false;
                        SetupColorPreview(combo.SelectedItem?.ToString());
                    }
                }
                else
                {
                    label.BackColor = Colors.GreyBackground;
                    label.Text = "Wrong parameter: " + (Parameter?.ToString() ?? "<null>");
                    combo.Visible = false;
                    numericUpDown.Visible = false;
                    label.Visible = true;
                }
            }
            finally
            {
                _currentlyChanging = false;
            }
        }

        private void SetupColorPreview(string text)
        {
            if (string.IsNullOrEmpty(text))
                return;
            byte red, green, blue;
            if (!byte.TryParse(ParseFor(text, "Red="), out red))
            {
                colorPreview.Visible = false;
                return;
            }
            if (!byte.TryParse(ParseFor(text, "Green="), out green))
            {
                colorPreview.Visible = false;
                return;
            }
            if (!byte.TryParse(ParseFor(text, "Blue="), out blue))
            {
                colorPreview.Visible = false;
                return;
            }
            colorPreview.BackColor = Color.FromArgb(red, green, blue);
            colorPreview.Visible = true;
        }

        private static string ParseFor(string text, string searched)
        {
            int startPos = text.IndexOf(searched, StringComparison.InvariantCultureIgnoreCase);
            if (startPos == -1)
                return null;
            startPos += searched.Length;

            int endPos = startPos;
            while (endPos < text.Length)
            {
                if (char.IsWhiteSpace(text[endPos]))
                    break;
                if (text[endPos] == ')' || text[endPos] == '(' || text[endPos] == '=')
                    break;
                ++endPos;
            }
            return text.Substring(startPos, endPos - startPos);
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [ReadOnly(true)]
        public override Size MinimumSize
        {
            get { return base.MinimumSize; }
            set  { base.MinimumSize = value; }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [ReadOnly(true)]
        public override Size MaximumSize
        {
            get { return base.MaximumSize; }
            set { base.MaximumSize = value; }
        }
    }
}
