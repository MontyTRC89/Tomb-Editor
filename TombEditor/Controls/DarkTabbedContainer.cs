using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Drawing;
using DarkUI.Config;
using DarkUI.Controls;

namespace TombEditor.Controls
{
    class DarkTabbedContainer : TabControl
    {
        private DarkListView _linkedListView;

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public DarkListView LinkedListView
        {
            get { return _linkedListView; }
            set
            {
                // Unlink previous list view, if any
                if (value != null)
                {
                    value.SelectedIndicesChanged -= linkedListView_SelectionChanged;
                    value.Items.Clear();
                }

                _linkedListView = value;

                if (value != null)
                {
                    value.Items.Clear();

                    // Populate options list and select first entry
                    foreach (TabPage tab in TabPages)
                        value.Items.Add(new DarkListItem(tab.Text));
                    value.SelectItem(0);

                    // Link list selection event to container
                    value.SelectedIndicesChanged += linkedListView_SelectionChanged;
                }
            }
        }

        public DarkTabbedContainer()
        {
            SetStyle(ControlStyles.OptimizedDoubleBuffer |
                     ControlStyles.SupportsTransparentBackColor |
                     ControlStyles.ResizeRedraw, true);

            if (LicenseManager.UsageMode == LicenseUsageMode.Runtime)
            {
                SetStyle(ControlStyles.UserPaint, true);

                Appearance = TabAppearance.FlatButtons;
                ItemSize = new Size(0, 1);
                SizeMode = TabSizeMode.Fixed;
            }
        }

        private void linkedListView_SelectionChanged(object sender, EventArgs e)
        {
            SelectedIndex = _linkedListView.SelectedIndices.First();
        }

        protected override bool ProcessDialogKey(Keys keyData)
        {
            if (keyData == (Keys.Tab | Keys.Control))
                return true;

            return base.ProcessDialogKey(keyData);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            using (var b = new SolidBrush(Colors.GreyBackground))
                e.Graphics.FillRectangle(b, ClientRectangle);
        }
    }
}
