using System;
using TombLib.Scripting.UI.Completion;
using TombLib.Scripting.UI.Diagnostics;
using TombLib.Scripting.UI.Documents;
using TombLib.Scripting.UI.Editing;
using TombLib.Scripting.UI.Editors;
using TombLib.Scripting.UI.Navigation;
using TombLib.Scripting.UI.Presentation;

namespace TombLib.Scripting.UI.Bases;

/// <summary>
/// Internal construction-time bundle of the per-editor services created in the
/// <see cref="TextEditorBase"/> constructor.
/// This is not a general-purpose service locator: every member is 1:1 with the editor
/// instance it was created for and depends on the editor being constructed (document,
/// text area, popup host, etc.), so real DI composition is not applicable here.
/// Ownership and lifetime: the bundle (and each service) is owned by the editor instance
/// that created it and lives exactly as long as that editor; the constructor fields it
/// into <see cref="TextEditorBase"/> readonly fields, after which the bundle itself is
/// discarded.
/// </summary>
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

	/// <summary>
	/// The auto-closing service used by the editor.
	/// </summary>
	public TextAutoClosingService AutoClosingService { get; }

	/// <summary>
	/// The bookmark coordinator used by the editor.
	/// </summary>
	public BookmarkCoordinator BookmarkCoordinator { get; }

	/// <summary>
	/// The line comment service used by the editor.
	/// </summary>
	public TextLineCommentService CommentService { get; }

	/// <summary>
	/// The completion window coordinator used by the editor.
	/// </summary>
	public CompletionWindowCoordinator CompletionWindowCoordinator { get; }

	/// <summary>
	/// The content persistence coordinator used by the editor.
	/// </summary>
	public ContentPersistenceCoordinator ContentPersistenceCoordinator { get; }

	/// <summary>
	/// The definition navigation service used by the editor.
	/// </summary>
	public TextDefinitionNavigationService DefinitionNavigationService { get; }

	/// <summary>
	/// The diagnostic tooltip service used by the editor.
	/// </summary>
	public TextDiagnosticToolTipService DiagnosticToolTipService { get; }

	/// <summary>
	/// The status coordinator used by the editor.
	/// </summary>
	public TextEditorStatusCoordinator StatusCoordinator { get; }

	/// <summary>
	/// The tooltip presenter used by the editor.
	/// </summary>
	public EditorToolTipPresenter ToolTipPresenter { get; }

	/// <summary>
	/// The view service used by the editor.
	/// </summary>
	public TextEditorViewService ViewService { get; }

	/// <summary>
	/// Creates the service collection for the given editor.
	/// </summary>
	/// <param name="editor">The editor the services are created for.</param>
	/// <returns>The service collection bound to the editor.</returns>
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
