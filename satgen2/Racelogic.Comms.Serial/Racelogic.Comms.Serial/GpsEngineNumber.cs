using System.ComponentModel;

namespace Racelogic.Comms.Serial;

[TypeConverter(typeof(LocalisedEnumeration))]
public enum GpsEngineNumber
{
	One = 6,
	Two = 8
}
