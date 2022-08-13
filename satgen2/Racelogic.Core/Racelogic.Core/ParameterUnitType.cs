using System.Runtime.Serialization;

namespace Racelogic.Core;

[DataContract]
public enum ParameterUnitType
{
	[EnumMember]
	None,
	[EnumMember]
	Speed,
	[EnumMember]
	Distance,
	[EnumMember]
	Angle,
	[EnumMember]
	Acceleration,
	[EnumMember]
	LatitudeMinutes,
	[EnumMember]
	LongitudeMinutes,
	[EnumMember]
	Pressure,
	[EnumMember]
	Time,
	[EnumMember]
	TimeAlwaysInSeconds,
	[EnumMember]
	TimeAlwaysInMinutes,
	[EnumMember]
	Mass,
	[EnumMember]
	Temperature,
	[EnumMember]
	Sound,
	[EnumMember]
	IntegerDataWithoutUnit
}
