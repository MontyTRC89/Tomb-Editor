using System.Windows;
using System.Windows.Controls;

namespace DarkUI.WPF.Controls
{
	public class SearchableComboBox : ComboBox
	{
		public static readonly DependencyProperty IsPopupOpenProperty;
		public static readonly DependencyProperty SearchTextProperty;

		public bool IsPopupOpen
		{
			get => (bool)GetValue(IsPopupOpenProperty);
			set => SetValue(IsPopupOpenProperty, value);
		}

		public string SearchText
		{
			get => (string)GetValue(SearchTextProperty);
			set => SetValue(SearchTextProperty, value);
		}

		static SearchableComboBox()
		{
			IsPopupOpenProperty = DependencyProperty.Register(
				nameof(IsPopupOpen),
				typeof(bool),
				typeof(SearchableComboBox),
				new FrameworkPropertyMetadata(false, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

			SearchTextProperty = DependencyProperty.Register(
				nameof(SearchText),
				typeof(string),
				typeof(SearchableComboBox),
				new FrameworkPropertyMetadata(string.Empty, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));
		}
	}
}
