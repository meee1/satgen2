using System;
using System.Globalization;
using System.Linq;
using Racelogic.Core;

namespace Racelogic.DataSource;

public class ChannelToUnitTypeConverter
{
	private string[] speedBaseUnits = new string[2] { "kmh", "km/h" };

	private string[] distanceBaseUnits = new string[5] { "m", "metre", "metres", "meters", "meter" };

	private string[] accelerationBaseUnits = new string[1] { "g" };

	private string[] angleBaseUnits = new string[3] { "degree", "degrees", "°" };

	private string[] timeAlwaysInSecondsBaseUnits = new string[2] { "s", "seconds" };

	private string[] temperatureBaseUnits = new string[6] { "°c", "c", "degreec", "degree°c", "degc", "deg°c" };

	private string[] pressureBaseUnits = new string[1] { "psi" };

	private string[] soundBaseUnits = new string[2] { "db", "dba" };

	private string[] massBaseUnits = new string[1] { "kg" };

	public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
	{
		if (value is RacelogicChannel)
		{
			RacelogicChannel racelogicChannel = (RacelogicChannel)value;
			if (racelogicChannel.StandardChannel != VBoxChannel.None)
			{
				VBoxChannel standardChannel = racelogicChannel.StandardChannel;
				if (standardChannel <= VBoxChannel.Distance)
				{
					if (standardChannel <= VBoxChannel.Height)
					{
						if (standardChannel <= VBoxChannel.Longitude)
						{
							VBoxChannel num = standardChannel - 1;
							if (num <= (VBoxChannel.Satellites | VBoxChannel.UtcTime))
							{
								switch (num)
								{
								case VBoxChannel.Satellites | VBoxChannel.UtcTime:
									return ParameterUnitType.LatitudeMinutes;
								case VBoxChannel.Satellites:
									goto IL_01af;
								case VBoxChannel.None:
									goto IL_01b6;
								case VBoxChannel.UtcTime:
									goto IL_01be;
								}
							}
							if (standardChannel == VBoxChannel.Longitude)
							{
								return ParameterUnitType.LongitudeMinutes;
							}
						}
						else
						{
							if (standardChannel == VBoxChannel.Speed)
							{
								goto IL_017d;
							}
							if (standardChannel == VBoxChannel.Heading)
							{
								return ParameterUnitType.Angle;
							}
							if (standardChannel == VBoxChannel.Height)
							{
								goto IL_0184;
							}
						}
					}
					else if (standardChannel <= VBoxChannel.LongitudinalAcceleration)
					{
						if (standardChannel == VBoxChannel.VerticalVelocity)
						{
							goto IL_017d;
						}
						if (standardChannel == VBoxChannel.LongitudinalAcceleration)
						{
							goto IL_018b;
						}
					}
					else
					{
						if (standardChannel == VBoxChannel.LateralAcceleration)
						{
							goto IL_018b;
						}
						if (standardChannel == VBoxChannel.BrakeDistance || standardChannel == VBoxChannel.Distance)
						{
							goto IL_0184;
						}
					}
				}
				else if (standardChannel <= VBoxChannel.VelocityQuality)
				{
					if (standardChannel <= VBoxChannel.GpsSatellites)
					{
						if (standardChannel == VBoxChannel.GlonassSatellites || standardChannel == VBoxChannel.GpsSatellites)
						{
							goto IL_01b6;
						}
					}
					else
					{
						if (standardChannel == VBoxChannel.Yaw01LateralAcceleration)
						{
							goto IL_018b;
						}
						if (standardChannel == VBoxChannel.SolutionType)
						{
							goto IL_01b6;
						}
						if (standardChannel == VBoxChannel.VelocityQuality)
						{
							goto IL_017d;
						}
					}
				}
				else if (standardChannel <= VBoxChannel.AviFileIndex)
				{
					if (standardChannel == VBoxChannel.TriggerEventTime || standardChannel == VBoxChannel.Event2Time)
					{
						goto IL_01af;
					}
					if (standardChannel == VBoxChannel.AviFileIndex)
					{
						goto IL_01b6;
					}
				}
				else
				{
					if (standardChannel == VBoxChannel.AviSyncTime)
					{
						goto IL_01af;
					}
					if (standardChannel == VBoxChannel.BrakeTrigger)
					{
						goto IL_01b6;
					}
					if (standardChannel == VBoxChannel.TimeFromTestStart)
					{
						return ParameterUnitType.TimeAlwaysInSeconds;
					}
				}
				goto IL_01be;
			}
			return GetUnitTypeForCanChannel(racelogicChannel.Name, racelogicChannel.Units);
		}
		if (value is string)
		{
			return GetUnitTypeForCanChannel(null, (string)value);
		}
		return ParameterUnitType.None;
		IL_01be:
		return ParameterUnitType.None;
		IL_01af:
		return ParameterUnitType.Time;
		IL_0184:
		return ParameterUnitType.Distance;
		IL_017d:
		return ParameterUnitType.Speed;
		IL_018b:
		return ParameterUnitType.Acceleration;
		IL_01b6:
		return ParameterUnitType.IntegerDataWithoutUnit;
	}

