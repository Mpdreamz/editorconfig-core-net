namespace EditorConfig.Core
{
	public class IniPropertyData : IniLineData
	{
		public IniPropertyData(string key, string value)
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