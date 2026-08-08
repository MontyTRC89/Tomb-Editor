using System;

namespace TombLib.Scripting.UI.Bases;

/// <summary>
/// A plain text editor with no language-specific services.
/// </summary>
public sealed class PlainTextEditor : TextEditorBase
{
	/// <inheritdoc/>
	public override string DefaultFileExtension => ".txt";

	/// <summary>
	/// Initializes a new instance of the <see cref="PlainTextEditor"/> class.
	/// </summary>
	/// <param name="engineVersion">The engine version the editor targets.</param>
	public PlainTextEditor(Version engineVersion) : base(engineVersion)
	{ }
}
