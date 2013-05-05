using System;
using System.IO;
using System.Runtime.Serialization;

namespace Microsoft.Xna.Framework.Content
{
	[Serializable]
	public class ContentLoadException : Exception
	{
		public ContentLoadException() : base()
		{
		}

		public ContentLoadException(string message) : base(message)
		{
		}

		public ContentLoadException(string message, Exception innerException) : base(message, innerException)
		{
		}

	}
}
