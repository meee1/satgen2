using System;

namespace Racelogic.DataSource;

[Flags]
public enum ToStringOptions
{
	None = 0,
	AlwaysSigned = 1,
	ReturnAbsolute = 2
}
