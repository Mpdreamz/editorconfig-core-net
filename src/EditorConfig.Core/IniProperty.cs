using System.Text.RegularExpressions;

namespace EditorConfig.Core
{
	public class IniPropertyData : IniLineData
	{
		public IniPropertyData(string key, string value)
			: base(IniLineType.Property, null)
		{
			Key = key;
			Value = value;
		}

		public IniPropertyData(string existingText) : base(IniLineType.Property, existingText)
		{
			var matches = LineRegex.Matches(existingText);

			Key = matches[0].Groups[1].Value.Trim();
			Value = matches[0].Groups[2].Value.Trim();
		}

		public string Key { get; }

		public string Value { get; }

		public override Regex LineRegex => EditorConfigFile.PropertyRegex;

		/// <inheritdoc />
		protected override string ToLine() => $"{Key} = {Value}";
	}
}