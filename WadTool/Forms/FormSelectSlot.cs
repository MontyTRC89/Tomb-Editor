﻿using DarkUI.Controls;
using DarkUI.Forms;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using TombLib.LevelData;
using TombLib.Wad;
using TombLib.Wad.Catalog;

namespace WadTool
{
    public partial class FormSelectSlot : DarkForm
    {
        public Type TypeClass { get; internal set; }
        public TRVersion.Game GameVersion { get; internal set; }
        public IWadObjectId NewId { get; internal set; }

        private Wad2 _wad;
        private List<uint> _additionalObjectsToHide;

        public FormSelectSlot(Wad2 wad, IWadObjectId currentId, List<uint> additionalObjectsToHide = null)
        {
            InitializeComponent();

            NewId = currentId;
            TypeClass = currentId.GetType();
            GameVersion = wad.GameVersion.Native();

            _wad = wad;
            _additionalObjectsToHide = additionalObjectsToHide;

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
            if (lstSlots.Items.Count > 0)
                chosenId.Value = (decimal)(uint)lstSlots.Items.First().Tag;
        }

        private void ReloadSlots()
        {
            // Decide on ID type
            if (TypeClass == typeof(WadMoveableId))
                PopulateSlots(TrCatalog.GetAllMoveables(GameVersion).Where(item => !_wad.Moveables.Any(moveable => moveable.Key.TypeId == item.Key) && !(_additionalObjectsToHide?.Any(add => add == item.Key) ?? false)).ToList());
            else if (TypeClass == typeof(WadStaticId))
                PopulateSlots(TrCatalog.GetAllStatics(GameVersion).Where(item => !_wad.Statics.Any(stat => stat.Key.TypeId == item.Key) && !(_additionalObjectsToHide?.Any(add => add == item.Key) ?? false)).ToList());
            else if (TypeClass == typeof(WadSpriteSequenceId))
                PopulateSlots(TrCatalog.GetAllSpriteSequences(GameVersion).Where(item => !_wad.SpriteSequences.Any(sprite => sprite.Key.TypeId == item.Key) && !(_additionalObjectsToHide?.Any(add => add == item.Key) ?? false)).ToList());
            else
                throw new NotImplementedException("The " + TypeClass + " is not implemented yet.");

            // Make sure it redraws
            lstSlots.Invalidate();
        }

        private void PopulateSlots(List<KeyValuePair<uint, string>> objectSlotSuggestions)
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

        private void ConfirmAndClose()
        {
            if (lstSlots.Items.Count == 0 || lstSlots.SelectedItems.Count == 0)
                DialogResult = DialogResult.Cancel;
            else
            {
                uint newId = (uint)lstSlots.SelectedItems[0].Tag;

                if (TypeClass == typeof(WadMoveableId))
                    NewId = new WadMoveableId(newId);
                else if (TypeClass == typeof(WadStaticId))
                    NewId = new WadStaticId(newId);
                else if (TypeClass == typeof(WadSpriteSequenceId))
                    NewId = new WadSpriteSequenceId(newId);

                // FIXME: Are we still handling copying/moving/etc of deprecated sound info objects?

                else if (TypeClass == typeof(WadFixedSoundInfoId))
                    NewId = new WadFixedSoundInfoId((uint) chosenId.Value);
                else if (TypeClass == typeof(WadAdditionalSoundInfoId))
                    NewId = new WadAdditionalSoundInfoId(chosenIdText.Text);
                else
                    throw new NotImplementedException("The " + TypeClass + " is not implemented yet.");

                DialogResult = DialogResult.OK;
            }

            Close();
        }

        private void chosenId_ValueChanged(object sender, EventArgs e)
        {
            for (int i = 0; i < lstSlots.Items.Count; i++)
                if (lstSlots.Items[i].Tag is uint)
                    if ((uint)lstSlots.Items[i].Tag == (uint)chosenId.Value)
                    {
                        lstSlots.SelectItem(i);
                        return;
                    }

            lstSlots.ClearSelection();
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

        private void butCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }

        private void butOK_Click(object sender, EventArgs e) => ConfirmAndClose();
        private void lstSlots_MouseDoubleClick(object sender, MouseEventArgs e) => ConfirmAndClose();
    }
}
