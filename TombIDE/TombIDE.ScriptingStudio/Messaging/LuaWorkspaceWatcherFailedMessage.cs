#nullable enable

using CommunityToolkit.Mvvm.Messaging.Messages;
using Nickelony.LanguageServer.Core;

namespace TombIDE.ScriptingStudio.Messaging;

/// <summary>
/// Signals that the Lua workspace file watcher failed.
/// </summary>
public sealed class LuaWorkspaceWatcherFailedMessage(WorkspaceWatcherFailure failure)
	: ValueChangedMessage<WorkspaceWatcherFailure>(failure);
