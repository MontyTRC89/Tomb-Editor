using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using TombEditor.Geometry;
using System.IO;
using System.Media;

namespace TombEditor
{
    public partial class FormSound : Form
    {
        private Dictionary<short, string> _sounds;
        private SoundSourceInstance _soundSource;

        public FormSound(SoundSourceInstance soundSource)
        {
            _soundSource = soundSource;

            InitializeComponent();

            // Load sound.txt
            using (StreamReader reader = new StreamReader(File.OpenRead("Sounds\\Sounds.txt")))
            {
                List<RowSoundSample> rows = new List<RowSoundSample>();
                _sounds = new Dictionary<short, string>();
                short i = 0;
                while (reader.EndOfStream == false)
                {
                    string s = reader.ReadLine();

                    string name = "";
                    string file = "";

                    string[] tokens = s.Split(':');
                    name = tokens[0];

                    if (tokens.Length > 1)
                    {
                        string temp = tokens[1].Trim(' ', '\t');
                        tokens = temp.Split(' ', '\t');
                        file = tokens[0];
                    }

                    RowSoundSample row = new RowSoundSample(i, name, file);

                    rows.Add(row);
                    _sounds.Add(i, name);

                    i++;
                }

                lstSamples.SetObjects(rows);
            }

            tbSound.Text = _sounds[_soundSource.SoundId];
            cbBit1.Checked = _soundSource.Bits[0];
            cbBit2.Checked = _soundSource.Bits[1];
            cbBit3.Checked = _soundSource.Bits[2];
            cbBit4.Checked = _soundSource.Bits[3];
            cbBit5.Checked = _soundSource.Bits[4];
        }

        private void butCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            this.Close();
        }
        
        private void butOK_Click(object sender, EventArgs e)
        {
            if (lstSamples.SelectedObject == null)
                return;
            RowSoundSample row = (RowSoundSample)lstSamples.SelectedObject;

            _soundSource.SoundId = row.ID;
            _soundSource.Bits[0] = cbBit1.Checked;
            _soundSource.Bits[1] = cbBit2.Checked;
            _soundSource.Bits[2] = cbBit3.Checked;
            _soundSource.Bits[3] = cbBit4.Checked;
            _soundSource.Bits[4] = cbBit5.Checked;

            DialogResult = DialogResult.OK;
            this.Close();
        }

        private void butPlay_Click(object sender, EventArgs e)
        {
            if (lstSamples.SelectedObject == null)
                return;
            RowSoundSample row = (RowSoundSample)lstSamples.SelectedObject;

            if (row.File != "")
            {
                if (!File.Exists("Sounds\\Samples\\" + row.File + ".wav"))
                    return;
                SoundPlayer player = new SoundPlayer("Sounds\\Samples\\" + row.File + ".wav");
                player.Play();
            }
        }

        private void lstSamples_Click(object sender, EventArgs e)
        {
            if (lstSamples.SelectedObject == null)
                return;
            RowSoundSample row = (RowSoundSample)lstSamples.SelectedObject;
            tbSound.Text = row.Name;
        }
    }
}
