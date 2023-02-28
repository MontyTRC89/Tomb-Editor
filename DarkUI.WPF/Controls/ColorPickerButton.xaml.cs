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
		SelectedColorProperty = DependencyProperty.Register("SelectedColor", typeof(Color), typeof(ColorPickerButton), new PropertyMetadata(Colors.Transparent));
	}

	protected override void OnPropertyChanged(DependencyPropertyChangedEventArgs e)
	{
		base.OnPropertyChanged(e);

		if (e.Property.Name == nameof(SelectedColor))
			Background = new SolidColorBrush(SelectedColor);
	}
}
