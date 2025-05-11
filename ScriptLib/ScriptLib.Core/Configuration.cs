using ScriptLib.Core.Interfaces;
using System.IO;
using System.Xml.Linq;
using WeakEvent;

namespace ScriptLib.Core;

/// <summary>
/// Represents a configuration class that stores and retrieves typed settings.
/// </summary>
/// <typeparam name="TKeys">An enumeration type that defines the available configuration keys.</typeparam>
public sealed class Configuration<TKeys> : IConfiguration<TKeys> where TKeys : struct
{
	public string FilePath { get; }
	public DefaultConfigBase<TKeys> DefaultConfig { get; }

	private readonly WeakEventSource<EventArgs> _configurationChangedSource = new();
	public event EventHandler<EventArgs> ConfigurationChanged
	{
		add => _configurationChangedSource.Subscribe(value);
		remove => _configurationChangedSource.Unsubscribe(value);
	}

	private readonly Dictionary<TKeys, object?> _values = [];
	public object? this[TKeys key]
	{
		get => _values.TryGetValue(key, out object? value) ? value : null;
		set => _values[key] = value;
	}

	private Configuration(string filePath, DefaultConfigBase<TKeys> defaultConfig)
	{
		FilePath = filePath;
		DefaultConfig = defaultConfig;
	}

	public T GetReference<T>(TKeys key) where T : class
		=> (T)Get(key, typeof(T));

	public T GetValue<T>(TKeys key) where T : struct
		=> (T)Get(key, typeof(T));

	private object Get(TKeys key, Type type)
	{
		if (!_values.TryGetValue(key, out object? value) || value is null)
			return DefaultConfig.Get(key);

		try
		{
			return type.IsEnum
				? Enum.Parse(type, value.ToString()!)
				: Convert.ChangeType(value, type);
		}
		catch
		{
			return DefaultConfig.Get(key);
		}
	}

	public void Save()
		=> Save(FilePath);

	public void Save(string filePath)
	{
		using var stream = new FileStream(filePath, FileMode.Create, FileAccess.Write);

		var rootElement = new XElement("Configuration", _values.Select(kv => new XElement(kv.Key.ToString()!, kv.Value)));
		rootElement.Save(stream);

		_configurationChangedSource.Raise(this, EventArgs.Empty);
	}

	public static Configuration<TKeys> Load(string filePath, DefaultConfigBase<TKeys> defaultConfig)
	{
		try
		{
			using var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read);

			var rootElement = XElement.Load(stream);
			var configuration = new Configuration<TKeys>(filePath, defaultConfig);

			foreach (XElement element in rootElement.Elements())
				configuration[Enum.Parse<TKeys>(element.Name.LocalName)] = element.Value;

			return configuration;
		}
		catch
		{
			return new Configuration<TKeys>(filePath, defaultConfig);
		}
	}
}
