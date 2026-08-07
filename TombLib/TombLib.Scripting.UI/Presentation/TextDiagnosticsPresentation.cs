namespace TombLib.Scripting.UI.Presentation;

public sealed record class TextDiagnosticsPresentation(
	string NoActiveDocumentText,
	string PendingText,
	string EmptyText,
	string ErrorsLabel,
	string WarningsLabel,
	string MessagesLabel,
	string SeverityHeader,
	string LineHeader,
	string ColumnHeader,
	string MessageHeader);
