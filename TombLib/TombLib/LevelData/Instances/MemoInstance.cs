namespace TombLib.LevelData
{
    public class MemoInstance : PositionBasedObjectInstance
    {
        private const int _trimWords = 25;

        public string Text { get; set; } = "New memo";
        public bool AlwaysDisplay { get; set; } = false;

        public override string ToString()
        {
            var result = "Memo - ";
            if (string.IsNullOrEmpty(Text))
                result += "empty";
            else
            {
                var firstNewLine = Text.IndexOf(System.Environment.NewLine);
                var oneLiner = firstNewLine >= 0 ? Text.Substring(0, firstNewLine) : Text;
                if (string.IsNullOrEmpty(oneLiner)) oneLiner = "...";

                if (oneLiner.Length < _trimWords || oneLiner.IndexOf(" ", _trimWords) == -1)
                    result += "'" + oneLiner + "'";
                else
                    result += "'" + oneLiner.Substring(0, oneLiner.IndexOf(" ", _trimWords)) + "...'";
            }
            return result;
        }

        public string ShortName() => ToString();
    }
}
