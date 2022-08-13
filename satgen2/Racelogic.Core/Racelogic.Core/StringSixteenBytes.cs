using System.Runtime.InteropServices;

namespace Racelogic.Core;

[StructLayout(LayoutKind.Sequential, Pack = 1, Size = 16)]
public struct StringSixteenBytes
{
	[MarshalAs(UnmanagedType.ByValArray, SizeConst = 16)]
	private byte[] data;

	public string Data
	{
		get
		{
			return StructureFunctions.ByteArrayToString(data);
		}
		set
		{
			data = StructureFunctions.StringToByteArray(value, 16u);
		}
	}
}
