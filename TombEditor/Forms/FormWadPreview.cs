using DarkUI.Forms;
using System.Linq;
using TombLib.Graphics;
using TombLib.Rendering;
using TombLib.Wad;

namespace TombEditor.Forms
{
    public partial class FormWadPreview : DarkForm
    {
        private Wad2 _wad;

        public FormWadPreview(Wad2 wad, RenderingDevice device, Editor editor)
        {
            _wad = wad;
            InitializeComponent();

            panelItem.InitializeRendering(device);
            wadTree.Wad = wad;
            wadTree.MultiSelect = false;
            wadTree.SelectFirst();
        }

        private void wadTree_SelectedWadObjectIdsChanged(object sender, System.EventArgs e)
        {
            IWadObjectId selectedObjectId = wadTree.SelectedWadObjectIds.FirstOrDefault();
            panelItem.CurrentObject = selectedObjectId == null ? null : _wad.TryGet(selectedObjectId);
        }

        private void FormWadPreview_Deactivate(object sender, System.EventArgs e)
        {
            Close();
        }
    }
}
