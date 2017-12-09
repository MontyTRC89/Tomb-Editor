using DarkUI.Controls;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using TombEditor.Geometry;

namespace TombEditor.Controls.ContextMenus
{
    class SolidGeometryContextMenu : BaseContextMenu
    {
        private ToolStripMenuItem _itemPaste;
        private ToolStripMenuItem _itemAddCamera;
        private ToolStripMenuItem _itemAddSink;
        private ToolStripMenuItem _itemAddFlybyCamera;
        private ToolStripMenuItem _itemAddSoundSource;
        private ToolStripMenuItem _itemAddImportedGeometry;
        //private ToolStripMenuItem _itemAddItem;

        public SolidGeometryContextMenu(PanelRendering3D panel3D)
            : base()
        {
            _itemPaste = new ToolStripMenuItem("Paste", global::TombEditor.Properties.Resources.general_clipboard_16, (o, e) =>
            {
                EditorActions.PasteObject(panel3D.LastSelectedBlock);
            });

            /*_itemItem = new ToolStripMenuItem("Add camera", global::TombEditor.Properties.Resources.objects_Camera_16, (o, e) =>
            {
                EditorActions.PlaceObject(_editor.SelectedRoom, panel3D.LastSelectedBlock, ItemInstance.FromItemType(_editor.Action.ItemType));
            });*/

            _itemAddCamera = new ToolStripMenuItem("Add camera", global::TombEditor.Properties.Resources.objects_Camera_16, (o, e) =>
            {
                EditorActions.PlaceObject(_editor.SelectedRoom, panel3D.LastSelectedBlock, new CameraInstance());
            });

            _itemAddFlybyCamera = new ToolStripMenuItem("Add fly-by camera", global::TombEditor.Properties.Resources.objects_movie_projector_16, (o, e) =>
            {
                EditorActions.PlaceObject(_editor.SelectedRoom, panel3D.LastSelectedBlock, new FlybyCameraInstance());
            });

            _itemAddSink = new ToolStripMenuItem("Add sink", global::TombEditor.Properties.Resources.objects_tornado_16, (o, e) =>
            {
                EditorActions.PlaceObject(_editor.SelectedRoom, panel3D.LastSelectedBlock, new SinkInstance());
            });

            _itemAddSoundSource = new ToolStripMenuItem("Add sound source", global::TombEditor.Properties.Resources.objects_speaker_16, (o, e) =>
            {
                EditorActions.PlaceObject(_editor.SelectedRoom, panel3D.LastSelectedBlock, new SoundSourceInstance());
            });

            _itemAddImportedGeometry = new ToolStripMenuItem("Add imported geometry", global::TombEditor.Properties.Resources.objects_custom_geometry, (o, e) =>
            {
                EditorActions.PlaceObject(_editor.SelectedRoom, panel3D.LastSelectedBlock, new ImportedGeometryInstance());
            });
        }

        public override void OpenMenu(Control c, Point p)
        {
            Items.Clear();

            if (Clipboard.HasObjectToPaste)
            {
                Items.Add(_itemPaste);
                Items.Add(new ToolStripSeparator());
            }

            Items.Add(_itemAddCamera);
            Items.Add(_itemAddFlybyCamera);
            Items.Add(_itemAddSink);
            Items.Add(_itemAddSoundSource);
            Items.Add(_itemAddImportedGeometry);

            Show(c, p);
        }
    }
}
