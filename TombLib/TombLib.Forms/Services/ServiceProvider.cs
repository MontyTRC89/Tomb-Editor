#nullable enable

using Microsoft.Extensions.DependencyInjection;
using System;

namespace TombLib.Services;

/// <summary>
/// Service locator for TombLib.Forms that provides easy access to dependency injection.
/// </summary>
public static class ServiceProvider
{
	private static IServiceProvider? _serviceProvider;
	private static readonly object _lock = new();

	/// <summary>
	/// Gets the current service provider instance.
	/// </summary>
	public static IServiceProvider Current
	{
		get
		{
			if (_serviceProvider == null)
			{
				lock (_lock)
					_serviceProvider ??= ServiceCollectionExtensions.CreateTombLibServiceProvider();
			}

			return _serviceProvider;
		}
	}

	/// <summary>
	/// Configures the service provider with a custom instance.
	/// </summary>
	/// <param name="serviceProvider">The service provider to use.</param>
	public static void Configure(IServiceProvider serviceProvider)
	{
		lock (_lock)
			_serviceProvider = serviceProvider;
	}

	/// <summary>
	/// Gets a service of the specified type.
	/// </summary>
	/// <typeparam name="T">The type of service to get.</typeparam>
	/// <returns>The service instance.</returns>
	public static T GetRequiredService<T>() where T : notnull
		=> Current.GetRequiredService<T>();

	/// <summary>
	/// Gets a service of the specified type, or <see langword="null" /> if not found.
	/// </summary>
	/// <typeparam name="T">The type of service to get.</typeparam>
	/// <returns>The service instance or <see langword="null" />.</returns>
	public static T? GetService<T>()
		=> Current.GetService<T>();

	/// <summary>
	/// Creates a new scope for scoped services.
	/// </summary>
	/// <returns>A new service scope.</returns>
	public static IServiceScope CreateScope()
		=> Current.CreateScope();
}
