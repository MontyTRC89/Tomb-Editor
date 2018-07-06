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
        public int MinValue { get { return trackBar.Minimum; } set { trackBar.Minimum = value; } }
        public int MaxValue { get { return trackBar.Maximum; } set { trackBar.Maximum = value; } }
        public int Value { get { return trackBar.Value; } set { trackBar.Value = value; } }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public List<WadAnimCommand> AnimationCommands { get; set; } = new List<WadAnimCommand>();
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public AnimationNode Animation { get; set; }

        private Font _font = new Font("Segoe UI", 8, FontStyle.Regular);

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
                label.Location = new Point(10 + (int)((float)(cmd.Parameter1 / MaxValue) * (float)(Width / MaxValue) * 0.98f), (cmd.Type == WadAnimCommandType.FlipEffect ? 0 : 15)); ;
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

        public event EventHandler ValueChanged
        {
            add { trackBar.ValueChanged += value; }
            remove { trackBar.ValueChanged -= value; }
        }
    }
}
