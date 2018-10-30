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

            // Calculate the sizes at runtime since they actually depend on the choosen layout.
            // https://stackoverflow.com/questions/1808243/how-does-one-calculate-the-minimum-client-size-of-a-net-windows-form
            MinimumSize = new Size(440, 180) + (Size - ClientSize);

            // Set position and size
            Size = _editor.Configuration.Window_FormFootStepSounds_Size;
            Location = _editor.Configuration.Window_FormFootStepSounds_Position;
            WindowState = _editor.Configuration.Window_FormFootStepSounds_Maximized ? FormWindowState.Maximized : FormWindowState.Normal;

            if (Location.X == -1000 && Location.Y == -1000)
                StartPosition = FormStartPosition.CenterParent;
            else
                StartPosition = FormStartPosition.Manual;

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
            ConservativeRasterizer.RasterizeQuad(
                textureMap.SelectedTexture.TexCoord0 / LevelTexture.FootStepSoundGranularity,
                textureMap.SelectedTexture.TexCoord1 / LevelTexture.FootStepSoundGranularity,
                textureMap.SelectedTexture.TexCoord2 / LevelTexture.FootStepSoundGranularity,
                textureMap.SelectedTexture.TexCoord3 / LevelTexture.FootStepSoundGranularity,
                (startX, startY, endX, endY) =>
                {
                    for (int y = startY; y < endY; ++y)
                        for (int x = startX; x < endX; ++x)
                            textureMap.VisibleTexture.SetFootStepSound(x, y, sound);
                });
            textureMap.Invalidate();
            _editor.TextureSoundsChange();
        }

        public class PanelTextureMapForSounds : Controls.PanelTextureMap
        {
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
                ConservativeRasterizer.RasterizeQuadUniquely(
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
                    });

                base.OnPaintSelection(e);
            }
        }

        protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
        {
            // Set position and size
            Editor.Instance.Configuration.Window_FormFootStepSounds_Size = Size;
            Editor.Instance.Configuration.Window_FormFootStepSounds_Position = Location;
            Editor.Instance.Configuration.Window_FormFootStepSounds_Maximized = WindowState == FormWindowState.Maximized;

            base.OnClosing(e);
        }
    }
}
