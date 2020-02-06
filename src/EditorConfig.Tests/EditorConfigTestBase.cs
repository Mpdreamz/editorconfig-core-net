namespace EditorConfig.Tests
{
	using System;
	using System.Globalization;
	using System.IO;
	using System.Reflection;

	using EditorConfig.Core;

	using FluentAssertions;

	public class EditorConfigTestBase
	{
		protected static FileConfiguration GetConfig(MethodBase? method, string fileName, string configurationFile = ".editorconfig")
		{
			if (method == null)
			{
				throw new ArgumentException("The method must not be null", nameof(method));
			}

			var file = GetFileFromMethod(method, fileName);
			var parser = new EditorConfigParser(configurationFile);
			var fileConfigs = parser.Parse(file);
			fileConfigs.Should().NotBeNull();
			return fileConfigs;
		}

		protected static string GetFileFromMethod(MethodBase? method, string fileName)
		{
			if (method is null)
			{
				throw new ArgumentNullException(nameof(method));
			}

			if (string.IsNullOrEmpty(fileName))
			{
				throw new ArgumentException("message", nameof(fileName));
			}

			var type = method.DeclaringType;

			if (type == null)
			{
				throw new ArgumentException("The method's declaring type must be non-null", nameof(method));
			}

			var @namespace = type.Namespace;
			var folderSep = Path.DirectorySeparatorChar.ToString(CultureInfo.InvariantCulture);

			if (@namespace == null)
			{
				throw new ArgumentException("The method's declaring type's namespace must be non-null", nameof(method));
			}

			var folder = @namespace
				.Replace("EditorConfig.Tests.", "", StringComparison.Ordinal)
				.Replace(".", folderSep, StringComparison.Ordinal);

			var file = Path.Combine(folder, fileName.Replace(@"\", folderSep, StringComparison.Ordinal));

			var cwd = Environment.CurrentDirectory;

			return Path.Combine(
			                    cwd.Replace(OutputPath("Release"), "", StringComparison.Ordinal)
				                    .Replace(OutputPath("Debug"), "", StringComparison.Ordinal),
			                    file);

			string OutputPath(string configuration) => $"bin{folderSep}netcoreapp3.1{folderSep}{configuration}";
		}

		protected static void HasBogusKey(FileConfiguration file, string key)
		{
			if (file is null)
			{
				throw new ArgumentNullException(nameof(file));
			}

			if (string.IsNullOrEmpty(key))
			{
				throw new ArgumentException("message", nameof(key));
			}

			file.Properties.Should().NotBeEmpty().And.HaveCount(1).And.ContainKey(key);
			var bogusCharset = file.Properties[key];
			bogusCharset.Should().Be("bogus");
		}
	}
}