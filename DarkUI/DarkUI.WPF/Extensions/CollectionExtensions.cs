using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace DarkUI.WPF.Extensions;

public static class CollectionExtensions
{
	public static ObservableCollection<T> ToObservableCollection<T>(this IEnumerable<T> source)
		=> source is not null
			? new ObservableCollection<T>(source)
			: throw new ArgumentNullException(nameof(source));
}
