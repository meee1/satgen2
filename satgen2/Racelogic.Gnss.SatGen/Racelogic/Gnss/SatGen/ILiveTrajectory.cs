using System;

namespace Racelogic.Gnss.SatGen
{
	public interface ILiveTrajectory
	{
		internal void AdvanceSampleClock(TimeSpan advance);
	}
}
