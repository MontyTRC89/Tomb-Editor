using CommunityToolkit.Mvvm.Messaging;
using Microsoft.Extensions.DependencyInjection;
using MvvmDialogs;
using System;

namespace TombLib.Services;

/// <summary>
/// Extension methods for configuring dependency injection services for TombLib.Forms
/// </summary>
public static class ServiceCollectionExtensions
{
	/// <summary>
	/// Adds TombLib.Forms services to the dependency injection container
	/// </summary>
	/// <param name="services">The service collection to add services to</param>
	/// <returns>The service collection for chaining</returns>
	public static IServiceCollection AddTombLibFormServices(this IServiceCollection services)
	{
		// Register core services
		services.AddSingleton<IMessenger>(WeakReferenceMessenger.Default);
		services.AddSingleton<IDialogService, DialogService>();

		return services;
	}

	/// <summary>
	/// Creates a configured service provider for TombLib.Forms
	/// </summary>
	/// <returns>A configured service provider</returns>
	public static IServiceProvider CreateTombLibServiceProvider()
	{
		var services = new ServiceCollection();
		services.AddTombLibFormServices();
		return services.BuildServiceProvider();
	}
}
