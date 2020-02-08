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
		public static readonly Regex SectionRegex = new Regex(@"^\s*\[(([^#;]|\\#|\\;)+)\]\s*([#;].*)?$");

		public static readonly Regex CommentRegex = new Regex(@"^\s*[#;](.*)");

		public static readonly Regex PropertyRegex = new Regex(@"^\s*([\w\.\-_]+)\s*[=:]\s*(.*?)\s*([#;].*)?$");

		private readonly Dictionary<string, IniLine<IniSectionData>> _sections = new Dictionary<string, IniLine<IniSectionData>>();

		private readonly List<string> _lines = new List<string>();

		private readonly Encoding _encoding;

		public EditorConfigFile(string file, Encoding? encoding = null, Version? developmentVersion = null)
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

			ParseVersion = developmentVersion ?? EditorConfigParser.Version;

			Parse();

			FileConfiguration = new FileConfiguration(ParseVersion, file, Global.Properties.ToDictionary(p => p.Prop.Key, p => p.Prop.Value));
		}

		public string Directory { get; }

		public FileConfiguration FileConfiguration { get; }

		public string FullPath { get; }

		public IniSectionData Global { get; private set; }

		public bool IsRoot { get; private set; }

		/// <summary>
		/// The editor config parser version in use, defaults to latest <see cref="EditorConfigParser.Version"/>
		/// </summary>
		public Version ParseVersion { get; }

		public IReadOnlyList<IniSectionData> Sections => _sections.Values.Select(s => s.Data).ToList();

		public EditContext Edit(EditorConfigFileOptions? options = null)
		{
			return new EditContext(this, options);
		}

		public IEnumerable<IniLine<IniPropertyData>> GetSectionProperties(IniSectionData section)
		{
			if (section is null)
			{
				throw new ArgumentNullException(nameof(section));
			}

			return section.Properties.Select(tuple => new IniLine<IniPropertyData>(GetLineNumber(section, tuple.Offset), tuple.Prop));
		}

		public bool TryGetComment(string commentText, IniSectionData section, [NotNullWhen(true)] out IniLine<IniCommentData>? comment)
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

			var lineNumber = GetLineNumber(section, offset);

			comment = new IniLine<IniCommentData>(lineNumber, commentData);
			return true;
		}

		public bool TryGetProperty(string key, IniSectionData section, [NotNullWhen(true)] out IniLine<IniPropertyData>? property)
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

			var lineNumber = GetLineNumber(section, offset);

			property = new IniLine<IniPropertyData>(lineNumber, propertyData);
			return true;
		}

		public bool TryGetSection(string name, [NotNullWhen(true)] out IniLine<IniSectionData>? section) => _sections.TryGetValue(name, out section);

		private int GetSectionLineNumber(IniSectionData section) =>
					section.IsGlobal ? 0 : _sections[section.Name].LineNumber;

		private int GetLineNumber(IniSectionData section, int offset) => GetSectionLineNumber(section) + offset + 1;

		private void Parse()
		{
			int currentLineNumber = 0;

			IniSectionData activeSection = Global;
			foreach (var line in _lines)
			{
				try
				{
					var matches = CommentRegex.Matches(line);

					if (matches.Count > 0)
					{
						IniCommentData iniComment = IniCommentData.FromLine(line);

						// We will discard any comments from before the first section
						activeSection.AddLine(iniComment);

						continue;
					}

					matches = PropertyRegex.Matches(line);
					if (matches.Count > 0)
					{
						var prop = new IniPropertyData(line);

						activeSection.AddLine(prop);
						continue;
					}

					matches = SectionRegex.Matches(line);
					if (matches.Count > 0)
					{
						var sectionName = matches[0].Groups[1].Value;
						activeSection = IniSectionData.FromLine(line);

						var iniSection = new IniLine<IniSectionData>(currentLineNumber, activeSection);

						_sections.Add(sectionName, iniSection);
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

			if (bool.TryParse(rootProp.Data.Value, out var isRoot))
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
			private readonly EditorConfigFileOptions _options;
			private FileStream _lock;

			private readonly List<string> _lines;

			public EditContext(EditorConfigFile editorConfigFile, EditorConfigFileOptions? options = null)
			{
				_editorConfigFile = editorConfigFile ?? throw new ArgumentNullException(nameof(editorConfigFile));
				_options = options ?? new EditorConfigFileOptions();

				_lock = CreateLock();

				// Make a copy of the data for editing
				_lines = _editorConfigFile._lines.ToList();

				Global = new SectionEditContext(_editorConfigFile.Global);
				Sections = _editorConfigFile.Sections.ToDictionary(s => s.Name, s => new SectionEditContext(s));
			}

			public SectionEditContext Global { get; }

			public Dictionary<string, SectionEditContext> Sections { get; }

			public SectionEditContext AddSection(IniSectionData sectionData)
			{
				if (sectionData is null)
				{
					throw new ArgumentNullException(nameof(sectionData));
				}

				var sectionEdit = new SectionEditContext(sectionData);
				Sections.Add(sectionData.Name, sectionEdit);

				return sectionEdit;
			}

			public SectionEditContext GetOrAddSection(string sectionName)
			{
				// Create or retrieve the section name
				if (!Sections.TryGetValue(sectionName, out var sectionEditContext))
				{
					return AddSection(new IniSectionData(sectionName));
				}

				return sectionEditContext;
			}

			/// <inheritdoc />
			public void Dispose()
			{
				Dispose(true);
				GC.SuppressFinalize(this);
			}

			public void SaveChanges()
			{
				_lines.Clear();

				foreach (var line in Global)
				{
					WriteLine(line);
				}

				foreach (var section in Sections.Values)
				{
					IniLineData? lastLine = null;
					foreach (var line in section)
					{
						lastLine = line;
						WriteLine(line);
					}

					// Ensure sections end with a comment or a blank line
					IniLineType? lineType = lastLine?.LineType;

					if (_options.EndSectionWithBlankLineOrComment && lineType != IniLineType.Comment && lineType != IniLineType.None)
					{
						WriteLine(new IniEmptyLine());
					}
				}

				_lock.Dispose();
				try
				{
					File.WriteAllLines(_editorConfigFile.FullPath, _lines, _editorConfigFile._encoding);
				}
				finally
				{
					_lock = CreateLock();
				}

				// Replace the file class' data if the operation is successful
				_editorConfigFile.ResetTo(_lines);
			}

			private void WriteLine(IniLineData line)
			{
				if (!_options.AllowConsecutiveEmptyLines && line.LineType == IniLineType.None && string.IsNullOrWhiteSpace(_lines.Last()))
				{
					return;
				}

				_lines.Add(line.ToString());
			}

			protected virtual void Dispose(bool disposing)
			{
				if (!disposing)
				{
					return;
				}

				_lock.Dispose();
			}

			private FileStream CreateLock() => File.OpenWrite(_editorConfigFile.FullPath);
		}
	}
}