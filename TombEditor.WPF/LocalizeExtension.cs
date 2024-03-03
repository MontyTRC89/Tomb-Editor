using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows.Data;
using System.Windows.Markup;

namespace TombEditor.WPF;

public class LocalizeExtension : MarkupExtension, IMultiValueConverter
{
	public string Key { get; set; }
	public bool FindHotkeys { get; set; }

	public LocalizeExtension(string key)
		=> Key = key;

	public override object ProvideValue(IServiceProvider serviceProvider)
	{
		string keyToUse = Key;
		var binding = new MultiBinding { Converter = this };

		binding.Bindings.Add(new Binding($"[{keyToUse}]")
		{
			Mode = BindingMode.OneWay,
			Source = Localizer.Instance,
		});

		if (FindHotkeys)
		{
			HotkeySets uiKeys = Editor.Instance.Configuration.UI_Hotkeys;

			if (uiKeys.Any(keyPair => keyPair.Key == keyToUse && keyPair.Value.Count > 0))
			{
				binding.Bindings.Add(new Binding($"[{keyToUse}]")
				{
					Mode = BindingMode.OneWay,
					Source = uiKeys,
				});
			}
		}
		
		return binding.ProvideValue(serviceProvider);
	}

	public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
		=> values.Length > 1 ? $"{values[0]} ({string.Join(", ", (SortedSet<Hotkey>)values[1])})" : values[0];

	public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
		=> throw new NotSupportedException();
}
