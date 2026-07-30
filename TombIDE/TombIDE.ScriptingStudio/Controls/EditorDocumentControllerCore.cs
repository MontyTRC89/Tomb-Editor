using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using TombIDE.ScriptingStudio.Editors;
using TombIDE.ScriptingStudio.Helpers;
using TombIDE.ScriptingStudio.UI;
using TombLib.Scripting.ClassicScript.Documents;
using TombLib.Scripting.ClassicScript.Services;
using TombLib.Scripting.UI.Editors;

namespace TombIDE.ScriptingStudio.Controls
{
	internal readonly record struct EditorOpenResult(IEditorControl Editor, bool IsNewDocument);

	internal sealed class EditorDocumentControllerCore
	{
		private readonly EditorFactoryService _editorFactory = new EditorFactoryService();
		private readonly List<IEditorControl> _openEditors = new List<IEditorControl>();
		private readonly Version _currentEngineVersion;
		private readonly IClassicScriptLineService? _lineService;

		public EditorDocumentControllerCore(Version currentEngineVersion, IClassicScriptLineService? lineService = null)
		{
			_currentEngineVersion = currentEngineVersion ?? throw new ArgumentNullException(nameof(currentEngineVersion));
			_lineService = lineService;
		}

		public DocumentMode GetDocumentMode(IEditorControl editor)
			=> editor is null ? DocumentMode.None : _editorFactory.GetDocumentMode(editor);

		public string GetDocumentTitle(IEditorControl editor)
			=> _editorFactory.BuildTabTitle(editor?.FilePath, editor?.EditorType ?? EditorType.Default);

		public IEnumerable<IEditorControl> GetOpenEditors()
			=> _openEditors;

		public List<string> GetFilePaths()
			=> _openEditors
				.Where(editor => editor != null && !string.IsNullOrWhiteSpace(editor.FilePath))
				.Select(editor => editor.FilePath)
				.Distinct(StringComparer.OrdinalIgnoreCase)
				.ToList();

		public IEnumerable<IEditorControl> FindEditorsOfFile(string filePath)
			=> _openEditors.Where(editor => editor.FilePath.Equals(filePath, StringComparison.OrdinalIgnoreCase));

		public IEditorControl FindEditor(string filePath, EditorType editorType = EditorType.Default)
		{
			if (editorType == EditorType.Default)
				editorType = _editorFactory.GetDefaultEditorType(filePath);

			return _openEditors.FirstOrDefault(editor =>
				editor.FilePath.Equals(filePath, StringComparison.OrdinalIgnoreCase)
				&& editor.EditorType == editorType);
		}

		public IEditorControl FindSourceEditor(string filePath)
			=> FindEditor(filePath, _editorFactory.GetSourceViewEditorType(filePath));

		public IEditorControl GetMostRecentlyModifiedEditorOfFile(string filePath)
		{
			IEditorControl mostRecentEditor = null;

			foreach (IEditorControl editor in FindEditorsOfFile(filePath))
			{
				if (mostRecentEditor is null || editor.LastModified > mostRecentEditor.LastModified)
					mostRecentEditor = editor;
			}

			return mostRecentEditor;
		}

		public bool IsMostRecentlyModifiedEditorOfFile(IEditorControl editor)
		{
			if (editor is null)
				return false;

			IEditorControl mostRecentEditor = GetMostRecentlyModifiedEditorOfFile(editor.FilePath);
			return mostRecentEditor is not null && editor.LastModified == mostRecentEditor.LastModified;
		}

		public EditorType GetSourceViewEditorType(string filePath)
			=> _editorFactory.GetSourceViewEditorType(filePath);

		public bool ContainsEditor(IEditorControl editor)
			=> editor != null && _openEditors.Contains(editor);

		public EditorOpenResult OpenFile(string filePath, EditorType editorType = EditorType.Default, bool silentSession = false)
		{
			IEditorControl existingEditor = FindEditor(filePath, editorType);

			if (existingEditor != null)
				return new EditorOpenResult(existingEditor, false);

			IEditorControl newEditor = InitializeEditor(filePath, editorType, silentSession);

			if (newEditor is null)
				return default;

			_openEditors.Add(newEditor);

			return new EditorOpenResult(newEditor, true);
		}

		public void RegisterJson5Editor(Func<Version, IEditorControl> factory, DocumentMode documentMode)
			=> RegisterEditor(EditorType.Text, documentMode, FileHelper.IsJson5File, static _ => true, factory);

		public void RegisterLuaEditor(Func<Version, IEditorControl> factory, DocumentMode documentMode)
			=> RegisterEditor(EditorType.Text, documentMode, FileHelper.IsLuaFile, static _ => true, factory);

		public void RegisterPlainTextEditor(Func<Version, IEditorControl> factory, DocumentMode documentMode)
			=> _editorFactory.SetPlainTextEditorFactory(factory, documentMode);

		public void RegisterStringsEditor(Func<Version, IEditorControl> factory)
		{
			if (_lineService is null)
				return;

			RegisterEditor(
				EditorType.Strings,
				DocumentMode.Strings,
				FileHelper.IsTextFile,
				filePath => FileHelper.GetClassicScriptFileKind(filePath, _lineService) == ClassicScriptFileKind.Strings,
				factory);
		}

		public void RegisterTextEditor(Func<Version, IEditorControl> factory, DocumentMode documentMode)
			=> RegisterTextEditor(factory, documentMode, _ => true);

		public void RegisterTextEditor(
			Func<Version, IEditorControl> factory,
			DocumentMode documentMode,
			Func<string, bool> isDefaultForFile)
			=> RegisterEditor(EditorType.Text, documentMode, FileHelper.IsTextFile, isDefaultForFile, factory);

		public void RemoveEditor(IEditorControl editor)
		{
			if (editor is null)
				return;

			_openEditors.Remove(editor);
		}

		private IEditorControl InitializeEditor(string filePath, EditorType editorType, bool silentSession)
		{
			IEditorControl newEditor = _editorFactory.CreateEditor(filePath, editorType, _currentEngineVersion);

			if (newEditor is null)
				return null;

			if (File.Exists(filePath))
				newEditor.Load(filePath, silentSession);
			else
				newEditor.FilePath = filePath;

			return newEditor;
		}

		private void RegisterEditor(
			EditorType editorType,
			DocumentMode documentMode,
			Func<string, bool> supportsFile,
			Func<string, bool> isDefaultForFile,
			Func<Version, IEditorControl> factory)
			=> _editorFactory.Register(new EditorRegistration(editorType, documentMode, supportsFile, isDefaultForFile, factory));
	}
}
