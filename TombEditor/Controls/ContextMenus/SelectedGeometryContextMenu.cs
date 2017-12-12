using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace TombEditor.Controls.ContextMenus
{
    class SelectedGeometryContextMenu : BaseContextMenu
    {
        private ToolStripItem _itemAddTrigger;
        private ToolStripItem _itemAddPortal;
        private ToolStripItem _itemCropRoom;
        private ToolStripItem _itemSplitRoom;

        public SelectedGeometryContextMenu(PanelRendering3D panel3D)
            : base()
        {
            _itemAddTrigger = new ToolStripMenuItem("Add trigger", null, (o, e) =>
            {
                EditorActions.AddTrigger(_editor.SelectedRoom, _editor.SelectedSectors.Area, panel3D);
            });

            _itemAddPortal = new ToolStripMenuItem("Add portal", null, (o, e) =>
            {
                EditorActions.AddPortal(_editor.SelectedRoom, _editor.SelectedSectors.Area, panel3D);
            });

            _itemCropRoom = new ToolStripMenuItem("Crop room", global::TombEditor.Properties.Resources.general_crop_16, (o, e) =>
            {
                EditorActions.CropRoom(_editor.SelectedRoom, _editor.SelectedSectors.Area, panel3D);
            });

            _itemSplitRoom = new ToolStripMenuItem("Split room", global::TombEditor.Properties.Resources.general_split_files_16, (o, e) =>
            {
                EditorActions.SplitRoom(panel3D);
            });
        }

        public override void OpenMenu(Control c, Point p)
        {
            Items.Clear();

            Items.Add(_itemAddTrigger);
            Items.Add(_itemAddPortal);
            Items.Add(new ToolStripSeparator());
            Items.Add(_itemCropRoom);
            Items.Add(_itemSplitRoom);

            Show(c, p);
        }
    }
}
