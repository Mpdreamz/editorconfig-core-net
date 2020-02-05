namespace EditorConfig.Core
{
	public abstract class IniLine
	{
		protected IniLine(uint lineNumber, IniLineType lineType)
        {
			LineType = lineType;
			LineNumber = lineNumber;
		}

		public IniLineType LineType { get; }

		public uint LineNumber { get; }
	}
}
