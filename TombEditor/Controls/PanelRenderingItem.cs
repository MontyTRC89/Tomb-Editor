using DarkUI.Config;
using System;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Numerics;
using System.Windows.Forms;
using TombLib.LevelData;
using TombLib.RenderingV2.Preview;
using TombLib.Utils;
using TombLib.Wad;

namespace TombEditor.Controls
{
    /// <summary>
    /// Item-browser preview panel — renderer edition. Wires the panel's
    /// abstract render hooks to the editor configuration and the WAD-loaded
    /// state of the current level.
    /// </summary>
    public class PanelRenderingItem : ItemPreviewPanel
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
            if (disposing && _editor != null)
                _editor.EditorEventRaised -= EditorEventRaised;
            base.Dispose(disposing);
        }

        private void EditorEventRaised(IEditorEvent obj)
        {
            // Update field of view. Guard against transient null Configuration
            // (e.g. during settings reload) and against a Camera that hasn't
            // been initialised yet on the panel base.
            if (obj is Editor.ConfigurationChangedEvent)
            {
                var cfg = _editor?.Configuration;
                if (cfg != null && Camera != null)
                {
                    Camera.FieldOfView = cfg.RenderingItem_FieldOfView * (float)(Math.PI / 180);
                    Invalidate();
                }
            }

            // Update currently viewed item.
            if (obj is Editor.ChosenItemsChangedEvent itemsChanged)
            {
                if (itemsChanged.Current?.Any(o => o is WadMoveable or WadStatic) == true)
                {
                    ResetCamera();
                    Invalidate();
                    Update(); // Magic fix for room view leaking into item view
                }
            }

            if (obj is Editor.LoadedWadsChangedEvent ||
                obj is Editor.EditorFocusedEvent)
                Invalidate();

            if (obj is Editor.LoadedWadsChangedEvent ||
                obj is Editor.LevelChangedEvent)
            {
                // Drop the entire shared preview cache: meshes / atlases that
                // referenced the old WAD textures must be rebuilt against the
                // newly loaded ones.
                PreviewDevice.InvalidateAll();
            }
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
                    notifyMessage = "Click here to load new WAD file.";
                else
                {
                    string filePath = settings.MakeAbsolute(errorWad.Path);
                    string fileName = PathC.GetFileNameWithoutExtensionTry(filePath) ?? "";
                    if (PathC.IsFileNotFoundException(errorWad.LoadException))
                        notifyMessage = "Wad file '" + fileName + "' was not found!\n";
                    else
                        notifyMessage = "Unable to load wad from file '" + fileName + "'.\n";
                    notifyMessage += "Click here to choose a replacement.\n\n";
                    notifyMessage += "Path: " + (filePath ?? "");
                }

                e.Graphics.Clear(Parent.BackColor);
                using (var b = new SolidBrush(Colors.DisabledText))
                    e.Graphics.DrawString(notifyMessage, Font, b, ClientRectangle,
                        new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center });

                ControlPaint.DrawBorder(e.Graphics, ClientRectangle, Colors.GreySelection, ButtonBorderStyle.Solid);
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
                    {
                        ReferencedWad wadToUpdate = settings.Wads.FirstOrDefault(wad => wad.LoadException != null);
                        if (wadToUpdate != null)
                            EditorActions.ReloadResource(Parent, settings, wadToUpdate);
                        else
                            EditorActions.AddWad(Parent);
                    }
                    else
                    {
                        if (CurrentObject != null)
                            DoDragDrop(CurrentObject, DragDropEffects.Copy);
                    }
                    break;
            }
        }

        // Sensible defaults if Configuration isn't ready yet (designer
        // session, very early paint before Editor.Configuration is wired).
        // Each accessor is hit on every paint and on early events so a
        // transient null must not throw.
        protected override Vector4 ClearColor =>
            _editor?.Configuration?.UI_ColorScheme.Color3DBackground ?? new Vector4(0.392f, 0.584f, 0.929f, 1f);
        public override float FieldOfView =>
            _editor?.Configuration?.RenderingItem_FieldOfView ?? 45f;
        public override float NavigationSpeedMouseWheelZoom =>
            _editor?.Configuration?.RenderingItem_NavigationSpeedMouseWheelZoom ?? 1f;
        public override float NavigationSpeedMouseZoom =>
            _editor?.Configuration?.RenderingItem_NavigationSpeedMouseZoom ?? 1f;
        public override float NavigationSpeedMouseTranslate =>
            _editor?.Configuration?.RenderingItem_NavigationSpeedMouseTranslate ?? 1f;
        public override float NavigationSpeedMouseRotate =>
            _editor?.Configuration?.RenderingItem_NavigationSpeedMouseRotate ?? 1f;
    }
}
