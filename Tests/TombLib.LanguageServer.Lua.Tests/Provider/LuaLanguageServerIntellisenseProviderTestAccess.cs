using System.Reflection;

namespace Nickelony.LanguageServer.Lua.Tests;

internal static class LuaLanguageServerIntellisenseProviderTestAccess
{
	public static Task DispatchWorkspaceFileChangesAsync(
		LuaLanguageServerIntellisenseProvider provider,
		FileChangeBatch batch,
		CancellationToken cancellationToken)
		=> GetWorkspaceChangeCoordinator(provider).DispatchWorkspaceFileChangesAsync(batch, cancellationToken);

	public static WorkspaceFileWatcher? GetWorkspaceWatcher(LuaLanguageServerIntellisenseProvider provider)
	{
		LuaWorkspaceChangeCoordinator coordinator = GetWorkspaceChangeCoordinator(provider);

		PropertyInfo currentWatcherProperty = coordinator.GetType().GetProperty("CurrentWatcher", BindingFlags.Instance | BindingFlags.NonPublic)
			?? throw new InvalidOperationException("Property 'CurrentWatcher' was not found on the workspace change coordinator.");

		return currentWatcherProperty.GetValue(coordinator) as WorkspaceFileWatcher;
	}

	public static SemaphoreSlim GetProviderStartLock(LuaLanguageServerIntellisenseProvider provider)
	{
		FieldInfo field = typeof(LuaLanguageServerIntellisenseProvider).GetField("_startLock", BindingFlags.Instance | BindingFlags.NonPublic)
			?? throw new InvalidOperationException("Private field '_startLock' was not found.");

		return (SemaphoreSlim)(field.GetValue(provider)
			?? throw new InvalidOperationException("Provider start lock was null."));
	}

	public static CancellationTokenSource GetProviderDisposeCancellationTokenSource(LuaLanguageServerIntellisenseProvider provider)
	{
		FieldInfo field = typeof(LuaLanguageServerIntellisenseProvider).GetField("_disposeCts", BindingFlags.Instance | BindingFlags.NonPublic)
			?? throw new InvalidOperationException("Private field '_disposeCts' was not found.");

		return (CancellationTokenSource)(field.GetValue(provider)
			?? throw new InvalidOperationException("Provider dispose token source was null."));
	}

	public static int GetTrackedDocumentCount(LuaLanguageServerIntellisenseProvider provider)
	{
		FieldInfo field = typeof(LuaLanguageServerIntellisenseProvider).GetField("_documents", BindingFlags.Instance | BindingFlags.NonPublic)
			?? throw new InvalidOperationException("Private field '_documents' was not found.");

		var documentStore = (LuaDocumentStore)(field.GetValue(provider)
			?? throw new InvalidOperationException("Provider document store was null."));

		return documentStore.TrackedDocumentCount;
	}

	public static LuaWorkspaceChangeCoordinator GetWorkspaceChangeCoordinator(LuaLanguageServerIntellisenseProvider provider)
	{
		FieldInfo coordinatorField = typeof(LuaLanguageServerIntellisenseProvider).GetField("_workspaceChanges", BindingFlags.Instance | BindingFlags.NonPublic)
			?? throw new InvalidOperationException("Private field '_workspaceChanges' was not found.");

		return (LuaWorkspaceChangeCoordinator)(coordinatorField.GetValue(provider)
			?? throw new InvalidOperationException("Provider workspace change coordinator was null."));
	}
}
