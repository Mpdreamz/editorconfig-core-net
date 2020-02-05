namespace EditorConfig.Core
{
	public class IniComment : IniLine
	{
		public IniComment(uint lineNumber, string text) : base(lineNumber, IniLineType.Comment)
        {
			Text = text;
		}

		public string Text { get; }
	}
}
