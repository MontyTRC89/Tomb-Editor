namespace ScriptLib.Core;

/// <summary>
/// Base class for default configurations that provide fallback values when requested keys are not found.
/// </summary>
/// <typeparam name="TKeys">An enumeration type that defines the available configuration keys.</typeparam>
public abstract class DefaultConfigBase<TKeys> where TKeys : struct
{
	/// <summary>
	/// Gets the dictionary containing all default configuration values.
	/// </summary>
	public abstract IDictionary<TKeys, object> Values { get; }

	/// <summary>
	/// Gets a typed value from the default configuration.
	/// </summary>
	/// <typeparam name="T">The type to cast the value to.</typeparam>
	/// <param name="key">The configuration key.</param>
	/// <returns>The value associated with the key cast to the specified type.</returns>
	public T Get<T>(TKeys key)
		=> (T)Values[key];

	/// <summary>
	/// Gets an untyped value from the default configuration.
	/// </summary>
	/// <param name="key">The configuration key.</param>
	/// <returns>The value associated with the key.</returns>
	public object Get(TKeys key)
		=> Values[key];
}
