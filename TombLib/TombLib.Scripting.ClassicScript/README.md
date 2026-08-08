# TombLib.Scripting.ClassicScript

Purpose: ClassicScript language provider for the classic Tomb Raider script format.

Implements the shared neutral provider contracts for ClassicScript:
- completion - `ClassicScriptCompletionProvider`
- signature help - `ClassicScriptSignatureHelpProvider`
- hover - `ClassicScriptHoverProvider`
- definition - `ClassicScriptDefinitionProvider`
- diagnostics - `ErrorDetector`
- formatting - `ClassicScriptDocumentFormatter`

Provides the editor (`ClassicScriptEditor`), configuration
(`ClassicScriptEditorConfiguration`), the language-services composition root
(`ClassicScriptLanguageServices`), command and syntax catalogs, mnemonic and description
tables, index and reference services, compilers (`NGCompiler`, `TR4Compiler`), content
nodes, and color scheme resources.

Depends on `TombLib.Scripting.UI` and `TombLib.Scripting`. See
[ScriptingLibraries_Architecture.md](../ScriptingLibraries_Architecture.md) for the provider
standards and the extension-point guide.
