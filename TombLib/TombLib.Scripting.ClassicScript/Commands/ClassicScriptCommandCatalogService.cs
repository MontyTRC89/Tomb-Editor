#nullable enable

namespace TombLib.Scripting.ClassicScript.Commands;

// Loads the authoritative command kind and section data from Commands.json
// (Resources/ClassicScript/Commands.json) once and serves it as read-only lists.
// The command list data is the authoritative command-kind source: a command is old only
// when it is listed with kind "old" and not also with kind "new" (an entry in both kinds,
// such as FMV, is treated as new). Commands absent from the catalog fall back to new,
// matching the pre-catalog behavior where a command without an OLD description was treated
// as new.
public sealed class ClassicScriptCommandCatalogService
{
	private readonly ClassicScriptCommandsLoader _loader;
	private readonly Lazy<ClassicScriptCommandCatalogSnapshot> _catalog;

	public ClassicScriptCommandCatalogService()
		: this(new ClassicScriptCommandsLoader())
	{ }

	internal ClassicScriptCommandCatalogService(ClassicScriptCommandsLoader loader)
	{
		_loader = loader ?? throw new ArgumentNullException(nameof(loader));
		_catalog = new Lazy<ClassicScriptCommandCatalogSnapshot>(LoadCatalog);
	}

	public IReadOnlyList<string> Sections => _catalog.Value.Sections;

	public IReadOnlyList<string> NewCommands => _catalog.Value.NewCommands;

	public IReadOnlyList<string> OldCommands => _catalog.Value.OldCommands;

	public bool IsOldCommand(string command)
		=> _catalog.Value.OldCommands.Contains(command, StringComparer.OrdinalIgnoreCase)
			&& !_catalog.Value.NewCommands.Contains(command, StringComparer.OrdinalIgnoreCase);

	public bool IsNewCommand(string command)
		=> !IsOldCommand(command);

	private ClassicScriptCommandCatalogSnapshot LoadCatalog()
	{
		ClassicScriptCommandsCatalog catalog = _loader.Load();

		return new ClassicScriptCommandCatalogSnapshot(
			catalog.Sections,
			GetCommandNames(catalog, ClassicScriptCommandKind.New),
			GetCommandNames(catalog, ClassicScriptCommandKind.Old));
	}

	private static IReadOnlyList<string> GetCommandNames(ClassicScriptCommandsCatalog catalog, ClassicScriptCommandKind kind)
		=> catalog.Commands
			.Where(entry => entry.Kind == kind)
			.Select(entry => entry.Name)
			.ToArray();
}

internal sealed record class ClassicScriptCommandCatalogSnapshot(
	IReadOnlyList<string> Sections,
	IReadOnlyList<string> NewCommands,
	IReadOnlyList<string> OldCommands);
