using DarkUI.Controls;
using System;
using System.Drawing;
using System.Windows.Forms;
using TombLib.Forms;
using TombLib.Utils;

namespace TombLib.Controls.VisualScripting
{
    public partial class FormMultilineText : PopUpWindow
    {
        private DarkTextBox _callbackControl = null;

        public FormMultilineText(Point position, DarkTextBox callbackControl) : base(position, true)
        {
            InitializeComponent();

            _callbackControl = callbackControl;
            tbText.Text = TextExtensions.SingleLineToMultiLine(callbackControl.Text);
        }

        protected override void OnShown(EventArgs e)
        {
            base.OnShown(e);
            tbText.DeselectAll();
        }

        protected override void OnDeactivate(EventArgs e)
        {
            base.OnDeactivate(e);
            Close();
        }

        private void tbText_TextChanged(object sender, EventArgs e)
        {
            using (var g = tbText.CreateGraphics())
            {
                var size = g.MeasureString(tbText.Text, tbText.Font, tbText.Width);
                var dimensions = new Size((int)Math.Ceiling(size.Width), (int)Math.Ceiling(size.Height));
                tbText.ScrollBars = dimensions.Height > tbText.Height - tbText.Padding.Top - tbText.Padding.Bottom ? ScrollBars.Vertical : ScrollBars.None;
            }

            _callbackControl.Text = TextExtensions.MultiLineToSingleLine(tbText.Text);
        }
    }
}
