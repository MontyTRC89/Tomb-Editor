namespace TombLib.Scripting.Objects
{
	public class FindReplaceItem
	{
		public int LineNumber { get; }
		public string LineText { get; }
		public string MatchSegmentText { get; }
		public int MatchSegmentIndex { get; }

		public FindReplaceItem(int lineNumber, string lineText, string matchSegmentText, int matchSegmentIndex)
		{
			LineNumber = lineNumber;
			LineText = lineText;
			MatchSegmentText = matchSegmentText;
			MatchSegmentIndex = matchSegmentIndex;
		}
	}
}
