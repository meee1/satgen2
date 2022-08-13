using System;

namespace Racelogic.Utilities;

public class EventArgs<T> : EventArgs
{
	public T Parameter { get; set; }

	public EventArgs(T input)
	{
		Parameter = input;
	}
}
