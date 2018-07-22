using System.Numerics;
using System.Windows.Forms;
using TombLib;
using TombLib.Forms;

namespace TombEditor.Controls.ContextMenus
{
    class Space2DMapContextMenu : BaseContextMenu
    {
        public Space2DMapContextMenu(Editor editor, IWin32Window owner, Vector2 position)
            : base(editor, owner)
        {
            Items.Add(new ToolStripMenuItem("Paste rooms", Properties.Resources.general_clipboard_16, (o, e) =>
            {
                var roomClipboardData = Clipboard.GetDataObject().GetData(typeof(RoomClipboardData)) as RoomClipboardData;
                if (roomClipboardData == null)
                    _editor.SendMessage("Clipboard contains no room data.", PopupType.Error);
                else
                    roomClipboardData.MergeInto(_editor, VectorInt2.FromRounded(position - roomClipboardData.DropPosition));
            })
            { Enabled = Clipboard.ContainsData(typeof(RoomClipboardData).FullName) });
        }
    }
}
