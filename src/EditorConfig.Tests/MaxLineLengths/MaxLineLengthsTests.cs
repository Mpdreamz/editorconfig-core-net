using System.Reflection;
using FluentAssertions;
using NUnit.Framework;

namespace EditorConfig.Tests.MaxLineLengths
{
	[TestFixture]
	public class MaxLineLengthsTests : EditorConfigTestBase
	{
		[Test]
		public void PositiveNumber()
		{
			var file = GetConfig(MethodBase.GetCurrentMethod(), "f.x", ".positive.editorconfig");
			file.MaxLineLength.Should().HaveValue();
#pragma warning disable CS8629 // Nullable value type may be null.
			file.MaxLineLength.Value.Should().Be(120);
#pragma warning restore CS8629 // Nullable value type may be null.
		}

		[Test]
		public void NegativeNumber()
		{
			var file = GetConfig(MethodBase.GetCurrentMethod(), "f.x", ".negative.editorconfig");
			file.MaxLineLength.Should().NotHaveValue();
		}

		[Test]
		public void Bogus()
		{
			var file = GetConfig(MethodBase.GetCurrentMethod(), "f.x", ".bogus.editorconfig");
			file.MaxLineLength.Should().NotHaveValue();
			HasBogusKey(file, "max_line_length");
		}
	}
}
