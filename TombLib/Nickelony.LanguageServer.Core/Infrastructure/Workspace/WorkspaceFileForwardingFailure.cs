namespace Nickelony.LanguageServer.Core;

/// <summary>
/// Describes one workspace file forwarding failure together with the affected batch context.
/// </summary>
/// <param name="Exception">The underlying forwarding exception.</param>
/// <param name="BatchCount">The number of file changes in the affected batch.</param>
/// <param name="FirstPath">The first affected normalized path, when available.</param>
/// <param name="WasDropped">Whether the batch was dropped instead of buffered for replay.</param>
public readonly record struct WorkspaceFileForwardingFailure(Exception Exception, int BatchCount, string? FirstPath, bool WasDropped);
