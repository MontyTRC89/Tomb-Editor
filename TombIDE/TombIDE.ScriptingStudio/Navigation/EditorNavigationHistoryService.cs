#nullable enable

using System;
using System.Collections.Generic;

namespace TombIDE.ScriptingStudio.Navigation;

internal sealed class EditorNavigationHistoryService
{
	private const int MinimumCaretMoveDistance = 32;
	private const int MinimumSelectionMoveDistance = 8;

	private readonly Stack<EditorNavigationLocation> _backStack = new();
	private readonly Stack<EditorNavigationLocation> _forwardStack = new();

	private EditorNavigationLocation? _currentLocation;
	private int _suppressionDepth;

	public bool CanNavigateBack => _backStack.Count > 0;

	public bool CanNavigateForward => _forwardStack.Count > 0;

	public void Observe(EditorNavigationLocation location)
	{
		if (_suppressionDepth > 0)
		{
			_currentLocation = location;
			return;
		}

		if (_currentLocation is not EditorNavigationLocation currentLocation)
		{
			_currentLocation = location;
			return;
		}

		if (!IsMeaningfulChange(currentLocation, location))
		{
			_currentLocation = location;
			return;
		}

		PushDistinct(_backStack, currentLocation);
		_forwardStack.Clear();
		_currentLocation = location;
	}

	public void RecordProgrammaticJump(EditorNavigationLocation currentLocation, EditorNavigationLocation targetLocation)
	{
		_currentLocation = currentLocation;

		if (currentLocation.IsEquivalentTo(targetLocation))
			return;

		PushDistinct(_backStack, currentLocation);
		_forwardStack.Clear();
	}

	public void SetCurrentLocation(EditorNavigationLocation location)
		=> _currentLocation = location;

	public bool TryNavigateBack(EditorNavigationLocation currentLocation, out EditorNavigationLocation? targetLocation)
	{
		targetLocation = null;

		if (_backStack.Count == 0)
			return false;

		PushDistinct(_forwardStack, currentLocation);
		targetLocation = _backStack.Pop();
		_currentLocation = targetLocation;
		return true;
	}

	public bool TryNavigateForward(EditorNavigationLocation currentLocation, out EditorNavigationLocation? targetLocation)
	{
		targetLocation = null;

		if (_forwardStack.Count == 0)
			return false;

		PushDistinct(_backStack, currentLocation);
		targetLocation = _forwardStack.Pop();
		_currentLocation = targetLocation;
		return true;
	}

	public IDisposable SuppressRecording()
	{
		_suppressionDepth++;
		return new RecordingScope(this);
	}

	private static bool IsMeaningfulChange(EditorNavigationLocation previous, EditorNavigationLocation current)
	{
		if (!string.Equals(previous.FilePath, current.FilePath, StringComparison.OrdinalIgnoreCase))
			return true;

		if (previous.SelectionLength != current.SelectionLength)
			return true;

		if (previous.SelectionLength > 0 || current.SelectionLength > 0)
			return Math.Abs(previous.SelectionStart - current.SelectionStart) >= MinimumSelectionMoveDistance;

		return Math.Abs(previous.CaretOffset - current.CaretOffset) >= MinimumCaretMoveDistance;
	}

	private static void PushDistinct(Stack<EditorNavigationLocation> stack, EditorNavigationLocation location)
	{
		if (stack.Count == 0 || !stack.Peek().IsEquivalentTo(location))
			stack.Push(location);
	}

	private sealed class RecordingScope : IDisposable
	{
		private EditorNavigationHistoryService? _owner;

		public RecordingScope(EditorNavigationHistoryService owner)
			=> _owner = owner;

		public void Dispose()
		{
			if (_owner is null)
				return;

			_owner._suppressionDepth = Math.Max(0, _owner._suppressionDepth - 1);
			_owner = null;
		}
	}
}