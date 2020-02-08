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

			var iniProperty = new IniPropertyData(TestPropertyKey, TestPropertyValue);

			var sectionName = editorConfigFile.Sections[TestSection].Name;

			using (var editContext = editorConfigFile.Edit())
			{
				editContext.Sections[sectionName].Add(iniProperty);
			}

			var originalText = File.ReadAllText(file);
			var copyText = File.ReadAllText(workingFile);

			originalText.Should().Be(copyText);
		}

		[Test]
		public void AddSectionWorks()
		{
			EditorConfigFile editorConfigFile = PrepareTest("addsection", out var file, out var workingFile);

			IniSectionData iniSectionData = new IniSectionData("*.cs")
			{
				new IniCommentData("TEST ADDED SECTION"),
				new IniPropertyData("testProperty", "testValue"),
				new IniEmptyLine(),
				new IniCommentData("ANOTHER COMMENT"),
				new IniPropertyData("anotherProperty", "anotherValue")
			};

			var sectionLength = iniSectionData.Length;

			using (var editContext = editorConfigFile.Edit())
			{
				editContext.AddSection(iniSectionData);

				editContext.SaveChanges();
			}

			editorConfigFile.Sections.Count.Should().Be(2);

			// Confirm the file is one line longer
			var fileLength = File.ReadAllLines(file).Length;
			var workingFileLength = File.ReadAllLines(workingFile).Length;

			workingFileLength.Should().Be(fileLength + sectionLength);
		}

		[Test]
		public void AddWorks()
		{
			EditorConfigFile editorConfigFile = PrepareTest("add", out var file, out var workingFile);

			const int TestSection = 0;
			const string TestPropertyKey = "testProperty";
			const string TestPropertyValue = "testValue";

			var section = editorConfigFile.Sections[TestSection];
			var sectionName = section.Name;

			// Find aftercomment property line
			editorConfigFile.TryGetProperty("aftercomment", section, out var prop).Should().BeTrue();
			var lineNumber = prop!.LineNumber;

			var iniProperty = new IniPropertyData(TestPropertyKey, TestPropertyValue);

			using (var editContext = editorConfigFile.Edit())
			{
				editContext.Sections[sectionName].Add(iniProperty);

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
		public void RemoveSectionWorks()
		{
			EditorConfigFile editorConfigFile = PrepareTest("removesection", out var file, out var workingFile);

			const int TestSection = 0;

			// Find number of lines in section
			var sectionLength = editorConfigFile.Sections[TestSection].Length;
			var sectionName = editorConfigFile.Sections[TestSection].Name;

			using (var editContext = editorConfigFile.Edit())
			{
				editContext.Sections.Remove(sectionName);

				editContext.SaveChanges();
			}

			editorConfigFile.Sections.Count.Should().Be(0);

			// Confirm the file is shorter by the number of removed lines
			var fileLength = File.ReadAllLines(file).Length;
			var workingFileLength = File.ReadAllLines(workingFile).Length;

			workingFileLength.Should().Be(fileLength - sectionLength);
		}

		[Test]
		public void RemoveWorks()
		{
			EditorConfigFile editorConfigFile = PrepareTest("remove", out var file, out var workingFile);

			const int TestSection = 0;
			var sectionName = editorConfigFile.Sections[TestSection].Name;

			// Find aftercomment property line
			editorConfigFile.TryGetProperty("aftercomment", editorConfigFile.Sections[TestSection], out var afterProp).Should().BeTrue();
			var lineNumber = afterProp!.LineNumber;

			// Remove beforecommment
			editorConfigFile.TryGetProperty("beforecomment", editorConfigFile.Sections[TestSection], out var prop).Should().BeTrue();
			using (var editContext = editorConfigFile.Edit())
			{
				editContext.Sections[sectionName].Remove(prop!).Should().BeTrue();

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

		private static EditorConfigFile PrepareTest(
			string filename,
			out string file,
			out string workingFile)
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
