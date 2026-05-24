#nullable enable

using System;
using System.Collections.Generic;
using System.Linq;

namespace TombLib.Scripting.Specifications.ClassicScript.Syntaxes;

public sealed class ClassicScriptSyntaxCatalogService
{
	private readonly ClassicScriptSyntaxDefinitionsLoader _loader;
	private readonly Lazy<ClassicScriptSyntaxCatalogSnapshot> _snapshot;

	public ClassicScriptSyntaxCatalogService()
		: this(new ClassicScriptSyntaxDefinitionsLoader())
	{
	}

	internal ClassicScriptSyntaxCatalogService(ClassicScriptSyntaxDefinitionsLoader loader)
	{
		_loader = loader ?? throw new ArgumentNullException(nameof(loader));
		_snapshot = new Lazy<ClassicScriptSyntaxCatalogSnapshot>(LoadSnapshot);
	}

	public IReadOnlyList<ClassicScriptSyntaxDefinition> GetCommandSyntaxDefinitions()
		=> _snapshot.Value.CommandDefinitions;

	public ClassicScriptSyntaxDefinition? GetCommandDefinition(string key)
		=> GetDefinition(_snapshot.Value.CommandDefinitionsByKey, key);

	public string? GetCommandSyntax(string key)
		=> GetCommandDefinition(key)?.SyntaxText;

	public string? GetCustomizeSyntax(string key)
		=> GetDefinition(_snapshot.Value.CustomizeSyntaxesByKey, key)?.SyntaxText;

	public string? GetParameterSyntax(string key)
		=> GetDefinition(_snapshot.Value.ParameterSyntaxesByKey, key)?.SyntaxText;

	private static ClassicScriptSyntaxDefinition? GetDefinition(
		IReadOnlyDictionary<string, ClassicScriptSyntaxDefinition> definitions,
		string key)
	{
		if (string.IsNullOrWhiteSpace(key))
			return null;

		return definitions.TryGetValue(key, out ClassicScriptSyntaxDefinition? definition) ? definition : null;
	}

	private ClassicScriptSyntaxCatalogSnapshot LoadSnapshot()
	{
		IReadOnlyList<ClassicScriptSyntaxDefinition> customizeSyntaxes = _loader.Load(ClassicScriptResourcePaths.GetSyntaxPath("CustSyntaxes.resx"));
		IReadOnlyList<ClassicScriptSyntaxDefinition> newCommandSyntaxes = _loader.Load(ClassicScriptResourcePaths.GetSyntaxPath("NewCommandSyntaxes.resx"));
		IReadOnlyList<ClassicScriptSyntaxDefinition> oldCommandSyntaxes = _loader.Load(ClassicScriptResourcePaths.GetSyntaxPath("OldCommandSyntaxes.resx"));
		IReadOnlyList<ClassicScriptSyntaxDefinition> parameterSyntaxes = _loader.Load(ClassicScriptResourcePaths.GetSyntaxPath("ParamSyntaxes.resx"));

		var commandDefinitions = new List<ClassicScriptSyntaxDefinition>(oldCommandSyntaxes.Count + newCommandSyntaxes.Count);
		commandDefinitions.AddRange(oldCommandSyntaxes);
		commandDefinitions.AddRange(newCommandSyntaxes);

		return new ClassicScriptSyntaxCatalogSnapshot(
			commandDefinitions,
			ToLookup(commandDefinitions),
			ToLookup(customizeSyntaxes),
			ToLookup(parameterSyntaxes));
	}

	private static IReadOnlyDictionary<string, ClassicScriptSyntaxDefinition> ToLookup(IReadOnlyList<ClassicScriptSyntaxDefinition> definitions)
		=> definitions.ToDictionary(definition => definition.Key, StringComparer.OrdinalIgnoreCase);

	private sealed record class ClassicScriptSyntaxCatalogSnapshot(
		IReadOnlyList<ClassicScriptSyntaxDefinition> CommandDefinitions,
		IReadOnlyDictionary<string, ClassicScriptSyntaxDefinition> CommandDefinitionsByKey,
		IReadOnlyDictionary<string, ClassicScriptSyntaxDefinition> CustomizeSyntaxesByKey,
		IReadOnlyDictionary<string, ClassicScriptSyntaxDefinition> ParameterSyntaxesByKey);
}