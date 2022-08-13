using System;
using Racelogic.Core;

namespace Racelogic.DataSource;

public class UnitsCache
{
	private static string distanceUnitText;

	private static string speedUnitText;

	private static string accelUnitText;

	private static string angleUnitText;

	private static string pressureUnitText;

	private static string massUnitText;

	private static string soundUnitText;

	private static string tempUnitText;

	private static string latLongUnitText;

	private static DistanceUnit distanceUnits;

	private static SpeedUnit speedUnits;

	private static AccelerationUnit accelUnits;

	private static AngleUnit angleUnits;

	private static PressureUnit pressureUnits;

	private static MassUnit massUnits;

	private static SoundUnit soundUnits;

	private static TemperatureUnit tempUnits;

	private static LatLongUnit latLongUnits;

	private static int distancePrecision;

	private static int speedPrecision;

	private static int accelPrecision;

	private static int anglePrecision;

	private static int pressurePrecision;

	private static int massPrecision;

	private static int soundPrecision;

	private static int tempPrecision;

	private static int latLongPrecision;

	private static int noUnitDataPrecision;

	private static int timePrecision;

	private static int timeInSecondsPrecision;

	private static string distanceFormat;

	private static string speedFormat;

	private static string accelFormat;

	private static string angleFormat;

	private static string pressureFormat;

	private static string massFormat;

	private static string soundFormat;

	private static string tempFormat;

	private static string latLongFormat;

	private static string noUnitDataFormat;

	private static string timeFormat;

	private static string timeInSecondsFormat;

	private static bool cached;

	[ThreadStatic]
	private static bool requireBaseUnitInThisThread;

	public static string DistanceUnitText
	{
		get
		{
			if (requireBaseUnitInThisThread)
			{
				return DistanceUnit.Metres.LocalisedString();
			}
			if (!cached)
			{
				return FormatOptions.Instance.Distance.Units.LocalisedString();
			}
			return distanceUnitText;
		}
	}

	public static string SpeedUnitText
	{
		get
		{
			if (requireBaseUnitInThisThread)
			{
				return SpeedUnit.KilometresPerHour.LocalisedString();
			}
			if (!cached)
			{
				return FormatOptions.Instance.Speed.Units.LocalisedString();
			}
			return speedUnitText;
		}
	}

	public static string AccelUnitText
	{
		get
		{
			if (requireBaseUnitInThisThread)
			{
				return AccelerationUnit.G.LocalisedString();
			}
			if (!cached)
			{
				return FormatOptions.Instance.Acceleration.Units.LocalisedString();
			}
			return accelUnitText;
		}
	}

	public static string LatLongUnitText
	{
		get
		{
			if (requireBaseUnitInThisThread)
			{
				return LatLongUnit.Minutes.LocalisedString();
			}
			if (!cached)
			{
				return FormatOptions.Instance.LatLong.Units.LocalisedString();
			}
			return latLongUnitText;
		}
	}

	public static string MassUnitText
	{
		get
		{
			if (requireBaseUnitInThisThread)
			{
				return MassUnit.Kilogram.LocalisedString();
			}
			if (!cached)
			{
				return FormatOptions.Instance.Mass.Units.LocalisedString();
			}
			return massUnitText;
		}
	}

	public static string SoundUnitText
	{
		get
		{
			if (requireBaseUnitInThisThread)
			{
				return SoundUnit.dBA.LocalisedString();
			}
			if (!cached)
			{
				return FormatOptions.Instance.Sound.Units.LocalisedString();
			}
			return soundUnitText;
		}
	}

	public static string PressureUnitText
	{
		get
		{
			if (requireBaseUnitInThisThread)
			{
				return PressureUnit.Psi.LocalisedString();
			}
			if (!cached)
			{
				return FormatOptions.Instance.Pressure.Units.LocalisedString();
			}
			return pressureUnitText;
		}
	}

	public static string AngleUnitText
	{
		get
		{
			if (requireBaseUnitInThisThread)
			{
				return AngleUnit.Degree.LocalisedString();
			}
			if (!cached)
			{
				return FormatOptions.Instance.Angle.Units.LocalisedString();
			}
			return angleUnitText;
		}
	}

	public static string TempUnitText
	{
		get
		{
			if (requireBaseUnitInThisThread)
			{
				return TemperatureUnit.DegreeC.LocalisedString();
			}
			if (!cached)
			{
				return FormatOptions.Instance.Temperature.Units.LocalisedString();
			}
			return tempUnitText;
		}
	}

	public static DistanceUnit DistanceUnits
	{
		get
		{
			if (requireBaseUnitInThisThread)
			{
				return DistanceUnit.Metres;
			}
			if (!cached)
			{
				return FormatOptions.Instance.Distance.Units;
			}
			return distanceUnits;
		}
	}

	public static SpeedUnit SpeedUnits
	{
		get
		{
			if (requireBaseUnitInThisThread)
			{
				return SpeedUnit.KilometresPerHour;
			}
			if (!cached)
			{
				return FormatOptions.Instance.Speed.Units;
			}
			return speedUnits;
		}
	}

