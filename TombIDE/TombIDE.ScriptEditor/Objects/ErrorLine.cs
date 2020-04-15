namespace TombIDE.ScriptEditor.Objects
{
	public class ErrorLine
	{
		public ErrorLine(int lineNumber, string message)
		{
			LineNumber = lineNumber;
			Message = message;
		}

		public int LineNumber { get; internal set; }
		public string Message { get; internal set; }
	}
}
