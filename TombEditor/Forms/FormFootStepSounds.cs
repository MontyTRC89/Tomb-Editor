﻿using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Numerics;
using System.Windows.Forms;
using DarkUI.Forms;
using TombLib;
using TombLib.LevelData;
using TombLib.Utils;
using Color = System.Drawing.Color;
using RectangleF = System.Drawing.RectangleF;

namespace TombEditor.Forms
{
    public partial class FormFootStepSounds : DarkForm
    {
        private const byte _alpha = 212;
        private static readonly Brush[] _textureSoundBrushes = new Brush[16]
            {
                new SolidBrush(Color.FromArgb(_alpha, 255, 188, 143)), // 0: Mud
                new SolidBrush(Color.FromArgb(_alpha, 220, 224, 250)), // 1: Snow
                new SolidBrush(Color.FromArgb(_alpha, 190, 190, 10)), // 2: Sand
                new SolidBrush(Color.FromArgb(_alpha, 128, 128, 128)), // 3: Gravel
                new SolidBrush(Color.FromArgb(_alpha, 140, 170, 250)), // 4: Ice
                new SolidBrush(Color.FromArgb(_alpha, 40, 80, 230)), // 5: Water
                new SolidBrush(Color.FromArgb(_alpha, 160, 160, 170)), // 6: Stone
                new SolidBrush(Color.FromArgb(_alpha, 222, 184, 135)), // 7: Wood
                new SolidBrush(Color.FromArgb(_alpha, 190, 180, 180)), // 8: Metal
                new SolidBrush(Color.FromArgb(_alpha, 244, 164, 96)), // 9: Marble
                new SolidBrush(Color.FromArgb(_alpha, 34, 139, 34)), // 10: Grass
                new SolidBrush(Color.FromArgb(_alpha, 112, 128, 144)), // 11: Concrete
                new HatchBrush(HatchStyle.WideUpwardDiagonal, Color.FromArgb(_alpha, 222, 184, 135), Color.FromArgb(_alpha, 205, 133, 63)), // 12: OldWood
                new HatchBrush(HatchStyle.WideUpwardDiagonal, Color.FromArgb(_alpha, 190, 180, 180), Color.FromArgb(_alpha, 205, 133, 63)), // 13: OldMetal
                new HatchBrush(HatchStyle.WideUpwardDiagonal, Color.FromArgb(_alpha, 199, 21, 133), Color.FromArgb(_alpha, 255, 0, 255)), // 14: Unknown14
                new HatchBrush(HatchStyle.WideUpwardDiagonal, Color.FromArgb(_alpha, 138, 43, 226), Color.FromArgb(_alpha, 255, 0, 255))  // 15: Unknown15
            };
        private static readonly Brush _coverBrush = new SolidBrush(Color.FromArgb(128, 15, 15, 200));
        private static readonly StringFormat _textureSoundStringFormat = new StringFormat(StringFormatFlags.NoWrap) { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };
        private const float _textureSoundStringSize = 0.4f;
        private const float _textureSoundProportion = 1.0f / 4.0f;
        private Editor _editor;

        public FormFootStepSounds(Editor editor, LevelTexture texture)
        {
            InitializeComponent();
            _editor = editor;

            this.SetActualSize();

            // Set window property handlers
            Configuration.LoadWindowProperties(this, _editor.Configuration);
            FormClosing += new FormClosingEventHandler((s, e) => Configuration.SaveWindowProperties(this, _editor.Configuration));

            // Initialize texture map
            if (editor.SelectedTexture.TextureIsInvisible)
                textureMap.ResetVisibleTexture(texture);
            else
                textureMap.ShowTexture(editor.SelectedTexture);

            // Add texture sounds to combo box
            foreach (TextureFootStepSound sound in Enum.GetValues(typeof(TextureFootStepSound)))
                comboSounds.Items.Add(sound);
        }

        private void butOk_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.OK;
            Close();
        }

