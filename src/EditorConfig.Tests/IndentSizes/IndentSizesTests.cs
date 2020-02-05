namespace EditorConfig.Tests.IndentSizes
{
	using System.Reflection;

	using FluentAssertions;

	using NUnit.Framework;

	[TestFixture]
	public class IndentSizesTests : EditorConfigTestBase
	{
		[Test]
		public void PositiveNumber()
		{
			var file = GetConfig(MethodBase.GetCurrentMethod(), "f.x", ".positive.editorconfig");
			file.IndentSize.Should().NotBeNull();
#pragma warning disable CS8602 // Dereference of a possibly null reference.
			file.IndentSize.NumberOfColumns.Should().Be(2);
#pragma warning restore CS8602 // Dereference of a possibly null reference.
			file.IndentSize.UseTabWidth.Should().BeFalse();

			//tab_width is unspecified and indent_size is a positive integer, editorconfig dictates 
			//that tabwidth should thus default to indent_size
			file.Properties.Should().HaveCount(2);
			file.TabWidth.Should().Be(file.IndentSize.NumberOfColumns);
		}

		[Test]
		public void NegativeNumber()
		{
			var file = GetConfig(MethodBase.GetCurrentMethod(), "f.x", ".negative.editorconfig");
			file.IndentSize.Should().BeNull();
		}

		[Test]
		public void Tab()
		{
			var file = GetConfig(MethodBase.GetCurrentMethod(), "f.x", ".tab.editorconfig");
			file.IndentSize.Should().NotBeNull();
#pragma warning disable CS8602 // Dereference of a possibly null reference.
			file.IndentSize.NumberOfColumns.Should().NotHaveValue();
#pragma warning restore CS8602 // Dereference of a possibly null reference.
			file.IndentSize.UseTabWidth.Should().BeTrue();
		}

		[Test]
		public void Bogus()
		{
			var file = GetConfig(MethodBase.GetCurrentMethod(), "f.x", ".bogus.editorconfig");
			file.IndentSize.Should().BeNull();
			HasBogusKey(file, "indent_size");
		}
	}
}
