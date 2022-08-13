using Racelogic.Core;

namespace Racelogic.DataSource;

public enum TimeUnit
{
	[LocalizableDescription("TimeUnit_SecondsSinceMidnight", typeof(Resources))]
	SecondsSinceMidnight,
	[LocalizableDescription("TimeUnit_DaysHoursMinutesSeconds", typeof(Resources))]
	DaysHoursMinutesSeconds,
	[LocalizableDescription("TimeUnit_HoursMinutesSecondsPlusNonZeroDays", typeof(Resources))]
	HoursMinutesSecondsPlusNonZeroDays,
	[LocalizableDescription("TimeUnit_DaysHoursMinutesWholeSeconds", typeof(Resources))]
	DaysHoursMinutesWholeSeconds,
	[LocalizableDescription("TimeUnit_HoursMinutesSeconds", typeof(Resources))]
	HoursMinutesSeconds,
	[LocalizableDescription("TimeUnit_HoursMinutesWholeSeconds", typeof(Resources))]
	HoursMinutesWholeSeconds,
	[LocalizableDescription("TimeUnit_HoursMinutesSecondsNoSeparator", typeof(Resources))]
	HoursMinutesSecondsNoSeparator,
	[LocalizableDescription("TimeUnit_HoursMinutesWholeSecondsNoSeparator", typeof(Resources))]
	HoursMinutesWholeSecondsNoSeparator,
	[LocalizableDescription("TimeUnit_HoursMinutes", typeof(Resources))]
	HoursMinutes,
	[LocalizableDescription("TimeUnit_Milliseconds", typeof(Resources))]
	Milliseconds
}
