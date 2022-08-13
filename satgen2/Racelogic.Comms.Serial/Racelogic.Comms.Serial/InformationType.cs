using System.ComponentModel;

namespace Racelogic.Comms.Serial;

[TypeConverter(typeof(LocalisedEnumeration))]
public enum InformationType
{
	Information,
	Warning,
	Error
}
