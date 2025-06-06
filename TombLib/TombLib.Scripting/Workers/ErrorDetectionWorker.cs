﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows.Threading;
using TombLib.Scripting.Interfaces;

namespace TombLib.Scripting.Workers
{
	public class ErrorDetectionWorker : BackgroundWorker
	{
		#region Properties

		public IErrorDetector ErrorDetector { get; set; }

		public TimeSpan IdleDelayInterval
		{
			get => _errorUpdateTimer.Interval;
			set => _errorUpdateTimer.Interval = value;
		}

		public Version EngineVersion { get; set; }

		#endregion Properties

		#region Fields

		private DispatcherTimer _errorUpdateTimer = new();

		private string _editorContent = string.Empty;

		#endregion Fields

		#region Construction

		public ErrorDetectionWorker() : this(null, new Version(0, 0))
		{ }
		public ErrorDetectionWorker(IErrorDetector errorDetector, Version engineVersion) : this(errorDetector, engineVersion, new TimeSpan(500))
		{ }
		public ErrorDetectionWorker(IErrorDetector errorDetector, Version engineVersion, TimeSpan idleDelayInterval)
		{
			ErrorDetector = errorDetector;
			IdleDelayInterval = idleDelayInterval;
			EngineVersion = engineVersion;

			_errorUpdateTimer.Tick += ErrorUpdateTimer_Tick;
		}

		#endregion Construction

		#region Override methods

		protected override void OnDoWork(DoWorkEventArgs e)
		{
			base.OnDoWork(e);

			var errorDetector = (e.Argument as List<object>)[0] as IErrorDetector;
			string editorContent = (e.Argument as List<object>)[1].ToString();

			e.Result = errorDetector.FindErrors(editorContent, EngineVersion);
		}

		#endregion Override methods

		#region Public methods

		public void RunErrorCheckOnIdle(string editorContent)
		{
			if (_errorUpdateTimer.IsEnabled)
				_errorUpdateTimer.Stop();

			_editorContent = editorContent;
			_errorUpdateTimer.Start();
		}

		public void CheckForErrorsAsync(string editorContent)
		{
			if (ErrorDetector == null)
				return;

			var args = new List<object>
			{
				ErrorDetector,
				editorContent
			};

			base.RunWorkerAsync(args);
		}

		#endregion Public methods

		#region Events

		private void ErrorUpdateTimer_Tick(object sender, EventArgs e)
		{
			if (!IsBusy)
				CheckForErrorsAsync(_editorContent);

			_errorUpdateTimer.Stop();
		}

		#endregion Events

		#region Obsolete methods

		[Obsolete("Use CheckForErrorsAsync() instead!", true)]
		public new void RunWorkerAsync()
			=> base.RunWorkerAsync();

		[Obsolete("Use CheckForErrorsAsync() instead!", true)]
		public new void RunWorkerAsync(object argument)
			=> base.RunWorkerAsync(argument);

		#endregion Obsolete methods
	}
}