	public static AccelerationUnit AccelUnits
	{
		get
		{
			if (requireBaseUnitInThisThread)
			{
				return AccelerationUnit.G;
			}
			if (!cached)
			{
				return FormatOptions.Instance.Acceleration.Units;
			}
			return accelUnits;
		}
	}

	public static LatLongUnit LatLongUnits
	{
		get
		{
			if (requireBaseUnitInThisThread)
			{
				return LatLongUnit.Minutes;
			}
			if (!cached)
			{
				return FormatOptions.Instance.LatLong.Units;
			}
			return latLongUnits;
		}
	}

	public static MassUnit MassUnits
	{
		get
		{
			if (requireBaseUnitInThisThread)
			{
				return MassUnit.Kilogram;
			}
			if (!cached)
			{
				return FormatOptions.Instance.Mass.Units;
			}
			return massUnits;
		}
	}

	public static SoundUnit SoundUnits
	{
		get
		{
			if (requireBaseUnitInThisThread)
			{
				return SoundUnit.dBA;
			}
			if (!cached)
			{
				return FormatOptions.Instance.Sound.Units;
			}
			return soundUnits;
		}
	}

	public static PressureUnit PressureUnits
	{
		get
		{
			if (requireBaseUnitInThisThread)
			{
				return PressureUnit.Psi;
			}
			if (!cached)
			{
				return FormatOptions.Instance.Pressure.Units;
			}
			return pressureUnits;
		}
	}

	public static AngleUnit AngleUnits
	{
		get
		{
			if (requireBaseUnitInThisThread)
			{
				return AngleUnit.Degree;
			}
			if (!cached)
			{
				return FormatOptions.Instance.Angle.Units;
			}
			return angleUnits;
		}
	}

	public static TemperatureUnit TempUnits
	{
		get
		{
			if (requireBaseUnitInThisThread)
			{
				return TemperatureUnit.DegreeC;
			}
			if (!cached)
			{
				return FormatOptions.Instance.Temperature.Units;
			}
			return tempUnits;
		}
	}

	public static int DistancePrecision
	{
		get
		{
			if (!cached)
			{
				return FormatOptions.Instance.Distance.DecimalPlaces;
			}
			return distancePrecision;
		}
	}

	public static int SpeedPrecision
	{
		get
		{
			if (!cached)
			{
				return FormatOptions.Instance.Speed.DecimalPlaces;
			}
			return speedPrecision;
		}
	}

	public static int AccelPrecision
	{
		get
		{
			if (!cached)
			{
				return FormatOptions.Instance.Acceleration.DecimalPlaces;
			}
			return accelPrecision;
		}
	}

	public static int MassPrecision
	{
		get
		{
			if (!cached)
			{
				return FormatOptions.Instance.Mass.DecimalPlaces;
			}
			return massPrecision;
		}
	}

	public static int SoundPrecision
	{
		get
		{
			if (!cached)
			{
				return FormatOptions.Instance.Sound.DecimalPlaces;
			}
			return soundPrecision;
		}
	}

	public static int PressurePrecision
	{
		get
		{
			if (!cached)
			{
				return FormatOptions.Instance.Pressure.DecimalPlaces;
			}
			return pressurePrecision;
		}
	}

	public static int AnglePrecision
	{
		get
		{
			if (!cached)
			{
				return FormatOptions.Instance.Angle.DecimalPlaces;
			}
			return anglePrecision;
		}
	}

	public static int TempPrecision
	{
		get
		{
			if (!cached)
			{
				return FormatOptions.Instance.Temperature.DecimalPlaces;
			}
			return tempPrecision;
		}
	}

	public static int LatLongPrecision
	{
		get
		{
			if (!cached)
			{
				return FormatOptions.Instance.LatLong.DecimalPlaces;
			}
			return latLongPrecision;
		}
	}

	public static int TimePrecision
	{
		get
		{
			if (!cached)
			{
				return FormatOptions.Instance.Time.DecimalPlaces;
			}
			return timePrecision;
		}
	}

	public static int TimeInSecondsPrecision
	{
		get
		{
			if (!cached)
			{
				return FormatOptions.Instance.TimeAlwaysInSeconds.DecimalPlaces;
			}
			return timeInSecondsPrecision;
		}
	}

	public static int NoUnitDataPrecision
	{
		get
		{
			if (!cached)
			{
				return FormatOptions.Instance.Can.DecimalPlaces;
			}
			return noUnitDataPrecision;
		}
	}

	public static string DistanceFormat
	{
		get
		{
			if (!cached)
			{
				return FormatOptions.Instance.Distance.Format;
			}
			return distanceFormat;
		}
	}

	public static string SpeedFormat
	{
		get
		{
			if (!cached)
			{
				return FormatOptions.Instance.Speed.Format;
			}
			return speedFormat;
		}
	}

	public static string AccelFormat
	{
		get
		{
			if (!cached)
			{
				return FormatOptions.Instance.Acceleration.Format;
			}
			return accelFormat;
		}
	}

