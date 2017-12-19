using DarkUI.Controls;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

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
