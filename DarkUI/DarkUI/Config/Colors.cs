using System.Drawing;

namespace DarkUI.Config
{
    public static class Colors
    {
        public static Color GreyBackground { get; set; } = Color.FromArgb(60, 63, 65);
        public static Color HeaderBackground { get; set; } = Color.FromArgb(57, 60, 62);
        public static Color BlueBackground { get; set; } = Color.FromArgb(66, 77, 95);
        public static Color DarkBlueBackground { get; set; } = Color.FromArgb(52, 57, 66);
        public static Color DarkBackground { get; set; } = Color.FromArgb(43, 43, 43);
        public static Color MediumBackground { get; set; } = Color.FromArgb(49, 51, 53);
        public static Color LightBackground { get; set; } = Color.FromArgb(69, 73, 74);
        public static Color LighterBackground { get; set; } = Color.FromArgb(95, 101, 102);
        public static Color LightestBackground { get; set; } = Color.FromArgb(178, 178, 178);
        public static Color LightBorder { get; set; } = Color.FromArgb(81, 81, 81);
        public static Color DarkBorder { get; set; } = Color.FromArgb(51, 51, 51);
        public static Color LightText { get; set; } = Color.FromArgb(220, 220, 220);
        public static Color DisabledText { get; set; } = Color.FromArgb(153, 153, 153);
        public static Color BlueHighlight { get; set; } = Color.FromArgb(104, 151, 187);
        public static Color BlueSelection { get; set; } = Color.FromArgb(75, 110, 175);
        public static Color GreyHighlight { get; set; } = Color.FromArgb(122, 128, 132);
        public static Color GreySelection { get; set; } = Color.FromArgb(92, 92, 92);
        public static Color DarkGreySelection { get; set; } = Color.FromArgb(82, 82, 82);
        public static Color DarkBlueBorder { get; set; } = Color.FromArgb(51, 61, 78);
        public static Color LightBlueBorder { get; set; } = Color.FromArgb(86, 97, 114);
        public static Color ActiveControl { get; set; } = Color.FromArgb(159, 178, 196);
        public static Color MenuItemToggledOnBorder { get; set; } = Color.FromArgb(104, 151, 187);
        public static Color MenuItemToggledOnFill
        {
            get
            {
                return Color.FromArgb((int)(MenuItemToggledOnBorder.R * 0.66f),
                                      (int)(MenuItemToggledOnBorder.G * 0.45f),
                                      (int)(MenuItemToggledOnBorder.B * 0.56f));
            }
        }
    }
}
