namespace EditorConfig.Core
{
	using System;
	using System.ComponentModel;
	using System.Text.RegularExpressions;

	public abstract class IniLineData
	{
		protected IniLineData(IniLineType lineType, string? existingText)
		{
			if (!Enum.IsDefined(typeof(IniLineType), lineType))
			{
				throw new InvalidEnumArgumentException(nameof(lineType), (int)lineType, typeof(IniLineType));
			}

			LineType = lineType;
			ExistingText = existingText;
		}

		public IniLineType LineType { get; }

		public string? ExistingText { get; }

		public abstract Regex LineRegex { get; }

		protected abstract string ToLine();

		/// <inheritdoc />
		public sealed override string ToString() => ExistingText ?? ToLine();
	}
}