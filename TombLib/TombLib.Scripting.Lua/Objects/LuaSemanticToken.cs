using System;
using System.Collections.Generic;

namespace TombLib.Scripting.Lua.Objects;

public sealed class LuaSemanticToken
{
	public LuaSemanticToken(int line, int character, int length, string type, IReadOnlyList<string> modifiers)
	{
		Line = Math.Max(0, line);
		Character = Math.Max(0, character);
		Length = Math.Max(0, length);
		Type = type ?? string.Empty;
		Modifiers = modifiers ?? [];
	}

	public int Line { get; }
	public int Character { get; }
	public int Length { get; }
	public string Type { get; }
	public IReadOnlyList<string> Modifiers { get; }

	public bool HasModifier(string modifier)
	{
		if (string.IsNullOrWhiteSpace(modifier) || Modifiers.Count == 0)
			return false;

		for (int i = 0; i < Modifiers.Count; i++)
		{
			if (string.Equals(Modifiers[i], modifier, StringComparison.Ordinal))
				return true;
		}

		return false;
	}
}
