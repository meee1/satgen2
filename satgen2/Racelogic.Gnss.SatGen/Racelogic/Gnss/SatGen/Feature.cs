namespace Racelogic.Gnss.SatGen
{
	internal enum Feature
	{
		None = 0,
		SatGen = 20,
		Gps = 21,
		Glonass = 22,
		SingleConstellation = 0x1F,
		DualConstellation = 0x20,
		TripleConstellation = 33,
		RealTime = 40,
		Wideband = 41
	}
}
