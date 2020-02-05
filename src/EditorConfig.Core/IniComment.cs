namespace EditorConfig.Core
{
	public class IniComment : IniLine
	{
		public IniComment(int lineNumber, string text) : base(lineNumber, IniLineType.Comment)
        {
			Text = text;
		}

		public string Text { get; }
	}
}
