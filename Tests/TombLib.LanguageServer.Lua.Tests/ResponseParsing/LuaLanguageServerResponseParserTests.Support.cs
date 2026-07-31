using System.Text.Json;

namespace Nickelony.LanguageServer.Lua.Tests;

public partial class LuaLanguageServerResponseParserTests
{
	private static CompletionItemPayload CreateCompletionItem(string label, int kind, string? detail, string? documentation, string? insertText = null)
		=> DeserializeCompletionItemPayload(new Dictionary<string, object?>
		{
			["label"] = label,
			["kind"] = kind,
			["detail"] = detail,
			["documentation"] = documentation,
			["insertText"] = insertText ?? label,
			["filterText"] = label
		});

	private static CompletionItemPayload DeserializeCompletionItemPayload(object payload)
		=> JsonSerializer.Deserialize<CompletionItemPayload>(JsonSerializer.Serialize(payload))
			?? throw new InvalidOperationException("Failed to deserialize the Lua completion-item test payload.");

	private static CompletionResponse? DeserializeCompletionResponse(object payload)
		=> JsonSerializer.Deserialize<CompletionResponse>(JsonSerializer.Serialize(payload));

	private static HoverResponse DeserializeHoverResponse(object payload)
		=> JsonSerializer.Deserialize<HoverResponse>(JsonSerializer.Serialize(payload))
			?? throw new InvalidOperationException("Failed to deserialize the hover response test payload.");

	private static SignatureHelpResponse? DeserializeSignatureHelpResponse(object payload)
		=> JsonSerializer.Deserialize<SignatureHelpResponse>(JsonSerializer.Serialize(payload));

	private static WorkspaceEditResponse? DeserializeWorkspaceEditResponse(object payload)
		=> JsonSerializer.Deserialize<WorkspaceEditResponse>(JsonSerializer.Serialize(payload));

	private static TextEditPayload[]? DeserializeTextEdits(object payload)
		=> JsonSerializer.Deserialize<TextEditPayload[]>(JsonSerializer.Serialize(payload));

	private static DefinitionResponse DeserializeDefinitionResponse(object payload)
		=> JsonSerializer.Deserialize<DefinitionResponse>(JsonSerializer.Serialize(payload))
			?? throw new InvalidOperationException("Failed to deserialize the definition response test payload.");

	private static ReferenceResponse[]? DeserializeReferenceResponse(object payload)
		=> JsonSerializer.Deserialize<ReferenceResponse[]>(JsonSerializer.Serialize(payload));
}
