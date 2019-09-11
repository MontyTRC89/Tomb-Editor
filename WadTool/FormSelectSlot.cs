using DarkUI.Controls;
using DarkUI.Forms;
using System;
using System.Collections.Generic;
using System.Windows.Forms;
using TombLib.Wad;
using TombLib.Wad.Catalog;

namespace WadTool
{
    public partial class FormSelectSlot : DarkForm
    {
        public Type TypeClass { get; }
        public WadGameVersion GameVersion { get; }
        public IWadObjectId NewId { get; set; }

        public FormSelectSlot(IWadObjectId currentId, WadGameVersion gameVersion)
        {
            InitializeComponent();

            NewId = currentId;
            TypeClass = currentId.GetType();
            GameVersion = gameVersion;

            if (TypeClass == typeof(WadMoveableId))
                chosenId.Value = ((WadMoveableId)currentId).TypeId;
            else if (TypeClass == typeof(WadStaticId))
                chosenId.Value = ((WadStaticId)currentId).TypeId;
            else if (TypeClass == typeof(WadSpriteSequenceId))
                chosenId.Value = ((WadSpriteSequenceId)currentId).TypeId;
            else if (TypeClass == typeof(WadFixedSoundInfoId))
                chosenId.Value = ((WadFixedSoundInfoId)currentId).TypeId;
            else if (TypeClass == typeof(WadAdditionalSoundInfoId))
            {
                chosenId.Visible = false;
                chosenIdText.Visible = true;
                lstSlots.Enabled = false;
                tbSearchLabel.Enabled = false;
                tbSearch.Enabled = false;

                chosenIdText.Text = ((WadAdditionalSoundInfoId)currentId).Name;
            }
            else
                throw new NotImplementedException("The " + TypeClass + " is not implemented yet.");

            ReloadSlots();
        }

        private void ReloadSlots()
        {
            // Decide on ID type
            if (TypeClass == typeof(WadMoveableId))
                PopulateSlots(TrCatalog.GetAllMoveables(GameVersion));
            else if (TypeClass == typeof(WadStaticId))
                PopulateSlots(TrCatalog.GetAllStatics(GameVersion));
            else if (TypeClass == typeof(WadSpriteSequenceId))
                PopulateSlots(TrCatalog.GetAllSpriteSequences(GameVersion));
            else
                throw new NotImplementedException("The " + TypeClass + " is not implemented yet.");

            // Make sure it redraws
            lstSlots.Invalidate();
        }

        private void PopulateSlots(IDictionary<uint, string> objectSlotSuggestions)
        {
            lstSlots.Items.Clear();

            string searchKeyword = tbSearch.Text;
            foreach (var objectSlotSuggestion in objectSlotSuggestions)
            {
                if (!string.IsNullOrEmpty(searchKeyword))
                    if (objectSlotSuggestion.Value.IndexOf(searchKeyword, StringComparison.OrdinalIgnoreCase) == -1)
                        continue;
                string label = "(" + objectSlotSuggestion.Key + ") " + objectSlotSuggestion.Value;

                lstSlots.Items.Add(new DarkListItem(label) { Tag = objectSlotSuggestion.Key });
            }
        }

        private void butCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }

        private void butOK_Click(object sender, EventArgs e)
        {
            if (TypeClass == typeof(WadMoveableId))
                NewId = new WadMoveableId((uint)chosenId.Value);
            else if (TypeClass == typeof(WadStaticId))
                NewId = new WadStaticId((uint)chosenId.Value);
            else if (TypeClass == typeof(WadSpriteSequenceId))
                NewId = new WadSpriteSequenceId((uint)chosenId.Value);
            else if (TypeClass == typeof(WadFixedSoundInfoId))
                NewId = new WadFixedSoundInfoId((uint)chosenId.Value);
            else if (TypeClass == typeof(WadAdditionalSoundInfoId))
                NewId = new WadAdditionalSoundInfoId(chosenIdText.Text);
            else
                throw new NotImplementedException("The " + TypeClass + " is not implemented yet.");

            DialogResult = DialogResult.OK;
            Close();
        }

        private void chosenId_ValueChanged(object sender, EventArgs e)
        {
            for(int i = 0; i < lstSlots.Items.Count; i++)
                if (lstSlots.Items[i].Tag is uint)
                    if ((uint)lstSlots.Items[i].Tag == (uint)chosenId.Value)
                    {
                        lstSlots.SelectItem(i);
                        return;
                    }
        }

        private void lstSlots_SelectedIndicesChanged(object sender, EventArgs e)
        {
            if (lstSlots.SelectedItems.Count > 0)
                if (lstSlots.SelectedItems[0].Tag is uint)
                    chosenId.Value = (uint)lstSlots.SelectedItems[0].Tag;
        }

        private void tbSearch_TextChanged(object sender, EventArgs e)
        {
            ReloadSlots();
        }
    }
}
