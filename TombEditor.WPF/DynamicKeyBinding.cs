using System.Windows;
using System.Windows.Forms;
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

		protected override void OnPropertyChanged(DependencyPropertyChangedEventArgs e)
		{
			if (e.Property == InputGestureTextProperty && e.NewValue is string text)
			{
				if (string.IsNullOrEmpty(text))
					Gesture = new KeyGesture(Key.NoName);
				else
				{
					var hotkey = Hotkey.FromString(text);

					ModifierKeys modifiers = ModifierKeys.None;
					Key key = Key.NoName;

					if ((hotkey.Keys & Keys.Control) != Keys.None)
						modifiers |= ModifierKeys.Control;

					if ((hotkey.Keys & Keys.Shift) != Keys.None)
						modifiers |= ModifierKeys.Shift;

					if ((hotkey.Keys & Keys.Alt) != Keys.None)
						modifiers |= ModifierKeys.Alt;

					if (hotkey.MainKey != Keys.None)
						key = KeyInterop.KeyFromVirtualKey((int)hotkey.MainKey);

					Modifiers = modifiers;
					Key = key;
				}
			}

			base.OnPropertyChanged(e);
		}
	}
}
