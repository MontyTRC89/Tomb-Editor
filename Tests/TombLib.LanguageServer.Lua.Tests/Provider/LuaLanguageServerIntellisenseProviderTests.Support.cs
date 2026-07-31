using System.Reflection;

namespace Nickelony.LanguageServer.Lua.Tests;

public partial class LuaLanguageServerIntellisenseProviderTests
{
	private static PublishDiagnosticsParams CreateDiagnostics(string filePath, int? version, int startCharacter, int endCharacter, string message) => new(
		new Uri(filePath).AbsoluteUri,
		version,
		[
			new DiagnosticPayload(
				new ProtocolRangePayload(
					new ProtocolNullablePosition(0, startCharacter),
					new ProtocolNullablePosition(0, endCharacter)),
				2,
				message,
				null,
				null)
		]);

	private static WorkspaceFileWatcher? GetWorkspaceWatcher(LuaLanguageServerIntellisenseProvider provider)
		=> LuaLanguageServerIntellisenseProviderTestAccess.GetWorkspaceWatcher(provider);

	private static SemaphoreSlim GetProviderStartLock(LuaLanguageServerIntellisenseProvider provider)
		=> LuaLanguageServerIntellisenseProviderTestAccess.GetProviderStartLock(provider);

	private static CancellationTokenSource GetProviderDisposeCancellationTokenSource(LuaLanguageServerIntellisenseProvider provider)
		=> LuaLanguageServerIntellisenseProviderTestAccess.GetProviderDisposeCancellationTokenSource(provider);

	private static int GetTrackedDocumentCount(LuaLanguageServerIntellisenseProvider provider)
		=> LuaLanguageServerIntellisenseProviderTestAccess.GetTrackedDocumentCount(provider);

	private static int CountSentMethods(FakeLanguageServerClient client, string method)
	{
		int count = 0;
		string[] methods = client.GetSentMethodNames();

		for (int i = 0; i < methods.Length; i++)
		{
			if (string.Equals(methods[i], method, StringComparison.Ordinal))
				count++;
		}

		return count;
	}

	private static T InvokePrivateMethodWithReturn<T>(object instance, string methodName, params object?[] parameters)
	{
		MethodInfo method = instance.GetType().GetMethod(methodName, BindingFlags.Instance | BindingFlags.NonPublic)
			?? throw new InvalidOperationException($"Private method '{methodName}' was not found.");

		object? result = method.Invoke(instance, parameters);

		if (result is T typedResult)
			return typedResult;

		throw new InvalidOperationException($"Private method '{methodName}' returned '{result?.GetType().FullName ?? "null"}' instead of '{typeof(T).FullName}'.");
	}

	private static Task DispatchWorkspaceFileChangesAsync(LuaLanguageServerIntellisenseProvider provider, FileChangeBatch batch, CancellationToken cancellationToken)
		=> LuaLanguageServerIntellisenseProviderTestAccess.DispatchWorkspaceFileChangesAsync(provider, batch, cancellationToken);
}
