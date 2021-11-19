using System;
using System.Drawing;
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
    public partial class FormBumpMaps : DarkForm
    {
        private static readonly Brush[] _bumpBrushes = new Brush[]
        {
            new SolidBrush(Color.FromArgb(200, 160, 160, 160)), // 0: None
            new SolidBrush(Color.FromArgb(200, 235, 200, 120)), // 1: Level 1
            new SolidBrush(Color.FromArgb(200, 245, 180, 100)), // 2: Level 2
            new SolidBrush(Color.FromArgb(200, 255, 160,  80))  // 3: Level 3
        };

        private static readonly Brush _coverBrush = new SolidBrush(Color.FromArgb(128, 15, 15, 200));
        private const float _bumpStringSize = 0.4f;
        private const float _bumpProportion = 1.0f / 4.0f;
        private const string _noCustomMapMessage = "custom file not selected";

        private Editor _editor;

        public FormBumpMaps(Editor editor, LevelTexture texture)
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

            // Add bumpmaps to combo box
            foreach (BumpMappingLevel bump in Enum.GetValues(typeof(BumpMappingLevel)))
                cmbBump.Items.Add(bump);
            cmbBump.SelectedIndex = 0;

            UpdateDialog();
        }

        private void UpdateDialog()
        {
            if (textureMap.VisibleTexture != null)
            {
                bool isCustomMap = !String.IsNullOrEmpty(textureMap.VisibleTexture.BumpPath);
                cmbBump.Enabled = !isCustomMap;
                butAssignBumpmap.Enabled = !isCustomMap;
                cbUseCustomFile.Checked = isCustomMap;
                lblCustomMapPath.Text = (isCustomMap ? _editor.Level.Settings.MakeAbsolute(textureMap.VisibleTexture.BumpPath) : _noCustomMapMessage);
            }
            else
            {
                cmbBump.Enabled = false;
                butAssignBumpmap.Enabled = false;
                cbUseCustomFile.Enabled = false;
            }
        }

        private void SwitchCustomBumpmap()
        {
            if (textureMap.VisibleTexture != null)
            {
                var currentTexture = textureMap.VisibleTexture;

                if (cbUseCustomFile.Checked)
                    currentTexture.BumpPath = null;
                else
                {
                    using (OpenFileDialog dialog = new OpenFileDialog())
                    {
                        dialog.Multiselect = false;
                        dialog.Filter = ImageC.FromFileFileExtensions.GetFilter();
                        dialog.Title = "Open custom bump/normal map image";

                        if (!string.IsNullOrWhiteSpace(currentTexture.Path))
                            dialog.InitialDirectory = _editor.Level?.Settings?.MakeAbsolute(currentTexture.Path) ?? currentTexture.Path;

                        if (dialog.ShowDialog(this) != DialogResult.OK)
                            currentTexture.BumpPath = null;
                        else
                        {
                            var tempImage = ImageC.FromFile(dialog.FileName);
                            if (tempImage.Size != currentTexture.Image.Size)
                            {
                                DarkMessageBox.Show(this, "Selected image file has different size. Please select image with size similar to original texture file.", "Wrong image size", MessageBoxIcon.Error);
                                currentTexture.BumpPath = null;
                            }
                            else
                                currentTexture.BumpPath = _editor.Level?.Settings?.MakeRelative(dialog.FileName, VariableType.LevelDirectory);
                        }
                    }
                }
            }

            UpdateDialog();
        }

        private void butOk_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.OK;
            Close();
        }

        private void butAssignBumpmap_Click(object sender, EventArgs e)
        {
            if (!(cmbBump.SelectedItem is BumpMappingLevel))
                return;

            var bump = (BumpMappingLevel)cmbBump.SelectedItem;

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
                    textureMap.VisibleTexture.SetBumpMappingLevel(x, y, bump);

            textureMap.Invalidate();
            _editor.BumpmapsChange();
        }

        public class PanelTextureMapForBumpmaps : Controls.PanelTextureMap
        {
            protected override SelectionPrecisionType GetSelectionPrecision(bool rectangularSelection)
            {
                return new SelectionPrecisionType(LevelTexture.BumpMappingGranularity, true);
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
                int bumpTileStartX = (int)Math.Floor(start.X / LevelTexture.BumpMappingGranularity);
                int bumpTileStartY = (int)Math.Floor(start.Y / LevelTexture.BumpMappingGranularity);
                int bumpTileEndX = (int)Math.Ceiling(end.X / LevelTexture.BumpMappingGranularity);
                int bumpTileEndY = (int)Math.Ceiling(end.Y / LevelTexture.BumpMappingGranularity);

                // Draw bumpmaps
                using (Font bumpFont = new Font(Font.FontFamily, _bumpStringSize * _bumpProportion * LevelTexture.BumpMappingGranularity * Math.Min(100, ViewScale)))
                    for (int y = bumpTileStartY; y < bumpTileEndY; ++y)
                        for (int x = bumpTileStartX; x < bumpTileEndX; ++x)
                        {
                            if (x < 0 || x >= texture.BumpMappingWidth || y < 0 || y >= texture.BumpMappingHeight)
                                continue;

                            BumpMappingLevel bump = texture.GetBumpMapLevel(x, y);

                            Vector2 tileStartTexCoord = new Vector2(x, y) * LevelTexture.BumpMappingGranularity;
                            PointF tileStart = ToVisualCoord(tileStartTexCoord);
                            PointF tileEnd = ToVisualCoord(tileStartTexCoord + new Vector2(LevelTexture.BumpMappingGranularity));
                            PointF descStart = new PointF(tileStart.X, tileStart.Y * _bumpProportion + tileEnd.Y * (1 - _bumpProportion));

                            RectangleF descArea = RectangleF.FromLTRB(descStart.X, descStart.Y, tileEnd.X, tileEnd.Y);
                            e.Graphics.FillRectangle(_bumpBrushes[(int)bump], descArea);
                            if(ViewScale > 6)
                                e.Graphics.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAliasGridFit;
                            e.Graphics.DrawString(bump.ToString(), bumpFont, Brushes.Black, descArea, new StringFormat(StringFormatFlags.NoWrap)
                                { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center });

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

        private void cbUseCustomFile_MouseDown(object sender, MouseEventArgs e)
        {
            SwitchCustomBumpmap();
        }

        private void lblCustomMapPath_Click(object sender, EventArgs e)
        {
            SwitchCustomBumpmap();
        }

        private void comboCurrentTexture_SelectedValueChanged(object sender, EventArgs e)
        {
            if (textureMap.VisibleTexture != comboCurrentTexture.SelectedItem)
            {
                textureMap.ResetVisibleTexture(comboCurrentTexture.SelectedItem as LevelTexture);
                textureMap.SelectedTexture = TextureArea.None;
                UpdateDialog();
            }
        }
    }
}
