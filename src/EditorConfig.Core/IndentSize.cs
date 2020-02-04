namespace EditorConfig.Core
{
	/// <summary>
	/// a whole number defining the number of columns used for each indentation level and the width of soft tabs (when supported). 
	/// When set to tab, the value of tab_width (if specified) will be used.
	/// </summary>
	public class IndentSize
	{
		public bool Unset { get; }
		public bool UseTabWidth { get; }
		public int? NumberOfColumns { get; private set; }

		public IndentSize()
		{
			Unset = true;
		}

		public IndentSize(bool useTabs)
		{
			UseTabWidth = useTabs;
		}

		public IndentSize(int numberOfColumns)
		{
			NumberOfColumns = numberOfColumns;
		}
	}
}