using System;
using System.ComponentModel;
using System.IO;
using System.Text;

namespace TombLib.Scripting.Workers
{
	public class ContentChangedWorker : BackgroundWorker
	{
		#region Properties

		private volatile string _filePath;
		public string FilePath
		{
			get => _filePath;
			set
			{
				if (value == null || !value.Equals(_filePath, StringComparison.OrdinalIgnoreCase))
					DeleteBackupFile();

				_filePath = value;
			}
		}

		private volatile bool _createBackupFiles;
		public bool CreateBackupFiles
		{
			get => _createBackupFiles;
			set
			{
				if (value == false)
					DeleteBackupFile();

				_createBackupFiles = value;
			}
		}

		#endregion Properties

		#region Construction

		public ContentChangedWorker() : this(string.Empty)
		{ }
		public ContentChangedWorker(string filePath) : this(filePath, true)
		{ }
		public ContentChangedWorker(string filePath, bool createBackupFiles)
		{
			FilePath = filePath;
			CreateBackupFiles = createBackupFiles;
		}

		#endregion Construction

		#region Override methods

		protected override void Dispose(bool disposing)
		{
			base.Dispose(disposing);

			DeleteBackupFile();
		}

		protected override void OnDoWork(DoWorkEventArgs e)
		{
			base.OnDoWork(e);

			try
			{
				string editorContent = e.Argument.ToString();
				string fileContent = File.ReadAllText(_filePath, Encoding.GetEncoding(1252));

				bool isChanged = editorContent != fileContent;

				if (_createBackupFiles)
					try
					{
						if (isChanged)
							CreateBackupFile(_filePath, editorContent);
						else
							DeleteBackupFile(_filePath); // We don't need the backup file when there are no changes made to the original file
					}
					catch { }

				e.Result = isChanged;
			}
			catch
			{
				e.Result = true;
			}
		}

		#endregion Override methods

		#region Public methods

		public void RunAsync(string editorContent)
		{
			if (string.IsNullOrEmpty(FilePath))
				return;

			base.RunWorkerAsync(editorContent);
		}

		public void CreateBackupFile(string editorContent)
			=> CreateBackupFile(FilePath, editorContent);

		public void DeleteBackupFile()
			=> DeleteBackupFile(FilePath);

		#endregion Public methods

		#region Private methods

		private void CreateBackupFile(string originalFilePath, string editorContent)
		{
			string backupFilePath = originalFilePath + ".backup";
			File.WriteAllText(backupFilePath, editorContent, Encoding.GetEncoding(1252));
		}

		private void DeleteBackupFile(string originalFilePath)
		{
			string backupFilePath = originalFilePath + ".backup";

			if (File.Exists(backupFilePath))
				File.Delete(backupFilePath);
		}

		#endregion Private methods

		#region Obsolete methods

		[Obsolete("Use RunAsync() instead!", true)]
		public new void RunWorkerAsync()
			=> base.RunWorkerAsync();

		[Obsolete("Use RunAsync() instead!", true)]
		public new void RunWorkerAsync(object argument)
			=> base.RunWorkerAsync(argument);

		#endregion Obsolete methods
	}
}
