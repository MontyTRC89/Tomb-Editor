namespace TombLib.LanguageServer.Core;

/// <summary>
/// The exception thrown when a request result no longer belongs to the active language-server transport.
/// </summary>
public sealed class LanguageServerTransportChangedException : IOException
{
	private const string DefaultMessage = "The language server transport changed before the request completed.";

	/// <summary>
	/// Initializes a new instance of the <see cref="LanguageServerTransportChangedException"/> class.
	/// </summary>
	public LanguageServerTransportChangedException()
		: base(DefaultMessage)
	{ }

	/// <summary>
	/// Initializes a new instance of the <see cref="LanguageServerTransportChangedException"/> class.
	/// </summary>
	/// <param name="message">The exception message.</param>
	public LanguageServerTransportChangedException(string? message)
		: base(message ?? DefaultMessage)
	{ }

	/// <summary>
	/// Initializes a new instance of the <see cref="LanguageServerTransportChangedException"/> class.
	/// </summary>
	/// <param name="message">The exception message.</param>
	/// <param name="innerException">The inner exception.</param>
	public LanguageServerTransportChangedException(string? message, Exception? innerException)
		: base(message ?? DefaultMessage, innerException)
	{ }

	/// <summary>
	/// Initializes a new instance of the <see cref="LanguageServerTransportChangedException"/> class.
	/// </summary>
	/// <param name="message">The exception message.</param>
	/// <param name="hresult">The HRESULT error code.</param>
	public LanguageServerTransportChangedException(string? message, int hresult)
		: base(message, hresult)
	{ }
}
