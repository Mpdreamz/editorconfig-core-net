namespace EditorConfig.Core
{
	public class IniEmptyLine : IniLineData
	{
		/// <inheritdoc />
		public IniEmptyLine()
			: base(IniLineType.None)
		{
		}

		/// <inheritdoc />
		public override string ToString()
		{
			return string.Empty;
		}
	}
}