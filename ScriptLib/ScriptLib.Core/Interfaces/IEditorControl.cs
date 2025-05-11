using ScriptLib.Core.Enums;

namespace ScriptLib.Core.Interfaces;

/// <summary>
/// Defines a control for editing various types of content.
/// </summary>
public interface IEditorControl : IDisposable
{
	#region Editor State Properties

	/// <summary>
	/// Gets the type of this editor, whether it's a text editor, designer view, etc.
	/// </summary>
	EditorType EditorType { get; }

	/// <summary>
	/// Gets the default file extension for this editor type.
	/// </summary>
	string DefaultFileExtension { get; }

	/// <summary>
	/// Gets the file path of the content being edited or <see langword="null"/> if not applicable.
	/// </summary>
	string? FilePath { get; }

	/// <summary>
	/// Gets a value indicating whether the content has been modified since it was last saved.
	/// </summary>
	bool IsContentChanged { get; }

	/// <summary>
	/// Gets the last modified date and time of the content.
	/// </summary>
	DateTime LastModified { get; }

	/// <summary>
	/// Gets the version of the game engine associated with this editor.
	/// </summary>
	Version GameEngineVersion { get; }

	/// <summary>
	/// Silent session prevents the control from checking if the content has changed, therefore not running any workers to do so.
	/// <para>Setting this to <see langword="true"/> will also prevent the creation of backup files.</para>
	/// </summary>
	bool IsSilentSession { get; }

	/// <summary>
	/// Gets or sets the time interval to wait after content changes before firing the <see cref="ContentChangedDelayed"/> event.
	/// <para>This delay helps reduce excessive event firing during rapid content changes such as continuous typing.</para>
	/// </summary>
	TimeSpan ContentChangedDelayedInterval { get; set; }

	#endregion Editor State Properties

	#region Editing State Properties

	/// <summary>
	/// Gets a value indicating whether an undo operation can be performed.
	/// </summary>
	bool CanUndo { get; }

	/// <summary>
	/// Gets a value indicating whether a redo operation can be performed.
	/// </summary>
	bool CanRedo { get; }

	/// <summary>
	/// Gets the current row position of the cursor in the editor.
	/// </summary>
	int CurrentRow { get; }

	/// <summary>
	/// Gets the current column position of the cursor in the editor.
	/// </summary>
	int CurrentColumn { get; }

	/// <summary>
	/// Gets the currently selected content in the editor.
	/// </summary>
	object SelectedContent { get; }

	/// <summary>
	/// Gets the length of the current selection.
	/// </summary>
	int SelectionLength { get; }

	#endregion Editing State Properties

	#region Zoom Properties

	/// <summary>
	/// Gets or sets the current zoom level of the editor.
	/// </summary>
	int Zoom { get; set; }

	/// <summary>
	/// Gets or sets the minimum allowed zoom level.
	/// </summary>
	int MinZoom { get; set; }

	/// <summary>
	/// Gets or sets the maximum allowed zoom level.
	/// </summary>
	int MaxZoom { get; set; }

	/// <summary>
	/// Gets or sets the step size for zoom operations.
	/// </summary>
	int ZoomStepSize { get; set; }

	#endregion Zoom Properties

	#region Edit Operation Methods

	/// <summary>
	/// Performs an undo operation, reverting the most recent edit action.
	/// </summary>
	/// <returns><see langword="true"/> if the undo operation was successful; otherwise, <see langword="false"/>.</returns>
	bool Undo();

	/// <summary>
	/// Performs a redo operation, reapplying an edit action that was previously undone.
	/// </summary>
	/// <returns><see langword="true"/> if the redo operation was successful; otherwise, <see langword="false"/>.</returns>
	bool Redo();

	/// <summary>
	/// Cuts the selected content to the clipboard.
	/// </summary>
	void Cut();

	/// <summary>
	/// Copies the selected content to the clipboard.
	/// </summary>
	void Copy();

	/// <summary>
	/// Pastes content from the clipboard at the current cursor position.
	/// </summary>
	void Paste();

	/// <summary>
	/// Selects all content in the editor.
	/// </summary>
	void SelectAll();

	#endregion Edit Operation Methods

	#region Events

	/// <summary>
	/// Occurs when the content has changed and a delay period has elapsed.
	/// </summary>
	event EventHandler ContentChangedDelayed;

	/// <summary>
	/// Occurs when the editor status has changed.
	/// </summary>
	event EventHandler StatusChanged;

	/// <summary>
	/// Occurs when the zoom level has changed.
	/// </summary>
	event EventHandler ZoomChanged;

	#endregion Events
}
