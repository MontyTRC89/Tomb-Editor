namespace TombLib.Messages;

public sealed record InputBoxWindowCloseMessage(bool IsOK, string Value);
