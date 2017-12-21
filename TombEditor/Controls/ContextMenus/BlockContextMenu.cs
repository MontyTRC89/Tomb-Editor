using DarkUI.Controls;
using SharpDX;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using TombLib.LevelData;
using TombLib.Utils;

namespace TombEditor.Controls.ContextMenus
{
    class BlockContextMenu : BaseContextMenu
    {
        private ToolStripMenuItem _itemPaste;
        public BlockContextMenu(Editor editor, Room targetRoom, DrawingPoint targetBlock)
            : base(editor)
        {
            Items.Add(_itemPaste = new ToolStripMenuItem("Paste", global::TombEditor.Properties.Resources.general_clipboard_16, (o, e) =>
            {
                EditorActions.PasteObject(targetBlock);
            }));
            Items.Add(new ToolStripSeparator());

            Items.Add(new ToolStripMenuItem("Move Lara", null, (o, e) =>
            {
                EditorActions.MoveLara(this, targetBlock);
            }));
            Items.Add(new ToolStripSeparator());

            /*Items.Add(new ToolStripMenuItem("Add camera", global::TombEditor.Properties.Resources.objects_Camera_16, (o, e) =>
            {
                EditorActions.PlaceObject(targetRoom, targetBlock, ItemInstance.FromItemType(_editor.Action.ItemType));
            }));*/

            Items.Add(new ToolStripMenuItem("Add camera", global::TombEditor.Properties.Resources.objects_Camera_16, (o, e) =>
            {
                EditorActions.PlaceObject(targetRoom, targetBlock, new CameraInstance());
            }));

            Items.Add(new ToolStripMenuItem("Add fly-by camera", global::TombEditor.Properties.Resources.objects_movie_projector_16, (o, e) =>
            {
                EditorActions.PlaceObject(targetRoom, targetBlock, new FlybyCameraInstance());
            }));

            Items.Add(new ToolStripMenuItem("Add sink", global::TombEditor.Properties.Resources.objects_tornado_16, (o, e) =>
            {
                EditorActions.PlaceObject(targetRoom, targetBlock, new SinkInstance());
            }));

            Items.Add(new ToolStripMenuItem("Add sound source", global::TombEditor.Properties.Resources.objects_speaker_16, (o, e) =>
            {
                EditorActions.PlaceObject(targetRoom, targetBlock, new SoundSourceInstance());
            }));

            Items.Add(new ToolStripMenuItem("Add imported geometry", global::TombEditor.Properties.Resources.objects_custom_geometry, (o, e) =>
            {
                EditorActions.PlaceObject(targetRoom, targetBlock, new ImportedGeometryInstance());
            }));

            ClipboardEvents.ClipboardChanged += ClipboardEvents_ClipboardChanged;
            ClipboardEvents_ClipboardChanged(this, EventArgs.Empty);
        }

        protected override void Dispose(bool disposing)
        {
            ClipboardEvents.ClipboardChanged -= ClipboardEvents_ClipboardChanged;
            base.Dispose(disposing);
        }

        private void ClipboardEvents_ClipboardChanged(object sender, EventArgs e)
        {
            _itemPaste.Enabled = Clipboard.ContainsData(typeof(ObjectClipboardData).FullName);
        }
    }
}
