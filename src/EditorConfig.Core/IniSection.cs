using System.Collections.Generic;
using System.Linq;

namespace EditorConfig.Core
{
	/// <summary>
	/// Represents an ini section within the editorconfig file
	/// </summary>
	public class IniSection : IniLine
	{
		private readonly Dictionary<uint, IniLine> _lineDictionary = new Dictionary<uint, IniLine>();
		private readonly Dictionary<string, IniProperty> _propertyDictionary = new Dictionary<string, IniProperty>();

		public IniSection(uint lineNumber, string name) : base(lineNumber, IniLineType.SectionHeader)
		{
			Name = name;
			AddLine(this);
		}

		public string Name { get; }

		public IDictionary<uint, IniProperty> Properties => GetLinesOfType<IniProperty>(IniLineType.Property);

		public IDictionary<uint, IniComment> Comments => GetLinesOfType<IniComment>(IniLineType.Comment);

		public void AddLine(IniLine iniLine)
		{
			if (iniLine is null)
			{
				throw new System.ArgumentNullException(nameof(iniLine));
			}

			_lineDictionary.Add(iniLine.LineNumber, iniLine);

			if (iniLine.LineType == IniLineType.Property)
			{
				var prop = (IniProperty)iniLine;
				_propertyDictionary.Add(prop.Key, prop);
			}
		}

		public bool TryGetProperty(string key, out IniProperty prop)
		{
			return _propertyDictionary.TryGetValue(key, out prop);
		}

		private IDictionary<uint, TLine> GetLinesOfType<TLine>(IniLineType lineType)
			where TLine : IniLine => _lineDictionary.Where(kvp => kvp.Value.LineType == lineType).ToDictionary(kvp => kvp.Key, kvp => (TLine)kvp.Value);
	}

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

	public class IniComment : IniLine
	{
		public IniComment(uint lineNumber, string text) : base(lineNumber, IniLineType.Comment)
        {
			Text = text;
		}

		public string Text { get; }
	}

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

	public enum IniLineType
	{
		None,
		SectionHeader,
		Property,
		Comment
	}
}
