using System.ComponentModel;

namespace Racelogic.Core.Filters;

[TypeConverter(typeof(LocalisedEnumeration))]
public enum CutOffFrequency
{
	SevenHz = 7,
	TenHz = 10,
	FifteenHz = 15
}
