using DarkUI.Controls;
using DarkUI.Forms;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using TombLib.Wad;

namespace WadTool
{
    public partial class FormAnimCommandsEditor : DarkForm
    {
        private readonly WadToolClass _tool;
        public IEnumerable<WadAnimCommand> AnimCommands => treeCommands.Nodes.Select(node => node.Tag).OfType<WadAnimCommand>();
        private bool _currentlyDoingCommandSelection = false;
        private List<WadSoundInfo> _sounds;

        private WadAnimCommand _selectedCommand => treeCommands.SelectedNodes.FirstOrDefault()?.Tag as WadAnimCommand;

        public FormAnimCommandsEditor(WadToolClass tool, List<WadAnimCommand> animCommands)
        {
            InitializeComponent();
            _tool = tool;

            // Setup panels for form
            foreach (Panel panel in new[] { panelPosition, panelJumpDistance, panelSound, panelEffect })
                panel.SetBounds(panelPosition.Left, panelPosition.Top, panel.Width, panelSound.Bottom - panelPosition.Top);

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

                switch (cmd.Type)
                {
                    case WadAnimCommandType.SetPosition:
                        comboCommandType.Enabled = true;
                        panelEffect.Visible = false;
                        panelJumpDistance.Visible = false;
                        panelPosition.Visible = true;
                        panelSound.Visible = false;

                        comboCommandType.SelectedIndex = (int)(cmd.Type) - 1;
                        tbPosX.Value = cmd.Parameter1;
                        tbPosY.Value = cmd.Parameter2;
                        tbPosZ.Value = cmd.Parameter3;
                        break;

                    case WadAnimCommandType.SetJumpDistance:
                        comboCommandType.Enabled = true;
                        panelEffect.Visible = false;
                        panelJumpDistance.Visible = true;
                        panelPosition.Visible = false;
                        panelSound.Visible = false;

                        comboCommandType.SelectedIndex = (int)(cmd.Type) - 1;
                        tbHorizontal.Value = cmd.Parameter1;
                        tbVertical.Value = cmd.Parameter2;
                        break;

                    case WadAnimCommandType.EmptyHands:
                    case WadAnimCommandType.KillEntity:
                        comboCommandType.Enabled = true;
                        panelEffect.Visible = false;
                        panelJumpDistance.Visible = false;
                        panelPosition.Visible = false;
                        panelSound.Visible = false;

                        comboCommandType.SelectedIndex = (int)(cmd.Type) - 1;
                        break;

                    case WadAnimCommandType.PlaySound:
                        comboCommandType.Enabled = true;
                        panelEffect.Visible = false;
                        panelJumpDistance.Visible = false;
                        panelPosition.Visible = false;
                        panelSound.Visible = true;

                        comboCommandType.SelectedIndex = (int)(cmd.Type) - 1;
                        tbPlaySoundFrame.Value = cmd.Parameter1;
                        soundInfoEditor.SoundInfo = cmd.SoundInfo;
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
                        comboCommandType.Enabled = true;
                        panelEffect.Visible = true;
                        panelJumpDistance.Visible = false;
                        panelPosition.Visible = false;
                        panelSound.Visible = false;

                        comboCommandType.SelectedIndex = (int)(cmd.Type) - 1;
                        tbFlipEffectFrame.Value = cmd.Parameter1;
                        tbFlipEffect.Value = cmd.Parameter2;
                        break;

                    default:
                        //comboCommandType.Enabled = false;
                        panelEffect.Visible = false;
                        panelJumpDistance.Visible = false;
                        panelPosition.Visible = false;
                        panelSound.Visible = false;
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
            var newCmd = new WadAnimCommand();
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
            ReloadSounds();

            if (_selectedCommand == null || _currentlyDoingCommandSelection)
                return;

            WadAnimCommandType newType = (WadAnimCommandType)(comboCommandType.SelectedIndex + 1);
            _selectedCommand.Type = newType;

            // Add a new sound info if needed
            if (_selectedCommand.Type == WadAnimCommandType.PlaySound)
            {
                if (_selectedCommand.SoundInfo == null)
                    _selectedCommand.SoundInfo = new WadSoundInfo();
            }

            treeCommands.SelectedNodes.First().Text = treeCommands.SelectedNodes.First().Tag.ToString();
            SelectCommand(_selectedCommand, false);
        }

        private void tbPosX_ValueChanged(object sender, EventArgs e)
        {
            if (_selectedCommand == null)
                return;
            _selectedCommand.Parameter1 = (short)tbPosX.Value;
            treeCommands.SelectedNodes.First().Text = treeCommands.SelectedNodes.First().Tag.ToString();
        }

        private void tbPosY_ValueChanged(object sender, EventArgs e)
        {
            if (_selectedCommand == null)
                return;
            _selectedCommand.Parameter2 = (short)tbPosY.Value;
            treeCommands.SelectedNodes.First().Text = treeCommands.SelectedNodes.First().Tag.ToString();
        }

        private void tbPosZ_ValueChanged(object sender, EventArgs e)
        {
            if (_selectedCommand == null)
                return;
            _selectedCommand.Parameter3 = (short)tbPosZ.Value;
            treeCommands.SelectedNodes.First().Text = treeCommands.SelectedNodes.First().Tag.ToString();
        }

        private void tbHorizontal_ValueChanged(object sender, EventArgs e)
        {
            if (_selectedCommand == null)
                return;
            _selectedCommand.Parameter1 = (short)tbHorizontal.Value;
            treeCommands.SelectedNodes.First().Text = treeCommands.SelectedNodes.First().Tag.ToString();
        }

        private void tbVertical_ValueChanged(object sender, EventArgs e)
        {
            if (_selectedCommand == null)
                return;
            _selectedCommand.Parameter2 = (short)tbVertical.Value;
            treeCommands.SelectedNodes.First().Text = treeCommands.SelectedNodes.First().Tag.ToString();
        }

        private void tbFlipEffectFrame_ValueChanged(object sender, EventArgs e)
        {
            if (_selectedCommand == null)
                return;
            _selectedCommand.Parameter1 = (short)tbFlipEffectFrame.Value;
            treeCommands.SelectedNodes.First().Text = treeCommands.SelectedNodes.First().Tag.ToString();
        }

        private void tbFlipEffect_ValueChanged(object sender, EventArgs e)
        {
            if (_selectedCommand == null)
                return;
            _selectedCommand.Parameter2 = (short)tbFlipEffect.Value;
            treeCommands.SelectedNodes.First().Text = treeCommands.SelectedNodes.First().Tag.ToString();
        }

        private void tbPlaySoundFrame_ValueChanged(object sender, EventArgs e)
        {
            if (_selectedCommand == null)
                return;
            _selectedCommand.Parameter1 = (short)tbPlaySoundFrame.Value;
            treeCommands.SelectedNodes.First().Text = treeCommands.SelectedNodes.First().Tag.ToString();
        }

        private void comboPlaySoundConditions_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (_selectedCommand == null)
                return;
            _selectedCommand.Parameter2 &= unchecked((short)~0xC000);
            _selectedCommand.Parameter2 |= (short)(comboPlaySoundConditions.SelectedIndex << 14);
            treeCommands.SelectedNodes.First().Text = treeCommands.SelectedNodes.First().Tag.ToString();
        }

        private void soundInfoEditor_SoundInfoChanged(object sender, EventArgs e)
        {
            if (_selectedCommand == null)
                return;
            _selectedCommand.SoundInfo = soundInfoEditor.SoundInfo;
            treeCommands.SelectedNodes.First().Text = treeCommands.SelectedNodes.First().Tag.ToString();
        }

        private void ReloadSounds()
        {
            _sounds = new List<WadSoundInfo>();
            comboSound.Items.Clear();
            comboSound.Items.Add("(Select a sound info)");
            foreach (var sound in _tool.DestinationWad.SoundInfosUnique)
            {
                _sounds.Add(sound);
                comboSound.Items.Add(sound.Name);
            }
            comboSound.SelectedIndex = 0;
        }
    }
}
