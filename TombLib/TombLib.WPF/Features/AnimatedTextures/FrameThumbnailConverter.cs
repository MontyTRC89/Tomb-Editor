#nullable enable

using System;
using System.Globalization;
using System.Windows.Data;
using TombLib.LevelData;

namespace TombLib.WPF.Features.AnimatedTextures
{
    /// <summary>Renders an <see cref="AnimatedTextureFrame"/> as a small perspective thumbnail for the frames grid.</summary>
    public sealed class FrameThumbnailConverter : IValueConverter
    {
        public object? Convert(object value, Type targetType, object parameter, CultureInfo culture)
            => value is AnimatedTextureFrame frame ? AnimatedTexturesWindowViewModel.RenderFrame(frame, 32) : null;

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotSupportedException();
    }
}
