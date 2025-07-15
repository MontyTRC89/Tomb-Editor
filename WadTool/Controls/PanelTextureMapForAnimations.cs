﻿using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using TombLib.Forms;
using TombLib.LevelData;

namespace WadTool.Controls
{
	public class PanelTextureMapForAnimations : Controls.PanelTextureMap
	{
		protected override float MaxTextureSize => float.PositiveInfinity;
		protected override bool DrawTriangle => false;

		private static readonly Pen outlinePen = new Pen(Color.FromArgb(80, 192, 192, 192), 2);
		private static readonly Pen activeOutlinePen = new Pen(Color.FromArgb(200, 238, 82, 238), 2);
		private static readonly Brush textBrush = new SolidBrush(Color.Violet);
		private static readonly Brush textShadowBrush = new SolidBrush(Color.Black);
		private static readonly Font textFont = new Font("Segoe UI", 12.0f, FontStyle.Bold, GraphicsUnit.Pixel);
		private static readonly StringFormat textFormat = new StringFormat() { LineAlignment = StringAlignment.Center, Alignment = StringAlignment.Center };
		
		private readonly WadToolClass _tool;

		public PanelTextureMapForAnimations(WadToolClass tool) : base()
		{
			_tool = tool;
		}

		protected override void OnPaintSelection(PaintEventArgs e)
		{
			// Paint other animated textures
			if (_tool.DestinationWad?.AnimatedTextureSets.Count > 0)
			{
				var selectedSet = ParentForm.SelectedSet;

				foreach (AnimatedTextureSet set in _tool.DestinationWad?.AnimatedTextureSets)
					if (set != selectedSet)
						DrawSetOutlines(e, set, false);

				DrawSetOutlines(e, selectedSet, true);
			}

			// Paint current selection
			base.OnPaintSelection(e);
		}

		private void DrawSetOutlines(PaintEventArgs e, AnimatedTextureSet set, bool current)
		{
			if (set == null)
				return;

			for (int i = 0; i < set.Frames.Count; ++i)
			{
				AnimatedTextureFrame frame = set.Frames[i];
				if (frame.Texture == VisibleTexture)
				{
					PointF[] edges = new[]
					{
								ToVisualCoord(frame.TexCoord0),
								ToVisualCoord(frame.TexCoord1),
								ToVisualCoord(frame.TexCoord2),
								ToVisualCoord(frame.TexCoord3)
						};

					PointF upperLeft = new PointF(
						Math.Min(Math.Min(edges[0].X, edges[1].X), Math.Min(edges[2].X, edges[3].X)),
						Math.Min(Math.Min(edges[0].Y, edges[1].Y), Math.Min(edges[2].Y, edges[3].Y)));
					PointF lowerRight = new PointF(
						Math.Max(Math.Max(edges[0].X, edges[1].X), Math.Max(edges[2].X, edges[3].X)),
						Math.Max(Math.Max(edges[0].Y, edges[1].Y), Math.Max(edges[2].Y, edges[3].Y)));

					if (current)
					{
						string counterString = i + 1 + "/" + set.Frames.Count;
						SizeF textSize = e.Graphics.MeasureString(counterString, textFont);
						RectangleF textArea = RectangleF.FromLTRB(upperLeft.X, upperLeft.Y, lowerRight.X, lowerRight.Y);

						if (textArea.Width > textSize.Width && textArea.Height > textSize.Height)
						{
							e.Graphics.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAliasGridFit;
							e.Graphics.DrawString(counterString, textFont, textShadowBrush, textArea, textFormat);
							textArea.X -= 1;
							textArea.Y -= 1;
							e.Graphics.DrawString(counterString, textFont, textBrush, textArea, textFormat);
						}
					}
					e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
					e.Graphics.DrawPolygon(current ? activeOutlinePen : outlinePen, edges);
				}
			}
		}

		protected FormAnimatedTextures ParentForm
		{
			get
			{
				Control parent = Parent;
				while (!(parent is FormAnimatedTextures))
					parent = parent.Parent;
				return (FormAnimatedTextures)parent;
			}
		}
	}
}
