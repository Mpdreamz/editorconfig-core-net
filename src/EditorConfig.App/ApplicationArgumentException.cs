using System;

namespace EditorConfig.App
{
	public class ApplicationArgumentException : Exception
	{
		public ApplicationArgumentException(string message, params object[] args)
			: base(string.Format(message, args))
		{
		}

		public ApplicationArgumentException() : base()
		{
		}

		public ApplicationArgumentException(string message) : base(message)
		{
		}

		public ApplicationArgumentException(string message, Exception innerException) : base(message, innerException)
		{
		}
	}
}