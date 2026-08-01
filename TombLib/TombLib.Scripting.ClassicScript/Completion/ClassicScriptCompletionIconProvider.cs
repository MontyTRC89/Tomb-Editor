using System.Windows.Media;
using Nickelony.LanguageServer.Abstractions.Completion;

namespace TombLib.Scripting.ClassicScript.Completion;

internal static class ClassicScriptCompletionIconProvider
{
	private static readonly ImageSource? SectionImage = new ImageSourceConverter()
		.ConvertFromString("pack://application:,,,/TombLib.Scripting.ClassicScript;component/Resources/Icons/Section.png") as ImageSource;

	private static readonly ImageSource? OldCommandImage = new ImageSourceConverter()
		.ConvertFromString("pack://application:,,,/TombLib.Scripting.ClassicScript;component/Resources/Icons/OldCommand.png") as ImageSource;

	private static readonly ImageSource? NewCommandImage = new ImageSourceConverter()
		.ConvertFromString("pack://application:,,,/TombLib.Scripting.ClassicScript;component/Resources/Icons/NewCommand.png") as ImageSource;

	private static readonly ImageSource? ConstantImage = new ImageSourceConverter()
		.ConvertFromString("pack://application:,,,/TombLib.Scripting.ClassicScript;component/Resources/Icons/Constant.png") as ImageSource;

	private static readonly ImageSource? DirectiveImage = new ImageSourceConverter()
		.ConvertFromString("pack://application:,,,/TombLib.Scripting.ClassicScript;component/Resources/Icons/Directive.png") as ImageSource;

	public static ImageSource? GetImage(TextCompletionItem item) => item.Kind switch
	{
		TextCompletionItemKind.Section => SectionImage,
		TextCompletionItemKind.OldCommand => OldCommandImage,
		TextCompletionItemKind.NewCommand => NewCommandImage,
		TextCompletionItemKind.Directive => DirectiveImage,
		_ => ConstantImage,
	};
}
