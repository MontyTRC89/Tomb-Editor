﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using DarkUI.Config;
using TombLib.Utils;
using TombLib.LevelData;
using DarkUI.Controls;
using TombLib.Wad;
using System.Linq;

namespace TombLib.Forms
{
    public partial class PopUpSearch : PopUpWindow
    {
        private readonly Color _correctColor;
        private readonly Color _wrongColor;

        private List<string> _searchItems = new List<string>();
        private int _currentIndex;
        private TRVersion.Game _version;

        private Control _callbackControl;

        public bool ShowAboveControl { get; set; } = false;

        public PopUpSearch(Control callbackControl, TRVersion.Game version = TRVersion.Game.TR4) : base(new Point(), true)
        {
            InitializeComponent();

            // Remember colors
            _correctColor = txtSearchString.BackColor;
            _wrongColor = _correctColor.MixWith(Color.DarkRed, 0.55);

            // Set up parameters
            _currentIndex = -1;
            _searchItems.Clear();
            _callbackControl = callbackControl;
            _version = version;

            // TODO: Support other control types?
            if (_callbackControl is DarkComboBox)
            {
                var callbackCombo = (DarkComboBox)_callbackControl;
                foreach (var item in callbackCombo.Items)
                {
                    if (item is IWadObject)
                        _searchItems.Add((item as IWadObject).ToString(_version));
                    else
                        _searchItems.Add(item.ToString());
                }
            }
            else if (_callbackControl is DarkTreeView)
            {
                var callbackTree = (DarkTreeView)_callbackControl;
                _searchItems = callbackTree.GetAllNodes().Select(n => n.Text).ToList();
            }
            else if (_callbackControl is DarkListView)
            {
                var callbackList = (DarkListView)_callbackControl;
                _searchItems = callbackList.Items.Select(n => n.Text).ToList();
            }
            else if (_callbackControl is DarkDataGridView)
            {
                var callbackDataGrid = (DarkDataGridView)_callbackControl;
                _searchItems = callbackDataGrid.Rows.Cast<DataGridViewRow>().Select(n => string.Join(' ', n.Cells.Cast<DataGridViewCell>().Select(c => c.Value))).ToList();
            }

            // Set pop-up width to parent control width
            Size = new Size(_callbackControl.Size.Width, MinimumSize.Height);

            // Set backcolor
            BackColor = Colors.DarkBackground;

            // Start intro animation
            _animTimer.Tick += UpdateTimer;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
                components.Dispose();

            _animTimer.Tick -= UpdateTimer;
            _animTimer.Dispose();

            base.Dispose(disposing);
        }

        private Point GetCallbackControlLocation(Control callbackControl)
        {
            var callbackControlLocation = callbackControl.PointToScreen(Point.Empty);

            if (ShowAboveControl)
                return new Point(callbackControlLocation.X, callbackControlLocation.Y - Height);
            else
                return new Point(callbackControlLocation.X, callbackControlLocation.Y + callbackControl.Size.Height);
        }

        private void UpdateTimer(object sender, EventArgs e)
        {
            _descend = !ShowAboveControl;
            _initialPosition = GetCallbackControlLocation(_callbackControl);

            if (_animProgress >= 0.0f)
                return;

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

        private void FormPopUpSearch_KeyDown(object sender, KeyEventArgs e)
        {
            if (_animTimer.Enabled)
                return;

            if (e.KeyCode == Keys.Escape) // Quit
            {
                Close();
                return;
            }

            if (e.KeyCode != Keys.Enter)
                return;

            var exactMatch = _searchItems.Any(s => s.IndexOf(txtSearchString.Text, StringComparison.OrdinalIgnoreCase) != -1);

            if (_searchItems.Count == 1)
                _currentIndex = 0; // Point to only one existing entry
            else
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
                            
                    int startIndex;
                    int levenshtein = Levenshtein.DistanceSubstring(_searchItems[i].ToLower(), txtSearchString.Text.ToLower(), out startIndex);
                    var match = _searchItems[i].IndexOf(txtSearchString.Text, StringComparison.OrdinalIgnoreCase) != -1;

                    if ((exactMatch && match) || (!exactMatch && levenshtein < 2))
                    {
                        _currentIndex = i;
                        break;
                    }
                }

            if (_currentIndex != -1)
            {
                // TODO: Support other control types?
                if (_callbackControl is DarkComboBox)
                {
                    ((DarkComboBox)_callbackControl).SelectedIndex = _currentIndex;
                }
                else if (_callbackControl is DarkTreeView)
                {
                    var tree = (DarkTreeView)_callbackControl;
                    tree.SelectNode(tree.GetAllNodes()[_currentIndex]);
                    tree.EnsureVisible();
                }
                else if (_callbackControl is DarkListView)
                {
                    var list = (DarkListView)_callbackControl;
                    list.SelectItem(_currentIndex);
                    list.EnsureVisible();
                }
                else if (_callbackControl is DarkDataGridView)
                {
                    var dataGrid = (DarkDataGridView)_callbackControl;
                    dataGrid.ClearSelection();
                    dataGrid.Rows[_currentIndex].Selected = true;
                }
				return;
            }

            // Indicate failed search
            _animProgress = -0.1f;
            _animTimer.Start();
        }

        protected override void OnDeactivate(EventArgs e)
        {
            base.OnDeactivate(e);
            Close();
        }

        private void txtSearchString_TextChanged(object sender, EventArgs e)
        {
            // Search conditions changed, restart search from beginning
            _currentIndex = -1;
        }
    }
}
