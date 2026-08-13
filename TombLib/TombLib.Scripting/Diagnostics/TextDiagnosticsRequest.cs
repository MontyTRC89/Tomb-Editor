using System;

namespace TombLib.Scripting.Diagnostics;

/// <summary>
/// Describes a diagnostics request against an immutable document snapshot.
/// </summary>
public sealed record TextDiagnosticsRequest
{
	/// <summary>
	/// Initializes a new instance of the <see cref="TextDiagnosticsRequest"/> class.
	/// </summary>
	/// <param name="documentText">The current document snapshot text.</param>
	/// <param name="engineVersion">The engine version that should be used when evaluating diagnostics.</param>
	public TextDiagnosticsRequest(string documentText, Version engineVersion)
	{
		DocumentText = documentText;
		EngineVersion = engineVersion;
	}

	/// <summary>
	/// Gets the current document snapshot text.
	/// </summary>
	public string DocumentText { get; }

	/// <summary>
	/// Gets the engine version used when evaluating diagnostics.
	/// </summary>
	public Version EngineVersion { get; }
}
