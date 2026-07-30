#nullable enable

using System;

namespace TombLib.Scripting.UI.Bases;

public sealed class PlainTextEditor : TextEditorBase
{
	public override string DefaultFileExtension => ".txt";

	public PlainTextEditor(Version engineVersion) : base(engineVersion)
	{
	}
}
