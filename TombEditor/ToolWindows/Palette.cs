using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using DarkUI.Docking;
using TombEditor.Geometry;

namespace TombEditor.ToolWindows
{
    public partial class Palette : DarkToolWindow
    {
        private Editor _editor;

        public Palette()
        {
            InitializeComponent();

            _editor = Editor.Instance;
            _editor.EditorEventRaised += EditorEventRaised;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
                _editor.EditorEventRaised -= EditorEventRaised;
            if (disposing && (components != null))
                components.Dispose();
            base.Dispose(disposing);
        }

        public void Initialize()
        {
            // Update palette
            lightPalette.SelectedColorChanged += delegate
            {
                Light light = _editor.SelectedObject as Light;
                if (light == null)
                    return;
                light.Color = lightPalette.SelectedColor.ToFloatColor3();
                _editor.SelectedRoom.UpdateCompletely();
                _editor.ObjectChange(light);
            };
        }

        private void EditorEventRaised(IEditorEvent obj)
        {

        }

    }
}
