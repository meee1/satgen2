namespace Racelogic.Libraries.Nmea;

public class Sv
{
	public int Azimuth { get; set; }

	public int Elevation { get; set; }

	public int Prn { get; set; }

	public int Snr { get; set; }

	public bool UsedInSolution { get; set; }

	public Constellation Constellation { get; set; }

	public static bool operator ==(Sv x, Sv y)
	{
		if ((object)x != null && (object)y != null && x.Prn == y.Prn && x.Elevation == y.Elevation && x.Azimuth == y.Azimuth && x.Snr == y.Snr)
		{
			return x.UsedInSolution == y.UsedInSolution;
		}
		return false;
	}

	public static bool operator !=(Sv x, Sv y)
	{
		return !(x == y);
	}

	public override bool Equals(object obj)
	{
		if (obj is Sv)
		{
			return this == (Sv)obj;
		}
		return false;
	}

	public override int GetHashCode()
	{
		return Prn.GetHashCode() ^ Elevation.GetHashCode() ^ Azimuth.GetHashCode() ^ Snr.GetHashCode() ^ UsedInSolution.GetHashCode();
	}
}
