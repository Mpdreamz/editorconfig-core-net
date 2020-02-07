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
		private readonly Dictionary<string, IniPropertyData> _propertyDictionary = new Dictionary<string, IniPropertyData>();

		public IniSectionData(string name)
			: base(IniLineType.SectionHeader)
		{
			if (string.IsNullOrWhiteSpace(name))
			{
				throw new ArgumentException("message", nameof(name));
			}

			Name = name;
			AddLine(this);
		}

		protected IniSectionData(IniSectionData section)
			: base(IniLineType.SectionHeader)
		{
			if (section is null)
			{
				throw new ArgumentNullException(nameof(section));
			}

			Name = section.Name;
			IsGlobal = section.IsGlobal;

			Lines.AddRange(section.Lines);
		}

		private IniSectionData()
			: base(IniLineType.SectionHeader)
		{
			Name = "Global";
			IsGlobal = true;
		}

		public IEnumerable<(IniComment Prop, int Offset)> Comments => GetLinesOfType<IniComment>();

		public bool IsGlobal { get; }

		public string Name { get; }

		public int Length => Lines.Count;

		public IEnumerable<(IniPropertyData Prop, int Offset)> Properties => GetLinesOfType<IniPropertyData>();

		protected List<IniLineData> Lines { get; } = new List<IniLineData>();

		public static IniSectionData Global() => new IniSectionData();

		// AddLine is nicer syntax, this is here so collection initializers can be used
		public void Add(IniLineData iniLine) => AddLine(iniLine);

		public void AddLine(IniLineData iniLine)
		{
			if (iniLine is null)
			{
				throw new ArgumentNullException(nameof(iniLine));
			}

			Lines.Add(iniLine);

			if (iniLine.LineType != IniLineType.Property)
			{
				return;
			}

			var prop = (IniPropertyData)iniLine;

			if (IsGlobal && prop.Key != "root")
			{
				throw new InvalidOperationException("Only the root property can be added to the global section");
			}

			_propertyDictionary.Add(prop.Key, prop);
		}

		public SectionEditContext Edit() => new SectionEditContext(this);

		public IEnumerator<IniLineData> GetEnumerator() => Lines.GetEnumerator();

		/// <inheritdoc />
		public override string ToString() => $"[{Name}]";

		internal bool TryGetComment(
			string commentText,
			[NotNullWhen(true)] out IniComment? comment,
			out int offset)
		{
			if (string.IsNullOrWhiteSpace(commentText))
			{
				throw new ArgumentException("message", nameof(commentText));
			}

			(comment, offset) = Comments.FirstOrDefault(c => c.Prop.Text.Equals(commentText, StringComparison.OrdinalIgnoreCase));
			return comment != null;
		}

		internal bool TryGetProperty(
			string key,
			[NotNullWhen(true)] out IniPropertyData? prop,
			out int offset)
		{
			var found = _propertyDictionary.TryGetValue(key, out prop);
			offset = Lines.IndexOf(prop);
			return found;
		}

		IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

		private IEnumerable<(TLine Line, int Offset)> GetLinesOfType<TLine>()
			where TLine : IniLineData =>
			Lines.OfType<TLine>().Select(l => (l, Lines.IndexOf(l)));
	}
}