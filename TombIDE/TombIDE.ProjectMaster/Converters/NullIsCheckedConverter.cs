using System;
using System.Windows.Data;

namespace TombIDE.ProjectMaster.Converters;

public sealed class NullIsCheckedConverter : IValueConverter
{
	public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
		=> value is null;

	public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
		=> throw new NotImplementedException();
}
