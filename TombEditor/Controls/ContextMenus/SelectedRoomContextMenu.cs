using System.Numerics;
using System.Windows.Forms;
using TombLib;
using TombLib.Forms;
using TombLib.Utils;

namespace TombEditor.Controls.ContextMenus
{
    class SelectedRoomContextMenu : BaseContextMenu
    {
        public SelectedRoomContextMenu(Editor editor, IWin32Window owner, Vector2 position)
            : base(editor, owner)
        {
            Items.Add(new ToolStripMenuItem("Export rooms", Properties.Resources.general_Save_As_16, (o, e) =>
            {
                EditorActions.ExportRooms(_editor.SelectedRooms, this);
            }));

            Items.Add(new ToolStripSeparator());

            Items.Add(new ToolStripMenuItem("Delete rooms", Properties.Resources.general_trash_16, (o, e) =>
            {
                EditorActions.DeleteRooms(_editor.SelectedRooms, this);
            }));

            Items.Add(new ToolStripMenuItem("Copy rooms", Properties.Resources.general_copy_16, (o, e) =>
            {
                Clipboard.SetDataObject(new RoomClipboardData(_editor, position), true);
            }));

            Items.Add(new ToolStripMenuItem("Paste rooms", Properties.Resources.general_clipboard_16, (o, e) =>
            {
                var roomClipboardData = Clipboard.GetDataObject().GetData(typeof(RoomClipboardData)) as RoomClipboardData;
                if (roomClipboardData == null)
                    _editor.SendMessage("Clipboard contains no room data.", PopupType.Error);
                else
                    roomClipboardData.MergeInto(_editor, VectorInt2.FromRounded(position - roomClipboardData.DropPosition));
            })
            { Enabled = Clipboard.ContainsData(typeof(RoomClipboardData).FullName) });

            Items.Add(new ToolStripSeparator());

            Items.Add(new ToolStripMenuItem("Rotate rooms clockwise", null, (o, e) =>
            {
                EditorActions.TransformRooms(new RectTransformation { QuadrantRotation = -1 }, Parent);
            }));

            Items.Add(new ToolStripMenuItem("Rotate rooms counterclockwise", null, (o, e) =>
            {
                EditorActions.TransformRooms(new RectTransformation { QuadrantRotation = 1 }, Parent);
            }));

            Items.Add(new ToolStripMenuItem("Mirror rooms on X axis", null, (o, e) =>
            {
                EditorActions.TransformRooms(new RectTransformation { MirrorX = true }, Parent);
            }));

            Items.Add(new ToolStripMenuItem("Mirror rooms on Z axis", null, (o, e) =>
            {
                EditorActions.TransformRooms(new RectTransformation { MirrorX = true, QuadrantRotation = 2 }, Parent);
            }));

            Items.Add(new ToolStripMenuItem("Merge rooms horizontally", null, (o, e) =>
            {
                EditorActions.MergeRoomsHorizontally(_editor.SelectedRooms, Parent);
            }));
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
        }
    }
}
