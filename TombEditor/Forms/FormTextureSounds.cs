using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Numerics;
using System.Windows.Forms;
using DarkUI.Forms;
using TombLib.LevelData;
using TombLib.Utils;
using Color = System.Drawing.Color;
using RectangleF = System.Drawing.RectangleF;

namespace TombEditor.Forms
{
    public partial class FormTextureSounds : DarkForm
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

        public FormTextureSounds(Editor _editor)
        {
            InitializeComponent();

            // Calculate the sizes at runtime since they actually depend on the choosen layout.
            // https://stackoverflow.com/questions/1808243/how-does-one-calculate-the-minimum-client-size-of-a-net-windows-form
            MinimumSize = new Size(440, 180) + (Size - ClientSize);

            // Initialize texture map
            if (_editor.SelectedTexture.TextureIsInvisble)
                textureMap.ResetVisibleTexture(_editor.Level.Settings.Textures.Count > 0 ? _editor.Level.Settings.Textures[0] : null);
            else
                textureMap.ShowTexture(_editor.SelectedTexture);

            // Add texture sounds to combo box
            foreach (TextureSound sound in Enum.GetValues(typeof(TextureSound)))
                comboSounds.Items.Add(sound);
        }

        private void butOk_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.OK;
            Close();
        }

        private void butAssignSound_Click(object sender, EventArgs e)
        {
            if (!(comboSounds.SelectedItem is TextureSound))
                return;

            var sound = (TextureSound)comboSounds.SelectedItem;
            ConservativeRasterizer.RasterizeQuad(
                textureMap.SelectedTexture.TexCoord0 / LevelTexture.TextureSoundGranularity,
                textureMap.SelectedTexture.TexCoord1 / LevelTexture.TextureSoundGranularity,
                textureMap.SelectedTexture.TexCoord2 / LevelTexture.TextureSoundGranularity,
                textureMap.SelectedTexture.TexCoord3 / LevelTexture.TextureSoundGranularity,
                (startX, startY, endX, endY) =>
                {
                    for (int y = startY; y < endY; ++y)
                        for (int x = startX; x < endX; ++x)
                            textureMap.VisibleTexture.SetTextureSound(x, y, sound);
                });
            textureMap.Invalidate();
        }

        public class PanelTextureMapForSounds : Controls.PanelTextureMap
        {
            protected override SelectionPrecisionType GetSelectionPrecision(bool rectangularSelection)
            {
                return new SelectionPrecisionType(LevelTexture.TextureSoundGranularity, true);
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
                int soundTileStartX = (int)Math.Floor(start.X / LevelTexture.TextureSoundGranularity);
                int soundTileStartY = (int)Math.Floor(start.Y / LevelTexture.TextureSoundGranularity);
                int soundTileEndX = (int)Math.Ceiling(end.X / LevelTexture.TextureSoundGranularity);
                int soundTileEndY = (int)Math.Ceiling(end.Y / LevelTexture.TextureSoundGranularity);

                // Draw texture sounds
                using (Font textureSoundFont = new Font(Font.FontFamily, _textureSoundStringSize * _textureSoundProportion * LevelTexture.TextureSoundGranularity * Math.Min(100, ViewScale)))
                    for (int y = soundTileStartY; y <= soundTileEndY; ++y)
                        for (int x = soundTileStartX; x <= soundTileEndX; ++x)
                        {
                            if (x < 0 || x >= texture.TextureSoundWidth || y < 0 || y >= texture.TextureSoundHeight)
                                continue;

                            TextureSound sound = texture.GetTextureSound(x, y);
                            Brush soundBrush = _textureSoundBrushes[(int)sound];

                            Vector2 tileStartTexCoord = new Vector2(x, y) * LevelTexture.TextureSoundGranularity;
                            PointF tileStart = ToVisualCoord(tileStartTexCoord);
                            PointF tileEnd = ToVisualCoord(tileStartTexCoord + new Vector2(LevelTexture.TextureSoundGranularity));
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
                ConservativeRasterizer.RasterizeQuadUniquely(
                    SelectedTexture.TexCoord0 / LevelTexture.TextureSoundGranularity,
                    SelectedTexture.TexCoord1 / LevelTexture.TextureSoundGranularity,
                    SelectedTexture.TexCoord2 / LevelTexture.TextureSoundGranularity,
                    SelectedTexture.TexCoord3 / LevelTexture.TextureSoundGranularity,
                    (startX, startY, endX, endY) =>
                    {
                        PointF tileStart = ToVisualCoord(new Vector2(startX, startY) * LevelTexture.TextureSoundGranularity);
                        PointF tileEnd = ToVisualCoord(new Vector2(endX, endY) * LevelTexture.TextureSoundGranularity);
                        RectangleF tileArea = RectangleF.FromLTRB(tileStart.X, tileStart.Y, tileEnd.X, tileEnd.Y);
                        e.Graphics.FillRectangle(_coverBrush, tileArea);
                    });

                base.OnPaintSelection(e);
            }
        }
    }
}
