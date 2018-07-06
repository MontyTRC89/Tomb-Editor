using DarkUI.Controls;

namespace TombEditor.Controls.ContextMenus
{
    public abstract class BaseContextMenu : DarkContextMenu
    {
        protected Editor _editor;

        public BaseContextMenu(Editor editor)
        {
            _editor = editor;
        }
    }
}
