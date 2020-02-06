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

			Global = IniSectionData.Global();

			Parse();

			if (!Global.TryGetProperty("root", out var rootProp))
			{
				return;
			}

			if (bool.TryParse(rootProp.Value, out var isRoot))
			{
				IsRoot = isRoot;
			}
		}

		public string Directory { get; }

		public string FullPath { get; }

		public IniSectionData Global { get; }

		public bool IsRoot { get; }

		public IReadOnlyList<IniSectionData> Sections => _sections.Values.ToList();

		public EditContext Edit()
		{
			return new EditContext(this);
		}

		public bool TryFindComment(string commentText, IniSectionData section, [NotNullWhen(true)] out IniLine<IniComment>? comment)
		{
			if (string.IsNullOrWhiteSpace(commentText))
			{
				throw new ArgumentException("message", nameof(commentText));
			}

			if (section == null)
			{
				throw new ArgumentNullException(nameof(section));
			}

			if (!section.TryFindComment(commentText, out var commentData, out var offset))
			{
				comment = null;
				return false;
			}

			var baseLineNumber = section.IsGlobal ? 0 : _sections.First(kvp => ReferenceEquals(kvp.Value, section)).Key;

			var lineNumber = baseLineNumber + offset + 1;

			comment = new IniLine<IniComment>(lineNumber, commentData);
			return true;
		}

		private void Parse()
		{
			_lines.Clear();
			_lines.AddRange(File.ReadAllLines(FullPath, _encoding));

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
					}

					activeSection.AddLine(new IniEmptyLine());
				}
				finally
				{
					currentLineNumber++;
				}
			}
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

				Sections = _editorConfigFile.Sections.Select(s => new IniSectionData.EditContext(s)).ToList();
			}

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
				string fullText = string.Join(Environment.NewLine, _editorConfigFile._lines);

				var bytes = _editorConfigFile._encoding.GetBytes(fullText);

				_lock.Write(bytes, 0, bytes.Length);

				// Replace the file class' data if the operation is successful
				_editorConfigFile._lines.Clear();
				_editorConfigFile._lines.AddRange(_lines);
			}

			protected virtual void Dispose(bool disposing)
			{
				if (!disposing)
				{
					return;
				}

				_lock.Dispose();

				_editorConfigFile.Parse();
			}
		}
	}
}