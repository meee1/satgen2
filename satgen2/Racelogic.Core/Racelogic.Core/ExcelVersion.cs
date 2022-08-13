using System.ComponentModel;

namespace Racelogic.Core;

[TypeConverter(typeof(LocalisedEnumeration))]
public enum ExcelVersion
{
	None = 0,
	Excel95 = 7,
	Excel97 = 8,
	Excel2000 = 9,
	Excel2002 = 10,
	Excel2003 = 11,
	Excel2007 = 12,
	Excel2010 = 14,
	Excel2015 = 15,
	Excel2016 = 16,
	Unknown = 1000
}
