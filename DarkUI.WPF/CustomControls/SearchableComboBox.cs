﻿using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;

namespace DarkUI.WPF.CustomControls
{
	[TemplatePart(Name = "PART_SearchTextBox", Type = typeof(TextBox))]
	[TemplatePart(Name = "PART_SearchButton", Type = typeof(Button))]
	public class SearchableComboBox : ComboBox
	{
		public static readonly DependencyProperty SearchTextProperty;

		public string SearchText
		{
			get => (string)GetValue(SearchTextProperty);
			set => SetValue(SearchTextProperty, value);
		}

		/// <summary>
		/// Setting this to <see langword="true"/> in the <c>KeyDown</c> event will keep the drop down open when the user presses ENTER.
		/// <para>NOTE: Focus will be shifted onto the control's ItemsPresenter.</para>
		/// </summary>
		public bool PerformedSpecialAction { get; set; }

		/// <summary>
		/// Determines whether search is being performed by the user.
		/// </summary>
		public bool IsPerformingSearch { get; set; }

		public TextBox? SearchTextBox { get; set; }
		public Button? SearchButton { get; set; }

		static SearchableComboBox()
		{
			SearchTextProperty = DependencyProperty.Register(
				nameof(SearchText),
				typeof(string),
				typeof(SearchableComboBox),
				new FrameworkPropertyMetadata(string.Empty, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));
		}

		public override void OnApplyTemplate()
		{
			base.OnApplyTemplate();

			SearchTextBox = GetTemplateChild("PART_SearchTextBox") as TextBox;
			SearchButton = GetTemplateChild("PART_SearchButton") as Button;

			if (SearchTextBox is not null)
				SearchTextBox.KeyDown += SearchTextBox_KeyDown;

			if (SearchButton is not null)
				SearchButton.Click += SearchButton_Click;
		}

		protected override void OnPropertyChanged(DependencyPropertyChangedEventArgs e)
		{
			base.OnPropertyChanged(e);

			if (e.Property.Name == nameof(SearchText) && IsPerformingSearch && string.IsNullOrEmpty(SearchText))
				ClearFilter();

			if (e.Property.Name == nameof(IsDropDownOpen) && PerformedSpecialAction)
			{
				IsDropDownOpen = true;
				PerformedSpecialAction = false;
			}
			else if (e.Property.Name == nameof(IsDropDownOpen) && !IsDropDownOpen && Items.Filter is not null)
				ClearFilter();
			else if (e.Property.Name == nameof(IsDropDownOpen) && !IsDropDownOpen)
				SearchText = string.Empty;
		}

		protected override void OnKeyDown(KeyEventArgs e)
		{
			if (e.Key is not Key.Enter and not Key.Tab
				and not Key.Up and not Key.Down
				and not Key.Left and not Key.Right)
			{
				var textBox = Template.FindName("PART_SearchTextBox", this) as TextBox;

				if (textBox is not null)
					Dispatcher.BeginInvoke(() => textBox.Focus(), DispatcherPriority.Loaded);
			}

			base.OnKeyDown(e);
		}

		private void SearchTextBox_KeyDown(object sender, KeyEventArgs e)
		{
			if (e.Key is Key.Enter)
			{
				FindNext();
				PerformedSpecialAction = true;
			}
		}

		private void SearchButton_Click(object sender, RoutedEventArgs e)
			=> FindNext();

		public void FindNext()
		{
			if (string.IsNullOrWhiteSpace(SearchText))
			{
				if (IsPerformingSearch)
					ClearFilter();

				return;
			}

			bool containsExactMatches = Items.Cast<object>().Any(item =>
			{
				string itemString = item.ToString()!.Replace("System.Windows.Controls.ComboBoxItem: ", ""); // TODO: Change this
				return itemString.Contains(SearchText, StringComparison.OrdinalIgnoreCase);
			});

			Items.Filter = item =>
			{
				string itemString = item.ToString()!.Replace("System.Windows.Controls.ComboBoxItem: ", ""); // TODO: Change this
				bool exactlyMatches = itemString.Contains(SearchText, StringComparison.OrdinalIgnoreCase);

				if (containsExactMatches)
					return exactlyMatches;
				else
				{
					bool levenshteinValid = Levenshtein.DistanceSubstring(itemString.ToLower(), SearchText.ToLower(), out _) < 2;
					return exactlyMatches || levenshteinValid;
				}
			};

			if (!IsPerformingSearch || SelectedIndex + 1 >= Items.Count)
				SelectedIndex = 0;
			else
				SelectedIndex++;

			IsPerformingSearch = true;
		}

		public void ClearFilter(bool clearSearchText = false)
		{
			Items.Filter = null;

			if (clearSearchText)
				SearchText = string.Empty;

			IsPerformingSearch = false;
		}
	}
}
