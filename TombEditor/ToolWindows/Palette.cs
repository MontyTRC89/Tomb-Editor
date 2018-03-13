using DarkUI.Docking;
using System.Numerics;
using TombLib.LevelData;

namespace TombEditor.ToolWindows
{
    public partial class Palette : DarkToolWindow
    {
        private readonly Editor _editor;

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
                Vector4 result = lightPalette.SelectedColor.ToFloatColor();
                light.Color = new Vector3(result.X, result.Y, result.Z) * 2.0f;
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
