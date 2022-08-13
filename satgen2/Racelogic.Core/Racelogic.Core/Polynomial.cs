using System.Runtime.InteropServices;

namespace Racelogic.Core;

[StructLayout(LayoutKind.Sequential, Size = 1)]
internal struct Polynomial
{
	internal const uint VBOX = 4129u;

	internal const uint DriftBox = 4132u;

	internal const uint VideoVBoxAndMfd = 17884u;

	internal const uint LabSat3 = 3988292384u;
}
