namespace TombLib.LanguageServer.Core;

/// <summary>
/// Describes the host-specific payload factories used to initialize a language-server client.
/// </summary>
public sealed class LanguageServerClientOptions
{
	private TimeSpan _initializeTimeout = TimeSpan.FromSeconds(20.0f);
	private TimeSpan _shutdownRequestTimeout = TimeSpan.FromSeconds(3.0f);
	private TimeSpan _disposeWaitTimeout = TimeSpan.FromSeconds(5.0f);

	/// <summary>
	/// Stores the client capabilities payload factory.
	/// </summary>
	private Func<string, object?> _clientCapabilitiesProvider = static _ => new { };

	/// <summary>
	/// Stores the initialization options payload factory.
	/// </summary>
	private Func<string, object?> _initializationOptionsProvider = static _ => new { };

	/// <summary>
	/// Initializes a new instance of the <see cref="LanguageServerClientOptions"/> class.
	/// </summary>
	/// <param name="settingsProvider">Produces the current settings payload for <c>workspace/didChangeConfiguration</c>.</param>
	public LanguageServerClientOptions(Func<object> settingsProvider)
	{
		ArgumentNullException.ThrowIfNull(settingsProvider);
		SettingsProvider = settingsProvider;
	}

	/// <summary>
	/// Gets the settings payload factory for <c>workspace/didChangeConfiguration</c>.
	/// </summary>
	public Func<object> SettingsProvider { get; }

	/// <summary>
	/// Gets or initializes how long startup waits for the server to answer the <c>initialize</c> request.
	/// </summary>
	public TimeSpan InitializeTimeout
	{
		get => _initializeTimeout;
		init => _initializeTimeout = ValidateTimeout(value, nameof(InitializeTimeout));
	}

	/// <summary>
	/// Gets or initializes how long graceful shutdown waits for the server to answer the <c>shutdown</c> request.
	/// </summary>
	public TimeSpan ShutdownRequestTimeout
	{
		get => _shutdownRequestTimeout;
		init => _shutdownRequestTimeout = ValidateTimeout(value, nameof(ShutdownRequestTimeout));
	}

	/// <summary>
	/// Gets or initializes how long disposal waits for background transport work to quiesce before teardown continues.
	/// </summary>
	public TimeSpan DisposeWaitTimeout
	{
		get => _disposeWaitTimeout;
		init => _disposeWaitTimeout = ValidateTimeout(value, nameof(DisposeWaitTimeout));
	}

	/// <summary>
	/// Gets or initializes the client capabilities payload factory for the <c>initialize</c> request.
	/// </summary>
	public Func<string, object?> ClientCapabilitiesProvider
	{
		get => _clientCapabilitiesProvider;
		init => _clientCapabilitiesProvider = value ?? throw new ArgumentNullException(nameof(value));
	}

	/// <summary>
	/// Gets or initializes the language-specific initialization options factory for the <c>initialize</c> request.
	/// </summary>
	public Func<string, object?> InitializationOptionsProvider
	{
		get => _initializationOptionsProvider;
		init => _initializationOptionsProvider = value ?? throw new ArgumentNullException(nameof(value));
	}

	private static TimeSpan ValidateTimeout(TimeSpan value, string propertyName)
	{
		if (value <= TimeSpan.Zero)
			throw new ArgumentOutOfRangeException(propertyName, value, "The timeout must be greater than zero.");

		if (value == Timeout.InfiniteTimeSpan)
			throw new ArgumentOutOfRangeException(propertyName, value, "Infinite timeouts are not supported for transport lifecycle operations.");

		return value;
	}
}
