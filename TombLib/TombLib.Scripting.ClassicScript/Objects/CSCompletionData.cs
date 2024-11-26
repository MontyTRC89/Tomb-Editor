using ICSharpCode.AvalonEdit.CodeCompletion;
using ICSharpCode.AvalonEdit.Document;
using ICSharpCode.AvalonEdit.Editing;
using System;
using System.Windows.Media;

namespace TombLib.Scripting.ClassicScript.Objects
{
	public enum CompletionType
	{
		Section,
		OldCommand,
		NewCommand,
		Constant,
		Directive
	}

	public sealed class CSCompletionData : ICompletionData
	{
		private static readonly ImageSource _sectionImage = new ImageSourceConverter()
			.ConvertFromString("pack://application:,,,/TombLib.Scripting.ClassicScript;component/Resources/Icons/Section.png") as ImageSource;

		private static readonly ImageSource _oldCommandImage = new ImageSourceConverter()
			.ConvertFromString("pack://application:,,,/TombLib.Scripting.ClassicScript;component/Resources/Icons/OldCommand.png") as ImageSource;

		private static readonly ImageSource _newCommandImage = new ImageSourceConverter()
			.ConvertFromString("pack://application:,,,/TombLib.Scripting.ClassicScript;component/Resources/Icons/NewCommand.png") as ImageSource;

		private static readonly ImageSource _constantImage = new ImageSourceConverter()
			.ConvertFromString("pack://application:,,,/TombLib.Scripting.ClassicScript;component/Resources/Icons/Constant.png") as ImageSource;

		private static readonly ImageSource _directiveImage = new ImageSourceConverter()
			.ConvertFromString("pack://application:,,,/TombLib.Scripting.ClassicScript;component/Resources/Icons/Directive.png") as ImageSource;

		public CSCompletionData(string text, CompletionType type)
		{
			Text = text;

			Image = type switch
			{
				CompletionType.Section => _sectionImage,
				CompletionType.OldCommand => _oldCommandImage,
				CompletionType.NewCommand => _newCommandImage,
				CompletionType.Constant => _constantImage,
				CompletionType.Directive => _directiveImage,
				_ => null,
			};
		}

		public ImageSource Image { get; }

		public string Text { get; }
		public object Content => Text;
		public object Description => null;
		public double Priority => 0;

		public void Complete(TextArea textArea, ISegment completionSegment, EventArgs insertionRequestEventArgs)
			=> textArea.Document.Replace(completionSegment, Text);
	}
}
