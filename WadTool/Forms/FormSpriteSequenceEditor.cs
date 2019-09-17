using DarkUI.Controls;
using DarkUI.Forms;
using NLog;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using TombLib.Utils;
using TombLib.Wad;

namespace WadTool
{
    public partial class FormSpriteSequenceEditor : DarkForm
    {
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();

        public WadSpriteSequence SpriteSequence { get; }
        public Wad2 Wad { get; }

        private string _currentPath;
        private readonly Cache<WadTexture, Bitmap> _imageCache = new Cache<WadTexture, Bitmap>(1024, sprite => sprite.Image.ToBitmap());

        public FormSpriteSequenceEditor(Wad2 wad, WadSpriteSequence spriteSequence)
        {
            InitializeComponent();

            SpriteSequence = spriteSequence;
            Wad = wad;
            _currentPath = Wad.FileName;

            // Load data
            Text = "Sprite sequence '" + spriteSequence.Id.ToString(Wad.SuggestedGameVersion) + "'";
            dataGridView.DataSource = new BindingList<WadSprite>(new List<WadSprite>(spriteSequence.Sprites));
            dataGridViewControls.CreateNewRow = newObject;
            dataGridViewControls.DataGridView = dataGridView;
            dataGridViewControls.Enabled = true;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing && components != null)
            {
                _imageCache.Dispose();
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        private object newObject()
        {
            using (var fileDialog = new OpenFileDialog())
            {
                fileDialog.Filter = ImageC.FromFileFileExtensions.GetFilter();
                fileDialog.Multiselect = true;
                if (!string.IsNullOrWhiteSpace(_currentPath))
                    try
                    {
                        fileDialog.InitialDirectory = Path.GetDirectoryName(_currentPath);
                        fileDialog.FileName = Path.GetFileName(_currentPath);
                    }
                    catch { }
                fileDialog.Title = "Select sprite files that you want to see imported.";

                DialogResult dialogResult = fileDialog.ShowDialog(this);
                _currentPath = fileDialog.FileName;
                if (dialogResult != DialogResult.OK)
                    return null;

                // Load sprites
                List<WadSprite> sprites = new List<WadSprite>();
                foreach (string fileName in fileDialog.FileNames)
                {
                Retry:
                    ;
                    try
                    {
                        sprites.Add(new WadSprite { Texture = new WadTexture(ImageC.FromFile(fileName)) });
                    }
                    catch (Exception exc)
                    {
                        logger.Error(exc, "Unable to open file '" + fileName + "'.");
                        switch (DarkMessageBox.Show(this, "Unable to load sprite from file '" + fileName + "'. " + exc, "Unable to load sprite.",
                            fileDialog.FileNames.Length == 1 ? MessageBoxButtons.RetryCancel : MessageBoxButtons.AbortRetryIgnore, MessageBoxIcon.Error,
                            fileDialog.FileNames.Length == 1 ? MessageBoxDefaultButton.Button2 : MessageBoxDefaultButton.Button1))
                        {
                            case DialogResult.Ignore:
                                continue;
                            case DialogResult.Retry:
                                goto Retry;
                        }
                        return null;
                    }
                }
                return sprites;
            }
        }

        private void btExport_Click(object sender, EventArgs e)
        {
            using (var fileDialog = new SaveFileDialog())
            {
                fileDialog.Filter = ImageC.SaveFileFileExtensions.GetFilter();
                if (!string.IsNullOrWhiteSpace(_currentPath))
                    try
                    {
                        fileDialog.InitialDirectory = Path.GetDirectoryName(_currentPath);
                        fileDialog.FileName = Path.GetFileName(_currentPath);
                    }
                    catch { }
                fileDialog.Title = "Choose a sprite file name.";
                fileDialog.AddExtension = true;

                DialogResult dialogResult = fileDialog.ShowDialog(this);
                _currentPath = fileDialog.FileName;
                if (dialogResult != DialogResult.OK)
                    return;

                // Save sprites
                try
                {
                    foreach (DataGridViewRow row in dataGridView.SelectedRows)
                    {
                        string fileName = fileDialog.FileName;
                        if (dataGridView.SelectedRows.Count > 1)
                            fileName = Path.Combine(Path.GetDirectoryName(fileName),
                                Path.GetFileNameWithoutExtension(fileName) + row.Index.ToString("0000") + Path.GetExtension(fileName));
                        ((WadSprite)row.DataBoundItem).Texture.Image.Save(fileName);
                    }
                }
                catch (Exception exc)
                {
                    logger.Error(exc, "Unable to save file '" + fileDialog.FileName + "'.");
                    DarkMessageBox.Show(this, "Unable to save sprite. " + exc, "Saving sprite failed.", MessageBoxIcon.Error);
                }
            }
        }

        private void dataGridView_SelectionChanged(object sender, EventArgs e)
        {
            btExport.Enabled = dataGridView.SelectedRows.Count > 0;
        }

        private void dataGridView_Click(object sender, EventArgs e)
        {
            // Update big image view
            if (dataGridView.SelectedRows.Count <= 0)
                return;

            picSprite.Image = _imageCache[((WadSprite)dataGridView.SelectedRows[0].DataBoundItem).Texture];
        }

        private void dataGridView_CellFormattingSafe(object sender, DarkDataGridViewSafeCellFormattingEventArgs e)
        {
            if (!(e.Row.DataBoundItem is WadSprite))
                return;
            WadSprite item = (WadSprite)e.Row.DataBoundItem;

            if (e.Column.Name == SizeColumn.Name)
                e.Value = item.Texture.Image.Size.ToString();
            else if (e.Column.Name == IdColumn.Name)
                e.Value = e.RowIndex.ToString();
            else if (e.Column.Name == PreviewColumn.Name)
                e.Value = _imageCache[item.Texture];
            e.FormattingApplied = true;
        }

        private void btCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }

