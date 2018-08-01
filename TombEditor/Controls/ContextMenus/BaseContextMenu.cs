using DarkUI.Controls;
using System.Windows.Forms;

namespace TombEditor.Controls.ContextMenus
{
    public abstract class BaseContextMenu : DarkContextMenu
    {
        protected readonly Editor _editor;
        protected readonly IWin32Window _owner;

        public BaseContextMenu(Editor editor, IWin32Window owner)
        {
            _editor = editor;
            _owner = owner;
        }
    }
}
