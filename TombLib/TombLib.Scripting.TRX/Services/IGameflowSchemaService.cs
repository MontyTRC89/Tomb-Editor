using Newtonsoft.Json.Schema;
using TombLib.Scripting.TRX.Models;

namespace TombLib.Scripting.TRX.Services;

/// <summary>
/// Loads and serves the GameFlow level schema used for schema-driven completion and validation.
/// </summary>
public interface IGameFlowSchemaService
{
	/// <summary>
	/// Gets the loaded schema, or null when the schema could not be loaded.
	/// </summary>
	JSchema? Schema { get; }

	/// <summary>
	/// Gets the schema-derived keywords, or null when the schema could not be loaded.
	/// </summary>
	SchemaKeywords? GetSchemaKeywords();
}
