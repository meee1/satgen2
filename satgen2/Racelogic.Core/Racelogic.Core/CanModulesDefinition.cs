using System.Runtime.InteropServices;

namespace Racelogic.Core;

[StructLayout(LayoutKind.Sequential, Pack = 1, Size = 12)]
public struct CanModulesDefinition
{
	public int SerialNumber;

	public byte UnitType;

	[MarshalAs(UnmanagedType.ByValArray, SizeConst = 7)]
	public byte[] ExtraInformation;
}
