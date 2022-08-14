using Racelogic.Geodetics;

namespace Racelogic.Gnss.SatGen;

internal abstract class Modulation
{
	protected readonly ModulationBank ModulationBank;

	protected readonly Signal Signal;

	protected readonly double IntervalLength;

	protected readonly GnssTime TimeStamp;

	protected Modulation(ModulationBank modulationBank, Signal signal, in double intervalLength, in GnssTime timeStamp)
	{
		ModulationBank = modulationBank;
		Signal = signal;
		IntervalLength = intervalLength;
		TimeStamp = timeStamp;
	}

	public abstract sbyte[] Modulate();
}