        private void butAssignSound_Click(object sender, EventArgs e)
        {
            if (!(comboSounds.SelectedItem is TextureFootStepSound))
                return;

            var sound = (TextureFootStepSound)comboSounds.SelectedItem;

            Vector2 p0 = textureMap.SelectedTexture.TexCoord0 / LevelTexture.FootStepSoundGranularity;
            Vector2 p1 = textureMap.SelectedTexture.TexCoord1 / LevelTexture.FootStepSoundGranularity;
            Vector2 p2 = textureMap.SelectedTexture.TexCoord2 / LevelTexture.FootStepSoundGranularity;
            Vector2 p3 = textureMap.SelectedTexture.TexCoord3 / LevelTexture.FootStepSoundGranularity;

            int xMin = (int)Math.Min(Math.Min(Math.Min(p0.X, p1.X), p2.X), p3.X);
            int xMax = (int)Math.Max(Math.Max(Math.Max(p0.X, p1.X), p2.X), p3.X);
            int yMin = (int)Math.Min(Math.Min(Math.Min(p0.Y, p1.Y), p2.Y), p3.Y);
            int yMax = (int)Math.Max(Math.Max(Math.Max(p0.Y, p1.Y), p2.Y), p3.Y);

            for (int y = yMin; y < yMax; ++y)
                for (int x = xMin; x < xMax; ++x)
                    textureMap.VisibleTexture.SetFootStepSound(x, y, sound);

            // TODO: disabled for now
            /*ConservativeRasterizer.RasterizeQuad(
                textureMap.SelectedTexture.TexCoord0 / LevelTexture.FootStepSoundGranularity,
                textureMap.SelectedTexture.TexCoord1 / LevelTexture.FootStepSoundGranularity,
                textureMap.SelectedTexture.TexCoord2 / LevelTexture.FootStepSoundGranularity,
                textureMap.SelectedTexture.TexCoord3 / LevelTexture.FootStepSoundGranularity,
                (startX, startY, endX, endY) =>
                {
                    for (int y = startY; y < endY; ++y)
                        for (int x = startX; x < endX; ++x)
                            textureMap.VisibleTexture.SetFootStepSound(x, y, sound);
                });*/

            textureMap.Invalidate();
            _editor.TextureSoundsChange();
        }

        public class PanelTextureMapForSounds : Controls.PanelTextureMap
        {
            public PanelTextureMapForSounds()
                : base()
            {
                _allowFreeCornerEdit = false;
            }

            protected override SelectionPrecisionType GetSelectionPrecision(bool rectangularSelection)
            {
                return new SelectionPrecisionType(LevelTexture.FootStepSoundGranularity, true);
            }

            protected override float MaxTextureSize => float.PositiveInfinity;
            protected override bool DrawTriangle => false;

