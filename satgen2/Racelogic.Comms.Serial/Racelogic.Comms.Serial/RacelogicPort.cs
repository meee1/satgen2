namespace Racelogic.Comms.Serial;

public class RacelogicPort
{
	public readonly string PortName;

	public readonly int Vid;

	public readonly int Pid;

	public RacelogicPort(string portName, int vid, int pid)
	{
		PortName = portName;
		Vid = vid;
		Pid = pid;
	}
}
