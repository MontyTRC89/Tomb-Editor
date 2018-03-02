using DarkUI.Forms;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using TombLib.Graphics;
using TombLib.Utils;
using TombLib.Wad;
using TombLib.Wad.Catalog;

namespace WadTool
{
    public partial class FormSpriteSequenceEditor : DarkForm
    {
        public WadSpriteSequence SpriteSequence { get; set; }

        private WadToolClass _tool;
        private Dictionary<Hash, WadSprite> _loadedSprites;
        private List<WadSprite> _currentSprites;

        public FormSpriteSequenceEditor(WadToolClass tool, Wad2 wad, WadSpriteSequence spriteSequence)
        {
            _tool = tool;
            SpriteSequence = spriteSequence;

            InitializeComponent();

            openFileDialogSprites.Filter = ImageC.FromFileFileExtensions.GetFilter();

            foreach (var slot in TrCatalog.GetAllSpriteSequences(_tool.DestinationWad.SuggestedGameVersion))
                comboSlot.Items.Add(slot.Value);

            /*var spritesCatalog = TrCatalog.GetAllSprites(_tool.DestinationWad.Version);

            for (int i = 0; i < spritesCatalog.Count; i++)
            {
                if (spritesCatalog.ElementAt(i).Key == SpriteSequence.Id)
                {
                    comboSlot.SelectedIndex = i;
                    break;
                }
            }

            if (comboSlot.SelectedIndex == -1) comboSlot.SelectedIndex = 0;

            // Load sprites
            _loadedSprites = new Dictionary<Hash, WadSprite>();
            _currentSprites = new List<WadSprite>();
            foreach (var sprite in SpriteSequence.Sprites)
            {
                _loadedSprites.Add(sprite.Hash, sprite);
                _currentSprites.Add(sprite);
            }

            ReloadSprites();*/
            int COMMENT;
        }

        private void lstSprites_MouseClick(object sender, MouseEventArgs e)
        {
            /*if (lstSprites.SelectedIndices.Count == 0) return;

            // Get the current sprite
            var item = lstSprites.Items[lstSprites.SelectedIndices[0]];
            var sprite = (WadSprite)item.Tag;

            // Dispose old bitmap and set the new one
            UnloadCurrentSprite();
            picSprite.Image = sprite.Image.ToBitmap();*/
            int COMMENT;
        }

        private void ReloadSprites()
        {
            lstSprites.Items.Clear();
            UnloadCurrentSprite();

            for (int i = 0; i < _currentSprites.Count; i++)
            {
                var item = new DarkUI.Controls.DarkListItem("Sprite #" + i);
                item.Tag = _currentSprites[i];
                lstSprites.Items.Add(item);
            }
        }

        private void UnloadCurrentSprite()
        {
            if (picSprite.Image != null)
            {
                picSprite.Image.Dispose();
                picSprite.Image = null;
            }
        }

        private void butAddNewTexture_Click(object sender, EventArgs e)
        {
            /*if (openFileDialogSprites.ShowDialog(this) == DialogResult.Cancel) return;

            var sprite = new WadSprite();
            var image = ImageC.FromFile(openFileDialogSprites.FileName);

            sprite.Image = image;
            sprite.UpdateHash();

            if (_loadedSprites.ContainsKey(sprite.Hash))
                sprite = _loadedSprites[sprite.Hash];
            else
                _loadedSprites.Add(sprite.Hash, sprite);

            _currentSprites.Add(sprite);*/
            int COMMENT;

            ReloadSprites();
        }

        private void butDeleteSprite_Click(object sender, EventArgs e)
        {
            if (lstSprites.SelectedIndices.Count == 0) return;

            // Ask to the user the permission to delete sprite
            /*if (DarkMessageBox.Show(this,
                   "Are you really sure to delete sprite #" + lstSprites.SelectedIndices[0] + "?",
                   "Delete sprite", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
                return;

            // Get the current sprite
            var item = lstSprites.Items[lstSprites.SelectedIndices[0]];
            var sprite = (WadSprite)item.Tag;

            _currentSprites.RemoveAt(lstSprites.SelectedIndices[0]);

            bool found = false;
            for (int i = 0; i < _currentSprites.Count; i++)
            {
                if (i != lstSprites.SelectedIndices[0] && _currentSprites[i].Hash == sprite.Hash)
                {
                    found = true;
                    break;
                }
            }

            if (!found)
                _loadedSprites.Remove(sprite.Hash);*/
            int COMMENTED;

            ReloadSprites();
        }

        private void butSaveChanges_Click(object sender, EventArgs e)
        {
            /*uint objectId = (uint)TrCatalog.GetAllSprites(_tool.DestinationWad.Version).ElementAt(comboSlot.SelectedIndex).Key;

            // Check for already existing sequence
            if (objectId != SpriteSequence.Id)
            {
                foreach (var seq in _tool.DestinationWad.SpriteSequences)
                    if (seq.Id == objectId)
                    {
                        DarkMessageBox.Show(this, "The selected slot is already assigned to another sprite sequence", "Error", MessageBoxIcon.Error);
                        return;
                    }
            }

            UnloadCurrentSprite();

            SpriteSequence.Id = objectId;

            // Add sprites
            SpriteSequence.Sprites.Clear();
            foreach (var sprite in _currentSprites)
            {
                if (_tool.DestinationWad.SpriteTextures.ContainsKey(sprite.Hash))
                {
                    SpriteSequence.Sprites.Add(_tool.DestinationWad.SpriteTextures[sprite.Hash]);
                }
                else
                {
                    sprite.DirectXTexture = TextureLoad.Load(_tool.Device, sprite.Image);
                    _tool.DestinationWad.SpriteTextures.Add(sprite.Hash, sprite);
                    SpriteSequence.Sprites.Add(sprite);
                }
            }*/

            DialogResult = DialogResult.OK;
            Close();
        }
    }
}
