using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Schema;
using NLog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using TombLib.Scripting.TRX.Models;

namespace TombLib.Scripting.TRX.Services;

/// <summary>
/// Loads and serves the GameFlow level schema used for schema-driven completion and validation.
/// </summary>
public sealed class GameFlowSchemaService : IGameFlowSchemaService
{
	private static readonly Logger Log = LogManager.GetCurrentClassLogger();

	/// <inheritdoc />
	public JSchema? Schema { get; }

	/// <summary>
	/// Initializes a new instance of the <see cref="GameFlowSchemaService"/> class.
	/// </summary>
	/// <param name="schemaFilePath">The path of the schema file to load.</param>
	public GameFlowSchemaService(string schemaFilePath)
	{
		try
		{
			string schemaContent = File.ReadAllText(schemaFilePath);
			Schema = JSchema.Parse(schemaContent);
		}
		catch (Exception exception)
		{
			Log.Warn(exception, "Failed to load the GameFlow schema from '{Path}'; schema-aware features are disabled.", schemaFilePath);
			Schema = null;
		}
	}

	/// <inheritdoc />
	public SchemaKeywords? GetSchemaKeywords()
	{
		var schema = Schema;

		if (schema is null)
			return null;

		var collections = new HashSet<string>();
		var properties = new HashSet<string>();
		var constants = new HashSet<string>();

		// Extract from the main schema
		ExtractKeywordsRecursively(schema, collections, properties, constants);

		// Also extract from definitions section
		if (schema.ExtensionData?.ContainsKey("definitions") == true)
		{
			var definitions = schema.ExtensionData["definitions"] as JObject;

			if (definitions is not null)
			{
				foreach (var definition in definitions)
				{
					var defSchema = definition.Value?.ToObject<JSchema>();

					if (defSchema is not null)
						ExtractKeywordsRecursively(defSchema, collections, properties, constants);
				}
			}
		}

		return new SchemaKeywords
		{
			Collections = collections.ToArray(),
			Properties = properties.ToArray(),
			Constants = constants.ToArray()
		};
	}

	private static void ExtractKeywordsRecursively(JSchema schema, HashSet<string> collections, HashSet<string> properties, HashSet<string> constants)
	{
		foreach (JSchema currentSchema in SchemaTraversal.FlattenSchemas(schema))
		{
			// Extract const values at any level
			if (currentSchema.Const is not null && currentSchema.Const.Type == JTokenType.String)
				constants.Add(currentSchema.Const.ToString());

			// Extract enum values at any level
			if (currentSchema.Enum is not null)
			{
				foreach (var enumValue in currentSchema.Enum)
				{
					if (enumValue.Type == JTokenType.String)
						constants.Add(enumValue.ToString());
				}
			}

			// Classify the schema's own properties
			if (currentSchema.Properties is not null)
			{
				foreach (var property in currentSchema.Properties)
				{
					if (property.Value.Type == JSchemaType.Array)
						collections.Add(property.Key);
					else
						properties.Add(property.Key);
				}
			}
		}
	}
}
