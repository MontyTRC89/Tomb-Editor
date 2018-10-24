using System;
using System.Drawing;
using System.Numerics;
using System.Windows.Forms;
using DarkUI.Forms;
using TombLib.LevelData;
using TombLib.Utils;
using Color = System.Drawing.Color;
using RectangleF = System.Drawing.RectangleF;

namespace TombEditor.Forms
{
    public partial class FormBumpMaps : DarkForm
    {
        private static readonly Brush[] _bumpBrushes = new Brush[]
        {
            new SolidBrush(Color.FromArgb(200, 160, 160, 160)), // 0: None
            new SolidBrush(Color.FromArgb(200, 235, 200, 120)), // 1: Level 1
            new SolidBrush(Color.FromArgb(200, 235, 200, 100)), // 2: Level 2
            new SolidBrush(Color.FromArgb(200, 235, 200,  80))  // 3: Level 3
        };
        private static readonly Brush _coverBrush = new SolidBrush(Color.FromArgb(128, 15, 15, 200));
        private const float _bumpStringSize = 0.4f;
        private const float _bumpProportion = 1.0f / 4.0f;
        private const string _noCustomMapMessage = "[ custom file not selected ]";

        private Editor _editor;

        public FormBumpMaps(Editor editor, LevelTexture texture)
        {
            InitializeComponent();
            _editor = editor;

            // Calculate the sizes at runtime since they actually depend on the choosen layout.
            // https://stackoverflow.com/questions/1808243/how-does-one-calculate-the-minimum-client-size-of-a-net-windows-form
            MinimumSize = new Size(440, 180) + (Size - ClientSize);

            // Initialize texture map
            if (editor.SelectedTexture.TextureIsInvisble)
                textureMap.ResetVisibleTexture(texture);
            else
                textureMap.ShowTexture(editor.SelectedTexture);

            // Add bumpmaps to combo box
            foreach (BumpMappingLevel bump in Enum.GetValues(typeof(BumpMappingLevel)))
                cmbBump.Items.Add(bump);
            cmbBump.SelectedIndex = 0;

            UpdateDialog();
        }

        private void UpdateDialog()
        {
            if(textureMap.VisibleTexture != null && textureMap.VisibleTexture is LevelTexture)
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
            ConservativeRasterizer.RasterizeQuad(

                textureMap.SelectedTexture.TexCoord0 / LevelTexture.BumpMappingGranularity,
                textureMap.SelectedTexture.TexCoord1 / LevelTexture.BumpMappingGranularity,
                textureMap.SelectedTexture.TexCoord2 / LevelTexture.BumpMappingGranularity,
                textureMap.SelectedTexture.TexCoord3 / LevelTexture.BumpMappingGranularity,
                (startX, startY, endX, endY) =>
                {
                    for (int y = startY; y < endY; ++y)
                        for (int x = startX; x < endX; ++x)
                            textureMap.VisibleTexture.SetBumpMappingLevel(x, y, bump);
                });
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
                    for (int y = bumpTileStartY; y <= bumpTileEndY; ++y)
                        for (int x = bumpTileStartX; x <= bumpTileEndX; ++x)
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
                ConservativeRasterizer.RasterizeQuadUniquely(
                    SelectedTexture.TexCoord0 / LevelTexture.BumpMappingGranularity,
                    SelectedTexture.TexCoord1 / LevelTexture.BumpMappingGranularity,
                    SelectedTexture.TexCoord2 / LevelTexture.BumpMappingGranularity,
                    SelectedTexture.TexCoord3 / LevelTexture.BumpMappingGranularity,
                    (startX, startY, endX, endY) =>
                    {
                        PointF tileStart = ToVisualCoord(new Vector2(startX, startY) * LevelTexture.BumpMappingGranularity);
                        PointF tileEnd = ToVisualCoord(new Vector2(endX, endY) * LevelTexture.BumpMappingGranularity);
                        RectangleF tileArea = RectangleF.FromLTRB(tileStart.X, tileStart.Y, tileEnd.X, tileEnd.Y);
                        e.Graphics.FillRectangle(_coverBrush, tileArea);
                    });

                base.OnPaintSelection(e);
            }
        }

        private void cbUseCustomFile_MouseDown(object sender, MouseEventArgs e)
        {
            if (textureMap.VisibleTexture != null && textureMap.VisibleTexture is LevelTexture)
            {
                var currentTexture = textureMap.VisibleTexture as LevelTexture;

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
    }
}
