using System;
using System.ComponentModel;
using System.Windows.Forms;
using TombIDE.ScriptingStudio.UI;
using TombIDE.Shared;
using TombLib.Scripting.Bases;
using TombLib.Scripting.ClassicScript;
using TombLib.Scripting.ClassicScript.Parsers;
using TombLib.Scripting.Interfaces;

namespace TombIDE.ScriptingStudio.ToolStrips
{
	public partial class StudioStatusStrip : UserControl
	{
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
				UpdateItemVisibility();
			}
		}

		private IEditorControl _editorControl;
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

		private void UpdateStatus()
		{
			UpdateItemVisibility();

			if (_editorControl != null)
			{
				if (_editorControl is TextEditorBase)
					label_RowNumber.Text = string.Format(Strings.Default.Line, _editorControl.CurrentRow);
				else if (_editorControl is StringEditor)
					label_RowNumber.Text = string.Format(Strings.Default.Row, _editorControl.CurrentRow);

				label_ColNumber.Text = string.Format(Strings.Default.Column, _editorControl.CurrentColumn);
				label_Selected.Text = string.Format(Strings.Default.Selected, _editorControl.SelectionLength);

				label_Zoom.Text = string.Format(Strings.Default.Zoom, _editorControl.Zoom);
				button_ResetZoom.Enabled = _editorControl.Zoom != 100;

				toolTip.SetToolTip(button_ResetZoom, button_ResetZoom.Enabled ? Strings.Default.ResetZoom : string.Empty);
			}

			UpdateSyntaxPreview();
		}

		private void UpdateItemVisibility()
		{
			label_RowNumber.Visible = _editorControl != null;
			label_ColNumber.Visible = _editorControl != null;
			label_Selected.Visible = _editorControl != null;

			label_Zoom.Visible = _editorControl != null;
			button_ResetZoom.Visible = _editorControl != null;

			panel_Syntax.Visible = _editorControl != null && _documentMode == DocumentMode.ClassicScript;
		}

		private void UpdateSyntaxPreview()
		{
			if (_editorControl is ClassicScriptEditor csEditor)
			{
				SyntaxPreview.CurrentArgumentIndex = ArgumentParser.GetArgumentIndexAtOffset(csEditor.Document, csEditor.CaretOffset);
				SyntaxPreview.Text = CommandParser.GetCommandSyntax(csEditor.Document, csEditor.CaretOffset);
			}
		}

		#endregion Methods
	}
}