	public static string MassFormat
	{
		get
		{
			if (!cached)
			{
				return FormatOptions.Instance.Mass.Format;
			}
			return massFormat;
		}
	}

	public static string SoundFormat
	{
		get
		{
			if (!cached)
			{
				return FormatOptions.Instance.Sound.Format;
			}
			return soundFormat;
		}
	}

	public static string PressureFormat
	{
		get
		{
			if (!cached)
			{
				return FormatOptions.Instance.Pressure.Format;
			}
			return pressureFormat;
		}
	}

	public static string AngleFormat
	{
		get
		{
			if (!cached)
			{
				return FormatOptions.Instance.Angle.Format;
			}
			return angleFormat;
		}
	}

	public static string TempFormat
	{
		get
		{
			if (!cached)
			{
				return FormatOptions.Instance.Temperature.Format;
			}
			return tempFormat;
		}
	}

	public static string LatLongFormat
	{
		get
		{
			if (!cached)
			{
				return FormatOptions.Instance.LatLong.Format;
			}
			return latLongFormat;
		}
	}

	public static string TimeFormat
	{
		get
		{
			if (!cached)
			{
				return FormatOptions.Instance.Time.Format;
			}
			return timeFormat;
		}
	}

	public static string TimeInSecondsFormat
	{
		get
		{
			if (!cached)
			{
				return FormatOptions.Instance.TimeAlwaysInSeconds.Format;
			}
			return timeInSecondsFormat;
		}
	}

	public static string NoUnitDataFormat
	{
		get
		{
			if (!cached)
			{
				return FormatOptions.Instance.Can.Format;
			}
			return noUnitDataFormat;
		}
	}

	public static void SwitchToBaseUnitModeInThisThread()
	{
		requireBaseUnitInThisThread = true;
	}

	public static void RevertBaseUnitMode()
	{
		requireBaseUnitInThisThread = false;
	}

	public static void RefreshUnitsAndDecimalPlaces()
	{
		distanceUnits = FormatOptions.Instance.Distance.Units;
		speedUnits = FormatOptions.Instance.Speed.Units;
		accelUnits = FormatOptions.Instance.Acceleration.Units;
		massUnits = FormatOptions.Instance.Mass.Units;
		soundUnits = FormatOptions.Instance.Sound.Units;
		angleUnits = FormatOptions.Instance.Angle.Units;
		pressureUnits = FormatOptions.Instance.Pressure.Units;
		tempUnits = FormatOptions.Instance.Temperature.Units;
		latLongUnits = FormatOptions.Instance.LatLong.Units;
		distanceUnitText = distanceUnits.LocalisedString();
		speedUnitText = speedUnits.LocalisedString();
		accelUnitText = accelUnits.LocalisedString();
		massUnitText = massUnits.LocalisedString();
		soundUnitText = soundUnits.LocalisedString();
		angleUnitText = angleUnits.LocalisedString();
		pressureUnitText = pressureUnits.LocalisedString();
		tempUnitText = tempUnits.LocalisedString();
		latLongUnitText = latLongUnits.LocalisedString();
		distancePrecision = FormatOptions.Instance.Distance.DecimalPlaces;
		speedPrecision = FormatOptions.Instance.Speed.DecimalPlaces;
		accelPrecision = FormatOptions.Instance.Acceleration.DecimalPlaces;
		massPrecision = FormatOptions.Instance.Mass.DecimalPlaces;
		soundPrecision = FormatOptions.Instance.Sound.DecimalPlaces;
		anglePrecision = FormatOptions.Instance.Angle.DecimalPlaces;
		pressurePrecision = FormatOptions.Instance.Pressure.DecimalPlaces;
		tempPrecision = FormatOptions.Instance.Temperature.DecimalPlaces;
		latLongPrecision = FormatOptions.Instance.LatLong.DecimalPlaces;
		timePrecision = FormatOptions.Instance.Time.DecimalPlaces;
		timeInSecondsPrecision = FormatOptions.Instance.TimeAlwaysInSeconds.DecimalPlaces;
		noUnitDataPrecision = FormatOptions.Instance.Can.DecimalPlaces;
		distanceFormat = FormatOptions.Instance.Distance.Format;
		speedFormat = FormatOptions.Instance.Speed.Format;
		accelFormat = FormatOptions.Instance.Acceleration.Format;
		massFormat = FormatOptions.Instance.Mass.Format;
		soundFormat = FormatOptions.Instance.Sound.Format;
		angleFormat = FormatOptions.Instance.Angle.Format;
		pressureFormat = FormatOptions.Instance.Pressure.Format;
		tempFormat = FormatOptions.Instance.Temperature.Format;
		latLongFormat = FormatOptions.Instance.LatLong.Format;
		timeFormat = FormatOptions.Instance.Time.Format;
		timeInSecondsFormat = FormatOptions.Instance.TimeAlwaysInSeconds.Format;
		noUnitDataFormat = FormatOptions.Instance.Can.Format;
		cached = true;
	}
}
