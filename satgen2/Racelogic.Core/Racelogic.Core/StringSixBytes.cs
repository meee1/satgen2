using System.Runtime.InteropServices;

namespace Racelogic.Core;

[StructLayout(LayoutKind.Sequential, Pack = 1, Size = 6)]
public struct StringSixBytes
{
	[MarshalAs(UnmanagedType.ByValArray, SizeConst = 6)]
	private byte[] data;

	public string Data
	{
		get
		{
			return StructureFunctions.ByteArrayToString(data);
		}
		set
		{
			data = StructureFunctions.StringToByteArray(value, 6u);
		}
	}
}
