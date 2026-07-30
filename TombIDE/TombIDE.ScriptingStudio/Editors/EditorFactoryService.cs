#nullable enable

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using TombIDE.ScriptingStudio.UI;
using TombLib.Scripting.UI.Bases;
using TombLib.Scripting.UI.Editors;

namespace TombIDE.ScriptingStudio.Editors;

internal sealed class EditorFactoryService
{
	private readonly ConditionalWeakTable<IEditorControl, EditorRegistration> _editorRegistrations = [];
	private readonly List<EditorRegistration> _registrations = [];

	private DocumentMode _plainTextDocumentMode = DocumentMode.PlainText;
	private Func<Version, IEditorControl>? _plainTextEditorFactoryOverride;

	public string BuildTabTitle(string filePath, EditorType editorType)
	{
		string tabTypeText = editorType == GetDefaultEditorType(filePath) ? string.Empty : $" [{editorType}]";
		return Path.GetFileName(filePath) + tabTypeText;
	}

	public IEditorControl CreateEditor(string filePath, EditorType editorType, Version engineVersion)
	{
		EditorRegistration? registration = ResolveRegistration(filePath, editorType);
		IEditorControl editor = (registration?.Factory ?? ResolvePlainTextFactory()).Invoke(engineVersion);

		if (registration is not null)
			_editorRegistrations.Add(editor, registration);

		return editor;
	}

	public DocumentMode GetDocumentMode(IEditorControl editor)
	{
		if (_editorRegistrations.TryGetValue(editor, out EditorRegistration? registration))
			return registration.DocumentMode;

		return _registrations.FirstOrDefault(candidate => candidate.EditorType == editor.EditorType)?.DocumentMode
			?? _plainTextDocumentMode;
	}

	public EditorType GetDefaultEditorType(string filePath)
		=> ResolveDefaultRegistration(filePath)?.EditorType ?? EditorType.Text;

	public EditorType GetSourceViewEditorType(string filePath)
	{
		return _registrations.FirstOrDefault(registration => registration.SupportsFile(filePath) && registration.EditorType == EditorType.Text)?.EditorType
			?? GetDefaultEditorType(filePath);
	}

	public void Register(EditorRegistration registration)
	{
		ArgumentNullException.ThrowIfNull(registration);

		_registrations.Add(registration);
	}

	public void SetPlainTextEditorFactory(Func<Version, IEditorControl>? factory, DocumentMode documentMode)
	{
		_plainTextEditorFactoryOverride = factory;
		_plainTextDocumentMode = documentMode;
	}

	private EditorRegistration? ResolveDefaultRegistration(string filePath)
	{
		return _registrations.FirstOrDefault(registration => registration.SupportsFile(filePath) && registration.IsDefaultForFile(filePath))
			?? _registrations.FirstOrDefault(registration => registration.SupportsFile(filePath));
	}

	private Func<Version, IEditorControl> ResolvePlainTextFactory()
	{
		if (_plainTextEditorFactoryOverride is not null)
			return _plainTextEditorFactoryOverride;

		return static engineVersion => new PlainTextEditor(engineVersion);
	}

	private EditorRegistration? ResolveRegistration(string filePath, EditorType editorType)
	{
		EditorRegistration? defaultRegistration = ResolveDefaultRegistration(filePath);

		if (editorType == EditorType.Default)
			return defaultRegistration;

		return _registrations.FirstOrDefault(registration => registration.SupportsFile(filePath) && registration.EditorType == editorType)
			?? defaultRegistration;
	}
}
