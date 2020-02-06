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
        public void CanFindFile()
        {
            //We only place an editorconfig in this folder to force root.
            //An editorconfig file is not necessary for defaults but we do not want any parent 
            //config files leaking into our test
            var file = GetFileFromMethod(MethodBase.GetCurrentMethod(), ".editorconfig");
            File.Exists(file).Should().BeTrue();
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

            const string TestPropertyKey = "testProperty";
            const string TestPropertyValue = "testValue";

            var iniProperty = new IniProperty(TestPropertyKey, TestPropertyValue);

            using (var editContext = editorConfigFile.Edit())
            {
                editContext.Sections[1].Add(iniProperty);

                editContext.SaveChanges();
            }

            editorConfigFile.Sections[1].TryGetProperty(TestPropertyKey, out var updatedProperty).Should().BeTrue();

            updatedProperty!.Value.Should().Be(TestPropertyValue);
        }
    }
}
