using System;
using System.Collections.Generic;

namespace TombLib.Scripting.Lua.Objects
{
	public sealed class LuaSignatureInfo
	{
		public LuaSignatureInfo(string label, string? documentation, IReadOnlyList<LuaParameterInfo> parameters,
			int activeParameter)
		{
			Label = label ?? throw new ArgumentNullException(nameof(label));
			Documentation = documentation;
			Parameters = parameters ?? Array.Empty<LuaParameterInfo>();
			ActiveParameter = Math.Max(0, activeParameter);
		}

		public string Label { get; }
		public string? Documentation { get; }
		public IReadOnlyList<LuaParameterInfo> Parameters { get; }
		public int ActiveParameter { get; }
	}

	public sealed class LuaParameterInfo
	{
		public LuaParameterInfo(string label, string? documentation)
		{
			Label = label ?? string.Empty;
			Documentation = documentation;
		}

		public string Label { get; }
		public string? Documentation { get; }
	}
}
