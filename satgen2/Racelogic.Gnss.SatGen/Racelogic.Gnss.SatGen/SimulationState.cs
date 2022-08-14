namespace Racelogic.Gnss.SatGen;

public enum SimulationState
{
	None,
	Ready,
	Initializing,
	Running,
	Pausing,
	Paused,
	Cancelling,
	Cancelled,
	Finished
}
