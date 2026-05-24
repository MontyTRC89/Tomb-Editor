using System;
using System.ComponentModel;
using System.Linq;
using TombIDE.ScriptingStudio.Shell;
using System.Windows.Forms;
using TombIDE.ScriptingStudio.UI;
using TombLib.Scripting.Signatures;
using TombLib.Scripting.UI.Editors;
using TombLib.Scripting.UI.Presentation;

namespace TombIDE.ScriptingStudio.ToolStrips
{
	public partial class StudioStatusStrip : UserControl
	{
		private readonly TextEditorStatusPresentationService _statusPresentationService = new();
		private readonly TextEditorStatusText _statusText = new(
			Shared.Strings.Default.Row,
			Shared.Strings.Default.Line,
			Shared.Strings.Default.Column,
			Shared.Strings.Default.Selected,
			Shared.Strings.Default.Zoom,
			Shared.Strings.Default.ResetZoom);

		#region Properties

		private DocumentMode _documentMode;
		[Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public DocumentMode DocumentMode
		{
			get => _documentMode;
			set
			{
				_documentMode = value;
				UpdateStatus();
			}
		}

		private IEditorControl _editorControl;
		private StudioStatusStripSegment[] _segmentContributions = Array.Empty<StudioStatusStripSegment>();
		[Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public IEditorControl EditorControl
		{
			get => _editorControl;
			set
			{
				if (_editorControl != null)
					_editorControl.StatusChanged -= EditorControl_StatusChanged;

				_editorControl = value;

				if (_editorControl != null)
					_editorControl.StatusChanged += EditorControl_StatusChanged;

				UpdateStatus();
			}
		}

		[Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public StudioStatusStripSegment[] SegmentContributions
		{
			get => _segmentContributions;
			set
			{
				_segmentContributions = value ?? Array.Empty<StudioStatusStripSegment>();
				UpdateStatus();
			}
		}

		#endregion Properties

		#region Construction

		public StudioStatusStrip()
			=> InitializeComponent();

		#endregion Construction

		#region Events

		private void button_ResetZoom_Click(object sender, EventArgs e)
		{
			_editorControl.Zoom = 100;
			UpdateStatus();
		}

		private void EditorControl_StatusChanged(object sender, EventArgs e)
			=> UpdateStatus();

		#endregion Events

		#region Methods

		public void ReloadContributionSettings()
		{
			if (HasSegment(StudioStatusStripSegment.SyntaxPreview))
				SyntaxPreview.ReloadSettings();
		}

		private void UpdateStatus()
		{
			bool showSyntaxPreviewWhenEmpty = _editorControl is null && HasSegment(StudioStatusStripSegment.SyntaxPreview);
			ApplyStatusPresentation(_statusPresentationService.Create(_editorControl, showSyntaxPreviewWhenEmpty, _statusText));
		}

		private void ApplyStatusPresentation(TextEditorStatusPresentation presentation)
		{
			label_RowNumber.Visible = presentation.HasEditor && HasSegment(StudioStatusStripSegment.CaretPosition);
			label_ColNumber.Visible = presentation.HasEditor && HasSegment(StudioStatusStripSegment.CaretPosition);
			label_Selected.Visible = presentation.HasEditor && HasSegment(StudioStatusStripSegment.SelectionLength);

			label_Zoom.Visible = presentation.HasEditor && HasSegment(StudioStatusStripSegment.Zoom);
			button_ResetZoom.Visible = presentation.HasEditor && HasSegment(StudioStatusStripSegment.Zoom);

			label_RowNumber.Text = presentation.RowLabelText;
			label_ColNumber.Text = presentation.ColumnLabelText;
			label_Selected.Text = presentation.SelectionLabelText;

			label_Zoom.Text = presentation.ZoomLabelText;
			button_ResetZoom.Enabled = presentation.CanResetZoom;
			toolTip.SetToolTip(button_ResetZoom, presentation.ResetZoomToolTipText);

			panel_Syntax.Visible = HasSegment(StudioStatusStripSegment.SyntaxPreview)
				&& presentation.HasEditor
				&& presentation.ShowSyntaxPreview;
			ApplySyntaxPreview(presentation.SyntaxPreview);
		}

		private bool HasSegment(StudioStatusStripSegment segment)
			=> _segmentContributions.Contains(segment);

		private void ApplySyntaxPreview(TextSignatureHelpInfo syntaxPreview)
		{
			SyntaxPreview.CurrentArgumentIndex = syntaxPreview?.ActiveParameterIndex ?? -1;
			SyntaxPreview.Text = syntaxPreview?.Label ?? string.Empty;
		}

		#endregion Methods
	}
}
