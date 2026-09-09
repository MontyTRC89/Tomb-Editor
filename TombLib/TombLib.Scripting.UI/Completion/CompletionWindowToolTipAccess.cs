using ICSharpCode.AvalonEdit.CodeCompletion;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Windows.Controls;

namespace TombLib.Scripting.UI.Completion;

/// <summary>
/// Provides version-guarded access to the completion window's private tooltip field.
/// AvalonEdit's <see cref="CompletionWindow"/> does not expose its tooltip through a public API,
/// so the field is resolved lazily and defensively: if a future AvalonEdit version renames or
/// removes the field, <see cref="TryGetToolTip"/> returns false and callers degrade gracefully.
/// </summary>
internal static class CompletionWindowToolTipAccess
{
	private static readonly Lazy<FieldInfo?> ToolTipField = new(() =>
		typeof(CompletionWindow).GetField("toolTip", BindingFlags.NonPublic | BindingFlags.Instance));

	/// <summary>
	/// Attempts to resolve the completion window's tooltip.
	/// </summary>
	/// <param name="completionWindow">The completion window whose tooltip is requested.</param>
	/// <param name="toolTip">The resolved tooltip when available.</param>
	/// <returns><see langword="true"/> when the tooltip could be resolved; otherwise, <see langword="false"/>.</returns>
	public static bool TryGetToolTip(CompletionWindow completionWindow, [NotNullWhen(true)] out ToolTip? toolTip)
	{
		ArgumentNullException.ThrowIfNull(completionWindow);

		toolTip = ToolTipField.Value?.GetValue(completionWindow) as ToolTip;
		return toolTip is not null;
	}
}
