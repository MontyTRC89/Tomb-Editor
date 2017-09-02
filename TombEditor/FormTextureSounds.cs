using DarkUI.Forms;

namespace TombEditor
{
    public class FormTextureSounds : DarkForm
    { }
    /*public partial class FormTextureSounds : DarkForm
    {
        private LevelTexture _texture;

        public FormTextureSounds(LevelTexture texture)
        {
            _texture = texture;
            InitializeComponent();
            picTextureMap.ContainerForm = this;
        }

        private void butCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            this.Close();
        }

        private void FormTextureSounds_Load(object sender, EventArgs e)
        {
            picTextureMap.Image = _editor.Level.TextureMap;
            comboSounds.SelectedIndex = 6;
        }

        private void butOK_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.OK;
            this.Close();
        }

        public void SelectTexture()
        {
            int idTexture = GetTextureSound();

            if (idTexture == -1)
            {
                comboSounds.SelectedIndex = 6;
            }
            else
            {
                comboSounds.SelectedIndex = (int)_editor.Level.TextureSounds[idTexture].Sound;
            }
        }

        private int GetTextureSound()
        {
            for (int i = 0; i < _editor.Level.TextureSounds.Count; i++)
            {
                TextureSound txtSound = _editor.Level.TextureSounds[i];

                if (txtSound.X == picTextureMap.SelectedX && txtSound.Y == picTextureMap.SelectedY && txtSound.Page == picTextureMap.Page)
                    return i;
            }

            return -1;
        }

        private void butAssignSound_Click(object sender, EventArgs e)
        {
            int idTexture = GetTextureSound();

            if (idTexture == -1)
            {
                TextureSound txtSound = new TextureSound(picTextureMap.SelectedX, picTextureMap.SelectedY, picTextureMap.Page);
                txtSound.Sound = (TextureSounds)comboSounds.SelectedIndex;
                _editor.Level.TextureSounds.Add(txtSound);
                picTextureMap.Invalidate();
                picTextureMap.Refresh();
            }
            else
            {
                _editor.Level.TextureSounds[idTexture].Sound = (TextureSounds)comboSounds.SelectedIndex;
                picTextureMap.Invalidate();
                picTextureMap.Refresh();
            }
        }
    }*/
}
