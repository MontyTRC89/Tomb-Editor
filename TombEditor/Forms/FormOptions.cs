using DarkUI.Controls;
using DarkUI.Forms;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Numerics;
using System.Windows.Forms;
using TombLib.Controls;
using TombLib.Utils;

namespace TombEditor.Forms
{
    public partial class FormOptions : DarkForm
    {
        private readonly Editor _editor;

        public FormOptions(Editor editor)
        {
            InitializeComponent();
            InitializeDialog();

            // Calculate the sizes at runtime since they actually depend on the choosen layout.
            // https://stackoverflow.com/questions/1808243/how-does-one-calculate-the-minimum-client-size-of-a-net-windows-form
            MinimumSize = new Size(630, 380) + (Size - ClientSize);
            MaximumSize = new Size(MinimumSize.Width, 1000);
            Size = MinimumSize;

            _editor = editor;
            _editor.EditorEventRaised += EditorEventRaised;

            ReadConfigIntoControls(this);
        }

        private void EditorEventRaised(IEditorEvent obj)
        {
            if (obj is Editor.ConfigurationChangedEvent)
                ReadConfigIntoControls(this);
        }

        private void InitializeDialog()
        {
            tabbedContainer.LinkedListView = optionsList;

            foreach (var font in FontFamily.Families)
                try { var ff = new FontFamily(font.Name);
                      cmbRendering3DFont.Items.Add(font.Name); }
                catch { throw; }

            for (int i = 5; i <= 8; i++)
                cmbSelectionTileSize.Items.Add((float)(Math.Pow(2, i)));

            var panels = AllOptionControls(this).Where(c => c is Panel && c.Tag != null).ToList();
            foreach(var panel in panels)
                panel.Click += (sender, e) =>
                {
                    using (var colorDialog = new RealtimeColorDialog(null, _editor.Configuration.UI_ColorScheme))
                    {
                        colorDialog.Color = panel.BackColor;
                        colorDialog.FullOpen = true;
                        if (colorDialog.ShowDialog(this) != DialogResult.OK)
                            return;
                        panel.BackColor = colorDialog.Color;
                    }
                };
        }

        private IEnumerable<Control> AllOptionControls(Control control)
        {
            return WinFormsUtils.AllSubControls(control).Where(c => (c is DarkCheckBox ||
                                                                     c is DarkTextBox ||
                                                                     c is DarkComboBox ||
                                                                     c is DarkNumericUpDown ||
                                                                     c is Panel) && c.Tag != null).ToList();
        }

        private Object GetOptionObject(Control control, Configuration configuration)
        {
            var name = control.Tag?.ToString();
            var option = configuration.GetType().GetProperty(name)?.GetValue(configuration);

            // Try to get sub-option from color scheme
            if (option == null)
            {
                var type = configuration.UI_ColorScheme.GetType();
                var prop = type.GetField(name);

                if (prop != null)
                    option = prop.GetValue(configuration.UI_ColorScheme);
            }

            return option;
        }

        private void SetOptionValue(String name, Object obj, Object value)
        {
            var prop = obj.GetType().GetProperty(name);
            if(prop != null)
                prop.SetValue(obj, value);
            else
            {
                var field = obj.GetType().GetField(name);
                if (field != null)
                    field.SetValue(obj, value);
            }
        }

        private void ReadConfigIntoControls(Control parent, bool resetToDefault = false)
        {
            var controls    = AllOptionControls(parent);
            var configToUse = resetToDefault ? new Configuration() : _editor.Configuration;

            foreach (var control in controls)
            {
                var option = GetOptionObject(control, configToUse);
                if(option != null)
                {
                    if (control is DarkCheckBox && option is bool)
                        ((DarkCheckBox)control).Checked = (bool)option;
                    else if (control is DarkTextBox && option is string)
                        ((DarkTextBox)control).Text = (string)option;
                    else if (control is DarkComboBox)
                    {
                             if (option is string) ((DarkComboBox)control).SelectedItem = (string)option;
                        else if (option is int)    ((DarkComboBox)control).SelectedItem = (int)option;
                        else if (option is float)  ((DarkComboBox)control).SelectedItem = (float)option;
                    }
                    else if (control is DarkNumericUpDown)
                    {
                             if(option is float) ((DarkNumericUpDown)control).Value = (decimal)(float)option;
                        else if(option is int)   ((DarkNumericUpDown)control).Value = (decimal)(int)option;
                    }
                    else if (control is Panel && option is Vector4)
                        ((Panel)control).BackColor = ((Vector4)option).ToWinFormsColor();
                }
            }
        }

        private void WriteConfigFromControls()
        {
            var controls = AllOptionControls(this);
            var config   = _editor.Configuration;

            foreach (var control in controls)
            {
                var option = GetOptionObject(control, config);
                if (option != null)
                {
                    var name = control.Tag.ToString();

                    if (control is DarkCheckBox && option is bool)
                        SetOptionValue(name, config, ((DarkCheckBox)control).Checked);
                    else if (control is DarkTextBox && option is string)
                        SetOptionValue(name, config, ((DarkTextBox)control).Text);
                    else if (control is DarkComboBox)
                    {
                             if (option is string) SetOptionValue(name, config, ((DarkComboBox)control).SelectedItem.ToString());
                        else if (option is int)    SetOptionValue(name, config, (int)((DarkComboBox)control).SelectedItem);
                        else if (option is float)  SetOptionValue(name, config, (float)((DarkComboBox)control).SelectedItem);
                    }
                    else if (control is DarkNumericUpDown)
                    {
                             if (option is int)   SetOptionValue(name, config,   (int)((DarkNumericUpDown)control).Value);
                        else if (option is float) SetOptionValue(name, config, (float)((DarkNumericUpDown)control).Value);
                    }
                    else if (control is Panel && option is Vector4)
                    {
                        var panelColor = ((Panel)control).BackColor;
                        var newColor = new Vector4(panelColor.R, panelColor.G, panelColor.B, 0) / 255.0f;
                        newColor.W = ((Vector4)option).W; // Preserve alpha for now, until alpha color dialog is implemented
                        SetOptionValue(name, config.UI_ColorScheme, newColor);
                    }
                }
            }

            _editor.ConfigurationChange();
        }

        private void butApply_Click(object sender, EventArgs e)
        {
            WriteConfigFromControls();
        }

        private void butOk_Click(object sender, EventArgs e)
        {
            WriteConfigFromControls();
            Close();
        }

        private void butCancel_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void butPageDefaults_Click(object sender, EventArgs e)
        {
            ReadConfigIntoControls(tabbedContainer.SelectedTab, true);
        }
    }
}
