using System;

namespace Racelogic.Gnss.SatGen;

[Flags]
internal enum GeneratorFeatures
{
	None = 0,
	Levels = 1,
	Noise = 2,
	InPhaseBpsk = 4,
	QuadratureBpsk = 8,
	DualBpsk = 0x10,
	DualBpskSync = 0x20,
	SinBocBpsk = 0x40,
	MultiConstellaton = 0x80,
	MultiBand = 0x100
}
