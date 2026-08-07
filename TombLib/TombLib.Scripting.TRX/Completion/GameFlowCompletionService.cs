using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Schema;
using Nickelony.LanguageServer.Abstractions.Completion;
using NLog;
using System;
using System.Collections.Generic;
using TombLib.Scripting.Completion;
using TombLib.Scripting.TRX.Resources;
using TombLib.Scripting.TRX.Services;

namespace TombLib.Scripting.TRX.Completion;

/// <summary>
/// Builds completion items from the GameFlow JSON schema.
/// </summary>
public sealed class GameFlowCompletionService : ITextCompletionProvider
{
	private static readonly Logger Log = LogManager.GetCurrentClassLogger();

	private readonly IGameFlowSchemaService _schemaService;

	/// <summary>
	/// Initializes a new instance of the <see cref="GameFlowCompletionService"/> class.
	/// </summary>
	/// <param name="schemaService">The schema service used to source the GameFlow schema.</param>
	public GameFlowCompletionService(IGameFlowSchemaService schemaService)
		=> _schemaService = schemaService;

	/// <inheritdoc />
	public IReadOnlyList<TextCompletionItem> GetCompletionItems(TextCompletionContext context)
	{
		try
		{
			var completionBuilder = new CompletionDataBuilder();
			var schema = _schemaService.Schema;

			if (schema is not null && schema.Properties is not null)
			{
				// Process schema data
				ProcessTopLevelProperties(completionBuilder, schema);
				ProcessDefinitions(completionBuilder, schema);
				AddJsonPrimitives(completionBuilder);
			}

			return TextCompletionFilter.FilterByCurrentWord(completionBuilder.Build(), context);
		}
		catch (Exception exception)
		{
			Log.Warn(exception, "Failed to build GameFlow completion items; returning an empty list.");
			return [];
		}
	}

	private static void ProcessTopLevelProperties(CompletionDataBuilder builder, JSchema schema)
	{
		var schemaProcessor = new SchemaProcessor(builder);
		schemaProcessor.ProcessTopLevelPropertiesOnly(schema);
	}

	private static void ProcessDefinitions(CompletionDataBuilder builder, JSchema schema)
	{
		if (schema.ExtensionData?.ContainsKey("definitions") != true)
			return;

		if (schema.ExtensionData["definitions"] is not JObject definitions)
			return;

		foreach (var definition in definitions)
		{
			var defSchema = definition.Value?.ToObject<JSchema>();

			if (defSchema is null)
				continue;

			var schemaProcessor = new SchemaProcessor(builder);
			schemaProcessor.ProcessSchema(defSchema);
		}
	}

	private static void AddJsonPrimitives(CompletionDataBuilder builder)
	{
		foreach (string primitive in Keywords.Values)
			builder.TryAdd(primitive, TextCompletionItemKind.Constant);
	}
}
