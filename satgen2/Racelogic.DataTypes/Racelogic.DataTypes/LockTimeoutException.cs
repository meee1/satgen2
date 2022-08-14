using System;
using System.Globalization;
using System.Runtime.Serialization;

namespace Racelogic.DataTypes;

[Serializable]
public class LockTimeoutException : Exception
{
	public LockTimeoutException()
	{
	}

	public LockTimeoutException(string message)
		: base(message)
	{
	}

	public LockTimeoutException(string message, Exception innerException)
		: base(message, innerException)
	{
	}

	public LockTimeoutException(string format, params object[] args)
		: this(string.Format(CultureInfo.InvariantCulture, format, args))
	{
	}

	protected LockTimeoutException(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
	}
}
