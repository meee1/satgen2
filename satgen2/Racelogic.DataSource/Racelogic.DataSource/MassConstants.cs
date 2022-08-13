namespace Racelogic.DataSource;

public static class MassConstants
{
	public static readonly double DefaultKilogramToGramme;

	public static readonly double DefaultKilogramToPound;

	public static readonly double DefaultKilogramToStone;

	public static readonly double DefaultKilogramToTonne;

	public static readonly double DefaultKilogramToTon;

	public static readonly double DefaultKilogramToNewton;

	public static double KilogramToGramme { get; set; }

	public static double KilogramToPound { get; set; }

	public static double KilogramToStone { get; set; }

	public static double KilogramToTonne { get; set; }

	public static double KilogramToTon { get; set; }

	public static double KilogramToNewton { get; set; }

	static MassConstants()
	{
		DefaultKilogramToGramme = 1000.0;
		DefaultKilogramToPound = 2.2046226218487761;
		DefaultKilogramToStone = 0.15747304441776969;
		DefaultKilogramToTonne = 0.001;
		DefaultKilogramToTon = 0.00110231;
		DefaultKilogramToNewton = 9.80665002863885;
		Reset();
	}

	public static void Reset()
	{
		KilogramToGramme = DefaultKilogramToGramme;
		KilogramToPound = DefaultKilogramToPound;
		KilogramToStone = DefaultKilogramToStone;
		KilogramToTonne = DefaultKilogramToTonne;
		KilogramToTon = DefaultKilogramToTon;
		KilogramToNewton = DefaultKilogramToNewton;
	}

	public static double ConvertFromDefaultMassUnit(double currentMass, MassUnit targetUnit)
	{
		return targetUnit switch
		{
			MassUnit.Gram => currentMass * KilogramToGramme, 
			MassUnit.Pound => currentMass * KilogramToPound, 
			MassUnit.Stone => currentMass * KilogramToStone, 
			MassUnit.Tonne => currentMass * KilogramToTonne, 
			MassUnit.Ton => currentMass * KilogramToTon, 
			MassUnit.Newton => currentMass * KilogramToNewton, 
			_ => currentMass, 
		};
	}

	public static double ConvertToDefaultMassUnit(double currentMass, MassUnit currentUnit)
	{
		return currentUnit switch
		{
			MassUnit.Gram => currentMass / KilogramToGramme, 
			MassUnit.Pound => currentMass / KilogramToPound, 
			MassUnit.Stone => currentMass / KilogramToStone, 
			MassUnit.Tonne => currentMass / KilogramToTonne, 
			MassUnit.Ton => currentMass / KilogramToTon, 
			MassUnit.Newton => currentMass / KilogramToNewton, 
			_ => currentMass, 
		};
	}
}
