using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using TombLib.Scripting.Lua.Objects;

namespace TombLib.Scripting.Lua
{
	public sealed partial class LuaEditor
	{
		private async void TextEditor_KeyDown(object sender, KeyEventArgs e)
		{
			if (e.Key == Key.Escape && (_signaturePopup.IsOpen || _specialToolTip.IsOpen))
			{
				DismissTransientToolTips();
				e.Handled = true;
				return;
			}

			if (_signaturePopup.IsOpen && (e.Key == Key.Back || e.Key == Key.Delete))
				ScheduleSignatureHelpRefresh();

			if (e.Key == Key.F12)
			{
				Debug.WriteLine("[LuaLS] F12 pressed, attempting definition navigation.");

				if (await TryNavigateToDefinitionAsync(CaretOffset, CancellationToken.None).ConfigureAwait(true))
					e.Handled = true;
			}
		}

		private void TextEditor_PreviewMouseDown(object sender, MouseButtonEventArgs e)
		{
			if (e.ChangedButton == MouseButton.Left || e.ChangedButton == MouseButton.Right)
				DismissTransientToolTips();
		}

		private async void TextEditor_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
		{
			if (!Keyboard.Modifiers.HasFlag(ModifierKeys.Control) || e.ChangedButton != MouseButton.Left)
				return;

			int hoveredOffset = GetOffsetFromPoint(e.GetPosition(this));

			if (hoveredOffset == -1)
				return;

			Debug.WriteLine("[LuaLS] CTRL+Click detected, attempting definition navigation.");

			if (await TryNavigateToDefinitionAsync(hoveredOffset, CancellationToken.None).ConfigureAwait(true))
				e.Handled = true;
		}

		private async Task<bool> TryNavigateToDefinitionAsync(int offset, CancellationToken cancellationToken)
		{
			if (!IsIntellisenseAvailable())
			{
				Debug.WriteLine("[LuaLS] Definition aborted: intellisense not available.");
				return false;
			}

			if (DefinitionNavigationRequested is null)
			{
				Debug.WriteLine("[LuaLS] Definition aborted: DefinitionNavigationRequested callback not set.");
				return false;
			}

			try
			{
				foreach (int candidateOffset in GetDefinitionCandidateOffsets(offset))
				{
					(int line, int column) = GetPositionFromOffset(candidateOffset);

					Debug.WriteLine($"[LuaLS] Requesting definition at offset {candidateOffset} -> ({line},{column}).");

					LuaDefinitionLocation definitionLocation = await IntellisenseProvider
						.GetDefinitionAsync(FilePath, Text, line, column, cancellationToken)
						.ConfigureAwait(true);

					if (definitionLocation is null)
						continue;

					Debug.WriteLine($"[LuaLS] Definition found: '{definitionLocation.FilePath}' L{definitionLocation.LineNumber}.");
					DefinitionNavigationRequested(definitionLocation);
					return true;
				}

				Debug.WriteLine("[LuaLS] Definition not found for any candidate offset.");
				return false;
			}
			catch (OperationCanceledException)
			{
				return false;
			}
			catch (Exception exception)
			{
				Debug.WriteLine($"[LuaLS] Definition navigation failed: {exception.Message}");
				return false;
			}
		}

		public async void NavigateToDefinitionAtCaretAsync()
			=> await TryNavigateToDefinitionAsync(CaretOffset, CancellationToken.None).ConfigureAwait(true);

		private IEnumerable<int> GetDefinitionCandidateOffsets(int offset)
		{
			int safeOffset = Math.Max(0, Math.Min(offset, Document.TextLength));
			var offsets = new List<int>();

			AddDefinitionCandidateOffset(offsets, safeOffset);

			if (safeOffset > 0)
				AddDefinitionCandidateOffset(offsets, safeOffset - 1);

			if (TryGetDefinitionWordBounds(safeOffset, out int wordStart, out int wordEnd))
			{
				AddDefinitionCandidateOffset(offsets, wordStart);
				AddDefinitionCandidateOffset(offsets, wordEnd - 1);
			}

			return offsets;
		}

		private bool TryGetDefinitionWordBounds(int offset, out int wordStart, out int wordEnd)
		{
			wordStart = 0;
			wordEnd = 0;

			if (Document.TextLength == 0)
				return false;

			int safeOffset = Math.Max(0, Math.Min(offset, Document.TextLength));
			int probeOffset = safeOffset;

			if (probeOffset >= Document.TextLength)
				probeOffset = Document.TextLength - 1;

			if (probeOffset > 0
				&& !IsDefinitionIdentifierCharacter(Document.GetCharAt(probeOffset))
				&& IsDefinitionIdentifierCharacter(Document.GetCharAt(probeOffset - 1)))
			{
				probeOffset--;
			}

			if (!IsDefinitionIdentifierCharacter(Document.GetCharAt(probeOffset)))
				return false;

			wordStart = probeOffset;
			wordEnd = probeOffset + 1;

			while (wordStart > 0 && IsDefinitionIdentifierCharacter(Document.GetCharAt(wordStart - 1)))
				wordStart--;

			while (wordEnd < Document.TextLength && IsDefinitionIdentifierCharacter(Document.GetCharAt(wordEnd)))
				wordEnd++;

			return wordEnd > wordStart;
		}

		private static bool IsDefinitionIdentifierCharacter(char c)
			=> char.IsLetterOrDigit(c) || c == '_';

		private static void AddDefinitionCandidateOffset(ICollection<int> offsets, int offset)
		{
			if (!offsets.Contains(offset))
				offsets.Add(offset);
		}
	}
}