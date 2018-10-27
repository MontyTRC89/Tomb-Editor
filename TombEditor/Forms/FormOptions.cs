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

            // Calculate the sizes at runtime since they actually depend on the choosen layout.
            // https://stackoverflow.com/questions/1808243/how-does-one-calculate-the-minimum-client-size-of-a-net-windows-form
            MinimumSize = new Size(793, 533) + (Size - ClientSize);
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
                cbRendering3DFont.Items.Add(font.Name);
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
                if (!string.IsNullOrEmpty(control.Tag?.ToString()))
                {
                    var name = control.Tag.ToString();

                    if (!options.Contains(name))
                        continue;

                    var option = _editor.Configuration.GetType().GetProperty(name)?.GetValue(_editor.Configuration);
                    if (option == null)
                        continue;

                    if (control is DarkCheckBox && option is bool)
                        ((DarkCheckBox)control).Checked = (bool)option;
                    else if (control is DarkTextBox && option is string)
                        ((DarkTextBox)control).Text = (string)option;
                    else if (control is DarkComboBox && option is string)
                        ((DarkComboBox)control).SelectedItem = (string)option;
                    else if (control is DarkNumericUpDown)
                    {
                        if(option is float)
                            ((DarkNumericUpDown)control).Value = (decimal)(float)option;
                        else if(option is int)
                            ((DarkNumericUpDown)control).Value = (decimal)(int)option;
                    }
                    else if (control is Panel && option is ColorScheme)
                    {
                        var color = _editor.Configuration.UI_ColorScheme.GetType().GetProperty(name).GetValue(_editor.Configuration.UI_ColorScheme);
                        if (color is Vector4)
                            ((Panel)control).BackColor = ((Vector4)option).ToWinFormsColor();
                    }
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
                if (!string.IsNullOrEmpty(control.Tag?.ToString()))
                {
                    var name = control.Tag.ToString();
                    var option = _editor.Configuration.GetType().GetProperty(name)?.GetValue(_editor.Configuration);
                    if (option == null)
                        continue;

                    if (control is DarkCheckBox && option is bool)
                        option = ((DarkCheckBox)control).Checked;
                    else if (control is DarkTextBox && option is string)
                        option = ((DarkCheckBox)control).Text;
                    else if (control is DarkComboBox && option is string)
                        option = ((DarkComboBox)control).SelectedItem.ToString();
                    else if (control is DarkNumericUpDown)
                    {
                        if (option is int)
                            option = (int)((DarkNumericUpDown)control).Value;
                        else if (option is float)
                        option = (float)((DarkNumericUpDown)control).Value;
                    }
                    else if (control is Panel && option is ColorScheme)
                    {
                        var color = _editor.Configuration.GetType().GetProperty("ColorScheme").GetType().GetProperty(name).GetValue(_editor.Configuration.UI_ColorScheme);

                        if(color != null && color is Vector4)
                        {
                            var saveAlpha = ((Vector4)color).W;
                            var panelColor = ((Panel)control).BackColor;
                            var newColor = new Vector4((float)(panelColor.R / 255.0), (float)(panelColor.G / 255.0), (float)(panelColor.B / 255.0), saveAlpha);

                            _editor.Configuration.UI_ColorScheme.GetType().GetProperty(name).SetValue(_editor.Configuration.UI_ColorScheme, newColor);
                        }
                    }
                }
            }
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
