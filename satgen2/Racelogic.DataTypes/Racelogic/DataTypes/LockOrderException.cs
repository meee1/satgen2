using System;
using System.Globalization;
using System.Runtime.Serialization;

namespace Racelogic.DataTypes
{
	[Serializable]
	public class LockOrderException : Exception
	{
		public LockOrderException()
		{
		}

		public LockOrderException(string message)
			: base(message)
		{
		}

		public LockOrderException(string message, Exception innerException)
			: base(message, innerException)
		{
		}

		protected LockOrderException(SerializationInfo info, StreamingContext context)
			: base(info, context)
		{
		}

		public LockOrderException(string format, params object[] args)
			: this(string.Format(CultureInfo.InvariantCulture, format, args))
		{
		}
	}
}
