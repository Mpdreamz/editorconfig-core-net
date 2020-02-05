namespace EditorConfig.App
{
	using System;
	using System.Globalization;
	using System.Runtime.Serialization;

	[Serializable]
	public class ApplicationArgumentException : Exception
	{
		public ApplicationArgumentException(string message, params object[] args)
			: base(string.Format(CultureInfo.InvariantCulture, message, args))
		{
		}

		public ApplicationArgumentException()
		{
		}

		public ApplicationArgumentException(string message) : base(message)
		{
		}

		public ApplicationArgumentException(string message, Exception innerException) : base(message, innerException)
		{
		}

		protected ApplicationArgumentException(SerializationInfo serializationInfo, StreamingContext streamingContext) :
			base(serializationInfo, streamingContext)
		{
		}
	}
}