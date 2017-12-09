using DarkUI.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace TombEditor.Controls.ContextMenus
{
    class MoveableOrStaticContextMenu : DarkContextMenu
    {
        private ToolStripMenuItem _itemProperties;
        private ToolStripMenuItem _itemCopy;
        private ToolStripMenuItem _itemClone;
        private ToolStripMenuItem _itemDelete;
        private ToolStripMenuItem _itemSnapToGrid;

        private Editor _editor;

        public MoveableOrStaticContextMenu(PanelRendering3D panel3D)
        {
            _editor = Editor.Instance;

            _itemProperties = new ToolStripMenuItem("Properties", null, (o, e) =>
            {
              EditorActions.EditObject(_editor.SelectedObject, panel3D);
            });
            Items.Add(_itemProperties);

            Items.Add(new ToolStripSeparator());

            _itemCopy = new ToolStripMenuItem("Copy", global::TombEditor.Properties.Resources.general_copy_link_16, (o, e) =>
            {
                EditorActions.Copy(panel3D);
            });
            Items.Add(_itemCopy);

            _itemClone = new ToolStripMenuItem("Clone", global::TombEditor.Properties.Resources.actions_rubber_stamp_16, (o, e) =>
            {
                EditorActions.Clone(panel3D);
            });
            Items.Add(_itemClone);

            _itemDelete = new ToolStripMenuItem("Delete", global::TombEditor.Properties.Resources.toolbox_Eraser_16, (o, e) =>
            {
                EditorActions.DeleteObjectWithWarning(_editor.SelectedObject, panel3D);
            });
            Items.Add(_itemDelete);

            //Items.Add(new ToolStripMenuItem("-"));

            /*_itemSnapToGrid = new ToolStripMenuItem("Copy", global::TombEditor.Properties.Resources.actions_center_direction_16, (o, e) =>
            {
                EditorActions.SnapToGrid(_editor.SelectedObject, panel3D);
            });
            Items.Add(_itemSnapToGrid);*/

        }
    }
}
