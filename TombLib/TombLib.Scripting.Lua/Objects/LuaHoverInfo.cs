using System;

namespace TombLib.Scripting.Lua.Objects
{
	public sealed class LuaHoverInfo
	{
		public LuaHoverInfo(string content, bool isMarkdown)
		{
			Content = content ?? throw new ArgumentNullException(nameof(content));
			IsMarkdown = isMarkdown;
		}

		public string Content { get; }
		public bool IsMarkdown { get; }
	}
}