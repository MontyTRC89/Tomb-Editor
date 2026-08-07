using System;
using System.Windows.Input;
using TombLib.Scripting.UI.Rendering;

namespace TombLib.Scripting.UI.Bases;

public abstract partial class TextEditorBase
{
	#region Events

	public event EventHandler? StatusChanged;

	protected virtual void OnStatusChanged(EventArgs e)
		=> StatusChanged?.Invoke(this, e);

	internal void RaiseStatusChanged()
		=> OnStatusChanged(EventArgs.Empty);

	public event EventHandler? ZoomChanged;

	protected virtual void OnZoomChanged(EventArgs e)
	{
		ZoomChanged?.Invoke(this, e);
		OnStatusChanged(EventArgs.Empty);
	}

	internal void RaiseZoomChanged()
		=> OnZoomChanged(EventArgs.Empty);

	public event EventHandler? TextChangedDelayed;

	protected virtual void OnTextChangedDelayed(EventArgs e)
		=> TextChangedDelayed?.Invoke(this, e);

	public event EventHandler? ContentChangedWorkerRunCompleted;

	protected virtual void OnContentChangedWorkerRunCompleted(EventArgs e)
		=> ContentChangedWorkerRunCompleted?.Invoke(this, e);

	private void ContentPersistenceCoordinator_ContentChangedWorkerRunCompleted(object? sender, EventArgs e)
	{
		OnContentChangedWorkerRunCompleted(EventArgs.Empty);
	}

	private void TextArea_TextEntering(object? sender, TextCompositionEventArgs e)
	{
		CloseDefinitionToolTip(true); // Prevents the ToolTip from covering the screen while typing
		HandleAutoClosing(e);
		OnLanguageTextEntering(e);
	}

	private void TextEditor_TextEntered(object? sender, TextCompositionEventArgs e)
		=> OnLanguageTextEntered(e);

	private void TextEditor_TextChanged(object? sender, EventArgs e)
	{
		LastModified = DateTime.Now;
		IsContentChanged = _contentPersistenceCoordinator.HandleContentChanged();
		OnLanguageTextChanged(e);
	}

	private void TextEditor_KeyDown(object? sender, KeyEventArgs e)
		=> OnLanguageKeyDown(e);

	private void TextEditor_PreviewMouseLeftButtonDown(object? sender, MouseButtonEventArgs e)
		=> OnLanguagePreviewMouseLeftButtonDown(e);

	private void ContentPersistenceCoordinator_TextChangedDelayed(object? sender, EventArgs e)
	{
		OnTextChangedDelayed(EventArgs.Empty);
	}

	private void TextEditor_MouseHover(object? sender, MouseEventArgs e)
		=> OnLanguageMouseHover(e);

	/// <summary>
	/// Handles mouse hover for error tooltips. Override to customize hover behavior.
	/// </summary>
	protected virtual void HandleMouseHover(MouseEventArgs e)
		=> HandleErrorToolTips(e);

	private void TextEditor_MouseHoverStopped(object? sender, MouseEventArgs e)
		=> ScheduleDefinitionToolTipClose();

	private void TextEditor_PreviewMouseWheel(object? sender, MouseWheelEventArgs e)
	{
		if (Keyboard.Modifiers == ModifierKeys.Control
			&& _statusCoordinator.TryHandleZoom(e.Delta, MinZoom, MaxZoom, ZoomStepSize, DefaultFontSize, fontSize => FontSize = fontSize))
		{
			e.Handled = true;
		}
	}

	private void TextEditor_MouseRightButtonDown(object? sender, MouseButtonEventArgs e)
	{
		_viewService.TryMoveCaretToMousePosition();

		if (ContextMenu is null)
			ContextMenu = TextEditorContextMenuFactory.BuildDefault();

		ContextMenu.IsOpen = true;
		e.Handled = true;
	}

	protected void CloseDefinitionToolTip(bool force = false)
		=> _toolTipPresenter.Close(force);

	private void ScheduleDefinitionToolTipClose()
		=> _toolTipPresenter.ScheduleClose();

	#endregion Events
}
