namespace Racelogic.Gnss.SatGen;

internal readonly struct AkimaCoeff
{
	public readonly double C0;

	public readonly double C1;

	public readonly double C2;

	public readonly double C3;

	public AkimaCoeff(in double c0, in double c1, in double c2, in double c3)
	{
		C0 = c0;
		C1 = c1;
		C2 = c2;
		C3 = c3;
	}
}
