namespace EditorConfig.Core
{
	public class IniProperty : IniLineData
	{
		public IniProperty(string key, string value)
			: base(IniLineType.Property)
		{
			Key = key;
			Value = value;
		}

		public string Key { get; }

		public string Value { get; }

		/// <inheritdoc />
		public override string ToString() => $"{Key}={Value}";
	}
}