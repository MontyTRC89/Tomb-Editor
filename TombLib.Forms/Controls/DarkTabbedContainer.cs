using DarkUI.Config;
using DarkUI.Controls;
using System;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace TombLib.Controls
{
    public class DarkTabbedContainer : TabControl
    {
        private Control _linkedControl;

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public Control LinkedControl
        {
            get { return _linkedControl; }
            set
            {
                if (!(value is DarkListView) && !(value is DarkListBox))
                    return;

                // Unlink previous list view, if any
                if (_linkedControl != null)
                {
                    if (_linkedControl is DarkListView)
                    {
                        var listView = (DarkListView)_linkedControl;
                        listView.SelectedIndicesChanged -= OnChange;
                        listView.Items.Clear();
                    }
                    else if (_linkedControl is DarkComboBox)
                    {
                        var combo = (DarkComboBox)_linkedControl;
                        combo.SelectedIndexChanged -= OnChange;
                        combo.Items.Clear();
                    }
                }

                _linkedControl = value;

                if (_linkedControl != null)
                {
                    if (_linkedControl is DarkListView)
                    {
                        var listView = (DarkListView)_linkedControl;
                        listView.Items.Clear();

                        // Populate options list and select first entry
                        foreach (TabPage tab in TabPages)
                            listView.Items.Add(new DarkListItem(tab.Text));
                        listView.SelectItem(0);

                        // Link list selection event to container
                        listView.SelectedIndicesChanged += OnChange;
                    }
                    else if (_linkedControl is DarkComboBox)
                    {
                        var combo = (DarkComboBox)_linkedControl;
                        combo.Items.Clear();

                        // Populate combo and select first entry
                        foreach (TabPage tab in TabPages)
                            combo.Items.Add(new DarkListItem(tab.Text));
                        combo.SelectedIndex = 0;

                        // Link combo selection event to container
                        combo.SelectedIndexChanged += OnChange;
                    }
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

        private void OnChange(object sender, EventArgs e)
        {
            if (_linkedControl == null)
                return;

            if (_linkedControl is DarkListView)
            {
                var listView = (DarkListView)_linkedControl;
                SelectedIndex = listView.SelectedIndices.First();
                listView.Focus(); // Return focus back to list
            }
            else if (_linkedControl is DarkComboBox)
            {
                var combo = (DarkComboBox)_linkedControl;
                SelectedIndex = combo.SelectedIndex;
                combo.Focus(); // Return focus back to combo
            }
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

        protected override void OnParentBackColorChanged(EventArgs e)
        {
            foreach (var tab in TabPages)
                ((TabPage)tab).BackColor = Parent.BackColor;

            base.OnParentBackColorChanged(e);
        }
    }
}
