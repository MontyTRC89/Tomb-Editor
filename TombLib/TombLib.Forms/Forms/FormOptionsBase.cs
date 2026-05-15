using DarkUI.Config;
using DarkUI.Controls;
using DarkUI.Forms;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Numerics;
using System.Windows.Forms;
using TombLib.Controls;
using TombLib.Graphics;
using TombLib.LevelData;
using TombLib.Utils;

namespace TombLib.Forms
{
    public partial class FormOptionsBase : DarkForm
    {
        private readonly ConfigurationBase _currentConfig;

        public List<string> FieldsToSearch { get; private set; } = null;

        public FormOptionsBase()
        {
            InitializeComponent();
        }

        public FormOptionsBase(ConfigurationBase currentConfig, List<string> fieldsForSearch = null)
        {
            InitializeComponent();

            if (LicenseManager.UsageMode == LicenseUsageMode.Runtime)
            {
                if (fieldsForSearch != null)
                    FieldsToSearch = fieldsForSearch;
                _currentConfig = currentConfig;
            }
        }

        protected virtual void InitializeDialog()
        {
            // Inject the renderer selector if the config exposes a
            // Rendering_GraphicsApi property. Done before the auto-bind read
            // so the combo starts with the correct value.
            InjectRendererSelector();

            // Link options list
            tabbedContainer.LinkedControl = optionsList;

            // Assign event for every color panel
            var panels = AllOptionControls(this).Where(c => c is DarkPanel).ToList();
            foreach (var panel in panels)
                panel.MouseDown += (sender, e) =>
                {
                    using (var colorDialog = new RealtimeColorDialog())
                    {
                        colorDialog.Color = panel.BackColor;
                        colorDialog.FullOpen = true;
                        if (colorDialog.ShowDialog(this) != DialogResult.OK)
                            return;

                        if (panel.BackColor != colorDialog.Color)
                            panel.BackColor = colorDialog.Color;
                    }
                };

            // Reset color for all tab pages
            foreach (var tab in tabbedContainer.TabPages)
                ((TabPage)tab).BackColor = Colors.GreyBackground;

            ReadConfigIntoControls(this);
        }

        protected virtual IEnumerable<Control> AllOptionControls(Control control) =>
            WinFormsUtils.AllSubControls(control).Where(c => (c is DarkCheckBox ||
                                                              c is DarkTextBox ||
                                                              c is DarkComboBox ||
                                                              c is DarkNumericUpDown ||
                                                              c is DarkPanel) && c.Tag != null).ToList();

        protected virtual Object GetOptionObject(Control control, ConfigurationBase configuration)
        {
            var name = control.Tag?.ToString();
            var option = configuration.GetType().GetProperty(name)?.GetValue(configuration);

            // Try to get sub-option from provided fields
            if (option == null && FieldsToSearch != null) 
                foreach (var field in FieldsToSearch)
                {
                    var obj = configuration.GetType().GetProperty(field)?.GetValue(configuration);
                    if (obj != null)
                    {
                        var val = obj.GetType().GetField(name)?.GetValue(obj);
                        if (val != null)
                        {
                            option = val;
                            break;
                        }
                    }
                }
            return option;
        }

        protected virtual void SetOptionValue(string name, ConfigurationBase configuration, Object value)
        {
            var prop = configuration.GetType().GetProperty(name);
            if (prop != null)
                prop.SetValue(configuration, value);
            else if (FieldsToSearch != null)
            {
                foreach (var field in FieldsToSearch)
                {
                    prop = configuration.GetType().GetProperty(field);
                    if (prop == null) continue;

                    var obj = prop.GetValue(configuration);
                    var fld = obj.GetType().GetField(name);
                    if (fld.GetValue(obj).GetType() == value.GetType())
                    {
                        fld.SetValue(obj, value);
                        break;
                    }
                }
            }
        }

        protected virtual void ReadConfigIntoControls(Control parent, bool resetToDefault = false, Type onlyType = null)
        {
            var configToUse = resetToDefault ? (ConfigurationBase)(Activator.CreateInstance(_currentConfig.GetType())) : _currentConfig;
            var controls = AllOptionControls(parent);

            foreach (var control in controls)
            {
                if (onlyType != null && control.GetType() != onlyType)
                    continue;

                var option = GetOptionObject(control, configToUse);
                if (option != null)
                {
                    if (control is DarkCheckBox && option is bool)
                        ((DarkCheckBox)control).Checked = (bool)option;
                    else if (control is DarkTextBox && option is string)
                        ((DarkTextBox)control).Text = (string)option;
                    else if (control is DarkComboBox)
                    {
                        if (option is string) ((DarkComboBox)control).SelectedItem = (string)option;
                        else if (option is int) ((DarkComboBox)control).SelectedIndex = (int)option;
                        else if (option is float) ((DarkComboBox)control).SelectedItem = (float)option;
                        else if (option is TRVersion.Game) ((DarkComboBox)control).SelectedItem = (TRVersion.Game)option;
                    }
                    else if (control is DarkNumericUpDown)
                    {
                        if (option is float) ((DarkNumericUpDown)control).Value = (decimal)(float)option;
                        else if (option is int) ((DarkNumericUpDown)control).Value = (decimal)(int)option;
                    }
                    else if (control is DarkPanel)
                    {
                        if (option is Vector4) ((DarkPanel)control).BackColor = ((Vector4)option).ToWinFormsColor();
                        else if (option is string) ((DarkPanel)control).BackColor = ColorTranslator.FromHtml((string)option);
                    }
                }
            }
        }

