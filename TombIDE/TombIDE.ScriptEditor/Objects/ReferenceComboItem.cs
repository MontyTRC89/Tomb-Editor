namespace TombIDE.ScriptEditor.Objects
{
	internal class ReferenceComboItem
	{
		public string Text { get; }
		public ReferenceComboType ReferenceType { get; }

		public ReferenceComboItem(string text, ReferenceComboType type)
		{
			Text = text;
			ReferenceType = type;
		}

		public override string ToString() => Text;
	}
}
