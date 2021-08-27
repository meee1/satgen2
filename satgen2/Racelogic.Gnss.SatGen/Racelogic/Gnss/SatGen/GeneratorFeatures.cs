using System;

namespace Racelogic.Gnss.SatGen
{
	[Flags]
	internal enum GeneratorFeatures
	{
		None = 0x0,
		Levels = 0x1,
		Noise = 0x2,
		InPhaseBpsk = 0x4,
		QuadratureBpsk = 0x8,
		DualBpsk = 0x10,
		DualBpskSync = 0x20,
		SinBocBpsk = 0x40,
		MultiConstellaton = 0x80,
		MultiBand = 0x100
	}
}
