using System.Text.RegularExpressions;

namespace EditorConfig.Core
{
	public class IniEmptyLine : IniLineData
	{
		/// <inheritdoc />
		public IniEmptyLine()
			: base(IniLineType.None, null)
		{
		}

		public override Regex LineRegex { get; } = new Regex(string.Empty);

		/// <inheritdoc />
		protected override string ToLine() => string.Empty;
	}
}