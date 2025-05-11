namespace ScriptLib.Core.Interfaces;

/// <summary>
/// Defines a generic configuration interface for storing and retrieving typed settings.
/// </summary>
/// <typeparam name="TKeys">An enumeration type that defines the available configuration keys.</typeparam>
public interface IConfiguration<TKeys>
{
	/// <summary>
	/// Event that is raised when the configuration is changed.
	/// </summary>
	event EventHandler<EventArgs> ConfigurationChanged;

	/// <summary>
	/// Gets or sets the configuration value associated with the specified key.
	/// </summary>
	/// <param name="key">The configuration key.</param>
	/// <returns>The configuration value associated with the key, or <see langword="null" /> if the key is not found.</returns>
	object? this[TKeys key] { get; set; }

	/// <summary>
	/// Gets a reference-type value from the configuration.
	/// </summary>
	/// <typeparam name="T">The reference type to retrieve.</typeparam>
	/// <param name="key">The configuration key.</param>
	/// <returns>The value associated with the key cast to the specified reference type.</returns>
	T GetReference<T>(TKeys key) where T : class;

	/// <summary>
	/// Gets a value-type value from the configuration.
	/// </summary>
	/// <typeparam name="T">The value type to retrieve.</typeparam>
	/// <param name="key">The configuration key.</param>
	/// <returns>The value associated with the key cast to the specified value type.</returns>
	T GetValue<T>(TKeys key) where T : struct;

	/// <summary>
	/// Saves the configuration to the same file path it was loaded from.
	/// </summary>
	void Save();
}
