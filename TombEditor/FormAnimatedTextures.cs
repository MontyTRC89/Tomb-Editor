using DarkUI.Forms;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using TombEditor.Geometry;

namespace TombEditor
{
    public partial class FormAnimatedTextures : DarkForm
    {
        private Editor _editor;
        private int _currentFrame = 0;

        public FormAnimatedTextures()
        {
            InitializeComponent();
        }

        private void butCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            this.Close();
        }

        private void FormSink_Load(object sender, EventArgs e)
        {
            _editor = Editor.Instance;

            picTextureMap.Image = _editor.Level.TextureMap;

            lstTextures.GetColumn(0).ImageGetter = delegate (object row)
            {
                RowAnimatedTexture obj = (RowAnimatedTexture)row;
                return obj.Texture;
            };

            ReloadTextureSets();

            comboItems.SelectedIndex = 0;
        }

        private void ReloadTextureSets()
        {
            while (comboItems.Items.Count > 1)
                comboItems.Items.RemoveAt(1);

            for (int i = 0; i < _editor.Level.AnimatedTextures.Count; i++)
            {
                comboItems.Items.Add("Animated Textures Set #" + (i + 1));
            }
        }

        private void butOK_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.OK;
            this.Close();
        }

        private void comboItems_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (comboItems.SelectedIndex > 0)
            {
                AnimatedTextureSet aSet = _editor.Level.AnimatedTextures[comboItems.SelectedIndex - 1];

                butAddNewTexture.Enabled = true;
                butDeleteTexture.Enabled = true;
                comboEffect.Enabled = true;
                lstTextures.ClearObjects();

                comboEffect.SelectedIndex = (int)aSet.Effect;

                for (int i = 0; i < imgList.Images.Count; i++)
                {
                    imgList.Images[i].Dispose();
                }

                imgList.Images.Clear();

                for (int i = 0; i < aSet.Textures.Count; i++)
                {
                    Bitmap bmp = Utils.GetTextureTileFromMap(aSet.Textures[i].X, aSet.Textures[i].Y, aSet.Textures[i].Page);
                    imgList.Images.Add(bmp);
                    RowAnimatedTexture row = new TombEditor.RowAnimatedTexture(aSet.Textures[i].X, aSet.Textures[i].Y,
                                                                               aSet.Textures[i].Page, i);
                    lstTextures.AddObject(row);
                }
            }
            else
            {
                butAddNewTexture.Enabled = false;
                butDeleteTexture.Enabled = false;
                comboEffect.Enabled = false;
                picTextureMap.IsTextureSelected = false;
                lstTextures.Items.Clear();
            }
        }

        private void butAddNew_Click(object sender, EventArgs e)
        {
            AnimatedTextureSet newSet = new Geometry.AnimatedTextureSet();
            _editor.Level.AnimatedTextures.Add(newSet);
            ReloadTextureSets();
            comboItems.SelectedIndex = comboItems.SelectedIndex - 1;
        }

        private void butDelete_Click(object sender, EventArgs e)
        {
            if (DarkMessageBox.ShowWarning("Do you really want to delete this animated texture set?",
                                           "Confirm", DarkDialogButton.YesNo) != DialogResult.Yes)
                return;
            _editor.Level.AnimatedTextures.RemoveAt(comboItems.SelectedIndex - 1);
            ReloadTextureSets();
            comboItems.SelectedIndex = 0;
        }

        private void butAddNewTexture_Click(object sender, EventArgs e)
        {
            if (!picTextureMap.IsTextureSelected)
                return;

            AnimatedTextureSet aSet = _editor.Level.AnimatedTextures[comboItems.SelectedIndex - 1];

            for (int i = 0; i < aSet.Textures.Count; i++)
            {
                if (aSet.Textures[i].X == picTextureMap.SelectedX && aSet.Textures[i].Y == picTextureMap.SelectedY &&
                    aSet.Textures[i].Page == picTextureMap.Page)
                    return;
            }

            Bitmap newTexture = Utils.GetTextureTileFromMap(picTextureMap.SelectedX, picTextureMap.SelectedY,
                                                            picTextureMap.Page);
            imgList.Images.Add(newTexture);

            _editor.Level.AnimatedTextures[comboItems.SelectedIndex - 1].Textures.Add(new AnimatedTexture(
                picTextureMap.SelectedX, picTextureMap.SelectedY, picTextureMap.Page));
            lstTextures.AddObject(new RowAnimatedTexture(picTextureMap.SelectedX, picTextureMap.SelectedY,
                picTextureMap.Page, imgList.Images.Count - 1));
        }

        private void butPlayAndStop_Click(object sender, EventArgs e)
        {
            timerPreview.Start();
        }

        private void timerPreview_Tick(object sender, EventArgs e)
        {
            if (imgList.Images.Count == 0)
                return;
            if (_currentFrame > imgList.Images.Count - 1)
                _currentFrame = 0;
            picPreview.Image = imgList.Images[_currentFrame];
            _currentFrame++;
        }

        private void lstTextures_Click(object sender, EventArgs e)
        {
            if (lstTextures.SelectedObject == null)
                return;

            RowAnimatedTexture obj = (RowAnimatedTexture)lstTextures.SelectedObject;

            picTextureMap.SelectedX = obj.X;
            picTextureMap.SelectedY = obj.Y;
            picTextureMap.Page = obj.Page;
            picTextureMap.IsTextureSelected = true;
            picTextureMap.Invalidate();
            picTextureMap.Refresh();
        }

        private void lstTextures_CellClick(object sender, BrightIdeasSoftware.CellClickEventArgs e)
        {

        }

        private void butDeleteTexture_Click(object sender, EventArgs e)
        {
            if (lstTextures.SelectedObject != null)
            {
                // imgList.Images.RemoveAt(lstTextures.SelectedItem.Index);
                _editor.Level.AnimatedTextures[comboItems.SelectedIndex - 1].Textures.RemoveAt(lstTextures.SelectedItem.Index);
                lstTextures.RemoveObject(lstTextures.SelectedObject);
            }
        }
    }
}
