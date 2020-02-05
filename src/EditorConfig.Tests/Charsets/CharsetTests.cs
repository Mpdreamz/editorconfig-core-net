namespace EditorConfig.Tests.Charsets
{
	using System.Reflection;

	using EditorConfig.Core;

	using FluentAssertions;

	using NUnit.Framework;

	[TestFixture]
	public class CharsetTests : EditorConfigTestBase
	{
		[Test]
		public void Utf8()
		{
			var file = GetConfig(MethodBase.GetCurrentMethod(), "f.x", ".utf8.editorconfig");
			file.Charset.Should().Be(Charset.Utf8);
		}

		[Test]
		public void Utf8Bom()
		{
			var file = GetConfig(MethodBase.GetCurrentMethod(), "f.x", ".utf8-bom.editorconfig");
			file.Charset.Should().Be(Charset.Utf8Bom);
		}

		[Test]
		public void Utf16Le()
		{
			var file = GetConfig(MethodBase.GetCurrentMethod(), "f.x", ".utf16le.editorconfig");
			file.Charset.Should().Be(Charset.Utf16Le);
		}

		[Test]
		public void Utf16Be()
		{
			var file = GetConfig(MethodBase.GetCurrentMethod(), "f.x", ".utf16be.editorconfig");
			file.Charset.Should().Be(Charset.Utf16Be);
		}

		[Test]
		public void Latin1()
		{
			var file = GetConfig(MethodBase.GetCurrentMethod(), "f.x", ".latin1.editorconfig");
			file.Charset.Should().Be(Charset.Latin1);
		}

		[Test]
		public void Bogus()
		{
			var file = GetConfig(MethodBase.GetCurrentMethod(), "f.x", ".bogus.editorconfig");
			file.Charset.Should().BeNull();
			HasBogusKey(file, "charset");
		}
	}
}
