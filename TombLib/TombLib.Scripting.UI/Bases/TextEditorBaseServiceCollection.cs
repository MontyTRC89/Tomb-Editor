using System;
using TombLib.Scripting.UI.Completion;
using TombLib.Scripting.UI.Diagnostics;
using TombLib.Scripting.UI.Documents;
using TombLib.Scripting.UI.Editing;
using TombLib.Scripting.UI.Editors;
using TombLib.Scripting.UI.Navigation;
using TombLib.Scripting.UI.Presentation;

namespace TombLib.Scripting.UI.Bases;

internal sealed class TextEditorBaseServiceCollection
{
	private TextEditorBaseServiceCollection(
		TextAutoClosingService autoClosingService,
		BookmarkCoordinator bookmarkCoordinator,
		TextLineCommentService commentService,
		CompletionWindowCoordinator completionWindowCoordinator,
		ContentPersistenceCoordinator contentPersistenceCoordinator,
		TextDefinitionNavigationService definitionNavigationService,
		TextDiagnosticToolTipService diagnosticToolTipService,
		TextEditorStatusCoordinator statusCoordinator,
		EditorToolTipPresenter toolTipPresenter,
		TextEditorViewService viewService)
	{
		AutoClosingService = autoClosingService;
		BookmarkCoordinator = bookmarkCoordinator;
		CommentService = commentService;
		CompletionWindowCoordinator = completionWindowCoordinator;
		ContentPersistenceCoordinator = contentPersistenceCoordinator;
		DefinitionNavigationService = definitionNavigationService;
		DiagnosticToolTipService = diagnosticToolTipService;
		StatusCoordinator = statusCoordinator;
		ToolTipPresenter = toolTipPresenter;
		ViewService = viewService;
	}

	public TextAutoClosingService AutoClosingService { get; }

	public BookmarkCoordinator BookmarkCoordinator { get; }

	public TextLineCommentService CommentService { get; }

	public CompletionWindowCoordinator CompletionWindowCoordinator { get; }

	public ContentPersistenceCoordinator ContentPersistenceCoordinator { get; }

	public TextDefinitionNavigationService DefinitionNavigationService { get; }

	public TextDiagnosticToolTipService DiagnosticToolTipService { get; }

	public TextEditorStatusCoordinator StatusCoordinator { get; }

	public EditorToolTipPresenter ToolTipPresenter { get; }

	public TextEditorViewService ViewService { get; }

	public static TextEditorBaseServiceCollection Create(TextEditorBase editor)
	{
		ArgumentNullException.ThrowIfNull(editor);

		return new TextEditorBaseServiceCollection(
			autoClosingService: new TextAutoClosingService(),
			bookmarkCoordinator: new BookmarkCoordinator(
				() => editor.Document,
				onBookmarksChanged: () =>
				{
					editor.TextArea.TextView.InvalidateLayer(ICSharpCode.AvalonEdit.Rendering.KnownLayer.Background);
					editor.SaveBookmarks();
				}),
			commentService: new TextLineCommentService(),
			completionWindowCoordinator: new CompletionWindowCoordinator(
				new CompletionWindowHost(editor.TextArea),
				TextEditorBase.DefaultToolTipBorder,
				TextEditorBase.DefaultToolTipBackground,
				TextEditorBase.ToolTipForeground),
			contentPersistenceCoordinator: new ContentPersistenceCoordinator(() => editor.Content, () => editor.IsSilentSession, useDelayedScheduling: true),
			definitionNavigationService: new TextDefinitionNavigationService(),
			diagnosticToolTipService: new TextDiagnosticToolTipService(onDiagnosticsChanged: () => editor.InvalidateDiagnosticLayer()),
			statusCoordinator: new TextEditorStatusCoordinator(editor.TextArea, editor.RaiseStatusChanged, editor.RaiseZoomChanged),
			toolTipPresenter: new EditorToolTipPresenter(editor),
			viewService: new TextEditorViewService(editor));
	}
}
