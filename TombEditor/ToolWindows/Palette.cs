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
using SharpDX;

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

            // Update palette
            lightPalette.SelectedColorChanged += delegate
            {
                LightInstance light = _editor.SelectedObject as LightInstance;
                if (light == null)
                    return;
                light.Color = (Vector3)lightPalette.SelectedColor.ToFloatColor() * 2.0f;
                _editor.SelectedRoom.UpdateCompletely();
                _editor.ObjectChange(light, ObjectChangeType.Change);
            };
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
                _editor.EditorEventRaised -= EditorEventRaised;
            if (disposing && (components != null))
                components.Dispose();
            base.Dispose(disposing);
        }

        private void EditorEventRaised(IEditorEvent obj)
        {

        }

    }
}
