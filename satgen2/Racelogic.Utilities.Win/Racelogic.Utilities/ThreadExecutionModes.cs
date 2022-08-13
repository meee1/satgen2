using System;

namespace Racelogic.Utilities;

[Flags]
public enum ThreadExecutionModes
{
	None = 0,
	KeepSystemAwake = 1,
	KeepDisplayOn = 2,
	AwayMode = 4
}
