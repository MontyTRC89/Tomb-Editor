using System.Text;

namespace ScriptLib.Core.Workers;

/// <summary>
/// Defines operations for working with file content, including change detection and backup functionality.
/// </summary>
public interface IFileContentWorker : IDisposable
{
	/// <summary>
	/// Gets the path to the file being monitored.
	/// </summary>
	string FilePath { get; }

	/// <summary>
	/// Gets a value indicating whether this worker creates backup files when content changes.
	/// </summary>
	bool CreatesBackupFile { get; }

	/// <summary>
	/// Determines whether the file content has changed compared to the provided content.
	/// </summary>
	/// <param name="currentContent">The current content to compare against the file.</param>
	/// <param name="encoding">The encoding to use when reading the file.</param>
	/// <returns>
	/// A task that represents the asynchronous operation.
	/// The task result contains <c>true</c> if the content has changed; otherwise, <c>false</c>.
	/// </returns>
	Task<bool> HasContentChanged(string currentContent, Encoding encoding);
}
