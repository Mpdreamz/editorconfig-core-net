namespace EditorConfig.App
{
	using System;
	using System.Collections.Generic;
	using System.Diagnostics;
	using System.Diagnostics.CodeAnalysis;
	using System.Linq;

	using EditorConfig.Core;

	public static class Program
	{
		private static readonly string FullVersionInfo = "EditorConfig .NET Version " + EditorConfigParser.VersionString;

		private static readonly string Usage = @"
Usage: editorconfig [OPTIONS] FILEPATH1 [FILEPATH2 FILEPATH3 ...]

" + FullVersionInfo + @"

FILEPATH can be a hyphen (-) if you want path(s) to be read from stdin.

Options:

	-h, --help     output usage information
	-v, --version  output the version number
	-f <path>      Specify conf filename other than "".editorconfig""
	-b <version>   Specify version (used by devs to test compatibility)
";

		[SuppressMessage("Design", "CA1031:Do not catch general exception types", Justification = "<Pending>")]
		public static void Main(string[] args)
		{
			if (args is null)
			{
				throw new ArgumentNullException(nameof(args));
			}

			try
			{
				var arguments = new ArgumentsParser(args);
				if (arguments.PrintVersion)
				{
					Console.WriteLine(FullVersionInfo);
					return;
				}
				if (arguments.PrintHelp)
				{
					Console.WriteLine(Usage.Trim());
					return;
				}

				var configParser = new EditorConfigParser(arguments.ConfigFileName, arguments.DevelopVersion);

				var results = configParser.ParseMany(arguments.FileNames).ToList();
				if (results.Count == 0)
				{
					PrintError("Did not find any config for files:{0}", string.Join(",", args));
					Environment.Exit(1);
				}

				PrintParserResults(results.ToList());
			}
			catch (ApplicationArgumentException e)
			{
				PrintUsage(e.Message);
				Environment.Exit(1);
			}
			catch (Exception e)
			{
				PrintError(e.Message);
				Environment.Exit(1);
			}
		}

		private static void PrintParserResults(IList<FileConfiguration> configurations)
		{
			Debug.WriteLine(":: OUTPUT ::::::::::::::::::");
			foreach (var config in configurations)
			{
				if (configurations.Count != 1)
				{
					Console.WriteLine("[{0}]", config.FileName);
					Debug.WriteLine("[{0}]", config.FileName);
				}
				foreach (var kv in config.Properties)
				{
					Console.WriteLine("{0}={1}", kv.Key, kv.Value);
					Debug.WriteLine("{0}={1}", kv.Key, kv.Value);
				}
			}
		}

		private static void PrintError(string errorMessageFormat, params object[] args)
		{
			var d = Console.ForegroundColor;
			Console.ForegroundColor = ConsoleColor.Red;
			Console.WriteLine(errorMessageFormat, args);
			Console.Error.WriteLine(errorMessageFormat, args);
			Console.ForegroundColor = d;
		}

		private static void PrintUsage(string? errorMessageFormat = null, params object[] args)
		{
			if (!string.IsNullOrWhiteSpace(errorMessageFormat))
			{
#pragma warning disable CS8604 // Possible null reference argument.
				PrintError(errorMessageFormat, args);
#pragma warning restore CS8604 // Possible null reference argument.
			}

			Console.WriteLine(Usage.Trim());
			Environment.Exit(1);
		}
	}
}