        protected virtual void WriteConfigFromControls()
        {
            var controls = AllOptionControls(this);

            foreach (var control in controls)
            {
                var option = GetOptionObject(control, _currentConfig);
                if (option != null)
                {
                    var name = control.Tag.ToString();

                    if (control is DarkCheckBox && option is bool)
                        SetOptionValue(name, _currentConfig, ((DarkCheckBox)control).Checked);
                    else if (control is DarkTextBox && option is string)
                        SetOptionValue(name, _currentConfig, ((DarkTextBox)control).Text);
                    else if (control is DarkComboBox)
                    {
                        if (option is string) SetOptionValue(name, _currentConfig, ((DarkComboBox)control).SelectedItem.ToString());
                        else if (option is int) SetOptionValue(name, _currentConfig, (int)((DarkComboBox)control).SelectedIndex);
                        else if (option is float) SetOptionValue(name, _currentConfig, (float)((DarkComboBox)control).SelectedItem);
                        else if (option is TRVersion.Game) SetOptionValue(name, _currentConfig, (TRVersion.Game)((DarkComboBox)control).SelectedItem);
                    }
                    else if (control is DarkNumericUpDown)
                    {
                        if (option is int) SetOptionValue(name, _currentConfig, (int)((DarkNumericUpDown)control).Value);
                        else if (option is float) SetOptionValue(name, _currentConfig, (float)((DarkNumericUpDown)control).Value);
                    }
                    else if (control is DarkPanel)
                    {
                        if (option is Vector4)
                        {
                            var newColor = ((DarkPanel)control).BackColor.ToFloat4Color();
                            newColor.W = ((Vector4)option).W; // Preserve alpha for now, until alpha color dialog is implemented
                            SetOptionValue(name, _currentConfig, newColor);
                        }
                        else if (option is string) SetOptionValue(name, _currentConfig, ColorTranslator.ToHtml(((DarkPanel)control).BackColor));
                    }
                }
            }
        }

        // Adds a "Renderer" combo to the first tab whose text matches one of
        // the host apps' rendering tabs ("3D window" in TombEditor, "General"
        // in WadTool). The combo lists only renderers available on the
        // current platform; changing the selection saves into the config
        // immediately (independent of the Apply button — the value won't
        // take effect until the next launch anyway) and shows a "restart
        // required" MessageBox.
        private void InjectRendererSelector()
        {
            if (_currentConfig == null) return;
            var prop = _currentConfig.GetType().GetProperty("Rendering_GraphicsApi");
            if (prop == null) return;

            // Find a suitable tab — prefer "3D window", fall back to "General".
            TabPage target = null;
            foreach (TabPage tab in tabbedContainer.TabPages)
            {
                if (string.Equals(tab.Text, "3D window", StringComparison.OrdinalIgnoreCase))
                { target = tab; break; }
            }
            if (target == null)
                foreach (TabPage tab in tabbedContainer.TabPages)
                {
                    if (string.Equals(tab.Text, "General", StringComparison.OrdinalIgnoreCase))
                    { target = tab; break; }
                }
            if (target == null) return;

            var group = new DarkGroupBox
            {
                Text = "Renderer",
                Dock = DockStyle.Top,
                Height = 56,
            };
            var label = new DarkLabel
            {
                Text = "Graphics API (requires restart):",
                AutoSize = true,
                Location = new Point(6, 24),
                ForeColor = Color.FromArgb(220, 220, 220),
            };
            var combo = new DarkComboBox
            {
                Location = new Point(180, 21),
                Size = new Size(170, 23),
                DropDownStyle = ComboBoxStyle.DropDownList,
            };

            string currentId = (prop.GetValue(_currentConfig) as string) ?? RendererCatalog.DefaultId;
            currentId = RendererCatalog.Resolve(currentId).Id;
            int selectedIdx = 0;
            for (int i = 0; i < RendererCatalog.Available.Count; ++i)
            {
                var entry = RendererCatalog.Available[i];
                combo.Items.Add(entry);
                if (entry.Id == currentId) selectedIdx = i;
            }
            combo.SelectedIndex = selectedIdx;

            string lastId = currentId;
            combo.SelectedIndexChanged += (s, e) =>
            {
                if (combo.SelectedItem is not RendererCatalog.Entry sel) return;
                if (sel.Id == lastId) return;
                lastId = sel.Id;
                prop.SetValue(_currentConfig, sel.Id);
                DarkMessageBox.Show(this,
                    "The selected renderer will be used the next time the application starts.",
                    "Renderer changed",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
            };

            group.Controls.Add(label);
            group.Controls.Add(combo);

            // Find the tab's content panel (Dock=Fill child) so the group
            // docks above existing groups instead of overlapping the header.
            Control container = target;
            foreach (Control c in target.Controls)
                if (c.Dock == DockStyle.Fill) { container = c; break; }
            container.Controls.Add(group);
            group.BringToFront();
        }

        private void butApply_Click(object sender, EventArgs e) => WriteConfigFromControls();
        private void butPageDefaults_Click(object sender, EventArgs e) => ReadConfigIntoControls(tabbedContainer.SelectedTab, true);

        private void butOk_Click(object sender, EventArgs e)
        {
            WriteConfigFromControls();
            Close();
        }
    }
}
