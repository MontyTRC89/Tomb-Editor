using System;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;

namespace DarkUI.WPF
{
	public static class Constants
	{
		public const double BasicCornerRadiusPrim = 4;
		public static CornerRadius BasicCornerRadius = new(BasicCornerRadiusPrim);

		public const double GroupCornerRadiusPrim = 8;
		public static CornerRadius GroupCornerRadius = new(GroupCornerRadiusPrim);

		public const double BasicOneLineHeight = 24;

		public const double ButtonPaddingX = 10;
		public const double ButtonPaddingY = 3;

		public static readonly TreeListViewItemMarginConverter TreeListViewItemFirstMarginConverter = new()
		{
			IsFirstColumn = true,
			Length = 19
		};

		public static readonly TreeListViewItemMarginConverter TreeListViewItemMarginConverter = new()
		{
			Length = 19
		};

		public static readonly IndentToMarginConverter LengthConverter = new();

		public static Brush CheckerBrush => _CheckerBrush ??= (Brush)Application.Current.TryFindResource("CheckerBrush");

		private static Brush? _CheckerBrush;
	}

	public class TreeListViewItemMarginConverter : IValueConverter
	{
		public double Length { get; set; }
		public bool IsFirstColumn { get; set; }

		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			if (value is not TreeViewItem item)
				return new Thickness(0);

			double right = Length * (item.GetDepth() + 1.0);
			double left = IsFirstColumn ? -6.0 : right * -1.0;

			return new Thickness(left, 0, right, 0);
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			throw new NotImplementedException();
		}
	}

	public class IndentToMarginConverter : IMultiValueConverter
	{
		public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
		{
			if (values is null)
				return new Thickness(0);

			if (values[0] is not TreeViewItem item)
				return new Thickness(0);

			double length = 19.0;

			if (values.Length == 2)
				if (values[1] is double v)
					length = v;

			double k = length * item.GetDepth();

			if (k == 0)
				return new Thickness(0);

			if (_thicknessCache.TryGetValue(k, out object? result) == false)
			{
				result = new Thickness(k, 0, 0, 0);

				_thicknessCache.Add(k, result);
			}

			return result;
		}

		private static readonly Dictionary<double, object> _thicknessCache = new(8);

		public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
		{
			throw new NotImplementedException();
		}
	}

	internal static class TreeViewItemExtensions
	{
		internal static int GetDepth(this TreeViewItem item)
		{
			try
			{
				TreeViewItem? parent = GetParent(item);

				while (parent is not null)
					return GetDepth(parent) + 1;
			}
			catch { }

			return 0;
		}

		private static TreeViewItem? GetParent(TreeViewItem item)
		{
			DependencyObject parent = VisualTreeHelper.GetParent(item);

			while (parent is not TreeViewItem or TreeView)
				parent = VisualTreeHelper.GetParent(parent ?? throw new InvalidOperationException());

			return parent as TreeViewItem;
		}
	}

	public static class AlternationExtensions
	{
		private static readonly MethodInfo? SetAlternationIndexMethod;

		static AlternationExtensions()
			=> SetAlternationIndexMethod = typeof(ItemsControl).GetMethod("SetAlternationIndex", BindingFlags.Static | BindingFlags.NonPublic);

		public static int SetAlternationIndexRecursively(this ItemsControl control, int firstAlternationIndex)
		{
			int alternationCount = control.AlternationCount;

			if (alternationCount == 0)
				return 0;

			foreach (object? item in control.Items)
			{
				if (control.ItemContainerGenerator.ContainerFromItem(item) is TreeViewItem container)
				{
					int nextAlternation = firstAlternationIndex++ % alternationCount;
					SetAlternationIndexMethod?.Invoke(null, new object[] { container, nextAlternation });

					if (container.IsExpanded)
						firstAlternationIndex = SetAlternationIndexRecursively(container, firstAlternationIndex);
				}
			}

			return firstAlternationIndex;
		}
	}
}
