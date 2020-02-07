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
            EditorConfigFile editorConfigFile = PrepareTest("add", out var file, out var workingFile);

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
            updatedProperty!.Data.Value.Should().Be(TestPropertyValue);

			// Use the original value rather than re-reading the property. This ensures that the file is otherwise unchanged
			updatedProperty!.LineNumber.Should().Be(lineNumber + 1);

			// Confirm the file is one line longer
			var fileLength = File.ReadAllLines(file).Length;
			var workingFileLength = File.ReadAllLines(workingFile).Length;

			workingFileLength.Should().Be(fileLength + 1);
		}

		[Test]
		public void RemoveWorks()
		{
			EditorConfigFile editorConfigFile = PrepareTest("remove", out var file, out var workingFile);

			const int TestSection = 0;

			// Find aftercomment property line
			editorConfigFile.TryGetProperty("aftercomment", editorConfigFile.Sections[TestSection], out var afterProp).Should().BeTrue();
			var lineNumber = afterProp!.LineNumber;

			// Remove beforecommment
			editorConfigFile.TryGetProperty("beforecomment", editorConfigFile.Sections[TestSection], out var prop).Should().BeTrue();
			using (var editContext = editorConfigFile.Edit())
			{
				editContext.Sections[TestSection].Remove(prop!).Should().BeTrue();

				editContext.SaveChanges();
			}

			editorConfigFile.TryGetProperty("beforecomment", editorConfigFile.Sections[TestSection], out _).Should().BeFalse();

			// Confirm aftercomment has moved up a line
			editorConfigFile.TryGetProperty("aftercomment", editorConfigFile.Sections[TestSection], out var updatedProperty).Should().BeTrue();
			updatedProperty!.LineNumber.Should().Be(lineNumber - 1);

			// Confirm the file is one line shorter
			var fileLength = File.ReadAllLines(file).Length;
			var workingFileLength = File.ReadAllLines(workingFile).Length;

			workingFileLength.Should().Be(fileLength - 1);
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
