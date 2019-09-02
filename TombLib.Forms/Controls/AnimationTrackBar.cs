using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using TombLib.Wad;
using TombLib.Graphics;

namespace TombLib.Controls
{
    public partial class AnimationTrackBar : UserControl
    {
        public int Minimum { get; set; }
        public int Maximum { get; set; }

        private int _value;
        public int Value
        {
            get { return _value; }
            set
            {
                // Make sure the new value is within bounds.
                if (value < Minimum) value = Minimum;
                if (value > Maximum) value = Maximum;

                // See if the value has changed.
                if (Value == value) return;

                // Save the new value.
                _value = value;

                // Call event
                ValueChanged?.Invoke(this, new EventArgs());

                // Redraw to show the new value.
                picSlider.Refresh();
            }
        }

        public event EventHandler ValueChanged;

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public List<WadAnimCommand> AnimationCommands { get; set; } = new List<WadAnimCommand>();
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public AnimationNode Animation { get; set; }

        private Font _font = new Font("Segoe UI", 8, FontStyle.Regular);

        private bool MouseIsDown = false;

        public AnimationTrackBar()
        {
            InitializeComponent();
        }

        public void UpdateAnimationCommands()
        {
            panelCommands.Controls.Clear();
            foreach (var cmd in AnimationCommands)
            {
                var label = new Label();
                label.BackColor = (cmd.Type == WadAnimCommandType.FlipEffect ? Color.Goldenrod : Color.Blue);
                label.Size = new Size(10, 15);
                label.Location = new Point(10 + (int)((float)(cmd.Parameter1 / Maximum) * (float)(Width / Maximum) * 0.98f), (cmd.Type == WadAnimCommandType.FlipEffect ? 0 : 15)); ;
                label.Tag = cmd;
                label.Font = _font;
                label.UseCompatibleTextRendering = true;
                //label.BorderStyle = BorderStyle.FixedSingle;
                label.ForeColor = Color.White;
                label.Text = (cmd.Type == WadAnimCommandType.FlipEffect ? "F" : "S");

                var tooltip = new ToolTip();
                tooltip.ToolTipTitle= (cmd.Type == WadAnimCommandType.FlipEffect ? "FlipEffect" : "Play sound");
                tooltip.ToolTipIcon = ToolTipIcon.Info;
                tooltip.IsBalloon = true;
                tooltip.ShowAlways = true;
                tooltip.SetToolTip(label, cmd.ToString());

                panelCommands.Controls.Add(label);
            }
        }

        private void picSlider_MouseDown(object sender, MouseEventArgs e)
        {
            MouseIsDown = true;
            SetValue(XtoValue(e.X));
        }
        private void picSlider_MouseMove(object sender, MouseEventArgs e)
        {
            if (!MouseIsDown) return;
            SetValue(XtoValue(e.X));
        }
        private void picSlider_MouseUp(object sender, MouseEventArgs e)
        {
            MouseIsDown = false;
        }

        // Convert an X coordinate to a value.
        private int XtoValue(int x)
        {
            return Minimum + (Maximum - Minimum) *
                x / (int)(picSlider.ClientSize.Width - 1);
        }

        // Convert value to an X coordinate.
        private int ValueToX(int value)
        {
            return (picSlider.ClientSize.Width - 1) *
                (value - Minimum) / (int)(Maximum - Minimum);
        }

        // Draw the needle.
        private void picSlider_Paint(object sender, PaintEventArgs e)
        {
            // Calculate the needle's X coordinate.
            float x = ValueToX(Value);

            // Draw it.
            using (Pen pen = new Pen(Color.Blue, 2))
            {
                e.Graphics.DrawLine(pen, x, 0,
                    x, picSlider.ClientSize.Height);
            }
        }

        // Set the slider's value. If the value has changed,
        // display the value tooltip.
        private void SetValue(int value)
        {
        }
    }
}
