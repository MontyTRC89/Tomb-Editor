using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TombEditor.WPF.ViewModels
{
    public class OptionsViewModel : INotifyPropertyChanged
    {

		private readonly Configuration _config;

		#region Properties
		public bool IsAutoSaveEnabled
		{
			get => _config.AutoSave_Enable;
			set
			{
				_config.AutoSave_Enable = value;
				NotifyPropertyChanged(nameof(IsAutoSaveEnabled));
			}
		}

		public int AutoSave_TimeInSeconds
		{
			get => _config.AutoSave_TimeInSeconds;
			set
			{
				_config.AutoSave_TimeInSeconds = value;
				NotifyPropertyChanged(nameof(AutoSave_TimeInSeconds));
			}
		}

		public string AutoSave_DateTimeFormat {
			get => _config.AutoSave_DateTimeFormat;
			set
			{
				_config.AutoSave_DateTimeFormat = value;
				NotifyPropertyChanged(nameof(AutoSave_DateTimeFormat));
			}
		}

		public bool AutoSave_CleanupEnable
		{
			get => _config.AutoSave_CleanupEnable;
			set
			{
				_config.AutoSave_CleanupEnable = value;
				NotifyPropertyChanged(nameof(AutoSave_CleanupEnable));
			}
		}

		public int AutoSave_CleanupMaxAutoSaves
		{
			get => _config.AutoSave_CleanupMaxAutoSaves;
			set
			{
				_config.AutoSave_CleanupMaxAutoSaves = value;
				NotifyPropertyChanged(nameof(AutoSave_CleanupMaxAutoSaves));
			}
		}

		public bool AutoSave_NamePutDateFirst
		{
			get => _config.AutoSave_NamePutDateFirst;
			set
			{
				_config.AutoSave_NamePutDateFirst = value;
				NotifyPropertyChanged(nameof(AutoSave_NamePutDateFirst));
			}
		}

		public string AutoSave_NameSeparator
		{
			get => _config.AutoSave_NameSeparator;
			set
			{
				_config.AutoSave_NameSeparator = value;
				NotifyPropertyChanged(nameof(AutoSave_NameSeparator));
			}
		}


		#endregion


		public OptionsViewModel(Configuration config)
        {
			this._config = config;
		}

		public event PropertyChangedEventHandler? PropertyChanged;

		private void NotifyPropertyChanged(string prop)
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prop));
		}
	}
}
