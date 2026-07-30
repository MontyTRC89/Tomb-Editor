#nullable enable

using NLog;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Windows.Input;
using TombIDE.ScriptingStudio.UI;

namespace TombIDE.ScriptingStudio.Shortcuts;

/// <summary>
/// Shell-scoped active shortcut binding service.
/// Merges catalog defaults with persisted overrides and provides
/// lookup, display, validation, and mutation operations.
/// </summary>
public sealed class ShortcutBindingService : IShortcutBindingService
{
	private static readonly ILogger Logger = LogManager.GetCurrentClassLogger();

	private readonly StudioCommandCatalog _catalog;
	private readonly Func<ShortcutOverrideCollection, bool> _saveOverrides;
	private readonly ShortcutOverrideCollection _appliedOverrides;

	private ImmutableDictionary<UICommand, ImmutableArray<ShortcutKey>> _bindingsByCommand;
	private ImmutableDictionary<ShortcutKey, UICommand> _commandsByShortcut;

	public ShortcutBindingService(
		StudioCommandCatalog catalog,
		ShortcutOverrideCollection loadedOverrides,
		Func<ShortcutOverrideCollection, bool> saveOverrides)
	{
		_catalog = catalog ?? throw new ArgumentNullException(nameof(catalog));
		_saveOverrides = saveOverrides ?? throw new ArgumentNullException(nameof(saveOverrides));
		_appliedOverrides = loadedOverrides ?? throw new ArgumentNullException(nameof(loadedOverrides));

		_bindingsByCommand = ImmutableDictionary<UICommand, ImmutableArray<ShortcutKey>>.Empty;
		_commandsByShortcut = ImmutableDictionary<ShortcutKey, UICommand>.Empty;

		Rebuild();
	}

	public event EventHandler? BindingsChanged;

	public bool TryGetCommand(ShortcutKey shortcut, out UICommand command)
		=> _commandsByShortcut.TryGetValue(shortcut, out command);

	public IReadOnlyList<ShortcutKey> GetBindings(UICommand command)
	{
		if (_bindingsByCommand.TryGetValue(command, out ImmutableArray<ShortcutKey> bindings))
			return bindings;

		return [];
	}

	public string GetDisplayText(UICommand command, string fallbackDisplayText = "")
	{
		IReadOnlyList<ShortcutKey> bindings = GetBindings(command);

		if (bindings.Count == 0)
			return fallbackDisplayText;

		return string.Join(" / ", bindings.Select(b => b.GetDisplayText()));
	}

	public ShortcutValidationResult Validate(UICommand command, IReadOnlyList<ShortcutKey> bindings)
		=> ValidateInternal(command, bindings, checkConflicts: true);

	public ShortcutValidationResult Apply(UICommand command, IReadOnlyList<ShortcutKey> bindings, bool replaceConflicts)
	{
		ShortcutValidationResult result = ValidateInternal(command, bindings, checkConflicts: !replaceConflicts);

		if (result != ShortcutValidationResult.Valid && result != ShortcutValidationResult.Conflict)
			return result;

		if (result == ShortcutValidationResult.Conflict && !replaceConflicts)
			return ShortcutValidationResult.Conflict;

		// Build new overrides snapshot.
		var newOverrides = new ShortcutOverrideCollection();

		foreach (ShortcutOverrideEntry existing in _appliedOverrides.Overrides)
		{
			if (!string.Equals(existing.CommandId, GetSerializedId(command), StringComparison.Ordinal))
			{
				newOverrides.Overrides.Add(new ShortcutOverrideEntry
				{
					CommandId = existing.CommandId,
					Bindings = [.. existing.Bindings.Select(b => new ShortcutBindingSettings { KeyName = b.KeyName, Modifiers = b.Modifiers })]
				});
			}
		}

		// Add/replace the target command's override.
		var settingsList = bindings
			.Select(b => new ShortcutBindingSettings { KeyName = b.Key.ToString(), Modifiers = (int)b.Modifiers })
			.ToList();

		newOverrides.Overrides.Add(new ShortcutOverrideEntry
		{
			CommandId = GetSerializedId(command),
			Bindings = settingsList
		});

		// If replacing conflicts, remove conflicting bindings from other commands.
		if (replaceConflicts)
			RemoveConflictingOverrides(newOverrides, command, bindings);

		// Persist.
		if (!_saveOverrides(newOverrides))
			return ShortcutValidationResult.Conflict; // Persistence failure — treat as conflict.

		// Update in-memory state.
		UpdateOverridesInPlace(newOverrides);
		RebuildAndNotify();

		return ShortcutValidationResult.Valid;
	}

