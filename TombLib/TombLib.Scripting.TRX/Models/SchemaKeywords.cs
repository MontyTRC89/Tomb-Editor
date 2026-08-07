using System;

namespace TombLib.Scripting.TRX.Models;

/// <summary>
/// Holds the keyword categories derived from the GameFlow schema.
/// </summary>
public class SchemaKeywords
{
	/// <summary>
	/// Gets or sets the collection keywords.
	/// </summary>
	public string[] Collections { get; set; } = [];

	/// <summary>
	/// Gets or sets the property keywords.
	/// </summary>
	public string[] Properties { get; set; } = [];

	/// <summary>
	/// Gets or sets the constant keywords.
	/// </summary>
	public string[] Constants { get; set; } = [];
}
