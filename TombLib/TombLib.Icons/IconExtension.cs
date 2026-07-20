using System;
using System.Windows.Markup;
using System.Windows.Media;

namespace TombLib.Icons;

/// XAML markup extension so views can write {tomb:Icon Actions/Play}.
public sealed class IconExtension : MarkupExtension
{
    public string Path { get; set; } = "";

    public IconExtension() { }
    public IconExtension(string path) { Path = path; }

    public override object ProvideValue(IServiceProvider serviceProvider)
        => IconSources.Load(Path);
}
