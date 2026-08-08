using System;
using System.Windows;
using TombLib.Scripting.Lua.Highlighting;
using TombLib.Scripting.Lua.Resources;
using TombLib.Scripting.Lua.Themes;
using static TombLib.Tests.WPFTestHelper;

namespace TombLib.Tests.Lua;

/// <summary>
/// Direct tests for <see cref="LuaSemanticTokensColorizer"/> lifecycle and theme
/// updates, driven through a hosted editor without private-field reflection.
/// </summary>
[TestClass]
public class LuaSemanticTokensColorizerTests
{
	[TestMethod]
	public void SetSemanticTokens_AppliesTokens_WithoutThrowing()
	{
		RunInSta(() =>
		{
			var editor = new LuaEditor(new Version(1, 0))
			{
				Text = "local value = 1"
			};

			Window window = ShowInHostWindow(editor);

			try
			{
				editor.SetSemanticTokens(
				[
					new LuaSemanticToken(0, 6, 5, "variable", []),
					new LuaSemanticToken(0, 14, 1, "number", [])
				]);
			}
			finally
			{
				window.Close();
			}
		});
	}

	[TestMethod]
	public void ClearSemanticTokens_ResetsStyling_WithoutThrowing()
	{
		RunInSta(() =>
		{
			var editor = new LuaEditor(new Version(1, 0))
			{
				Text = "local value = 1"
			};

			Window window = ShowInHostWindow(editor);

			try
			{
				editor.SetSemanticTokens([new LuaSemanticToken(0, 6, 5, "variable", [])]);
				editor.SetSemanticTokens([]);
				editor.ClearSemanticTokens();
			}
			finally
			{
				window.Close();
			}
		});
	}

	[TestMethod]
	public void UpdateTheme_RebuildsStyledTokens_WithoutThrowing()
	{
		RunInSta(() =>
		{
			var editor = new LuaEditor(new Version(1, 0))
			{
				Text = "local value = 1"
			};

			Window window = ShowInHostWindow(editor);

			try
			{
				var colorizer = new LuaSemanticTokensColorizer(
					editor.TextArea.TextView,
					LuaEditorColorPalette.Create(new LuaTheme { Name = "Default" }));

				colorizer.SetTokens([new LuaSemanticToken(0, 6, 5, "variable", [])]);

				// A theme change rebuilds the styled token cache without throwing.
				colorizer.UpdateTheme(LuaEditorColorPalette.Create(new LuaTheme { Name = "Alternate" }));
			}
			finally
			{
				window.Close();
			}
		});
	}
}
