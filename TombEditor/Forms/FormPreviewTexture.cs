using DarkUI.Forms;
using System.Linq;
using TombLib.LevelData;
using TombLib.Rendering;
using TombLib.Wad;
using System.Windows.Forms;

namespace TombEditor.Forms
{
    public partial class FormPreviewTexture : DarkForm
    {
        public FormPreviewTexture(LevelTexture texture, Editor editor)
        {
            InitializeComponent();
            panelTextureMapForPreview.VisibleTexture = texture;
        }

        private void FormWadPreview_Deactivate(object sender, System.EventArgs e)
        {
            Close();
        }

        public class PanelTextureMapForPreview : Controls.PanelTextureMap
        {
            protected override void OnPaintSelection(PaintEventArgs e)
            {
                // Don't paint a selection
            }
        }
    }
}
