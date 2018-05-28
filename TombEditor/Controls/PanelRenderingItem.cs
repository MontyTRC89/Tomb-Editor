using SharpDX.Toolkit.Graphics;
using System;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Numerics;
using System.Windows.Forms;
using TombLib.Controls;
using TombLib.Graphics;
using TombLib.LevelData;
using TombLib.Utils;
using TombLib.Wad;

namespace TombEditor.Controls
{
    public class PanelRenderingItem : PanelItemPreview
    {
        private readonly Editor _editor;

        public PanelRenderingItem()
        {
            if (LicenseManager.UsageMode == LicenseUsageMode.Runtime)
            {
                _editor = Editor.Instance;
                _editor.EditorEventRaised += EditorEventRaised;
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
                _editor.EditorEventRaised -= EditorEventRaised;
            base.Dispose(disposing);
        }

        private void EditorEventRaised(IEditorEvent obj)
        {
            // Update field of view
            if (obj is Editor.ConfigurationChangedEvent)
            {
                Camera.FieldOfView = ((Editor.ConfigurationChangedEvent)obj).Current.RenderingItem_FieldOfView * (float)(Math.PI / 180);
                Invalidate();
            }

            // Update currently viewed item
            if (obj is Editor.ChosenItemChangedEvent)
            {
                Editor.ChosenItemChangedEvent e = (Editor.ChosenItemChangedEvent)obj;
                if (e.Current != null)
                    ResetCamera();
                Invalidate();
                Update(); // Magic fix for room view leaking into item view
            }

            if (obj is Editor.LoadedWadsChangedEvent)
                Invalidate();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            LevelSettings settings = _editor?.Level?.Settings;
            if (settings == null)
                return;
            if (settings.Wads.All(wad => wad.LoadException != null))
            {
                ReferencedWad errorWad = settings.Wads.FirstOrDefault(wad => wad.LoadException != null);
                string notifyMessage;
                if (errorWad == null)
                    notifyMessage = "Click here to load a new WAD file.";
                else
                {
                    string filePath = settings.MakeAbsolute(errorWad.Path);
                    string fileName = FileSystemUtils.GetFileNameWithoutExtensionTry(filePath) ?? "";
                    if (FileSystemUtils.IsFileNotFoundException(errorWad.LoadException))
                        notifyMessage = "Wad file '" + fileName + "' was not found!\n";
                    else
                        notifyMessage = "Unable to load wad from file '" + fileName + "'.\n";
                    notifyMessage += "Click here to choose a replacement.\n\n";
                    notifyMessage += "Path: " + (filePath ?? "");
                }

                e.Graphics.Clear(Parent.BackColor);
                e.Graphics.DrawString(notifyMessage, Font, Brushes.DarkGray, ClientRectangle,
                    new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center });
            }
            else
                base.OnPaint(e);
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);

            switch (e.Button)
            {
                case MouseButtons.Left:
                    LevelSettings settings = _editor?.Level?.Settings;
                    if (settings != null && settings.Wads.All(wad => wad.LoadException != null))
                        EditorActions.AddWad(Parent, settings.Wads.FirstOrDefault(wad => wad.LoadException != null));
                    else if (_editor.ChosenItem != null)
                        DoDragDrop(_editor.ChosenItem, DragDropEffects.Copy);
                    break;
            }
        }

        protected override Vector4 ClearColor => _editor.Configuration.RenderingItem_BackgroundColor;
        public override float FieldOfView => _editor.Configuration.RenderingItem_FieldOfView;
        public override float NavigationSpeedMouseWheelZoom => _editor.Configuration.RenderingItem_NavigationSpeedMouseWheelZoom;
        public override float NavigationSpeedMouseZoom => _editor.Configuration.RenderingItem_NavigationSpeedMouseZoom;
        public override float NavigationSpeedMouseTranslate => _editor.Configuration.RenderingItem_NavigationSpeedMouseTranslate;
        public override float NavigationSpeedMouseRotate => _editor.Configuration.RenderingItem_NavigationSpeedMouseRotate;
        public override bool ReadOnly => true;
    }
}
