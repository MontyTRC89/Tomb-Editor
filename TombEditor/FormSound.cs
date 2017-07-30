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
        public bool IsNew { get; set; }
        private Editor _editor;
        public short SoundID;
        private Dictionary<short, string> _sounds;

        public FormSound()
        {
            InitializeComponent();
        }

        private void butCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            this.Close();
        }

        private void FormSound_Load(object sender, EventArgs e)
        {
            _editor = Editor.Instance;

            // Load sound.txt
            StreamReader reader = new StreamReader(File.OpenRead("Sounds\\Sounds.txt"));
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


            if (!IsNew)
            {
                SoundInstance sound = (SoundInstance)_editor.Level.Objects[_editor.PickingResult.Element];
                SoundID = sound.SoundId;
                tbSound.Text = _sounds[SoundID];
                cbBit1.Checked = sound.Bits[0];
                cbBit2.Checked = sound.Bits[1];
                cbBit3.Checked = sound.Bits[2];
                cbBit4.Checked = sound.Bits[3];
                cbBit5.Checked = sound.Bits[4];
            }
        }

        private void butOK_Click(object sender, EventArgs e)
        {
            if (lstSamples.SelectedObject == null)
                return;
            RowSoundSample row = (RowSoundSample)lstSamples.SelectedObject;

            if (!IsNew)
            {
                SoundInstance sound = (SoundInstance)_editor.Level.Objects[_editor.PickingResult.Element];
                sound.SoundId = row.ID;
                sound.Bits[0] = cbBit1.Checked;
                sound.Bits[1] = cbBit2.Checked;
                sound.Bits[2] = cbBit3.Checked;
                sound.Bits[3] = cbBit4.Checked;
                sound.Bits[4] = cbBit5.Checked;
                _editor.Level.Objects[_editor.PickingResult.Element] = sound;
            }
            else
            {
                /* sound.Bits[0] = cbBit1.Checked;
                 sound.Bits[1] = cbBit2.Checked;
                 sound.Bits[2] = cbBit3.Checked;
                 sound.Bits[3] = cbBit4.Checked;
                 sound.Bits[4] = cbBit5.Checked;*/
                SoundID = row.ID;
            }

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
            SoundID = row.ID;
            tbSound.Text = row.Name;
        }
    }
}
