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
            //We only place an editorconfig in this folder to force root.
            //An editorconfig file is not necessary for defaults but we do not want any parent 
            //config files leaking into our test
            var file = GetFileFromMethod(MethodBase.GetCurrentMethod(), ".nochange.editorconfig");
            File.Exists(file).Should().BeTrue();

            var directory = Path.GetDirectoryName(file);
            string workingFile = Path.Combine(directory!, ".nochange.copy.editorconfig");

            if (File.Exists(workingFile))
            {
                File.Delete(workingFile);
            }

            File.Copy(file, workingFile);

            var editorConfigFile = new EditorConfigFile(workingFile);

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
        public void AddRangeWorks()
        {
            //We only place an editorconfig in this folder to force root.
            //An editorconfig file is not necessary for defaults but we do not want any parent 
            //config files leaking into our test
            var file = GetFileFromMethod(MethodBase.GetCurrentMethod(), ".addrange.editorconfig");
            File.Exists(file).Should().BeTrue();

            var directory = Path.GetDirectoryName(file);
            string workingFile = Path.Combine(directory!, ".addrange.copy.editorconfig");

            if (File.Exists(workingFile))
            {
                File.Delete(workingFile);
            }

            File.Copy(file, workingFile);

			var editorConfigFile = new EditorConfigFile(workingFile);

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
			updatedProperty!.LineNumber.Should().Be(lineNumber + 1);
        }
    }
}
