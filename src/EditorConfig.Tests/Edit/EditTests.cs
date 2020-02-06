namespace EditorConfig.Tests.Edit
{
    using System.IO;
    using System.Reflection;

    using EditorConfig.Core;

    using FluentAssertions;

    using NUnit.Framework;

    [TestFixture]
    public class EditTests : EditorConfigTestBase
    {
        [Test]
        public void NotSavingMakesNoChanges()
		{
			EditorConfigFile editorConfigFile = PrepareTest("nochange", out string file, out string workingFile);

			const int TestSection = 0;
			const string TestPropertyKey = "testProperty";
			const string TestPropertyValue = "testValue";

			var iniProperty = new IniProperty(TestPropertyKey, TestPropertyValue);

			using (var editContext = editorConfigFile.Edit())
			{
				editContext.Sections[TestSection].Add(iniProperty);
			}

			var originalText = File.ReadAllText(file);
			var copyText = File.ReadAllText(workingFile);

			originalText.Should().Be(copyText);
		}

		[Test]
        public void AddWorks()
        {
            EditorConfigFile editorConfigFile = PrepareTest("add");

			const int TestSection = 0;
            const string TestPropertyKey = "testProperty";
            const string TestPropertyValue = "testValue";

			// Find aftercomment property line
			editorConfigFile.TryGetProperty("aftercomment", editorConfigFile.Sections[TestSection], out var prop).Should().BeTrue();
			var lineNumber = prop!.LineNumber;

			var iniProperty = new IniProperty(TestPropertyKey, TestPropertyValue);

            using (var editContext = editorConfigFile.Edit())
            {
                editContext.Sections[TestSection].Add(iniProperty);

                editContext.SaveChanges();
            }

            editorConfigFile.TryGetProperty(TestPropertyKey, editorConfigFile.Sections[TestSection], out var updatedProperty).Should().BeTrue();
            updatedProperty!.Line.Value.Should().Be(TestPropertyValue);

			// Use the original value rather than re-reading the property. This ensures that the file is otherwise unchanged
			updatedProperty!.LineNumber.Should().Be(lineNumber + 1);
		}

		private static EditorConfigFile PrepareTest(string filename) => PrepareTest(filename, out _, out _);

		private static EditorConfigFile PrepareTest(string filename, out string file, out string workingFile)
		{
			// Even though this isn't the test method, GetFileFromMethod is actually just determining the namespace
			file = GetFileFromMethod(MethodBase.GetCurrentMethod(), $".{filename}.editorconfig");
			File.Exists(file).Should().BeTrue();

			var directory = Path.GetDirectoryName(file);
			workingFile = Path.Combine(directory!, $".{filename}.copy.editorconfig");
			if (File.Exists(workingFile))
			{
				File.Delete(workingFile);
			}

			File.Copy(file, workingFile);

			return new EditorConfigFile(workingFile);
		}
	}
}
