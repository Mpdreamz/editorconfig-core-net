namespace EditorConfig.Core
{
	public class IniProperty : IniLine
	{
		public IniProperty(uint lineNumber, string key, string value) : base(lineNumber, IniLineType.Property)
        {
			Key = key;
			Value = value;
		}

		public string Key { get; }

		public string Value { get; }
	}
}
