namespace EditorConfig.Core
{
	using System;
	using System.Collections.Generic;
	using System.Diagnostics.CodeAnalysis;
	using System.IO;
	using System.Linq;
	using System.Text;
	using System.Text.RegularExpressions;

	/// <summary>
	///     Represents the raw config file as INI
	/// </summary>
	public class EditorConfigFile
	{
		private readonly Regex _section = new Regex(@"^\s*\[(([^#;]|\\#|\\;)+)\]\s*([#;].*)?$");

		private readonly Regex _comment = new Regex(@"^\s*[#;](.*)");

		private readonly Regex _property = new Regex(@"^\s*([\w\.\-_]+)\s*[=:]\s*(.*?)\s*([#;].*)?$");

		private readonly Dictionary<int, IniSectionData> _sections = new Dictionary<int, IniSectionData>();

		private readonly List<string> _lines = new List<string>();

		private readonly Encoding _encoding;

		public EditorConfigFile(string file, Encoding? encoding = null)
		{
			if (string.IsNullOrWhiteSpace(file))
			{
				throw new ArgumentException("The given path must be non-empty", nameof(file));
			}

			if (!File.Exists(file))
			{
				throw new ArgumentException($"The given file {file} does not exist", nameof(file));
			}

			FullPath = file;

			_encoding = encoding ?? Encoding.Default;

			Directory = Path.GetDirectoryName(file);

			_lines.AddRange(File.ReadAllLines(FullPath, _encoding));

			Global = IniSectionData.Global();

			Parse();
		}

		public string Directory { get; }

		public string FullPath { get; }

		public IniSectionData Global { get; private set; }

		public bool IsRoot { get; private set; }

		public IReadOnlyList<IniSectionData> Sections => _sections.Values.ToList();

		public EditContext Edit()
		{
			return new EditContext(this);
		}

		public bool TryGetComment(string commentText, IniSectionData section, [NotNullWhen(true)] out IniLine<IniComment>? comment)
		{
			if (string.IsNullOrWhiteSpace(commentText))
			{
				throw new ArgumentException("message", nameof(commentText));
			}

			if (section == null)
			{
				throw new ArgumentNullException(nameof(section));
			}

			if (!section.TryGetComment(commentText, out var commentData, out var offset))
			{
				comment = null;
				return false;
			}

			var baseLineNumber = section.IsGlobal ? 0 : _sections.First(kvp => ReferenceEquals(kvp.Value, section)).Key;

			var lineNumber = baseLineNumber + offset + 1;

			comment = new IniLine<IniComment>(lineNumber, commentData);
			return true;
		}

		public bool TryGetProperty(string key, IniSectionData section, [NotNullWhen(true)] out IniLine<IniProperty>? property)
		{
			if (string.IsNullOrWhiteSpace(key))
			{
				throw new ArgumentException("message", nameof(key));
			}

			if (section == null)
			{
				throw new ArgumentNullException(nameof(section));
			}

			if (!section.TryGetProperty(key, out var propertyData, out var offset))
			{
				property = null;
				return false;
			}

			var baseLineNumber = section.IsGlobal ? 0 : _sections.First(kvp => ReferenceEquals(kvp.Value, section)).Key;

			var lineNumber = baseLineNumber + offset + 1;

			property = new IniLine<IniProperty>(lineNumber, propertyData);
			return true;
		}

		private void Parse()
		{
			int currentLineNumber = 0;

			IniSectionData activeSection = Global;
			foreach (var line in _lines)
			{
				try
				{
					var matches = _comment.Matches(line);

					if (matches.Count > 0)
					{
						var text = matches[0].Groups[1].Value.Trim();
						IniComment iniComment = new IniComment(text);

						// We will discard any comments from before the first section
						activeSection.AddLine(iniComment);

						continue;
					}

					matches = _property.Matches(line);
					if (matches.Count > 0)
					{
						var key = matches[0].Groups[1].Value.Trim();
						var value = matches[0].Groups[2].Value.Trim();

						var prop = new IniProperty(key, value);

						activeSection.AddLine(prop);
						continue;
					}

					matches = _section.Matches(line);
					if (matches.Count > 0)
					{
						var sectionName = matches[0].Groups[1].Value;
						activeSection = new IniSectionData(sectionName);

						_sections.Add(currentLineNumber, activeSection);
						continue;
					}

					activeSection.AddLine(new IniEmptyLine());
				}
				finally
				{
					currentLineNumber++;
				}
			}

			if (!TryGetProperty("root", Global, out var rootProp))
			{
				return;
			}

			if (bool.TryParse(rootProp.Line.Value, out var isRoot))
			{
				IsRoot = isRoot;
			}
		}

		private void ResetTo(IEnumerable<string> lines)
		{
			_lines.Clear();
			_sections.Clear();

			_lines.AddRange(lines);
			Global = IniSectionData.Global();

			Parse();
		}

		public class EditContext : IDisposable
		{
			private readonly EditorConfigFile _editorConfigFile;

			private readonly FileStream _lock;

			private readonly List<string> _lines;

			public EditContext(EditorConfigFile editorConfigFile)
			{
				_editorConfigFile = editorConfigFile ?? throw new ArgumentNullException(nameof(editorConfigFile));

				_lock = File.OpenWrite(editorConfigFile.FullPath);

				// Make a copy of the data for editing
				_lines = _editorConfigFile._lines.ToList();

				Global = new IniSectionData.EditContext(_editorConfigFile.Global);
				Sections = _editorConfigFile.Sections.Select(s => new IniSectionData.EditContext(s)).ToList();
			}

			public IniSectionData.EditContext Global { get; }

			public IReadOnlyList<IniSectionData.EditContext> Sections { get; }

			public void Add(int startLine, IniLineData line) => AddRange(startLine, new[] { line });

			public void AddRange(int startLine, IEnumerable<IniLineData> newLines)
			{
				_lines.InsertRange(startLine - 1, newLines.Select(l => l.ToString()));
			}

			/// <inheritdoc />
			public void Dispose()
			{
				Dispose(true);
				GC.SuppressFinalize(this);
			}

			public void RemoveRange(int startLine, int endLine)
			{
				_lines.RemoveRange(startLine - 1, endLine - startLine);
			}

			public void ReplaceRange(int startLine, int endLine, IEnumerable<IniLineData> newLines)
			{
				// Need something to track and calculate offsets when multiple changes are made
				// For initial implementation, limit things to one change at a time
				RemoveRange(startLine, endLine);
				AddRange(startLine, newLines);
			}

			public void SaveChanges()
			{
				_lines.Clear();

				foreach (var line in Global.Lines)
				{
					_lines.Add(line.ToString());
				}

				foreach (var section in Sections)
				{
					foreach (var line in section.Lines)
					{
						_lines.Add(line.ToString());
					}
				}

				string fullText = string.Join(Environment.NewLine, _lines);

				var bytes = _editorConfigFile._encoding.GetBytes(fullText);

				var length = Math.Max(bytes.Length, _lock.Length);

				_lock.Write(bytes, 0, bytes.Length);

				// Replace the file class' data if the operation is successful
				_editorConfigFile.ResetTo(_lines);
			}

			protected virtual void Dispose(bool disposing)
			{
				if (!disposing)
				{
					return;
				}

				_lock.Dispose();
			}
		}
	}
}