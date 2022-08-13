namespace Racelogic.Core.Filters;

public struct TwoPoleButterworthFilterCoEfficients
{
	public readonly double Gain;

	public readonly double CoEfficient1;

	public readonly double CoEfficient2;

	public TwoPoleButterworthFilterCoEfficients(double gain, double coEfficient1, double coEfficient2)
	{
		Gain = gain;
		CoEfficient1 = coEfficient1;
		CoEfficient2 = coEfficient2;
	}
}
