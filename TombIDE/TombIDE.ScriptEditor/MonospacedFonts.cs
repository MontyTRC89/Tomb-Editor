using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Text;
using System.Runtime.InteropServices;

namespace TombIDE.ScriptEditor
{
	internal class MonospacedFonts
	{
		[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
		private class LOGFONT
		{
			public int lfHeight;
			public int lfWidth;
			public int lfEscapement;
			public int lfOrientation;
			public int lfWeight;
			public byte lfItalic;
			public byte lfUnderline;
			public byte lfStrikeOut;
			public byte lfCharSet;
			public byte lfOutPrecision;
			public byte lfClipPrecision;
			public byte lfQuality;
			public byte lfPitchAndFamily;

			[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
			public string lfFaceName;
		}

		private static bool IsMonospaced(Graphics g, Font f)
		{
			float w1, w2;

			w1 = g.MeasureString("i", f).Width;
			w2 = g.MeasureString("W", f).Width;
			return w1 == w2;
		}

		private static bool IsSymbolFont(Font font)
		{
			const byte SYMBOL_FONT = 2;

			LOGFONT logicalFont = new LOGFONT();
			font.ToLogFont(logicalFont);
			return logicalFont.lfCharSet == SYMBOL_FONT;
		}

		private static bool IsSuitableFont(string fontName)
		{
			return !fontName.StartsWith("ESRI") && !fontName.StartsWith("Oc_") && !fontName.Contains("MDL2");
		}

		public static List<string> GetMonospacedFontNames()
		{
			List<string> fontList = new List<string>();
			InstalledFontCollection fontCollection = new InstalledFontCollection();

			Bitmap bitmap = new Bitmap(1, 1);
			Graphics graphics = Graphics.FromImage(bitmap);

			foreach (FontFamily fontFamily in fontCollection.Families)
			{
				if (fontFamily.IsStyleAvailable(FontStyle.Regular) && fontFamily.IsStyleAvailable(FontStyle.Bold)
					&& fontFamily.IsStyleAvailable(FontStyle.Italic) && IsSuitableFont(fontFamily.Name))
				{
					Font font = new Font(fontFamily, 10);

					if (IsMonospaced(graphics, font) && !IsSymbolFont(font))
						fontList.Add(fontFamily.Name);
				}
			}

			return fontList;
		}
	}
}
