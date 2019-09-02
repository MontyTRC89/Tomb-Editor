using DarkUI.Controls;
using DarkUI.Forms;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using TombLib.Wad;
using TombLib.Wad.Catalog;

namespace WadTool
{
    public partial class FormAnimCommandsEditor : DarkForm
    {
        private readonly WadToolClass _tool;
        public IEnumerable<WadAnimCommand> AnimCommands => treeCommands.Nodes.Select(node => node.Tag).OfType<WadAnimCommand>();
        private bool _currentlyDoingCommandSelection = false;
        private List<int> _sounds;

        private WadAnimCommand _selectedCommand => treeCommands.SelectedNodes.FirstOrDefault()?.Tag as WadAnimCommand;

        public FormAnimCommandsEditor(WadToolClass tool, List<WadAnimCommand> animCommands)
        {
            InitializeComponent();
            _tool = tool;

            ReloadSounds();

            foreach (var cmd in animCommands)
                treeCommands.Nodes.Add(new DarkTreeNode(cmd.ToString()) { Tag = cmd.Clone() });
        }

        private void SelectCommand(WadAnimCommand cmd, bool selectInTree = true)
        {
            if (_currentlyDoingCommandSelection)
                return;

            try
            {
                _currentlyDoingCommandSelection = true;
                if (selectInTree)
                    treeCommands.SelectNode(treeCommands.Nodes.FirstOrDefault(node => node.Tag == cmd));

                comboCommandType.SelectedIndex = (int)(cmd.Type) - 1;

                switch (cmd.Type)
                {
                    case WadAnimCommandType.SetPosition:
                        commandControls.Visible = true;
                        commandControls.SelectedTab = tabSetPosition;

                        tbPosX.Value = cmd.Parameter1;
                        tbPosY.Value = cmd.Parameter2;
                        tbPosZ.Value = cmd.Parameter3;
                        break;

                    case WadAnimCommandType.SetJumpDistance:
                        commandControls.Visible = true;
                        commandControls.SelectedTab = tabSetJumpDistance;

                        tbHorizontal.Value = cmd.Parameter1;
                        tbVertical.Value = cmd.Parameter2;
                        break;

                    case WadAnimCommandType.EmptyHands:
                    case WadAnimCommandType.KillEntity:
                        commandControls.Visible = false;
                        break;

                    case WadAnimCommandType.PlaySound:
                        commandControls.Visible = true;
                        commandControls.SelectedTab = tabPlaySound;

                        tbPlaySoundFrame.Value = cmd.Parameter1;
                        comboSound.SelectedIndex = _sounds.IndexOf(cmd.Parameter2 & 0x3FFF);

                        switch (cmd.Parameter2 & 0xC000)
                        {
                            default:
                                comboPlaySoundConditions.SelectedIndex = 0;
                                break;
                            case 0x4000:
                                comboPlaySoundConditions.SelectedIndex = 1;
                                break;
                            case 0x8000:
                                comboPlaySoundConditions.SelectedIndex = 2;
                                break;
                        }
                        break;

                    case WadAnimCommandType.FlipEffect:
                        commandControls.Visible = true;
                        commandControls.SelectedTab = tabFlipeffect;

                        tbFlipEffectFrame.Value = cmd.Parameter1;
                        tbFlipEffect.Value = cmd.Parameter2;
                        break;
                }
            }
            finally
            {
                _currentlyDoingCommandSelection = false;
            }
        }

        private void butAddEffect_Click(object sender, EventArgs e)
        {
            var newCmd = new WadAnimCommand() { Type = WadAnimCommandType.SetPosition };
            treeCommands.Nodes.Add(new DarkTreeNode(newCmd.ToString()) { Tag = newCmd });
            SelectCommand(newCmd);
        }

        private void butDeleteEffect_Click(object sender, EventArgs e)
        {
            if (treeCommands.SelectedNodes.Count == 0)
                return;
            treeCommands.Nodes.Remove(treeCommands.SelectedNodes[0]);
            SelectCommand(treeCommands.Nodes.FirstOrDefault()?.Tag as WadAnimCommand);
        }

        private void btOk_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.OK;
            Close();
        }