        private void btOk_Click(object sender, EventArgs e)
        {
            // Update data
            SpriteSequence.Sprites.Clear();
            SpriteSequence.Sprites.AddRange((IEnumerable<WadSprite>)dataGridView.DataSource);

            // Close
            DialogResult = DialogResult.OK;
            Close();
        }

        private void ButReplaceSprite_Click(object sender, EventArgs e)
        {
            if (dataGridView.SelectedRows.Count == 0)
                return;
            var row = dataGridView.SelectedRows[0];

            using (var fileDialog = new OpenFileDialog())
            {
                fileDialog.Filter = ImageC.FromFileFileExtensions.GetFilter();
                fileDialog.Multiselect = false;
                if (!string.IsNullOrWhiteSpace(_currentPath))
                    try
                    {
                        fileDialog.InitialDirectory = Path.GetDirectoryName(_currentPath);
                        fileDialog.FileName = Path.GetFileName(_currentPath);
                    }
                    catch { }
                fileDialog.Title = "Select sprite file that you want to see imported.";

                DialogResult dialogResult = fileDialog.ShowDialog(this);
                _currentPath = fileDialog.FileName;
                if (dialogResult != DialogResult.OK)
                    return;

                // Load sprites
                WadSprite sprite;
                try
                {
                    sprite = new WadSprite { Texture = new WadTexture(ImageC.FromFile(fileDialog.FileName)) };
                    dataGridView.EditableRowCollection[row.Index] = sprite;
                    dataGridView.Refresh();
                    dataGridView.Invalidate();
                }
                catch (Exception exc)
                {
                    logger.Error(exc, "Unable to open file '" + fileDialog.FileName + "'.");
                    DarkMessageBox.Show(this, "Unable to load sprite from file '" + fileDialog.FileName + "'. " + exc, "Unable to load sprite",
                                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
            }
                      
        }
    }
}
