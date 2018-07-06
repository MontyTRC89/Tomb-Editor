using DarkUI.Forms;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using TombLib.Wad;

namespace WadTool
{
    public partial class FormSoundOverview : DarkForm
    {
        private struct SoundUse
        {
            public bool GrayedOut;
            public string Name { get; set; }
            public Func<WadSoundInfo> Get { get; set; }
            public Action<WadSoundInfo> Set { get; set; }
        };

        private class SoundInfo
        {
            public WadSoundInfo NewSoundInfo;
            public List<SoundUse> Uses;
            public string Name
            {
                get { return NewSoundInfo.Name; }
                set
                {
                    WadSoundInfoMetaData metaData = NewSoundInfo.Data;
                    metaData.Name = value;
                    NewSoundInfo = new WadSoundInfo(metaData);
                }
            }
        };

        private readonly List<SoundInfo> _soundInfos = new List<SoundInfo>();

        public FormSoundOverview(Wad2 wad)
        {
            InitializeComponent();

            // Gather sound uses froms wad
            List<SoundUse> SoundUses = new List<SoundUse>();
            foreach (WadMoveable moveable in wad.Moveables.Values)
            {
                string moveableName = "Moveable - " + moveable.ToString(wad.SuggestedGameVersion);
                foreach (WadAnimation animation in moveable.Animations)
                {
                    string animationName = moveableName + " - " + animation.Name;
                    for (int i = 0; i < animation.AnimCommands.Count; ++i)
                        if (animation.AnimCommands[i].Type == WadAnimCommandType.PlaySound)
                        {
                            string animCommandName = moveableName + " - " + animation.Name + " - frame " + animation.AnimCommands[i].Parameter1;
                            int iCapturedByLambda = i;
                            SoundUses.Add(new SoundUse
                            {
                                Name = animCommandName,
                                Get = () => animation.AnimCommands[iCapturedByLambda].SoundInfo,
                                Set = soundInfo =>
                                {
                                    WadAnimCommand animCommand = animation.AnimCommands[iCapturedByLambda];
                                    animCommand.SoundInfo = soundInfo;
                                    animation.AnimCommands[iCapturedByLambda] = animCommand;
                                }
                            });
                        }
                }
            }
            foreach (WadFixedSoundInfo fixedSoundInfo in wad.FixedSoundInfos.Values)
            {
                SoundUses.Add(new SoundUse
                {
                    Name = "Fixed sound slot - " + fixedSoundInfo.ToString(wad.SuggestedGameVersion),
                    Get = () => fixedSoundInfo.SoundInfo,
                    Set = soundInfo => fixedSoundInfo.SoundInfo = soundInfo
                });
            }
            foreach (WadAdditionalSoundInfo additionalSoundInfos in wad.AdditionalSoundInfos.Values)
            {
                SoundUses.Add(new SoundUse
                {
                    Name = "Additional sound info - " + additionalSoundInfos.ToString(wad.SuggestedGameVersion),
                    Get = () => additionalSoundInfos.SoundInfo,
                    Set = soundInfo => additionalSoundInfos.SoundInfo = soundInfo,
                    GrayedOut = true
                });
            }

            // Group sound info uses
            _soundInfos = SoundUses
                .GroupBy(soundUse => soundUse.Get())
                .Select(group => new SoundInfo { Uses = group.ToList(), NewSoundInfo = group.Key })
                .OrderBy(SoundInfo => SoundInfo.NewSoundInfo.Name)
                .ToList();

            UpdateSoundInfoList();
        }

        public void UpdateSoundInfoList()
        {
            string search = soundInfosDataGridViewTxtSearch.Text;
            if (string.IsNullOrWhiteSpace(search))
                soundInfosDataGridView.DataSource = _soundInfos;
            else
                soundInfosDataGridView.DataSource = _soundInfos
                    .Where(soundInfo => soundInfo.Name.IndexOf(search, StringComparison.CurrentCultureIgnoreCase) >= 0)
                    .ToList();
        }

        private void btOk_Click(object sender, EventArgs e)
        {
            // Update wad
            foreach (SoundInfo soundInfo in _soundInfos)
                foreach (SoundUse use in soundInfo.Uses)
                    use.Set(soundInfo.NewSoundInfo);

            // Close
            DialogResult = DialogResult.OK;
            Close();
        }

        private void btCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }

        private void soundInfosDataGridView_SelectionChanged(object sender, EventArgs e)
        {
            if (soundInfosDataGridView.SelectedRows.Count == 1)
            {
                var soundInfo = (SoundInfo)(soundInfosDataGridView.SelectedRows[0].DataBoundItem);
                soundInfoEditor.SoundInfo = soundInfo.NewSoundInfo;
                usedForDataGridView.DataSource = soundInfo.Uses;
                usedForDataGridView.Enabled = true;
                soundInfoEditor.Enabled = true;
            }
            else
            {
                usedForDataGridView.DataSource = null;
                soundInfoEditor.SoundInfo = WadSoundInfo.Empty;
                usedForDataGridView.Enabled = false;
                soundInfoEditor.Enabled = false;
            }
        }

        private void soundInfosDataGridView_CellFormattingSafe(object sender, DarkUI.Controls.DarkDataGridViewSafeCellFormattingEventArgs e)
        {
            SoundInfo soundInfo = _soundInfos[e.RowIndex];
            if (soundInfo.Uses.All(use => use.GrayedOut))
            {
                e.CellStyle.ForeColor = System.Drawing.Color.FromArgb(100, 100, 100);
                e.FormattingApplied = true;
            }
        }

        private void soundInfoEditor_SoundInfoChanged(object sender, EventArgs e)
        {
            // Update name if necessary / save new sound info
            foreach (DataGridViewRow row in soundInfosDataGridView.SelectedRows)
            {
                var soundInfo = (SoundInfo)(row.DataBoundItem);
                soundInfo.NewSoundInfo = soundInfoEditor.SoundInfo;
                soundInfosDataGridView.InvalidateRow(row.Index);
            }
        }

        private void soundInfosDataGridViewTxtSearch_TextChanged(object sender, EventArgs e)
        {
            UpdateSoundInfoList();
        }
    }
}
