using System;
using TombLib.Utils;

namespace TombLib.WPF.Features.AnimatedTextures
{
    /// <summary>
    /// Abstraction over the WinForms texture-map control used by the animated-textures editor. The
    /// control itself lives in TombLib.Forms (which references TombLib.WPF), so it cannot be referenced
    /// directly here; the host app wraps its concrete control in this adapter and the WPF window hosts
    /// <see cref="Control"/> via a WindowsFormsHost.
    /// </summary>
    public interface ITextureMapAdapter
    {
        System.Windows.Forms.Control Control { get; }
        TextureArea SelectedTexture { get; }
        Texture VisibleTexture { get; }
        void ShowTexture(TextureArea area);
        void ResetVisibleTexture(Texture texture);
        void Invalidate();
        event EventHandler DoubleClick;
    }
}
