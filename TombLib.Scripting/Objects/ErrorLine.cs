namespace TombLib.Scripting.Objects
{
	public class ErrorLine
	{
		public string Message { get; }
		public int LineNumber { get; }
		public string ErrorSegmentText { get; }

		public ErrorLine(string message, int lineNumber, string errorSegmentText)
		{
			Message = message;
			LineNumber = lineNumber;
			ErrorSegmentText = errorSegmentText;
		}
	}
}
