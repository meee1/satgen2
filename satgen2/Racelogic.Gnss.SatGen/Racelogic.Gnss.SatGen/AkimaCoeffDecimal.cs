namespace Racelogic.Gnss.SatGen;

internal readonly struct AkimaCoeffDecimal
{
	public readonly decimal C0;

	public readonly decimal C1;

	public readonly decimal C2;

	public readonly decimal C3;

	public AkimaCoeffDecimal(in decimal c0, in decimal c1, in decimal c2, in decimal c3)
	{
		C0 = c0;
		C1 = c1;
		C2 = c2;
		C3 = c3;
	}
}
