namespace Racelogic.Gnss.SatGen;

public readonly struct SignalObservation
{
	private readonly double pseudoRange;

	private readonly decimal dopplerFrequency;

	public double PseudoRange => pseudoRange;

	public decimal DopplerFrequency => dopplerFrequency;

	public SignalObservation(in double pseudoRange, in decimal dopplerFrequency)
	{
		this.pseudoRange = pseudoRange;
		this.dopplerFrequency = dopplerFrequency;
	}

	public static bool operator ==(SignalObservation left, SignalObservation right)
	{
		return left.Equals(right);
	}

	public static bool operator !=(SignalObservation left, SignalObservation right)
	{
		return !(left == right);
	}

	public override bool Equals(object? obj)
	{
		if (obj == null)
		{
			return false;
		}
		if (obj!.GetType() != typeof(SignalObservation))
		{
			return false;
		}
		return Equals((SignalObservation)obj);
	}

	public bool Equals(SignalObservation other)
	{
		if (other.dopplerFrequency.Equals(dopplerFrequency))
		{
			return other.pseudoRange.Equals(pseudoRange);
		}
		return false;
	}

	public override int GetHashCode()
	{
		return (5993773 + dopplerFrequency.GetHashCode()) * 9973 + pseudoRange.GetHashCode();
	}
}
