using System.Text.Json.Serialization;

namespace Nickelony.LanguageServer.Core;

/// <summary>
/// Represents the typed payload for a <c>workspace/didChangeWatchedFiles</c> notification.
/// </summary>
/// <param name="Changes">The watched file changes to report.</param>
public readonly record struct DidChangeWatchedFilesParams(
	[property: JsonPropertyName("changes")] FileEventPayload[] Changes);

/// <summary>
/// Represents a single watched file event.
/// </summary>
/// <param name="Uri">The affected file URI.</param>
/// <param name="Type">The protocol change kind.</param>
public readonly record struct FileEventPayload(
	[property: JsonPropertyName("uri")] string Uri,
	[property: JsonPropertyName("type")] int Type);
