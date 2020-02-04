using System.Collections.Generic;

namespace EditorConfig.Core
{
	/// <summary>
	/// Represents an ini section within the editorconfig file
	/// </summary>
	public class IniSection : Dictionary<string, string>
	{
		public string Name { get; set; }
	}
}
