using System.ComponentModel;
using TombLib.WPF.Services.Abstract;

namespace TombLib.WPF.Services;

public sealed class LocalizationService : ILocalizationService
{
	public string? NamespaceName { get; private set; }
	public string? ComponentName { get; private set; }

	public ILocalizationService For(INotifyPropertyChanged viewModel)
	{
		Type viewModelType = viewModel.GetType();

		NamespaceName = viewModelType.Namespace?.Split('.')[0];
		ComponentName = viewModelType.Name.Replace("ViewModel", "");

		return this;
	}

	public string this[string key]
	{
		get
		{
			string fullKey = key;

			if (!string.IsNullOrEmpty(ComponentName))
				fullKey = $"{ComponentName}.{fullKey}";

			if (!string.IsNullOrEmpty(NamespaceName))
				fullKey = $"{NamespaceName}.{fullKey}";

			return Localizer.Instance[fullKey];
		}
	}

	public string Format(string key, params object[] args)
		=> string.Format(this[key], args);
}