        private void btCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }

        private void treeCommands_SelectedNodesChanged(object sender, EventArgs e)
        {
            if (_selectedCommand == null)
                return;
                SelectCommand(_selectedCommand, false);
        }

        private void comboCommandType_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (_selectedCommand == null || _currentlyDoingCommandSelection)
                return;

            WadAnimCommandType newType = (WadAnimCommandType)(comboCommandType.SelectedIndex + 1);
            _selectedCommand.Type = newType;

            treeCommands.SelectedNodes.First().Text = treeCommands.SelectedNodes.First().Tag.ToString();
            SelectCommand(_selectedCommand, false);
        }

        private void tbPosX_ValueChanged(object sender, EventArgs e)
        {
            if (_selectedCommand == null || _selectedCommand.Type != WadAnimCommandType.SetPosition)
                return;
            _selectedCommand.Parameter1 = (short)tbPosX.Value;
            treeCommands.SelectedNodes.First().Text = treeCommands.SelectedNodes.First().Tag.ToString();
        }

        private void tbPosY_ValueChanged(object sender, EventArgs e)
        {
            if (_selectedCommand == null || _selectedCommand.Type != WadAnimCommandType.SetPosition)
                return;
            _selectedCommand.Parameter2 = (short)tbPosY.Value;
            treeCommands.SelectedNodes.First().Text = treeCommands.SelectedNodes.First().Tag.ToString();
        }

        private void tbPosZ_ValueChanged(object sender, EventArgs e)
        {
            if (_selectedCommand == null || _selectedCommand.Type != WadAnimCommandType.SetPosition)
                return;
            _selectedCommand.Parameter3 = (short)tbPosZ.Value;
            treeCommands.SelectedNodes.First().Text = treeCommands.SelectedNodes.First().Tag.ToString();
        }

        private void tbHorizontal_ValueChanged(object sender, EventArgs e)
        {
            if (_selectedCommand == null || _selectedCommand.Type != WadAnimCommandType.SetJumpDistance)
                return;
            _selectedCommand.Parameter1 = (short)tbHorizontal.Value;
            treeCommands.SelectedNodes.First().Text = treeCommands.SelectedNodes.First().Tag.ToString();
        }

        private void tbVertical_ValueChanged(object sender, EventArgs e)
        {
            if (_selectedCommand == null || _selectedCommand.Type != WadAnimCommandType.SetJumpDistance)
                return;
            _selectedCommand.Parameter2 = (short)tbVertical.Value;
            treeCommands.SelectedNodes.First().Text = treeCommands.SelectedNodes.First().Tag.ToString();
        }

        private void tbFlipEffectFrame_ValueChanged(object sender, EventArgs e)
        {
            if (_selectedCommand == null || _selectedCommand.Type != WadAnimCommandType.FlipEffect)
                return;
            _selectedCommand.Parameter1 = (short)tbFlipEffectFrame.Value;
            treeCommands.SelectedNodes.First().Text = treeCommands.SelectedNodes.First().Tag.ToString();
        }

        private void tbFlipEffect_ValueChanged(object sender, EventArgs e)
        {
            if (_selectedCommand == null || _selectedCommand.Type != WadAnimCommandType.FlipEffect)
                return;
            _selectedCommand.Parameter2 = (short)tbFlipEffect.Value;
            treeCommands.SelectedNodes.First().Text = treeCommands.SelectedNodes.First().Tag.ToString();
        }

        private void tbPlaySoundFrame_ValueChanged(object sender, EventArgs e)
        {
            if (_selectedCommand == null || _selectedCommand.Type != WadAnimCommandType.PlaySound)
                return;
            _selectedCommand.Parameter1 = (short)tbPlaySoundFrame.Value;
            treeCommands.SelectedNodes.First().Text = treeCommands.SelectedNodes.First().Tag.ToString();
        }

        private void comboPlaySoundConditions_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (_selectedCommand == null || _selectedCommand.Type != WadAnimCommandType.PlaySound)
                return;
            _selectedCommand.Parameter2 &= unchecked((short)~0xC000);
            _selectedCommand.Parameter2 |= (short)(comboPlaySoundConditions.SelectedIndex << 14);
            treeCommands.SelectedNodes.First().Text = treeCommands.SelectedNodes.First().Tag.ToString();
        }

        private void ReloadSounds()
        {
            _sounds = new List<int>();
            comboSound.Items.Clear();
            foreach (var sound in TrCatalog.GetAllSounds(_tool.DestinationWad.SuggestedGameVersion))
            {
                _sounds.Add((int)sound.Key);
                comboSound.Items.Add(sound.Key.ToString().PadLeft(4, '0') + ": " + sound.Value);
            }
            comboSound.SelectedIndex = 0;
        }

        private void ComboSound_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (_selectedCommand == null || _selectedCommand.Type != WadAnimCommandType.PlaySound)
                return;

            _selectedCommand.Parameter2 &= unchecked((short)~0x3FFF);
            _selectedCommand.Parameter2 |= (short)(_sounds[comboSound.SelectedIndex]);
            treeCommands.SelectedNodes.First().Text = treeCommands.SelectedNodes.First().Tag.ToString();
        }

        private void butCommandUp_Click(object sender, EventArgs e) => MoveCommand(false);
        private void butCommandDown_Click(object sender, EventArgs e) => MoveCommand(true);

        private void MoveCommand(bool down)
        {
            var node = treeCommands.SelectedNodes.First();
            int index = treeCommands.Nodes.IndexOf(node);

            if ( down && index > treeCommands.Nodes.Count - 1) return;
            if (!down && index <= 0) return;

            treeCommands.Nodes.RemoveAt(index);
            treeCommands.Nodes.Insert((down ? index + 1 : index - 1), node);

            treeCommands.SelectNode(node);
            treeCommands.SelectedNodes.First().Text = "";
            treeCommands.SelectedNodes.First().Text = treeCommands.SelectedNodes.First().Tag.ToString();
        }
    }
}
