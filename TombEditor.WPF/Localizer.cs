using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Reflection;
using System.Text.Json;
using System.Windows;
using System.Windows.Input;

namespace TombEditor.WPF
{
	public sealed class DynamicKeyBinding : KeyBinding
	{
		public static readonly DependencyProperty InputGestureTextProperty = DependencyProperty.Register(nameof(InputGestureText), typeof(string), typeof(DynamicKeyBinding));

		public string? InputGestureText
		{
			get => (string)GetValue(InputGestureTextProperty);
			set => SetValue(InputGestureTextProperty, value);
		}

		private readonly KeyGestureValueSerializer _serializer = new();

		protected override void OnPropertyChanged(DependencyPropertyChangedEventArgs e)
		{
			if (e.Property == InputGestureTextProperty && e.NewValue is string text)
			{
				if (string.IsNullOrEmpty(text))
					Gesture = new KeyGesture(Key.NoName);
				else
					Gesture = (KeyGesture)_serializer.ConvertFromString(text, null);
			}

			base.OnPropertyChanged(e);
		}
	}

	public class Localizer : INotifyPropertyChanged
	{
		public static Localizer Instance { get; set; } = new Localizer();

		private const string IndexerName = "Item";
		private const string IndexerArrayName = "Item[]";

		private Dictionary<string, string>? Strings;

		public bool LoadLanguage(string language)
		{
			Language = language;
			Stream? resource = Assembly.GetExecutingAssembly().GetManifestResourceStream($"{GetType().Namespace}.Assets.i18n.{language}.json");

			if (resource is not null)
			{
				using var reader = new StreamReader(resource);
				Strings = JsonSerializer.Deserialize<Dictionary<string, string>>(reader.ReadToEnd(), new JsonSerializerOptions { ReadCommentHandling = JsonCommentHandling.Skip });

				Invalidate();
				return true;
			}

			return false;
		}

		public string? Language { get; private set; }

		public string this[string key]
		{
			get
			{
				if (Strings is not null && Strings.TryGetValue(key, out string? value))
					return value.Replace("\\n", "\n");

				return $"{Language}:{key}";
			}
		}

		public event PropertyChangedEventHandler? PropertyChanged;

		public void Invalidate()
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(IndexerName));
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(IndexerArrayName));
		}
	}
}
