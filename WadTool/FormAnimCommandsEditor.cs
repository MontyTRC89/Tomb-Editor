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
    public partial class FormAnimCommandsEditor : DarkUI.Forms.DarkForm
    {
        private enum AnimCommandType
        {
            SetPosition,
            SetJumpDistance,
            EmptyHands,
            KillEntity,
            PlaySound,
            PlaySoundLand,
            PlaySoundWater,
            FlipEffect
        }

        private class AnimCommand
        {
            public AnimCommandType Type { get; set; }
            public short X { get; set; }
            public short Y { get; set; }
            public short Z { get; set; }
            public short Horizontal { get; set; }
            public short Vertical { get; set; }
            public short Frame { get; set; }
            public short Effect { get; set; }
            public WadSoundInfo SoundInfo { get; set; }

            public AnimCommand(AnimCommandType type)
            {
                Type = type;
            }

            public AnimCommand() { }

            public override string ToString()
            {
                switch (Type)
                {
                    case AnimCommandType.EmptyHands:
                        return "Remove guns from hands";
                    case AnimCommandType.SetJumpDistance:
                        return "Set jump reference <V, H> = <" + Horizontal + ", " + Vertical + ">";
                    case AnimCommandType.KillEntity:
                        return "Kill entity";
                    case AnimCommandType.SetPosition:
                        return "Set position reference <X, Y, Z> = " + X + ", " + Y + ", " + Z + ">";
                    case AnimCommandType.PlaySound:
                        return "Play Sound ID = " + SoundInfo.ToString() + " on Frame = " + Frame;
                    case AnimCommandType.PlaySoundLand:
                        return "Play Sound ID = " + SoundInfo.ToString() + " (land) on Frame = " + Frame;
                    case AnimCommandType.PlaySoundWater:
                        return "Play Sound ID = " + SoundInfo.ToString() + " (water) on Frame = " + Frame;
                    case AnimCommandType.FlipEffect:
                        return "Play FlipEffect ID = " + Effect + " on Frame = " + Frame;
                }

                return "";
            }
        }

        public List<WadAnimCommand> AnimCommands { get; private set; }

        private AnimCommand _selectedCommand = null;
        private WadToolClass _tool;
        private bool _isNew = false;

        public FormAnimCommandsEditor(WadToolClass tool, List<WadAnimCommand> animCommands)
        {
            InitializeComponent();

            _tool = tool;

            foreach (var soundInfo in _tool.DestinationWad.SoundInfosUnique)
                comboSound.Items.Add(soundInfo);

            foreach (var cmd in animCommands)
            {
                var workingCmd = new AnimCommand();
                if (cmd.Type == WadAnimCommandType.SetPosition)
                {
                    workingCmd.Type = AnimCommandType.SetPosition;
                    workingCmd.X = cmd.Parameter1;
                    workingCmd.Y = cmd.Parameter1;
                    workingCmd.Z = cmd.Parameter1;
                }
                else if (cmd.Type == WadAnimCommandType.JumpDistance)
                {
                    workingCmd.Type = AnimCommandType.SetJumpDistance;
                    workingCmd.Horizontal = cmd.Parameter1;
                    workingCmd.Vertical = cmd.Parameter2;
                }
                else if (cmd.Type == WadAnimCommandType.EmptyHands)
                    workingCmd.Type = AnimCommandType.EmptyHands;
                else if (cmd.Type == WadAnimCommandType.KillEntity)
                    workingCmd.Type = AnimCommandType.KillEntity;
                else if (cmd.Type == WadAnimCommandType.PlaySound)
                {
                    if ((cmd.Parameter2 & 0xC000) == 0x8000)
                        workingCmd.Type = AnimCommandType.PlaySoundWater;
                    else if ((cmd.Parameter2 & 0xC000) == 0x4000)
                        workingCmd.Type = AnimCommandType.PlaySoundLand;
                    else
                        workingCmd.Type = AnimCommandType.PlaySound;
                    workingCmd.Frame = cmd.Parameter1;
                    workingCmd.SoundInfo = cmd.SoundInfo;
                }
                else if (cmd.Type== WadAnimCommandType.FlipEffect)
                {
                    workingCmd.Type = AnimCommandType.FlipEffect;
                    workingCmd.Frame = cmd.Parameter1;
                    workingCmd.Effect = (short)(cmd.Parameter2 & 0x3FFF);
                }

                var node = new DarkUI.Controls.DarkTreeNode(workingCmd.ToString());
                node.Tag = workingCmd;
                treeCommands.Nodes.Add(node);
            }

            if (treeCommands.Nodes.Count > 0)
                treeCommands.SelectNode(treeCommands.Nodes[0]);
            else
            {
                _isNew = true;
                comboCommandType.SelectedIndex = 0;
            }
        }

        private void SelectCommand(AnimCommand cmd)
        {
            _selectedCommand = null;
            comboCommandType.SelectedIndex = (int)cmd.Type;

            if (cmd.Type == AnimCommandType.SetPosition)
            {
                tbPosX.Text = cmd.X.ToString();
                tbPosY.Text = cmd.Y.ToString();
                tbPosZ.Text = cmd.Z.ToString();
            }
            else if (cmd.Type == AnimCommandType.SetJumpDistance)
            {
                tbHorizontal.Text = cmd.Horizontal.ToString();
                tbVertical.Text = cmd.Vertical.ToString();
            }
            else if (cmd.Type == AnimCommandType.PlaySound || cmd.Type == AnimCommandType.PlaySoundLand ||
                     cmd.Type == AnimCommandType.PlaySoundWater)
            {
                tbPlaySoundFrame.Text = cmd.Frame.ToString();
                comboSound.SelectedItem = cmd.SoundInfo;
            }
            else if (cmd.Type == AnimCommandType.FlipEffect)
            {
                tbFlipEffectFrame.Text = cmd.Frame.ToString();
                tbFlipEffect.Text = cmd.Effect.ToString();
            }
        }

        private void butAddEffect_Click(object sender, EventArgs e)
        {
            _isNew = true;
            _selectedCommand = null;
            comboCommandType.SelectedIndex = 0;
        }

        private void comboCommandType_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (comboCommandType.SelectedIndex < 0)
            {
                panelEffect.Visible = false;
                panelJumpDistance.Visible = false;
                panelPosition.Visible = false;
                panelSound.Visible = false;
            }
            else if (comboCommandType.SelectedIndex == (int)AnimCommandType.SetPosition)
            {
                panelEffect.Visible = false;
                panelJumpDistance.Visible = false;
                panelPosition.Visible = true;
                panelSound.Visible = false;
                panelPosition.Location = new Point(12, 275);
            }
            else if (comboCommandType.SelectedIndex == (int)AnimCommandType.SetJumpDistance)
            {
                panelEffect.Visible = false;
                panelJumpDistance.Visible = true;
                panelPosition.Visible = false;
                panelSound.Visible = false;
                panelJumpDistance.Location = new Point(12, 275);
            }
            else if (comboCommandType.SelectedIndex == (int)AnimCommandType.KillEntity ||
                     comboCommandType.SelectedIndex == (int)AnimCommandType.EmptyHands)
            {
                panelEffect.Visible = false;
                panelJumpDistance.Visible = false;
                panelPosition.Visible = false;
                panelSound.Visible = false;
            }
            else if (comboCommandType.SelectedIndex == (int)AnimCommandType.PlaySound ||
                     comboCommandType.SelectedIndex == (int)AnimCommandType.PlaySoundLand ||
                     comboCommandType.SelectedIndex == (int)AnimCommandType.PlaySoundWater)
            {
                panelEffect.Visible = false;
                panelJumpDistance.Visible = false;
                panelPosition.Visible = false;
                panelSound.Visible = true;
                panelSound.Location = new Point(12, 275);
            }
            else if (comboCommandType.SelectedIndex == (int)AnimCommandType.FlipEffect)
            {
                panelEffect.Visible = true;
                panelJumpDistance.Visible = false;
                panelPosition.Visible = false;
                panelSound.Visible = false;
                panelEffect.Location = new Point(12, 275);
            }
        }

        private void butDeleteEffect_Click(object sender, EventArgs e)
        {
            if (treeCommands.SelectedNodes.Count!= 0)
            {
                treeCommands.Nodes.Remove(treeCommands.SelectedNodes[0]);
                _selectedCommand = null;
                if (treeCommands.Nodes.Count > 0)
                    treeCommands.SelectNode(treeCommands.Nodes[0]);
                else
                {
                    _isNew = true;
                    comboCommandType.SelectedIndex = 0;
                }
            }
        }

        private void butSaveChanges_Click(object sender, EventArgs e)
        {
            var cmd = (_isNew ? new AnimCommand() : _selectedCommand);
            cmd.Type = (AnimCommandType)comboCommandType.SelectedIndex;

            if (cmd.Type == AnimCommandType.SetPosition)
            {
                cmd.X = short.Parse(tbPosX.Text);
                cmd.Y = short.Parse(tbPosY.Text);
                cmd.Z = short.Parse(tbPosZ.Text);
            }
            else if (cmd.Type== AnimCommandType.SetJumpDistance)
            {
                cmd.Horizontal = short.Parse(tbHorizontal.Text);
                cmd.Vertical = short.Parse(tbVertical.Text);
            }
            else if (cmd.Type== AnimCommandType.PlaySound ||
                     cmd.Type == AnimCommandType.PlaySoundLand ||
                     cmd.Type == AnimCommandType.PlaySoundWater)
            {
                cmd.Frame= short.Parse(tbPlaySoundFrame.Text);
                cmd.SoundInfo = (WadSoundInfo)comboSound.SelectedItem;
            }
            else if (cmd.Type == AnimCommandType.FlipEffect)
            {
                cmd.Frame = short.Parse(tbFlipEffectFrame.Text);
                cmd.Effect = short.Parse(tbFlipEffect.Text);
            }

            if (_isNew)
            {
                _isNew = false;
                var node = new DarkUI.Controls.DarkTreeNode(cmd.ToString());
                node.Tag = cmd;
                treeCommands.Nodes.Add(node);
                treeCommands.SelectNode(treeCommands.Nodes.Last());
            }
        }

        private void btOk_Click(object sender, EventArgs e)
        {
            AnimCommands = new List<WadAnimCommand>();
            foreach (var node in treeCommands.Nodes)
            {
                var newCmd = new WadAnimCommand();
                var oldCmd = (AnimCommand)node.Tag;

                if (oldCmd.Type == AnimCommandType.SetPosition)
                {
                    newCmd.Type = WadAnimCommandType.SetPosition;
                    newCmd.Parameter1 = oldCmd.X;
                    newCmd.Parameter2 = oldCmd.Y;
                    newCmd.Parameter3 = oldCmd.Z;
                }
                else if (oldCmd.Type == AnimCommandType.SetJumpDistance)
                {
                    newCmd.Type = WadAnimCommandType.JumpDistance;
                    newCmd.Parameter1 = oldCmd.Horizontal;
                    newCmd.Parameter2 = oldCmd.Vertical;
                }
                else if (oldCmd.Type == AnimCommandType.EmptyHands)
                    newCmd.Type = WadAnimCommandType.EmptyHands;
                else if (oldCmd.Type == AnimCommandType.KillEntity)
                    newCmd.Type = WadAnimCommandType.KillEntity;
                else if (oldCmd.Type== AnimCommandType.PlaySound)
                {
                    newCmd.Type = WadAnimCommandType.PlaySound;
                    newCmd.Parameter1 = oldCmd.Frame;
                    newCmd.SoundInfo = oldCmd.SoundInfo;
                }
                else if (oldCmd.Type == AnimCommandType.PlaySoundLand)
                {
                    newCmd.Type = WadAnimCommandType.PlaySound;
                    newCmd.Parameter1 = oldCmd.Frame;
                    newCmd.Parameter2 |= unchecked((short)0x4000);
                    newCmd.SoundInfo = oldCmd.SoundInfo;
                }
                else if (oldCmd.Type == AnimCommandType.PlaySoundWater)
                {
                    newCmd.Type = WadAnimCommandType.PlaySound;
                    newCmd.Parameter1 = oldCmd.Frame;
                    newCmd.Parameter2 |= unchecked((short)0x8000);
                    newCmd.SoundInfo = oldCmd.SoundInfo;
                }
                else if (oldCmd.Type == AnimCommandType.FlipEffect)
                {
                    newCmd.Type = WadAnimCommandType.FlipEffect;
                    newCmd.Parameter1 = oldCmd.Frame;
                    newCmd.Parameter2 = oldCmd.Effect;
                }

                AnimCommands.Add(newCmd);
            }

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
            if (treeCommands.SelectedNodes.Count != 0)
                SelectCommand((AnimCommand)treeCommands.SelectedNodes[0].Tag);
        }
    }
}
