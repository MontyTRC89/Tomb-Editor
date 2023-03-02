using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace DarkUI.WPF.Controls;

public class ColorPickerButton : Button
{
	public static readonly DependencyProperty SelectedColorProperty;

	public Color SelectedColor
	{
		get => (Color)GetValue(SelectedColorProperty);
		set => SetValue(SelectedColorProperty, value);
	}

	static ColorPickerButton()
	{
		SelectedColorProperty = DependencyProperty.Register(
			nameof(SelectedColor),
			typeof(Color),
			typeof(ColorPickerButton),
			new FrameworkPropertyMetadata(Colors.Transparent, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));
	}

	protected override void OnPropertyChanged(DependencyPropertyChangedEventArgs e)
	{
		base.OnPropertyChanged(e);

		if (e.Property.Name == nameof(SelectedColor))
			Background = new SolidColorBrush(SelectedColor);
	}
}
