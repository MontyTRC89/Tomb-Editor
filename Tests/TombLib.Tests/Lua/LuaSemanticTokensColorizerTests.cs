using System;
using System.Diagnostics;
using System.Linq;
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

	[TestMethod]
	public void LargeSemanticTokenSet_AppliesCorrectlyWithinGenerousAllocationAndTimeBounds()
	{
		RunInSta(() =>
		{
			const int lineCount = 400;
			const int tokensPerLine = 10;
			const int iterations = 20;
			string text = CreateDocument(lineCount);
			LuaSemanticToken[] tokens = CreateTokens(lineCount, tokensPerLine);
			var editor = new LuaEditor(new Version(1, 0))
			{
				Text = text
			};
			Window window = ShowInHostWindow(editor);

			try
			{
				editor.SetSemanticTokens(tokens);
				Assert.AreEqual(text, editor.Text);
				Assert.IsTrue(editor.TextArea.TextView.LineTransformers.Any(transformer => transformer is LuaSemanticTokensColorizer));

				long allocated = MeasureAllocations(() => editor.SetSemanticTokens(tokens), iterations);
				double elapsed = MeasureElapsed(() => editor.SetSemanticTokens(tokens), iterations);

				Assert.IsTrue(allocated < 256L * 1024L * 1024L,
					$"Applying {tokens.Length} semantic tokens allocated {allocated} B.");
				Assert.IsTrue(elapsed < 10_000.0,
					$"Applying {tokens.Length} semantic tokens took {elapsed:F1} ms.");
				Assert.AreEqual(text, editor.Text);

				editor.ClearSemanticTokens();
				Assert.AreEqual(text, editor.Text);
			}
			finally
			{
				window.Close();
			}
		});
	}

	private static string CreateDocument(int lineCount)
		=> string.Concat(System.Linq.Enumerable.Repeat("local value = 1\n", lineCount));

	private static LuaSemanticToken[] CreateTokens(int lineCount, int tokensPerLine)
	{
		var tokens = new LuaSemanticToken[lineCount * tokensPerLine];
		int index = 0;

		for (int line = 0; line < lineCount; line++)
		{
			for (int token = 0; token < tokensPerLine; token++)
				tokens[index++] = new LuaSemanticToken(line, token % 15, 1, "variable", []);
		}

		return tokens;
	}

	private static long MeasureAllocations(Action action, int iterations)
	{
		long before = GC.GetAllocatedBytesForCurrentThread();

		for (int iteration = 0; iteration < iterations; iteration++)
			action();

		return GC.GetAllocatedBytesForCurrentThread() - before;
	}

	private static double MeasureElapsed(Action action, int iterations)
	{
		var stopwatch = Stopwatch.StartNew();

		for (int iteration = 0; iteration < iterations; iteration++)
			action();

		stopwatch.Stop();
		return stopwatch.Elapsed.TotalMilliseconds;
	}
}
