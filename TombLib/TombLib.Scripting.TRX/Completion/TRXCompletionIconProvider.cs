using Nickelony.LanguageServer.Abstractions.Completion;
using System.Windows.Media;

namespace TombLib.Scripting.TRX.Completion;

internal static class TRXCompletionIconProvider
{
	private static readonly ImageSource? ArrayImage = new ImageSourceConverter()
		.ConvertFromString("pack://application:,,,/TombLib.Scripting.TRX;component/Resources/Icons/Array.png") as ImageSource;

	private static readonly ImageSource? ConstantImage = new ImageSourceConverter()
		.ConvertFromString("pack://application:,,,/TombLib.Scripting.TRX;component/Resources/Icons/Constant.png") as ImageSource;

	private static readonly ImageSource? PropertyImage = new ImageSourceConverter()
		.ConvertFromString("pack://application:,,,/TombLib.Scripting.TRX;component/Resources/Icons/Property.png") as ImageSource;

	public static ImageSource? GetImage(TextCompletionItem item)
		=> item.Kind switch
		{
			TextCompletionItemKind.Array => ArrayImage,
			TextCompletionItemKind.Property => PropertyImage,
			TextCompletionItemKind.Constant => ConstantImage,
			_ => null,
		};
}
