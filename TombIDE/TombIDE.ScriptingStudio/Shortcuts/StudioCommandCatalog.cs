#nullable enable

using System;
using System.Collections.Generic;
using TombIDE.ScriptingStudio.UI;

namespace TombIDE.ScriptingStudio.Shortcuts;

/// <summary>
/// Immutable singleton catalog of all shortcut-eligible commands.
/// This is the sole source of default bindings; command surfaces and
/// the binding service must obtain defaults from here.
/// </summary>
public sealed class StudioCommandCatalog
{
	private readonly Dictionary<UICommand, StudioCommandDescriptor> _descriptorsByCommand;
	private readonly Dictionary<string, StudioCommandDescriptor> _descriptorsById;

	public StudioCommandCatalog(IReadOnlyList<StudioCommandDescriptor> descriptors)
	{
		ArgumentNullException.ThrowIfNull(descriptors);

		_descriptorsByCommand = new Dictionary<UICommand, StudioCommandDescriptor>(descriptors.Count);
		_descriptorsById = new Dictionary<string, StudioCommandDescriptor>(descriptors.Count, StringComparer.Ordinal);

		foreach (StudioCommandDescriptor descriptor in descriptors)
		{
			if (descriptor.Command == UICommand.None)
				throw new ArgumentException("UICommand.None must not be catalogued.", nameof(descriptors));

			if (_descriptorsByCommand.ContainsKey(descriptor.Command))
				throw new ArgumentException($"Duplicate command in catalog: {descriptor.Command}.", nameof(descriptors));

			if (_descriptorsById.ContainsKey(descriptor.SerializedId))
				throw new ArgumentException($"Duplicate serialized ID in catalog: {descriptor.SerializedId}.", nameof(descriptors));

			_descriptorsByCommand[descriptor.Command] = descriptor;
			_descriptorsById[descriptor.SerializedId] = descriptor;
		}
	}

	/// <summary>
	/// All catalog entries.
	/// </summary>
	public IReadOnlyCollection<StudioCommandDescriptor> Descriptors => _descriptorsByCommand.Values;

	/// <summary>
	/// Gets a descriptor by <see cref="UICommand"/>. Returns <see langword="null"/> when
	/// the command is not catalogued.
	/// </summary>
	public StudioCommandDescriptor? TryGetDescriptor(UICommand command)
	{
		_descriptorsByCommand.TryGetValue(command, out StudioCommandDescriptor? descriptor);
		return descriptor;
	}

	/// <summary>
	/// Gets a descriptor by its stable serialized identifier. Returns <see langword="null"/>
	/// when the identifier is unknown.
	/// </summary>
	public StudioCommandDescriptor? TryGetDescriptorById(string serializedId)
	{
		_descriptorsById.TryGetValue(serializedId, out StudioCommandDescriptor? descriptor);
		return descriptor;
	}

	/// <summary>
	/// Verifies that no two catalogued commands share a default shortcut in the same workspace.
	/// Returns a list of violation descriptions, or an empty list when the catalog is conflict-free.
	/// </summary>
	public IReadOnlyList<string> ValidateNoDuplicateDefaults()
	{
		var violations = new List<string>();
		var seen = new Dictionary<ShortcutKey, UICommand>();

		foreach (StudioCommandDescriptor descriptor in _descriptorsByCommand.Values)
		{
			foreach (ShortcutKey binding in descriptor.DefaultBindings)
			{
				if (seen.TryGetValue(binding, out UICommand existingCommand))
				{
					violations.Add(
						$"Default shortcut {binding.GetDisplayText()} is used by both " +
						$"{descriptor.Command} and {existingCommand}.");
				}
				else
				{
					seen[binding] = descriptor.Command;
				}
			}
		}

		return violations;
	}
}
