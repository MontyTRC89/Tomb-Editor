using DarkUI.Forms;
using System.Linq;
using TombLib.Graphics;
using TombLib.Wad;

namespace TombEditor.Forms
{
    public partial class FormWadPreview : DarkForm
    {
        private Wad2 _wad;

        public FormWadPreview(Wad2 wad, DeviceManager deviceManager, Editor editor)
        {
            _wad = wad;
            InitializeComponent();

            panelItem.InitializePanel(deviceManager);
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
