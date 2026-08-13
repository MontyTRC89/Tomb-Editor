using Microsoft.Extensions.DependencyInjection;
using System.Runtime.ExceptionServices;
using TombLib.WPF.Services;
using TombLib.WPF.Services.Abstract;

namespace TombEditor.Tests.ScriptingStudio;

internal static class StaTestHelper
{
    public static void RunInSta(Action action)
    {
        ExceptionDispatchInfo? capturedException = null;

        var thread = new Thread(() =>
        {
            try
            {
                ConfigureTestServiceLocator();
                action();
            }
            catch (Exception exception)
            {
                capturedException = ExceptionDispatchInfo.Capture(exception);
            }
        });

        thread.SetApartmentState(ApartmentState.STA);
        thread.Start();
        thread.Join();

        capturedException?.Throw();
    }

    /// <summary>
    /// Configures the global <see cref="ServiceLocator"/> with a minimal set
    /// of services required for unit testing ScriptingStudio components.
    /// Call once per test assembly or before tests that resolve services.
    /// </summary>
    public static void ConfigureTestServiceLocator()
    {
        var services = new ServiceCollection();
        services.AddSingleton<ILocalizationService>(new TestLocalizationService());
        ServiceLocator.Configure(services.BuildServiceProvider());
    }

    private sealed class TestLocalizationService : ILocalizationService
    {
        public string? NamespaceName => null;

        public string? ComponentName => null;

        public string this[string key] => key;

        public string Format(string key, params object[] args)
            => string.Format(this[key], args);

        public ILocalizationService WithKeysFor(System.ComponentModel.INotifyPropertyChanged viewModel) => this;
    }
}
