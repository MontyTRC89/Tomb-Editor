using System.Numerics;
using System.Windows.Forms;
using TombLib.Utils;

namespace TombEditor.Controls.ContextMenus
{
    class TextureMapContextMenu : BaseContextMenu
    {
        public TextureMapContextMenu(Editor editor, IWin32Window owner, Vector2 position)
            : base(editor, owner)
        {
            if (editor.SelectedTexture != TextureArea.None)
            {
                Items.Add(new ToolStripMenuItem("Set as default texture", null, (o, e) =>
                {
                    editor.Level.Settings.DefaultTexture = editor.SelectedTexture;
                    (owner as Control).Invalidate();
                }));
            }

            if (editor.Level.Settings.DefaultTexture != TextureArea.None)
            {
                Items.Add(new ToolStripMenuItem("Clear default texture", null, (o, e) =>
                {
                    editor.Level.Settings.DefaultTexture = TextureArea.None;
                    (owner as Control).Invalidate();
                }));
            }
        }
    }
}
