namespace EditorConfig.Core
{
	using System;
	using System.Collections.Generic;

	public class SectionEditContext : IniSectionData
	{
		public SectionEditContext(IniSectionData section) : base(section)
		{
			Section = section ?? throw new ArgumentNullException(nameof(section));
		}

		public IniSectionData Section { get; }

		public int Count => Lines.Count;

		public void AddRange(IEnumerable<IniLineData> items) => Lines.AddRange(items);

		public void Clear() => Lines.Clear();

		public bool Contains(IniLineData item) => Lines.Contains(item);

		public void CopyTo(IniLineData[] array, int arrayIndex) => Lines.CopyTo(array, arrayIndex);

		public int IndexOf(IniLineData item) => Lines.IndexOf(item);

		public void Insert(int index, IniLineData item) => Lines.Insert(index, item);

		public bool Remove(IniLine line)
		{
			if (line is null)
			{
				throw new ArgumentNullException(nameof(line));
			}

			return Lines.Remove(line.GetLineData());
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

		public void RemoveAt(int index) => Lines.RemoveAt(index);
	}
}