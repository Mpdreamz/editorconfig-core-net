namespace EditorConfig.Core
{
	public class EditorConfigFileOptions
	{
		public bool AllowConsecutiveEmptyLines { get; set; }

		public bool EndSectionWithBlankLineOrComment { get; set; } = true;

		public bool TrimEmptyLineFromEndOfFile { get; set; } = true;
	}
}