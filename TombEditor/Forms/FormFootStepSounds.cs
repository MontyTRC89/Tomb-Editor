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
using System.Linq;

namespace TombEditor.Forms
{
    public partial class FormFootStepSounds : DarkForm
    {
        private const byte _alpha = 212;
        private static readonly Brush[] _textureSoundBrushes = new Brush[23]
        {
            new SolidBrush(Color.FromArgb(_alpha, 255, 188, 143)), // 0: Mud
            new SolidBrush(Color.FromArgb(_alpha, 220, 224, 250)), // 1: Snow
            new SolidBrush(Color.FromArgb(_alpha, 190, 190, 10)),  // 2: Sand
            new SolidBrush(Color.FromArgb(_alpha, 128, 128, 128)), // 3: Gravel
            new SolidBrush(Color.FromArgb(_alpha, 140, 170, 250)), // 4: Ice
            new SolidBrush(Color.FromArgb(_alpha, 40, 80, 230)),   // 5: Water
            new SolidBrush(Color.FromArgb(_alpha, 160, 160, 170)), // 6: Stone
            new SolidBrush(Color.FromArgb(_alpha, 222, 184, 135)), // 7: Wood
            new SolidBrush(Color.FromArgb(_alpha, 190, 180, 180)), // 8: Metal
            new SolidBrush(Color.FromArgb(_alpha, 244, 164, 96)),  // 9: Marble
            new SolidBrush(Color.FromArgb(_alpha, 34, 139, 34)),   // 10: Grass
            new SolidBrush(Color.FromArgb(_alpha, 112, 128, 144)), // 11: Concrete
            new SolidBrush(Color.FromArgb(_alpha, 111, 92, 67)),   // 12: Old Wood
            new SolidBrush(Color.FromArgb(_alpha, 205, 133, 63)),  // 12: OldWood
            new SolidBrush(Color.FromArgb(_alpha, 205, 133, 63)),  // 13: OldMetal
            new SolidBrush(Color.FromArgb(_alpha, 114, 222, 231)), // 14: Custom 1
            new SolidBrush(Color.FromArgb(_alpha, 139, 113, 255)), // 15: Custom 2
            new SolidBrush(Color.FromArgb(_alpha, 240, 128, 164)), // 16: Custom 3
            new SolidBrush(Color.FromArgb(_alpha, 249, 74, 92)),   // 17: Custom 4
            new SolidBrush(Color.FromArgb(_alpha, 238, 139, 91)),  // 18: Custom 5
            new SolidBrush(Color.FromArgb(_alpha, 114, 216, 129)), // 19: Custom 6
            new SolidBrush(Color.FromArgb(_alpha, 88, 241, 169)),  // 20: Custom 7
            new SolidBrush(Color.FromArgb(_alpha, 170, 80, 169))   // 21: Custom 8
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
            Configuration.ConfigureWindow(this, _editor.Configuration);

            // Populate texture list
            comboCurrentTexture.Items.AddRange(_editor.Level.Settings.Textures.ToArray());
            if (texture != null)
                comboCurrentTexture.SelectedItem = texture;
            else
                comboCurrentTexture.SelectedItem = _editor.Level.Settings.Textures.FirstOrDefault();

            // Add texture sounds to combo box
            foreach (var type in TextureFootStep.GetNames(editor.Level.Settings))
                comboSounds.Items.Add(type);
        }

        private void butOk_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.OK;
            Close();
        }

        private void butAssignSound_Click(object sender, EventArgs e)
        {
            var sound = (TextureFootStep.Type)comboSounds.SelectedIndex;

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
                    for (int y = soundTileStartY; y < soundTileEndY; ++y)
                        for (int x = soundTileStartX; x < soundTileEndX; ++x)
                        {
                            if (x < 0 || x >= texture.FootStepSoundWidth || y < 0 || y >= texture.FootStepSoundHeight)
                                continue;

                            var sound = texture.GetFootStepSound(x, y);
                            Brush soundBrush = _textureSoundBrushes[(int)sound];

                            Vector2 tileStartTexCoord = new Vector2(x, y) * LevelTexture.FootStepSoundGranularity;
                            PointF tileStart = ToVisualCoord(tileStartTexCoord);
                            PointF tileEnd = ToVisualCoord(tileStartTexCoord + new Vector2(LevelTexture.FootStepSoundGranularity));
                            PointF descStart = new PointF(tileStart.X, tileStart.Y * _textureSoundProportion + tileEnd.Y * (1 - _textureSoundProportion));

                            RectangleF descArea = RectangleF.FromLTRB(descStart.X, descStart.Y, tileEnd.X, tileEnd.Y);
                            e.Graphics.FillRectangle(soundBrush, descArea);
                            if(ViewScale > 6)
                                e.Graphics.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAliasGridFit;
                            e.Graphics.DrawString(sound.ToString().SplitCamelcase(), textureSoundFont, Brushes.Black, descArea, _textureSoundStringFormat);

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

                base.OnPaintSelection(e);
            }
        }

        private void comboCurrentTexture_SelectedValueChanged(object sender, EventArgs e)
        {
            if (textureMap.VisibleTexture != comboCurrentTexture.SelectedItem)
            {
                textureMap.ResetVisibleTexture(comboCurrentTexture.SelectedItem as LevelTexture);
                textureMap.SelectedTexture = TextureArea.None;
            }
        }

        private void comboCurrentTexture_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void comboSounds_SelectedIndexChanged(object sender, EventArgs e)
        {

        }
    }
}
