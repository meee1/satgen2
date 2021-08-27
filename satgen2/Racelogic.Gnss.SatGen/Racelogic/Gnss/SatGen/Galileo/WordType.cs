namespace Racelogic.Gnss.SatGen.Galileo
{
	internal enum WordType : uint
	{
		Spare = 0u,
		Word1 = 1u,
		Word2 = 2u,
		Word3 = 3u,
		Word4 = 4u,
		Word5 = 5u,
		Word6 = 6u,
		Word7 = 7u,
		Word8 = 8u,
		Word9 = 9u,
		Word10 = 10u,
		Dummy = 0x3Fu,
		Unresolved = 100u,
		Word79 = 107u,
		Word810 = 108u
	}
}
