namespace Racelogic.Gnss.SatGen.Gps;

internal enum CNavMessageType : uint
{
	Default = 0u,
	Ephemeris1 = 10u,
	Ephemeris2 = 11u,
	ClockIonoAndGroupDelay = 30u,
	ClockAndUTC = 33u
}
