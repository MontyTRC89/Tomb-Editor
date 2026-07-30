#nullable enable

using CommunityToolkit.Mvvm.Messaging.Messages;
using TombLib.Scripting.Navigation;

namespace TombIDE.ScriptingStudio.Messaging;

/// <summary>
/// Requests navigation to a Lua definition location.
/// </summary>
public sealed class LuaDefinitionNavigationMessage(TextDefinitionLocation location)
	: ValueChangedMessage<TextDefinitionLocation>(location);
