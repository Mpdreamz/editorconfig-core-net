namespace EditorConfig.Core
{
	using System;

	public class IniLine<T> : IniLine
		where T : IniLineData
	{
		public IniLine(int lineNumber, T data)
			: base(lineNumber, data)
		{
			if (lineNumber <= 0)
			{
				throw new ArgumentOutOfRangeException(nameof(lineNumber));
			}

			Data = data;
		}

		public T Data { get; }

		/// <inheritdoc />
		public override IniLineData GetLineData() => Data;
	}

	public abstract class IniLine
	{
		protected IniLine(int lineNumber, IniLineData data)
		{
			if (lineNumber <= 0)
			{
				throw new ArgumentOutOfRangeException(nameof(lineNumber));
			}

			if (data == null)
			{
				throw new ArgumentNullException(nameof(data));
			}

			LineNumber = lineNumber;
			LineType = data.LineType;
		}

		public int LineNumber { get; }

		public IniLineType LineType { get; }

		public abstract IniLineData GetLineData();
	}
}