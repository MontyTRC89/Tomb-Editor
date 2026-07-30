#nullable enable

using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;
using TombLib.Scripting.UI.Editors;

namespace TombIDE.ScriptingStudio.Editors;

internal sealed class FileReloadCoordinator
{
	private readonly List<string> _pendingFileReloads = [];

	public bool IsRunning { get; private set; }

	public void QueueFile(string filePath)
	{
		if (string.IsNullOrWhiteSpace(filePath) || _pendingFileReloads.Contains(filePath))
			return;

		_pendingFileReloads.Add(filePath);
	}

	public void ProcessQueuedFiles(
		Func<string, IReadOnlyList<IEditorControl>> getEditorsOfFile,
		Func<string, DialogResult> promptReload)
	{
		ArgumentNullException.ThrowIfNull(getEditorsOfFile);
		ArgumentNullException.ThrowIfNull(promptReload);

		if (IsRunning)
			return;

		IsRunning = true;

		try
		{
			foreach (string filePath in _pendingFileReloads)
			{
				try
				{
					IReadOnlyList<IEditorControl> editors = getEditorsOfFile(filePath);

					if (editors.Count > 0)
						TryReloadEditors(editors, promptReload);
				}
				catch
				{ }
			}

			_pendingFileReloads.Clear();
		}
		finally
		{
			IsRunning = false;
		}
	}

	private static void TryReloadEditors(IReadOnlyList<IEditorControl> editors, Func<string, DialogResult> promptReload)
	{
		DialogResult? result = null;

		foreach (IEditorControl editor in editors)
		{
			if (string.IsNullOrWhiteSpace(editor.FilePath) || !File.Exists(editor.FilePath))
				continue;

			string fileContent = File.ReadAllText(editor.FilePath);

			if (editor.Content == fileContent)
				continue;

			if (result is null)
				result = promptReload(editor.FilePath);

			if (result == DialogResult.Yes)
			{
				fileContent = File.ReadAllText(editor.FilePath);
				editor.ApplyPersistedContent(fileContent);
			}
			else if (result == DialogResult.No)
			{
				editor.TryRunContentChangedWorker();
			}
		}
	}
}
