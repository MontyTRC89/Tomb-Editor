namespace TombLib.LanguageServer.Core;

/// <summary>
/// The exception thrown when the active language-server transport becomes unavailable while sending an operation.
/// </summary>
public sealed class LanguageServerTransportUnavailableException : IOException
{
	private const string DefaultMessage = "The language server transport became unavailable before the request completed.";

	/// <summary>
	/// Initializes a new instance of the <see cref="LanguageServerTransportUnavailableException"/> class.
	/// </summary>
	public LanguageServerTransportUnavailableException()
		: base(DefaultMessage)
	{ }

	/// <summary>
	/// Initializes a new instance of the <see cref="LanguageServerTransportUnavailableException"/> class.
	/// </summary>
	/// <param name="message">The exception message.</param>
	public LanguageServerTransportUnavailableException(string? message)
		: base(message ?? DefaultMessage)
	{ }

	/// <summary>
	/// Initializes a new instance of the <see cref="LanguageServerTransportUnavailableException"/> class.
	/// </summary>
	/// <param name="message">The exception message.</param>
	/// <param name="innerException">The inner exception.</param>
	public LanguageServerTransportUnavailableException(string? message, Exception? innerException)
		: base(message ?? DefaultMessage, innerException)
	{ }
}
