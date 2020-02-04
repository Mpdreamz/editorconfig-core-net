﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using EditorConfig.Core;

namespace EditorConfig.App
{
	public static class Program
	{
		private static readonly string _fullVersionInfo = "EditorConfig .NET Version " + EditorConfigParser.VersionString;

		private static readonly string _usage = @"
Usage: editorconfig [OPTIONS] FILEPATH1 [FILEPATH2 FILEPATH3 ...]

" + _fullVersionInfo + @"

FILEPATH can be a hyphen (-) if you want path(s) to be read from stdin.

Options:

	-h, --help     output usage information
	-v, --version  output the version number
	-f <path>      Specify conf filename other than "".editorconfig""
	-b <version>   Specify version (used by devs to test compatibility)
";

		private static void Main(string[] args)
		{
			try
			{
				var arguments = new ArgumentsParser(args);
				if (arguments.PrintVersion)
				{
					Console.WriteLine(_fullVersionInfo);
					return;
				}
				if (arguments.PrintHelp)
				{
					Console.WriteLine(_usage.Trim());
					return;
				}

				var configParser = new EditorConfigParser(arguments.ConfigFileName, arguments.DevelopVersion);

				var results = configParser.Parse(arguments.FileNames).ToList();
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

		private static void PrintUsage(string errorMessageFormat = null, params object[] args)
		{
			if (!string.IsNullOrWhiteSpace(errorMessageFormat))
			{
				PrintError(errorMessageFormat, args);
			}

			Console.WriteLine(_usage.Trim());
			Environment.Exit(1);
		}
	}
}
