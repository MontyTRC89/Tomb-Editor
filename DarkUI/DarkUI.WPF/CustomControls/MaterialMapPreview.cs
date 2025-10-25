using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace DarkUI.WPF.CustomControls;

/// <summary>
/// A custom control for displaying material map previews with a checker pattern background.
/// Displays an image preview with a standardized checker pattern background for transparency visualization.
/// </summary>
public class MaterialMapPreview : Control
{
	#region Dependency Properties

	/// <summary>
	/// The image source to display in the preview.
	/// </summary>
	public static readonly DependencyProperty ImageSourceProperty =
		DependencyProperty.Register(
			nameof(ImageSource),
			typeof(ImageSource),
			typeof(MaterialMapPreview),
			new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsRender));

	/// <summary>
	/// The stretch mode for the image.
	/// </summary>
	public static readonly DependencyProperty StretchProperty =
		DependencyProperty.Register(
			nameof(Stretch),
			typeof(Stretch),
			typeof(MaterialMapPreview),
			new FrameworkPropertyMetadata(Stretch.Uniform, FrameworkPropertyMetadataOptions.AffectsRender));

	/// <summary>
	/// The stretch direction for the image.
	/// </summary>
	public static readonly DependencyProperty StretchDirectionProperty =
		DependencyProperty.Register(
			nameof(StretchDirection),
			typeof(StretchDirection),
			typeof(MaterialMapPreview),
			new FrameworkPropertyMetadata(StretchDirection.Both, FrameworkPropertyMetadataOptions.AffectsRender));

	#endregion Dependency Properties

	#region Properties

	/// <summary>
	/// Gets or sets the image source to display in the preview.
	/// </summary>
	public ImageSource? ImageSource
	{
		get => (ImageSource?)GetValue(ImageSourceProperty);
		set => SetValue(ImageSourceProperty, value);
	}

	/// <summary>
	/// Gets or sets the stretch mode for the image.
	/// </summary>
	public Stretch Stretch
	{
		get => (Stretch)GetValue(StretchProperty);
		set => SetValue(StretchProperty, value);
	}

	/// <summary>
	/// Gets or sets the stretch direction for the image.
	/// </summary>
	public StretchDirection StretchDirection
	{
		get => (StretchDirection)GetValue(StretchDirectionProperty);
		set => SetValue(StretchDirectionProperty, value);
	}

	#endregion Properties

	#region Constructor

	static MaterialMapPreview()
	{
		DefaultStyleKeyProperty.OverrideMetadata(typeof(MaterialMapPreview), new FrameworkPropertyMetadata(typeof(MaterialMapPreview)));
	}

	#endregion Constructor
}
