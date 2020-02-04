using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;

namespace EditorConfig.Core
{
	/// <summary>
	/// Represents the raw config file as INI
	/// </summary>
	public class EditorConfigFile
	{
		private readonly Regex _section = new Regex(@"^\s*\[(([^#;]|\\#|\\;)+)\]\s*([#;].*)?$");
		private readonly Regex _comment = new Regex(@"^\s*[#;]");
		private readonly Regex _property = new Regex(@"^\s*([\w\.\-_]+)\s*[=:]\s*(.*?)\s*([#;].*)?$");

		private readonly bool _isRoot;

		public EditorConfigFile(string file)
		{
			Directory = Path.GetDirectoryName(file);
			Parse(file);

			if (Global.ContainsKey("root"))
			{
				bool.TryParse(Global["root"], out _isRoot);
			}
		}

		public IniSection Global { get; } = new IniSection();

		public List<IniSection> Sections { get; } = new List<IniSection>();

		public string Directory { get; }

		public bool IsRoot => _isRoot;

		public void Parse(string file)
		{
			var lines = File.ReadLines(file);

			var activeSection = Global;
			foreach (var line in lines)
			{
				if (_comment.IsMatch(line))
				{
					continue;
				}

				var matches = _property.Matches(line);
				if (matches.Count > 0)
				{
					var key = matches[0].Groups[1].Value.Trim();
					var value = matches[0].Groups[2].Value.Trim();
					activeSection.Add(key, value);
					continue;
				}
				matches = _section.Matches(line);
				if (matches.Count <= 0)
				{
					continue;
				}

				var sectionName = matches[0].Groups[1].Value;
				activeSection = new IniSection { Name = sectionName };
				Sections.Add(activeSection);
			}
		}
	}
}
