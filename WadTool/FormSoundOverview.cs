using DarkUI.Forms;
using System;
using System.Collections.Generic;
using System.Reflection;
using TombLib.Wad;

namespace WadTool
{
    public partial class FormSoundOverview : DarkForm
    {
        private struct SoundUse
        {
            public string Name;
            public Func<WadSoundInfo> Get;
            public Action<WadSoundInfo> Set;
        };

        private readonly List<SoundUse> SoundUses = new List<SoundUse>();

        public FormSoundOverview()
        {
            InitializeComponent();
        }

        public FormSoundOverview(Wad2 wad)
            : this()
        {
            // Gather sound uses froms wad
            foreach (WadMoveable moveable in wad.Moveables.Values)
            {
                string moveableName = moveable.ToString(wad.SuggestedGameVersion);
                foreach (WadAnimation animation in moveable.Animations)
                {
                    string animationName = moveableName + " - " + animation.Name;
                    foreach (WadAnimCommand animCommand in animation.AnimCommands)
                    {
                        string animCommandName = moveableName + " - " + animation.Name + " - From frame " +





                        animation.AnimCommands[j].SoundInfo;
                    }

                        SoundUses.Add(new SoundUse
                    {
                        Name = moveable.ToString(wad.SuggestedGameVersion),
                        Get = () => fixedSoundInfo.SoundInfo,
                        Set = soundInfo => fixedSoundInfo.SoundInfo = soundInfo
                    });
                }
            }

            foreach (WadFixedSoundInfo fixedSoundInfo in wad.FixedSoundInfos.Values)
            {
                SoundUses.Add(new SoundUse
                {
                    Name = fixedSoundInfo.ToString(wad.SuggestedGameVersion),
                    Get = () => fixedSoundInfo.SoundInfo,
                    Set = soundInfo => fixedSoundInfo.SoundInfo = soundInfo
                });
            }





        }

        private void btOk_Click(object sender, EventArgs e)
        {

        }

        private void btCancel_Click(object sender, EventArgs e)
        {

        }

        private void soundInfosDataGridViewTxtSearch_TextChanged(object sender, EventArgs e)
        {

        }
    }
}
