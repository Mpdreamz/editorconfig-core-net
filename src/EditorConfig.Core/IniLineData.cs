namespace EditorConfig.Core
{
	using System;
	using System.ComponentModel;

	public abstract class IniLineData
	{
		protected IniLineData(IniLineType lineType)
		{
			if (!Enum.IsDefined(typeof(IniLineType), lineType))
			{
				throw new InvalidEnumArgumentException(nameof(lineType), (int)lineType, typeof(IniLineType));
			}

			LineType = lineType;
		}

		public IniLineType LineType { get; }

		/// <inheritdoc />
		public abstract override string ToString();
	}
}