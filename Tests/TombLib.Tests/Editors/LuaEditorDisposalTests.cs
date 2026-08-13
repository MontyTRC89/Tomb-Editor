using Moq;
using System;
using System.Windows;
using static TombLib.Tests.WPFTestHelper;

namespace TombLib.Tests;

[TestClass]
public class LuaEditorDisposalTests
{
	[TestMethod]
	public void Dispose_WithoutUnloaded_ClosesLanguageServerDocument()
	{
		RunInSta(() =>
		{
			var provider = new Mock<ILuaIntelliSenseProvider>();
			var editor = new LuaEditor(new Version(1, 0))
			{
				FilePath = @"C:\Workspace\Scripts\test.lua",
				IntelliSenseProvider = provider.Object
			};

			// Explicit disposal must close the LSP document even when Unloaded never fires.
			editor.Dispose();

			provider.Verify(x => x.CloseDocument(@"C:\Workspace\Scripts\test.lua"), Times.Once);
		});
	}

	[TestMethod]
	public void Dispose_WhileLoaded_ClosesDocumentOnce_AndUnloadedIsIdempotent()
	{
		RunInSta(() =>
		{
			var provider = new Mock<ILuaIntelliSenseProvider>();
			var editor = new LuaEditor(new Version(1, 0))
			{
				FilePath = @"C:\Workspace\Scripts\test.lua",
				IntelliSenseProvider = provider.Object
			};

			Window hostWindow = ShowInHostWindow(editor);

			try
			{
				editor.Dispose();
			}
			finally
			{
				// Unloaded fires after disposal; the cleanup must stay idempotent.
				hostWindow.Close();
			}

			provider.Verify(x => x.CloseDocument(@"C:\Workspace\Scripts\test.lua"), Times.Once);
		});
	}

	[TestMethod]
	public void Dispose_IsIdempotent_AndDoesNotThrow()
	{
		RunInSta(() =>
		{
			var editor = new LuaEditor(new Version(1, 0))
			{
				FilePath = @"C:\Workspace\Scripts\test.lua"
			};

			editor.Dispose();
			editor.Dispose();
		});
	}
}