            protected override void OnPaintSelection(PaintEventArgs e)
            {
                var texture = VisibleTexture;

                // Determine relevant area
                Vector2 start = FromVisualCoord(new PointF());
                Vector2 end = FromVisualCoord(new PointF() + ClientSize);
                start = Vector2.Min(texture.Image.Size, Vector2.Max(new Vector2(0), start));
                end = Vector2.Min(texture.Image.Size, Vector2.Max(new Vector2(0), end));
                int soundTileStartX = (int)Math.Floor(start.X / LevelTexture.FootStepSoundGranularity);
                int soundTileStartY = (int)Math.Floor(start.Y / LevelTexture.FootStepSoundGranularity);
                int soundTileEndX = (int)Math.Ceiling(end.X / LevelTexture.FootStepSoundGranularity);
                int soundTileEndY = (int)Math.Ceiling(end.Y / LevelTexture.FootStepSoundGranularity);

                // Draw texture sounds
                using (Font textureSoundFont = new Font(Font.FontFamily, _textureSoundStringSize * _textureSoundProportion * LevelTexture.FootStepSoundGranularity * Math.Min(100, ViewScale)))
                    for (int y = soundTileStartY; y <= soundTileEndY; ++y)
                        for (int x = soundTileStartX; x <= soundTileEndX; ++x)
                        {
                            if (x < 0 || x >= texture.FootStepSoundWidth || y < 0 || y >= texture.FootStepSoundHeight)
                                continue;

                            TextureFootStepSound sound = texture.GetFootStepSound(x, y);
                            Brush soundBrush = _textureSoundBrushes[(int)sound];

                            Vector2 tileStartTexCoord = new Vector2(x, y) * LevelTexture.FootStepSoundGranularity;
                            PointF tileStart = ToVisualCoord(tileStartTexCoord);
                            PointF tileEnd = ToVisualCoord(tileStartTexCoord + new Vector2(LevelTexture.FootStepSoundGranularity));
                            PointF descStart = new PointF(tileStart.X, tileStart.Y * _textureSoundProportion + tileEnd.Y * (1 - _textureSoundProportion));

                            RectangleF descArea = RectangleF.FromLTRB(descStart.X, descStart.Y, tileEnd.X, tileEnd.Y);
                            e.Graphics.FillRectangle(soundBrush, descArea);
                            if(ViewScale > 6)
                                e.Graphics.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAliasGridFit;
                            e.Graphics.DrawString(sound.ToString(), textureSoundFont, Brushes.Black, descArea, _textureSoundStringFormat);

                            RectangleF tileArea = RectangleF.FromLTRB(tileStart.X, tileStart.Y, tileEnd.X, tileEnd.Y);
                            e.Graphics.DrawRectangle(Pens.White, tileArea);
                        }

                // Fill covered tiles
                Vector2 p0 = SelectedTexture.TexCoord0 / LevelTexture.FootStepSoundGranularity;
                Vector2 p1 = SelectedTexture.TexCoord1 / LevelTexture.FootStepSoundGranularity;
                Vector2 p2 = SelectedTexture.TexCoord2 / LevelTexture.FootStepSoundGranularity;
                Vector2 p3 = SelectedTexture.TexCoord3 / LevelTexture.FootStepSoundGranularity;

                int xMin = (int)Math.Min(Math.Min(Math.Min(p0.X, p1.X), p2.X), p3.X);
                int xMax = (int)Math.Max(Math.Max(Math.Max(p0.X, p1.X), p2.X), p3.X);
                int yMin = (int)Math.Min(Math.Min(Math.Min(p0.Y, p1.Y), p2.Y), p3.Y);
                int yMax = (int)Math.Max(Math.Max(Math.Max(p0.Y, p1.Y), p2.Y), p3.Y);

                PointF selStart = ToVisualCoord(new Vector2(xMin, yMin) * LevelTexture.FootStepSoundGranularity);
                PointF selEnd = ToVisualCoord(new Vector2(xMax, yMax) * LevelTexture.FootStepSoundGranularity);
                RectangleF selArea = RectangleF.FromLTRB(selStart.X, selStart.Y, selEnd.X, selEnd.Y);

                e.Graphics.FillRectangle(_coverBrush, selArea);

                // TODO: disabled for now
                /*ConservativeRasterizer.RasterizeQuadUniquely(
                    SelectedTexture.TexCoord0 / LevelTexture.FootStepSoundGranularity,
                    SelectedTexture.TexCoord1 / LevelTexture.FootStepSoundGranularity,
                    SelectedTexture.TexCoord2 / LevelTexture.FootStepSoundGranularity,
                    SelectedTexture.TexCoord3 / LevelTexture.FootStepSoundGranularity,
                    (startX, startY, endX, endY) =>
                    {
                        PointF tileStart = ToVisualCoord(new Vector2(startX, startY) * LevelTexture.FootStepSoundGranularity);
                        PointF tileEnd = ToVisualCoord(new Vector2(endX, endY) * LevelTexture.FootStepSoundGranularity);
                        RectangleF tileArea = RectangleF.FromLTRB(tileStart.X, tileStart.Y, tileEnd.X, tileEnd.Y);
                        e.Graphics.FillRectangle(_coverBrush, tileArea);
                    });*/

                base.OnPaintSelection(e);
            }
        }
    }
}
