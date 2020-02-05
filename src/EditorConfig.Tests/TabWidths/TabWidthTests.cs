namespace EditorConfig.Tests.TabWidths
{
	using System.Reflection;

	using FluentAssertions;

	using NUnit.Framework;

	[TestFixture]
	public class TabWidthTests : EditorConfigTestBase
	{
		[Test]
		public void PositiveNumber()
		{
			var file = GetConfig(MethodBase.GetCurrentMethod(), "f.x", ".positive.editorconfig");
			file.TabWidth.Should().HaveValue();
#pragma warning disable CS8629 // Nullable value type may be null.
			file.TabWidth?.Should().Be(4);
#pragma warning restore CS8629 // Nullable value type may be null.
		}

		[Test]
		public void NegativeNumber()
		{
			var file = GetConfig(MethodBase.GetCurrentMethod(), "f.x", ".negative.editorconfig");
			file.TabWidth.Should().NotHaveValue();
		}

		[Test]
		public void TabIndentSizeAndSpecifiedTabWidth()
		{
			var file = GetConfig(MethodBase.GetCurrentMethod(), "f.x", ".tab.editorconfig");
			file.TabWidth.Should().HaveValue();
#pragma warning disable CS8629 // Nullable value type may be null.
			file.TabWidth?.Should().Be(4);
#pragma warning restore CS8629 // Nullable value type may be null.

			// Set indent_size to tab_width if indent_size is "tab"
			file.IndentSize.Should().NotBeNull();
#pragma warning disable CS8602 // Dereference of a possibly null reference.
			file.IndentSize.NumberOfColumns.Should().Be(file.TabWidth);
#pragma warning restore CS8602 // Dereference of a possibly null reference.
		}

		[Test]
		public void Bogus()
		{
			var file = GetConfig(MethodBase.GetCurrentMethod(), "f.x", ".bogus.editorconfig");
			file.IndentSize.Should().BeNull();
			HasBogusKey(file, "tab_width");
		}
	}
}
