using System;
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
		private readonly Regex _comment = new Regex(@"^\s*[#;](.*)");
		private readonly Regex _property = new Regex(@"^\s*([\w\.\-_]+)\s*[=:]\s*(.*?)\s*([#;].*)?$");

		private readonly bool _isRoot;
		private readonly List<IniSection> _sections = new List<IniSection>();

		public EditorConfigFile(string file)
		{
			if (string.IsNullOrWhiteSpace(file))
			{
				throw new ArgumentException("The given path must be non-empty", nameof(file));
			}

			if (!File.Exists(file))
			{
				throw new ArgumentException("The given file {file} does not exist", nameof(file));
			}

			Directory = Path.GetDirectoryName(file);

			Global = Parse(file);

			if (Global != null && Global.TryGetProperty("root", out var rootProp))
			{
				_ = bool.TryParse(rootProp.Value, out _isRoot);
			}
		}

		public IniSection? Global { get; }

		public IReadOnlyList<IniSection> Sections => _sections;

		public string Directory { get; }

		public bool IsRoot => _isRoot;

		private IniSection? Parse(string file)
		{
			var lines = File.ReadLines(file);

			uint currentLineNumber = 0;

			IniSection? globalSection = null;
			IniSection? activeSection = null;
			foreach (var line in lines)
			{
				try
				{
					var matches = _comment.Matches(line);

					if (matches.Count > 0)
					{
						var text = matches[0].Groups[1].Value.Trim();
						IniComment iniComment = new IniComment(currentLineNumber, text);

						// We will discard any comments from before the first section
						activeSection?.AddLine(iniComment);

						continue;
					}

					matches = _property.Matches(line);
					if (matches.Count > 0)
					{
						var key = matches[0].Groups[1].Value.Trim();
						var value = matches[0].Groups[2].Value.Trim();

						var prop = new IniProperty(currentLineNumber, key, value);

						if (activeSection == null)
						{
							throw new InvalidOperationException($"Trying to add a property before the active section has been set. Line: {currentLineNumber}");
						}

						activeSection.AddLine(prop);
						continue;
					}
					matches = _section.Matches(line);
					if (matches.Count <= 0)
					{
						continue;
					}

					var sectionName = matches[0].Groups[1].Value;
					activeSection = new IniSection(currentLineNumber, sectionName);

					if (sectionName.Equals("*", StringComparison.Ordinal))
					{
						globalSection = activeSection;
					}

					_sections.Add(activeSection);
				}
				finally
				{
					currentLineNumber++;
				}
			}

			return globalSection;
		}
	}
}
