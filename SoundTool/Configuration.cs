using System.Drawing;
using TombLib;

namespace SoundTool
{
    public class Configuration : ConfigurationBase
    {
        public override string ConfigName { get { return "SoundToolConfiguration.xml"; } }

        public string SoundTool_ReferenceProject { get; set; } = string.Empty;
        public float UI_FormColor_Brightness { get; set; } = 100.0f;

        public Point Window_FormMain_Position { get; set; } = new Point(-1);
        public Size Window_FormMain_Size { get; set; } = new Size(810, 520);
        public bool Window_FormMain_Maximized { get; set; } = false;
    }
}
