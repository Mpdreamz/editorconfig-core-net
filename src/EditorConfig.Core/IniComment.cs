namespace EditorConfig.Core
{
    public class IniComment : IniLineData
    {
        public IniComment(string text) : base(IniLineType.Comment)
        {
            Text = text;
        }

        public string Text { get; }

        /// <inheritdoc />
        public override string ToString()
        {
            return $"# {Text}";
        }
    }
}
