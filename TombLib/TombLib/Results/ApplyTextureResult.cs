namespace TombLib.Results;

/// <summary>
/// Result of applying a texture to a face in a room.
/// </summary>
public readonly struct ApplyTextureResult
{
	/// <summary>
	/// Whether the texture application was successful - changes were made.
	/// </summary>
	public bool Success { get; }

	/// <summary>
	/// Whether the geometry needs to be rebuilt after applying the texture.
	/// </summary>
	public bool NeedsGeometryRebuild { get; }

	public ApplyTextureResult(bool success, bool needsGeometryRebuild)
	{
		Success = success;
		NeedsGeometryRebuild = needsGeometryRebuild;
	}

	/// <summary>
	/// Result indicating that the texture application was not successful - no changes were made.
	/// </summary>
	public static ApplyTextureResult NoChange => new(false, false);
}
