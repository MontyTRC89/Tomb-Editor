using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using System.Linq;
using DarkUI.Forms;
using TombLib.Utils;

namespace TombLib.Forms
{
    public partial class PopUpSearch : DarkForm
    {
        private readonly Timer _animTimer = new Timer() { Interval = 10 };
        private float _animProgress = 0.0f;
        private readonly Color _correctColor;
        private readonly Color _wrongColor;

        private List<string> _searchItems = new List<string>();
        private int _currentIndex;

        private Control _callbackControl;

        public PopUpSearch(Control callbackControl, string startText = "")
        {
            InitializeComponent();

            // Remember colors
            _correctColor = txtSearchString.BackColor;
            _wrongColor = _correctColor.MixWith(Color.DarkRed, 0.55);

            // Set up parameters
            _currentIndex = -1;
            _searchItems.Clear();
            _callbackControl = callbackControl;

            // TODO: Support other control types?
            if (_callbackControl is DarkUI.Controls.DarkComboBox)
            {
                DarkUI.Controls.DarkComboBox callbackCombo = (DarkUI.Controls.DarkComboBox)_callbackControl;
                foreach (var item in callbackCombo.Items)
                    _searchItems.Add(item.ToString());
            }

            // Set pop-up width to parent control width
            Size = new Size(_callbackControl.Size.Width, MinimumSize.Height);

            // In case we invoke pop-up from another text control, we can pass existing string here
            if(startText.Length != 0)
            {
                txtSearchString.Text = startText;
                txtSearchString.Select(0, 0);
                txtSearchString.SelectionStart = txtSearchString.Text.Length;
            }

            // Start intro animation
            _animProgress = 0.0f;
            _animTimer.Tick += UpdateTimer;
            _animTimer.Start();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
                components.Dispose();

            _animTimer.Tick -= UpdateTimer;
            _animTimer.Dispose();

            base.Dispose(disposing);
        }

        private void UpdateTimer(object sender, EventArgs e)
        {
            if(_animProgress >= 0.0f) // Intro animation
            {
                _animProgress += 0.1f;
                if (_animProgress > 1.0f) _animProgress = 1.0f;

                // Smoothly descend pop-up window from parent control using sine function
                var callbackControlLocation = _callbackControl.PointToScreen(Point.Empty);
                Location = new Point(callbackControlLocation.X, callbackControlLocation.Y + _callbackControl.Size.Height - MinimumSize.Height + (int)(MinimumSize.Height * Math.Sin(_animProgress * Math.PI / 2)));

                Opacity = _animProgress;

                if (_animProgress == 1.0f)
                {
                    _animProgress = 0.0f;
                    _animTimer.Stop();
                }
            }
            else // Search error animation
            {
                _animProgress -= 0.1f;

                if (_animProgress > -1.0f)
                    txtSearchString.BackColor = _wrongColor;
                else
                {
                    txtSearchString.BackColor = _correctColor;
                    _animProgress = 0.0f;
                    _animTimer.Stop();
                }
            }
        }

        private void FormPopUpSearch_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter) // Search function. TODO: Implement TRTombLevBauer's cool search!
            {
                if (txtSearchString.Text != String.Empty && _searchItems.Count > 1)
                {
                    for (int i = _currentIndex+1; i <= _searchItems.Count; i++)
                    {
                        if (i == _searchItems.Count)
                        {
                            if (_currentIndex == -1)
                                break; // No match
                            else
                            {
                                i = -1;
                                _currentIndex = -1;
                                continue; // Restart search
                            }
                        }

                        if (_searchItems[i].IndexOf(txtSearchString.Text, StringComparison.OrdinalIgnoreCase) != -1)
                        {
                            _currentIndex = i;
                            break;
                        }
                    }

                    if (_currentIndex != -1)
                    {
                        // TODO: Support other control types?
                        if (_callbackControl is DarkUI.Controls.DarkComboBox)
                            ((DarkUI.Controls.DarkComboBox)_callbackControl).SelectedIndex = _currentIndex;
                        return;
                    }
                }

                // Indicate failed search
                _animProgress = -0.1f;
                _animTimer.Start();
            }
            else if (e.KeyCode == Keys.Escape) // Quit
                Close();
        }

        private void FormPopUpSearch_Deactivate(object sender, EventArgs e)
        {
            Close();
        }

        private void txtSearchString_TextChanged(object sender, EventArgs e)
        {
            // Search conditions changed, restart search from beginning
            _currentIndex = -1;
        }
    }
}
