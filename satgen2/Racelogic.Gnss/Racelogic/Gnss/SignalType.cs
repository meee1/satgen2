using System;

namespace Racelogic.Gnss
{
	[Flags]
	public enum SignalType : ulong
	{
		None = 0x0uL,
		GpsL1CA = 0x1uL,
		GpsL1C = 0x2uL,
		GpsL1P = 0x4uL,
		GpsL1M = 0x8uL,
		GpsL2C = 0x10uL,
		GpsL2P = 0x20uL,
		GpsL2M = 0x40uL,
		GpsL5I = 0x80uL,
		GpsL5Q = 0x100uL,
		GpsL5 = 0x180uL,
		GlonassL1OF = 0x200uL,
		GlonassL2OF = 0x400uL,
		BeiDouB1I = 0x800uL,
		BeiDouB2I = 0x2000uL,
		BeiDouB3I = 0x4000uL,
		GalileoE1BC = 0x8000uL,
		GalileoE5aI = 0x10000uL,
		GalileoE5aQ = 0x20000uL,
		GalileoE5a = 0x30000uL,
		GalileoE5bI = 0x40000uL,
		GalileoE5bQ = 0x80000uL,
		GalileoE5b = 0xC0000uL,
		GalileoE5AltBocI = 0x100000uL,
		GalileoE5AltBocQ = 0x200000uL,
		GalileoE5AltBoc = 0x300000uL,
		GalileoE6BC = 0x400000uL,
		NavicL5SPS = 0x800000uL,
		NavicSSPS = 0x1000000uL
	}
}
