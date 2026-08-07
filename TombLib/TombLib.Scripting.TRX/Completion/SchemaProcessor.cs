using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Schema;
using Nickelony.LanguageServer.Abstractions.Completion;
using System.Linq;

namespace TombLib.Scripting.TRX.Completion;

/// <summary>
/// Handles recursive schema processing and data extraction.
/// </summary>
public sealed class SchemaProcessor
{
	private readonly CompletionDataBuilder _builder;

	/// <summary>
	/// Initializes a new instance of the <see cref="SchemaProcessor"/> class.
	/// </summary>
	/// <param name="builder">The completion data builder that receives extracted data.</param>
	public SchemaProcessor(CompletionDataBuilder builder)
		=> _builder = builder;

	/// <summary>
	/// Processes the given schema, extracting completion data from all reachable schemas.
	/// </summary>
	/// <param name="schema">The schema to process.</param>
	public void ProcessSchema(JSchema schema)
	{
		foreach (JSchema currentSchema in SchemaTraversal.FlattenSchemas(schema))
		{
			ExtractProperties(currentSchema);
			ExtractEnumValues(currentSchema);
			ExtractConstValue(currentSchema);
		}
	}

	/// <summary>
	/// Processes only the top-level properties of the given schema.
	/// </summary>
	/// <param name="schema">The schema to process.</param>
	public void ProcessTopLevelPropertiesOnly(JSchema schema)
		=> ExtractProperties(schema);

	private void ExtractProperties(JSchema schema)
	{
		if (schema.Properties is null)
			return;

		foreach (var property in schema.Properties)
		{
			var text = $"\"{property.Key}\": ";
			var completionKind = GetCompletionKind(property.Value);
			var description = property.Value.Description ?? $"Property: {property.Key}";

			_builder.TryAdd(text, completionKind, description);
		}
	}

	private void ExtractEnumValues(JSchema schema)
	{
		if (schema.Enum?.Count > 0)
		{
			var stringEnums = schema.Enum
				.Where(e => e.Type == JTokenType.String)
				.Select(e => $"\"{e}\"");

			foreach (var enumValue in stringEnums)
				_builder.TryAdd(enumValue, TextCompletionItemKind.Constant);
		}
	}

	private void ExtractConstValue(JSchema schema)
	{
		if (schema.Const?.Type == JTokenType.String)
		{
			var constValue = $"\"{schema.Const}\"";
			_builder.TryAdd(constValue, TextCompletionItemKind.Constant);
		}
	}

	private static TextCompletionItemKind GetCompletionKind(JSchema schema) => schema.Type switch
	{
		JSchemaType.Array => TextCompletionItemKind.Array,
		_ => TextCompletionItemKind.Property
	};
}
