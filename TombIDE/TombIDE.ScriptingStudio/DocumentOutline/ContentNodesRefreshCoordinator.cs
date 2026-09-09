#nullable enable

using DarkUI.Controls;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using TombLib.Scripting.UI.ContentNodes;

namespace TombIDE.ScriptingStudio.DocumentOutline;

internal sealed class ContentNodesRefreshCoordinator
{
	private int _latestRefreshRequestId;

	public void InvalidatePendingRequests()
		=> Interlocked.Increment(ref _latestRefreshRequestId);

	public void RequestRefresh(
		ContentNodesProviderBase nodesProvider,
		string content,
		string filter,
		Func<ContentNodesProviderBase, bool> canApplyRefresh,
		Action<IReadOnlyList<DarkTreeNode>> applyNodes)
	{
		ArgumentNullException.ThrowIfNull(nodesProvider);
		ArgumentNullException.ThrowIfNull(canApplyRefresh);
		ArgumentNullException.ThrowIfNull(applyNodes);

		int requestId = Interlocked.Increment(ref _latestRefreshRequestId);
		_ = RefreshAsync(nodesProvider, content ?? string.Empty, filter ?? string.Empty, requestId, canApplyRefresh, applyNodes);
	}

	private async Task RefreshAsync(
		ContentNodesProviderBase nodesProvider,
		string content,
		string filter,
		int requestId,
		Func<ContentNodesProviderBase, bool> canApplyRefresh,
		Action<IReadOnlyList<DarkTreeNode>> applyNodes)
	{
		IReadOnlyList<DarkTreeNode> nodes;

		try
		{
			nodes = await Task.Run(() => nodesProvider.GetNodes(content, filter));
		}
		catch
		{
			return;
		}

		if (requestId != Volatile.Read(ref _latestRefreshRequestId) || !canApplyRefresh(nodesProvider))
			return;

		applyNodes(nodes);
	}
}
