using System;
using System.Drawing;
using DarkUI.Extensions;

namespace DarkUI.Config
{
    public static class Colors
    {
        public static Color GreyBackground { get; private set; } = Color.FromArgb(60, 63, 65);
        public static Color HeaderBackground { get; private set; } = Color.FromArgb(57, 60, 62);
        public static Color BlueBackground { get; private set; } = Color.FromArgb(66, 77, 95);
        public static Color DarkBlueBackground { get; private set; } = Color.FromArgb(52, 57, 66);
        public static Color DarkBackground { get; private set; } = Color.FromArgb(43, 43, 43);
        public static Color MediumBackground { get; private set; } = Color.FromArgb(49, 51, 53);
        public static Color LightBackground { get; private set; } = Color.FromArgb(69, 73, 74);
        public static Color LighterBackground { get; private set; } = Color.FromArgb(95, 101, 102);
        public static Color LightestBackground { get; private set; } = Color.FromArgb(178, 178, 178);
        public static Color LightBorder { get; private set; } = Color.FromArgb(81, 81, 81);
        public static Color DarkBorder { get; private set; } = Color.FromArgb(51, 51, 51);
        public static Color LightText { get; private set; } = Color.FromArgb(220, 220, 220);
        public static Color DisabledText { get; private set; } = Color.FromArgb(153, 153, 153);
        public static Color BlueHighlight { get; private set; } = Color.FromArgb(104, 151, 187);
        public static Color BlueSelection { get; private set; } = Color.FromArgb(75, 110, 175);
        public static Color GreyHighlight { get; private set; } = Color.FromArgb(122, 128, 132);
        public static Color GreySelection { get; private set; } = Color.FromArgb(92, 92, 92);
        public static Color DarkGreySelection { get; private set; } = Color.FromArgb(82, 82, 82);
        public static Color DarkBlueBorder { get; private set; } = Color.FromArgb(51, 61, 78);
        public static Color LightBlueBorder { get; private set; } = Color.FromArgb(86, 97, 114);
        public static Color ActiveControl { get; private set; } = Color.FromArgb(159, 178, 196);

        public static Color HighlightBase { get; set; } = Color.FromArgb(104, 151, 187);
        public static Color HighlightFill => HighlightBase.Multiply(0.66f, 0.45f, 0.56f);

        private static Color DarkBase { get; } = Color.FromArgb(60, 63, 65);

        public const float MaxBrightness = 1.0f;
        public const float MinBrightness = 0.5f;
        public static float InvertedBrightness => MaxBrightness - Brightness;
        public static float Brightness
        {
            get { return _brightness; }
            set
            {
                _brightness = Math.Min(Math.Max(value, MinBrightness), MaxBrightness);

                GreyBackground  	= DarkBase.Multiply(value * 1.000f, value * 1.000f, value * 1.000f);
				HeaderBackground  	= DarkBase.Multiply(value * 0.950f, value * 0.952f, value * 0.954f);
				BlueBackground  	= DarkBase.Multiply(value * 1.100f, value * 1.222f, value * 1.462f);
				DarkBlueBackground 	= DarkBase.Multiply(value * 0.867f, value * 0.905f, value * 1.015f);
				DarkBackground  	= DarkBase.Multiply(value * 0.717f, value * 0.683f, value * 0.662f);
				MediumBackground  	= DarkBase.Multiply(value * 0.817f, value * 0.810f, value * 0.815f);
				LightBackground  	= DarkBase.Multiply(value * 1.150f, value * 1.159f, value * 1.138f);
				LighterBackground   = DarkBase.Multiply(value * 1.583f, value * 1.603f, value * 1.569f);
				LightestBackground  = DarkBase.Multiply(value * 2.967f, value * 2.825f, value * 2.738f);
				LightBorder  		= DarkBase.Multiply(value * 1.350f, value * 1.286f, value * 1.246f);
				DarkBorder  		= DarkBase.Multiply(value * 0.850f, value * 0.810f, value * 0.785f);
				LightText  		 	= DarkBase.Multiply(value * 3.667f, value * 3.492f, value * 3.385f);
				DisabledText  		= DarkBase.Multiply(value * 2.550f, value * 2.429f, value * 2.354f);
				BlueHighlight  	 	= DarkBase.Multiply(value * 1.733f, value * 2.397f, value * 2.877f);
				BlueSelection  	 	= DarkBase.Multiply(value * 1.250f, value * 1.746f, value * 2.692f);
				GreyHighlight  	 	= DarkBase.Multiply(value * 2.033f, value * 2.032f, value * 2.031f);
				GreySelection  	 	= DarkBase.Multiply(value * 1.533f, value * 1.460f, value * 1.415f);
				DarkGreySelection   = DarkBase.Multiply(value * 1.367f, value * 1.302f, value * 1.262f);
				DarkBlueBorder  	= DarkBase.Multiply(value * 0.850f, value * 0.968f, value * 1.200f);
				LightBlueBorder  	= DarkBase.Multiply(value * 1.433f, value * 1.540f, value * 1.754f);
				ActiveControl  	 	= DarkBase.Multiply(value * 2.650f, value * 2.825f, value * 3.015f);
            }
        }
        private static float _brightness;
    }
}
