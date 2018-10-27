using DarkUI.Controls;
using DarkUI.Forms;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using TombLib.Rendering;
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

            tabbedContainer.LinkedListView = optionsList;

            // Calculate the sizes at runtime since they actually depend on the choosen layout.
            // https://stackoverflow.com/questions/1808243/how-does-one-calculate-the-minimum-client-size-of-a-net-windows-form
            MinimumSize = new Size(630, 380) + (Size - ClientSize);
            MaximumSize = new Size(MinimumSize.Width, 1000);
            Size = MinimumSize;

            _editor = editor;
            _editor.EditorEventRaised += EditorEventRaised;

            ReadConfigIntoControls();
        }

        private void EditorEventRaised(IEditorEvent obj)
        {
            if (obj is Editor.ConfigurationChangedEvent)
                ReadConfigIntoControls();
        }

        private void InitializeDialog()
        {
            foreach (FontFamily font in System.Drawing.FontFamily.Families)
                cmbRendering3DFont.Items.Add(font.Name);

            cmbSelectionTileSize.Items.Add(32.0f);
            cmbSelectionTileSize.Items.Add(64.0f);
            cmbSelectionTileSize.Items.Add(128.0f);
            cmbSelectionTileSize.Items.Add(256.0f);

            var panels = WinFormsUtils.AllSubControls(this).Where(c => c is Panel && !String.IsNullOrEmpty(((Panel)c).Tag?.ToString())).ToList();
            foreach(var panel in panels)
            {
                panel.Click += (sender, e) =>
                {
                    using (var colorDialog = new System.Windows.Forms.ColorDialog())
                    {
                        colorDialog.Color = panel.BackColor;
                        colorDialog.FullOpen = true;
                        if (colorDialog.ShowDialog(this) != DialogResult.OK)
                            return;
                        panel.BackColor = colorDialog.Color;
                    }
                };
            }
        }

        private Object GetOptionObjectFromControl(Control control)
        {
            if (control.Tag == null || string.IsNullOrEmpty(control.Tag.ToString()))
                return null;

            var name = control.Tag.ToString();
            var option = _editor.Configuration.GetType().GetProperty(name)?.GetValue(_editor.Configuration);
            // Try to get sub-option from color scheme
            if (option == null)
            {
                var type = _editor.Configuration.UI_ColorScheme.GetType();
                var prop = type.GetField(name);

                if (prop != null)
                    option = prop.GetValue(_editor.Configuration.UI_ColorScheme);
            }

            return option;
        }

        private void ReadConfigIntoControls()
        {
            var options = _editor.Configuration.GetType().GetProperties().Select(p => p.Name).ToList();

            var controls = WinFormsUtils.AllSubControls(this).Where(c => c is DarkCheckBox ||
                                                                         c is DarkTextBox ||
                                                                         c is DarkComboBox ||
                                                                         c is DarkNumericUpDown ||
                                                                         c is Panel).ToList();
            foreach (var control in controls)
            {
                var option = GetOptionObjectFromControl(control);
                if(option != null)
                {
                    if (control is DarkCheckBox && option is bool)
                        ((DarkCheckBox)control).Checked = (bool)option;
                    else if (control is DarkTextBox && option is string)
                        ((DarkTextBox)control).Text = (string)option;
                    else if (control is DarkComboBox && option is string)
                        ((DarkComboBox)control).SelectedItem = (string)option;
                    else if (control is DarkComboBox && option is int)
                        ((DarkComboBox)control).SelectedItem = (int)option;
                    else if (control is DarkComboBox && option is float)
                        ((DarkComboBox)control).SelectedItem = (float)option;
                    else if (control is DarkNumericUpDown)
                    {
                        if(option is float)
                            ((DarkNumericUpDown)control).Value = (decimal)(float)option;
                        else if(option is int)
                            ((DarkNumericUpDown)control).Value = (decimal)(int)option;
                    }
                    else if (control is Panel && option is Vector4)
                        ((Panel)control).BackColor = ((Vector4)option).ToWinFormsColor();
                }
            }
        }

        private void WriteConfigFromControls()
        {
            var controls = WinFormsUtils.AllSubControls(this).Where(c => c is DarkCheckBox ||
                                                                         c is DarkTextBox ||
                                                                         c is DarkComboBox ||
                                                                         c is DarkNumericUpDown ||
                                                                         c is Panel).ToList();
            foreach (var control in controls)
            {
                var option = GetOptionObjectFromControl(control);
                if (option != null)
                {
                    var name = control.Tag.ToString();

                    if (control is DarkCheckBox && option is bool)
                        _editor.Configuration.GetType().GetProperty(name).SetValue(_editor.Configuration, ((DarkCheckBox)control).Checked);
                    else if (control is DarkTextBox && option is string)
                        _editor.Configuration.GetType().GetProperty(name).SetValue(_editor.Configuration, ((DarkTextBox)control).Text);
                    else if (control is DarkComboBox && option is string)
                        _editor.Configuration.GetType().GetProperty(name).SetValue(_editor.Configuration, ((DarkComboBox)control).SelectedItem.ToString());
                    else if (control is DarkComboBox && option is int)
                        _editor.Configuration.GetType().GetProperty(name).SetValue(_editor.Configuration, (int)((DarkComboBox)control).SelectedItem);
                    else if (control is DarkComboBox && option is float)
                        _editor.Configuration.GetType().GetProperty(name).SetValue(_editor.Configuration, (float)((DarkComboBox)control).SelectedItem);
                    else if (control is DarkNumericUpDown)
                    {
                        if (option is int)
                            _editor.Configuration.GetType().GetProperty(name).SetValue(_editor.Configuration, (int)((DarkNumericUpDown)control).Value);
                        else if (option is float)
                            _editor.Configuration.GetType().GetProperty(name).SetValue(_editor.Configuration, (float)((DarkNumericUpDown)control).Value);
                    }
                    else if (control is Panel && option is Vector4)
                    {
                        var saveAlpha = ((Vector4)option).W;
                        var panelColor = ((Panel)control).BackColor;
                        var newColor = new Vector4((float)(panelColor.R / 255.0), (float)(panelColor.G / 255.0), (float)(panelColor.B / 255.0), saveAlpha);

                        _editor.Configuration.UI_ColorScheme.GetType().GetField(name).SetValue(_editor.Configuration.UI_ColorScheme, newColor);
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
    }
}
