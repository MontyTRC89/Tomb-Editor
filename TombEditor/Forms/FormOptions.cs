using DarkUI.Controls;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Numerics;
using TombLib.Rendering;
using TombLib.Forms;
using TombLib.Utils;
using static TombLib.LevelData.TRVersion;
using TombLib.LevelData;

namespace TombEditor.Forms
{
    public partial class FormOptions : FormOptionsBase
    {
        private readonly Editor _editor;
        private bool _lockColorScheme;

        public FormOptions(Editor editor) : base(editor.Configuration, new List<string>() { "UI_ColorScheme" })
        {
            _editor = editor;
            _editor.EditorEventRaised += EditorEventRaised;

            InitializeComponent();
            InitializeDialog();
            this.SetActualSize(630, 570);
            this.LockWidth();
        }

        private void EditorEventRaised(IEditorEvent obj)
        {
            if (obj is Editor.ConfigurationChangedEvent)
                ReadConfigIntoControls(this);
        }

        protected override void InitializeDialog()
        {
            // Filter out non-TrueType fonts by catching an exception on font creation.
            foreach (var font in FontFamily.Families)
            {
                try
                {
                    var testFont = new FontFamily(font.Name);
                    cmbRendering3DFont.Items.Add(font.Name);
                }
                catch
                {
                    // Absorb exception for non-TrueType fonts.
                }
            }

            // Populate versions
            cmbGameVersion.Items.AddRange(AllVersions.Cast<object>().ToArray());

            // Populate events
            cmbVolumeEvent.Items.AddRange(Event.VolumeEventTypes.Select(e => e.ToString().SplitCamelcase()).ToArray());
            cmbGlobalEvent.Items.AddRange(Event.GlobalEventTypes.Select(e => e.ToString().SplitCamelcase()).ToArray());

            // Populate color scheme presets
            typeof(ColorScheme)
                .GetFields()
                .Where(f => f.FieldType == typeof(ColorScheme))
                .ToList()
                .ForEach(item => cmbColorScheme.Items.Add(item.Name));

            // Add the rendering-backend selector programmatically. We
            // could put this in FormOptions.Designer.cs but keeping the
            // backend choice in code-behind avoids touching the autogen
            // designer file and lets the option list grow with future
            // backends without a designer round-trip. The combo + label
            // bind to Configuration.Rendering3D_Backend via Tag, picked up
            // by FormOptionsBase.ReadConfigIntoControls.
            int backendY = darkGroupBox4.Height + 4;
            darkGroupBox4.Height += 56;
            var cmbBackend = new DarkUI.Controls.DarkComboBox
            {
                FormattingEnabled = true,
                Location = new Point(232, backendY),
                Size     = new Size(123, 23),
                Tag      = "Rendering3D_Backend",
                DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList,
            };
            cmbBackend.Items.AddRange(new object[] { "Default", "Vulkan", "OpenGL", "DX11" });
            var lblBackend = new DarkUI.Controls.DarkLabel
            {
                AutoSize  = true,
                ForeColor = Color.FromArgb(220, 220, 220),
                Location  = new Point(3, backendY + 3),
                Text      = "Rendering backend:",
            };
            var lblBackendHint = new DarkUI.Controls.DarkLabel
            {
                AutoSize  = true,
                ForeColor = Color.FromArgb(160, 160, 160),
                Location  = new Point(3, backendY + 28),
                Text      = "Default = auto (Vulkan → OpenGL → DX11). Restart the editor for a change to take effect.",
            };
            darkGroupBox4.Controls.Add(cmbBackend);
            darkGroupBox4.Controls.Add(lblBackend);
            darkGroupBox4.Controls.Add(lblBackendHint);

            // Reset color scheme combo if color was changed
            foreach (var panel in AllOptionControls(this).Where(c => c is DarkPanel))
                panel.BackColorChanged += (sender, e) =>
                {
                    if (!_lockColorScheme && panel.Parent.Text == "Color scheme")
                        cmbColorScheme.SelectedIndex = -1;
                };

            base.InitializeDialog();
        }

        protected override void WriteConfigFromControls()
        {
            base.WriteConfigFromControls();
            _editor.ConfigurationChange();
        }

        private void cmbColorScheme_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (cmbColorScheme.SelectedIndex == -1) return;
            var config = _editor.Configuration;

            _lockColorScheme = true;
            foreach (var prop in typeof(ColorScheme).GetFields())
                if (prop.FieldType == typeof(ColorScheme) && prop.Name == cmbColorScheme.SelectedItem.ToString())
                {
                    foreach (var field in typeof(ColorScheme).GetFields().Where(f => f.FieldType == typeof(Vector4)))
                        foreach (var panel in AllOptionControls(groupColorScheme).OfType<DarkPanel>())
                            if (panel.Tag.ToString() == field.Name)
                                panel.BackColor = ((Vector4)field.GetValue(prop.GetValue(config.UI_ColorScheme))).ToWinFormsColor();
                    break;
                }
            _lockColorScheme = false;
        }
    }
}
