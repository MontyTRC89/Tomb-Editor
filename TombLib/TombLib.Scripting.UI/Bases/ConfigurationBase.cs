using NLog;
using System;
using System.IO;
using TombLib.Utils;

namespace TombLib.Scripting.UI.Bases;

public abstract class ConfigurationBase
{
	private static readonly Logger Log = LogManager.GetCurrentClassLogger();

	public abstract string DefaultPath { get; }

	// Loading

	/// <summary>
	/// Loads the configuration from a stream.
	/// </summary>
	/// <typeparam name="T">The configuration type to load.</typeparam>
	/// <param name="stream">The stream to read the configuration from.</param>
	/// <returns>
	/// The loaded configuration, or a new default instance when the stream is missing, empty or corrupt.
	/// The fallback instance is not saved automatically; callers save it to persist the defaults.
	/// </returns>
	public T Load<T>(Stream stream) where T : ConfigurationBase, new()
	{
		try
		{
			return XmlUtils.ReadXmlFile<T>(stream);
		}
		catch (Exception exception)
		{
			Log.Warn(exception, "Configuration '{Type}' could not be loaded from a stream; using defaults.", typeof(T).Name);
			return new T();
		}
	}

	/// <summary>
	/// Loads the configuration from a file.
	/// </summary>
	/// <typeparam name="T">The configuration type to load.</typeparam>
	/// <param name="filePath">The path of the configuration file to read.</param>
	/// <returns>
	/// The loaded configuration, or a new default instance when the file is missing or corrupt.
	/// The fallback instance is not saved automatically; callers save it to persist the defaults.
	/// </returns>
	public T Load<T>(string filePath) where T : ConfigurationBase, new()
	{
		try
		{
			return XmlUtils.ReadXmlFile<T>(filePath);
		}
		catch (Exception exception)
		{
			Log.Warn(exception, "Configuration '{Type}' could not be loaded from '{Path}'; using defaults.", typeof(T).Name, filePath);
			return new T();
		}
	}

	public T Load<T>() where T : ConfigurationBase, new()
		=> Load<T>(DefaultPath);

	// Saving

	public void Save(Stream stream)
		=> XmlUtils.WriteXmlFile(stream, GetType(), this);

	public void Save(string path)
	{
		string? directoryName = Path.GetDirectoryName(path);

		if (!string.IsNullOrEmpty(directoryName) && !Directory.Exists(directoryName))
			Directory.CreateDirectory(directoryName);

		XmlUtils.WriteXmlFile(path, GetType(), this);
	}

	public void Save()
		=> Save(DefaultPath);

}
