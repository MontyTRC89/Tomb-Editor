using DarkUI.Controls;
using DarkUI.Forms;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using TombLib.Wad;
using TombLib.Wad.Catalog;

namespace WadTool
{
    public partial class FormSelectSlot : DarkForm
    {
        public Type TypeClass { get; }
        public WadGameVersion GameVersion { get; }
        public IWadObjectId NewId { get; set; } = null;

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
            else
                throw new NotImplementedException("The " + TypeClass + " is not implemented yet.");

            ReloadSlots();
        }

        private void ReloadSlots()
        {
            IDictionary<uint, string> objectSlotSuggestions;
            if (TypeClass == typeof(WadMoveableId))
                objectSlotSuggestions = TrCatalog.GetAllMoveables(GameVersion);
            else if (TypeClass == typeof(WadStaticId))
                objectSlotSuggestions = TrCatalog.GetAllStatics(GameVersion);
            else if (TypeClass == typeof(WadSpriteSequenceId))
                objectSlotSuggestions = TrCatalog.GetAllSpriteSequences(GameVersion);
            else if (TypeClass == typeof(WadFixedSoundInfoId))
                objectSlotSuggestions = TrCatalog.GetAllSounds(GameVersion);
            else
                throw new NotImplementedException("The " + TypeClass + " is not implemented yet.");

            // TODO Implement fuzzy search?
            string searchKeyword = tbSearch.Text;
            var nodes = new List<DarkTreeNode>();
            foreach (var objectSlotSuggestion in objectSlotSuggestions)
            {
                if (!string.IsNullOrEmpty(searchKeyword))
                    if (objectSlotSuggestion.Value.IndexOf(searchKeyword, StringComparison.OrdinalIgnoreCase) == -1)
                        continue;
                string label = "(" + objectSlotSuggestion.Key + ") " + objectSlotSuggestion.Value.ToString();
                nodes.Add(new DarkTreeNode(label) { Tag = objectSlotSuggestion.Key });
            }
            treeSlots.Nodes.Clear();
            treeSlots.Nodes.AddRange(nodes);
            treeSlots.Invalidate();
        }

        private void butCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }

        private void butOK_Click(object sender, EventArgs e)
        {
            if (TypeClass == typeof(WadMoveableId) || TypeClass == typeof(WadMoveable))
                NewId = new WadMoveableId((uint)chosenId.Value);
            else if (TypeClass == typeof(WadStaticId) || TypeClass == typeof(WadStatic))
                NewId = new WadStaticId((uint)chosenId.Value);
            else if (TypeClass == typeof(WadSpriteSequenceId) || TypeClass == typeof(WadSpriteSequence))
                NewId = new WadSpriteSequenceId((uint)chosenId.Value);
            else if (TypeClass == typeof(WadFixedSoundInfoId) || TypeClass == typeof(WadFixedSoundInfo))
                NewId = new WadFixedSoundInfoId((uint)chosenId.Value);
            else
                throw new NotImplementedException("The " + TypeClass + " is not implemented yet.");

            DialogResult = DialogResult.OK;
            Close();
        }

        private void chosenId_ValueChanged(object sender, EventArgs e)
        {
            foreach (DarkTreeNode node in treeSlots.Nodes)
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
                chosenId.Value = (uint)treeSlots.SelectedNodes[0].Tag;
        }

        private void tbSearch_TextChanged(object sender, EventArgs e)
        {
            ReloadSlots();
        }
    }
}
