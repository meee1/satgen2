namespace Racelogic.DataSource;

public static class PressureConstants
{
	public static readonly double DefaultPsiToBar;

	public static readonly double DefaultPsiToAtmosphere;

	public static readonly double DefaultPsiToKiloPascal;

	public static readonly double DefaultPsiToInHg;

	public static double PsiToBar { get; set; }

	public static double PsiToAtmosphere { get; set; }

	public static double PsiToKiloPascal { get; set; }

	public static double PsiToInHg { get; set; }

	static PressureConstants()
	{
		DefaultPsiToBar = 0.06894757;
		DefaultPsiToAtmosphere = 0.068045961016531;
		DefaultPsiToKiloPascal = 6.894757;
		DefaultPsiToInHg = 2.036025;
		Reset();
	}

	public static void Reset()
	{
		PsiToBar = DefaultPsiToBar;
		PsiToAtmosphere = DefaultPsiToAtmosphere;
		PsiToKiloPascal = DefaultPsiToKiloPascal;
		PsiToInHg = DefaultPsiToInHg;
	}

	public static double ConvertToDefaultPressureUnit(double currentPressure, PressureUnit currentUnit)
	{
		return currentUnit switch
		{
			PressureUnit.Atmosphere => currentPressure / PsiToAtmosphere, 
			PressureUnit.Bar => currentPressure / PsiToBar, 
			PressureUnit.KiloPascal => currentPressure / PsiToKiloPascal, 
			PressureUnit.inHg => currentPressure / PsiToInHg, 
			_ => currentPressure, 
		};
	}

	public static double ConvertFromDefaultPressureUnit(double currentPressure, PressureUnit targetUnit)
	{
		return targetUnit switch
		{
			PressureUnit.Atmosphere => currentPressure * PsiToAtmosphere, 
			PressureUnit.Bar => currentPressure * PsiToBar, 
			PressureUnit.KiloPascal => currentPressure * PsiToKiloPascal, 
			PressureUnit.inHg => currentPressure * PsiToInHg, 
			_ => currentPressure, 
		};
	}
}
