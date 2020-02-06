namespace EditorConfig.Core
{
    using System;

    public class IniLine<T> : IniLine
        where T : IniLineData
    {
        public IniLine(int lineNumber, T line)
            : base(lineNumber, line)
        {
            if (lineNumber <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(lineNumber));
            }

            Line = line;
        }

        public T Line { get; }

        /// <inheritdoc />
        public override IniLineData GetLineData() => Line;
    }

    public abstract class IniLine
    {
        protected IniLine(int lineNumber, IniLineData line)
        {
	        if (lineNumber <= 0)
	        {
		        throw new ArgumentOutOfRangeException(nameof(lineNumber));
			}

			if (line == null)
			{
				throw new ArgumentNullException(nameof(line));
			}

			LineNumber = lineNumber;
            LineType = line.LineType;
        }

        public int LineNumber { get; }

        public IniLineType LineType { get; }

        public abstract IniLineData GetLineData();
    }
}