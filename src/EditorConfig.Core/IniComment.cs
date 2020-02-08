using System.Text.RegularExpressions;

namespace EditorConfig.Core
{
	public class IniCommentData : IniLineData
	{
		public IniCommentData(string text) : this(text, false)
		{
		}

		private IniCommentData(string textOrLine, bool isLine) : base(IniLineType.Comment, isLine ? textOrLine : null)
		{
			if (string.IsNullOrWhiteSpace(textOrLine))
			{
				throw new System.ArgumentException("message", nameof(textOrLine));
			}

			if (isLine)
			{
				var matches = LineRegex.Matches(textOrLine);
				Text = matches[0].Groups[1].Value.Trim();
			}
			else
			{
				Text = textOrLine;
			}
		}

		public static IniCommentData FromLine(string line) => new IniCommentData(line, true);

		public string Text { get; }

		public override Regex LineRegex => EditorConfigFile.CommentRegex;

		/// <inheritdoc />
		protected override string ToLine() => $"# {Text}";
	}
}
