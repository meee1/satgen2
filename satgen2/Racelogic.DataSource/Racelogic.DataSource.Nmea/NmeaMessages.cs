using System;
using Racelogic.Core;

namespace Racelogic.DataSource.Nmea;

[Flags]
public enum NmeaMessages
{
	[LocalizableDescription("NmeaMessages_None", typeof(Resources))]
	None = 0,
	[LocalizableDescription("NmeaMessages_Gga", typeof(Resources))]
	Gga = 1,
	[LocalizableDescription("NmeaMessages_Rmc", typeof(Resources))]
	Rmc = 2,
	[LocalizableDescription("NmeaMessages_Gsa", typeof(Resources))]
	Gsa = 4,
	[LocalizableDescription("NmeaMessages_Vtg", typeof(Resources))]
	Vtg = 8,
	[LocalizableDescription("NmeaMessages_Gll", typeof(Resources))]
	Gll = 0x10,
	[LocalizableDescription("NmeaMessages_Gst", typeof(Resources))]
	Gst = 0x20,
	[LocalizableDescription("NmeaMessages_Gsv", typeof(Resources))]
	Gsv = 0x40,
	[LocalizableDescription("NmeaMessages_Zda", typeof(Resources))]
	Zda = 0x80
}
