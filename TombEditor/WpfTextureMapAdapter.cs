using System;
using System.Windows.Forms;
using TombLib.Controls;
using TombLib.Utils;
using TombLib.WPF.Features.AnimatedTextures;

namespace TombEditor
{
    /// <summary>
    /// Bridges the WinForms <see cref="TextureMapBase"/> control to the WPF animated-textures editor's
    /// <see cref="ITextureMapAdapter"/> (TombLib.WPF cannot reference TombLib.Forms directly).
    /// </summary>
    public sealed class WpfTextureMapAdapter : ITextureMapAdapter
    {
        private readonly TextureMapBase _map;

        public WpfTextureMapAdapter(TextureMapBase map) => _map = map;

        public Control Control => _map;
        public TextureArea SelectedTexture => _map.SelectedTexture;
        public Texture VisibleTexture => _map.VisibleTexture;
        public void ShowTexture(TextureArea area) => _map.ShowTexture(area);
        public void ResetVisibleTexture(Texture texture) => _map.ResetVisibleTexture(texture);
        public void Invalidate() => _map.Invalidate();

        public event EventHandler DoubleClick
        {
            add => _map.DoubleClick += value;
            remove => _map.DoubleClick -= value;
        }
    }
}
