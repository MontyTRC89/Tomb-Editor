using System;
using TombLib.Scripting.Bases;
using TombLib.Scripting.Enums;

namespace TombLib.Scripting.Interfaces
{
	public interface IEditorControl : IDisposable
	{
		#region Properties

		EditorType EditorType { get; }

		string FilePath { get; set; }

		/// <summary>
		/// Silent session prevents the control from checking if the content has changed, therefore not running any BackgroundWorkers to do so.
		/// <para>Setting this to <c>true</c> will also prevent the creation of backup files.</para>
		/// </summary>
		bool IsSilentSession { get; set; }

		bool CreateBackupFiles { get; set; }

		/// <summary>
		/// A string representation of the editor's content.
		/// <para><b>Note:</b> Every <c>IEditorControl</c> should have some way of representing its contents using a string!</para>
		/// </summary>
		string Content { get; set; }
		bool IsContentChanged { get; set; }

		DateTime LastModified { get; set; }

		bool CanUndo { get; }
		bool CanRedo { get; }

		/* Status data */

		int CurrentRow { get; }
		int CurrentColumn { get; }

		object SelectedContent { get; }
		int SelectionLength { get; }

		int Zoom { get; set; }

		int MinZoom { get; set; }
		int MaxZoom { get; set; }
		int ZoomStepSize { get; set; }

		#endregion Properties

		#region Methods

		void Load(string fileName, bool silentSession);

		void Save();
		void Save(string fileName);

		void Undo();
		void Redo();

		void Cut();
		void Copy();
		void Paste();

		void SelectAll();

		void GoToObject(string objectName, object identifyingObject = null);

		void UpdateSettings(ConfigurationBase configuration);

		void TryRunContentChangedWorker();

		#endregion Methods

		#region Events

		event EventHandler ContentChangedWorkerRunCompleted;
		event EventHandler StatusChanged;
		event EventHandler ZoomChanged;

		#endregion Events
	}
}
