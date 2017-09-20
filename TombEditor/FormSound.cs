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
using TombLib.Wad;
using NLog;

namespace TombEditor
{
    public partial class FormSound : Form
    {
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();
        
        private SoundSourceInstance _soundSource;
        private Wad2 _wad;

        public FormSound(SoundSourceInstance soundSource, Wad2 wad)
        {
            _soundSource = soundSource;
            _wad = wad;

            InitializeComponent();
            
            using (StreamReader reader = new StreamReader(new FileStream("Sounds\\Sounds.txt", FileMode.Open, FileAccess.Read, FileShare.Read)))
            {
                List<RowSoundSample> rows = new List<RowSoundSample>();
                short id = 0;
                while (reader.EndOfStream == false)
                {
                    string line = reader.ReadLine();
                    string[] tokens = line.Split(':');
                    string file = "";

                    if (tokens.Length > 1)
                    {
                        string temp = tokens[1].Trim(' ', '\t');
                        tokens = temp.Split(' ', '\t');
                        file = tokens[0];
                    }
                    
                    rows.Add(new RowSoundSample { ID = id++, File = file });
                }

                lstSamples.SetObjects(rows);
                tbSound.Text = rows.FirstOrDefault(row => row.ID == _soundSource.SoundId).Name;
            }

            cbBit1.Checked = (_soundSource.CodeBits & (1 << 0)) != 0;
            cbBit2.Checked = (_soundSource.CodeBits & (1 << 1)) != 0;
            cbBit3.Checked = (_soundSource.CodeBits & (1 << 2)) != 0;
            cbBit4.Checked = (_soundSource.CodeBits & (1 << 3)) != 0;
            cbBit5.Checked = (_soundSource.CodeBits & (1 << 4)) != 0;
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
            byte codeBits = 0;
            codeBits |= (byte)(cbBit1.Checked ? (1 << 0) : 0);
            codeBits |= (byte)(cbBit1.Checked ? (1 << 1) : 0);
            codeBits |= (byte)(cbBit1.Checked ? (1 << 2) : 0);
            codeBits |= (byte)(cbBit1.Checked ? (1 << 3) : 0);
            codeBits |= (byte)(cbBit1.Checked ? (1 << 4) : 0);
            _soundSource.CodeBits = codeBits;

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
                try
                {
                    SoundPlayer player = new SoundPlayer("Sounds\\Samples\\" + row.File + ".wav");
                    player.Play();
                }
                catch (Exception exc)
                {
                    logger.Warn(exc, "Unable to play sample \"" + row.File + "\"");
                }
            }
        }

        private void lstSamples_Click(object sender, EventArgs e)
        {
            if (lstSamples.SelectedObject == null)
                return;
            RowSoundSample row = (RowSoundSample)lstSamples.SelectedObject;
            tbSound.Text = row.Name;
        }

        private struct RowSoundSample
        {
            public short ID { get; set; }
            public string Name => OriginalSoundsDefinitions.SoundNames[ID];
            public string File { get; set; }
        }

        private void FormSound_Load(object sender, EventArgs e)
        {

        }
    }
}
