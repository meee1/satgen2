namespace Racelogic.Gnss.SatGen;

internal enum Feature
{
	None = 0,
	SatGen = 20,
	Gps = 21,
	Glonass = 22,
	SingleConstellation = 31,
	DualConstellation = 32,
	TripleConstellation = 33,
	RealTime = 40,
	Wideband = 41
}
