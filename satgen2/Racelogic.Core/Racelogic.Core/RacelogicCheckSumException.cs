using System;

namespace Racelogic.Core;

public class RacelogicCheckSumException : Exception
{
	public RacelogicCheckSumException(string message)
		: base(message)
	{
	}

	public RacelogicCheckSumException(string message, Exception innerException)
		: base(message, innerException)
	{
	}
}
