using DarkUI.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WadTool.Controls.ContextMenus
{
    public abstract class BaseContextMenu : DarkContextMenu
    {
        protected WadToolClass _tool;

        public BaseContextMenu(WadToolClass tool)
        {
            _tool = tool;
        }
    }
}
