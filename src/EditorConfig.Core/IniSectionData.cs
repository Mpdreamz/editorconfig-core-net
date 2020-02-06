namespace EditorConfig.Core
{
	using System;
	using System.Collections.Generic;
	using System.Diagnostics.CodeAnalysis;
	using System.Linq;

	/// <summary>
	///     Represents an ini section within the editorconfig file
	/// </summary>
	public class IniSectionData : IniLineData
	{
		private readonly List<IniLineData> _lines = new List<IniLineData>();

		private readonly Dictionary<string, IniProperty> _propertyDictionary = new Dictionary<string, IniProperty>();

		public IniSectionData(string name)
			: base(IniLineType.SectionHeader)
		{
			Name = name;
			AddLine(this);
		}

		private IniSectionData()
			: base(IniLineType.SectionHeader)
		{
			Name = "Global";
			IsGlobal = true;
		}

		public IEnumerable<IniComment> Comments => GetLinesOfType<IniComment>();

		public bool IsGlobal { get; }

		public string Name { get; }

		public IEnumerable<IniProperty> Properties => GetLinesOfType<IniProperty>();

		public static IniSectionData Global() => new IniSectionData();

		public void AddLine(IniLineData iniLine)
		{
			if (iniLine is null)
			{
				throw new ArgumentNullException(nameof(iniLine));
			}

			_lines.Add(iniLine);

			if (iniLine.LineType != IniLineType.Property)
			{
				return;
			}

			var prop = (IniProperty)iniLine;
			_propertyDictionary.Add(prop.Key, prop);
		}

		public EditContext Edit() => new EditContext(this);

		/// <inheritdoc />
		public override string ToString() => $"[{Name}]";

		public bool TryGetComment(string commentText, [NotNullWhen(true)] out IniComment? comment, out int offset)
		{
			if (string.IsNullOrWhiteSpace(commentText))
			{
				throw new ArgumentException("message", nameof(commentText));
			}

			comment = Comments.FirstOrDefault(c => c.Text.Equals(commentText, StringComparison.OrdinalIgnoreCase));
			offset = _lines.IndexOf(comment);
			return comment != null;
		}

		public bool TryGetProperty(string key, [NotNullWhen(true)] out IniProperty? prop, out int offset)
		{
			var found = _propertyDictionary.TryGetValue(key, out prop);
			offset = _lines.IndexOf(prop);
			return found;
		}

		private IEnumerable<TLine> GetLinesOfType<TLine>()
			where TLine : IniLineData =>
			_lines.OfType<TLine>();

		public class EditContext : IDisposable
		{
			public EditContext(IniSectionData section)
			{
				Section = section ?? throw new ArgumentNullException(nameof(section));

				Lines.AddRange(section._lines);
			}

			public IniSectionData Section { get; }

			public List<IniLineData> Lines { get; } = new List<IniLineData>();

			public void Add(IniLineData line)
			{
				if (line is null)
				{
					throw new ArgumentNullException(nameof(line));
				}

				Lines.Add(line);
			}

			/// <inheritdoc />
			public void Dispose()
			{
				Dispose(true);
				GC.SuppressFinalize(this);
			}

			protected virtual void Dispose(bool disposing)
			{
				if (disposing)
				{
				}
			}
		}
	}
}