	public ShortcutValidationResult Clear(UICommand command)
	{
		StudioCommandDescriptor? descriptor = _catalog.TryGetDescriptor(command);

		if (descriptor is null)
			return ShortcutValidationResult.NotRemappable;

		if (descriptor.IsHostReserved)
			return ShortcutValidationResult.Reserved;

		if (!descriptor.IsRemappable)
			return ShortcutValidationResult.NotRemappable;

		// Build new overrides with an empty binding list for this command.
		var newOverrides = new ShortcutOverrideCollection();

		foreach (ShortcutOverrideEntry existing in _appliedOverrides.Overrides)
		{
			if (!string.Equals(existing.CommandId, descriptor.SerializedId, StringComparison.Ordinal))
			{
				newOverrides.Overrides.Add(new ShortcutOverrideEntry
				{
					CommandId = existing.CommandId,
					Bindings = [.. existing.Bindings.Select(b => new ShortcutBindingSettings { KeyName = b.KeyName, Modifiers = b.Modifiers })]
				});
			}
		}

		newOverrides.Overrides.Add(new ShortcutOverrideEntry
		{
			CommandId = descriptor.SerializedId,
			Bindings = [] // Explicitly empty = unbound.
		});

		if (!_saveOverrides(newOverrides))
			return ShortcutValidationResult.Conflict;

		UpdateOverridesInPlace(newOverrides);
		RebuildAndNotify();

		return ShortcutValidationResult.Valid;
	}

	public void Reset(UICommand command)
	{
		var newOverrides = new ShortcutOverrideCollection();

		foreach (ShortcutOverrideEntry existing in _appliedOverrides.Overrides)
		{
			if (!string.Equals(existing.CommandId, GetSerializedId(command), StringComparison.Ordinal))
			{
				newOverrides.Overrides.Add(new ShortcutOverrideEntry
				{
					CommandId = existing.CommandId,
					Bindings = [.. existing.Bindings.Select(b => new ShortcutBindingSettings { KeyName = b.KeyName, Modifiers = b.Modifiers })]
				});
			}
		}

		_saveOverrides(newOverrides);
		UpdateOverridesInPlace(newOverrides);
		RebuildAndNotify();
	}

	public void ResetAll()
	{
		var newOverrides = new ShortcutOverrideCollection();
		_saveOverrides(newOverrides);
		UpdateOverridesInPlace(newOverrides);
		RebuildAndNotify();
	}

	public void Dispose()
	{
		BindingsChanged = null;
	}

	private ShortcutValidationResult ValidateInternal(UICommand command, IReadOnlyList<ShortcutKey> bindings, bool checkConflicts)
	{
		StudioCommandDescriptor? descriptor = _catalog.TryGetDescriptor(command);

		if (descriptor is null)
			return ShortcutValidationResult.NotRemappable;

		if (descriptor.IsHostReserved)
			return ShortcutValidationResult.Reserved;

		if (!descriptor.IsRemappable)
			return ShortcutValidationResult.NotRemappable;

		var seen = new HashSet<ShortcutKey>();

		foreach (ShortcutKey binding in bindings)
		{
			if (!seen.Add(binding))
				return ShortcutValidationResult.DuplicateInCommand;
		}

		if (checkConflicts)
		{
			foreach (ShortcutKey binding in bindings)
			{
				if (_commandsByShortcut.TryGetValue(binding, out UICommand existingCommand) &&
					existingCommand != command)
				{
					return ShortcutValidationResult.Conflict;
				}
			}
		}

		return ShortcutValidationResult.Valid;
	}

