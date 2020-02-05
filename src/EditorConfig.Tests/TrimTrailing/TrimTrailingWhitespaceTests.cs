﻿namespace EditorConfig.Tests.TrimTrailing
{
	using System.Reflection;

	using FluentAssertions;

	using NUnit.Framework;

	[TestFixture]
	public class TrimTrailingWhitespaceTests : EditorConfigTestBase
	{
		[Test]
		public void True()
		{
			var file = GetConfig(MethodBase.GetCurrentMethod(), "f.x", ".true.editorconfig");
			file.TrimTrailingWhitespace.Should().BeTrue();
		}

		[Test]
		public void False()
		{
			var file = GetConfig(MethodBase.GetCurrentMethod(), "f.x", ".false.editorconfig");
			file.TrimTrailingWhitespace.Should().BeFalse();
		}

		[Test]
		public void Bogus()
		{
			var file = GetConfig(MethodBase.GetCurrentMethod(), "f.x", ".bogus.editorconfig");
			file.TrimTrailingWhitespace.Should().NotHaveValue();
			HasBogusKey(file, "trim_trailing_whitespace");
		}
	}
}
