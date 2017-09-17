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
using TombLib.Wad;

namespace WadTool
{
    public partial class FormMain : DarkUI.Forms.DarkForm
    {
        public FormMain()
        {
            InitializeComponent();
        }

        private void butTest_Click(object sender, EventArgs e)
        {
            var oldWad = new TR4Wad();
            oldWad.LoadWad("Graphics\\Wads\\karnak.wad");

            var newWad = WadOperations.ConvertTr4Wad(oldWad);

            using (var stream = File.OpenWrite("karnak.wad2"))
            {
                Wad2.SaveToStream(newWad, stream);
            }

            newWad.Dispose();

            using (var stream = File.OpenRead("karnak.wad2"))
            {
                newWad = Wad2.LoadFromStream(stream);
            }

            var wadKarnak = new Wad2();

           
                var oldKarnak = new TR4Wad();
                oldKarnak.LoadWad("Graphics\\Wads\\coastal.wad");
                wadKarnak = WadOperations.ConvertTr4Wad(oldKarnak);
           
            newWad.AddObject(wadKarnak.Moveables[35], 35);

            using (var stream = File.OpenWrite("karnak_edited.wad2"))
            {
                Wad2.SaveToStream(newWad, stream);
            }
        }
    }
}