	private void Rebuild()
	{
		var bindingsBuilder = ImmutableDictionary.CreateBuilder<UICommand, ImmutableArray<ShortcutKey>>();
		var commandsBuilder = ImmutableDictionary.CreateBuilder<ShortcutKey, UICommand>();

		foreach (StudioCommandDescriptor descriptor in _catalog.Descriptors)
		{
			IReadOnlyList<ShortcutKey> bindings = ResolveBindings(descriptor);
			bindingsBuilder[descriptor.Command] = [.. bindings];

			foreach (ShortcutKey binding in bindings)
			{
				if (commandsBuilder.ContainsKey(binding))
				{
					throw new InvalidOperationException(
						$"Shortcut collision detected: {binding.GetDisplayText()} is bound to both " +
						$"{commandsBuilder[binding]} and {descriptor.Command}.");
				}

				commandsBuilder[binding] = descriptor.Command;
			}
		}

		_bindingsByCommand = bindingsBuilder.ToImmutable();
		_commandsByShortcut = commandsBuilder.ToImmutable();
	}

	private void RebuildAndNotify()
	{
		Rebuild();
		BindingsChanged?.Invoke(this, EventArgs.Empty);
	}

	private IReadOnlyList<ShortcutKey> ResolveBindings(StudioCommandDescriptor descriptor)
	{
		ShortcutOverrideEntry? ovverride = _appliedOverrides.Overrides
			.FirstOrDefault(o => string.Equals(o.CommandId, descriptor.SerializedId, StringComparison.Ordinal));

		if (ovverride is null)
			return descriptor.DefaultBindings;

		if (ovverride.Bindings.Count == 0)
			return []; // Explicitly unbound.

		// Validate and deserialize override bindings.
		if (descriptor.IsHostReserved)
		{
			Logger.Warn(
				"Shortcut override for host-reserved command '{0}' ignored. Using catalog defaults.",
				descriptor.SerializedId);
			return descriptor.DefaultBindings;
		}

		var parsed = new List<ShortcutKey>();

		foreach (ShortcutBindingSettings bindingSettings in ovverride.Bindings)
		{
			ShortcutKey? parsedKey = ParseBindingSettings(bindingSettings, descriptor.SerializedId);

			if (parsedKey is not null)
				parsed.Add(parsedKey.Value);
		}

		if (parsed.Count == 0)
		{
			Logger.Warn(
				"All override bindings for command '{0}' were invalid. Falling back to catalog defaults.",
				descriptor.SerializedId);
			return descriptor.DefaultBindings;
		}

		return parsed;
	}

	private static ShortcutKey? ParseBindingSettings(ShortcutBindingSettings settings, string commandId)
	{
		if (string.IsNullOrEmpty(settings.KeyName))
		{
			Logger.Warn("Shortcut override for '{0}' has an empty key name. Skipping.", commandId);
			return null;
		}

		if (!Enum.TryParse(settings.KeyName, out Key key) || key == Key.None)
		{
			Logger.Warn(
				"Shortcut override for '{0}' has invalid key name '{1}'. Skipping.",
				commandId, settings.KeyName);
			return null;
		}

		try
		{
			return new ShortcutKey(key, (ModifierKeys)settings.Modifiers);
		}
		catch (ArgumentException ex)
		{
			Logger.Warn(
				"Shortcut override for '{0}' could not be parsed: {1}. Skipping.",
				commandId, ex.Message);
			return null;
		}
	}

	private void UpdateOverridesInPlace(ShortcutOverrideCollection newOverrides)
	{
		_appliedOverrides.Overrides.Clear();
		_appliedOverrides.Version = newOverrides.Version;

		foreach (ShortcutOverrideEntry entry in newOverrides.Overrides)
			_appliedOverrides.Overrides.Add(entry);
	}

	private static void RemoveConflictingOverrides(
		ShortcutOverrideCollection newOverrides,
		UICommand targetCommand,
		IReadOnlyList<ShortcutKey> newBindings)
	{
		var newBindingSet = new HashSet<ShortcutKey>(newBindings);

		foreach (ShortcutOverrideEntry entry in newOverrides.Overrides)
		{
			entry.Bindings.RemoveAll(bs =>
			{
				ShortcutKey? parsed = ParseBindingSettings(bs, entry.CommandId);
				return parsed is not null && newBindingSet.Contains(parsed.Value);
			});
		}
	}

	private static string GetSerializedId(UICommand command)
	{
		// When there is no catalog descriptor, fall back to enum name.
		return command.ToString();
	}
}
