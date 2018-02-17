using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using TombLib.Sounds;
using TombLib.Wad;

namespace WadTool
{
    public partial class FormTr2r3SoundManager : DarkUI.Forms.DarkForm
    {
        private WadToolClass _tool;
        private SortedDictionary<ushort, SoundCatalogInfo> _catalog;

        public FormTr2r3SoundManager()
        {
            InitializeComponent();

            _tool = WadToolClass.Instance;

            // Fill the listbox
            _catalog = SoundsCatalog.GetAllSounds(_tool.DestinationWad.Version);

            dgvSounds.Rows.Clear();
            foreach (var pair in _catalog)
                dgvSounds.Rows.Add(_tool.DestinationWad.Sounds.ContainsKey(pair.Key),
                                   pair.Key.ToString().PadLeft(3, '0'),
                                   pair.Value.Name);

            UpdateStatistics();
        }

        private void butSaveChanges_Click(object sender, EventArgs e)
        {
            foreach (DataGridViewRow row in dgvSounds.Rows)
            {
                if ((bool)(row.Cells[0].Value) == true)
                {
                    var soundId = UInt16.Parse(row.Cells[1].Value.ToString());
                    if (_tool.DestinationWad.Sounds.ContainsKey(soundId))
                        continue;

                    var oldInfo = SoundsCatalog.GetSound(_tool.DestinationWad.Version, soundId);
                    var newInfo = new WadSoundInfo();

                    newInfo.Name = oldInfo.Name;
                    newInfo.Pitch = oldInfo.Pitch;
                    newInfo.Volume = oldInfo.Volume;
                    newInfo.Chance = oldInfo.Chance;
                    newInfo.Range = oldInfo.Range;
                    newInfo.FlagN = oldInfo.FlagN;
                    newInfo.RandomizeGain = oldInfo.FlagV;
                    newInfo.RandomizePitch = oldInfo.FlagP;
                    if (oldInfo.FlagL)
                        newInfo.Loop = WadSoundLoopType.L;
                    else if (oldInfo.FlagW)
                        newInfo.Loop = WadSoundLoopType.W;
                    else if (oldInfo.FlagR)
                        newInfo.Loop = WadSoundLoopType.R;

                    // Load the samples
                    foreach (var sample in oldInfo.Samples)
                    {
                        string path;
                        if (_tool.DestinationWad.Version == WadTombRaiderVersion.TR2)
                            path = Path.GetDirectoryName(_tool.Configuration.MainSfx_Path_Tr2) + "\\Samples\\" + sample + ".wav";
                        else
                            path = Path.GetDirectoryName(_tool.Configuration.MainSfx_Path_Tr3) + "\\Samples\\" + sample + ".wav";

                        // Try to load the sample
                        var buffer = new byte[0];
                        try
                        {
                            using (var reader = new BinaryReader(File.OpenRead(path)))
                                buffer = reader.ReadBytes((int)reader.BaseStream.Length);
                        }
                        catch (Exception ex)
                        {
                            continue;
                        }

                        var newSample = new WadSample(sample, buffer);
                        newSample.UpdateHash();

                        if (!_tool.DestinationWad.Samples.ContainsKey(newSample.Hash))
                            _tool.DestinationWad.Samples.Add(newSample.Hash, newSample);
                        newInfo.Samples.Add(newSample);
                    }

                    _tool.DestinationWad.Sounds.Add(soundId, newInfo);
                }
                else
                {
                    // Remove samples
                    _tool.DestinationWad.DeleteSound(UInt16.Parse(row.Cells[1].Value.ToString()));
                }
            }

            MessageBox.Show("Sounds: " + _tool.DestinationWad.Sounds.Count + " of " +
                            _tool.DestinationWad.SoundMapSize + "    " +
                            "Embedded WAV samples: " + _tool.DestinationWad.Samples.Count,
                            "Error",
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Information);
            Close();
        }

        private void UpdateStatistics()
        {
            string message = "Sounds: " + _tool.DestinationWad.Sounds.Count + " of " +
                             _tool.DestinationWad.SoundMapSize + "    " +
                             "Embedded WAV samples: " + _tool.DestinationWad.Samples.Count;
            labelStatistics.Text = message;
        }

        private void butPlaySound_Click(object sender, EventArgs e)
        {
            
        }
    }
}
