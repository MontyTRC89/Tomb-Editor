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
                treeSlots.Enabled = false;
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
            treeSlots.Nodes.Clear();

            // Decide on ID type
            if (TypeClass == typeof(WadMoveableId))
                treeSlots.Nodes.AddRange(PopulateSlots(TrCatalog.GetAllMoveables(GameVersion)));
            else if (TypeClass == typeof(WadStaticId))
                treeSlots.Nodes.AddRange(PopulateSlots(TrCatalog.GetAllStatics(GameVersion)));
            else if (TypeClass == typeof(WadSpriteSequenceId))
                treeSlots.Nodes.AddRange(PopulateSlots(TrCatalog.GetAllSpriteSequences(GameVersion)));
            else if (TypeClass == typeof(WadFixedSoundInfoId))
            {
                DarkTreeNode usedSoundNode = new DarkTreeNode("Sounds used by the game");
                usedSoundNode.Nodes.AddRange(PopulateSlots(TrCatalog.GetAllFixedByDefaultSounds(GameVersion)));
                usedSoundNode.Expanded = true;
                treeSlots.Nodes.Add(usedSoundNode);

                DarkTreeNode allSoundNode = new DarkTreeNode("All sound slots");
                allSoundNode.Nodes.AddRange(PopulateSlots(TrCatalog.GetAllSounds(GameVersion)));
                treeSlots.Nodes.Add(allSoundNode);
            }
            else if (TypeClass == typeof(WadAdditionalSoundInfoId))
            { }
            else
                throw new NotImplementedException("The " + TypeClass + " is not implemented yet.");

            // Make sure it redraws
            treeSlots.Invalidate();
        }

        private IEnumerable<DarkTreeNode> PopulateSlots(IDictionary<uint, string> objectSlotSuggestions)
        {
            string searchKeyword = tbSearch.Text;
            foreach (var objectSlotSuggestion in objectSlotSuggestions)
            {
                if (!string.IsNullOrEmpty(searchKeyword))
                    if (objectSlotSuggestion.Value.IndexOf(searchKeyword, StringComparison.OrdinalIgnoreCase) == -1)
                        continue;
                string label = "(" + objectSlotSuggestion.Key + ") " + objectSlotSuggestion.Value;
                yield return new DarkTreeNode(label) { Tag = objectSlotSuggestion.Key };
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
            foreach (DarkTreeNode node in treeSlots.Nodes)
                if (node.Tag is uint)
                    if ((uint)node.Tag == (uint)chosenId.Value)
                    {
                        treeSlots.SelectNode(node);
                        return;
                    }
            if (treeSlots.SelectedNodes.Count > 0)
            {
                treeSlots.SelectedNodes.Clear();
                treeSlots.Invalidate();
            }
        }

        private void treeSlots_SelectedNodesChanged(object sender, EventArgs e)
        {
            if (treeSlots.SelectedNodes.Count > 0)
                if (treeSlots.SelectedNodes[0].Tag is uint)
                    chosenId.Value = (uint)treeSlots.SelectedNodes[0].Tag;
        }

        private void tbSearch_TextChanged(object sender, EventArgs e)
        {
            ReloadSlots();
        }
    }
}