	private ParameterUnitType GetUnitTypeForCanChannel(string channelName, string unit)
	{
		if (!string.IsNullOrEmpty(channelName))
		{
			if (channelName.ToLower().Contains("latitude"))
			{
				if (string.IsNullOrEmpty(unit) || string.Compare(unit, "minutes", ignoreCase: true) == 0 || string.Compare(unit, "minute", ignoreCase: true) == 0 || string.Compare(unit, "'", ignoreCase: true) == 0)
				{
					return ParameterUnitType.LatitudeMinutes;
				}
			}
			else if (channelName.ToLower().Contains("longitude") && (string.IsNullOrEmpty(unit) || string.Compare(unit, "minutes", ignoreCase: true) == 0 || string.Compare(unit, "minute", ignoreCase: true) == 0 || string.Compare(unit, "'", ignoreCase: true) == 0))
			{
				return ParameterUnitType.LongitudeMinutes;
			}
		}
		if (!string.IsNullOrEmpty(unit))
		{
			if (distanceBaseUnits.Any((string c) => string.Compare(c, unit, ignoreCase: true) == 0))
			{
				return ParameterUnitType.Distance;
			}
			if (accelerationBaseUnits.Any((string c) => string.Compare(c, unit, ignoreCase: true) == 0))
			{
				return ParameterUnitType.Acceleration;
			}
			if (angleBaseUnits.Any((string c) => string.Compare(c, unit, ignoreCase: true) == 0))
			{
				return ParameterUnitType.Angle;
			}
			if (speedBaseUnits.Any((string c) => string.Compare(c, unit, ignoreCase: true) == 0))
			{
				return ParameterUnitType.Speed;
			}
			if (timeAlwaysInSecondsBaseUnits.Any((string c) => string.Compare(c, unit, ignoreCase: true) == 0))
			{
				return ParameterUnitType.TimeAlwaysInSeconds;
			}
			if (temperatureBaseUnits.Any((string c) => string.Compare(c, unit, ignoreCase: true) == 0))
			{
				return ParameterUnitType.Temperature;
			}
			if (soundBaseUnits.Any((string c) => string.Compare(c, unit, ignoreCase: true) == 0))
			{
				return ParameterUnitType.Sound;
			}
			if (pressureBaseUnits.Any((string c) => string.Compare(c, unit, ignoreCase: true) == 0))
			{
				return ParameterUnitType.Pressure;
			}
			if (massBaseUnits.Any((string c) => string.Compare(c, unit, ignoreCase: true) == 0))
			{
				return ParameterUnitType.Mass;
			}
			return ParameterUnitType.None;
		}
		return ParameterUnitType.None;
	}

	public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
	{
		return (ParameterUnitType)value switch
		{
			ParameterUnitType.Distance => distanceBaseUnits, 
			ParameterUnitType.Acceleration => accelerationBaseUnits, 
			ParameterUnitType.Angle => angleBaseUnits, 
			ParameterUnitType.Speed => speedBaseUnits, 
			ParameterUnitType.TimeAlwaysInSeconds => timeAlwaysInSecondsBaseUnits, 
			ParameterUnitType.Temperature => temperatureBaseUnits, 
			ParameterUnitType.Sound => soundBaseUnits, 
			ParameterUnitType.Pressure => pressureBaseUnits, 
			ParameterUnitType.Mass => massBaseUnits, 
			_ => null, 
		};
	}
}
