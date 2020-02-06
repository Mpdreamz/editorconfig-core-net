namespace EditorConfig.Tests.Comments
{
    using System.IO;
    using System.Reflection;

    using EditorConfig.Core;

    using FluentAssertions;

    using NUnit.Framework;

    [TestFixture]
    public class CommentsTests : EditorConfigTestBase
    {
        private const string GlobalCommentText = "GLOBAL COMMENT";
        private const string SectionCommentText = "SECTION COMMENT";
        private const string FileName = ".editorconfig";

        [Test]
        public void CanFindFile()
        {
            //We only place an editorconfig in this folder to force root.
            //An editorconfig file is not necessary for defaults but we do not want any parent 
            //config files leaking into our test
            var file = GetFileFromMethod(MethodBase.GetCurrentMethod(), FileName);
            File.Exists(file).Should().BeTrue();
        }

        [Test]
        public void ExistingGlobalCommentIsFound()
        {
            var file = GetFileFromMethod(MethodBase.GetCurrentMethod(), FileName);
            EditorConfigFile editorConfigFile = new EditorConfigFile(file);

            editorConfigFile.TryFindComment(GlobalCommentText, editorConfigFile.Global, out var comment).Should().BeTrue();

            comment!.Line.Text.Should().Be(GlobalCommentText);
            comment.LineNumber.Should().Be(3);
        }

        [Test]
        public void NonexistentGlobalCommentIsNotFound()
        {
            var file = GetFileFromMethod(MethodBase.GetCurrentMethod(), FileName);
            EditorConfigFile editorConfigFile = new EditorConfigFile(file);

            editorConfigFile.TryFindComment("Not a real comment", editorConfigFile.Global, out var comment).Should().BeFalse();

            comment.Should().BeNull();
        }

        [Test]
        public void ExistingSectionCommentIsFound()
        {
            var file = GetFileFromMethod(MethodBase.GetCurrentMethod(), FileName);
            EditorConfigFile editorConfigFile = new EditorConfigFile(file);

            var section = editorConfigFile.Sections[0];
            editorConfigFile.TryFindComment(SectionCommentText, section, out var comment).Should().BeTrue();

            comment!.Line.Text.Should().Be(SectionCommentText);
            comment.LineNumber.Should().Be(8);
        }

        [Test]
        public void NonexistentSectionCommentIsNotFound()
        {
            var file = GetFileFromMethod(MethodBase.GetCurrentMethod(), FileName);
            EditorConfigFile editorConfigFile = new EditorConfigFile(file);

            var section = editorConfigFile.Sections[0];
            editorConfigFile.TryFindComment("Not a real comment", section, out var comment).Should().BeFalse();

            comment.Should().BeNull();
        }
    }
}
