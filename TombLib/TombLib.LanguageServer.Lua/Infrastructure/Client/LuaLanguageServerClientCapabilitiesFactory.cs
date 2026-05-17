namespace TombLib.LanguageServer.Lua;

public static class LuaLanguageServerClientCapabilitiesFactory
{
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
	public static object Create()
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
						documentationFormat = new[] { "markdown", "plaintext" },
						resolveSupport = new
						{
							properties = new[] { "detail", "documentation" }
						}
					}
				},
				hover = new
				{
					contentFormat = new[] { "markdown", "plaintext" }
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
						documentationFormat = new[] { "markdown", "plaintext" },
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
