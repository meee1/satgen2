namespace Racelogic.DataSource;

public static class TemperatureConstants
{
	public static double ConvertToDefaultTemperatureUnit(double currentTemperature, TemperatureUnit currentUnit)
	{
		return currentUnit switch
		{
			TemperatureUnit.Fahrenheit => (currentTemperature - 32.0) * 5.0 / 9.0, 
			TemperatureUnit.Kelvin => currentTemperature - 273.15, 
			_ => currentTemperature, 
		};
	}

	public static double ConvertFromDefaultTemperatureUnit(double currentTemperature, TemperatureUnit targetUnit)
	{
		return targetUnit switch
		{
			TemperatureUnit.Fahrenheit => currentTemperature * 9.0 / 5.0 + 32.0, 
			TemperatureUnit.Kelvin => currentTemperature + 273.15, 
			_ => currentTemperature, 
		};
	}
}
