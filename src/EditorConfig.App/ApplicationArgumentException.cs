using System;
using System.Globalization;

namespace EditorConfig.App
{
	[Serializable]
	public class ApplicationArgumentException : Exception
	{
		public ApplicationArgumentException(string message, params object[] args)
			: base(string.Format(CultureInfo.InvariantCulture, message, args))
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

		protected ApplicationArgumentException(System.Runtime.Serialization.SerializationInfo serializationInfo, System.Runtime.Serialization.StreamingContext streamingContext) :
			base(serializationInfo, streamingContext)
		{
		}
	}
}