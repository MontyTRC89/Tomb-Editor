using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using TombLib.Scripting.Lua.Objects;
using TombLib.Scripting.Objects;

namespace TombLib.Scripting.Lua.Services
{
	public interface ILuaIntellisenseProvider : IDisposable
	{
		bool IsAvailable { get; }
		event Action<string, IReadOnlyList<TextEditorDiagnostic>> DiagnosticsUpdated;

		IReadOnlyList<TextEditorDiagnostic> GetDiagnostics(string filePath);

		void OpenDocument(string filePath, string content);
		void UpdateDocument(string filePath, string content);
		void CloseDocument(string filePath);

		Task<IReadOnlyList<LuaCompletionItem>> GetCompletionItemsAsync(string filePath, string content,
			int line, int column, CancellationToken cancellationToken = default);

		Task<LuaHoverInfo> GetHoverAsync(string filePath, string content,
			int line, int column, CancellationToken cancellationToken = default);

		Task<LuaDefinitionLocation> GetDefinitionAsync(string filePath, string content,
			int line, int column, CancellationToken cancellationToken = default);

		Task<LuaSignatureInfo> GetSignatureHelpAsync(string filePath, string content,
			int line, int column, CancellationToken cancellationToken = default);
	}
}