namespace EditorConfig.Core
{
	using System;
	using System.Collections;
	using System.Collections.Generic;
	using System.Diagnostics.CodeAnalysis;
	using System.Linq;

	/// <summary>
	///     Represents an ini section within the editorconfig file
	/// </summary>
	public class IniSectionData : IniLineData, IEnumerable<IniLineData>
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

		public int Length => _lines.Count;

		public IEnumerable<IniProperty> Properties => GetLinesOfType<IniProperty>();

		public static IniSectionData Global() => new IniSectionData();

		// AddLine is nicer syntax, this is here so collection initializers can be used
		public void Add(IniLineData iniLine) => AddLine(iniLine);

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

			if (IsGlobal && prop.Key != "root")
			{
				throw new InvalidOperationException("Only the root property can be added to the global section");
			}

			_propertyDictionary.Add(prop.Key, prop);
		}

		public EditContext Edit() => new EditContext(this);

		public IEnumerator<IniLineData> GetEnumerator() => _lines.GetEnumerator();

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

		IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

		private IEnumerable<TLine> GetLinesOfType<TLine>()
			where TLine : IniLineData =>
			_lines.OfType<TLine>();

		public class EditContext
		{
			private readonly List<IniLineData> _list = new List<IniLineData>();

			public EditContext(IniSectionData section)
			{
				Section = section ?? throw new ArgumentNullException(nameof(section));

				AddRange(section._lines);
			}

			public IniSectionData Section { get; }

			public IReadOnlyList<IniLineData> Lines => _list;

			public int Count => _list.Count;

			public void Add(IniLineData item) => _list.Add(item);

			public void AddRange(IEnumerable<IniLineData> items) => _list.AddRange(items);

			public void Clear() => _list.Clear();

			public bool Contains(IniLineData item) => _list.Contains(item);

			public void CopyTo(IniLineData[] array, int arrayIndex) => _list.CopyTo(array, arrayIndex);

			public int IndexOf(IniLineData item) => _list.IndexOf(item);

			public void Insert(int index, IniLineData item) => _list.Insert(index, item);

			public bool Remove(IniLine line)
			{
				if (line is null)
				{
					throw new ArgumentNullException(nameof(line));
				}

				return _list.Remove(line.GetLineData());
			}

			public void RemoveRange(IEnumerable<IniLine> lines)
			{
				if (lines is null)
				{
					throw new ArgumentNullException(nameof(lines));
				}

				foreach (var line in lines)
				{
					Remove(line);
				}
			}

			public void RemoveAt(int index) => _list.RemoveAt(index);
		}
	}
}