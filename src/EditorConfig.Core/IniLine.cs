namespace EditorConfig.Core
{
	public abstract class IniLine
	{
		protected IniLine(int lineNumber, IniLineType lineType)
        {
			LineType = lineType;
			LineNumber = lineNumber;
		}

		public IniLineType LineType { get; }

		public int LineNumber { get; }
	}
}
