namespace Nickelony.LanguageServer.Lua;

/// <summary>
/// Builds the client-capabilities payload advertised to the bundled Lua language server during initialization.
/// </summary>
internal static class LuaLanguageServerClientCapabilitiesFactory
{
	private static readonly string[] SupportedDocumentationFormats = ["markdown", "plaintext"];

	private static readonly string[] SupportedSemanticTokenTypes =
	[
		"namespace", "type", "class", "enum", "interface", "struct", "typeParameter",
		"parameter", "variable", "property", "enumMember", "event", "function", "method",
		"macro", "keyword", "modifier", "comment", "string", "number", "regexp",
		"operator", "decorator"
	];

	private static readonly string[] SupportedSemanticTokenModifiers =
	[
		"declaration", "definition", "readonly", "static", "deprecated", "abstract",
		"async", "modification", "documentation", "defaultLibrary", "global"
	];

	/// <summary>
	/// Builds LuaLS-specific client capability advertisement for the initialize request.
	/// </summary>
	/// <returns>An anonymous capabilities object serialized into the initialize request.</returns>
	internal static object Create()
	{
		return new
		{
			workspace = new
			{
				workspaceFolders = true,
				configuration = true,
				didChangeWatchedFiles = new { dynamicRegistration = false }
			},
			textDocument = new
			{
				completion = new
				{
					contextSupport = true,
					completionItem = new
					{
						snippetSupport = false,
						documentationFormat = SupportedDocumentationFormats,
						resolveSupport = new
						{
							properties = new[] { "detail", "documentation" }
						}
					}
				},
				hover = new
				{
					contentFormat = SupportedDocumentationFormats
				},
				definition = new
				{
					linkSupport = true
				},
				references = new
				{
					dynamicRegistration = false
				},
				rename = new
				{
					dynamicRegistration = false,
					prepareSupport = false
				},
				formatting = new
				{
					dynamicRegistration = false
				},
				publishDiagnostics = new
				{
					versionSupport = true
				},
				signatureHelp = new
				{
					signatureInformation = new
					{
						documentationFormat = SupportedDocumentationFormats,
						parameterInformation = new
						{
							labelOffsetSupport = true
						}
					},
					contextSupport = true
				},
				semanticTokens = new
				{
					requests = new
					{
						range = false,
						full = new { delta = true }
					},
					tokenTypes = SupportedSemanticTokenTypes,
					tokenModifiers = SupportedSemanticTokenModifiers,
					formats = new[] { "relative" },
					multilineTokenSupport = false,
					overlappingTokenSupport = false,
					augmentsSyntaxTokens = true
				}
			}
		};
	}
}
