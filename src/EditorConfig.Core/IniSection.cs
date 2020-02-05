using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace EditorConfig.Core
{
	/// <summary>
	/// Represents an ini section within the editorconfig file
	/// </summary>
	public class IniSection : IniLine
	{
		private readonly Dictionary<int, IniLine> _lineDictionary = new Dictionary<int, IniLine>();
		private readonly Dictionary<string, IniProperty> _propertyDictionary = new Dictionary<string, IniProperty>();

		public IniSection(int lineNumber, string name) : base(lineNumber, IniLineType.SectionHeader)
		{
			Name = name;
			AddLine(this);
		}

		public string Name { get; }

		public IDictionary<int, IniProperty> Properties => GetLinesOfType<IniProperty>(IniLineType.Property);

		public IDictionary<int, IniComment> Comments => GetLinesOfType<IniComment>(IniLineType.Comment);

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

		public bool TryGetProperty(string key, [NotNullWhen(returnValue: true)] out IniProperty? prop)
		{
			return _propertyDictionary.TryGetValue(key, out prop);
		}

		public bool TryFindComment(string commentText, [NotNullWhen(returnValue: true)] out IniComment? comment)
		{
			if (string.IsNullOrWhiteSpace(commentText))
			{
				throw new System.ArgumentException("message", nameof(commentText));
			}

			comment = Comments.Values.FirstOrDefault(c => c.Text.Equals(commentText, System.StringComparison.OrdinalIgnoreCase));
			return comment != null;
		}

		private IDictionary<int, TLine> GetLinesOfType<TLine>(IniLineType lineType)
			where TLine : IniLine =>
			_lineDictionary.Where(kvp => kvp.Value.LineType == lineType).ToDictionary(kvp => kvp.Key, kvp => (TLine)kvp.Value);
	}
}
