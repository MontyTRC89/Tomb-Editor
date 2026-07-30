#nullable enable

using System;

namespace TombLib.Scripting.TRX.Models;

public class SchemaKeywords
{
	public string[] Collections { get; set; } = Array.Empty<string>();
	public string[] Properties { get; set; } = Array.Empty<string>();
	public string[] Constants { get; set; } = Array.Empty<string>();
}
